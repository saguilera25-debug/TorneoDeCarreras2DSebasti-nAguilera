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
        //Consigue el controlador de autos top-down.
        topDownCarController = GetComponentInParent<TopDownCarController>();

        carLayerHandler = GetComponentInParent<CarLayerHandler>();

        //Obtén el componente de trail renderer
        trailRenderer = GetComponent<TrailRenderer>();

        //Ajusta el trail renderer para que no se emita al inicio.
        trailRenderer.emitting = false;
    }


    // Update is called once per frame
    void Update()
    {
        trailRenderer.emitting = false;

        //Si las llantas estan chirriando entonces emitiremos un camino.
        if (topDownCarController.IsTireScreeching(out float lateralVelocity, out bool isBraking))
        {
            if (carLayerHandler.IsDrivingOnOverpass() && isOverpassEmitter)
                trailRenderer.emitting = true;

            if (!carLayerHandler.IsDrivingOnOverpass() && !isOverpassEmitter)
                trailRenderer.emitting = true;
        }
    }
}
