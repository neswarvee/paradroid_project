using UnityEngine;

[System.Serializable]
public class GameData
{
    public int deathCount;
    public Vector3 playerPosition;
    public float playerHealth;
    public string robotType;
    public string currentScene;
    public SerializableDictionary<string, Vector3> enemyPositions;
    public SerializableDictionary<string, int> enemyCurrentPointIndexes;
    public SerializableDictionary<string, float> enemyHealths;

    public bool reachedLevel;

    //the values defined in this constructor will be the default values
    // the game starts with when there's no data to load
    public GameData()
    {
        deathCount = 0;
        playerPosition = Vector3.zero;
        enemyPositions = new SerializableDictionary<string, Vector3>();
        enemyCurrentPointIndexes = new SerializableDictionary<string, int>();
        enemyHealths = new SerializableDictionary<string, float>();
        reachedLevel = false;

    } 
}

