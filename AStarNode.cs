using System;
using System.Collections.Generic;
using UnityEngine;

public class AStarNode
{
    //La posición en la cuadrícula
    public Vector2Int gridPosition;

    //Lista de los vecinos de los nodos.
    public List<AStarNode> neighbours = new List<AStarNode>();

    //El nodo es un obstáculo.
    public bool isObstacle = false;

    //Distancia del punto de partida al nodo.
    public int gCostDistanceFromStart = 0;

    //Distancia del nodo al objetivo.
    public int hCostDistanceFromGoal = 0;

    //El costo total del movimiento a la posición de la cuadrícula.
    public int fCostTotal = 0;

    //El orden en el que fue escogido.
    public int pickedOrder = 0;

    //Indica que se debe comprobar si el coste ya ha sido calculado.
    bool isCostCalculated = false;

    public AStarNode(Vector2Int gridPosition_)
    {
        gridPosition = gridPosition_;
    }

    public void CalculateCostsForNode(Vector2Int aiPosition, Vector2Int aiDestination)
    {
        //Si ya hemos calculado el nodo entonces no necesitamos hacerlo de nuevo.
        if (isCostCalculated)
            return;

        gCostDistanceFromStart = Mathf.Abs(gridPosition.x - aiPosition.x) * Mathf.Abs(gridPosition.y - aiPosition.y);

        hCostDistanceFromGoal = Mathf.Abs(gridPosition.x - aiDestination.x) + Mathf.Abs(gridPosition.y - aiDestination.y);

        fCostTotal = gCostDistanceFromStart + hCostDistanceFromGoal;

        isCostCalculated = true;
    }

    public void Reset()
    {
        isCostCalculated = false;
        pickedOrder = 0;
        gCostDistanceFromStart = 0;
        hCostDistanceFromGoal = 0;
        fCostTotal = 0;
    }
}