using System;
using System.Collections.Generic;
using UnityEngine;

public class AStarNode
{
    //La posición en la cuadrícula del nodo. Esto se usa para calcular las distancias y para saber dónde está el nodo en la cuadrícula, para poder movernos a esa posición cuando es necesario.
    public Vector2Int gridPosition;

    //Lista de los vecinos de los nodos en la cuadrícula. Esto se usa para saber a qué nodos podemos movernos desde el nodo actual, para poder explorar esos nodos cuando sea necesario.
    public List<AStarNode> neighbours = new List<AStarNode>();

    //El nodo es un obstáculo y no se puede mover a esa posición. Esto se usa para saber que no podemos movernos a esa posición, para evitar explorar ese nodo cuando sea necesario.
    public bool isObstacle = false;

    //Distancia del punto de partida al nodo actual. Esto se usa para calcular el costo total del movimiento a la posición de la cuadrícula, para poder comparar ese costo con el costo de otros nodos y decidir a qué nodo movernos cuando sea necesario.
    public int gCostDistanceFromStart = 0;

    //Distancia del nodo al objetivo final. Esto se usa para calcular el costo total del movimiento a la posición de la cuadrícula, para poder comparar ese costo con el costo de otros nodos y decidir a qué nodo movernos cuando sea necesario.
    public int hCostDistanceFromGoal = 0;

    //El costo total del movimiento a la posición de la cuadrícula desde el punto de partida, pasando por el nodo actual, y luego al objetivo final. Esto se usa para comparar ese costo con el costo de otros nodos y decidir a qué nodo movernos cuando sea necesario.
    public int fCostTotal = 0;

    //El orden en el que fue escogido el nodo para ser explorado. Esto se usa para saber en qué orden fueron explorados los nodos, para poder mostrar ese orden en la interfaz del juego o para depurar el algoritmo de A* si es necesario.
    public int pickedOrder = 0;

    //Indica que se debe comprobar si el coste ya ha sido calculado para este nodo, para evitar calcularlo varias veces y mejorar el rendimiento del algoritmo de A*.
    bool isCostCalculated = false;

    public AStarNode(Vector2Int gridPosition_)
    {
        gridPosition = gridPosition_;
    }

    public void CalculateCostsForNode(Vector2Int aiPosition, Vector2Int aiDestination)
    {
        //Si ya hemos calculado el nodo entonces no necesitamos hacerlo de nuevo y podemos simplemente retornar para evitar cálculos innecesarios y mejorar el rendimiento del algoritmo de A*.
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