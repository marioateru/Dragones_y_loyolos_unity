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

    // TARGET LOCKS: Evitan que el bot baile entre dos opciones con distancias similares
    private PuertaMazmorra puertaObjetivo = null;
    private Entidad enemigoObjetivo = null;

    // A* RAM (Reutilizada para que el Garbage Collector no sature la simulación)
    private List<Vector2Int> openList = new List<Vector2Int>(5000);
    private HashSet<Vector2Int> closedList = new HashSet<Vector2Int>(5000);
    private Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>(5000);
    private Dictionary<Vector2Int, int> gScore = new Dictionary<Vector2Int, int>(5000);
    private Dictionary<Vector2Int, int> fScore = new Dictionary<Vector2Int, int>(5000);

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
        int miX = Mathf.RoundToInt(xPos);
        int miY = Mathf.RoundToInt(yPos);
        
        if (gameManager.salaActual != null)
        {
            ML_Core.Instancia.salasVisitadas.Add(gameManager.salaActual.idSalaActual);
        }

        // 1. OMNISCIENCIA: Escanea la sala entera al instante
        int enemigosCercanos;
        Entidad enemigoPelear = EscanearMejorEnemigoGlobal(miX, miY, out enemigosCercanos); 

        // Si NO hay enemigos en toda la sala...
        if (enemigoPelear == null)
        {
            enemigoObjetivo = null; // Soltamos lock de enemigo

            PuertaMazmorra puertaDestino = SeleccionarMejorPuerta(miX, miY);
            if (puertaDestino != null)
            {
                // Todavía quedan salas por limpiar, vamos a la puerta
                int pX = puertaDestino.logicX;
                int pY = puertaDestino.logicY;
                
                if (Mathf.Max(Mathf.Abs(miX - pX), Mathf.Abs(miY - pY)) <= 1)
                {
                    ML_Core.Instancia?.RegistrarOperacionIA();
                    SubmitAction(Acciones.Moverse, pX, pY);
                    puertaObjetivo = null; // Reseteamos al cruzar
                    return;
                }
                
                Vector2Int avance = CalcularSiguientePasoAStar(miX, miY, pX, pY);
                if (avance.x == -9999) avance = CalcularCasillaAleatoria(miX, miY);
                
                ML_Core.Instancia?.RegistrarOperacionIA();
                EjecutarPaso(avance.x, avance.y);
                return;
            }
            else
            {
                // ¡CONDICIÓN DE ÉXITO ABSOLUTO O CALLEJÓN SIN SALIDA! 
                // Sin enemigos y sin puertas nuevas -> La simulación debe terminar este episodio.
                Debug.Log("<color=green><b>[ML-BOT] ¡MAZMORRA LIMPIADA / CALLEJON!</b> Reiniciando simulación.</color>");
                if (ML_Core.Instancia != null) ML_Core.Instancia.GestionarMuerteBot(); 
                else SubmitAction(Acciones.Moverse, miX, miY); 
                return;
            }
        }

        // 2. A PARTIR DE AQUÍ: HAY ENEMIGOS EN LA SALA
        pasosSinEnemigo = 0; 
        puertaObjetivo = null; // Si hay combate, nos olvidamos de la puerta temporalmente
        ML_Core.Instancia?.RegistrarContactoEnemigo();

        int vidaMax = sqlManager.ObtenerVidaMaximaDeEntidad(this.id_entidades); 
        
        // MODO SUPERVIVENCIA
        if (hp <= vidaMax * porcentajeVidaBaja)
        {
            if (enemigosCercanos == 0 && hp < vidaMax)
            {
                ML_Core.Instancia?.RegistrarOperacionIA();
                SubmitAction(Acciones.Consumir, miX, miY);
                return;
            }

            Vector2Int escape = CalcularCasillaHuida(miX, miY, enemigoPelear); 
            ML_Core.Instancia?.RegistrarOperacionIA();
            SubmitAction(Acciones.Moverse, escape.x, escape.y);
            return;
        }

        // 3. COMBATE TÁCTICO
        int enX = Mathf.RoundToInt(enemigoPelear.xPos);
        int enY = Mathf.RoundToInt(enemigoPelear.yPos);
        int distancia = Mathf.Max(Mathf.Abs(miX - enX), Mathf.Abs(miY - enY));

        if (distancia <= 1)
        {
            ML_Core.Instancia?.RegistrarOperacionIA();
            SubmitAction(Acciones.Atacar, enX, enY);
        }
        else if (enemigosCercanos >= 2 && distancia <= 4)
        {
            Vector2Int flanqueo = CalcularCasillaFlanqueo(miX, miY, enX, enY);
            ML_Core.Instancia?.RegistrarOperacionIA();
            EjecutarPaso(flanqueo.x, flanqueo.y);
        }
        else
        {
            Vector2Int avance = CalcularSiguientePasoAStar(miX, miY, enX, enY);
            if (avance.x == -9999) avance = CalcularCasillaAleatoria(miX, miY);
            
            ML_Core.Instancia?.RegistrarOperacionIA();
            EjecutarPaso(avance.x, avance.y);
        }
    }

    private void EjecutarPaso(int x, int y)
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

        // TARGET LOCK: Mantenemos el mismo objetivo a menos que muera
        if (enemigoObjetivo != null && !enemigoObjetivo.IsDead() && gameManager.ObtenerTodasLasEntidades().Contains(enemigoObjetivo))
        {
            return enemigoObjetivo;
        }

        enemigoObjetivo = mejorObjetivo;
        return mejorObjetivo;
    }

    private PuertaMazmorra SeleccionarMejorPuerta(int miX, int miY)
    {
        // TARGET LOCK: Si ya apuntamos a una puerta y es válida (no visitada), seguimos yendo allí sin cambiar de opinión
        if (puertaObjetivo != null && gameManager.salaActual.ObtenerTodasLasPuertas().Contains(puertaObjetivo))
        {
            if (ML_Core.Instancia != null && !ML_Core.Instancia.salasVisitadas.Contains(puertaObjetivo.idSalaDestino))
            {
                return puertaObjetivo;
            }
        }

        List<PuertaMazmorra> todas = gameManager.salaActual.ObtenerTodasLasPuertas();
        if (todas.Count == 0) return null;

        PuertaMazmorra puertaInexplorada = null;
        float mejorDist = float.MaxValue;

        foreach (var p in todas)
        {
            if (ML_Core.Instancia != null && !ML_Core.Instancia.salasVisitadas.Contains(p.idSalaDestino))
            {
                float dist = Mathf.Max(Mathf.Abs(miX - p.logicX), Mathf.Abs(miY - p.logicY));
                if (dist < mejorDist)
                {
                    mejorDist = dist;
                    puertaInexplorada = p;
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

    // REEMPLAZO DEL BFS POR A* (Más robusto para esquinas, no se queda atascado)
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

                    // CHEQUEO DE ESQUINAS: Evita que intente caminar a través de bordes sólidos en diagonal
                    if (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)
                    {
                        if (collisionChecker.HayMuroEnRuta(current.x, current.y, current.x + x, current.y) || 
                            collisionChecker.HayMuroEnRuta(current.x, current.y, current.x, current.y + y)) 
                            continue;
                    }

                    // CHEQUEO DE MUROS
                    bool isWall = collisionChecker.HayMuroEnRuta(current.x, current.y, neighbor.x, neighbor.y);
                    bool isDoor = gameManager.salaActual != null && gameManager.salaActual.ObtenerPuerta(neighbor.x, neighbor.y) != null;
                    if (isWall && !isDoor) continue;

                    int moveCost = 10;
                    
                    // Si hay un enemigo bloqueando, suma costo pero permite que el pathfinding cruce por ahí 
                    // (lo atacará al intentar pisar gracias a EjecutarPaso)
                    if (gameManager.ObtenerEntidadEnCasilla(neighbor.x, neighbor.y) != null && neighbor != target)
                    {
                        moveCost += 100; 
                    }

                    int tentativeG = gScore[current] + moveCost;

                    if (!openList.Contains(neighbor)) openList.Add(neighbor);
                    else if (tentativeG >= gScore.GetValueOrDefault(neighbor, int.MaxValue)) continue;

                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Mathf.Max(Mathf.Abs(neighbor.x - target.x), Mathf.Abs(neighbor.y - target.y));
                }
            }
        }
        
        return new Vector2Int(-9999, -9999); 
    }

    private Vector2Int CalcularCasillaHuida(int origenX, int origenY, Entidad enemigoMasCercano)
    {
        if (enemigoMasCercano == null) return CalcularCasillaAleatoria(origenX, origenY);

        int targetX = Mathf.RoundToInt(enemigoMasCercano.xPos);
        int targetY = Mathf.RoundToInt(enemigoMasCercano.yPos);

        Vector2Int mejorCasilla = new Vector2Int(origenX, origenY);
        int mejorDistancia = -1; 

        int rangoEscape = Mathf.Max(1, velocidad);
        List<Vector2Int> candidatas = ObtenerCasillasValidasAlrededor(origenX, origenY, rangoEscape);

        foreach (var cas in candidatas)
        {
            int distAlEnemigo = Mathf.Max(Mathf.Abs(cas.x - targetX), Mathf.Abs(cas.y - targetY));
            if (distAlEnemigo > mejorDistancia)
            {
                mejorDistancia = distAlEnemigo;
                mejorCasilla = cas;
            }
        }
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
    #endregion
}