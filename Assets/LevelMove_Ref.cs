using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelMove_Ref : MonoBehaviour
{
    public int sceneBuildIndex;
    public KeyCode teleportKey = KeyCode.E;
    public TMP_Text interactionText;
    public float teleportTime = 1.0f;
    private bool canTeleport = false;
    AudioManager audioManager;
    private SpriteRenderer playerSpriteRenderer;

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        playerSpriteRenderer = GameObject.FindGameObjectWithTag("Player").GetComponent<SpriteRenderer>();
    }

    //level move zoned enter, if collider is a player
    //move game to another scene
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canTeleport = true;
            interactionText.text = "Press " + teleportKey.ToString() + " to teleport";
            interactionText.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canTeleport = false;
            interactionText.enabled = false;
        }
    }
    void Update()
    {
        // Check if player is in range of teleporter and pressing the teleport key
        if (canTeleport && Input.GetKeyDown(teleportKey))
        {
            StartCoroutine(Teleport());
        }
    }
    IEnumerator Teleport()
    {
        // Play teleport sound effect
        audioManager.PlaySFX(audioManager.teleport);

        // Fade out player transparency
        float startTime = Time.time;
        Color startColor = playerSpriteRenderer.color;
        while (Time.time - startTime < teleportTime)
        {
            float t = (Time.time - startTime) / teleportTime;
            float alpha = Mathf.Lerp(startColor.a, 0.0f, t);
            playerSpriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        if (sceneBuildIndex == 4)
        {
            PlayerPrefs.SetInt("Won", 1);
        }

        if (sceneBuildIndex != -1) {
            // Switch scene
            SceneManager.LoadScene(sceneBuildIndex, LoadSceneMode.Single);
        }
        else
        {
            PlayerPrefs.SetInt("Won", -1);
            SceneManager.LoadScene(4, LoadSceneMode.Single);
        }


        // Load save data...

        PlayerPrefs.SetInt("isResetScene", 1);
    }
}
