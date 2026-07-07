using TMPro;
using UnityEngine;
using static Wii;

public class ConnectBalance : MonoBehaviour
{
    // =========================
    //  CONFIGURAÇÃO DA BALANÇA
    // =========================
    [Header("Configuração da Balança")]
    public static int remoteIndex = 0; // Índice do Wii Remote associado à Balance Board

 
    public TMP_Text connectText;       // Texto que exibe o estado da conexão
    public Color normalColor = Color.green; // Cor usada quando a balança está conectada
    public Color alertColor = Color.red;    // Cor usada quando a balança está desconectada

   
    void Update()
    {
        // Verifica continuamente o estado da conexão da Balance Board
        CheckConnection();
    }

    // =========================
    //   VERIFICAÇÃO DE CONEXÃO
    // =========================
    public void CheckConnection()
    {
        // Verifica se existe um Wii Remote ativo no índice informado
        if (Wii.IsActive(remoteIndex))
        {
            // Confere se o acessório conectado é uma Balance Board
            // ExpType == 3 indica Balance Board
            if (Wii.GetExpType(remoteIndex) == 3)
            {
                connectText.text = "Balance Board conectado!";
                connectText.color = normalColor;
            }
        }
        else
        {
            // Caso o Wii Remote não esteja ativo ou tenha sido desconectado
            connectText.text = "Balance Board desconectado!";
            connectText.color = alertColor;
        }
    }
}