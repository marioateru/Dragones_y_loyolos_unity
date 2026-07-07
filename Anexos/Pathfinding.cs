using UnityEngine;
using System.Collections.Generic;

public static class Pathfinding
{
    private static List<Vector2Int> openList = new List<Vector2Int>(500);
    private static HashSet<Vector2Int> closedList = new HashSet<Vector2Int>(500);
    private static Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>(500);
    private static Dictionary<Vector2Int, int> gScore = new Dictionary<Vector2Int, int>(500);
    private static Dictionary<Vector2Int, int> fScore = new Dictionary<Vector2Int, int>(500);

    // Ejecuta el algoritmo a* para determinar la ruta más óptima entre un inicio (start) y objetivo (target). Toma en cuenta muros y puertas.
    public static List<Vector2Int> GetAStarPath(Vector2Int start, Vector2Int target, TileCollisionChecker collisionChecker, GameManager gameManager)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        if (start == target) return path;

        openList.Clear();
        closedList.Clear();
        cameFrom.Clear();
        gScore.Clear();
        fScore.Clear();

        openList.Add(start);

        gScore[start] = 0;

        fScore[start] = GetHeuristic(start, target);

        int loops = 0;
        while (openList.Count > 0 && loops < 5000)
        {
            loops++;

            Vector2Int current = openList[0];

            int lowestScore = fScore.GetValueOrDefault(current, int.MaxValue);
            
            for (int i = 1; i < openList.Count; i++)
            {
                int score = fScore.GetValueOrDefault(openList[i], int.MaxValue);
                if (score < lowestScore)
                {
                    lowestScore = score;
                    current = openList[i];
                }
            }

            if (current == target)
            {
                Vector2Int currentPoint = target;

                while (cameFrom.ContainsKey(currentPoint))
                {
                    path.Add(currentPoint);

                    currentPoint = cameFrom[currentPoint];

                    if (currentPoint == start) break;
                }

                path.Reverse();

                return path;
            }

            openList.Remove(current);
            closedList.Add(current);

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;

                    Vector2Int neighbor = new Vector2Int(current.x + x, current.y + y);

                    if (closedList.Contains(neighbor)) continue;

                    if (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)
                    {
                        if (collisionChecker.HayMuroEnRuta(current.x, current.y, current.x + x, current.y) || 
                            collisionChecker.HayMuroEnRuta(current.x, current.y, current.x, current.y + y)) 
                            continue;
                    }

                    bool isWall = collisionChecker.HayMuroEnRuta(current.x, current.y, neighbor.x, neighbor.y);

                    bool isDoor = gameManager.salaActual != null && gameManager.salaActual.ObtenerPuerta(neighbor.x, neighbor.y) != null;

                    if (isWall && !isDoor) continue;

                    int moveCost = 10;

                    if (gameManager.ObtenerEntidadEnCasilla(neighbor.x, neighbor.y) != null && neighbor != target) moveCost += 100;

                    int tentativeG = gScore.GetValueOrDefault(current, 0) + moveCost;

                    if (!openList.Contains(neighbor)) openList.Add(neighbor);
                    else if (tentativeG >= gScore.GetValueOrDefault(neighbor, int.MaxValue)) continue;

                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + GetHeuristic(neighbor, target);
                }
            }
        }
        return path;
    }

    // Ejecuta el algoritmo BFS para determinar la ruta más óptima. Toma en cuenta muros.
    public static List<Vector2Int> GetBFSReachable(Vector2Int start, int maxSteps, TileCollisionChecker collisionChecker)
    {
        List<Vector2Int> reachable = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(start);
        visited.Add(start);

        int distance = 0;

        while(queue.Count > 0 && distance < maxSteps)
        {
            int levelSize = queue.Count;
            for(int i = 0; i < levelSize; i++)
            {
                Vector2Int currentPosition = queue.Dequeue();

                reachable.Add(currentPosition);

                for (int distanceX = -1; distanceX <= 1; distanceX++)
                {
                    for (int distanceY = -1; distanceY <= 1; distanceY++)
                    {
                        if (distanceX == 0 && distanceY == 0) continue;

                        Vector2Int neighbor = new Vector2Int(currentPosition.x + distanceX, currentPosition.y + distanceY);
                        
                        if (!visited.Contains(neighbor))
                        {
                            if (Mathf.Abs(distanceX) == 1 && Mathf.Abs(distanceY) == 1)
                            {
                                if (collisionChecker.HayMuroEnRuta(currentPosition.x, currentPosition.y, currentPosition.x + distanceX, currentPosition.y) || 
                                    collisionChecker.HayMuroEnRuta(currentPosition.x, currentPosition.y, currentPosition.x, currentPosition.y + distanceY)) 
                                    continue;
                            }

                            if (!collisionChecker.HayMuroEnRuta(currentPosition.x, currentPosition.y, neighbor.x, neighbor.y))
                            {
                                visited.Add(neighbor);
                                queue.Enqueue(neighbor);
                            }
                        }
                    }
                }
            }
            distance++;
        }
        return reachable;
    }

    // Determina casillas válidas alrededor de un punto. Toma en cuenta muros y puertas.
    public static List<Vector2Int> GetValidAdjacent(Vector2Int center, int range, TileCollisionChecker collisionChecker, GameManager gameManager)
    {
        List<Vector2Int> validTiles = new List<Vector2Int>();

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                if (x == 0 && y == 0) continue;

                Vector2Int target = new Vector2Int(center.x + x, center.y + y);
                
                if (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)
                {
                    if (collisionChecker.HayMuroEnRuta(center.x, center.y, center.x + x, center.y) || 
                        collisionChecker.HayMuroEnRuta(center.x, center.y, center.x, center.y + y)) 
                        continue;
                }

                bool isWall = collisionChecker.HayMuroEnRuta(center.x, center.y, target.x, target.y);

                bool isDoor = gameManager.salaActual != null && gameManager.salaActual.ObtenerPuerta(target.x, target.y) != null;
                
                if (isWall && !isDoor) continue;
                if (gameManager.ObtenerEntidadEnCasilla(target.x, target.y) != null) continue;

                validTiles.Add(target);
            }
        }
        return validTiles;
    }

    // Selecciona un conjunto de casillas aleatorias alrededor de un centro y devuelve aleatoriamente una de estas.
    public static Vector2Int GetRandomValidTile(Vector2Int center, TileCollisionChecker collisionChecker, GameManager gameManager)
    {
        List<Vector2Int> validTiles = GetValidAdjacent(center, 1, collisionChecker, gameManager);
        
        if (validTiles.Count > 0) 
        {
            int randomIndex = UnityEngine.Random.Range(0, validTiles.Count);
            return validTiles[randomIndex];
        }
        
        return center;
    }

    // Calcula la distancia máxima entre dos punto sin considerar diagonales.
    private static int GetHeuristic(Vector2Int a, Vector2Int b) => 10 * Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
}