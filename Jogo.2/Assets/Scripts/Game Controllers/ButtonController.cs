using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.SceneManagement.SceneManager;

public class ButtonController : MonoBehaviour
{
    [Header("Configuração")]
    public static int remoteIndex = 0;

    [Tooltip("Nome exato da cena do menu principal")]
    [SerializeField] private string nomeCenaMenuPrincipal = "MenuInicial";

    public void PlayButton()
    {
        ICommand play = new LoadSceneCommand("Jogo");
        play.Execute();    
    }

    public void ConfigButton()
    {
        ICommand config = new LoadSceneCommand("Config - SD Balance");
        config.Execute();      
    }

    public void PauseButton()
    {
        Time.timeScale = 0f; 
        SceneManager.LoadScene("Pause", LoadSceneMode.Additive);    
    }

    public void ContinueButton()
    {
       
        Time.timeScale = 1f;

        ICommand calib = new LoadSceneCommand("Calibração - Wii Board");
        calib.Execute();
    }
    
    public void DropButton()
    {
        Wii.DropWiiRemote(remoteIndex);
    }

    public void CalibrationButton()
    {
        ICommand Calibration = new LoadSceneCommand("Calibração - Wii Board");
        Calibration.Execute();    
    }

    public void RestartButton()
    {
        StopWatch.ResetTimer();
        Time.timeScale = 1f;

        ICommand restart = new LoadSceneCommand("Jogo");
        restart.Execute();
    }
    
    
    public void QuitButtom()
    {
        Time.timeScale = 1f;

    
        MenuManager.UltimaCenaVisitada = SceneManager.GetActiveScene().name;

        ICommand quit = new LoadSceneCommand(nomeCenaMenuPrincipal);
        quit.Execute();
    } 

    
    public void SairParaMenuPrincipal()
    {
        QuitButtom();
    }
}