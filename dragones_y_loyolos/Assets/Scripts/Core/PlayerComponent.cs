using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(TileCollisionChecker))]
public class PlayerComponent : Entidad
{
    private bool esMiTurno = false;
    
    [Header("Parámetros modo ML, no tocar.")]
    [SerializeField] private TileCollisionChecker collisionChecker;
    [SerializeField] private SQLManager sqlManager;
    [SerializeField] private float porcentajeVidaBaja = 0.4f;
    [SerializeField] private float probContraataqueML = 0.15f;

    // MEMORIA SIMPLE: Rastro de olor
    private List<Vector2Int> historialPosiciones = new List<Vector2Int>();
    private HashSet<Entidad> objetivosInalcanzables = new HashSet<Entidad>();
    private PuertaMazmorra puertaObjetivoActual = null; 
    private int lastEnemigosVivos = -1; 
    
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
            if (ML_Core.IsMLMode && ML_Core.Instancia != null) 
            {
                gameManager.enabled = false;
                ML_Core.Instancia.GestionarMuerteBot();
                return;
            }
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

        if (accion == Acciones.Consumir || accion == Acciones.Defender) return dist == 0; 

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
        
        // Rastro de olor: Guardamos hasta 15 pasos.
        historialPosiciones.Add(new Vector2Int(miX, miY));
        if (historialPosiciones.Count > 15) historialPosiciones.RemoveAt(0);
        
        if (gameManager.salaActual != null)
        {
            ML_Core.Instancia.salasVisitadas.Add(gameManager.salaActual.idSalaActual);
        }

        int enemigosVivosActuales = 0;
        foreach(var e in gameManager.ObtenerTodasLasEntidades()) if (e is EnemyComponent && !e.IsDead()) enemigosVivosActuales++;
        
        if (lastEnemigosVivos != -1 && enemigosVivosActuales < lastEnemigosVivos) 
        {
            ML_Core.Instancia?.RegistrarMuerteEnemigo();
            objetivosInalcanzables.Clear(); 
        }
        lastEnemigosVivos = enemigosVivosActuales;

        int enemigosCercanos;
        Entidad enemigoPelear = EscanearMejorEnemigoGlobal(miX, miY, out enemigosCercanos); 

        // NO HAY ENEMIGOS: BUSCAR PUERTA
        if (enemigoPelear == null)
        {
            if (puertaObjetivoActual == null || gameManager.salaActual == null || !gameManager.salaActual.ObtenerTodasLasPuertas().Contains(puertaObjetivoActual))
            {
                puertaObjetivoActual = SeleccionarPuertaAleatoria();
            }

            if (puertaObjetivoActual != null)
            {
                int pX = puertaObjetivoActual.logicX;
                int pY = puertaObjetivoActual.logicY;
                
                if (Mathf.Max(Mathf.Abs(miX - pX), Mathf.Abs(miY - pY)) <= 1)
                {
                    ML_Core.Instancia?.RegistrarOperacionIA();
                    SubmitAction(Acciones.Moverse, pX, pY);
                    
                    objetivosInalcanzables.Clear();
                    puertaObjetivoActual = null; 
                    return;
                }
                
                Vector2Int avance = CalcularPasoGreedy(miX, miY, pX, pY);

                if (avance.x == miX && avance.y == miY) 
                {
                    puertaObjetivoActual = null;
                    SubmitAction(Acciones.Moverse, miX, miY);
                    return;
                }

                ML_Core.Instancia?.RegistrarOperacionIA();
                SubmitAction(Acciones.Moverse, avance.x, avance.y);
                return;
            }
            else
            {
                if (objetivosInalcanzables.Count > 0)
                {
                    SubmitAction(Acciones.Moverse, miX, miY); 
                    return;
                }

                Debug.Log("<color=green><b>[ML-BOT] ¡SALA AISLADA / COMPLETA!</b> Reiniciando simulación.</color>");
                if (ML_Core.Instancia != null) 
                {
                    gameManager.enabled = false;
                    ML_Core.Instancia.GestionarMuerteBot(); 
                    return;
                }
                SubmitAction(Acciones.Moverse, miX, miY); 
                return;
            }
        }

        ML_Core.Instancia?.RegistrarContactoEnemigo();
        puertaObjetivoActual = null; 
        
        int vidaMax = this.MaxHpT0; 
        
        if (hp <= vidaMax * porcentajeVidaBaja)
        {
            bool valiente = UnityEngine.Random.value <= probContraataqueML;

            if (!valiente)
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
            else
            {
                Debug.Log("<color=orange>[ML-BOT]</color> Herido, pero decide contraatacar a la desesperada.");
            }
        }

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
            // GREEDY STEP: Va directo. Si choca, penaliza la casilla en memoria y rodea.
            Vector2Int avance = CalcularPasoGreedy(miX, miY, enX, enY);

