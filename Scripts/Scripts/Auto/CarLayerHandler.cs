using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Este script se encarga de manejar las capas de renderizado y colisión del auto para que pueda pasar por debajo o por encima de los pasos elevados en la pista. Cambia la capa de renderizado del auto para que se dibuje por debajo o por encima del paso elevado dependiendo de si el auto está conduciendo por debajo o por encima del paso elevado, y también cambia la capa de colisión del auto para que pueda colisionar con el paso elevado cuando esté conduciendo por debajo, pero no colisione con el paso elevado cuando esté conduciendo por encima.

public class CarLayerHandler : MonoBehaviour
{
    public SpriteRenderer carOutlineSpriteRenderer;

    List<SpriteRenderer> defaultLayerSpriteRenderers = new List<SpriteRenderer>();

    List<Collider2D> overpassColliderList = new List<Collider2D>();
    List<Collider2D> underpassColliderList = new List<Collider2D>();

    Collider2D carCollider;

    //Estado
    bool isDrivingOnOverpass = false;

    void Awake()
    {
        foreach (SpriteRenderer spriteRenderer in gameObject.GetComponentsInChildren<SpriteRenderer>())
        {
            if (spriteRenderer.sortingLayerName == "Default")
                defaultLayerSpriteRenderers.Add(spriteRenderer);
        }

        foreach (GameObject overpassColliderGameObject in GameObject.FindGameObjectsWithTag("OverpassCollider"))
        {
            overpassColliderList.Add(overpassColliderGameObject.GetComponent<Collider2D>());
        }

        foreach (GameObject underpassColliderGameObject in GameObject.FindGameObjectsWithTag("UnderpassCollider"))
        {
            underpassColliderList.Add(underpassColliderGameObject.GetComponent<Collider2D>());
        }

        carCollider = GetComponentInChildren<Collider2D>();

        //Manejar a través del paso inferior.
        carCollider.gameObject.layer = LayerMask.NameToLayer("ObjectOnUnderpass");
    }

// Start is called once before the first execution of Update after the MonoBehaviour is created
void Start()
    {
        UpdateSortingAndCollisionLayers();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateSortingAndCollisionLayers()
    {
        if (isDrivingOnOverpass)
        {
            SetSortingLayer("RaceTrackOverpass");

            carOutlineSpriteRenderer.enabled = false;
        }
        else
        {
            SetSortingLayer("Default");

            carOutlineSpriteRenderer.enabled = true;
        }

        SetCollisionWithOverPass();
    }

    void SetCollisionWithOverPass()
    {
        foreach (Collider2D collider2D in overpassColliderList)
        {
            Physics2D.IgnoreCollision(carCollider, collider2D, !isDrivingOnOverpass);
        }

        foreach (Collider2D collider2D in underpassColliderList)
        {
            if (isDrivingOnOverpass)
            Physics2D.IgnoreCollision(carCollider, collider2D, true);
            else Physics2D.IgnoreCollision(carCollider, collider2D, false);
        }
    }

    void SetSortingLayer(string layerName)
    {
        foreach (SpriteRenderer spriteRenderer in defaultLayerSpriteRenderers)
        {
            spriteRenderer.sortingLayerName = layerName;
        }
    }
    public bool IsDrivingOnOverpass()
    {
        return isDrivingOnOverpass;
    }

    void OnTriggerEnter2D(Collider2D collider2d)
    {
        if (collider2d.CompareTag("UnderpassTrigger"))
        {
            isDrivingOnOverpass = false;

            carCollider.gameObject.layer = LayerMask.NameToLayer("ObjectOnUnderpass");

            UpdateSortingAndCollisionLayers();
        }
        else if (collider2d.CompareTag("OverpassTrigger"))
        {
            isDrivingOnOverpass = true;

            carCollider.gameObject.layer = LayerMask.NameToLayer("ObjectOnOverpass");

            UpdateSortingAndCollisionLayers();
        }
    }
}