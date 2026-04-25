using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointNode : MonoBehaviour
{
    [Header("Este es el punto de referencia que nos estamos dirigiendo, pero no estamos cerca de el")]
    public float minDistanceToReachWaypoint = 5;

    [Header("Velocidad máxima permitida en este punto")]
    public float maxSpeed = 10f;

    public WaypointNode[] nextWaypointNode;
}
