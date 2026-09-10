using UnityEngine;
using UnityEngine.SceneManagement; 

public class MudaCena : MonoBehaviour
{
    // Função para carregar pelo nome exato da cena
    public void CarregarCena(string nomeDaCena)
    {
        // Garante que o tempo do jogo não fique pausado de partidas anteriores
        Time.timeScale = 1f; 
        SceneManager.LoadScene(nomeDaCena);
    }
}