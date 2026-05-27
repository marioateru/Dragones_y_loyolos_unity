using UnityEngine;

public abstract class Entidad : MonoBehaviour
{
    // === CONSTANTES ===
    // Define el desplazamiento visual para que el sprite encaje en el centro exacto de una celda del Grid.
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
        
        // Aplicamos la constante para la representación visual en el mundo real
        transform.position = new Vector3(xPos + TILE_CENTER_OFFSET, yPos + TILE_CENTER_OFFSET, 0);
        
        Debug.Log($"[Entidad] '{gameObject.name}' materializado en la celda lógica {xPos}, {yPos}.");
    }

    public bool IsDead()
    {
        return hp <= 0;
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
                
                // Mantenemos la pureza matemática usando nuestra constante
                transform.position = new Vector3(xPos + TILE_CENTER_OFFSET, yPos + TILE_CENTER_OFFSET, 0); 
                Debug.Log($"[Resolución] '{gameObject.name}' se desplazó a ({xPos}, {yPos})");
                break;
                
            case Acciones.Atacar:
                Debug.Log($"[Resolución] '{gameObject.name}' arremetió hacia ({targetX}, {targetY})");
                break;
                
            case Acciones.Defender:
                Debug.Log($"[Resolución] '{gameObject.name}' levantó su guardia.");
                break;
                
            case Acciones.Interactuar:
                Debug.Log($"[Resolución] '{gameObject.name}' manipuló la casilla ({targetX}, {targetY})");
                break;
                
            case Acciones.Consumir:
                Debug.Log($"[Resolución] '{gameObject.name}' usó un objeto.");
                break;
                
            default:
                Debug.LogWarning($"[Entidad] La acción {accion} no está contemplada.");
                break;
        }
    }
}