using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundControlManager : MonoBehaviour
{
    [SerializeField] Slider volumeSlider;

    void Start()
    {
        if(PlayerPrefs.HasKey("musicVolume"))
        {
            Load();
        }

        else
        {
            PlayerPrefs.SetFloat("musicVolume", 0.3f);
            Load();
        }
    }

    public void ChangeVolume()
    {
        AudioListener.volume = volumeSlider.value;
        Save();
    }

    private void Load()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("musicVolume");
    }

    private void Save()
    {
        PlayerPrefs.SetFloat("musicVolume", volumeSlider.value);
    }
}