using System.Collections.Generic;
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

// WARNING: Este código comprende responsabilidades como gráficos, inaceptable. REFACTORIZAR.
public class GameManager : MonoBehaviour
{
    private enum GameState { 
        Inicializando, PreparandoTurno, EsperandoEleccion, ProcesandoTurno, FinalizandoTurno 
    }
    private GameState estadoActual = GameState.Inicializando;

    private SQLManager sqlManager;
    
    [Header("Control del Nivel")]
    public ControladorSala salaActual;
    
    [Header("Cascarones Data-Driven")]
    [Tooltip("Prefab genérico que tiene PlayerComponent")]
    public Entidad prefabJugador; 
    [Tooltip("Prefab genérico vacío que tiene EnemyComponent")]
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
        var listaSQL = sqlManager.ObtenerEntidadesEnSala(salaActual.idSalaActual, timestepActual);

        foreach (var entidadSQL in listaSQL)
        {
            bool esJugador = sqlManager.EsJugador(entidadSQL.id_entidades);
            Entidad prefabAInstanciar = esJugador ? prefabJugador : prefabEnemigoGenerico;

            Entidad nuevoObj = Instantiate(prefabAInstanciar);
            
            // Extraemos el nombre de la BD para usarlo de ID visual ("Heroe_Principal", "Orco Furioso"...)
            string nombreVisual = sqlManager.ObtenerNombreEntidad(entidadSQL.id_entidades, esJugador);
            nuevoObj.gameObject.name = nombreVisual + "_" + entidadSQL.id_entidades;

            TileCollisionChecker checker = nuevoObj.GetComponent<TileCollisionChecker>();
            if (checker != null && salaActual.tilemapMuros != null)
            {
                checker.AsignarMuros(salaActual.tilemapMuros);
            }

            // Inyectamos el alma (Stats, vida, etc)
            sqlManager.CargarDatosDeEntidad(nuevoObj, entidadSQL.id_entidades, timestepActual);
            
            // Inyectamos la apariencia (El AnimatorOverrideController)
            nuevoObj.CargarVisuales(nombreVisual); 
            
            entidadesEnMapa.Add(nuevoObj);

            if (esJugador) jugadorPrincipal = (PlayerComponent)nuevoObj;
        }
    }

    private void PrepararColas()
    {
        entityQueue.Clear();
        
        int pX = Mathf.RoundToInt(jugadorPrincipal.xPos);
        int pY = Mathf.RoundToInt(jugadorPrincipal.yPos);

        foreach (Entidad entidad in entidadesEnMapa)
        {
            // Culling Global Centralizado
            if (entidad is EnemyComponent)
            {
                // Distancia Matemática Pura de Grid (Chebyshev)
                int dist = Mathf.Max(Mathf.Abs(Mathf.RoundToInt(entidad.xPos) - pX), Mathf.Abs(Mathf.RoundToInt(entidad.yPos) - pY));
                
                bool run = dist <= rangoLowPriority;
                bool highPrio = dist <= rangoHighPriority;
                
                entidad.EstablecerEstadoDeProcesamiento(run, highPrio);

                // Si está fuera del rango de simulación, no contamina la cola
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
        sqlManager.GuardarHistorialDeAcciones(historialPendiente);
        historialPendiente.Clear();
        Debug.Log($"[GameManager] Partida guardada en disco.");
    }
}