            if (avance.x == miX && avance.y == miY)
            {
                Debug.Log($"<color=yellow>[ML-BOT]</color> Enemigo {enemigoPelear.name} inalcanzable. Lista negra.");
                objetivosInalcanzables.Add(enemigoPelear);
                SubmitAction(Acciones.Moverse, miX, miY); 
                return;
            }

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
                if (objetivosInalcanzables.Contains(entidad)) continue;

                int ex = Mathf.RoundToInt(entidad.xPos);
                int ey = Mathf.RoundToInt(entidad.yPos);
                int dist = Mathf.Max(Mathf.Abs(origenX - ex), Mathf.Abs(origenY - ey));

                if (dist <= 3) enemigosCercanos++; 

                int score = (dist * 10) + entidad.hp; 
                
                // Ligera penalización si hay un muro, para que prefiera los de su habitación
                if (collisionChecker.HayMuroEnRuta(origenX, origenY, ex, ey)) score += 500;

                if (score < mejorPrioridad)
                {
                    mejorPrioridad = score;
                    mejorObjetivo = entidad;
                }
            }
        }
        return mejorObjetivo;
    }

    private PuertaMazmorra SeleccionarPuertaAleatoria()
    {
        if (gameManager.salaActual == null) return null;

        List<PuertaMazmorra> todas = gameManager.salaActual.ObtenerTodasLasPuertas();
        if (todas.Count == 0) return null;

        List<PuertaMazmorra> inexploradas = new List<PuertaMazmorra>();
        List<PuertaMazmorra> exploradas = new List<PuertaMazmorra>();

        foreach (var p in todas)
        {
            if (ML_Core.Instancia != null && !ML_Core.Instancia.salasVisitadas.Contains(p.idSalaDestino))
                inexploradas.Add(p);
            else 
                exploradas.Add(p);
        }
        
        if (inexploradas.Count > 0) return inexploradas[UnityEngine.Random.Range(0, inexploradas.Count)];
        if (exploradas.Count > 0) return exploradas[UnityEngine.Random.Range(0, exploradas.Count)];
        
        return null; 
    }

    // ALGORITMO GREEDY + PENALIZACION DE HISTORIAL
    // Se mueve directo al enemigo. Si se atasca, las casillas que pisa "huelen mal"
    // y empieza a rodear los muros naturalmente sin necesidad de A-Star.
    private Vector2Int CalcularPasoGreedy(int origenX, int origenY, int targetX, int targetY)
    {
        List<Vector2Int> validas = ObtenerCasillasValidasAlrededor(origenX, origenY, 1);
        
        if (validas.Count == 0) return new Vector2Int(origenX, origenY);

        Vector2Int mejorCasilla = validas[0];
        float mejorPuntuacion = float.MaxValue;

        // Desordenar para evitar decisiones deterministas en empates
        for (int i = 0; i < validas.Count; i++) {
            Vector2Int temp = validas[i];
            int randomIndex = UnityEngine.Random.Range(i, validas.Count);
            validas[i] = validas[randomIndex];
            validas[randomIndex] = temp;
        }

        foreach (var cas in validas)
        {
            // Puntuacion base: Distancia directa (ir hacia el objetivo)
            float puntuacion = Mathf.Max(Mathf.Abs(cas.x - targetX), Mathf.Abs(cas.y - targetY));

            // Penalización: Rastro de olor. Si ya pisaste aquí, es mala idea.
            int indexEnHistorial = historialPosiciones.LastIndexOf(cas);
            if (indexEnHistorial != -1)
            {
                int pasosAtras = historialPosiciones.Count - indexEnHistorial;
                puntuacion += (15.0f / pasosAtras); // Añade mucho peso a pasos recientes
            }

            if (puntuacion < mejorPuntuacion)
            {
                mejorPuntuacion = puntuacion;
                mejorCasilla = cas;
            }
        }

        return mejorCasilla;
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
                
                if (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)
                {
                    if (collisionChecker.HayMuroEnRuta(origenX, origenY, origenX + x, origenY) || 
                        collisionChecker.HayMuroEnRuta(origenX, origenY, origenX, origenY + y)) 
                    {
                        continue;
                    }
                }

                int cx = origenX + x;
                int cy = origenY + y;

                bool isDoorCurrent = false;
                bool isDoorNeighbor = false;
                
                if (gameManager.salaActual != null)
                {
                    isDoorCurrent = gameManager.salaActual.ObtenerPuerta(origenX, origenY) != null;
                    isDoorNeighbor = gameManager.salaActual.ObtenerPuerta(cx, cy) != null;
                }

                // Las puertas ignoran a los muros
                if (!isDoorCurrent && !isDoorNeighbor && collisionChecker.HayMuroEnRuta(origenX, origenY, cx, cy)) continue;
                
                if (gameManager.ObtenerEntidadEnCasilla(cx, cy) == null)
                {
                    lista.Add(new Vector2Int(cx, cy));
                }
            }
        }
        return lista;
    }
    #endregion
}