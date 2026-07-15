using System;
using UnityEngine;
using System.Collections;
using TMPro;
using static Wii;

public class BalanceBoardCalibration : MonoBehaviour
{
    // =========================
    //   CONFIGURAÇÃO DA BALANÇA
    // =========================
    [Header("Configuração da Balança")]
    public static int remoteIndex = 0;      // Índice do Wii Remote conectado à Balance Board
    public float detectionThreshold = 5f;   // Peso mínimo para detectar presença do jogador
    public float measureDuration = 5f;      // Tempo (em segundos) de medição do peso

    // =========================
    //      REFERÊNCIAS DE UI
    // =========================
    [Header("Referências de UI")]
    public TMP_Text messageText;     // Mensagens principais ao jogador
    public TMP_Text countdownText;   // Contagem regressiva da calibração
    public TMP_Text resultText;      // Resultado final da calibração
    public GameObject playButton;    // Botão para iniciar o jogo após calibrar

    // =========================
    //  RESULTADOS DA CALIBRAÇÃO
    // =========================
    // Peso médio calculado do jogador (acessível globalmente)
    public static float playerWeight { get; private set; } = 0f;

    // Indica se o processo de calibração está em andamento
    public bool isCalibrating { get; private set; } = false;

    // Indica se a calibração já foi concluída com sucesso
    public static float playerWeight = 0f;

    // Guarda o estado atual da conexão da Balance Board
    private bool boardConnected = false;

    void Start()
    {
        // Garante que o tempo do jogo esteja normal
        Time.timeScale = 1f;

        // Verifica a conexão logo ao iniciar a cena
        CheckConnection();
    }

    void Update()
    {
        // Detecta mudança no estado da conexão da Balance Board
        if (Wii.IsActive(remoteIndex) != boardConnected)
        {
            CheckConnection();
        }

        // Se a Balance Board estiver conectada, trata a calibração
        if (boardConnected)
        {
            HandleCalibration();
        }
    }

    // =========================
    //   VERIFICAÇÃO DE CONEXÃO
    // =========================
    void CheckConnection()
    {
        // Verifica se o Wii Remote está ativo e se o acessório é uma Balance Board (tipo 3)
        if (Wii.IsActive(remoteIndex) && Wii.GetExpType(remoteIndex) == 3)
        {
            boardConnected = true;

            // Atualiza a UI para instruir o jogador
            messageText.text = "Suba no aparelho para iniciar!";
            countdownText.text = "";
            resultText.text = "";
            playButton.SetActive(false);
        }
        else
        {
            boardConnected = false;

            // Informa que a Balance Board não está disponível
            messageText.text = "Balance Board desconectada!\nModo Manual ativado";
            countdownText.text = "";
            resultText.text = "";

            // Permite jogar sem a Balance Board
            playButton.SetActive(true);

            Debug.LogWarning("Modo Manual ativado (sem Balance Board)");
        }
    }

    // =========================
    //    LÓGICA DE CALIBRAÇÃO
    // =========================
    void HandleCalibration()
    {
        // Obtém o peso total atual da Balance Board
        float currentWeight = Wii.GetTotalWeight(remoteIndex);

        // Inicia a calibração quando o jogador sobe na balança
        if (!isCalibrating && !calibrationComplete && currentWeight > detectionThreshold)
        {
            StartCoroutine(CalibratePlayerWeight());
        }
    }

    // =========================
    //  ROTINA DE CALIBRAÇÃO
    // =========================
    private IEnumerator CalibratePlayerWeight()
    {
        isCalibrating = true;

        // Mensagens iniciais
        messageText.text = "Calibrando... Mantenha-se parado";
        resultText.text = "Calculando peso:";

        float elapsed = 0f;
        float sum = 0f;
        int samples = 0;

        // Mede o peso durante um intervalo fixo
        while (elapsed < measureDuration)
        {
            float w = Wii.GetTotalWeight(remoteIndex);

            sum += w;
            samples++;
            elapsed += Time.unscaledDeltaTime;

            // Atualiza a contagem regressiva
            countdownText.text = $"{measureDuration - elapsed:F1}s";

            // Se o jogador sair da balança, cancela a calibração
            if (w < detectionThreshold)
            {
                messageText.text = "Jogador saiu do aparelho! Tente novamente";
                countdownText.text = "";
                resultText.text = "";
                isCalibrating = false;
                yield break;
            }

            yield return null;
        }

        // Calcula o peso médio do jogador
        playerWeight = (samples > 0) ? (sum / samples) : 0f;

        calibrationComplete = true;
        isCalibrating = false;

        // Atualiza a UI com o resultado final
        messageText.text = "Ajuste finalizado!";
        countdownText.text = "";
        resultText.text = $"Peso armazenado: {playerWeight:F2} kg";

        // Libera o botão de jogar
        playButton.SetActive(true);
    }
}