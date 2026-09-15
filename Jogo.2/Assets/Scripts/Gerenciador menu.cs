using UnityEngine;

public class GerenciadorMenu : MonoBehaviour
{
    [Header("Painel de Login do Paciente")]
    public GameObject painelLoginPaciente;

    // Função para abrir o painel de login
    public void AbrirLoginPaciente()
    {
        if (painelLoginPaciente != null)
        {
            painelLoginPaciente.SetActive(true);
        }
    }

    // Função opcional para fechar o painel (para um botão "X" ou "Voltar")
    public void FecharLoginPaciente()
    {
        if (painelLoginPaciente != null)
        {
            painelLoginPaciente.SetActive(false);
        }
    }
}