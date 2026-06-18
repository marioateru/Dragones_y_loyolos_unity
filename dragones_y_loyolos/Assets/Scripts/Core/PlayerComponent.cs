using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(TileCollisionChecker))]
public class PlayerComponent : Entidad
{
    private bool esMiTurno = false;
    
    [Header("Parámetros modo ML, no tocar.")]
    [SerializeField] private TileCollisionChecker collisionChecker;
    [SerializeField] private SQLManager sqlManager;
    [SerializeField] private int pasosSinEnemigo = 0;
    [SerializeField] private float porcentajeVidaBaja = 0.4f;
    [SerializeField] private int areaCasillasDeHuidaBFS = 15;
    private bool modoRetirada = false;
    private PuertaMazmorra puertaObjetivo = null;
    private Entidad enemigoObjetivo = null;

    // A* Star
    private List<Vector2Int> openList = new List<Vector2Int>(5000);
    private HashSet<Vector2Int> closedList = new HashSet<Vector2Int>(5000);
    private Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>(5000);
    private Dictionary<Vector2Int, int> gScore = new Dictionary<Vector2Int, int>(5000);
    private Dictionary<Vector2Int, int> fScore = new Dictionary<Vector2Int, int>(5000);
    
    private List<Vector2Int> debugRutaActual = new List<Vector2Int>();
    private Dictionary<Vector2Int, float> debugScoresBFS = new Dictionary<Vector2Int, float>();
    private Vector2Int debugMejorCasillaBFS = new Vector2Int(-9999, -9999);

    public override void Awake()
    {
        base.Awake();
        collisionChecker = GetComponent<TileCollisionChecker>();
        sqlManager = FindFirstObjectByType<SQLManager>(); 
    }

    public override void ChooseAction() 
    {
        if (IsDead()) 
        {
            SubmitAction(Acciones.Moverse, xPos, yPos); 
            return;
        }
        
        if (ML_Core.IsMLMode) 
        {
            EjecutarComportamientoBotML();
            return;
        }

        esMiTurno = true;
    }

    public bool EsSuTurno() => esMiTurno;

    public List<Acciones> DeterminarOpcionesCasilla(int targetX, int targetY)
    {
        List<Acciones> opciones = new List<Acciones>();
        Entidad entidadDestino = gameManager.ObtenerEntidadEnCasilla(targetX, targetY);
        PuertaMazmorra puerta = gameManager.salaActual.ObtenerPuerta(targetX, targetY);

        if (entidadDestino != null)
        {
            if (entidadDestino == this)
            {
                if (accionesPermitidas.Contains(Acciones.Defender)) opciones.Add(Acciones.Defender);
                if (accionesPermitidas.Contains(Acciones.Consumir)) opciones.Add(Acciones.Consumir);
            }
            else
            {
                if (accionesPermitidas.Contains(Acciones.Atacar)) opciones.Add(Acciones.Atacar);
                if (accionesPermitidas.Contains(Acciones.Interactuar)) opciones.Add(Acciones.Interactuar);
            }
        }
        else if (puerta != null)
        {
            if (accionesPermitidas.Contains(Acciones.Moverse)) opciones.Add(Acciones.Moverse);
            if (accionesPermitidas.Contains(Acciones.Interactuar)) opciones.Add(Acciones.Interactuar);
        }
        else
        {
            if (accionesPermitidas.Contains(Acciones.Moverse)) opciones.Add(Acciones.Moverse);
        }

        return opciones;
    }

    public bool ValidarIntencion(Acciones accion, int targetX, int targetY)
    {
        if (!accionesPermitidas.Contains(accion)) return false;

        int origenX = Mathf.RoundToInt(xPos);
        int origenY = Mathf.RoundToInt(yPos);

        int dist = Mathf.Max(Mathf.Abs(origenX - targetX), Mathf.Abs(origenY - targetY));

        if (accion == Acciones.Consumir || accion == Acciones.Defender)
        {
            return dist == 0; 
        }

        int rangoPermitido = 1;
        if (accion == Acciones.Moverse) rangoPermitido = Mathf.Max(1, velocidad);
        if (dist > rangoPermitido) return false;

        if (accion == Acciones.Moverse)
        {
            if (collisionChecker.HayMuroEnRuta(origenX, origenY, targetX, targetY)) return false;
            if (gameManager.ObtenerEntidadEnCasilla(targetX, targetY) != null) return false;
        }

        return true;
    }

