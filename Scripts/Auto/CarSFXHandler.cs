using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class CarSFXHandler : MonoBehaviour
{
    [Header("Mixers")]
    public AudioMixer audioMixer;

    [Header("Muestras de audio")]
    public AudioSource tiresScreechingAudioSource;
    public AudioSource engineAudioSource;
    public AudioSource carHitAudioSource;
    public AudioSource carJumpAudioSource;
    public AudioSource carJumpLandingAudioSource;

    //Variables locales
    float desiredEnginePitch = 0.5f;
    float tireScreechPitch = 0.5f;

    //Componentes
    TopDownCarController topDownCarController;

    void Awake()
    {
        topDownCarController = GetComponentInParent<TopDownCarController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Ejemplo para grabar, mueve esta parte a cualquier ajuste de un script que el juego va a utilizar.
        //audioMixer.SetFloat("SFXVolume", 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateEngineSFX();
        UpdateTiresScreechingSFX();
    }

    void UpdateEngineSFX()
    {
        //Manejar el efecto de sonido del motor
        float velocityMagnitude = topDownCarController.GetVelocityMagnitude();

        //Incrementar el volumen del motor mientras que el auto va más rápido.
        float desiredEngineVolume = velocityMagnitude * 0.05f;

        //Pero mantiene un nivel minimo para que se reproduzca mientras que el auto está detenido.
        desiredEngineVolume = Mathf.Clamp(desiredEngineVolume, 0.2f, 1.0f);

        engineAudioSource.volume = Mathf.Lerp(engineAudioSource.volume, desiredEngineVolume, Time.deltaTime * 10);

        //Para añadir más variedad al sonido del motor también cambiamos el tono.
        desiredEnginePitch = velocityMagnitude * 0.2f;
        desiredEnginePitch = Mathf.Clamp(desiredEnginePitch, 0.5f, 2f);
        engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, desiredEnginePitch, Time.deltaTime * 1.5f);
    }

    void UpdateTiresScreechingSFX()
    {
        //Maneja el sonido de los chiridos de neumáticos
        if (topDownCarController.IsTireScreeching(out float lateralVelocity, out bool isBraking))
        {
            //Si el auto está frenando, queremos el chirrido de neumáticos sea más fuerte y también cambiar el tono.
            if (isBraking)
            {
                tiresScreechingAudioSource.volume = Mathf.Lerp(tiresScreechingAudioSource.volume, 1.0f, Time.deltaTime * 10);
                tireScreechPitch = Mathf.Lerp(tireScreechPitch, 0.5f, Time.deltaTime * 10);
            }
            else
            {
                //Si no estamos frenando todavia reproducimos el sonido de los frenos si el jugador está derrapando.
                tiresScreechingAudioSource.volume = Mathf.Abs(lateralVelocity) * 0.05f;
                tireScreechPitch = Mathf.Abs(lateralVelocity) * 0.1f;
            }
        }
        //Desvanece el sonido de los chirridos de neumáticos si no estamos frenando.
        else tiresScreechingAudioSource.volume = Mathf.Lerp(tiresScreechingAudioSource.volume, 0, Time.deltaTime * 10);
    }

    public void PlayJumpSFX()
    {
        carJumpAudioSource.Play();
    }

    public void PlayLandingSFX()
    {
        carJumpLandingAudioSource.Play();
    }

    void OnCollisionEnter2D(Collision2D collision2D)
    {
        //Consigue la velocidad relativa de la colisión
        float relativeVelocity = collision2D.relativeVelocity.magnitude;

        float volume = relativeVelocity * 0.1f;

        carHitAudioSource.pitch = Random.Range(0.95f, 1.05f);
        carHitAudioSource.volume = volume;

        if (!carHitAudioSource.isPlaying)
            carHitAudioSource.Play();
    }
}