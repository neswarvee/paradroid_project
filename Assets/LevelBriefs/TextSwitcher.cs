using UnityEngine;
using TMPro;

public class TextSwitcher : MonoBehaviour
{
    public TMP_Text initialText; // Reference to the first text object
    public TMP_Text secondText; // Reference to the second text object

    private void Update()
    {
        // Check for user input
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // Toggle visibility of text objects
            initialText.gameObject.SetActive(false);
            secondText.gameObject.SetActive(true);
        }
    }
}
