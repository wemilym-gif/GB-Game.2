using UnityEngine;

public class GerenciadorMenu : MonoBehaviour
{
    [Header("Painéis do Canvas")]
    public GameObject painelLoginPaciente;
    public GameObject painelCadastroPaciente;

    [Header("Botões da Praia (Menu Principal)")]
    public GameObject botaoLoginPrincipal;
    public GameObject botaoCadastroPrincipal; // Adicionado para esconder a madeira de cadastro

    // Ação: Abrir o Painel de Cadastro
    public void AbrirCadastroPaciente()
    {
        if (painelCadastroPaciente != null) painelCadastroPaciente.SetActive(true);
        if (painelLoginPaciente != null) painelLoginPaciente.SetActive(false);
        
        // Esconde AMBOS os botões da praia
        EsconderBotoesPrincipais();
    }

    // Ação: Abrir o Painel de Login
    public void AbrirLoginPaciente()
    {
        if (painelLoginPaciente != null) painelLoginPaciente.SetActive(true);
        if (painelCadastroPaciente != null) painelCadastroPaciente.SetActive(false);
        
        // Esconde AMBOS os botões da praia (corrige o botão 'cadastro' sobreposto)
        EsconderBotoesPrincipais();
    }

    // Ação: Voltar ao Menu Principal (Botão da Setinha)
    public void VoltarMenuPrincipal()
    {
        if (painelLoginPaciente != null) painelLoginPaciente.SetActive(false);
        if (painelCadastroPaciente != null) painelCadastroPaciente.SetActive(false);

        // Reativa os dois botões na praia para a tela inicial
        if (botaoLoginPrincipal != null) botaoLoginPrincipal.SetActive(true);
        if (botaoCadastroPrincipal != null) botaoCadastroPrincipal.SetActive(true);
    }

    // Função auxiliar para limpar a tela
    private void EsconderBotoesPrincipais()
    {
        if (botaoLoginPrincipal != null) botaoLoginPrincipal.SetActive(false);
        if (botaoCadastroPrincipal != null) botaoCadastroPrincipal.SetActive(false);
    }
}