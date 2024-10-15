using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Awake()
    {
        // Initialise PauseCounter to 0
        PlayerPrefs.SetInt("PauseCounter", 0);
    }

    public void PauseGame()
    {
        // If 'PauseCounter' in PlayerPrefs is 1 or higher, then increment it by 1, otherwise set it to 1 and set Time.timeScale to 0f
        PlayerPrefs.SetInt("PauseCounter", PlayerPrefs.GetInt("PauseCounter") + 1);
        if (PlayerPrefs.GetInt("PauseCounter") == 1)
        {
            Time.timeScale = 0f;
        } else if (PlayerPrefs.GetInt("PauseCounter") < 0)
        {
            PlayerPrefs.SetInt("PauseCounter", 0);
        }
    }

    public void ResumeGame()
    {
        // If 'PauseCounter' in PlayerPrefs is 1 or higher, then decrement it by 1, otherwise set it to 0 and set Time.timeScale to 1f
        PlayerPrefs.SetInt("PauseCounter", PlayerPrefs.GetInt("PauseCounter") - 1);
        if (PlayerPrefs.GetInt("PauseCounter") == 0)
        {
            Time.timeScale = 1f;
        } else if (PlayerPrefs.GetInt("PauseCounter") < 0)
        {
            PlayerPrefs.SetInt("PauseCounter", 0);
            Time.timeScale = 1f;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
