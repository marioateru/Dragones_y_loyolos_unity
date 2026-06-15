using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

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
    public enum GameState { 
        Inicializando, 
        PreparandoTurno, 
        EsperandoEleccion, 
        AvanzandoCola, 
        ProcesandoTurno, 
        FinalizandoTurno, 
        GameOver, 
        Pausa 
    }
    private GameState estadoActual = GameState.Inicializando;
    private GameState estadoPrevioPausa;
    [Header("Forzar modo ML")]
    public bool forzarModoML_EnEditor = false;

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
        
        idSalaInicial = sqlManager.ObtenerSalaDelJugador(timestepActual, idSalaInicial);
        
        CargarSalaInicial();
        GenerarEntidadesDesdeSQL();
        
        estadoActual = GameState.PreparandoTurno;
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame))
        {
            if (estadoActual != GameState.GameOver && estadoActual != GameState.Inicializando) 
            {
                InGameUIController ui = InGameUIController.Instancia != null 
                    ? InGameUIController.Instancia 
                    : FindFirstObjectByType<InGameUIController>(FindObjectsInactive.Include);
                
                if (ui != null)
                {
                    if (estadoActual == GameState.Pausa)
                    {
                        estadoActual = estadoPrevioPausa; 
                        ui.AlternarPausa(false);
                    }
                    else
                    {
                        estadoPrevioPausa = estadoActual; 
                        estadoActual = GameState.Pausa;   
                        ui.AlternarPausa(true);
                    }
                }
            }
        }

        if (estadoActual == GameState.GameOver || estadoActual == GameState.Pausa) 
        {
            return; 
        }

        int safeGuard = 0;
        bool procesandoLogica = true;
        
        // LIMITADOR INTELIGENTE DE OPERACIONES
        int limiteBucles = 100; // Normal

        if (ML_Core.IsMLMode && ML_Core.Instancia != null)
        {
            if (ML_Core.Instancia.operacionesPorSegundo <= 0)
            {
                limiteBucles = 500000; // Turbo
            }
            else
            {
                ML_Core.Instancia.acumuladorOperaciones += ML_Core.Instancia.operacionesPorSegundo * Time.deltaTime;
                limiteBucles = Mathf.FloorToInt(ML_Core.Instancia.acumuladorOperaciones);
                ML_Core.Instancia.acumuladorOperaciones -= limiteBucles;
            }
        }

        while (procesandoLogica && safeGuard < limiteBucles)
        {
            safeGuard++;
            switch (estadoActual)
            {
                case GameState.PreparandoTurno: PrepararColas(); break;
                case GameState.EsperandoEleccion: procesandoLogica = false; break; 
                case GameState.AvanzandoCola: PedirAccionASiguienteEntidad(); break;
                case GameState.ProcesandoTurno: ProcesarAcciones(); break;
                case GameState.FinalizandoTurno: LimpiarYComprobarGuardado(); break;
            }
        }

        if (jugadorPrincipal != null && jugadorPrincipal.IsDead())
        {
            if (ML_Core.IsMLMode)
            {
                ML_Core.Instancia.GestionarMuerteBot();
            }
            else
            {
                estadoActual = GameState.GameOver;
                GuardarPartidaEnDisco(); 
                InGameUIController.Instancia?.MostrarGameOver();
            }
        }
    }

    private void CargarSalaInicial()
    {
        ControladorSala mapaResidual = FindFirstObjectByType<ControladorSala>();
        if (mapaResidual != null) Destroy(mapaResidual.gameObject);

        string rutaMapa = $"Mapas/Mazmorra_{idSalaInicial}";
        ControladorSala prefabSala = Resources.Load<ControladorSala>(rutaMapa);

        if (prefabSala == null) return;
        salaActual = Instantiate(prefabSala, Vector3.zero, Quaternion.identity);
    }

    private void GenerarEntidadesDesdeSQL()
    {
        entidadesEnMapa.Clear();
        if (salaActual == null || prefabJugador == null || prefabEnemigoGenerico == null) return;

        var listaSQL = sqlManager.ObtenerEntidadesEnSala(salaActual.idSalaActual, timestepActual);
        PlayerInputController inputController = FindFirstObjectByType<PlayerInputController>();

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
            nuevoObj.accionesPermitidas = sqlManager.ObtenerAccionesPermitidas(entidadSQL.id_entidades);

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
                if (inputController != null) inputController.jugador = jugadorPrincipal;

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

        indiceEntidadPensando = 0;
        PedirAccionASiguienteEntidad();
    }

    private void PedirAccionASiguienteEntidad()
    {
        if (indiceEntidadPensando < entityQueue.Count)
        {
            Entidad actorTurno = entityQueue[indiceEntidadPensando];
            estadoActual = GameState.EsperandoEleccion;
            actorTurno.ChooseAction();
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
            if (salaActual == null) break;

            Entidad actor = accion.entidad;
            if (actor.IsDead()) continue;

            subTimestepActual++;
            accion.subTimestep = subTimestepActual;

            if (accion.tipoAccion == Acciones.Moverse)
            {
                PuertaMazmorra puerta = salaActual.ObtenerPuerta(accion.objetivoX, accion.objetivoY);
                if (puerta != null && actor == jugadorPrincipal)
                {
                    actor.EjecutarAccion(Acciones.Moverse, accion.objetivoX, accion.objetivoY);
                    ViajarAUbicacion(actor, puerta.idSalaDestino, puerta.destinoX, puerta.destinoY);
                    break;
                }
            }
            actor.EjecutarAccion(accion.tipoAccion, accion.objetivoX, accion.objetivoY);
        }
        
        if (jugadorPrincipal != null && jugadorPrincipal.IsDead())
        {
            estadoActual = GameState.GameOver;
            GuardarPartidaEnDisco();
            InGameUIController.Instancia?.MostrarGameOver();
            return;
        }
        
        if (salaActual != null) estadoActual = GameState.FinalizandoTurno;
    }

    private void LimpiarYComprobarGuardado()
    {
        historialPendiente.AddRange(actionQueue);
        actionQueue.Clear();
        timestepActual++;

        if (!ML_Core.IsMLMode && timestepActual % turnosParaAutoguardado == 0) GuardarPartidaEnDisco();

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
        salaActual = Instantiate(nuevaSalaPrefab, Vector3.zero, Quaternion.identity);

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
                return entidad;
        }
        return null;
    }

    public List<Entidad> ObtenerTodasLasEntidades()
    {
        return entidadesEnMapa;
    }

    public void RecargarPartidaDesdeTimestep(int targetTimestep)
    {
        sqlManager.RollbackATimestep(targetTimestep);
        this.timestepActual = targetTimestep;
        this.subTimestepActual = 0;

        actionQueue.Clear();
        entityQueue.Clear();
        historialPendiente.Clear();

        foreach (var entidad in entidadesEnMapa) { if (entidad != null) Destroy(entidad.gameObject); }
        entidadesEnMapa.Clear();

        jugadorPrincipal = null; 

        if (salaActual != null) Destroy(salaActual.gameObject);

        idSalaInicial = sqlManager.ObtenerSalaDelJugador(timestepActual, idSalaInicial);
        CargarSalaInicial();
        GenerarEntidadesDesdeSQL();

        estadoActual = GameState.PreparandoTurno;
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || jugadorPrincipal == null) return;

        float tamanoHigh = (rangoHighPriority * 2) + 1; 
        float tamanoLow = (rangoLowPriority * 2) + 1; 

        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireCube(jugadorPrincipal.transform.position, new Vector3(tamanoHigh, tamanoHigh, 0f));
        
        Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
        Gizmos.DrawWireCube(jugadorPrincipal.transform.position, new Vector3(tamanoLow, tamanoLow, 0f));
    }
}