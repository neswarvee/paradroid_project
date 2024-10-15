using UnityEngine;

public class SaveAndLoadMainMenu : MonoBehaviour
{

    public GameObject continueButton;

    void Start()
    {
        if (!PlayerPrefs.HasKey("CanContinue"))
        {
            PlayerPrefs.SetInt("CanContinue", 0);
        }
        continueButton.SetActive(PlayerPrefs.GetInt("CanContinue") == 1);
    }

    // Function that takes in difficulty, then saves it to PlayerPrefs, then loads the main game scene
    public void StartNewGame(int difficulty)
    {
        PlayerPrefs.SetInt("Score", 0);
        PlayerPrefs.SetInt("Difficulty", difficulty);
        
        // Create folder if it doesn't exist: Application.persistentDataPath + "/SaveData"'
        if (!System.IO.Directory.Exists(Application.persistentDataPath + "/SaveData"))
        {
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/SaveData");
        }

        // Delete Assets/SaveData/*.game
        string[] files = System.IO.Directory.GetFiles(Application.persistentDataPath + "/SaveData");
        foreach (string file in files)
        {
            if (file.EndsWith(".game"))
            {
                System.IO.File.Delete(file);
            }
        }
        
        // Start with Floor 5
        PlayerPrefs.SetString("Floor", "Floor5");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Floor5");

        PlayerPrefs.SetInt("isResetScene", 1);
        PlayerPrefs.SetFloat("TimeCounter", 0f);
    }

    // Function that loads the main game scene
    public void ContinueGame()
    {
        if (PlayerPrefs.GetInt("CanContinue") == 1) {
            // Set difficulty to 1 if it isn't set
            if (!PlayerPrefs.HasKey("Difficulty"))
            {
                PlayerPrefs.SetInt("Difficulty", 1);
            }
            
            // Get the scene to load
            string scene = "Floor5";
            if (PlayerPrefs.HasKey("Floor"))
            {
                scene = PlayerPrefs.GetString("Floor");
            }
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
        }
    }
}
