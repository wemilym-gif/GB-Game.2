using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using System;
using System.Collections;

// Classe responsável por controlar o jogador
public class Player : MonoBehaviour
{
    // =========================
    // COMPONENTES BÁSICOS
    // =========================

    private Rigidbody2D rigidbody2D;
    private Vector2 movement;
    private bool facingRight = true;
    public GameObject camera;

    // =========================
    // VIDA E PODERES
    // =========================

    public List<GameObject> Hearts = new List<GameObject>(3);

    [SerializeField]
    public FollowShield shield;

    [SerializeField]
    public GameObject gameOverScreen;

    public GameObject fallConnectionScreen;

    // =========================
    // VARIÁVEIS DE JOGO
    // =========================

    public float speed;
    private int life;
    private int countShell = 0;
    private int countObstacle = 0;
    private int countLife = 0;
    private int countShield = 0;
    private bool manualMode;
    private bool lastConnectionState = false;

    // =========================
    // UI
    // =========================

    [SerializeField]
    public TMP_Text countShellText;

    // =========================
    // WII BALANCE BOARD
    // =========================

    [Header("Configuração")]
    public static int remoteIndex = 0;

    // =========================
    // SD BALANCE (ARDUINO)
    // =========================

    private SD_Serial _sd_serial;
    public float renge = 10;
    public float PesoCalibrado = 0;
    public float Esquerda = 0;
    public float Direita = 0;

    // =========================
    // MÉTODOS UNITY
    // =========================

    private float intervalo = 1.0f; // Tempo em segundos
    
    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        life = Hearts.Count;

        bool isBoardConnected = Wii.IsActive(remoteIndex) && Wii.GetExpType(remoteIndex) == 3;
        manualMode = !isBoardConnected;
        
