using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StopWatch : MonoBehaviour, ITimeSubject
{
    [Header("UI & Referências")]
    public TMP_Text textTimeHud;
    public GameObject player;
    public GameObject gameOverScreen;

    [Header("Configurações de Cor do Tempo")]
    public Color normalColor = Color.white;
    public Color alertColor = Color.red;

    [Header("Configurações de Tempo")]
    private static float startTime = 120f;
    private static float restTime;
    public float tempoDeAviso = 11f;

    private bool activeTime = true;
    private bool partidaSalva = false; // Trava para salvar apenas uma vez no Firebase

    private List<ITimeObserver> observers = new List<ITimeObserver>();

    void Start()
    {
        restTime = startTime;
        partidaSalva = false;
        Time.timeScale = 1f; // Garante que o tempo volte ao normal ao reiniciar a cena
    }

    void Update()
    {
        if (!activeTime) return;

        // Caso o jogador morra/desapareça da cena
        if (player == null)
        {
            FinalizarEExibirGameOver();
            return;
        }

        // Contagem regressiva do tempo
        if (restTime > 0)
        {
            restTime -= Time.deltaTime;

            int minutes = Mathf.FloorToInt(restTime / 60);
            int seconds = Mathf.FloorToInt(restTime % 60);

            textTimeHud.text = $"{minutes:00}:{seconds:00}";
            textTimeHud.color = restTime <= tempoDeAviso ? alertColor : normalColor;

            NotifyTimeChanged(restTime);
        }
        else
        {
            textTimeHud.text = "00:00";
            textTimeHud.color = alertColor;

            FinalizarEExibirGameOver();
        }
    }

    private void FinalizarEExibirGameOver()
    {
        activeTime = false;

        // 1. Salva os dados da partida no Firebase
        FinalizarESalvarPartida();

        // 2. Exibe a tela de Game Over e Pausa o jogo
        ExibirGameOver();

        // 3. Notifica os observadores do evento de fim do tempo
        NotifyTimeEnded();
    }

    private void ExibirGameOver()
    {
        if (gameOverScreen != null)
        {
            // Ativa a tela (O GameOverDisplay irá disparar o OnEnable() e buscar os dados continuamente)
            gameOverScreen.SetActive(true);
        }

        Time.timeScale = 0f; // Pausa o jogo
    }

    private void FinalizarESalvarPartida()
    {
        if (partidaSalva) return;
        partidaSalva = true;

        int pontuacaoAtual = 0; 
        int colisoesAtuais = 0;   
        float oscilacaoCalculada = 0f;  

        float pressaoSuperiorEsquerdo = 0f;
        float pressaoSuperiorDireito  = 0f;
        float pressaoInferiorEsquerdo = 0f;
        float pressaoInferiorDireito  = 0f;

        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.SalvarPartidaReal(
                pontuacaoAtual, 
                colisoesAtuais, 
                oscilacaoCalculada, 
                pressaoSuperiorEsquerdo, 
                pressaoSuperiorDireito, 
                pressaoInferiorEsquerdo, 
                pressaoInferiorDireito
            );
        }
    }

    public static void ResetTimer()
    {
        restTime = startTime;
    }

    public void AddObserver(ITimeObserver observer) { if (!observers.Contains(observer)) observers.Add(observer); }
    public void RemoveObserver(ITimeObserver observer) { if (observers.Contains(observer)) observers.Remove(observer); }
    public void NotifyTimeChanged(float timeLeft) { foreach (var observer in observers) observer.OnTimeChanged(timeLeft); }
    public void NotifyTimeEnded() { foreach (var observer in observers) observer.OnTimeEnded(); }
}