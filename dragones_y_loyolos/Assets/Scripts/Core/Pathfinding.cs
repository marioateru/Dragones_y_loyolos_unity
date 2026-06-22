using UnityEngine;
using System.Collections.Generic;

public static class Pathfinding
{
    private static List<Vector2Int> openList = new List<Vector2Int>(500);
    private static HashSet<Vector2Int> closedList = new HashSet<Vector2Int>(500);
    private static Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>(500);
    private static Dictionary<Vector2Int, int> gScore = new Dictionary<Vector2Int, int>(500);
    private static Dictionary<Vector2Int, int> fScore = new Dictionary<Vector2Int, int>(500);

    public static List<Vector2Int> GetAStarPath(Vector2Int start, Vector2Int target, TileCollisionChecker col, GameManager gm)
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
                Vector2Int curr = target;
                while (cameFrom.ContainsKey(curr))
                {
                    path.Add(curr);
                    curr = cameFrom[curr];
                    if (curr == start) break;
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
                        if (col.HayMuroEnRuta(current.x, current.y, current.x + x, current.y) || 
                            col.HayMuroEnRuta(current.x, current.y, current.x, current.y + y)) 
                            continue;
                    }

                    bool isWall = col.HayMuroEnRuta(current.x, current.y, neighbor.x, neighbor.y);
                    bool isDoor = gm.salaActual != null && gm.salaActual.ObtenerPuerta(neighbor.x, neighbor.y) != null;
                    if (isWall && !isDoor) continue;

                    int moveCost = 10;
                    if (gm.ObtenerEntidadEnCasilla(neighbor.x, neighbor.y) != null && neighbor != target) moveCost += 100;

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

    public static List<Vector2Int> GetBFSReachable(Vector2Int start, int maxSteps, TileCollisionChecker col)
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
                Vector2Int curr = queue.Dequeue();
                reachable.Add(curr);

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        Vector2Int neighbor = new Vector2Int(curr.x + dx, curr.y + dy);
                        
                        if (!visited.Contains(neighbor))
                        {
                            if (Mathf.Abs(dx) == 1 && Mathf.Abs(dy) == 1)
                            {
                                if (col.HayMuroEnRuta(curr.x, curr.y, curr.x + dx, curr.y) || 
                                    col.HayMuroEnRuta(curr.x, curr.y, curr.x, curr.y + dy)) 
                                    continue;
                            }

                            if (!col.HayMuroEnRuta(curr.x, curr.y, neighbor.x, neighbor.y))
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

    public static List<Vector2Int> GetValidAdjacent(Vector2Int center, int range, TileCollisionChecker col, GameManager gm)
    {
        List<Vector2Int> valid = new List<Vector2Int>();
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                if (x == 0 && y == 0) continue;
                Vector2Int target = new Vector2Int(center.x + x, center.y + y);
                
                if (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)
                {
                    if (col.HayMuroEnRuta(center.x, center.y, center.x + x, center.y) || 
                        col.HayMuroEnRuta(center.x, center.y, center.x, center.y + y)) 
                        continue;
                }

                bool isWall = col.HayMuroEnRuta(center.x, center.y, target.x, target.y);
                bool isDoor = gm.salaActual != null && gm.salaActual.ObtenerPuerta(target.x, target.y) != null;
                
                if (isWall && !isDoor) continue;
                if (gm.ObtenerEntidadEnCasilla(target.x, target.y) != null) continue;

                valid.Add(target);
            }
        }
        return valid;
    }

    public static Vector2Int GetRandomValidTile(Vector2Int center, TileCollisionChecker col, GameManager gm)
    {
        List<Vector2Int> valid = GetValidAdjacent(center, 1, col, gm);
        
        if (valid.Count > 0) 
        {
            int randomIndex = UnityEngine.Random.Range(0, valid.Count);
            return valid[randomIndex];
        }
        
        return center;
    }

    private static int GetHeuristic(Vector2Int a, Vector2Int b) => 10 * Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
}