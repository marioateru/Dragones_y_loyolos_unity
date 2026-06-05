using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

// (Se asume que la clase AccionEnMemoria sigue existiendo aquí u en otro script, no la he borrado)
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
        Inicializando, PreparandoTurno, EsperandoEleccion, ProcesandoTurno, FinalizandoTurno 
    }
    private GameState estadoActual = GameState.Inicializando;

    [Header("Control de Cámara")]
    public CinemachineCamera camaraCinemachine;
    
    [Header("Control del Nivel")]
    public ControladorSala salaActual;
    
    [Header("Cascarones Data-Driven")]
    public Entidad prefabJugador; 
    public Entidad prefabEnemigoGenerico; 

    [Header("Rendimiento (Culling Global)")]
    [SerializeField] private int rangoHighPriority = 8;
    [SerializeField] private int rangoLowPriority = 15;

    [Header("Configuración de Guardado")]
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
        
        GenerarEntidadesDesdeSQL();
        estadoActual = GameState.PreparandoTurno;
    }

    void Update()
    {
        switch (estadoActual)
        {
            case GameState.PreparandoTurno:
                PrepararColas();
                break;
            case GameState.EsperandoEleccion:
                break;
            case GameState.ProcesandoTurno:
                ProcesarAcciones();
                break;
            case GameState.FinalizandoTurno:
                LimpiarYComprobarGuardado();
                break;
        }
    }

    private void GenerarEntidadesDesdeSQL()
    {
        entidadesEnMapa.Clear();

        if (salaActual == null || prefabJugador == null || prefabEnemigoGenerico == null)
        {
            Debug.LogError("[GameManager] Faltan referencias en el Inspector (Sala Actual o Cascarones).");
            return;
        }

        var listaSQL = sqlManager.ObtenerEntidadesEnSala(salaActual.idSalaActual, timestepActual);

        foreach (var entidadSQL in listaSQL)
        {
            bool esJugador = sqlManager.EsJugador(entidadSQL.id_entidades);

            // CORRECCIÓN: Si estamos cargando y el jugador ya existe (porque ha viajado de mapa), lo reutilizamos
            if (esJugador && jugadorPrincipal != null)
            {
                entidadesEnMapa.Add(jugadorPrincipal);
                continue; 
            }

            Entidad prefabAInstanciar = esJugador ? prefabJugador : prefabEnemigoGenerico;
            Entidad nuevoObj = Instantiate(prefabAInstanciar);
            nuevoObj.gameObject.name = esJugador ? "Jugador_" + entidadSQL.id_entidades : "Enemigo_" + entidadSQL.id_entidades;

            TileCollisionChecker checker = nuevoObj.GetComponent<TileCollisionChecker>();
            if (checker != null && salaActual.tilemapMuros != null)
            {
                checker.AsignarMuros(salaActual.tilemapMuros);
            }

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
                if (camaraCinemachine != null) camaraCinemachine.Follow = jugadorPrincipal.transform;
            }
        }
    }

    private void PrepararColas()
    {
        entityQueue.Clear();
        int pX = Mathf.RoundToInt(jugadorPrincipal.xPos);
        int pY = Mathf.RoundToInt(jugadorPrincipal.yPos);

        foreach (Entidad entidad in entidadesEnMapa)
        {
            if (entidad is EnemyComponent)
            {
                int dist = Mathf.Max(Mathf.Abs(Mathf.RoundToInt(entidad.xPos) - pX), Mathf.Abs(Mathf.RoundToInt(entidad.yPos) - pY));
                bool run = dist <= rangoLowPriority;
                bool highPrio = dist <= rangoHighPriority;
                
                entidad.EstablecerEstadoDeProcesamiento(run, highPrio);
                if (!run) continue; 
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
        PedirAccionASiguienteEntidad();
    }

    private void ProcesarAcciones()
    {
        actionQueue.Sort((a, b) => b.prioridad.CompareTo(a.prioridad)); 
        subTimestepActual = 0;

        foreach (var accion in actionQueue)
        {
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
        
        // CORRECCIÓN: Le pasamos el id de la sala actual para que deje de guardar las cosas en la "0"
        sqlManager.GuardarHistorialDeAcciones(historialPendiente, salaActual.idSalaActual);
        historialPendiente.Clear();
        Debug.Log($"[GameManager] Partida guardada en disco.");
    }

    // =================================================================
    // NUEVO SISTEMA DE VIAJE MODDEABLE (Carga de Mapas en Recursos)
    // =================================================================
    public void ViajarAUbicacion(Entidad jugador, int idSalaDestino, int destX, int destY)
    {
        GuardarPartidaEnDisco();
        sqlManager.GuardarEstadoMundoActual(entidadesEnMapa, salaActual.idSalaActual, timestepActual);

        // throw new System.NotImplementedException("El sistema de transición visual está pendiente.");

        foreach (var entidad in entidadesEnMapa)
        {
            if (entidad != jugador) Destroy(entidad.gameObject);
        }
        entidadesEnMapa.Clear();

        if (salaActual != null) Destroy(salaActual.gameObject);

        // Carga Dinámica orientada a Modding
        string rutaMapa = $"Mapas/Mazmorra_{idSalaDestino}";
        ControladorSala nuevaSalaPrefab = Resources.Load<ControladorSala>(rutaMapa);

        if (nuevaSalaPrefab == null)
        {
            Debug.LogError($"[GameManager] CRÍTICO: No se encontró el mapa en la carpeta 'Resources/{rutaMapa}'.");
            return;
        }

        salaActual = Instantiate(nuevaSalaPrefab);

        sqlManager.MoverEntidadASala(jugador.id_entidades, idSalaDestino, destX, destY, timestepActual);
        jugador.EjecutarAccion(Acciones.Moverse, destX, destY);

        TileCollisionChecker checker = jugador.GetComponent<TileCollisionChecker>();
        if (checker != null && salaActual.tilemapMuros != null) checker.AsignarMuros(salaActual.tilemapMuros);

        GenerarEntidadesDesdeSQL();
        
        estadoActual = GameState.PreparandoTurno;
    }
}