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

public class GameManager : MonoBehaviour
{
    private enum GameState { 
        Inicializando, 
        PreparandoTurno, 
        EsperandoEleccion, 
        ProcesandoTurno, 
        GuardandoSQL 
    }
    private GameState estadoActual = GameState.Inicializando;

    private SQLManager sqlManager;
    public List<Entidad> entidadesEnMapa;
    private List<Entidad> entityQueue = new List<Entidad>();
    private List<AccionEnMemoria> actionQueue = new List<AccionEnMemoria>();

    private int timestepActual = 0;
    private int subTimestepActual = 0;
    private int indiceEntidadPensando = 0;

    void Start()
    {
        sqlManager = GetComponent<SQLManager>();
        
        // Continuamos desde donde lo dejamos
        timestepActual = Mathf.Max(1, sqlManager.ObtenerUltimoTimestep()); 
        Debug.Log($"[GameManager] Iniciando mundo en el Timestep: {timestepActual}");

        foreach (var entidad in entidadesEnMapa) {
            sqlManager.CargarDatosDeEntidad(entidad, entidad.id_entidades, timestepActual);
        }

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
                // Esperamos a que los enemigos y el jugador escojan acción
                break;
            case GameState.ProcesandoTurno:
                ProcesarAcciones();
                break;
            case GameState.GuardandoSQL:
                GuardarYLimpiar();
                break;
        }
    }

    private void PrepararColas()
    {
        entityQueue.Clear();
        entityQueue.AddRange(entidadesEnMapa);
        
        // Ordenamos por destreza
        entityQueue.Sort((a, b) => b.destreza.CompareTo(a.destreza)); 

        indiceEntidadPensando = 0;
        PedirAccionASiguienteEntidad();
    }

    private void PedirAccionASiguienteEntidad()
    {
        // Es básicamente un while pero no explícito
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
            timestep = timestepActual,
            subTimestep = 0, 
            entidad = actor,
            tipoAccion = accion,
            objetivoX = objX,
            objetivoY = objY,
            prioridad = actor.destreza 
        });

        indiceEntidadPensando++;
        PedirAccionASiguienteEntidad();
    }

    private void ProcesarAcciones()
    {
        Debug.Log("[GameManager] Iniciando resolución de acciones...");
        
        // Ordenamos acciones por prioridad
        actionQueue.Sort((a, b) => b.prioridad.CompareTo(a.prioridad)); 

        subTimestepActual = 0;

        foreach (var accion in actionQueue)
        {
            subTimestepActual++;
            accion.subTimestep = subTimestepActual;
            
            accion.entidad.EjecutarAccion(accion.tipoAccion, accion.objetivoX, accion.objetivoY);
        }

        estadoActual = GameState.GuardandoSQL;
    }

    // Cambiar para volcar en disco en salida de partida, no al finalizar cada turno.
    private void GuardarYLimpiar()
    {
        sqlManager.GuardarHistorialDeAcciones(actionQueue);
        
        actionQueue.Clear();
        
        timestepActual++;
        
        estadoActual = GameState.PreparandoTurno;
    }
}