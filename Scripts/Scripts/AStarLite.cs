using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Este script se encarga de manejar el algoritmo de pathfinding A* para los autos controlados por IA en la carrera. Crea una cuadrícula de nodos en la escena, donde cada nodo representa un punto en la escena que el auto controlado por IA puede seguir para llegar a su destino. El algoritmo A* se utiliza para encontrar el camino más corto desde la posición actual del auto controlado por IA hasta su destino, evitando obstáculos en el camino. El resultado del algoritmo es una lista de puntos que el auto controlado por IA puede seguir para llegar a su destino de manera eficiente.
public class AStarLite : MonoBehaviour
{
    int gridSizeX = 50;
    int gridSizeY = 30;

    float cellSize = 2;

    AStarNode[,] aStarNodes;

    AStarNode startNode;

    List<AStarNode> nodesToCheck = new List<AStarNode>();
    List<AStarNode> nodesChecked = new List<AStarNode>();

    //Debug 
    Vector3 startPositionDebug = new Vector3(1000, 0, 0);
    Vector3 destinationPositionDebug = new Vector3(1000, 0, 0);

    void Start()
    {
        CreateGrid();

        FindPath(new Vector2(32, 17));
    }

    void CreateGrid()
    {
        //Asigna espacio en el array para los nodos de A*.
        aStarNodes = new AStarNode[gridSizeX, gridSizeY];

        //Crea la cuadrícula de nodos y revisa si cada nodo es un obstáculo.
        for (int x = 0; x < gridSizeX; x++)
            for (int y = 0; y < gridSizeY; y++)
            {
                aStarNodes[x, y] = new AStarNode(new Vector2Int(x, y));

                Vector3 worldPosition = ConvertGridPositionToWorldPosition(aStarNodes[x, y]);

                //Revisa si el nodo es un obstáculo usando un círculo de detección.
                Collider2D hitCollider2D = Physics2D.OverlapCircle(worldPosition, cellSize / 2.0f);

                if (hitCollider2D != null)
                {
                    //Ignora autos IA, no son obstáculos para otros autos IA.
                    if (hitCollider2D.transform.root.CompareTag("AI"))
                        continue;

                    //Ignorar autos del jugador., no son obstáculos para los autos IA.
                    if (hitCollider2D.transform.root.CompareTag("Player"))
                        continue;

                    //Marcar como obstáculo si detectamos algo.
                    aStarNodes[x, y].isObstacle = true;
                }
            }

        //Recorre la cuadricula otra vez y popula la lista de vecinos para cada nodo, ignorando nodos que son obstáculos.
        for (int x = 0; x < gridSizeX; x++)
            for (int y = 0; y < gridSizeY; y++)
            {
                //Revisar vecino al norte, si estamos en el borde entonces no lo añadimos a la lista de vecinos.
                if (y - 1 >= 0)
                {
                    if (!aStarNodes[x, y - 1].isObstacle)
                        aStarNodes[x, y].neighbours.Add(aStarNodes[x, y - 1]);
                }

                //Revisar vecino al sur, si estamos en el borde entonces no lo añadimos a la lista de vecinos.
                if (y + 1 <= gridSizeY - 1)
                {
                    if (!aStarNodes[x, y + 1].isObstacle)
                        aStarNodes[x, y].neighbours.Add(aStarNodes[x, y + 1]);
                }

                //Revisar vecino al este, si estamos en el borde entonces no lo añadimos a la lista de vecinos.
                if (x - 1 >= 0)
                {
                    if (!aStarNodes[x - 1, y].isObstacle)
                        aStarNodes[x, y].neighbours.Add(aStarNodes[x - 1, y]);
                }

                //Revisar vecino al oeste, si estamos en el borde entonces no lo añadimos a la lista de vecinos.
                if (x + 1 <= gridSizeX - 1)
                {
                    if (!aStarNodes[x + 1, y].isObstacle)
                        aStarNodes[x, y].neighbours.Add(aStarNodes[x + 1, y]);
                }
            }
    }

