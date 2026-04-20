using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Surface : MonoBehaviour
{
    public enum SurfaceTypes { Road, Sand, Grass, Oil };

    [Header("Surface")]
    public SurfaceTypes surfaceType;

    void Start()
    {

    }
}