    public void ConsumirTurno(Acciones accion, int targetX, int targetY)
    {
        esMiTurno = false;
        SubmitAction(accion, targetX, targetY);
    }

    #region Métodos para modo bot
    private void EjecutarComportamientoBotML()
    {
        debugScoresBFS.Clear(); 
        
        int miX = Mathf.RoundToInt(xPos);
        int miY = Mathf.RoundToInt(yPos);
        
        if (gameManager.salaActual != null)
        {
            ML_Core.Instancia.salasVisitadas.Add(gameManager.salaActual.idSalaActual);
        }

        int vidaMax = sqlManager.ObtenerVidaMaximaDeEntidad(this.id_entidades); 
        
        // Evaluar estado de curación
        if (hp <= vidaMax * porcentajeVidaBaja) modoRetirada = true;
        if (hp >= vidaMax) modoRetirada = false;

        int enemigosCercanos;
        Entidad enemigoPelear = EscanearMejorEnemigoGlobal(miX, miY, out enemigosCercanos); 

        // Escenario: Sala vacía (sin enemigos visibles en absoluto)
        if (enemigoPelear == null)
        {
            if (modoRetirada && hp < vidaMax)
            {
                debugRutaActual.Clear();
                ML_Core.Instancia?.RegistrarOperacionIA();
                SubmitAction(Acciones.Consumir, miX, miY);
                return;
            }

            enemigoObjetivo = null;

            PuertaMazmorra puertaDestino = SeleccionarMejorPuerta(miX, miY);
            if (puertaDestino != null)
            {
                int pX = puertaDestino.logicX;
                int pY = puertaDestino.logicY;
                
                if (Mathf.Max(Mathf.Abs(miX - pX), Mathf.Abs(miY - pY)) <= 1)
                {
                    debugRutaActual.Clear();
                    ML_Core.Instancia?.RegistrarOperacionIA();
                    SubmitAction(Acciones.Moverse, pX, pY);
                    puertaObjetivo = null;
                    return;
                }
                
                List<Vector2Int> ruta = CalcularRutaAStar(miX, miY, pX, pY);
                ML_Core.Instancia?.RegistrarOperacionIA();
                EjecutarAvanceEnRuta(ruta);
                return;
            }
            else
            {
                Debug.Log("<color=green><b>[ML-BOT] ¡MAZMORRA LIMPIADA / CALLEJON!</b> Reiniciando simulación.</color>");
                if (ML_Core.Instancia != null) ML_Core.Instancia.GestionarMuerteBot(); 
                else SubmitAction(Acciones.Moverse, miX, miY); 
                return;
            }
        }

        pasosSinEnemigo = 0; 
        puertaObjetivo = null;
        ML_Core.Instancia?.RegistrarContactoEnemigo();

        if (modoRetirada)
        {
            if (enemigosCercanos == 0)
            {
                debugRutaActual.Clear();
                ML_Core.Instancia?.RegistrarOperacionIA();
                SubmitAction(Acciones.Consumir, miX, miY);
                return;
            }

            Vector2Int escape = CalcularCasillaHuida(miX, miY, enemigoPelear); 
            List<Vector2Int> ruta = CalcularRutaAStar(miX, miY, escape.x, escape.y);
            ML_Core.Instancia?.RegistrarOperacionIA();
            EjecutarAvanceEnRuta(ruta);
            return;
        }

        int enX = Mathf.RoundToInt(enemigoPelear.xPos);
        int enY = Mathf.RoundToInt(enemigoPelear.yPos);
        int distancia = Mathf.Max(Mathf.Abs(miX - enX), Mathf.Abs(miY - enY));

        if (distancia <= 1)
        {
            debugRutaActual.Clear();
            ML_Core.Instancia?.RegistrarOperacionIA();
            SubmitAction(Acciones.Atacar, enX, enY);
        }
        else if (enemigosCercanos >= 2 && distancia <= 4)
        {
            Vector2Int flanqueo = CalcularCasillaFlanqueo(miX, miY, enX, enY);
            List<Vector2Int> ruta = CalcularRutaAStar(miX, miY, flanqueo.x, flanqueo.y);
            ML_Core.Instancia?.RegistrarOperacionIA();
            EjecutarAvanceEnRuta(ruta);
        }
        else
        {
            List<Vector2Int> ruta = CalcularRutaAStar(miX, miY, enX, enY);
            ML_Core.Instancia?.RegistrarOperacionIA();
            EjecutarAvanceEnRuta(ruta);
        }
    }

