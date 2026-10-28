using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Painéis de UI")]
    [SerializeField] private GameObject painelHistorico;
    [SerializeField] private GameObject painelAvatar;

    [Header("Gerenciadores")]
    [SerializeField] private HistoricoManager historicoManager;

    [Header("Configurações da Cena")]
    [Tooltip("Nome exato da cena que será carregada ao iniciar a partida")]
    [SerializeField] private string nomeCenaJogo = "Jogo";

    [Tooltip("Nome da cena padrão para voltar, caso o jogo inicie diretamente nesta cena")]
    [SerializeField] private string cenaAnteriorPadrao = "MenuInicial";

    // Variável estática mantida na memória para registrar a última cena visitada
    public static string UltimaCenaVisitada = "";

    // Pilha para registrar o histórico de navegação dos painéis
    private Stack<GameObject> historicoPaineis = new Stack<GameObject>();

    /// <summary>
    /// Método auxiliar para abrir um painel e salvá-lo no histórico
    /// </summary>
    public void AbrirPainel(GameObject novoPainel)
    {
        if (novoPainel == null) return;

        // Desativa o painel ativo atual, se houver um no topo do histórico
        if (historicoPaineis.Count > 0)
        {
            GameObject painelAtual = historicoPaineis.Peek();
            if (painelAtual != null) painelAtual.SetActive(false);
        }

        novoPainel.SetActive(true);
        historicoPaineis.Push(novoPainel);
    }

    /// <summary>
    /// Exibe o painel de histórico de sessões e carrega os dados do Firebase
    /// </summary>
    public void AbrirPainelHistorico()
    {
        AbrirPainel(painelHistorico);

        // Dispara a busca no Firebase assim que o painel abre
        if (historicoManager != null)
        {
            historicoManager.CarregarHistoricoSessoes();
        }
    }

    public void FecharPainelHistorico()
    {
        if (painelHistorico != null)
        {
            painelHistorico.SetActive(false);
        }
    }

    public void AbrirPainelAvatar()
    {
        AbrirPainel(painelAvatar);
    }

    public void FecharPainelAvatar()
    {
        if (painelAvatar != null)
        {
            painelAvatar.SetActive(false);
        }
    }

    public void FecharTodosPaineis()
    {
        if (painelHistorico != null) painelHistorico.SetActive(false);
        if (painelAvatar != null) painelAvatar.SetActive(false);
        historicoPaineis.Clear();
    }

    public void BotaoSetaVoltar()
    {
        if (historicoPaineis.Count > 0)
        {
            GameObject painelAtual = historicoPaineis.Pop();
            if (painelAtual != null)
            {
                painelAtual.SetActive(false);
            }

            if (historicoPaineis.Count > 0)
            {
                GameObject painelAnterior = historicoPaineis.Peek();
                if (painelAnterior != null)
                {
                    painelAnterior.SetActive(true);
                }
                return;
            }
        }

        VoltarParaCenaAnterior();
    }

    public void VoltarParaCenaAnterior()
    {
        if (!string.IsNullOrEmpty(UltimaCenaVisitada))
        {
            SceneManager.LoadScene(UltimaCenaVisitada.Trim());
        }
        else if (!string.IsNullOrEmpty(cenaAnteriorPadrao))
        {
            SceneManager.LoadScene(cenaAnteriorPadrao.Trim());
        }
        else
        {
            Debug.LogWarning("Nenhuma cena anterior foi registrada ou configurada no Inspector.");
        }
    }

    public void IniciarPartida()
    {
        CarregarCenaPorNome(nomeCenaJogo);
    }

    public void CarregarCenaPorNome(string nomeDaCena)
    {
        UltimaCenaVisitada = SceneManager.GetActiveScene().name.Trim();

        if (!string.IsNullOrEmpty(nomeDaCena))
        {
            SceneManager.LoadScene(nomeDaCena.Trim());
        }
    }

    public void VoltarParaCenaInicial()
    {
        SceneManager.LoadScene("cena inicial".Trim());
    }
}