using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class AudioManager : MonoBehaviour
{
    [Header("----Audio Source----")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("----Audio Clip----")]
    public AudioClip traversing;
    public AudioClip boss;
    public AudioClip movement;
    public AudioClip bump;
    public AudioClip shoot;
    public AudioClip correct;
    public AudioClip wrong;
    public AudioClip teleport;
    public AudioClip main;
    public AudioClip space;


    // Adjust these values to control the volume
    public float musicVolume = 0.5f;
    public float SFXVolume = 0.5f;
    public float bulletVolume = 0.25f;

    private void Start()
    {
        // Set the volume for music and SFX
        musicSource.volume = musicVolume;
        SFXSource.volume = SFXVolume;

        // Set music to loop
        musicSource.loop = true;
        
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        
        // Check the current scene index and assign the appropriate clip
        switch (sceneIndex)
        {
            case 0:
                musicSource.clip = main;
                break;
            case 1:
                musicSource.clip = traversing;
                break;
            case 2:
                musicSource.clip = space;
                break;
            case 3:
                musicSource.clip = boss;
                break;
            default:
                musicSource.clip = traversing; // Default to traversing clip
                break;
        }
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip){
      if (clip == shoot) // If the played sound effect is the bullet sound effect
        {
            SFXSource.PlayOneShot(clip, bulletVolume); // Adjust volume for bullet sound effect
        }
        else
        {
            SFXSource.PlayOneShot(clip, SFXVolume); // Play other sound effects with normal volume
        }
    } 
}
