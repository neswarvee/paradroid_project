using UnityEngine;
using TMPro;

public class ObjectiveManager : MonoBehaviour
{
    public TMP_Text objectiveText; // Reference to the TMP_Text element to display objectives
    public Objective[] objectives; // Array of objectives for the level
    private int currentObjectiveIndex = 0; // Index of the current objective

    private void Start()
    {
        // Display the initial objective
        DisplayObjective();
    }

    // Display the current objective on the screen
    private void DisplayObjective()
    {
        // If objectiveText doesn't exist then return
        if (objectiveText == null)
        {
            Debug.LogWarning("Objective Text not set in ObjectiveManager.");
            return;
        }
        objectiveText.text = objectives[currentObjectiveIndex].description;
    }

    // Method to call when an objective is completed
    public void CompleteObjective()
    {
        objectives[currentObjectiveIndex].completed = true;
        // Move to the next objective
        currentObjectiveIndex++;
        // Check if all objectives are completed
        if (currentObjectiveIndex < objectives.Length)
        {
            DisplayObjective();
        }
        else
        {
            // All objectives completed, do something (e.g., load next level)
            Debug.Log("All objectives completed!");
        }
    }
}
