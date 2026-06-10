using UnityEngine;

public abstract class Entidad : MonoBehaviour
{
    protected const float TILE_CENTER_OFFSET = 0.5f; 

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

    [Header("Posición Tilemap")]
    [field: SerializeField] public float xPos { get; protected set; }
    [field: SerializeField] public float yPos { get; protected set; }

    protected GameManager gameManager;

    // Si está en rango de jugador, se computa
    public bool isRun {get; protected set;}
    public bool isHighPriority {get; protected set;}

    public virtual void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public void InicializarDatosSQL(int idEnt, int idStats, StatsBaseEntidadesSQL estadoStats, int xInicial, int yInicial)
    {
        this.id_entidades = idEnt;
        this.id_stats_base = idStats;
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
        
        Debug.Log($"[Entidad] '{gameObject.name}' materializado en la celda lógica {xPos}, {yPos}.");
    }

    public bool IsDead()
    {
        return hp <= 0;
    }

    public void EstablecerEstadoDeProcesamiento(bool run, bool highPrio)
    {
        this.isRun = run;
        this.isHighPriority = highPrio;
    }

    public abstract void ChooseAction();

    protected void SubmitAction(Acciones accion, float targetX, float targetY)
    {
        gameManager.RegistrarEleccion(this, accion, Mathf.RoundToInt(targetX), Mathf.RoundToInt(targetY));
    }

    public void EjecutarAccion(Acciones accion, int targetX, int targetY)
    {
        switch (accion)
        {
            case Acciones.Moverse:
                this.xPos = targetX;
                this.yPos = targetY;
                
                ActualizarPosicionVisual();

                Debug.Log($"[Resolución] '{gameObject.name}' se desplazó a ({xPos}, {yPos})");
                break;
                
            case Acciones.Atacar:
                Debug.Log($"[Resolución] '{gameObject.name}' atacó hacia ({targetX}, {targetY})");
                break;
                
            case Acciones.Defender:
                Debug.Log($"[Resolución] '{gameObject.name}' se protegió.");
                break;
                
            case Acciones.Interactuar:
                Debug.Log($"[Resolución] '{gameObject.name}' interactuó en ({targetX}, {targetY})");
                break;
                
            case Acciones.Consumir:
                Debug.Log($"[Resolución] '{gameObject.name}' usó un objeto.");
                break;
                
            default:
                Debug.LogWarning($"[Entidad] La acción {accion} no está contemplada.");
                break;
        }
    }

    private void ActualizarPosicionVisual()
    {
        transform.position = new Vector3(xPos + TILE_CENTER_OFFSET, -yPos - TILE_CENTER_OFFSET, 0);
    }
}