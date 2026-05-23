using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        int underpassLayer = LayerMask.NameToLayer("ObjectOnUnderpass");

        if (underpassLayer != -1)
        {
            carCollider.gameObject.layer = underpassLayer;
        }
        else
        {
            Debug.LogError("Layer 'ObjectOnUnderpass' does not exist!");
        }
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

            int underpassLayer = LayerMask.NameToLayer("ObjectOnUnderpass");

            if (underpassLayer != -1)
            {
                carCollider.gameObject.layer = underpassLayer;
            }
            else
            {
                Debug.LogError("Layer 'ObjectOnUnderpass' does not exist!");
            }

            UpdateSortingAndCollisionLayers();
        }
        else if (collider2d.CompareTag("OverpassTrigger"))
        {
            isDrivingOnOverpass = true;

            int overpassLayer = LayerMask.NameToLayer("ObjectOnOverpass");

            if (overpassLayer != -1)
            {
                carCollider.gameObject.layer = overpassLayer;
            }
            else
            {
                Debug.LogError("Layer 'ObjectOnOverpass' does not exist!");
            }

            UpdateSortingAndCollisionLayers();
        }
    }
}
