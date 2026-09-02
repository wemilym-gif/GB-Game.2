using UnityEngine;

using static UnityEngine.SceneManagement.SceneManager;


public class ButtonController : MonoBehaviour
{
    [Header("Configuração")]

  
    public static int remoteIndex = 0;

    
    public void PlayButton()
    {
       
        ICommand play = new LoadSceneCommand("Jogo");

        // Executa o comando
        play.Execute();    
    }

    
    public void ConfigButton()
    {
       
        ICommand config = new LoadSceneCommand("Config - SD Balance");
        config.Execute();      
    }

   
    public void PauseButton()
    {
     
        ICommand pause = new LoadSceneCommand("Pause");
        pause.Execute();    
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

       
        ICommand quit = new LoadSceneCommand("Menu");
        quit.Execute();
    } 
}