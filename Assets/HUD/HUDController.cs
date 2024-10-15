using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    public TMP_Text healthText;
    public TMP_Text scoreText;

    // Start is called before the first frame update
    void Start()
    {
        // Load the score from PlayerPrefs when the game starts
        LoadScore();
    }

    // Update health text
    public void UpdateHealth(int health)
    {
        healthText.text = "Health: " + health.ToString();
    }

    // Update score text
    public void UpdateScore(int score)
    {
        // Increase the score
        int newScore = PlayerPrefs.GetInt("Score", 0) + score;
        PlayerPrefs.SetInt("Score", newScore);
        scoreText.text = "Score: " + newScore.ToString();
    }

    // Load the score from PlayerPrefs
    void LoadScore()
    {
        int loadedScore = PlayerPrefs.GetInt("Score", 0);
        scoreText.text = "Score: " + loadedScore.ToString();
    }
}

