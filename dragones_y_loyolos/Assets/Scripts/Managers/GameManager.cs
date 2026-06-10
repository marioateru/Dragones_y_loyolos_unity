using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class AccionEnMemoria {
    public int timestep;
    public int subTimestep;
    public Entidad entidad;
    public Acciones tipoAccion;
    public int objetivoX;
    public int objetivoY;
    public int prioridad; 
}

public class GameManager : MonoBehaviour
{
    private enum GameState { 
        Inicializando, PreparandoTurno, EsperandoEleccion, AvanzandoCola, ProcesandoTurno, FinalizandoTurno 
    }
    private GameState estadoActual = GameState.Inicializando;

    [Header("Sala inicial")]
    public int idSalaInicial = 0;

    [Header("Cámara")]
    public CinemachineCamera camaraCinemachine;
    
    [HideInInspector]
    public ControladorSala salaActual;
    
    [Header("Prefabs entidades")]
    public Entidad prefabJugador; 
    public Entidad prefabEnemigoGenerico; 

    [Header("Distancias de detección de enemigos")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private int rangoHighPriority = 8;
    [SerializeField] private int rangoLowPriority = 15;

    [Header("Guardado")]
    public int turnosParaAutoguardado = 10;

    private List<Entidad> entidadesEnMapa = new List<Entidad>();
    private List<Entidad> entityQueue = new List<Entidad>();
    private List<AccionEnMemoria> actionQueue = new List<AccionEnMemoria>();
    private List<AccionEnMemoria> historialPendiente = new List<AccionEnMemoria>();

    private SQLManager sqlManager;
    private int timestepActual = 0;
    private int subTimestepActual = 0;
    private int indiceEntidadPensando = 0;

    private PlayerComponent jugadorPrincipal;

    void Start()
    {
        sqlManager = GetComponent<SQLManager>();
        timestepActual = Mathf.Max(1, sqlManager.ObtenerUltimoTimestep()); 
        
        CargarSalaInicial();
        GenerarEntidadesDesdeSQL();
        
        estadoActual = GameState.PreparandoTurno;
    }

    void Update()
    {
        switch (estadoActual)
        {
            case GameState.PreparandoTurno: PrepararColas(); break;
            case GameState.EsperandoEleccion: break;
            case GameState.AvanzandoCola: PedirAccionASiguienteEntidad(); break;
            case GameState.ProcesandoTurno: ProcesarAcciones(); break;
            case GameState.FinalizandoTurno: LimpiarYComprobarGuardado(); break;
        }
    }

    private void CargarSalaInicial()
    {
        ControladorSala mapaResidual = FindFirstObjectByType<ControladorSala>();
        if (mapaResidual != null) Destroy(mapaResidual.gameObject);

        string rutaMapa = $"Mapas/Mazmorra_{idSalaInicial}";
        ControladorSala prefabSala = Resources.Load<ControladorSala>(rutaMapa);

        if (prefabSala == null) return;
        salaActual = Instantiate(prefabSala, new Vector3(0, 1, 0), Quaternion.identity);
    }

    private void GenerarEntidadesDesdeSQL()
    {
        entidadesEnMapa.Clear();

        if (salaActual == null || prefabJugador == null || prefabEnemigoGenerico == null) return;

        var listaSQL = sqlManager.ObtenerEntidadesEnSala(salaActual.idSalaActual, timestepActual);

        foreach (var entidadSQL in listaSQL)
        {
            bool esJugador = sqlManager.EsJugador(entidadSQL.id_entidades);

            if (esJugador && jugadorPrincipal != null)
            {
                entidadesEnMapa.Add(jugadorPrincipal);
                continue; 
            }

            Entidad prefabAInstanciar = esJugador ? prefabJugador : prefabEnemigoGenerico;
            Entidad nuevoObj = Instantiate(prefabAInstanciar);
            nuevoObj.gameObject.name = esJugador ? "Jugador_" + entidadSQL.id_entidades : "Enemigo_" + entidadSQL.id_entidades;

            TileCollisionChecker checker = nuevoObj.GetComponent<TileCollisionChecker>();
            if (checker != null && salaActual.tilemapMuros != null) checker.AsignarMuros(salaActual.tilemapMuros);

            sqlManager.CargarDatosDeEntidad(nuevoObj, entidadSQL.id_entidades, timestepActual);
            
            ComponenteVisual visuales = nuevoObj.GetComponent<ComponenteVisual>();
            if (visuales != null)
            {
                string nombreVisual = sqlManager.ObtenerNombreEntidad(entidadSQL.id_entidades, esJugador);
                visuales.InicializarVisuales(nombreVisual);
            }
            
            entidadesEnMapa.Add(nuevoObj);

            if (esJugador) 
            {
                jugadorPrincipal = (PlayerComponent)nuevoObj;
                if (camaraCinemachine != null) 
                {
                    camaraCinemachine.Follow = jugadorPrincipal.transform;
                    camaraCinemachine.PreviousStateIsValid = false;
                    camaraCinemachine.transform.position = new Vector3(jugadorPrincipal.transform.position.x, jugadorPrincipal.transform.position.y, camaraCinemachine.transform.position.z);
                }
            }
        }
    }

    private void PrepararColas()
    {
        entityQueue.Clear();
        int pX = 0, pY = 0;

        if (jugadorPrincipal != null)
        {
            pX = Mathf.RoundToInt(jugadorPrincipal.xPos);
            pY = Mathf.RoundToInt(jugadorPrincipal.yPos);
        }

        foreach (Entidad entidad in entidadesEnMapa)
        {
            if (entidad is EnemyComponent)
            {
                int distancia = Mathf.Max(Mathf.Abs(Mathf.RoundToInt(entidad.xPos) - pX), Mathf.Abs(Mathf.RoundToInt(entidad.yPos) - pY));
                
                bool isRun = distancia <= rangoLowPriority;
                bool isHighPriority = distancia <= rangoHighPriority;
                
                entidad.EstablecerEstadoDeProcesamiento(isRun, isHighPriority);
                if (!isRun) continue; 
            }
            entityQueue.Add(entidad);
        }
        
        entityQueue.Sort((a, b) => 
        {
            if (a.isHighPriority && !b.isHighPriority) return -1;
            if (!a.isHighPriority && b.isHighPriority) return 1;  
            return b.destreza.CompareTo(a.destreza);
        }); 

        indiceEntidadPensando = 0;
        PedirAccionASiguienteEntidad();
    }

    private void PedirAccionASiguienteEntidad()
    {
        if (indiceEntidadPensando < entityQueue.Count)
        {
            estadoActual = GameState.EsperandoEleccion;
            entityQueue[indiceEntidadPensando].ChooseAction();
        }
        else
        {
            estadoActual = GameState.ProcesandoTurno;
        }
    }

    public void RegistrarEleccion(Entidad actor, Acciones accion, int objX, int objY)
    {
        actionQueue.Add(new AccionEnMemoria
        {
            timestep = timestepActual, subTimestep = 0, entidad = actor, tipoAccion = accion,
            objetivoX = objX, objetivoY = objY, prioridad = actor.destreza 
        });

        indiceEntidadPensando++;
        estadoActual = GameState.AvanzandoCola; 
    }

    private void ProcesarAcciones()
    {
        actionQueue.Sort((a, b) => b.prioridad.CompareTo(a.prioridad)); 
        subTimestepActual = 0;

        foreach (var accion in actionQueue)
        {
            // Si el jugador no está en la sala, computar el turno es redundante.
            if (salaActual == null) break;

            subTimestepActual++;
            accion.subTimestep = subTimestepActual;
            accion.entidad.EjecutarAccion(accion.tipoAccion, accion.objetivoX, accion.objetivoY);
        }
        estadoActual = GameState.FinalizandoTurno;
    }

    private void LimpiarYComprobarGuardado()
    {
        historialPendiente.AddRange(actionQueue);
        actionQueue.Clear();
        timestepActual++;

        if (timestepActual % turnosParaAutoguardado == 0) GuardarPartidaEnDisco();

        estadoActual = GameState.PreparandoTurno;
    }

    public void GuardarPartidaEnDisco()
    {
        if (historialPendiente.Count == 0) return;
        
        sqlManager.GuardarHistorialDeAcciones(historialPendiente, salaActual.idSalaActual);
        historialPendiente.Clear();
    }

    public void ViajarAUbicacion(Entidad jugador, int idSalaDestino, int destX, int destY)
    {
        actionQueue.Clear();
        entityQueue.Clear();

        GuardarPartidaEnDisco();
        sqlManager.GuardarEstadoMundoActual(entidadesEnMapa, salaActual.idSalaActual, timestepActual);

        foreach (var entidad in entidadesEnMapa)
        {
            if (entidad != jugador) Destroy(entidad.gameObject);
        }
        entidadesEnMapa.Clear();

        if (salaActual != null) Destroy(salaActual.gameObject);

        string rutaMapa = $"Mapas/Mazmorra_{idSalaDestino}";
        ControladorSala nuevaSalaPrefab = Resources.Load<ControladorSala>(rutaMapa);

        if (nuevaSalaPrefab == null) return;

        salaActual = Instantiate(nuevaSalaPrefab, new Vector3(0, 1, 0), Quaternion.identity);

        sqlManager.MoverEntidadASala(jugador.id_entidades, idSalaDestino, destX, destY, timestepActual);
        jugador.EjecutarAccion(Acciones.Moverse, destX, destY);

        TileCollisionChecker checker = jugador.GetComponent<TileCollisionChecker>();
        if (checker != null && salaActual.tilemapMuros != null) checker.AsignarMuros(salaActual.tilemapMuros);

        if (camaraCinemachine != null)
        {
            camaraCinemachine.PreviousStateIsValid = false;
            camaraCinemachine.transform.position = new Vector3(jugador.transform.position.x, jugador.transform.position.y, camaraCinemachine.transform.position.z);
        }

        GenerarEntidadesDesdeSQL();
        estadoActual = GameState.PreparandoTurno;
    }

    public Entidad ObtenerEntidadEnCasilla(int x, int y)
    {
        foreach (Entidad entidad in entidadesEnMapa)
        {
            if (Mathf.RoundToInt(entidad.xPos) == x && Mathf.RoundToInt(entidad.yPos) == y && !entidad.IsDead())
            {
                return entidad;
            }
        }
        return null;
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || jugadorPrincipal == null) return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // Rojo translúcido
        Gizmos.DrawWireSphere(jugadorPrincipal.transform.position, rangoHighPriority);
        Gizmos.DrawSphere(jugadorPrincipal.transform.position, 0.1f);

        Gizmos.color = new Color(1f, 1f, 0f, 0.15f); // Amarillo muy translúcido
        Gizmos.DrawWireSphere(jugadorPrincipal.transform.position, rangoLowPriority);
    }
}