    public List<Vector2> FindPath(Vector2 destination)
    {
        if (aStarNodes == null)
            return null;

        //Convertir posiciones del mundo a cuadrícula para trabajar con los nodos de A*.
        Vector2Int destinationGridPoint = ConvertWorldToGridPoint(destination);
        Vector2Int currentPositionGridPoint = ConvertWorldToGridPoint(transform.position);

        //Guardar posición debug para que podamos mostrarla mientras desarrollamos el juego.
        destinationPositionDebug = destination;

        //Inicia el algoritmo calculando los costos para el primer nodo, que es el nodo de inicio.
        startNode = GetNodeFromPoint(currentPositionGridPoint);

        //Guarda la posición inicial de la cuadricula para que la podamos utilizar mientras desarrollamos el juego.
        startPositionDebug = ConvertGridPositionToWorldPosition(startNode);

        //Establece el nodo actual al nodo inicial.
        AStarNode currentNode = startNode;

        bool isDoneFindingPath = false;
        int pickedOrder = 1;

        //Gira mientras no hayamos terminado con el camino o mientras no hayamos revisado demasiados nodos, para evitar que el juego se congele si algo sale mal con el algoritmo.
        while (!isDoneFindingPath)
        {
            //Borra el nodo actual de la lista de nodos que deben ser revisados.
            nodesToCheck.Remove(currentNode);

            //Establece el orden de recogida del nodo actual, para que podamos mostrar el orden en el que se revisaron los nodos mientras desarrollamos el juego.
            currentNode.pickedOrder = pickedOrder;

            pickedOrder++;

            //Añade el nodo actual a la lista revisada.
            nodesChecked.Add(currentNode);

            //Si! Encontramos el destino y ya podemos salir del ciclo.
            if (currentNode.gridPosition == destinationGridPoint)
            {
                isDoneFindingPath = true;
                break;
            }

            //Calcular costos para todos los nodos vecinos del nodo actual, y añadirlos a la lista de nodos que deben ser revisados si no están en esa lista ni en la lista de nodos ya revisados.
            CalculateCostsForNodeAndNeighbours(currentNode, currentPositionGridPoint, destinationGridPoint);

            //Revisa si los nodos vecinos deben ser considerados para ser el siguiente nodo actual, y si es así, añádelos a la lista de nodos que deben ser revisados.
            foreach (AStarNode neighbourNode in currentNode.neighbours)
            {
                //Saltarse algún nodo que ya fue revisado, para evitar ciclos infinitos.
                if (nodesChecked.Contains(neighbourNode))
                    continue;

                //Saltarse algún nodo que ya está en la lista de nodos que deben ser revisados, para evitar añadir el mismo nodo varias veces a esa lista.
                if (nodesToCheck.Contains(neighbourNode))
                    continue;

                //Añadir el nodo a la lista que debemos revisar en algún momento, para que el algoritmo pueda revisar ese nodo en algún momento y avanzar hacia el destino.
                nodesToCheck.Add(neighbourNode);

            }

            //Organiza la lista para que los items con el costo total más bajo estén al principio de la lista, para que el algoritmo revise primero los nodos que parecen más prometedores para llegar al destino.
            nodesToCheck = nodesToCheck.OrderBy(x => x.fCostTotal).ThenBy(x => x.hCostDistanceFromGoal).ToList();

            //Escoge el nodo con el costo más bajo como el siguiente nodo.
            if (nodesToCheck.Count == 0)
            {
                Debug.LogWarning("$No se pudo encontrar un camino hacia el destino. No hay más nodos para revisar.");
                return null;
            }
            else
            {
                currentNode = nodesToCheck[0];
            }
        }

        return null;
    }
    
    List<Vector2> CreatePathForAI(Vector2Int currentPositionGridPoint)
    {
        List<Vector2> resultAIPath = new List<Vector2>();
        List<AStarNode> aiPath = new List<AStarNode>();

        //Invierte los nodos que revisar ya que el último nodo será el destino IA y el primer nodo será la posición actual del auto IA, para que el auto IA pueda seguir el camino desde su posición actual hasta el destino.
        nodesChecked.Reverse();

        bool isPathCreated = false;

        AStarNode currentNode = nodesChecked[0];

        int attempts = 0;

        while (!isPathCreated)
        {
            //Ve al reversa con el orden de creación más bajo, para encontrar el camino más corto desde la posición actual del auto IA hasta el destino.
            currentNode.neighbours = currentNode.neighbours.OrderBy(x => x.pickedOrder).ToList();

            //Escoge el vecino con el costo más menor si ya no está en la lista.
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
                Debug.LogWarning("CreatePathForAI falló después de varios intentos. Algo salió mal con el algoritmo.");
                break;
            }

            attempts++;
        }

        foreach (AStarNode aStarNode in aiPath)
        {
            resultAIPath.Add(ConvertGridPositionToWorldPosition(aStarNode));
        }

        //Voltea el resultado para que el primer item de la lista sea el primer punto que el auto IA debe seguir, y el último item de la lista sea el destino, para que el auto IA pueda seguir el camino desde su posición actual hasta el destino.
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
            //Calcular punto de cuadricula a partir de una posición en el mundo, para poder trabajar con los nodos de A*.
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

            //Dibuja una cuadricula de nodos en la escena, para que podamos ver los nodos y los obstáculos mientras desarrollamos el juego.
            for (int x = 0; x < gridSizeX; x++)
                for (int y = 0; y < gridSizeY; y++)
                {
                    if (aStarNodes[x, y].isObstacle)
                        Gizmos.color = Color.red;
                    else Gizmos.color = Color.green;

                    Gizmos.DrawWireCube(ConvertGridPositionToWorldPosition(aStarNodes[x, y]), new Vector3(cellSize, cellSize, cellSize));
                }

            //Dibujar la posición de inicio y destino en la escena, para que podamos ver a dónde se dirige el auto controlado por IA mientras desarrollamos el juego.
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(startPositionDebug, 1f);

            //Dibujar el final de la posición de destino en la escena, para que podamos ver a dónde se dirige el auto controlado por IA mientras desarrollamos el juego.
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(destinationPositionDebug, 1f);
        }
    }