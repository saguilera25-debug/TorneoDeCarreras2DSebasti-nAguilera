using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
