using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

public class AStarLite : MonoBehaviour
{
    int gridSizeX = 50;
    int gridSizeY = 30;

    float cellSize = 2;

    [SerializeField] LayerMask obstacleLayer;

    AStarNode[,] aStarNodes;

    AStarNode startNode;

    List<AStarNode> nodesToCheck = new List<AStarNode>();
    List<AStarNode> nodesChecked = new List<AStarNode>();

    List<Vector2> aiPath = new List<Vector2>();

    public bool isDebugActiveForCar = false;

    void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        //Asigna espacio en el array para los nodos.
        aStarNodes = new AStarNode[gridSizeX, gridSizeY];

        //Crear la cuadrícula de nodos
        for (int x = 0; x < gridSizeX; x++)
            for (int y = 0; y < gridSizeY; y++)
            {
                aStarNodes[x, y] = new AStarNode(new Vector2Int(x, y));

                Vector3 worldPosition = ConvertGridPositionToWorldPosition(aStarNodes[x, y]);

                //Revisar si el nodo es un obstáculo
                Collider2D hitCollider2D = Physics2D.OverlapCircle(worldPosition, 0.3f, obstacleLayer);

                if (hitCollider2D != null)
                {
                    //Ignorar los autos IA, no son obstáculos.
                    if (hitCollider2D.transform.root.CompareTag("AI"))
                        continue;

                    //Ignorar los autos del jugador, no son obstáculos.
                    if (hitCollider2D.transform.root.CompareTag("Player"))
                        continue;

                    //Marcar como obstáculo
                    aStarNodes[x, y].isObstacle = true;
                }
            }

