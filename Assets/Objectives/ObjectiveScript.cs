using UnityEngine;
using TMPro;

[System.Serializable]
public class Objective
{
    public string description; // Description of the objective
    public bool completed; // Flag indicating whether the objective is completed

    // Constructor
    public Objective(string desc)
    {
        description = desc;
        completed = false;
    }
}
