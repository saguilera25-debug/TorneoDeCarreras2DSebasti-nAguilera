using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointNode : MonoBehaviour
{
    //Cuál es la velocidad máxima permitida cuando pasamos por este punto de referencia.
    [Header("Velocidad establecida cuando llegamos al punto de referencia")]
    public float maxSpeed = 0;

    [Header("Este es el punto de referencia que nos estamos dirigiendo, pero no estamos cerca de el")]
    public float minDistanceToReachWaypoint = 5;

    public WaypointNode[] nextWaypointNode;
}
