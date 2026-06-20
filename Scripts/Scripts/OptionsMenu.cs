using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class OptionsMenu : MonoBehaviour

// Este script se encarga de manejar el menú de opciones del juego. Tiene funciones públicas para cambiar la pantalla completa, el volumen y la calidad gráfica del juego, y también tiene una referencia a un AudioMixer para cambiar el volumen del juego.
{
    [SerializeField] private AudioMixer audioMixer;

    public void FullScreen(bool fullScreen)
    {
        Screen.fullScreen = fullScreen;
    }

    public void ChangeVolume(float volume)
    {
        audioMixer.SetFloat("Volume", volume);
    }

    public void ChangeQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }
}
