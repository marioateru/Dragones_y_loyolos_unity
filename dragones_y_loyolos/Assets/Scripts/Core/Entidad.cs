using UnityEngine;
using System.Collections.Generic;

public abstract class Entidad : MonoBehaviour
{
    private const float TILE_CENTER_OFFSET = 0.5f;
    [Header("Referencias SQL")]
    [field: SerializeField] public int id_entidades { get; private set; } 
    [field: SerializeField] public int id_stats_base { get; private set; } 

    [Header("Estadísticas Base")]
    [field: SerializeField] public int hp { get; private set; }
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

    public bool EstaDefendido { get; private set; } = false;
    public int MaxHpT0 { get; private set; } // Memoria C# (lee de T=0). No SQL.
    public List<Acciones> accionesPermitidas = new List<Acciones>();

    protected GameManager gameManager;
    public bool IsRun {get; protected set;}
    public bool IsHighPriority {get; protected set;}

    public virtual void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
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
    }

    public bool IsDead() => hp <= 0;

    public void EstablecerEstadoDeProcesamiento(bool run, bool highPrio)
    {
        this.IsRun = run;
        this.IsHighPriority = highPrio;
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
                this.xPos = targetX;
                this.yPos = targetY;
                ActualizarPosicionVisual();
                break;

            case Acciones.Atacar:
                Entidad objetivoAtaque = gameManager.ObtenerEntidadEnCasilla(targetX, targetY);
                if (objetivoAtaque != null)
                {
                    bool desventaja = objetivoAtaque.EstaDefendido; 
                    int d20 = DnD_Rules.LanzarD20(ventaja: false, desventaja: desventaja);
                    int tiradaAtaque = d20 + fuerza;

                    // FIXME: esta regla de cuando sacas 20 aciertas es un poco random, puede putear al jugador.
                    bool hit = (d20 == 20) || (tiradaAtaque >= objetivoAtaque.ac);
                    
                    Debug.Log($"<color=orange>[Combate]</color> {gameObject.name} ataca a {objetivoAtaque.gameObject.name}. Tirada: {d20} + {fuerza} = {tiradaAtaque} vs AC {objetivoAtaque.ac}. ¿Impacta?: {hit}");

                    if (hit)
                    {
                        int dannoTotal = Mathf.Max(1, DnD_Rules.LanzarDados(1, 4) + fuerza);
                        objetivoAtaque.RecibirInteraccion(this, Acciones.Atacar, dannoTotal);
                    }
                }
                else 
                {
                    Debug.Log($"<color=grey>[Combate]</color> {gameObject.name} ataca al aire en ({targetX}, {targetY}). El objetivo se movió.");
                }
                break;

            case Acciones.Defender:
                this.EstaDefendido = true;
                break;

            case Acciones.Interactuar:
                Entidad objetivoInteraccion = gameManager.ObtenerEntidadEnCasilla(targetX, targetY);
                if (objetivoInteraccion != null) objetivoInteraccion.RecibirInteraccion(this, Acciones.Interactuar);
                break;
                
            case Acciones.Consumir:
                if (this.hp >= this.MaxHpT0)
                {
                    Debug.Log($"<color=yellow>[Salud]</color> {gameObject.name} ya tiene HP máximo ({this.MaxHpT0}). No hace nada.");
                    return; 
                }
                int cantidadACurar = DnD_Rules.LanzarDados(1, 8);
                RecibirInteraccion(this, Acciones.Consumir, cantidadACurar);
                break;
        }
    }

    public virtual void RecibirInteraccion(Entidad origen, Acciones tipoInteraccion, int valorData = 0)
    {
        switch (tipoInteraccion)
        {
            case Acciones.Atacar:
                this.hp -= valorData;
                Debug.Log($"<color=red>[Daño]</color> {gameObject.name} recibe {valorData} de daño. HP restante: {this.hp}");
                
                if (IsDead()) 
                {
                    Debug.Log($"<color=red>[Muerte]</color> {gameObject.name} ha muerto.");
                    gameObject.SetActive(false);
                }
                break;

            case Acciones.Consumir:
                this.hp = Mathf.Min(this.hp + valorData, this.MaxHpT0);
                Debug.Log($"<color=green>[Curación]</color> {gameObject.name} se cura. HP actual: {this.hp}/{this.MaxHpT0}");
                break;
            default:
                Debug.Log("El recibir esta acción aún no está implementado");
                break;
        }
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