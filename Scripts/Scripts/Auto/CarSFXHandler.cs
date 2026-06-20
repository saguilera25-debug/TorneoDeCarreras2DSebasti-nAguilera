using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// Este script se encarga de manejar los efectos de sonido del auto, como el sonido del motor, el sonido de los chirridos de neumáticos, el sonido de los golpes contra otros objetos, y el sonido de los saltos y aterrizajes. Cambia el volumen y el tono de los efectos de sonido basándose en la velocidad del auto, si está frenando, o si está derrapando, para hacer que los efectos de sonido sean más dinámicos y realistas. También se encarga de reproducir los efectos de sonido correspondientes cuando el auto salta o aterriza, o cuando choca contra otro objeto.

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

    //Variables locales que se usan para manejar el volumen y el tono de los efectos de sonido.
    float desiredEnginePitch = 0.5f;
    float tireScreechPitch = 0.5f;

    //Componentes que se necesitan para obtener información sobre el estado del auto y actualizar los efectos de sonido en consecuencia.
    TopDownCarController topDownCarController;

    void Awake()
    {
        topDownCarController = GetComponentInParent<TopDownCarController>();
    }

    // Update is called once per frame after the MonoBehaviour is created
    void Update()
    {
        UpdateEngineSFX();
        UpdateTiresScreechingSFX();
    }

    void UpdateEngineSFX()
    {
        //Manejar el efecto de sonido del motor basándose en la velocidad del auto. A medida que el auto va más rápido, el volumen y el tono del motor aumentan para hacer que el sonido sea más dinámico y realista.
        float velocityMagnitude = topDownCarController.GetVelocityMagnitude();

        //Incrementar el volumen del motor mientras que el auto va más rápido. Queremos que el volumen del motor sea más alto a medida que el auto va más rápido, pero también queremos que el volumen sea lo suficientemente bajo para que no sea molesto cuando el auto está detenido o moviéndose lentamente. Por eso multiplicamos la velocidad por un factor pequeño para obtener el volumen deseado, y luego usamos Mathf.Clamp para asegurarnos de que el volumen esté dentro de un rango razonable.
        float desiredEngineVolume = velocityMagnitude * 0.05f;

        //Pero mantiene un nivel minimo para que se reproduzca mientras que el auto está detenido. Esto hace que el sonido del motor se reproduzca incluso cuando el auto está detenido, lo que puede hacer que el juego se sienta más vivo y realista, en lugar de tener un silencio completo cuando el auto no se está moviendo.
        desiredEngineVolume = Mathf.Clamp(desiredEngineVolume, 0.2f, 1.0f);

        engineAudioSource.volume = Mathf.Lerp(engineAudioSource.volume, desiredEngineVolume, Time.deltaTime * 10);

        //Para añadir más variedad al sonido del motor también cambiamos el tono. Queremos que el tono del motor sea más alto a medida que el auto va más rápido, pero también queremos que el tono sea lo suficientemente bajo para que no suene molesto cuando el auto está detenido o moviéndose lentamente. Por eso multiplicamos la velocidad por un factor pequeño para obtener el tono deseado, y luego usamos Mathf.Clamp para asegurarnos de que el tono esté dentro de un rango razonable.
        desiredEnginePitch = velocityMagnitude * 0.2f;
        desiredEnginePitch = Mathf.Clamp(desiredEnginePitch, 0.5f, 2f);
        engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, desiredEnginePitch, Time.deltaTime * 1.5f);
    }

    void UpdateTiresScreechingSFX()
    {
        //Maneja el sonido de los chiridos de neumáticos. Queremos que el sonido de los chiridos de neumáticos se reproduzca cuando el auto esté derrapando, y que el volumen y el tono del sonido de los chiridos de neumáticos aumenten a medida que el auto derrapa más fuerte, para hacer que el sonido sea más dinámico y realista. Además, queremos que el sonido de los chiridos de neumáticos sea más fuerte y tenga un tono más bajo cuando el auto esté frenando, para hacer que el sonido sea más impactante y realista cuando el auto está frenando.
        if (topDownCarController.IsTireScreeching(out float lateralVelocity, out bool isBraking))
        {
            //Si el auto está frenando, queremos el chirrido de neumáticos sea más fuerte y también cambiar el tono. Esto hace que el sonido de los chirridos de neumáticos sea más impactante y realista cuando el auto está frenando, lo que puede mejorar la inmersión del jugador en el juego. Si el auto no está frenando pero sí está derrapando, entonces queremos que el volumen y el tono del sonido de los chirridos de neumáticos aumenten a medida que el auto derrapa más fuerte, para hacer que el sonido sea más dinámico y realista.
            if (isBraking)
            {
                tiresScreechingAudioSource.volume = Mathf.Lerp(tiresScreechingAudioSource.volume, 1.0f, Time.deltaTime * 10);
                tireScreechPitch = Mathf.Lerp(tireScreechPitch, 0.5f, Time.deltaTime * 10);
            }
            else
            {
                //Si no estamos frenando todavia reproducimos el sonido de los frenos si el jugador está derrapando. Queremos que el volumen del sonido de los chirridos de neumáticos aumente a medida que el auto derrapa más fuerte, para hacer que el sonido sea más dinámico y realista. Por eso multiplicamos la velocidad lateral por un factor pequeño para obtener el volumen deseado, y luego usamos Mathf.Clamp para asegurarnos de que el volumen esté dentro de un rango razonable.
                tiresScreechingAudioSource.volume = Mathf.Abs(lateralVelocity) * 0.05f;
                tireScreechPitch = Mathf.Abs(lateralVelocity) * 0.1f;
            }
        }
        //Desvanece el sonido de los chirridos de neumáticos si no estamos frenando. Esto hace que el sonido de los chirridos de neumáticos se desvanezca gradualmente cuando el auto deja de derrapar o de frenar, lo que puede mejorar la inmersión del jugador en el juego al hacer que los efectos de sonido sean más suaves y realistas.
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
        //Consigue la velocidad relativa de la colisión para determinar el volumen del efecto de sonido del golpe. Queremos que el volumen del efecto de sonido del golpe sea más alto a medida que la velocidad relativa de la colisión es mayor, para hacer que el sonido sea más impactante y realista cuando el auto choca contra otro objeto. Por eso multiplicamos la velocidad relativa por un factor pequeño para obtener el volumen deseado, y luego usamos Mathf.Clamp para asegurarnos de que el volumen esté dentro de un rango razonable.
        float relativeVelocity = collision2D.relativeVelocity.magnitude;

        float volume = relativeVelocity * 0.1f;

        carHitAudioSource.pitch = Random.Range(0.95f, 1.05f);
        carHitAudioSource.volume = volume;

        if (!carHitAudioSource.isPlaying)
            carHitAudioSource.Play();
    }
}