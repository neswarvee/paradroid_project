using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizUIManager : MonoBehaviour
{
    public GameObject quizUIPanel;
    public TextMeshProUGUI quizQuesitonText;
    public TextMeshProUGUI quizAnswer1Text;
    public TextMeshProUGUI quizAnswer2Text;
    public TextMeshProUGUI quizAnswer3Text;
    public TextMeshProUGUI quizAnswer4Text;
    public TextMeshProUGUI[] answerTexts;

    public TextMeshProUGUI xText;

    public Image tickIcon;

    public Image crossIcon;

    private string letters;

    private int selectedIndex = 0;

    private int correctAnswerIndex;

    private int numOfAnswers;

    private Color lightBlue = new Color(0.2f, 0.4f, 1f, 1f);

    private string questionTextToScroll = "";

    private string[] answerTextsToScroll = new string[] {"", "", "", ""};

    private string takeOverString = "Initiating Takeover Procedure ...    ";

    private string takeOverString2 = "Loading Next Stage of Takeover Procedure ...    ";

    private float takeOverStringTime = 1;

    private bool takeOverStringDisplayed;

    private float scrollTimer = 0;

    private int scrollSpeed = 5;

    private float elapsedTime = 0;
    private float lastTime = 0;

    private bool takeOverMessagePhase = false;

    AudioManager audioManager;

    private bool isPaused = false;

    private QuizManager quizManager;
    void Start()
    {
        answerTexts = new TextMeshProUGUI[] { quizAnswer1Text, quizAnswer2Text, quizAnswer3Text, quizAnswer4Text };
        SetVisibility(false);
        quizQuesitonText.enabled = false;
        letters = "ABCD";
        SetSelectedIndex(0);
    }
    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        quizManager = gameObject.GetComponent<QuizManager>();
    }

    public bool GetTakeOverMessagePhase()
    {
        return takeOverMessagePhase;
    }

    public void SetXText(string text)
    {
        xText.text = text;
    }

    public void SetSelectedIndex(int index)
    {
        answerTexts[selectedIndex].color = Color.white;
        selectedIndex = index;
        answerTexts[selectedIndex].color = lightBlue;
    }

    public void SetCorrectAnswerIndex(int index)
    {
        correctAnswerIndex = index;
    }



    public bool ApplyMarking()
    {
        for (int i = 0; i < numOfAnswers; i++)
        {
            if (i == correctAnswerIndex) {
                answerTexts[i].color = Color.green;
            }
            else {
                
                answerTexts[i].color = Color.red;
            }
        }
        if (selectedIndex == correctAnswerIndex) {
            audioManager.PlaySFX(audioManager.correct);
            tickIcon.enabled = true;
            return true;
        }
        else
        {
            audioManager.PlaySFX(audioManager.wrong);
            crossIcon.enabled = true;
            return false;

        }
    }

    public void SetQuestionText(string text)
    {
        quizQuesitonText.text = "";
        questionTextToScroll = "Q: " + text;
    }

    public void SetAnswerText(List<string> answers)
    {
        for (int i = 0; i < 4; i++)
        {
            answerTexts[i].text = "";
            answerTextsToScroll[i] = "";
        }

        int j = 0;
        foreach (string answer in answers)
        {
            answerTextsToScroll[j] = letters[j] + ") " + answer;
            answerTexts[j].color = Color.white;
            j += 1;
        }
        numOfAnswers = answers.Count;

        tickIcon.enabled = false;
        crossIcon.enabled = false;
    }



    public void SetVisibility(Boolean visible)
    {
        quizUIPanel.SetActive(visible);
        quizQuesitonText.enabled = visible;
        takeOverStringDisplayed = false;
        scrollTimer = 0;
        xText.enabled = false;
        takeOverMessagePhase = true;


        if (visible)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    public void PauseGame()
    {
        if (!isPaused)
        {
            // If 'PauseCounter' in PlayerPrefs is 1 or higher, then increment it by 1, otherwise set it to 1 and set Time.timeScale to 0f
            PlayerPrefs.SetInt("PauseCounter", PlayerPrefs.GetInt("PauseCounter") + 1);
            if (PlayerPrefs.GetInt("PauseCounter") == 1)
            {
                Time.timeScale = 0f;
                isPaused = true;
            } else if (PlayerPrefs.GetInt("PauseCounter") < 0)
            {
                PlayerPrefs.SetInt("PauseCounter", 0);
            }
        }
    }

    public void ResumeGame()
    {
        // If 'PauseCounter' in PlayerPrefs is 1 or higher, then decrement it by 1, otherwise set it to 0 and set Time.timeScale to 1f
        PlayerPrefs.SetInt("PauseCounter", PlayerPrefs.GetInt("PauseCounter") - 1);
        if (PlayerPrefs.GetInt("PauseCounter") == 0)
        {
            Time.timeScale = 1f;
            isPaused = false;
        } else if (PlayerPrefs.GetInt("PauseCounter") < 0)
        {
            PlayerPrefs.SetInt("PauseCounter", 0);
            Time.timeScale = 1f;
            isPaused = false;
        }
    }

    void Update()
    {

        float currentTime = Time.realtimeSinceStartup;
        elapsedTime = currentTime - lastTime;
        lastTime = currentTime;

        if (lastTime != 0)
        {
            scrollTimer += elapsedTime;
        }

        if (!takeOverStringDisplayed)
        {   
            string scrollingString;
            if (quizManager.questionNumberInSet == 0)
            {
                scrollingString = takeOverString;
            }
            else
            {
                scrollingString = takeOverString2;
            }

            if (quizQuesitonText.text.Length < scrollingString.Length && scrollTimer > 1/scrollSpeed * quizQuesitonText.text.Length)
            {
                quizQuesitonText.text = quizQuesitonText.text + scrollingString[quizQuesitonText.text.Length];
            }
            else if (!(scrollTimer < 1/scrollSpeed * scrollingString.Length + takeOverStringTime))
            {
                takeOverStringDisplayed = true;
                quizQuesitonText.text = "";
                scrollTimer = 0;
                xText.enabled = true;
                takeOverMessagePhase = false;
            }
        }
        else
        {
            if (quizQuesitonText.text.Length < questionTextToScroll.Length &&
             scrollTimer > 1/scrollSpeed * quizQuesitonText.text.Length)
            {
                quizQuesitonText.text = quizQuesitonText.text + questionTextToScroll[quizQuesitonText.text.Length];
            }

            for (int i = 0; i < 4; i++)
            {
                if (answerTexts[i].text.Length < answerTextsToScroll[i].Length  &&
                 scrollTimer > 1/scrollSpeed * answerTexts[i].text.Length)
                {
                    answerTexts[i].text = answerTexts[i].text + answerTextsToScroll[i][answerTexts[i].text.Length]; 
                }
            }
        }
    }
}
