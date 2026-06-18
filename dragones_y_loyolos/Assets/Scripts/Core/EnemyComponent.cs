using UnityEngine;
using System.Collections.Generic;

public class EnemyComponent : Entidad
{
    [Header("Configuración IA")]
    [Tooltip("Distancia a la que el enemigo detectará al jugador.")]
    public int rangoVision = 8; 

    [Range(0f, 1f)] public float umbralHuida = 0.3f;
    [Range(0f, 1f)] public float probContraataque = 0.35f;
    
    private PlayerComponent jugadorObjetivo;
    private TileCollisionChecker collisionChecker;
    private SQLManager sqlManager;
    
    private int vidaMaxima = -1;

    // A*  
    private List<Vector2Int> openList = new List<Vector2Int>(5000);
    private HashSet<Vector2Int> closedList = new HashSet<Vector2Int>(5000);
    private Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>(5000);
    private Dictionary<Vector2Int, int> gScore = new Dictionary<Vector2Int, int>(5000);
    private Dictionary<Vector2Int, int> fScore = new Dictionary<Vector2Int, int>(5000);

    public override void Awake()
    {
        base.Awake();
        collisionChecker = FindFirstObjectByType<TileCollisionChecker>();
        sqlManager = FindFirstObjectByType<SQLManager>();
    }

    public override void ChooseAction()
    {
        if (IsDead()) 
        {
            SubmitAction(Acciones.Moverse, xPos, yPos); 
            return;
        }

        if (vidaMaxima <= 0) 
            vidaMaxima = sqlManager.ObtenerVidaMaximaDeEntidad(this.id_entidades);

        if (jugadorObjetivo == null) jugadorObjetivo = FindFirstObjectByType<PlayerComponent>();

        int miX = Mathf.RoundToInt(xPos);
        int miY = Mathf.RoundToInt(yPos);

        if (jugadorObjetivo == null || jugadorObjetivo.IsDead())
        {
            Vector2Int patrulla = CalcularCasillaAleatoria(miX, miY);
            SubmitAction(Acciones.Moverse, patrulla.x, patrulla.y);
            return;
        }

        int jugadorX = Mathf.RoundToInt(jugadorObjetivo.xPos);
        int jugadorY = Mathf.RoundToInt(jugadorObjetivo.yPos);
        int distReal = Mathf.Max(Mathf.Abs(miX - jugadorX), Mathf.Abs(miY - jugadorY));

        bool tieneLineaVision = !collisionChecker.HayMuroEnRuta(miX, miY, jugadorX, jugadorY);

        if (distReal > rangoVision || !tieneLineaVision)
        {
            Vector2Int patrulla = CalcularCasillaAleatoria(miX, miY);
            SubmitAction(Acciones.Moverse, patrulla.x, patrulla.y);
            return;
        }

        bool quiereHuir = (hp <= vidaMaxima * umbralHuida);
        bool valiente = quiereHuir && (UnityEngine.Random.value <= probContraataque);

        if (quiereHuir && !valiente)
        {
            Vector2Int escape = CalcularSiguientePasoHuida(miX, miY, jugadorObjetivo);
            if (escape.x != -9999)
            {
                ProcesarPaso(escape);
                return;
            }
        }

        if (distReal <= 1)
        {
            SubmitAction(Acciones.Atacar, jugadorX, jugadorY);
            return;
        }

        Vector2Int avance = CalcularSiguientePasoAStar(miX, miY, jugadorX, jugadorY);

        if (avance.x != -9999)
        {
            ProcesarPaso(avance);
        }
        else
        {
            Vector2Int fallback = CalcularCasillaAleatoria(miX, miY);
            ProcesarPaso(fallback);
        }
    }

    private void ProcesarPaso(Vector2Int avance)
    {
        Entidad obstaculo = gameManager.ObtenerEntidadEnCasilla(avance.x, avance.y);
        if (obstaculo != null && obstaculo != this)
            SubmitAction(Acciones.Atacar, avance.x, avance.y);
        else
            SubmitAction(Acciones.Moverse, avance.x, avance.y);
    }

