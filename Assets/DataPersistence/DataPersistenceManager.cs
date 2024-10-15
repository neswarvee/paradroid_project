using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using System;
using TMPro;


public class DataPersistenceManager : MonoBehaviour
{
    [Header("File storage config")]
    [SerializeField] private string relativePathInAssets = "SaveData/";
    [SerializeField] private string fileName;
    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private  FileDataHandler dataHandler;

    public static DataPersistenceManager instance { get; private set;}


    public GameObject invulnerabilityCanvas;

    private TextMeshProUGUI invulnerabilityText;

    private int invulnerabilityTime = 3;

    private int invulnerabilityTimer;

    private float startTime;

    private bool invulnerabilityOver = false;


    private void Awake() 
    {
        if (instance != null){
            Debug.LogWarning("Found more than one data persistence manager in the scene");
        }
        instance = this;
        // Ensure this object persists across scene changes
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        // Save current floor
        PlayerPrefs.SetString("Floor", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        string dataDirPath = Application.persistentDataPath + "/" + relativePathInAssets;
        dataHandler = new FileDataHandler(dataDirPath, fileName);
        dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();

        invulnerabilityCanvas.SetActive(true);

        invulnerabilityTimer = invulnerabilityTime;

        PauseGame();

        startTime = Time.realtimeSinceStartup;

        invulnerabilityText = invulnerabilityCanvas.transform.Find("InvulnerabilityText").GetComponent<TextMeshProUGUI>();

    }

    private void Update()
    {   
        if (!invulnerabilityOver)
        {
            invulnerabilityTimer = invulnerabilityTime - (int)Mathf.Floor(Time.realtimeSinceStartup - startTime);

            invulnerabilityText.SetText(invulnerabilityTimer.ToString());
            if (invulnerabilityTimer <= 0)
            {
                invulnerabilityCanvas.SetActive(false);

                ResumeGame();
                invulnerabilityOver = true;
            }
        }

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

    public void NewGame()
    {
        gameData = new GameData();
    }

    public void LoadGame()
    {
        // Load any saved data from a file using the data handler
        gameData = dataHandler.Load();
        if(gameData == null){
            Debug.Log("No game data was found, setting to default.");
            NewGame();
        } 
        // Push loaded data to all the other scripts that need it
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(gameData);
        }  

        PlayerPrefs.SetInt("isResetScene", 0);
    }

    public void SaveGame()
    {
        dataPersistenceObjects = FindAllDataPersistenceObjects();


        gameData.enemyPositions.Clear();
        gameData.enemyHealths.Clear();
        gameData.enemyCurrentPointIndexes.Clear();
        
        // Update player data
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(ref gameData);
        }
        
        dataHandler.Save(gameData);
        
        // Debug log to check if game data is not null
        if (gameData == null)
        {
            Debug.LogError("GameData is null. Unable to save.");
            return;
        }
        // Debug log to check if game data is valid
        Debug.Log("Saving game data: " + gameData.deathCount + ", " + gameData.playerPosition);

    }
    private void OnApplicationQuit() {
        SaveGame();
    }
    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
        .OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjects);
    }
}

