using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(TileCollisionChecker))]
public class PlayerComponent : Entidad
{
    
    [Header("Parámetros modo ML.")]
    [SerializeField] private TileCollisionChecker collisionChecker;
    [SerializeField] private SQLManager sqlManager;
    [SerializeField] private int pasosSinEnemigo = 0;
    [SerializeField] private float porcentajeVidaBaja = 0.4f;
    [SerializeField] private int areaCasillasDeHuidaBFS = 15;
    private bool esMiTurno = false;
    private bool modoRetirada = false;
    private PuertaMazmorra puertaObjetivo = null;
    private Entidad enemigoObjetivo = null;
    
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
        
        int miXPos = Mathf.RoundToInt(xPos);
        int miYPos = Mathf.RoundToInt(yPos);
        
        if (gameManager.salaActual != null)
        {
            ML_Core.Instancia.salasVisitadas.Add(gameManager.salaActual.idSalaActual);
        }

        int vidaMax = sqlManager.ObtenerVidaMaximaDeEntidad(this.id_entidades); 
        
        if (hp <= vidaMax * porcentajeVidaBaja) modoRetirada = true;
        if (hp >= vidaMax) modoRetirada = false;

        int enemigosCercanos;
        Entidad enemigoPelear = EscanearMejorEnemigoGlobal(miXPos, miYPos, out enemigosCercanos); 

        if (enemigoPelear == null)
        {
            if (modoRetirada && hp < vidaMax)
            {
                debugRutaActual.Clear();
                ML_Core.Instancia?.RegistrarOperacionIA();
                SubmitAction(Acciones.Consumir, miXPos, miYPos);
                return;
            }

            enemigoObjetivo = null;

            PuertaMazmorra puertaDestino = SeleccionarMejorPuerta(miXPos, miYPos);
            if (puertaDestino != null)
            {
                int pX = puertaDestino.xPos;
                int pY = puertaDestino.yPos;
                
                if (Mathf.Max(Mathf.Abs(miXPos - pX), Mathf.Abs(miYPos - pY)) <= 1)
                {
                    debugRutaActual.Clear();
                    ML_Core.Instancia?.RegistrarOperacionIA();
                    SubmitAction(Acciones.Moverse, pX, pY);
                    puertaObjetivo = null;
                    return;
                }
                
                List<Vector2Int> ruta = CalcularRutaAStar(miXPos, miYPos, pX, pY);
                ML_Core.Instancia?.RegistrarOperacionIA();
                EjecutarAvanceEnRuta(ruta);
                return;
            }
            else
            {
                Debug.Log("<color=green><b>[ML-BOT] No se han detectado más puertas viables que explorar.</b> Reiniciando simulación.</color>");
                if (ML_Core.Instancia != null) ML_Core.Instancia.GestionarMuerteBot(); 
                else SubmitAction(Acciones.Moverse, miXPos, miYPos); 
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
                SubmitAction(Acciones.Consumir, miXPos, miYPos);
                return;
            }

            Vector2Int casillaEscape = CalcularCasillaHuida(miXPos, miYPos, enemigoPelear); 
            List<Vector2Int> rutaEscape = CalcularRutaAStar(miXPos, miYPos, casillaEscape.x, casillaEscape.y);
            ML_Core.Instancia?.RegistrarOperacionIA();
            EjecutarAvanceEnRuta(rutaEscape);
            return;
        }

        int enemigoXPos = Mathf.RoundToInt(enemigoPelear.xPos);
        int enemigoYPos = Mathf.RoundToInt(enemigoPelear.yPos);
        int distanciaEnemigo = Mathf.Max(Mathf.Abs(miXPos - enemigoXPos), Mathf.Abs(miYPos - enemigoYPos));

