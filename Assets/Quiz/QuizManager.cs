using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;


public class QuizManager : MonoBehaviour
{

    public Quiz quiz;
    public int topicNumber;

    public QuizUIManager quizUIManager;

    public Boolean quizOpen;

    public Boolean inMarkingState;

    public GameObject player;

    public GameObject[] enemies;

    private int selectedIndex;

    private QuizQuestion question;

    private int takeoverDistance = 3;

    private bool answerCorrect = false;

    private GameObject takeOverEnemy = null;

    private int questionNumber;

    public int questionsPerSet;

    public int questionNumberInSet = 0;


    void Start()
    {

        string filePath = Application.streamingAssetsPath + "/quiz.txt";

        // Read in the quiz text file using the ReadLinesFromFile function
        string[] lines = ReadLinesFromFile(filePath);

        quiz = MakeQuiz(lines);

        quizOpen = false;

        selectedIndex = 0;
        
        enemies = GameObject.FindGameObjectsWithTag("Enemy");

        questionNumber = 0;

        if (!PlayerPrefs.HasKey("Difficulty"))
        {
            PlayerPrefs.SetInt("Difficulty", 1);
        }
        questionNumberInSet *= PlayerPrefs.GetInt("Difficulty");
    }
    
    /* MakeQuiz, parse information from the lines taken from the quiz text file, use them to construct a tree of
        Quiz, QuizTopic and QuizQuestion objects to be used as necessary to extract quiz data during the game*/
    Quiz MakeQuiz(string[] lines)
    {
        Quiz quiz = new Quiz();

        QuizTopic topic = null;
        QuizQuestion question = null;
        int answerCounter = 0;

        foreach (string line in lines) {

            if (line.Length > 2)
            {
                if (line[0] == '#') 
                {
                    topic = new QuizTopic();
                    topic.SetTopicName(line.Substring(1));
                    quiz.AddTopic(topic);
                }
                else if (line.Substring(0,2) == "Q:")
                {
                    question = new QuizQuestion();
                    question.SetQustionText(line.Substring(2));
                    answerCounter = 0;
                    topic.AddQuestion(question);
                }
                else if (line[0] == '$' || line[0] == '+')
                {
                    question.AddAnswer(line.Substring(1));
                    if (line[0] == '+') {
                        question.SetCorrectAnswerIndex(answerCounter);
                    }
                    answerCounter += 1;
                }
            }
        }

        foreach (QuizTopic t in quiz.topics)
        {
            t.ShuffleQuestions();
        }

        return quiz;
    }

    /* Quiz, This class constructs the highest level object in the tree representing the quiz, it has topics which
        can be added with the AddTopic function
    */
    public class Quiz
    {
        public List<QuizTopic> topics = new List<QuizTopic>();

        public void AddTopic(QuizTopic topic)
        {
            topics.Add(topic);
        }
    }

    /* QuizTopic, This class can be used to construct objects representing whole quiz topics e.g AI, topics are the second
         highest level object in the quiz tree, they contain questions which can be added with AddQuestion function and
         a TopicName attribute which can be set with the SetTopicName function
    */
    public class QuizTopic
    {
        public string topicName;
        public List<QuizQuestion> questions = new List<QuizQuestion>();

        public void SetTopicName(string topicName)
        {
            this.topicName = topicName;
        }
        
        public void AddQuestion(QuizQuestion question)
        {
            questions.Add(question);
        }

        public void ShuffleQuestions()
        {
            Shuffle(questions);
        }
    }

    static void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /* QuizQuestion, This class can be used to construct objects representing questions in the quiz,
     questions have a question text attribute which can be set with the SetQuestionText unction and answers
     which can be added with the AddAnswer function, the index of the correct answer can be added with the 
     SetCorrectAnswerIndex function
    */
    public class QuizQuestion
    {
        public string questionText;
        public List<string> answers = new List<string>();

        public int correctAnswerIndex;

        public void SetQustionText(string questionText)
        {
            this.questionText = questionText;
        }
        
        public void AddAnswer(string answer)
        {
            answers.Add(answer);
        }

        public void SetCorrectAnswerIndex(int correctAnswerIndex)
        {
            this.correctAnswerIndex = correctAnswerIndex;
        }
    }

