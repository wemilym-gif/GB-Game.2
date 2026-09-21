using UnityEngine;

public class GerenciadorMenu : MonoBehaviour
{
    [Header("Painéis do Canvas")]
    public GameObject painelLoginPaciente;
    public GameObject painelCadastroPaciente;

    [Header("Botões do Menu Principal")]
    public GameObject botaoLoginPrincipal;

    // Ação: Abrir o Painel de Cadastro
    public void AbrirCadastroPaciente()
    {
        if (painelCadastroPaciente != null) painelCadastroPaciente.SetActive(true);
        if (painelLoginPaciente != null) painelLoginPaciente.SetActive(false);
        
        // Esconde o botão de Login enquanto estiver no cadastro
        if (botaoLoginPrincipal != null) botaoLoginPrincipal.SetActive(false);
    }

    // Ação: Abrir o Painel de Login
    public void AbrirLoginPaciente()
    {
        if (painelLoginPaciente != null) painelLoginPaciente.SetActive(true);
        if (painelCadastroPaciente != null) painelCadastroPaciente.SetActive(false);
        
        // Mantém o botão de Login visível na tela de login
        if (botaoLoginPrincipal != null) botaoLoginPrincipal.SetActive(true);
    }

    // Ação: Voltar ao Menu Principal (Botão da Setinha)
    public void VoltarMenuPrincipal()
    {
        if (painelLoginPaciente != null) painelLoginPaciente.SetActive(false);
        if (painelCadastroPaciente != null) painelCadastroPaciente.SetActive(false);

        // Garante que o botão de Login volte a aparecer no menu inicial
        if (botaoLoginPrincipal != null) botaoLoginPrincipal.SetActive(true);
    }
}