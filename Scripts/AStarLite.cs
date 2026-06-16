using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

//Permite recalcular rutas en tiempo real cuando el mapa cambia, los caminos se bloquean, o los oponentes persiguen al jugador.

public class AStarLite : MonoBehaviour
{
    [Header("Grid")]
    int gridSizeX = 50;
    int gridSizeY = 30;

    float cellSize = 2;

    [Header("Detección de Obstáculos")]
    public LayerMask obstacleLayer;

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
    }

    void CreateGrid()
    {
        //Asigna espacio en el array para los nodos.
        aStarNodes = new AStarNode[gridSizeX, gridSizeY];

        //Crea la cuadrícula de nodos.
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                aStarNodes[x, y] = new AStarNode(new Vector2Int(x, y));

                Vector3 worldPosition = ConvertGridPositionToWorldPosition(aStarNodes[x, y]);

                //Revisa si el nodo es un obstáculo.
                Collider2D hitCollider2D = Physics2D.OverlapCircle(worldPosition, 0.3f, obstacleLayer);

                if (hitCollider2D != null)
                {
                    //Ignorar autos IA.
                    if (hitCollider2D.transform.root.CompareTag("AI"))
                        continue;

                    //Ignorar autos del jugador.
                    if (hitCollider2D.transform.root.CompareTag("Player"))
                        continue;

                    //Ignorar waypoints.
                    if (hitCollider2D.transform.root.CompareTag("Waypoint"))
                        continue;

                    //Marcar obstáculo.
                    aStarNodes[x, y].isObstacle = true;
                }
            }
        }

        //Recorre la cuadrícula y asigna vecinos.
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                //No agregar vecinos si el nodo es obstáculo.
                if (aStarNodes[x, y].isObstacle)
                    continue;

                //Vecino norte.
                if (y - 1 >= 0)
                {
                    if (!aStarNodes[x, y - 1].isObstacle)
                    {
                        aStarNodes[x, y]
                            .neighbours
                            .Add(aStarNodes[x, y - 1]);
                    }
                }

                //Vecino sur.
                if (y + 1 < gridSizeY)
                {
                    if (!aStarNodes[x, y + 1].isObstacle)
                    {
                        aStarNodes[x, y]
                            .neighbours
                            .Add(aStarNodes[x, y + 1]);
                    }
                }

                //Vecino oeste.
                if (x - 1 >= 0)
                {
                    if (!aStarNodes[x - 1, y].isObstacle)
                    {
                        aStarNodes[x, y]
                            .neighbours
                            .Add(aStarNodes[x - 1, y]);
                    }
                }

                //Vecino este.
                if (x + 1 < gridSizeX)
                {
                    if (!aStarNodes[x + 1, y].isObstacle)
                    {
                        aStarNodes[x, y].neighbours.Add(aStarNodes[x + 1, y]);
                    }
                }

                //Diagonal superior izquierda.
                if (x - 1 >= 0 && y - 1 >= 0)
                {
                    if (!aStarNodes[x - 1, y - 1].isObstacle)
                    {
                        aStarNodes[x, y].neighbours.Add(aStarNodes[x - 1, y - 1]);
                    }
                }

                //Diagonal superior derecha.
                if (x + 1 < gridSizeX && y - 1 >= 0)
                {
                    if (!aStarNodes[x + 1, y - 1].isObstacle)
                    {
                        aStarNodes[x, y].neighbours.Add(aStarNodes[x + 1, y - 1]);
                    }
                }

                //Diagonal inferior izquierda.
                if (x - 1 >= 0 && y + 1 < gridSizeY)
                {
                    if (!aStarNodes[x - 1, y + 1].isObstacle)
                    {
                        aStarNodes[x, y].neighbours.Add(aStarNodes[x - 1, y + 1]);
                    }
                }

                //Diagonal inferior derecha.
                if (x + 1 < gridSizeX && y + 1 < gridSizeY)
                {
                    if (!aStarNodes[x + 1, y + 1].isObstacle)
                    {
                        aStarNodes[x, y].neighbours.Add(aStarNodes[x + 1, y + 1]);
                    }
                }
            }
        }
    }

    private void Reset()
    {
        nodesToCheck.Clear();
        nodesChecked.Clear();
        aiPath.Clear();

        if (aStarNodes == null)
            return;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                aStarNodes[x, y].Reset();
            }
        }
    }

    public List<Vector2> FindPath(Vector2 destination)
    {
        if (aStarNodes == null)
            return null;

        //Reiniciar datos.
        Reset();

        //Convertir posiciones del mundo a cuadrícula.
        Vector2Int destinationGridPoint = ConvertWorldToGridPoint(destination);

        Vector2Int currentPositionGridPoint = ConvertWorldToGridPoint(transform.position);

        //Guardar posición debug.
        destinationPositionDebug = destination;

        //Nodo inicial.
        startNode = GetNodeFromPoint(currentPositionGridPoint);

        //Nodo destino.
        AStarNode destinationNode = GetNodeFromPoint(destinationGridPoint);

        //Verificar nodos inválidos.
        if (startNode == null || destinationNode == null)
        {
            Debug.LogWarning("El nodo inicial o destino está fuera de la cuadrícula.");

            return null;
        }

        //Si el destino es obstáculo,
        //buscar el nodo libre más cercano.
        if (destinationNode.isObstacle)
        {
            destinationNode = FindClosestNonObstacleNode(destinationGridPoint);

            //Si no encontramos un nodo válido.
            if (destinationNode == null)
            {
                Debug.LogWarning("El destino está bloqueado y no existe nodo alternativo.");

                return null;
            }

            //Actualizar destino.
            destinationGridPoint = destinationNode.gridPosition;
        }

        //Guardar posición inicial debug.
        startPositionDebug = ConvertGridPositionToWorldPosition(startNode);

        //Nodo actual.
        AStarNode currentNode = startNode;

        bool isDoneFindingPath = false;

        int pickedOrder = 1;

        //Agregar nodo inicial.
        nodesToCheck.Add(currentNode);

        //Ejecutar algoritmo.
        while (!isDoneFindingPath)
        {
            //No quedan nodos.
            if (nodesToCheck.Count == 0)
            {
                Debug.LogWarning("No quedan nodos para comprobar, no existe solución.");

                return null;
            }

            //Ordenar lista.
            nodesToCheck.Sort((a, b) =>
            {
                int compare = a.fCostTotal.CompareTo(b.fCostTotal);

                if (compare == 0)
                {
                    compare = a.hCostDistanceFromGoal.CompareTo(b.hCostDistanceFromGoal);
                }

                return compare;
            });

            //Escoger nodo más barato.
            currentNode = nodesToCheck[0];

            //Quitar nodo actual.
            nodesToCheck.Remove(currentNode);

            //Guardar orden.
            currentNode.pickedOrder = pickedOrder;

            pickedOrder++;

            //Agregar revisado.
            nodesChecked.Add(currentNode);

            //Encontramos el destino.
            if (currentNode.gridPosition == destinationGridPoint)
            {
                isDoneFindingPath = true;

                break;
            }

            //Calcular costos.
            CalculateCostsForNodeAndNeighbours(currentNode, currentPositionGridPoint, destinationGridPoint);

            //Revisar vecinos.
            foreach (AStarNode neighbourNode
                in currentNode.neighbours)
            {
                if (nodesChecked.Contains(neighbourNode))
                    continue;

                if (nodesToCheck.Contains(neighbourNode))
                    continue;

                nodesToCheck.Add(neighbourNode);
            }
        }

        //Crear camino.
        aiPath = CreatePathForAI(currentPositionGridPoint);

        return aiPath;
    }

    AStarNode FindClosestNonObstacleNode(Vector2Int centerPoint)
    {
        int maxSearchDistance = 10;

        for (int distance = 1;
            distance <= maxSearchDistance;
            distance++)
        {
            for (int x = -distance;
                x <= distance;
                x++)
            {
                for (int y = -distance;
                    y <= distance;
                    y++)
                {
                    Vector2Int checkPoint = new Vector2Int(centerPoint.x + x, centerPoint.y + y);

                    AStarNode node =
                        GetNodeFromPoint(checkPoint);

                    if (node == null)
                        continue;

                    if (!node.isObstacle)
                        return node;
                }
            }
        }

        return null;
    }

    List<Vector2> CreatePathForAI(Vector2Int currentPositionGridPoint)
    {
        List<Vector2> resultAIPath = new List<Vector2>();

        List<AStarNode> finalPath = new List<AStarNode>();

        //No hay nodos revisados.
        if (nodesChecked.Count == 0)
            return resultAIPath;

        //Invertir nodos revisados.
        nodesChecked.Reverse();

        bool isPathCreated = false;

        AStarNode currentNode = nodesChecked[0];

        finalPath.Add(currentNode);

        int attempts = 0;

        while (!isPathCreated)
        {
            //Ordenar vecinos.
            currentNode.neighbours.Sort((a, b) => { return a.pickedOrder.CompareTo(b.pickedOrder); });

            bool foundNextNode = false;

            //Escoger vecino válido.
            foreach (AStarNode aStarNode
                in currentNode.neighbours)
            {
                if (!finalPath.Contains(aStarNode) &&
                    nodesChecked.Contains(aStarNode))
                {
                    finalPath.Add(aStarNode);

                    currentNode = aStarNode;

                    foundNextNode = true;

                    break;
                }
            }

            //No encontramos siguiente nodo.
            if (!foundNextNode)
            {
                Debug.LogWarning("No se pudo continuar creando el camino.");

                break;
            }

            //Llegamos al inicio.
            if (currentNode == startNode)
                isPathCreated = true;

            //Evitar loops infinitos.
            if (attempts > 1000)
            {
                Debug.LogWarning("CreatePathForAI falló después de demasiados intentos.");

                break;
            }

            attempts++;
        }

        //Convertir a posiciones del mundo.
        foreach (AStarNode aStarNode in finalPath)
        {
            resultAIPath.Add(ConvertGridPositionToWorldPosition(aStarNode));
        }

        //Invertir resultado.
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

        if (gridPoint.x >= gridSizeX)
            return null;

        if (gridPoint.y < 0)
            return null;

        if (gridPoint.y >= gridSizeY)
            return null;

        return aStarNodes[
            gridPoint.x,
            gridPoint.y
        ];
    }

    Vector2Int ConvertWorldToGridPoint(Vector2 position)
    {
        //Calcular punto de cuadrícula.
        Vector2Int gridPoint = new Vector2Int(Mathf.RoundToInt(position.x / cellSize + gridSizeX / 2.0f), Mathf.RoundToInt(position.y / cellSize + gridSizeY / 2.0f));

        return gridPoint;
    }

    Vector3 ConvertGridPositionToWorldPosition(AStarNode aStarNode)
    {
        return new Vector3(aStarNode.gridPosition.x * cellSize - (gridSizeX * cellSize) / 2.0f, aStarNode.gridPosition.y * cellSize - (gridSizeY * cellSize) / 2.0f, 0);
    }

    void OnDrawGizmos()
    {
        if (!isDebugActiveForCar)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(startPositionDebug, 1);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(destinationPositionDebug, 1);

        Gizmos.color = Color.yellow;

        foreach (Vector2 point in aiPath)
        {
            Gizmos.DrawSphere(point, 0.5f);
        }
    }
}