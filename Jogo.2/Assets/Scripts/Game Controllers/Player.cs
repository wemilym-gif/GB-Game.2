using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using System;

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

    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        life = Hearts.Count;

        bool isBoardConnected = Wii.IsActive(remoteIndex) && Wii.GetExpType(remoteIndex) == 3;
        manualMode = !isBoardConnected;
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
    // COLISÕES
    // =========================

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Caso colida com um obstáculo
        if (collision.gameObject.tag == "Obstacle")
        {
            camera.GetComponent<Tremor>().playTremor();
            life--;
            countObstacle++;

            Hearts[life].SetActive(false);
            ObjectPool.Instance.ReturnToPool("Obstacle", collision.gameObject);

            // Caso o jogador perca todas as vidas
            if (life == 0)
            {
                // 1. Procura o FirebaseManager na cena de forma atualizada (sem aviso de obsoleto)
                FirebaseManager firebase = FindFirstObjectByType<FirebaseManager>();
                if (firebase != null)
                {
                    // Tenta salvar usando os parâmetros. Se o FirebaseManager não aceitar 2 argumentos,
                    // use a dica abaixo para ajustar o script do Firebase.
                    firebase.SalvarPartida(countShell, 0.45f);
                }
                else
                {
                    Debug.LogWarning("FirebaseManager não foi encontrado na cena!");
                }

                // 2. Telas e congelamento do jogo
                Time.timeScale = 0f;
                gameOverScreen.SetActive(true);

                // 3. Desativa componentes para simular o sumiço do Player sem quebrá-lo
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

            // Invertendo o mapeamento com base no comportamento real do seu console:
            // Seu lado esquerdo físico está ativando os sensores X e Z.
            float totalEsquerda = sensors.x + sensors.z;
        
            // Seu lado direito físico está ativando os sensores Y e W.
            float totalDireita = sensors.y + sensors.w;

            // Margem de erro para o boneco não andar sozinho
            float sensibilidadeMover = 3.0f; 

            // Se o peso na esquerda for maior, move para a esquerda (Vetor -1)
            if (totalEsquerda > totalDireita + sensibilidadeMover)
            {
                movement = new Vector2(-1, 0);
                if (facingRight) Flip();
                Debug.Log($"Esquerda: {totalEsquerda:F2}kg | Direita: {totalDireita:F2}kg -> MOVENDO ESQUERDA");
            }
            // Se o peso na direita for maior, move para a direita (Vetor 1)
            else if (totalDireita > totalEsquerda + sensibilidadeMover)
            {
                movement = new Vector2(1, 0);
                if (!facingRight) Flip();
                Debug.Log($"Esquerda: {totalEsquerda:F2}kg | Direita: {totalDireita:F2}kg -> MOVENDO DIREITA");
            }
            else
            {
                movement = Vector2.zero;
                Debug.Log($"Esquerda: {totalEsquerda:F2}kg | Direita: {totalDireita:F2}kg -> PARADO NO CENTRO");
            }
        }
    }
}