using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class YouWinScript : MonoBehaviour
{

    public TextMeshProUGUI timeElapsedText;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI difficultyText;

    public TextMeshProUGUI youWonText;

    public TextMeshProUGUI longText;

    public Image panel;

    private string longWinText = @"You have successfully taken control of the spaceship by killing and taking over the enemy robots.
You entered the control room and initiated a self destruct sequence on all remaining evil robots in the ship.
Humanity can now use the ship to leave the Planet, we are forever in your debt.
Thank you paradroid.";

    private string longLostText = @"You have failed to take control of the spaceship by killing and taking over the enemy robots.
You did not reach the control room to initiate a self destruct sequence on all remaining evil robots in the ship.
We were relying on you as our last hope to leave the planet, and you failed us.";

    private string longLostText2 = @"You decided to eject yourself into space. Why? Nobody knows.
Perhaps the top teleporter was more appealing than the bottom one.
We were relying on you as our last hope to leave the planet, and you failed us.";


    void Start()
    {
        timeElapsedText.SetText("Time Taken on this run: " + PlayerPrefs.GetFloat("TimeCounter").ToString("F1"));
        scoreText.SetText("Score: " + PlayerPrefs.GetInt("Score").ToString());
        int difficulty = PlayerPrefs.GetInt("Difficulty");
        string difficultyString = "";
        switch (difficulty)
        {
            case 1:
                difficultyString = "Easy";
                break;
            case 2:
                difficultyString = "Normal";
                break;
            case 3:
                difficultyString = "Hell Mode";
                break;
        }
        difficultyText.SetText("Difficulty: " + difficultyString);

        if (PlayerPrefs.GetInt("Won") == 1)
        {
            youWonText.SetText("You Won!!!");
            longText.SetText(longWinText);
            panel.color = new Color(0.369f, 0.584f, 0.584f);
        }
        else
        {
            youWonText.SetText("You Lost!!!");
            if (PlayerPrefs.GetInt("Won") == -1)
            {
                longText.SetText(longLostText2);
            }
            else
            {
                longText.SetText(longLostText);
            }
            panel.color = new Color(0.8f, 0.2f, 0.2f);
        }
    }

    public void ReturnToMainMenu()
    {
        PlayerPrefs.SetInt("CanContinue", 0);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu");
    }
}