    private void EjecutarAvanceEnRuta(List<Vector2Int> ruta)
    {
        debugRutaActual = ruta;

        if (ruta == null || ruta.Count == 0) 
        {
            Vector2Int fallback = CalcularCasillaAleatoria(Mathf.RoundToInt(xPos), Mathf.RoundToInt(yPos));
            EjecutarPasoSimple(fallback.x, fallback.y);
            return;
        }

        int miX = Mathf.RoundToInt(xPos);
        int miY = Mathf.RoundToInt(yPos);

        int maxVelocidad = Mathf.Max(1, velocidad);
        int pasosAleatorios = UnityEngine.Random.Range(1, maxVelocidad + 1);
        int maxPasos = Mathf.Min(ruta.Count, pasosAleatorios);
        
        Vector2Int mejorSalto = ruta[0];
        
        for (int i = maxPasos - 1; i >= 0; i--)
        {
            Vector2Int nodo = ruta[i];
            
            if (i > 0 && gameManager.ObtenerEntidadEnCasilla(nodo.x, nodo.y) != null) continue;

            if (!collisionChecker.HayMuroEnRuta(miX, miY, nodo.x, nodo.y))
            {
                mejorSalto = nodo;
                break;
            }
        }

        EjecutarPasoSimple(mejorSalto.x, mejorSalto.y);
    }

    private void EjecutarPasoSimple(int x, int y)
    {
        Entidad obstaculo = gameManager.ObtenerEntidadEnCasilla(x, y);
        if (obstaculo != null && obstaculo != this)
            SubmitAction(Acciones.Atacar, x, y);
        else
            SubmitAction(Acciones.Moverse, x, y);
    }

    private Entidad EscanearMejorEnemigoGlobal(int origenX, int origenY, out int enemigosCercanos)
    {
        Entidad mejorObjetivo = null;
        int mejorPrioridad = int.MaxValue; 
        enemigosCercanos = 0;

        foreach (var entidad in gameManager.ObtenerTodasLasEntidades())
        {
            if (entidad is EnemyComponent && !entidad.IsDead())
            {
                int ex = Mathf.RoundToInt(entidad.xPos);
                int ey = Mathf.RoundToInt(entidad.yPos);
                int dist = Mathf.Max(Mathf.Abs(origenX - ex), Mathf.Abs(origenY - ey));

                if (dist <= 3) enemigosCercanos++; 

                int score = (dist * 10) + entidad.hp; 
                if (collisionChecker.HayMuroEnRuta(origenX, origenY, ex, ey)) score += 100;

                if (score < mejorPrioridad)
                {
                    mejorPrioridad = score;
                    mejorObjetivo = entidad;
                }
            }
        }

        if (enemigoObjetivo != null && !enemigoObjetivo.IsDead() && gameManager.ObtenerTodasLasEntidades().Contains(enemigoObjetivo))
        {
            return enemigoObjetivo;
        }

        enemigoObjetivo = mejorObjetivo;
        return mejorObjetivo;
    }

    private PuertaMazmorra SeleccionarMejorPuerta(int miCoordX, int miCoordY)
    {
        if (puertaObjetivo != null && gameManager.salaActual.ObtenerTodasLasPuertas().Contains(puertaObjetivo))
        {
            if (ML_Core.Instancia != null && !ML_Core.Instancia.salasVisitadas.Contains(puertaObjetivo.idSalaDestino))
            {
                return puertaObjetivo;
            }
        }

        List<PuertaMazmorra> todasPuertas = gameManager.salaActual.ObtenerTodasLasPuertas();
        if (todasPuertas.Count == 0) return null;

        PuertaMazmorra puertaInexplorada = null;
        float mejorDist = float.MaxValue;

        foreach (var puerta in todasPuertas)
        {
            if (ML_Core.Instancia != null && !ML_Core.Instancia.salasVisitadas.Contains(puerta.idSalaDestino))
            {
                float dist = Mathf.Max(Mathf.Abs(miCoordX - puerta.logicX), Mathf.Abs(miCoordY - puerta.logicY));
                if (dist < mejorDist)
                {
                    mejorDist = dist;
                    puertaInexplorada = puerta;
                }
            }
        }

        puertaObjetivo = puertaInexplorada;
        return puertaInexplorada; 
    }

