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
    
    // [Header("Límites de IA")]
    // [Tooltip("Si el bot da este número de pasos sin cruzarse con un enemigo, asumirá que está atascado.")]
    // [SerializeField] private int limitePasosExploracion = 250;

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
                    return;
                }
                
                Vector2Int avance = CalcularSiguientePasoRutaBFS(miX, miY, pX, pY);
                if (avance.x == miX && avance.y == miY) avance = CalcularCasillaAleatoria(miX, miY);
                
                ML_Core.Instancia?.RegistrarOperacionIA();
                SubmitAction(Acciones.Moverse, avance.x, avance.y);
                return;
            }
            else
            {
                // ¡CONDICIÓN DE ÉXITO ABSOLUTO! Sin enemigos y sin puertas nuevas.
                Debug.Log("<color=green><b>[ML-BOT] ¡MAZMORRA LIMPIADA!</b> Todos los enemigos han sido derrotados. Reiniciando simulación.</color>");
                if (ML_Core.Instancia != null) ML_Core.Instancia.GestionarMuerteBot(); 
                else SubmitAction(Acciones.Moverse, miX, miY); 
                return;
            }
        }

        // 2. A PARTIR DE AQUÍ: HAY ENEMIGOS EN LA SALA
        pasosSinEnemigo = 0; 
        ML_Core.Instancia?.RegistrarContactoEnemigo();

        int vidaMax = sqlManager.ObtenerVidaMaximaDeEntidad(this.id_entidades); 
        
        // MODO SUPERVIVENCIA
        if (hp <= vidaMax * porcentajeVidaBaja)
        {
            // FIX: Ignoramos la BD. Si está herido y a salvo (lejos), SIEMPRE se cura.
            if (enemigosCercanos == 0 && hp < vidaMax)
            {
                ML_Core.Instancia?.RegistrarOperacionIA();
                SubmitAction(Acciones.Consumir, miX, miY);
                return;
            }

            // SPRINT DE HUIDA: Si hay peligro, salta con todo su rango de velocidad
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
            SubmitAction(Acciones.Moverse, flanqueo.x, flanqueo.y);
        }
        else
        {
            Vector2Int avance = CalcularSiguientePasoRutaBFS(miX, miY, enX, enY);
            if (avance.x == miX && avance.y == miY) avance = CalcularCasillaAleatoria(miX, miY);
            
            ML_Core.Instancia?.RegistrarOperacionIA();
            SubmitAction(Acciones.Moverse, avance.x, avance.y);
        }
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
        return mejorObjetivo;
    }

    private PuertaMazmorra SeleccionarMejorPuerta(int miX, int miY)
    {
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

        // Si devuelve NULL, el GameManager interpretará que la mazmorra entera está explorada
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

    private Vector2Int CalcularSiguientePasoRutaBFS(int startX, int startY, int targetX, int targetY)
    {
        Queue<Vector2Int> cola = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> padres = new Dictionary<Vector2Int, Vector2Int>();
        
        Vector2Int inicio = new Vector2Int(startX, startY);
        Vector2Int destino = new Vector2Int(targetX, targetY);
        
        cola.Enqueue(inicio);
        padres[inicio] = inicio;
        bool encontrado = false;

        while (cola.Count > 0)
        {
            Vector2Int actual = cola.Dequeue();
            if (actual == destino) { encontrado = true; break; }

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;
                    Vector2Int vecino = new Vector2Int(actual.x + x, actual.y + y);

                    if (!padres.ContainsKey(vecino) && !collisionChecker.HayMuroEnRuta(actual.x, actual.y, vecino.x, vecino.y))
                    {
                        padres[vecino] = actual;
                        cola.Enqueue(vecino);
                    }
                }
            }
        }

        if (encontrado)
        {
            Vector2Int pasoAnterior = destino;
            while (padres[pasoAnterior] != inicio)
            {
                if (padres[pasoAnterior] == inicio) return pasoAnterior;
                pasoAnterior = padres[pasoAnterior];
            }
            return pasoAnterior;
        }
        
        return inicio; 
    }

    private Vector2Int CalcularCasillaHuida(int origenX, int origenY, Entidad enemigoMasCercano)
    {
        if (enemigoMasCercano == null) return CalcularCasillaAleatoria(origenX, origenY);

        int targetX = Mathf.RoundToInt(enemigoMasCercano.xPos);
        int targetY = Mathf.RoundToInt(enemigoMasCercano.yPos);

        Vector2Int mejorCasilla = new Vector2Int(origenX, origenY);
        int mejorDistancia = -1; 

        // EL SPRINT DE HUIDA: Usamos Mathf.Max(1, velocidad) para buscar la casilla más segura de un tirón.
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