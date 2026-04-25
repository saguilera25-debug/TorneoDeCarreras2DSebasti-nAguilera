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

    // Debug
    Vector3 startPositionDebug = new Vector3(1000, 0, 0);
    Vector3 destinationPositionDebug = new Vector3(1000, 0, 0);

    void Start()
    {
        CreateGrid();
        aiPath = FindPath(new Vector2(32, 17));
    }

    void CreateGrid()
    {
        aStarNodes = new AStarNode[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
            for (int y = 0; y < gridSizeY; y++)
            {
                aStarNodes[x, y] = new AStarNode(new Vector2Int(x, y));

                Vector3 worldPosition = ConvertGridPositionToWorldPosition(aStarNodes[x, y]);

                Collider2D hit = Physics2D.OverlapCircle(worldPosition, cellSize / 2f);

                if (hit != null)
                {
                    if (hit.transform.root.CompareTag("AI")) continue;
                    if (hit.transform.root.CompareTag("Player")) continue;

                    aStarNodes[x, y].isObstacle = true;
                }
            }

        // Vecinos (4 direcciones)
        for (int x = 0; x < gridSizeX; x++)
            for (int y = 0; y < gridSizeY; y++)
            {
                if (aStarNodes[x, y].isObstacle) continue;

                if (y - 1 >= 0 && !aStarNodes[x, y - 1].isObstacle)
                    aStarNodes[x, y].neighbours.Add(aStarNodes[x, y - 1]);

                if (y + 1 < gridSizeY && !aStarNodes[x, y + 1].isObstacle)
                    aStarNodes[x, y].neighbours.Add(aStarNodes[x, y + 1]);

                if (x - 1 >= 0 && !aStarNodes[x - 1, y].isObstacle)
                    aStarNodes[x, y].neighbours.Add(aStarNodes[x - 1, y]);

                if (x + 1 < gridSizeX && !aStarNodes[x + 1, y].isObstacle)
                    aStarNodes[x, y].neighbours.Add(aStarNodes[x + 1, y]);
            }
    }

    public List<Vector2> FindPath(Vector2 destination)
    {
        if (aStarNodes == null) return null;

        nodesToCheck.Clear();
        nodesChecked.Clear();

        // 🔥 Resetear nodos
        foreach (var node in aStarNodes)
        {
            node.Reset();
        }

        Vector2Int startGrid = ConvertWorldToGridPoint(transform.position);
        Vector2Int endGrid = ConvertWorldToGridPoint(destination);

        destinationPositionDebug = destination;

        startNode = GetNodeFromPoint(startGrid);
        if (startNode == null) return null;

        startPositionDebug = ConvertGridPositionToWorldPosition(startNode);

        // Calcular costos iniciales
        startNode.CalculateCostsForNode(startGrid, endGrid);

        nodesToCheck.Add(startNode);

        while (nodesToCheck.Count > 0)
        {
            nodesToCheck = nodesToCheck
                .OrderBy(n => n.fCostTotal)
                .ThenBy(n => n.hCostDistanceFromGoal)
                .ToList();

            AStarNode currentNode = nodesToCheck[0];
            nodesToCheck.Remove(currentNode);
            nodesChecked.Add(currentNode);

            if (currentNode.gridPosition == endGrid)
            {
                return RetracePath(startNode, currentNode);
            }

            foreach (AStarNode neighbour in currentNode.neighbours)
            {
                if (nodesChecked.Contains(neighbour)) continue;

                neighbour.CalculateCostsForNode(startGrid, endGrid);

                if (!nodesToCheck.Contains(neighbour))
                {
                    neighbour.parent = currentNode;
                    nodesToCheck.Add(neighbour);
                }
            }
        }

        Debug.LogWarning("No se encontró camino");
        return null;
    }

    List<Vector2> RetracePath(AStarNode startNode, AStarNode endNode)
    {
        List<AStarNode> path = new List<AStarNode>();
        AStarNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;

            if (currentNode == null) break;
        }

        path.Reverse();

        List<Vector2> result = new List<Vector2>();

        foreach (var node in path)
        {
            result.Add(ConvertGridPositionToWorldPosition(node));
        }

        return result;
    }

    AStarNode GetNodeFromPoint(Vector2Int gridPoint)
    {
        if (gridPoint.x < 0 || gridPoint.x >= gridSizeX ||
            gridPoint.y < 0 || gridPoint.y >= gridSizeY)
            return null;

        return aStarNodes[gridPoint.x, gridPoint.y];
    }

    Vector2Int ConvertWorldToGridPoint(Vector2 position)
    {
        return new Vector2Int(
            Mathf.RoundToInt(position.x / cellSize + gridSizeX / 2f),
            Mathf.RoundToInt(position.y / cellSize + gridSizeY / 2f)
        );
    }

    Vector3 ConvertGridPositionToWorldPosition(AStarNode node)
    {
        return new Vector3(
            (node.gridPosition.x * cellSize) - (gridSizeX * cellSize) / 2f,
            (node.gridPosition.y * cellSize) - (gridSizeY * cellSize) / 2f,
            0f
        );
    }

    void OnDrawGizmos()
    {
        if (aStarNodes == null) return;

        for (int x = 0; x < gridSizeX; x++)
            for (int y = 0; y < gridSizeY; y++)
            {
                Gizmos.color = aStarNodes[x, y].isObstacle ? Color.red : Color.green;
                Gizmos.DrawWireCube(
                    ConvertGridPositionToWorldPosition(aStarNodes[x, y]),
                    Vector3.one * cellSize
                );
            }

        Gizmos.color = Color.blue;

        foreach (var point in aiPath)
        {
            Gizmos.DrawSphere(point, 0.5f);
        }

        for (int i = 0; i < aiPath.Count - 1; i++)
        {
            Gizmos.DrawLine(aiPath[i], aiPath[i + 1]);
        }

        Gizmos.color = Color.black;
        Gizmos.DrawSphere(startPositionDebug, 1f);
        Gizmos.DrawSphere(destinationPositionDebug, 1f);
    }
}