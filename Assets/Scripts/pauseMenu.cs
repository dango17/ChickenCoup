//Author: Harry Oldham
//Collaborator: N/A
//Date Created: 08/03
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class pauseMenu : MonoBehaviour
{
    public static bool isPaused = false;

    public GameObject pauseUI;
    public GameObject pausefirstBtn;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

   void Pause()
   {
        //enables the pause menu and freezes the game's timescale
        pauseUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        EventSystem.current.SetSelectedGameObject(null);

        EventSystem.current.SetSelectedGameObject(pausefirstBtn);
    }

    public void Restart()
    {
        //Restarts the current level
        //threw in the resume function in here so that it's not still paused after resetting
        SceneManager.LoadScene(1);
        Resume();
    }

    public void Quit()
    {
        Application.Quit();
    }


}