        if (distanciaEnemigo <= 1)
        {
            debugRutaActual.Clear();
            ML_Core.Instancia?.RegistrarOperacionIA();
            SubmitAction(Acciones.Atacar, enemigoXPos, enemigoYPos);
        }
        else if (enemigosCercanos >= 2 && distanciaEnemigo <= 4)
        {
            Vector2Int casillaFlanqueo = CalcularCasillaFlanqueo(miXPos, miYPos, enemigoXPos, enemigoYPos);
            List<Vector2Int> rutaFlanqueo = CalcularRutaAStar(miXPos, miYPos, casillaFlanqueo.x, casillaFlanqueo.y);
            ML_Core.Instancia?.RegistrarOperacionIA();
            EjecutarAvanceEnRuta(rutaFlanqueo);
        }
        else
        {
            List<Vector2Int> ruta = CalcularRutaAStar(miXPos, miYPos, enemigoXPos, enemigoYPos);
            ML_Core.Instancia?.RegistrarOperacionIA();
            EjecutarAvanceEnRuta(ruta);
        }
    }

    private void EjecutarAvanceEnRuta(List<Vector2Int> rutaActual)
    {
        debugRutaActual = rutaActual;

        if (rutaActual == null || rutaActual.Count == 0) 
        {
            Vector2Int casillaAleatoria = CalcularCasillaAleatoria(Mathf.RoundToInt(xPos), Mathf.RoundToInt(yPos));
            EjecutarPasoSimple(casillaAleatoria.x, casillaAleatoria.y);
            return;
        }

        int miXPos = Mathf.RoundToInt(xPos);
        int miYPos = Mathf.RoundToInt(yPos);

        int maxVelocidad = Mathf.Max(1, velocidad);
        int pasosAleatorios = UnityEngine.Random.Range(1, maxVelocidad + 1);
        int maxPasos = Mathf.Min(rutaActual.Count, pasosAleatorios);
        
        Vector2Int mejorSalto = rutaActual[0];
        
        for (int i = maxPasos - 1; i >= 0; i--)
        {
            Vector2Int nodo = rutaActual[i];
            
            if (i > 0 && gameManager.ObtenerEntidadEnCasilla(nodo.x, nodo.y) != null) continue;

            if (!collisionChecker.HayMuroEnRuta(miXPos, miYPos, nodo.x, nodo.y))
            {
                mejorSalto = nodo;
                break;
            }
        }

        EjecutarPasoSimple(mejorSalto.x, mejorSalto.y);
    }

    private void EjecutarPasoSimple(int xPos, int yPos)
    {
        Entidad obstaculo = gameManager.ObtenerEntidadEnCasilla(xPos, yPos);
        if (obstaculo != null && obstaculo != this)
            SubmitAction(Acciones.Atacar, xPos, yPos);
        else
            SubmitAction(Acciones.Moverse, xPos, yPos);
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
                int entidadXPos = Mathf.RoundToInt(entidad.xPos);
                int entidadYPos = Mathf.RoundToInt(entidad.yPos);
                int distanciaEntidad = Mathf.Max(Mathf.Abs(origenX - entidadXPos), Mathf.Abs(origenY - entidadYPos));

                if (distanciaEntidad <= 3) enemigosCercanos++; 

                int score = (distanciaEntidad * 10) + entidad.hp; 
                if (collisionChecker.HayMuroEnRuta(origenX, origenY, entidadXPos, entidadYPos)) score += 100;

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
                float dist = Mathf.Max(Mathf.Abs(miCoordX - puerta.xPos), Mathf.Abs(miCoordY - puerta.yPos));
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
        List<Vector2Int> casillasCandidatas = ObtenerCasillasValidasAlrededor(origenX, origenY, 1);
        if (casillasCandidatas.Count == 0) return CalcularCasillaAleatoria(origenX, origenY);

        Vector2Int mejorCasilla = new Vector2Int(origenX, origenY);
        int mejorPuntuacion = -9999;

        foreach (var casilla in casillasCandidatas)
        {
            int distObjetivo = Mathf.Max(Mathf.Abs(casilla.x - objetivoX), Mathf.Abs(casilla.y - objetivoY));
            int distOtrosEnemigos = 0;

            foreach (var enemigo in gameManager.ObtenerTodasLasEntidades())
            {
                if (enemigo is EnemyComponent && !enemigo.IsDead() && Mathf.RoundToInt(enemigo.xPos) != objetivoX)
                {
                    distOtrosEnemigos += Mathf.Max(Mathf.Abs(casilla.x - Mathf.RoundToInt(enemigo.xPos)), Mathf.Abs(casilla.y - Mathf.RoundToInt(enemigo.yPos)));
                }
            }

            int puntuacion = distOtrosEnemigos - (distObjetivo * 3);
            if (puntuacion > mejorPuntuacion)
            {
                mejorPuntuacion = puntuacion;
                mejorCasilla = casilla;
            }
        }
        return mejorCasilla;
    }

    private List<Vector2Int> CalcularRutaAStar(int startX, int startY, int targetX, int targetY)
    {
        return Pathfinding.GetAStarPath(new Vector2Int(startX, startY), new Vector2Int(targetX, targetY), collisionChecker, gameManager);
    }

    private List<Vector2Int> GetReachableTiles(int startX, int startY, int maxPasos)
    {
        return Pathfinding.GetBFSReachable(new Vector2Int(startX, startY), maxPasos, collisionChecker);
    }

    private Vector2Int CalcularCasillaHuida(int origenX, int origenY, Entidad enemigoMasCercano)
    {
        int objetivoXPos = Mathf.RoundToInt(enemigoMasCercano.xPos);
        int objetivoYPos = Mathf.RoundToInt(enemigoMasCercano.yPos);

        List<Vector2Int> casillasCandidatas = GetReachableTiles(origenX, origenY, areaCasillasDeHuidaBFS);
        
        Vector2Int mejorCasilla = new Vector2Int(origenX, origenY);
        int mejorPuntuacion = -999999;
        int minPuntuacion = 999999;

        Dictionary<Vector2Int, int> puntuacionesBrutas = new Dictionary<Vector2Int, int>();

        foreach(var cas in casillasCandidatas)
        {
            int distEnemigo = Mathf.Max(Mathf.Abs(cas.x - objetivoXPos), Mathf.Abs(cas.y - objetivoYPos));
            
            int murosAdyacentes = 0;
            if (collisionChecker.HayMuroEnRuta(cas.x, cas.y, cas.x+1, cas.y)) murosAdyacentes++;
            if (collisionChecker.HayMuroEnRuta(cas.x, cas.y, cas.x-1, cas.y)) murosAdyacentes++;
            if (collisionChecker.HayMuroEnRuta(cas.x, cas.y, cas.x, cas.y+1)) murosAdyacentes++;
            if (collisionChecker.HayMuroEnRuta(cas.x, cas.y, cas.x, cas.y-1)) murosAdyacentes++;

            int puntuacion = (distEnemigo * 10) + (murosAdyacentes * 50); 
            puntuacionesBrutas[cas] = puntuacion;
            
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

        foreach(var keyValuePair in puntuacionesBrutas)
        {
            float valorNormalizado = 0f;
            if (mejorPuntuacion > minPuntuacion)
            {
                valorNormalizado = (float)(keyValuePair.Value - minPuntuacion) / (mejorPuntuacion - minPuntuacion);
            }
            debugScoresBFS[keyValuePair.Key] = valorNormalizado;
        }

        debugMejorCasillaBFS = mejorCasilla;
        return mejorCasilla;
    }

    private Vector2Int CalcularCasillaAleatoria(int origenX, int origenY)
    {
        return Pathfinding.GetRandomValidTile(new Vector2Int(origenX, origenY), collisionChecker, gameManager);
    }

    private List<Vector2Int> ObtenerCasillasValidasAlrededor(int origenX, int origenY, int rango)
    {
        return Pathfinding.GetValidAdjacent(new Vector2Int(origenX, origenY), rango, collisionChecker, gameManager);
    }
    
    private void OnDrawGizmos()
    {
        if (debugScoresBFS != null && debugScoresBFS.Count > 0)
        {
            foreach (var keyValuePair in debugScoresBFS)
            {
                Vector3 waypointPosition = new Vector3(keyValuePair.Key.x + 0.5f, -keyValuePair.Key.y - 0.5f, 0f);
                
                if (keyValuePair.Key == debugMejorCasillaBFS)
                {
                    Gizmos.color = new Color(0f, 1f, 0f, 0.85f); 
                    Gizmos.DrawCube(waypointPosition, new Vector3(0.9f, 0.9f, 0.1f));
                }
                else
                {
                    Gizmos.color = Color.Lerp(new Color(1f, 0f, 0f, 0.35f), new Color(0.5f, 1f, 0f, 0.35f), keyValuePair.Value);
                    Gizmos.DrawCube(waypointPosition, new Vector3(0.8f, 0.8f, 0.1f));
                }
            }
        }

        if (debugRutaActual == null || debugRutaActual.Count == 0) return;

        Gizmos.color = Color.cyan;
        Vector3 previousPos = new Vector3(xPos + 0.5f, -yPos - 0.5f, 0f);
        
        foreach (var waypoint in debugRutaActual)
        {
            Vector3 waypointPosition = new Vector3(waypoint.x + 0.5f, -waypoint.y - 0.5f, 0f);
            Gizmos.DrawSphere(waypointPosition, 0.2f);
            Gizmos.DrawLine(previousPos, waypointPosition);
            previousPos = waypointPosition;
        }
    }
    #endregion
}