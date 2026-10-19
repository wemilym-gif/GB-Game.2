using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Painéis de UI")]
    [SerializeField] private GameObject painelHistorico;
    [SerializeField] private GameObject painelAvatar;

    [Header("Configurações da Cena")]
    [Tooltip("Nome exato da cena que será carregada ao iniciar a partida")]
    [SerializeField] private string nomeCenaJogo = "Jogo";

    /// <summary>
    /// Exibe o painel de histórico de sessões
    /// </summary>
    public void AbrirPainelHistorico()
    {
        FecharTodosPaineis();
        if (painelHistorico != null)
        {
            painelHistorico.SetActive(true);
        }
    }

    /// <summary>
    /// Oculta o painel de histórico
    /// </summary>
    public void FecharPainelHistorico()
    {
        if (painelHistorico != null)
        {
            painelHistorico.SetActive(false);
        }
    }

    /// <summary>
    /// Exibe o painel de seleção de avatar
    /// </summary>
    public void AbrirPainelAvatar()
    {
        FecharTodosPaineis();
        if (painelAvatar != null)
        {
            painelAvatar.SetActive(true);
        }
    }

    /// <summary>
    /// Oculta o painel de avatar
    /// </summary>
    public void FecharPainelAvatar()
    {
        if (painelAvatar != null)
        {
            painelAvatar.SetActive(false);
        }
    }

    /// <summary>
    /// Fecha todos os painéis abertos
    /// </summary>
    public void FecharTodosPaineis()
    {
        if (painelHistorico != null) painelHistorico.SetActive(false);
        if (painelAvatar != null) painelAvatar.SetActive(false);
    }

    /// <summary>
    /// Carrega a cena configurada no Inspector
    /// </summary>
    public void IniciarPartida()
    {
        SceneManager.LoadScene(nomeCenaJogo);
    }

    /// <summary>
    /// Alternativa para carregar qualquer cena passando o nome via botão
    /// </summary>
    public void CarregarCenaPorNome(string nomeDaCena)
    {
        SceneManager.LoadScene(nomeDaCena);
    }
}