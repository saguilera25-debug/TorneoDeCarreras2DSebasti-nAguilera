using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Este script se encarga de manejar el TrailRenderer que emiten las ruedas del auto. Activa o desactiva la emisión del TrailRenderer basándose en si los neumáticos están chirriando, y también dependiendo de si el auto está conduciendo por debajo o por encima de un paso elevado, para asegurarse de que el TrailRenderer se dibuje por debajo o por encima del paso elevado en consecuencia.

public class WheelTrailRendererHandler : MonoBehaviour
{
    public bool isOverpassEmitter = false;

    //Componentes
    TopDownCarController topDownCarController;
    TrailRenderer trailRenderer;
    CarLayerHandler carLayerHandler;

    void Awake()
    {
        topDownCarController = GetComponentInParent<TopDownCarController>();
        carLayerHandler = GetComponentInParent<CarLayerHandler>();
        trailRenderer = GetComponent<TrailRenderer>();

        if (topDownCarController == null)
            Debug.LogError("No se encontró TopDownCarController");

        if (carLayerHandler == null)
            Debug.LogError("No se encontró CarLayerHandler");

        if (trailRenderer == null)
            Debug.LogError("No se encontró TrailRenderer");

        if (trailRenderer != null)
            trailRenderer.emitting = false;
    }


    // Update is called once per frame
    void Update()
    {
        if (topDownCarController == null || trailRenderer == null || carLayerHandler == null)
            return;

        trailRenderer.emitting = false;

        if (topDownCarController.IsTireScreeching(out float lateralVelocity, out bool isBraking))
        {
            if (carLayerHandler.IsDrivingOnOverpass() && isOverpassEmitter)
                trailRenderer.emitting = true;

            if (!carLayerHandler.IsDrivingOnOverpass() && !isOverpassEmitter)
                trailRenderer.emitting = true;
        }
    }
}
