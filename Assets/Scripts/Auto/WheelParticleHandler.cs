using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelParticleHandler : MonoBehaviour
{
    //Variables locales
    float particleEmissionRate = 0;

    //Componentes
    TopDownCarController topDownCarController;
    ParticleSystem particleSystemSmoke;
    ParticleSystem.EmissionModule particleSystemEmissionModule;
    ParticleSystem.MainModule particleSystemMainModule;

    void Awake()
    {
        //Consigue el controlador de autos top-down.
        topDownCarController = GetComponentInParent<TopDownCarController>();

        //Obtén el sistema de particulas
        particleSystemSmoke = GetComponent<ParticleSystem>();

        //Obtén el componente de emisión
        particleSystemEmissionModule = particleSystemSmoke.emission;

        //Obtén el módulo principal.
        particleSystemMainModule = particleSystemSmoke.main;

        //Pone la emisión a 0.
        particleSystemEmissionModule.rateOverTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //Reduce las particulas con el tiempo.
        particleEmissionRate = Mathf.Lerp(particleEmissionRate, 0, Time.deltaTime * 5);
        particleSystemEmissionModule.rateOverTime = particleEmissionRate;

        //Revisa en que superficie estamos conduciendo y aplica diferentes ajustes.
        switch (topDownCarController.GetSurface())
        {
            case Surface.SurfaceTypes.Road:
                particleSystemMainModule.startColor = new Color(0.83f, 0.83f, 0.83f);
                break;

            case Surface.SurfaceTypes.Sand:
                particleEmissionRate = topDownCarController.GetVelocityMagnitude();
                particleSystemMainModule.startColor = new Color(0.64f, 0.42f, 0.24f);
                break;

            case Surface.SurfaceTypes.Grass:
                particleEmissionRate = topDownCarController.GetVelocityMagnitude();
                particleSystemMainModule.startColor = new Color(0.15f, 0.4f, 0.13f);
                break;

            case Surface.SurfaceTypes.Oil:
                particleSystemMainModule.startColor = new Color(0.2f, 0.2f, 0.2f);
                break;
        }

        if (topDownCarController.IsTireScreeching(out float lateralVelocity, out bool isBraking))
        {
            //Si los neumáticos están chirriando entonces emitiremos humo. Si el jugador está frenando entonces emitiremos un montón de humo.
            if (isBraking)
                particleEmissionRate = 30;
            //Si el jugador está derarpando emitiremos humo based en cuánto estamos derrapando.
            else particleEmissionRate = Mathf.Abs(lateralVelocity) * 2;
        }
    }
}
