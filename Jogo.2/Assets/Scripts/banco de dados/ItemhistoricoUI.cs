using UnityEngine;
using TMPro; // Se usares Text nativo em vez de TextMeshPro, troca por: using UnityEngine.UI;

public class ItemHistoricoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtDataHora;
    [SerializeField] private TextMeshProUGUI txtPontuacao;
    [SerializeField] private TextMeshProUGUI txtColisoes;

    public void PreencherDados(SessaoData dados)
    {
        if (txtDataHora != null) txtDataHora.text = $"Data: {dados.dataHora}";
        if (txtPontuacao != null) txtPontuacao.text = $"Pontos: {dados.pontuacao}";
        if (txtColisoes != null) txtColisoes.text = $"Colisões: {dados.colisoes}";
    }
}