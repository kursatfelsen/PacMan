using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider slider;

    public void Start()
    {
        slider.value = PlayerPrefs.GetFloat("SliderValue", 0.75f);

    }

    public void SetVolume(float decimalVolume)

    {
        PlayerPrefs.SetFloat("SliderValue", decimalVolume);
        var dbVolume = Mathf.Log10(decimalVolume) * 20;

        if (decimalVolume == 0.0f)

        {

            dbVolume = -80.0f;

        }

        audioMixer.SetFloat("volume", dbVolume);
        

    }
}
