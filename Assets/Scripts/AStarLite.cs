using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AStarLite : MonoBehaviour
{
    int gridSizeX = 50;
    int gridSizeY = 30;

    float cellSize = 2;

    AStarNode[,] aStarNodes;

    AStarNode startNode;

    List<AStarNode> nodesToCheck = new List<AStarNode>();
    List<AStarNode> nodesChecked = new List<AStarNode>();

    List<Vector2> aiPath = new List<Vector2>();

    //Debug
    Vector3 startPositionDebug = new Vector3(1000, 0, 0);
    Vector3 destinationPositionDebug = new Vector3(1000, 0, 0);

    public bool isDebugActiveForCar = false;

    void Start()
    {
        CreateGrid();

        FindPath(new Vector2(32, 17));
    }

    void CreateGrid()
    {
        //Asigna espacio en el array para los nodos.
        aStarNodes = new AStarNode[gridSizeX, gridSizeY];

        //Crea la cuadrícula de nodos.
        for (int x = 0; x < gridSizeX; x++)
            for (int y = 0; y < gridSizeY; y++)
            {
                aStarNodes[x, y] = new AStarNode(new Vector2Int(x, y));

                Vector3 worldPosition = ConvertGridPositionToWorldPosition(aStarNodes[x, y]);

                //Revisa si el nodo es un obstáculo
                Collider2D hitCollider2D = Physics2D.OverlapCircle(worldPosition, cellSize / 2.0f);

                if (hitCollider2D != null)
                {
                    //Ignorar los autos IA, no son obstáculos
                    if (hitCollider2D.transform.root.CompareTag("AI"))
                        continue;

                    //Ignorar los autos de los jugadores, no son obstáculos.
                    if (hitCollider2D.transform.root.CompareTag("Player"))
                        continue;

                    //Marcar como obstáculo.
                    aStarNodes[x, y].isObstacle = true;
                }
            }

        //Recorre la cuadrícula de nuevo y asigna los vecinos.
        for (int x = 0; x < gridSizeX; x++)
            for (int y = 0; y < gridSizeY; y++)
            {
                //Comprueba con el vecino del norte, si estamos en el límite, no lo añadas.
                if (y - 1 >= 0)
                {
                    if (!aStarNodes[x, y - 1].isObstacle)
                        aStarNodes[x, y].neighbours.Add(aStarNodes[x, y - 1]);
                }

                //Comprueba con el vecino del sur, si estamos en el límite, no lo añadas.
                if (y + 1 <= gridSizeY - 1)
                {
                    if (!aStarNodes[x, y + 1].isObstacle)
                        aStarNodes[x, y].neighbours.Add(aStarNodes[x, y + 1]);
                }

                //Comprueba con el vecino del este, si estamos en el límite, no lo añadas.
                if (x - 1 >= 0)
                {
                    if (!aStarNodes[x - 1, y].isObstacle)
                        aStarNodes[x, y].neighbours.Add(aStarNodes[x - 1, y]);
                }

                //Comprueba con el vecino del oeste, si estamos en el límite, no lo añadas.
                if (x + 1 <= gridSizeX - 1)
                {
                    if (!aStarNodes[x + 1, y].isObstacle)
                        aStarNodes[x, y].neighbours.Add(aStarNodes[x + 1, y]);
                }
            }
    }

    private void Reset()
    {
        nodesToCheck.Clear();
        nodesChecked.Clear();
        aiPath.Clear();

        for (int x = 0; x < gridSizeX; x++)
            for (int y = 0; y < gridSizeY; y++)
                aStarNodes[x, y].Reset();
    }
     
    public List<Vector2> FindPath(Vector2 destination)
    {
        if (aStarNodes == null)
            return null;

        //Convierta el destino de posición mundial a posición de cuadrícula.
        Vector2Int destinationGridPoint = ConvertWorldToGridPoint(destination);
        Vector2Int currentPositionGridPoint = ConvertWorldToGridPoint(transform.position);

        //Establece una posición de depuración para que podamos mostrarlo durante el desarrollo.
        destinationPositionDebug = destination;

        //Comienza el algoritmo calculando los costos para el primer nodo.
        startNode = GetNodeFromPoint(currentPositionGridPoint);

        //Guarda la posición inicial de la cuadrícula para poder usarla durante el desarrollo.
        startPositionDebug = ConvertGridPositionToWorldPosition(startNode);

        //Establece el nodo actual en el nodo de inicio.
        AStarNode currentNode = startNode;

        bool isDoneFindingPath = false;
        int pickedOrder = 1;

        //Repetir el bucle mientras no hayamos terminado con el recorrido.
        while (!isDoneFindingPath)
        {
            //Borra el nodo reciente de la lista de nodos que deben ser revisados.
            nodesToCheck.Remove(currentNode);

            //Establece el orden de recogida.
            currentNode.pickedOrder = pickedOrder;

            pickedOrder++;

            //Agregar el nodo recurrente a la lista revisada.
            nodesChecked.Add(currentNode);

            //Si! Encontramos el destino!
            if (currentNode.gridPosition == destinationGridPoint)
            {
                isDoneFindingPath = true;
                break;
            }

            //Calcular el costo de todos los nodos
            CalculateCostsForNodeAndNeighbours(currentNode, currentPositionGridPoint, destinationGridPoint);

            //Revisar si los nodos vecinos deben ser considerados.
            foreach (AStarNode neighbourNode in currentNode.neighbours)
            {
                //Saltarse algún nodo que ya hemos revisado.
                if (nodesChecked.Contains(neighbourNode))
                    continue;

                //Saltarse algún nodo que ya está en la lista.
                if (nodesToCheck.Contains(neighbourNode))
                    continue;

                //Agrega el nodo a la lista que debemos revisar.
                nodesToCheck.Add(neighbourNode);
            }

            //Ordena la lista de manera que los elementos con el costo total más bajo (costo f) y si tienen el mismo valor, entonces elijamos el que tenga el costo más bajo para alcanzar el objetivo.
            nodesToCheck = nodesToCheck.OrderBy(x => x.fCostTotal).ThenBy(x => x.hCostDistanceFromGoal).ToList();

            //Escoger el nodo con el costo más bajo como el siguiente nodo.
            if (nodesToCheck.Count == 0)
            {
                Debug.LogWarning($"No quedan nodos en los siguientes nodos para comprobar, no tenemos solución");
                return null;
            }
            else
            {
                currentNode = nodesToCheck[0];
            }
        }

        aiPath = CreatePathForAI(currentPositionGridPoint);

        return null;
    }

    List<Vector2> CreatePathForAI(Vector2Int currentPositionGridPoint)
    {
        List<Vector2> resultAIPath = new List<Vector2>();
        List<AStarNode> aiPath = new List<AStarNode>();

        //Invierte los nodos para comprobarlo, ya que el último nodo añadido será el destino de la IA.
        nodesChecked.Reverse();

        bool isPathCreated = false;

        AStarNode currentNode = nodesChecked[0];

        int attempts = 0;

        while (!isPathCreated)
        {
            //Retrocede con el orden de creación más bajo.
            currentNode.neighbours = currentNode.neighbours.OrderBy(x => x.pickedOrder).ToList();

            //Recoge el vecino con el costo más bajo si no está en la lista.
            foreach (AStarNode aStarNode in currentNode.neighbours)
            {
                if (!aiPath.Contains(aStarNode) && nodesChecked.Contains(aStarNode))
                {
                    aiPath.Add(aStarNode);
                    currentNode = aStarNode;

                    break;
                }
            }

            if (currentNode == startNode)
                isPathCreated = true;

            if (attempts > 1000)
            {
                Debug.LogWarning("CreatePathForAI falló después de tantos intentos");
                break;
            }

            attempts++;
        }

        foreach (AStarNode aStarNode in aiPath)
        {
            resultAIPath.Add(ConvertGridPositionToWorldPosition(aStarNode));
        }

        //Voltea el resultado
        resultAIPath.Reverse();

        return resultAIPath;
    }

    void CalculateCostsForNodeAndNeighbours(AStarNode aStarNode, Vector2Int aiPosition, Vector2Int aiDestination)
        {
            aStarNode.CalculateCostsForNode(aiPosition, aiDestination);

            foreach (AStarNode neighbourNode in aStarNode.neighbours)
            {
                neighbourNode.CalculateCostsForNode(aiPosition, aiDestination);
            }
        }

    AStarNode GetNodeFromPoint(Vector2Int gridPoint)
        {
            if (gridPoint.x < 0)
                return null;

            if (gridPoint.x > gridSizeX - 1)
                return null;

            if (gridPoint.y < 0)
                return null;

            if (gridPoint.y > gridSizeY - 1)
                return null;

            return aStarNodes[gridPoint.x, gridPoint.y];
        }

    Vector2Int ConvertWorldToGridPoint(Vector2 position)
    {
        //Calcular el punto de la cuadrícula.
        Vector2Int gridPoint = new Vector2Int(Mathf.RoundToInt(position.x / cellSize + gridSizeX / 2.0f), Mathf.RoundToInt(position.y / cellSize + gridSizeY / 2.0f));

        return gridPoint;
    }

    Vector3 ConvertGridPositionToWorldPosition(AStarNode aStarNode)
    {
        return new Vector3(aStarNode.gridPosition.x * cellSize - (gridSizeX * cellSize) / 2.0f, aStarNode.gridPosition.y * cellSize - (gridSizeY * cellSize) / 2.0f, 0);
    }

    void OnDrawGizmos()
    {
        if (aStarNodes == null)
            return;

        if (!isDebugActiveForCar)
            return;

        //Dibujar una grilla.
        for (int x = 0; x < gridSizeX; x++)
            for (int y = 0; y < gridSizeY; y++)
            {
                if (aStarNodes[x, y].isObstacle)
                    Gizmos.color = Color.red;
                else Gizmos.color = Color.green;

                Gizmos.DrawWireCube(ConvertGridPositionToWorldPosition(aStarNodes[x, y]), new Vector3(cellSize, cellSize, cellSize));
            }

        //Dibujar los nodos que revisamos
        foreach (AStarNode checkedNode in nodesChecked)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(ConvertGridPositionToWorldPosition(checkedNode), 1.0f);
        }

        //Dibujar los nodos que debemos revisar
        foreach (AStarNode toCheckNode in nodesToCheck)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(ConvertGridPositionToWorldPosition(toCheckNode), 1.0f);
            }

        Vector3 lastAIPoint = Vector3.zero;
        bool isFirstStep = true;

        Gizmos.color = Color.black;

        foreach (Vector2 point in aiPath)
        {
            if (!isFirstStep)
                Gizmos.DrawLine(lastAIPoint, point);

            lastAIPoint = point;
        }

            //Dibujar la posición inicial
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(destinationPositionDebug, 1f);

            //Dibujar la posición final
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(destinationPositionDebug, 1f);
    }
}