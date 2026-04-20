using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSurfaceHandler : MonoBehaviour
{
    [Header("Detección de superficie")]
    public LayerMask surfaceLayer;

    //Revisar choque
    Collider2D[] surfaceCollidersHit = new Collider2D[10];
    Vector3 lastSampledSurfacePosition = Vector3.one * 10000;

    //TipoDeSuperficie
    Surface.SurfaceTypes drivingOnSurface = Surface.SurfaceTypes.Road;

    //Otros componentes
    Collider2D carCollider;

    void Awake()
    {
        carCollider = GetComponentInChildren<Collider2D>();

        if (carCollider == null)
        {
            Debug.LogError("No se encontró Collider2D en el auto.");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if ((transform.position - lastSampledSurfacePosition).sqrMagnitude < 0.75f)
            return;

        ContactFilter2D contactFilter2D = new ContactFilter2D();
        contactFilter2D.layerMask = surfaceLayer;
        contactFilter2D.useLayerMask = true;
        contactFilter2D.useTriggers = true;

        int numberOfHits = Physics2D.OverlapCollider(carCollider, contactFilter2D, surfaceCollidersHit);

        float lastSurfaceZValue = -1000;

        for (int i = 0; i < numberOfHits; i++)
        {
            if (surfaceCollidersHit[i] == null)
                continue;

            Surface surface = surfaceCollidersHit[i].GetComponent<Surface>();

            if (surface == null)
                continue;

            if (surface.transform.position.z > lastSurfaceZValue)
            {
                drivingOnSurface = surface.surfaceType;
                lastSurfaceZValue = surface.transform.position.z;
            }
        }

        if (numberOfHits == 0)
            drivingOnSurface = Surface.SurfaceTypes.Road;

        lastSampledSurfacePosition = transform.position;

        Debug.Log($"Driving on {drivingOnSurface}");
    }

    public Surface.SurfaceTypes GetCurrentSurface()
    {
        return drivingOnSurface;
    }
}

