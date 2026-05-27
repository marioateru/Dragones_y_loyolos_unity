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

    // Colas tácticas. Mantenemos las intenciones temporalmente en RAM porque 
    // la base de datos es demasiado lenta para grabar el debate de quién ataca primero.
    private List<Entidad> entityQueue = new List<Entidad>();
    private List<AccionEnMemoria> actionQueue = new List<AccionEnMemoria>();

    private int timestepActual = 0;
    private int subTimestepActual = 0;
    private int indiceEntidadPensando = 0;

    void Start()
    {
        sqlManager = GetComponent<SQLManager>();
        
        // Retomamos la simulación desde el último punto registrado exacto 
        // para mantener la continuidad de la base de datos.
        timestepActual = Mathf.Max(1, sqlManager.ObtenerUltimoTimestep()); 
        Debug.Log($"[GameManager] Despertando mundo en el Timestep: {timestepActual}");

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
                // El control se cede a las entidades (IA o Jugador). El ciclo queda congelado.
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
        
        // Quien tenga mejores reflejos dicta el orden de declaración, por eso ordenamos por destreza.
        entityQueue.Sort((a, b) => b.destreza.CompareTo(a.destreza)); 

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
        Debug.Log("[GameManager] Todas las decisiones tomadas. Iniciando resolución de acciones...");
        
        // Re-ordenamos la ejecución real. Aquí podrían entrar en juego alteradores mágicos de prioridad.
        actionQueue.Sort((a, b) => b.prioridad.CompareTo(a.prioridad)); 

        // Se resetea el delta de turno para que nadie actúe de forma simultánea a nivel de base de datos.
        subTimestepActual = 0;

        foreach (var accion in actionQueue)
        {
            // Avanzamos el fotograma temporal antes de ejecutar para mantener un orden numérico estricto
            subTimestepActual++;
            accion.subTimestep = subTimestepActual;
            
            // Delegamos la lógica dura de la acción (mover transform, restar vida, etc) a la propia entidad.
            accion.entidad.EjecutarAccion(accion.tipoAccion, accion.objetivoX, accion.objetivoY);
        }

        estadoActual = GameState.GuardandoSQL;
    }

    private void GuardarYLimpiar()
    {
        sqlManager.GuardarHistorialDeAcciones(actionQueue);
        
        actionQueue.Clear();
        
        // Solo avanzamos el tiempo global cuando todo el mundo ha terminado de moverse y se ha volcado al disco.
        timestepActual++;
        
        estadoActual = GameState.PreparandoTurno;
    }
}