        //Recorre la cuadrícula de nuevo y pobla a los vecinos.
        for (int x = 0; x < gridSizeX; x++)
            for (int y = 0; y < gridSizeY; y++)
            {
                if (y - 1 >= 0)
                    AddNeighbour(x, y, x, y - 1);

                if (y + 1 < gridSizeY)
                    AddNeighbour(x, y, x, y + 1);

                if (x - 1 >= 0)
                    AddNeighbour(x, y, x - 1, y);

                if (x + 1 < gridSizeX)
                    AddNeighbour(x, y, x + 1, y);
            }
    }

    void AddNeighbour(int x, int y, int nx, int ny)
    {
        if (!aStarNodes[nx, ny].isObstacle)
            aStarNodes[x, y].neighbours.Add(aStarNodes[nx, ny]);
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

        //Reiniciar sistema para que podamos empezar a una posición fresca.
        Reset();

        //Convertir el destino de posición mundial a posición de la cuadrícula.
        Vector2Int destinationGridPoint = ConvertWorldToGridPoint(destination);
        Vector2Int currentPositionGridPoint = ConvertWorldToGridPoint(transform.position);

        //Clamp al grid para evitar out-of-range
        destinationGridPoint.x = Mathf.Clamp(destinationGridPoint.x, 0, gridSizeX - 1);
        destinationGridPoint.y = Mathf.Clamp(destinationGridPoint.y, 0, gridSizeY - 1);

        startNode = GetNodeFromPoint(currentPositionGridPoint);

        if (startNode == null)
        {
            Debug.LogWarning("Start node inválido");
            return null;
        }

        AStarNode destinationNode = GetNodeFromPoint(destinationGridPoint);

        if (destinationNode == null || destinationNode.isObstacle)
        {

            destinationNode = FindClosestWalkableNode(destinationGridPoint);

            if (destinationNode == null)
            {
                Debug.LogWarning("No se encontró nodo caminable cercano");
                return null;
            }
        }

        AStarNode currentNode = startNode;

        bool isDoneFindingPath = false;
        int pickedOrder = 1;
        int safety = 0;

        //Repetir mientras no hayamos terminado el camino.
        while (!isDoneFindingPath)
        {
            safety++;
            if (safety > 5000)
            {
                Debug.LogWarning("A* Safety break");
                return null;
            }

            nodesToCheck.Remove(currentNode);

            currentNode.pickedOrder = pickedOrder;
            pickedOrder++;

            nodesChecked.Add(currentNode);

            //Si! Encontramos el destino.
            if (currentNode == destinationNode)
            {
                isDoneFindingPath = true;
                break;
            }

            //Calcular costo para todos los nodos.
            CalculateCostsForNodeAndNeighbours(currentNode, currentPositionGridPoint, destinationGridPoint);

            //Revisar vecinos
            foreach (AStarNode neighbourNode in currentNode.neighbours)
            {
                if (nodesChecked.Contains(neighbourNode))
                    continue;

                if (nodesToCheck.Contains(neighbourNode))
                    continue;

                nodesToCheck.Add(neighbourNode);
            }

            nodesToCheck = nodesToCheck.OrderBy(x => x.fCostTotal).ThenBy(x => x.hCostDistanceFromGoal).ToList();

            if (nodesToCheck.Count == 0)
            {
                Debug.LogWarning("No quedan nodos en los siguientes nodos para comprobar, no tenemos solución.");
                return null;
            }

            currentNode = nodesToCheck[0];
        }

        return CreatePathForAI();
    }

    List<Vector2> CreatePathForAI()
    {
        List<Vector2> resultAIPath = new List<Vector2>();

        nodesChecked.Reverse();

        AStarNode currentNode = nodesChecked[0];

        List<AStarNode> pathNodes = new List<AStarNode>();
        pathNodes.Add(currentNode);

        int attempts = 0;

        while (currentNode != startNode)
        {
            attempts++;
            if (attempts > 1000)
            {
                Debug.LogWarning("CreatePathForAI falló");
                break;
            }

            currentNode.neighbours = currentNode.neighbours.OrderBy(x => x.pickedOrder).ToList();

            foreach (AStarNode aStarNode in currentNode.neighbours)
            {
                if (!pathNodes.Contains(aStarNode) && nodesChecked.Contains(aStarNode))
                {
                    pathNodes.Add(aStarNode);
                    currentNode = aStarNode;
                    break;
                }
            }
        }

        foreach (AStarNode node in pathNodes)
        resultAIPath.Add(ConvertGridPositionToWorldPosition(node));

        resultAIPath.Reverse();

        return resultAIPath;
    }

    void CalculateCostsForNodeAndNeighbours(AStarNode aStarNode, Vector2Int aiPosition, Vector2Int aiDestination)
    {
        aStarNode.CalculateCostsForNode(aiPosition, aiDestination);

        foreach (AStarNode neighbourNode in aStarNode.neighbours)
        neighbourNode.CalculateCostsForNode(aiPosition, aiDestination);
    }

    AStarNode GetNodeFromPoint(Vector2Int gridPoint)
    {
        if (gridPoint.x < 0 || gridPoint.x >= gridSizeX)
            return null;

        if (gridPoint.y < 0 || gridPoint.y >= gridSizeY)
            return null;

        return aStarNodes[gridPoint.x, gridPoint.y];
    }

    AStarNode FindClosestWalkableNode(Vector2Int point)
    {
        for (int radius = 1; radius < 5; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    Vector2Int check = new Vector2Int(point.x + x, point.y + y);

                    AStarNode node = GetNodeFromPoint(check);

                    if (node != null && !node.isObstacle)
                        return node;
                }
            }
        }

        return null;
    }

    Vector2Int ConvertWorldToGridPoint(Vector2 position)
    {
        float halfX = gridSizeX * cellSize * 0.5f;
        float halfY = gridSizeY * cellSize * 0.5f;

        int x = Mathf.FloorToInt((position.x + halfX) / cellSize);
        int y = Mathf.FloorToInt((position.y + halfY) / cellSize);

        return new Vector2Int(x, y);
    }

    Vector3 ConvertGridPositionToWorldPosition(AStarNode node)
    {
        return new Vector3(node.gridPosition.x * cellSize - (gridSizeX * cellSize) / 2.0f,node.gridPosition.y * cellSize - (gridSizeY * cellSize) / 2.0f, 0);
    }

    void OnDrawGizmos()
    {
        if (aStarNodes == null) return;

        if (isDebugActiveForCar) return;

        for (int x = 0; x < gridSizeX; x++)
            for (int y = 0; y < gridSizeY; y++)
            {
                Gizmos.color = aStarNodes[x, y].isObstacle ? Color.red : Color.green;
                Gizmos.DrawWireCube(ConvertGridPositionToWorldPosition(aStarNodes[x, y]), new Vector3(cellSize, cellSize, cellSize));
            }

        Gizmos.color = Color.black;

        Vector3 last = Vector3.zero;
        bool first = true;

        foreach (Vector2 p in aiPath)
        {
            if (!first)
                Gizmos.DrawLine(last, p);

            last = p;
            first = false;
        }
    }
}