using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DrawPathHandler : MonoBehaviour
{
    public Transform transformRootObject;

    private WaypointNode[] waypointNodes;

    void OnDrawGizmos()
    {
        if (transformRootObject == null)
            return;

        Gizmos.color = Color.blue;

        // Obtener nodos hijos
        waypointNodes = transformRootObject.GetComponentsInChildren<WaypointNode>();

        if (waypointNodes == null || waypointNodes.Length == 0)
            return;

        foreach (WaypointNode waypoint in waypointNodes)
        {
            if (waypoint == null || waypoint.nextWaypointNode == null)
                continue;

            foreach (WaypointNode nextWayPoint in waypoint.nextWaypointNode)
            {
                if (nextWayPoint == null)
                    continue;

                Gizmos.DrawLine(
                    waypoint.transform.position,
                    nextWayPoint.transform.position
                );
            }
        }
    }
}