        // Inicia a rotina repetitiva..
        StartCoroutine(ExecutarAcaoRepetitiva());
    }
    
    IEnumerator ExecutarAcaoRepetitiva()
    {
        while (true)
        {
            // Executa a sua sub-rotina aqui
            MinhaSubRotina();

            // Espera pelo tempo determinado antes de continuar o loop
            yield return new WaitForSeconds(intervalo);
        }
    }

    void MinhaSubRotina()
    {
        Debug.Log("Sub-rotina executada via Corrotina às: " + Time.time);
        
        Vector4 sensors = Wii.GetBalanceBoard(remoteIndex);

        if (Wii.GetExpType(remoteIndex) == 3)
        {
            // Lado Esquerdo = Superior Esquerdo + Inferior Esquerdo
            float pesoEsquerda = sensors.y + sensors.w; 
            // Lado Direito = Superior Direito + Inferior Direito
            float pesoDireita = sensors.x + sensors.z;  
            
            Debug.Log("Peso Esquerda: " + pesoEsquerda);
            Debug.Log("Peso Direita: " + pesoDireita);
        }
    }
    
    void Update()
    {
        if (SD_Serial._connected) 
        {
            SDBalanceMove();
        }
        else if (Wii.IsActive(remoteIndex))
        {
            NintendoBalanceBoardMove();
        }
        else if (manualMode)
        {
            KeyboardMove();
        }
    }

    void SDBalanceMove()
    {
        if (_sd_serial == null) return;

        PesoCalibrado = _sd_serial.P;
        Esquerda = (_sd_serial.A + _sd_serial.C);
        Direita = (_sd_serial.B + _sd_serial.D);

        float threshold = (PesoCalibrado / 2) + renge;

        if (Esquerda > threshold)
        {
            movement = new Vector2(-1, 0);
            if (facingRight) Flip();
        }
        else if (Direita > threshold)
        {
            movement = new Vector2(1, 0);
            if (!facingRight) Flip();
        }
        else
        {
            movement = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        rigidbody2D.linearVelocity = movement * speed;
    }

    // =========================
    // COLISÕES E FIM DE JOGO
    // =========================

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Caso colida com um obstáculo
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            camera.GetComponent<Tremor>().playTremor();
            life--;
            countObstacle++;

            Hearts[life].SetActive(false);
            ObjectPool.Instance.ReturnToPool("Obstacle", collision.gameObject);

            // Caso o jogador perca todas as vidas
            if (life == 0)
            {
                // 1. Captura os valores dos 4 quadrantes da balança no instante da derrota
                float supEsq = 0f, supDir = 0f, infEsq = 0f, infDir = 0f;

                if (Wii.IsActive(remoteIndex) && Wii.GetExpType(remoteIndex) == 3)
                {
                    Vector4 sensoresFinais = Wii.GetBalanceBoard(remoteIndex);
                    supEsq = sensoresFinais.y; // Superior Esquerdo
                    supDir = sensoresFinais.x; // Superior Direito
                    infEsq = sensoresFinais.w; // Inferior Esquerdo
                    infDir = sensoresFinais.z; // Inferior Direito
                }
                else if (SD_Serial._connected && _sd_serial != null)
                {
                    supEsq = _sd_serial.A;
                    supDir = _sd_serial.B;
                    infEsq = _sd_serial.C;
                    infDir = _sd_serial.D;
                }

                // 2. Envia a partida completa para o Firebase
                if (FirebaseManager.Instance != null)
                {
                    FirebaseManager.Instance.SalvarPartidaReal(
                        countShell,     // Pontuação (conchas)
                        countObstacle,  // Colisões
                        1.85f,          // Oscilação média
                        supEsq, 
                        supDir, 
                        infEsq, 
                        infDir
                    );
                }
                else
                {
                    // Fallback caso o Singleton não esteja instanciado
                    FirebaseManager firebase = FindFirstObjectByType<FirebaseManager>();
                    if (firebase != null)
                    {
                        firebase.SalvarPartidaReal(
                            countShell, 
                            countObstacle, 
                            1.85f, 
                            supEsq, 
                            supDir, 
                            infEsq, 
                            infDir
                        );
                    }
                    else
                    {
                        Debug.LogWarning("FirebaseManager não foi encontrado na cena!");
                    }
                }

                // 3. Telas e congelamento do jogo
                Time.timeScale = 0f;
                gameOverScreen.SetActive(true);

                // 4. Desativa componentes para simular o sumiço do Player sem quebrá-lo
                GetComponent<SpriteRenderer>().enabled = false; 
                this.enabled = false; 
            }
        }

        // Caso colida com item de vida
        if (collision.gameObject.CompareTag("Life"))
        {
            if (life == 3)
            {
                ObjectPool.Instance.ReturnToPool("Life", collision.gameObject);
            }

            if (life < 3)
            {
                life++;
                countLife++;
                Hearts[life - 1].SetActive(true);
                ObjectPool.Instance.ReturnToPool("Life", collision.gameObject);
            }
        }

        // Caso colida com uma concha
        if (collision.gameObject.CompareTag("Shell"))
        {
            countShell++;
            countShellText.text = countShell.ToString();
            ObjectPool.Instance.ReturnToPool("Shell", collision.gameObject);
        }

        // Caso colida com escudo
        if (collision.gameObject.CompareTag("Shield"))
        {
            countShield++;
            shield.gameObject.SetActive(true);
            ObjectPool.Instance.ReturnToPool("Shield", collision.gameObject);
        }
    }

    // =========================
    // CONTROLE DE DIREÇÃO
    // =========================

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // =========================
    // CONTROLE POR TECLADO
    // =========================

    void KeyboardMove()
    {
        if (Input.GetKey(KeyCode.A))
        {
            movement = new Vector2(-1, 0).normalized;
            if (facingRight) Flip();
        }
        else if (Input.GetKey(KeyCode.D))
        {
            movement = new Vector2(1, 0).normalized;
            if (!facingRight) Flip();
        }
        else
        {
            movement = Vector2.zero;
        }
    }

    // =========================
    // CONTROLE POR WII BALANCE BOARD
    // =========================

    void NintendoBalanceBoardMove()
    {
        if (!Wii.IsActive(remoteIndex)) return;

        if (Wii.GetExpType(remoteIndex) == 3)
        {
            Vector4 sensors = Wii.GetBalanceBoard(remoteIndex);

            // 1. Filtros de ruído aplicados INDEPENDENTEMENTE para cada sensor (Valores positivos reais)
            float deadzone = 1.3f; 

            if (sensors.x >= 0f && sensors.x < deadzone) sensors.x = 0f;
            if (sensors.y >= 0f && sensors.y < deadzone) sensors.y = 0f;
            if (sensors.w >= 0f && sensors.w < deadzone) sensors.w = 0f;
            if (sensors.z >= 0f && sensors.z < deadzone) sensors.z = 0f;

            // 2. Recupera o peso calibrado do outro script
            float pesoReferencia = BalanceBoardCalibration.playerWeight;

            if (pesoReferencia < 10f)
            {
                pesoReferencia = sensors.x + sensors.y + sensors.w + sensors.z;
                if (pesoReferencia < 10f) pesoReferencia = 70f; 
            }

            // 3. Calcula o limiar de inclinação
            float threshold = (pesoReferencia / 2f) + 4f;

            // Lado Esquerdo = Superior Esquerdo + Inferior Esquerdo
            float pesoEsquerda = sensors.y + sensors.w; 
            // Lado Direito = Superior Direito + Inferior Direito
            float pesoDireita = sensors.x + sensors.z;  

            // 4. Aplica a movimentação baseada nas forças reais calculadas
            if (pesoEsquerda > threshold)
            {
                movement = new Vector2(-1, 0); // Move para a esquerda
                if (facingRight) Flip();
            }
            else if (pesoDireita > threshold)
            {
                movement = new Vector2(1, 0);  // Move para a direita
                if (!facingRight) Flip();
            }
            else
            {
                movement = Vector2.zero;       // Fica parado no centro
            }
        }
    }
}