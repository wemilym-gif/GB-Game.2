using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StopWatch : MonoBehaviour, ITimeSubject
{
    public TMP_Text textTimeHud;
    public GameObject player;
    public GameObject gameOverScreen;

    public Color normalColor = Color.white;
    public Color alertColor = Color.red;

    private static float startTime = 120f;
    private static float restTime;
    public float tempoDeAviso = 11f;

    private bool activeTime = true;
    private bool partidaSalva = false; // 👈 Trava para salvar apenas uma vez no Firebase

    private List<ITimeObserver> observers = new List<ITimeObserver>();

    void Start()
    {
        restTime = startTime;
        partidaSalva = false;
    }

    void Update()
    {
        if (!activeTime) return;

        if (player == null)
        {
            activeTime = false;
            FinalizarESalvarPartida();
            NotifyTimeEnded();
            return;
        }

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

            activeTime = false;
            FinalizarESalvarPartida(); // 👈 Salva os dados no Firebase ao zerar o tempo

            Time.timeScale = 0f;
            gameOverScreen.SetActive(true);

            NotifyTimeEnded();
        }
    }

    private void FinalizarESalvarPartida()
    {
        if (partidaSalva) return;
        partidaSalva = true;

        // Exemplo obtendo pontuação/moedas do sistema do jogo
        int pontuacaoAtual = 0; 
        int colisoesAtuais = 0;
        float oscilacaoCalculada = 0f;

        // Sensores da balança (substitua pelas variáveis reais da Wii Balance Board do UniWii)
        float se = 0f; // Superior Esquerdo
        float sd = 0f; // Superior Direito
        float ie = 0f; // Inferior Esquerdo
        float id = 0f; // Inferior Direito

        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.SalvarPartidaReal(
                pontuacaoAtual, 
                colisoesAtuais, 
                oscilacaoCalculada, 
                se, sd, ie, id
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