    private Vector2Int CalcularCasillaFlanqueo(int origenX, int origenY, int objetivoX, int objetivoY)
    {
        List<Vector2Int> candidatas = ObtenerCasillasValidasAlrededor(origenX, origenY, 1);
        if (candidatas.Count == 0) return CalcularCasillaAleatoria(origenX, origenY);

        Vector2Int mejor = new Vector2Int(origenX, origenY);
        int mejorPuntuacion = -9999;

        foreach (var cas in candidatas)
        {
            int distObjetivo = Mathf.Max(Mathf.Abs(cas.x - objetivoX), Mathf.Abs(cas.y - objetivoY));
            int distOtrosEnemigos = 0;

            foreach (var e in gameManager.ObtenerTodasLasEntidades())
            {
                if (e is EnemyComponent && !e.IsDead() && Mathf.RoundToInt(e.xPos) != objetivoX)
                {
                    distOtrosEnemigos += Mathf.Max(Mathf.Abs(cas.x - Mathf.RoundToInt(e.xPos)), Mathf.Abs(cas.y - Mathf.RoundToInt(e.yPos)));
                }
            }

            int puntuacion = distOtrosEnemigos - (distObjetivo * 3);
            if (puntuacion > mejorPuntuacion)
            {
                mejorPuntuacion = puntuacion;
                mejor = cas;
            }
        }
        return mejor;
    }

    private List<Vector2Int> CalcularRutaAStar(int startX, int startY, int targetX, int targetY)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int start = new Vector2Int(startX, startY);
        Vector2Int target = new Vector2Int(targetX, targetY);

        if (start == target) return path;

        openList.Clear();
        closedList.Clear();
        cameFrom.Clear();
        gScore.Clear();
        fScore.Clear();