    /* ReadLinesFromFile, reads lines from the text file representing the quiz throwing an error if the file
        cannot be found and logging this in the console
    */
    string[] ReadLinesFromFile(string filePath)
    {
        string[] lines = new string[0];

        // Check if the file exists
        if (File.Exists(filePath))
        {
            // Read all lines from the file
            lines = File.ReadAllLines(filePath);
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
        }

        return lines;
    }

    /* DisplayRandomQuestion, generates a random question from the selected topic in the quiz tree and displays it to the screen
    using the quizUIManager object
    */
    void DisplayNextQuestion()
    {
        question = quiz.topics[topicNumber].questions[questionNumber];
        quizUIManager.SetQuestionText(question.questionText);
        quizUIManager.SetAnswerText(question.answers);
        selectedIndex = 0;
        quizUIManager.SetSelectedIndex(0);
        quizUIManager.SetXText("[ x ] Submit");
        questionNumber += 1;
        if (questionNumber >= quiz.topics[topicNumber].questions.Count)
        {
            questionNumber = 0;
        }

    }

    /*InRangeOfEnemy, detects and returns (boolean) whether the player is in a sufficiently low range of any enemy in order to
        be able to do the take over quiz */
    Boolean InRangeOfEnemy(float distance)
    {
        if (distance < 0) {
            return false;
        }
        else
        {
            Transform playerTransform = player.transform;
            float playerX = playerTransform.position.x;
            float playerY = playerTransform.position.y;

            foreach (GameObject enemy in enemies)
            {
                if (enemy != null)
                {
                    Enemy enemyScript = enemy.GetComponent<Enemy>();
                    if (!enemyScript.GetDying() && !enemyScript.GetTased())
                    {
                        Transform enemyTransform = enemy.transform;
                        float enemyX = enemyTransform.position.x;
                        float enemyY = enemyTransform.position.y;

                        if (Math.Sqrt(Math.Pow(enemyX - playerX, 2) + Math.Pow(enemyY - playerY,2)) < distance)
                        {
                            takeOverEnemy = enemy;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }

    /* Update, contains some logic to update the state of the quiz including whether it is open and in the marking state
        using the X key as an input to submit and answr and the W and S keys to change the selected answer */
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && InRangeOfEnemy(takeoverDistance) && !quizOpen)
        {
            quizOpen = true;
            DisplayNextQuestion();
            quizUIManager.SetVisibility(true);
            quizUIManager.SetSelectedIndex(selectedIndex);
            quizUIManager.SetCorrectAnswerIndex(question.correctAnswerIndex);
            inMarkingState = false;
        }

        if (Input.GetKeyDown(KeyCode.W) && quizOpen && !inMarkingState)
        {
            selectedIndex -= 1;
            if (selectedIndex < 0)
            {
                selectedIndex = question.answers.Count - 1;
            }
            quizUIManager.SetSelectedIndex(selectedIndex);
        }

        if (Input.GetKeyDown(KeyCode.S) && quizOpen && !inMarkingState)
        {
            selectedIndex += 1;
            if (selectedIndex > question.answers.Count - 1)
            {
                selectedIndex = 0;
            }
            quizUIManager.SetSelectedIndex(selectedIndex);
        }

        if (Input.GetKeyDown(KeyCode.X) && quizOpen)
        {
            if (!quizUIManager.GetTakeOverMessagePhase())
            {
                if (!inMarkingState)
                {
                    inMarkingState = true;
                    answerCorrect = quizUIManager.ApplyMarking();
                    quizUIManager.SetXText("[ x ] Continue");
                }
                else {
                    if (questionNumberInSet < questionsPerSet - 1 && answerCorrect) {
                        questionNumberInSet += 1;

                        quizOpen = true;
                        DisplayNextQuestion();
                        quizUIManager.SetVisibility(true);
                        quizUIManager.SetSelectedIndex(selectedIndex);
                        quizUIManager.SetCorrectAnswerIndex(question.correctAnswerIndex);
                        inMarkingState = false;
                    }
                    else {
                        questionNumberInSet = 0;
                        quizOpen = false;
                        quizUIManager.SetVisibility(false);
                        quizUIManager.SetSelectedIndex(0);
                        quizUIManager.SetCorrectAnswerIndex(0);
                        inMarkingState = false;
                        if (answerCorrect)
                        {
                            ParadroidController controller = player.GetComponent<ParadroidController>();
                            controller.TaseEnemy(takeOverEnemy);
                        }
                    }
                }
            }
            
        }
    }
}
