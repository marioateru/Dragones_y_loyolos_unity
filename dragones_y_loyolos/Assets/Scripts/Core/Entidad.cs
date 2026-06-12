using UnityEngine;
using System.Collections.Generic;

public abstract class Entidad : MonoBehaviour
{
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
    [field: SerializeField] public int velocidad { get; protected set; } = 6; 

    [Header("Posición Tilemap")]
    [field: SerializeField] public float xPos { get; protected set; }
    [field: SerializeField] public float yPos { get; protected set; }

    public bool estaDefendido { get; set; } = false;
    public List<Acciones> accionesPermitidas = new List<Acciones>();

    protected GameManager gameManager;
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
    }

    public bool IsDead() => hp <= 0;

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
                    bool desventaja = objetivoAtaque.estaDefendido; 
                    int d20 = DnD_Rules.LanzarD20(ventaja: false, desventaja: desventaja);
                    int tiradaAtaque = d20 + fuerza;

                    if (tiradaAtaque >= objetivoAtaque.ac)
                    {
                        int dannoTotal = DnD_Rules.LanzarDados(1, 4) + fuerza;
                        objetivoAtaque.RecibirInteraccion(this, Acciones.Atacar, dannoTotal);
                    }
                }
                break;

            case Acciones.Defender:
                this.estaDefendido = true;
                break;

            case Acciones.Interactuar:
                Entidad objetivoInteraccion = gameManager.ObtenerEntidadEnCasilla(targetX, targetY);
                if (objetivoInteraccion != null) objetivoInteraccion.RecibirInteraccion(this, Acciones.Interactuar);
                break;
                
            case Acciones.Consumir:
                int cura = DnD_Rules.LanzarDados(1, 4);
                RecibirInteraccion(this, Acciones.Consumir, cura);
                break;
        }
    }

    public virtual void RecibirInteraccion(Entidad origen, Acciones tipoInteraccion, int valorData = 0)
    {
        if (tipoInteraccion == Acciones.Atacar)
        {
            this.hp -= valorData;
            if (IsDead()) gameObject.SetActive(false);
        }
        else if (tipoInteraccion == Acciones.Consumir)
        {
            this.hp += valorData;
        }
    }

    private void ActualizarPosicionVisual()
    {
        transform.position = new Vector3(xPos + 0.5f, -yPos - 0.5f, 0f);
    }
}