        openList.Add(start);
        gScore[start] = 0;
        fScore[start] = Mathf.Max(Mathf.Abs(start.x - target.x), Mathf.Abs(start.y - target.y));

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
                        if (collisionChecker.HayMuroEnRuta(current.x, current.y, current.x + x, current.y) || 
                            collisionChecker.HayMuroEnRuta(current.x, current.y, current.x, current.y + y)) 
                            continue;
                    }

                    bool isWall = collisionChecker.HayMuroEnRuta(current.x, current.y, neighbor.x, neighbor.y);
                    bool isDoor = gameManager.salaActual != null && gameManager.salaActual.ObtenerPuerta(neighbor.x, neighbor.y) != null;
                    if (isWall && !isDoor) continue;

                    int moveCost = 10;
                    
                    if (gameManager.ObtenerEntidadEnCasilla(neighbor.x, neighbor.y) != null && neighbor != target)
                    {
                        moveCost += 100; 
                    }

                    int tentativeG = gScore.GetValueOrDefault(current, 0) + moveCost;

                    if (!openList.Contains(neighbor)) openList.Add(neighbor);
                    else if (tentativeG >= gScore.GetValueOrDefault(neighbor, int.MaxValue)) continue;

                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Mathf.Max(Mathf.Abs(neighbor.x - target.x), Mathf.Abs(neighbor.y - target.y));
                }
            }
        }
        
        return path; 
    }

    private List<Vector2Int> GetReachableTiles(int startX, int startY, int maxPasos)
    {
        List<Vector2Int> reachable = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        Vector2Int start = new Vector2Int(startX, startY);
        queue.Enqueue(start);
        visited.Add(start);

        int distance = 0;
        
        while(queue.Count > 0 && distance < maxPasos)
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
                                if (collisionChecker.HayMuroEnRuta(curr.x, curr.y, curr.x + dx, curr.y) || 
                                    collisionChecker.HayMuroEnRuta(curr.x, curr.y, curr.x, curr.y + dy)) 
                                    continue;
                            }

                            if (!collisionChecker.HayMuroEnRuta(curr.x, curr.y, neighbor.x, neighbor.y))
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

    private Vector2Int CalcularCasillaHuida(int origenX, int origenY, Entidad enemigoMasCercano)
    {
        int targetX = Mathf.RoundToInt(enemigoMasCercano.xPos);
        int targetY = Mathf.RoundToInt(enemigoMasCercano.yPos);

        List<Vector2Int> candidatas = GetReachableTiles(origenX, origenY, areaCasillasDeHuidaBFS);
        
        Vector2Int mejorCasilla = new Vector2Int(origenX, origenY);
        int mejorPuntuacion = -999999;
        int minPuntuacion = 999999;

        Dictionary<Vector2Int, int> rawScores = new Dictionary<Vector2Int, int>();

        foreach(var cas in candidatas)
        {
            int distEnemigo = Mathf.Max(Mathf.Abs(cas.x - targetX), Mathf.Abs(cas.y - targetY));
            
            int murosAdyacentes = 0;
            if (collisionChecker.HayMuroEnRuta(cas.x, cas.y, cas.x+1, cas.y)) murosAdyacentes++;
            if (collisionChecker.HayMuroEnRuta(cas.x, cas.y, cas.x-1, cas.y)) murosAdyacentes++;
            if (collisionChecker.HayMuroEnRuta(cas.x, cas.y, cas.x, cas.y+1)) murosAdyacentes++;
            if (collisionChecker.HayMuroEnRuta(cas.x, cas.y, cas.x, cas.y-1)) murosAdyacentes++;

            int puntuacion = (distEnemigo * 10) + (murosAdyacentes * 50); 
            rawScores[cas] = puntuacion;
            
            if (puntuacion > mejorPuntuacion)
            {
                mejorPuntuacion = puntuacion;
                mejorCasilla = cas;
            }
            if (puntuacion < minPuntuacion)
            {
                minPuntuacion = puntuacion;
            }
        }

        // Normalizar puntuaciones para pintar colores de 0 a 1
        foreach(var kvp in rawScores)
        {
            float norm = 0f;
            if (mejorPuntuacion > minPuntuacion)
            {
                norm = (float)(kvp.Value - minPuntuacion) / (mejorPuntuacion - minPuntuacion);
            }
            debugScoresBFS[kvp.Key] = norm;
        }

        debugMejorCasillaBFS = mejorCasilla;
        return mejorCasilla;
    }

    private Vector2Int CalcularCasillaAleatoria(int origenX, int origenY)
    {
        List<Vector2Int> casillasValidas = ObtenerCasillasValidasAlrededor(origenX, origenY, 1);
        if (casillasValidas.Count > 0) return casillasValidas[UnityEngine.Random.Range(0, casillasValidas.Count)];
        return new Vector2Int(origenX, origenY);
    }

    private List<Vector2Int> ObtenerCasillasValidasAlrededor(int origenX, int origenY, int rango)
    {
        List<Vector2Int> lista = new List<Vector2Int>();
        for (int x = -rango; x <= rango; x++)
        {
            for (int y = -rango; y <= rango; y++)
            {
                if (x == 0 && y == 0) continue;
                int cx = origenX + x;
                int cy = origenY + y;
                
                if (!collisionChecker.HayMuroEnRuta(origenX, origenY, cx, cy) && gameManager.ObtenerEntidadEnCasilla(cx, cy) == null)
                {
                    lista.Add(new Vector2Int(cx, cy));
                }
            }
        }
        return lista;
    }
    
    private void OnDrawGizmos()
    {
        // 1. Draw BFS Search Area (Gradient Red->Yellow->Greenish)
        if (debugScoresBFS != null && debugScoresBFS.Count > 0)
        {
            foreach (var kvp in debugScoresBFS)
            {
                Vector3 wpPos = new Vector3(kvp.Key.x + 0.5f, -kvp.Key.y - 0.5f, 0f);
                
                if (kvp.Key == debugMejorCasillaBFS)
                {
                    // Casilla ganadora - Verde puro y más grande
                    Gizmos.color = new Color(0f, 1f, 0f, 0.85f); 
                    Gizmos.DrawCube(wpPos, new Vector3(0.9f, 0.9f, 0.1f));
                }
                else
                {
                    // Resto de casillas en gradiente
                    Gizmos.color = Color.Lerp(new Color(1f, 0f, 0f, 0.35f), new Color(0.5f, 1f, 0f, 0.35f), kvp.Value);
                    Gizmos.DrawCube(wpPos, new Vector3(0.8f, 0.8f, 0.1f));
                }
            }
        }

        // 2. Draw A* Pathing (Cyan dots & lines)
        if (debugRutaActual == null || debugRutaActual.Count == 0) return;

        Gizmos.color = Color.cyan;
        Vector3 previousPos = new Vector3(xPos + 0.5f, -yPos - 0.5f, 0f);
        
        foreach (var wp in debugRutaActual)
        {
            Vector3 wpPos = new Vector3(wp.x + 0.5f, -wp.y - 0.5f, 0f);
            Gizmos.DrawSphere(wpPos, 0.2f);
            Gizmos.DrawLine(previousPos, wpPos);
            previousPos = wpPos;
        }
    }
    #endregion
}