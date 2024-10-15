using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    public HealthSystem healthSystem;
    public TextMeshProUGUI healthText;
    public Slider healthSlider;

    // Update is called once per frame
    void Update()
    {
        // Sets the slider value to the health %
        healthSlider.value = healthSystem.GetHealth() / healthSystem.GetMaxHealth();

        // Scale the health bar, so if max health is 200 the width of the health bar is 200, using the rect transform width
        healthSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(healthSystem.GetMaxHealth(), healthSlider.GetComponent<RectTransform>().sizeDelta.y);

        // Sets the health text to the health value, formatted as: HP: <health value>/<max health value>
        healthText.text = "HP: " + (int)healthSystem.GetHealth() + "/" + (int)healthSystem.GetMaxHealth();
    }
}
