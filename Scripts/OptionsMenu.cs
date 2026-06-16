using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class OptionsMenu : MonoBehaviour

//Permite a los jugadores personalizar su experiencia de juego.
{
    [SerializeField] private AudioMixer audioMixer;

    public void FullScreen(bool fullScreen)
    {
        Screen.fullScreen = fullScreen;
    }

    public void ChangeVolume(float volumen)
    {
        audioMixer.SetFloat("Volumen", volumen);
    }

    public void ChangeQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }
}
