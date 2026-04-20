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

    void Awake()
    {
        //Consigue el controlador de autos top-down.
        topDownCarController = GetComponentInParent<TopDownCarController>();

        //Obtén el sistema de particulas
        particleSystemSmoke = GetComponent<ParticleSystem>();

        //Obtén el componente de emisión
        particleSystemEmissionModule = particleSystemSmoke.emission;

        //Pone la emisión a 0.
        particleSystemEmissionModule.rateOverTime = 0;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Reduce las particulas con el tiempo.
        particleEmissionRate = Mathf.Lerp(particleEmissionRate, 0, Time.deltaTime * 5);
        particleSystemEmissionModule.rateOverTime = particleEmissionRate;

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
