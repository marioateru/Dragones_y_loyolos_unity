using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class Entidad : MonoBehaviour
{
    private const float TILE_CENTER_OFFSET = 0.5f;
    [Header("Claves SQL")]
    [field: SerializeField] public int id_entidades { get; private set; } 
    [field: SerializeField] public int id_stats_base { get; private set; } 

    [Header("Estadísticas Base")]
    [field: SerializeField] public int hp { get; set; }
    [field: SerializeField] public int ac { get; private set; }
    [field: SerializeField] public int fuerza { get; private set; }
    [field: SerializeField] public int destreza { get; private set; }
    [field: SerializeField] public int constitucion { get; private set; }
    [field: SerializeField] public int inteligencia { get; private set; }
    [field: SerializeField] public int sabiduria { get; private set; }
    [field: SerializeField] public int carisma { get; private set; }
    [field: SerializeField] public int velocidad { get; protected set; } 

    [Header("Posición Tilemap")]
    [field: SerializeField] public float xPos { get; protected set; }
    [field: SerializeField] public float yPos { get; protected set; }
    public bool EstaDefendido { get; protected set; } = false;
    public int MaxHpT0 { get; private set; }
    public bool IsRun { get; protected set; }
    public bool IsHighPriority { get; protected set; }
    public List<Acciones> accionesPermitidas = new List<Acciones>();
    protected GameManager gameManager;

    public static event EventHandler onEntityCreated;
    public event EventHandler<onStatsChangedByActionArgs> onStatsChangedByAction;

    public class onStatsChangedByActionArgs : EventArgs
    {
        public Entidad entidad; 
    }

    public virtual void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public bool IsDead() => hp <= 0;

    public void EstablecerEstadoDeProcesamiento(bool run, bool highPrio)
    {
        this.IsRun = run;
        this.IsHighPriority = highPrio;
    }

    public void InicializarDatosSQL(int idEnt, int idStats, StatsBaseEntidadesSQL estadoStats, int xInicial, int yInicial, int hpMaximoT0)
    {
        this.id_entidades = idEnt;
        this.id_stats_base = idStats;
        this.MaxHpT0 = hpMaximoT0;
        this.hp = estadoStats.hp;
        this.ac = estadoStats.ac;
        this.fuerza = estadoStats.fuerza;
        this.destreza = estadoStats.destreza;
        this.constitucion = estadoStats.constitucion;
        this.inteligencia = estadoStats.inteligencia;
        this.sabiduria = estadoStats.sabiduria;
        this.carisma = estadoStats.carisma;
        
        this.xPos = xInicial;
        this.yPos = yInicial;
        ActualizarPosicionVisual();
        if (IsDead()) gameObject.SetActive(false);
        onEntityCreated?.Invoke(this, EventArgs.Empty);
        onStatsChangedByAction?.Invoke(this, new onStatsChangedByActionArgs { entidad = this });
    }

    public abstract void ChooseAction();

    protected void SubmitAction(Acciones accion, float targetX, float targetY)
    {
        gameManager.RegistrarEleccion(this, accion, Mathf.RoundToInt(targetX), Mathf.RoundToInt(targetY));
    }

    public virtual void EjecutarAccion(Acciones accion, int targetX, int targetY)
    {
        switch (accion)
        {
            case Acciones.Moverse:
                Moverse(targetX, targetY);
                ActualizarPosicionVisual();
                break;
            case Acciones.Atacar:
                Atacar(targetX, targetY);
                break;
            case Acciones.Defender:
                Defender();
                break;
            case Acciones.Consumir:
                Consumir();
                break;
            case Acciones.Interactuar:
                Entidad objetivoInteraccion = gameManager.ObtenerEntidadEnCasilla(targetX, targetY);
                if (objetivoInteraccion != null) objetivoInteraccion.Interactuar();
                break;
        }
        onStatsChangedByAction?.Invoke(this, new onStatsChangedByActionArgs { entidad = this });
    }

    protected abstract void Moverse(int targetX, int targetY);
    protected abstract void Atacar(int targetX, int targetY);
    protected abstract void Defender();
    protected abstract void Consumir();

    public void RecibirDanno(int damage)
    {
        this.hp -= damage;
        Debug.Log($"<color=red>[Daño]</color> {gameObject.name} recibe {damage} puntos de daño. HP: {this.hp}");
        if (IsDead()) gameObject.SetActive(false);
    }

    public void Sanar(int cantidadACurar)
    {
        this.hp = Mathf.Min(this.hp + cantidadACurar, this.MaxHpT0);
        Debug.Log($"<color=green>[Curación]</color> {gameObject.name} se sana {cantidadACurar} puntos. HP: {this.hp}/{this.MaxHpT0}");
    }

    public virtual void Interactuar()
    {
        Debug.LogWarning("Esta acción no está implementada");
    }

    private void ActualizarPosicionVisual()
    {
        transform.position = new Vector3(xPos + TILE_CENTER_OFFSET, -yPos - TILE_CENTER_OFFSET, 0f);
    }

    public void SetEstaDefendido(bool estaDefendido)
    {
        EstaDefendido = estaDefendido;
    }
}