    private Vector2Int CalcularSiguientePasoAStar(int startX, int startY, int targetX, int targetY)
    {
        Vector2Int start = new Vector2Int(startX, startY);
        Vector2Int target = new Vector2Int(targetX, targetY);

        if (start == target) return start;

        openList.Clear();
        closedList.Clear();
        cameFrom.Clear();
        gScore.Clear();
        fScore.Clear();

        openList.Add(start);
        gScore[start] = 0;
        fScore[start] = 10 * (Mathf.Abs(start.x - target.x) + Mathf.Abs(start.y - target.y));

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
                    Vector2Int prev = cameFrom[curr];
                    if (prev == start) return curr;
                    curr = prev;
                }
                return start;
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

                    bool isWall = collisionChecker.HayMuroEnRuta(current.x, current.y, neighbor.x, neighbor.y);
                    bool isDoor = gameManager.salaActual != null && gameManager.salaActual.ObtenerPuerta(neighbor.x, neighbor.y) != null;
                    if (isWall && !isDoor) continue;

                    if (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)
                    {
                        if (collisionChecker.HayMuroEnRuta(current.x, current.y, current.x + x, current.y) || 
                            collisionChecker.HayMuroEnRuta(current.x, current.y, current.x, current.y + y)) 
                            continue;
                    }

                    int moveCost = (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1) ? 14 : 10;
                    
                    if (gameManager.ObtenerEntidadEnCasilla(neighbor.x, neighbor.y) != null && neighbor != target)
                    {
                        moveCost += 100; 
                    }

                    int tentativeG = gScore[current] + moveCost;

                    if (!openList.Contains(neighbor)) openList.Add(neighbor);
                    else if (tentativeG >= gScore.GetValueOrDefault(neighbor, int.MaxValue)) continue;

                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + 10 * (Mathf.Abs(neighbor.x - target.x) + Mathf.Abs(neighbor.y - target.y));
                }
            }
        }
        return new Vector2Int(-9999, -9999);
    }

    private Vector2Int CalcularSiguientePasoHuida(int origenX, int origenY, Entidad enemigo)
    {
        int ex = Mathf.RoundToInt(enemigo.xPos);
        int ey = Mathf.RoundToInt(enemigo.yPos);
        Vector2Int mejor = new Vector2Int(origenX, origenY);
        int mejorDist = -1;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                if (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)
                {
                    if (collisionChecker.HayMuroEnRuta(origenX, origenY, origenX + x, origenY) || 
                        collisionChecker.HayMuroEnRuta(origenX, origenY, origenX, origenY + y)) 
                        continue;
                }

                Vector2Int vec = new Vector2Int(origenX + x, origenY + y);
                bool isWall = collisionChecker.HayMuroEnRuta(origenX, origenY, vec.x, vec.y);
                bool isDoor = gameManager.salaActual != null && gameManager.salaActual.ObtenerPuerta(vec.x, vec.y) != null;
                
                if (isWall && !isDoor) continue;
                if (gameManager.ObtenerEntidadEnCasilla(vec.x, vec.y) != null) continue;

                int dist = Mathf.Max(Mathf.Abs(vec.x - ex), Mathf.Abs(vec.y - ey));
                if (dist > mejorDist)
                {
                    mejorDist = dist;
                    mejor = vec;
                }
            }
        }
        return mejor;
    }

    private Vector2Int CalcularCasillaAleatoria(int origenX, int origenY)
    {
        List<Vector2Int> validas = new List<Vector2Int>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                if (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)
                {
                    if (collisionChecker.HayMuroEnRuta(origenX, origenY, origenX + x, origenY) || 
                        collisionChecker.HayMuroEnRuta(origenX, origenY, origenX, origenY + y)) 
                        continue;
                }

                Vector2Int vec = new Vector2Int(origenX + x, origenY + y);
                bool isWall = collisionChecker.HayMuroEnRuta(origenX, origenY, vec.x, vec.y);
                bool isDoor = gameManager.salaActual != null && gameManager.salaActual.ObtenerPuerta(vec.x, vec.y) != null;
                
                if (isWall && !isDoor) continue;
                if (gameManager.ObtenerEntidadEnCasilla(vec.x, vec.y) != null) continue;

                validas.Add(vec);
            }
        }
        if (validas.Count > 0) return validas[UnityEngine.Random.Range(0, validas.Count)];
        return new Vector2Int(origenX, origenY);
    }
}