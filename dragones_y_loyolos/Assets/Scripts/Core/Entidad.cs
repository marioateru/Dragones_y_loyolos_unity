using UnityEngine;
using System.Collections.Generic;

public abstract class Entidad : MonoBehaviour
{
    // Constante para alinear la entidad de juego con la loseta.
    private const float TILE_CENTER_OFFSET = 0.5f;

    [Header("Claves SQL")]
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
    public int MaxHpT0 { get; private set; }
    public bool IsRun {get; protected set;}
    public bool IsHighPriority {get; protected set;}
    public List<Acciones> accionesPermitidas = new List<Acciones>();
    protected GameManager gameManager;

    public virtual void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public bool IsDead() => hp <= 0;

    // Permite establecer si la entidad se procesa en cola y con destreza estándar.
    public void EstablecerEstadoDeProcesamiento(bool run, bool highPrio)
    {
        this.IsRun = run;
        this.IsHighPriority = highPrio;
    }

    // Función que permite hacer una copia de los valores del SQL.
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


    // Función que permite implementar lógica para seleccionar una acción.
    public abstract void ChooseAction();

    // Función que sube la acción al GameManager.
    protected void SubmitAction(Acciones accion, float targetX, float targetY)
    {
        gameManager.RegistrarEleccion(this, accion, Mathf.RoundToInt(targetX), Mathf.RoundToInt(targetY));
    }

    // Función a través de la cual la entidad de juego recibe una acción.
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
                    int resultadoTirada = DnD_Rules.LanzarD20(tieneVentaja: false, tieneDesventaja: desventaja);
                    int tiradaAtaque = resultadoTirada + fuerza;

                    bool isHit = (resultadoTirada == 20) || (tiradaAtaque >= objetivoAtaque.ac);
                    
                    Debug.Log($"<color=orange>[Combate]</color> {gameObject.name} ataca a {objetivoAtaque.gameObject.name}. Tirada: {resultadoTirada} + {fuerza} = {tiradaAtaque} vs AC {objetivoAtaque.ac}. ¿Impacta?: {isHit}");

                    if (isHit)
                    {
                        int dannoTotal = Mathf.Max(1, DnD_Rules.LanzarDados(1, 4) + fuerza);
                        objetivoAtaque.RecibirDanno(dannoTotal);
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
                if (objetivoInteraccion != null) objetivoInteraccion.Interactuar();
                break;
                
            case Acciones.Consumir:
                int cantidadACurar = DnD_Rules.LanzarDados(1, 8);
                Sanar(cantidadACurar);
                break;
        }
    }

    private void RecibirDanno(int damage)
    {
        this.hp -= damage;
        Debug.Log($"<color=red>[Daño]</color> {gameObject.name} recibe {damage} puntos de daño. HP: {this.hp}");
        
        if (IsDead()) gameObject.SetActive(false);
    }

    private void Sanar(int cantidadACurar)
    {
        // Impide que nos podamos sanar más allá de nuestra HP máxima.
        this.hp = Mathf.Min(this.hp + cantidadACurar, this.MaxHpT0);
        Debug.Log($"<color=green>[Curación]</color> {gameObject.name} se sana {cantidadACurar} puntos de vida. HP: {this.hp}/{this.MaxHpT0}");
    }
    public virtual void Interactuar()
    {
        Debug.LogWarning("Esta acción aún no está implementada");
    }

    // Función para alinear las entidades de juego con la cuadrícula.
    // Se usa "-yPos" en vez de + "yPos" pues en los mapas de tiled, el vector (0, -1) apunta hacia arriba.
    private void ActualizarPosicionVisual()
    {
        transform.position = new Vector3(xPos + TILE_CENTER_OFFSET, -yPos - TILE_CENTER_OFFSET, 0f);
    }

    public void SetEstaDefendido(bool estaDefendido)
    {
        EstaDefendido = estaDefendido;
    }
}