using UnityEngine;
using TMPro;
using System.Collections;
using System.Reflection;

public class BalanceBoardReconnector : MonoBehaviour
{
    
    [Header("Referências de UI")]
    public TMP_Text statusText;        
    public GameObject reconnectButton; 
    public GameObject manualModePanel; 

   
    [Header("Configuração da Balance Board")]
    public int remoteIndex = 0;        

    
    private bool isTrying = false;

    public void ReconnectButton()
    {
      
        if (!isTrying)
            StartCoroutine(ReconnectRoutine());
    }

    private bool HasMethod(string methodName)
    {
        var m = typeof(Wii).GetMethod(
            methodName,
            BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic
        );
        return m != null;
    }

   
    private IEnumerator ReconnectRoutine()
    {
        isTrying = true;

     
        reconnectButton.SetActive(false);

        statusText.text = "🔄 Tentando reconectar Balance Board...";
        Debug.Log("Iniciando rotina de reconexão...");

        bool reconectou = false;

       
        for (int tentativa = 1; tentativa <= 3; tentativa++)
        {
            Debug.Log($"Tentativa {tentativa} de reconexão...");

            // 1️⃣ Interrompe buscas antigas, se disponível
            if (HasMethod("StopSearch"))
            {
                try
                {
                    Wii.StopSearch();
                    Debug.Log("StopSearch chamado.");
                }
                catch { }
            }

            yield return new WaitForSeconds(0.5f);

          
            if (HasMethod("DropWiiRemote"))
            {
                try
                {
                    Wii.DropWiiRemote(remoteIndex);
                    Debug.Log("DropWiiRemote chamado.");
                }
                catch { }
            }

            yield return new WaitForSeconds(0.5f);

            // 3️⃣ Reativa o sistema, se existir
            if (HasMethod("WakeUp"))
            {
                try
                {
                    Wii.WakeUp();
                    Debug.Log("WakeUp chamado.");
                }
                catch { }
            }

            yield return new WaitForSeconds(0.5f);

            // 4️⃣ Inicia a busca pela Balance Board
            bool iniciouBusca = false;

            if (HasMethod("Find"))
            {
                try
                {
                    Wii.findWiiRemote();
                    iniciouBusca = true;
                    Debug.Log("Find chamado.");
                }
                catch { }
            }
            else if (HasMethod("StartSearch"))
            {
                try
                {
                    Wii.StartSearch();
                    iniciouBusca = true;
                    Debug.Log("StartSearch chamado.");
                }
                catch { }
            }

            // Aguarda o tempo necessário para a busca
            if (iniciouBusca)
                yield return new WaitForSeconds(2.5f);
            else
                yield return new WaitForSeconds(1f);

            // 5️⃣ Verifica se a reconexão foi bem-sucedida
            try
            {
                if (Wii.IsActive(remoteIndex) && Wii.GetExpType(remoteIndex) == 3)
                {
                    reconectou = true;
                    break;
                }
            }
            catch { }

            Debug.LogWarning($"Tentativa {tentativa} falhou. Tentando novamente...");
        }

        // =========================
        //      RESULTADO FINAL
        // =========================
        if (reconectou)
        {
            // Reconexão bem-sucedida
            statusText.text = "✅ Balance Board reconectada!";
            manualModePanel.SetActive(false);
            reconnectButton.SetActive(false);

            Debug.Log("Balance Board reconectada com sucesso!");
        }
        else
        {
            // Falha após todas as tentativas
            statusText.text = "❌ Falha na reconexão. Modo manual ativado.";
            manualModePanel.SetActive(true);
            reconnectButton.SetActive(true);

            Debug.LogWarning("Falha final na reconexão. Entrando em modo manual.");
        }

        isTrying = false;
    }
}