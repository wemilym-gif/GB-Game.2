using System;
using UnityEngine;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using static Wii;

public class BalanceBoardCalibration : MonoBehaviour
{

    [Header("Configuração da Balança")] public static int remoteIndex = 0;
    public float detectionThreshold = 5f;
    public float measureDuration = 5f;
    public float deadzone = 0.1f;


    [Header("Referências de UI")] public TMP_Text messageText;
    public TMP_Text countdownText;
    public TMP_Text resultText;
    public GameObject playButton;


    public static float playerWeight { get; private set; } = 0f;

    public static float HorizontalInput { get; private set; } = 0f;


    public bool isCalibrating { get; private set; } = false;


    public bool calibrationComplete { get; private set; } = false;


    private bool boardConnected = false;

  


void Start()
    {
        Time.timeScale = 1f;

       
        if (!Wii.IsSearching())
        {
            Wii.StartSearch();
        }

        CheckConnection();
        
    }

    void Update()
    {
        
        if (Wii.IsActive(remoteIndex) != boardConnected)
        {
            CheckConnection();
        }

        if (boardConnected)
        {
           
            CalculateCenterOfPressure();
            
          
            HandleCalibration();
        }
        else
        {
            HorizontalInput = 0f;
        }
    }

    
    void CheckConnection()
    {
        if (Wii.IsActive(remoteIndex) && Wii.GetExpType(remoteIndex) == 3)
        {
            boardConnected = true;

            if (messageText != null) messageText.text = "Suba no aparelho para iniciar!";
            if (countdownText != null) countdownText.text = "";
            if (resultText != null) resultText.text = "";
            if (playButton != null) playButton.SetActive(false);
        }
        else
        {
            boardConnected = false;

            if (messageText != null) messageText.text = "Balance Board desconectada!\nModo Manual ativado";
            if (countdownText != null) countdownText.text = "";
            if (resultText != null) resultText.text = "";

            if (playButton != null) playButton.SetActive(true);

            Debug.LogWarning("Modo Manual ativado (sem Balance Board)");
        }
    }

   
    private void CalculateCenterOfPressure()
    {
        float totalWeight = Wii.GetTotalWeight(remoteIndex);

        if (totalWeight > detectionThreshold)
        {
            // Obtém o Centro de Balanço retornado nativamente pelo WiiBuddy (Vector2 onde X = inclinação horizontal)
            Vector2 centerOfBalance = Wii.GetCenterOfBalance(remoteIndex);

            float rawInput = centerOfBalance.x;

            // Aplica a zona morta
            if (Mathf.Abs(rawInput) < deadzone)
            {
                HorizontalInput = 0f;
            }
            else
            {
                //Debug.Log(rawInput);
                HorizontalInput = Mathf.Clamp(rawInput, -1f, 1f);
               // Debug.Log(HorizontalInput);
            }
        }
        else
        {
            HorizontalInput = 0f;
        }
    }

   
    void HandleCalibration()
    {
        float currentWeight = Wii.GetTotalWeight(remoteIndex);

        if (!isCalibrating && !calibrationComplete && currentWeight > detectionThreshold)
        {
            StartCoroutine(CalibratePlayerWeight());
        }
    }

  
    private IEnumerator CalibratePlayerWeight()
    {
        isCalibrating = true;

        if (messageText != null) messageText.text = "Calibrando... Mantenha-se parado";
        if (resultText != null) resultText.text = "Calculando peso:";

        float elapsed = 0f;
        float sum = 0f;
        int samples = 0;

        while (elapsed < measureDuration)
        {
            float w = Wii.GetTotalWeight(remoteIndex);

            sum += w;
            samples++;
            elapsed += Time.unscaledDeltaTime;

            if (countdownText != null) countdownText.text = $"{measureDuration - elapsed:F1}s";

            if (w < detectionThreshold)
            {
                if (messageText != null) messageText.text = "Jogador saiu do aparelho! Tente novamente";
                if (countdownText != null) countdownText.text = "";
                if (resultText != null) resultText.text = "";
                isCalibrating = false;
                yield break;
            }

            yield return null;
        }

        playerWeight = (samples > 0) ? (sum / samples) : 0f;

        calibrationComplete = true;
        isCalibrating = false;

        if (messageText != null) messageText.text = "Ajuste finalizado!";
        if (countdownText != null) countdownText.text = "";
        if (resultText != null) resultText.text = $"Peso armazenado: {playerWeight:F2} kg";

        if (playButton != null) playButton.SetActive(true);
    }
}