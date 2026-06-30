using UnityEngine;
using System.Collections.Generic;

public class EnemyComponent : Entidad
{
    [Header("Configuración IA")]
    [Tooltip("Distancia a la que el enemigo detectará al jugador.")]
    public int rangoVision = 8; 
    
    [Range(0f, 1f)] public float umbralHuida = 0.3f;
    [Range(0f, 1f)] public float probContraataque = 0.35f;
    
    private PlayerComponent jugadorObjetivo;
    private TileCollisionChecker collisionChecker;
    private SQLManager sqlManager;
    private int vidaMaxima = -1;

    public override void Awake()
    {
        base.Awake();
        collisionChecker = FindFirstObjectByType<TileCollisionChecker>();
        sqlManager = FindFirstObjectByType<SQLManager>();
    }

    // Métodos abstractos implementados por EnemyComponent
    protected override void Moverse(int targetX, int targetY)
    {
        this.xPos = targetX;
        this.yPos = targetY;
    }

    // Función de ataque con multiplicadores DungeonMaster implementados.
    protected override void Atacar(int targetX, int targetY)
    {
        Entidad objetivoAtaque = gameManager.ObtenerEntidadEnCasilla(targetX, targetY);

        if (objetivoAtaque != null)
        {
            bool desventaja = objetivoAtaque.EstaDefendido; 
            int resultadoTirada = DnD_Rules.LanzarD20(tieneVentaja: false, tieneDesventaja: desventaja, DungeonMaster.EnemyHitMultiplier);
            int tiradaAtaque = resultadoTirada + fuerza;
            bool isHit = (resultadoTirada == 20) || (tiradaAtaque >= objetivoAtaque.ac);
            
            Debug.Log($"<color=red>[Combate-Enemigo]</color> {gameObject.name} ataca. Tirada: {resultadoTirada}");
            if (isHit)
            {
                int dannoBase = DnD_Rules.LanzarDados(1, 4, DungeonMaster.EnemyDamageMultiplier);
                int dannoTotal = Mathf.Max(1, dannoBase + fuerza);
                objetivoAtaque.RecibirDanno(dannoTotal);
            }
        }
    }

    protected override void Defender()
    {
        this.EstaDefendido = true;
    }

    protected override void Consumir()
    {
        int cantidadACurar = DnD_Rules.LanzarDados(1, 8);
        Sanar(cantidadACurar);
    }

    public override void ChooseAction()
    {
        // Si estamos muertos, no nos movemos del sitio
        if (IsDead()) 
        {
            SubmitAction(Acciones.Moverse, xPos, yPos); 
            return;
        }

        if (vidaMaxima <= 0) vidaMaxima = sqlManager.ObtenerVidaMaximaDeEntidad(this.id_entidades);
        if (jugadorObjetivo == null) jugadorObjetivo = FindFirstObjectByType<PlayerComponent>();
        
        int miPosX = Mathf.RoundToInt(xPos);
        int miPosY = Mathf.RoundToInt(yPos);
        
        if (jugadorObjetivo == null || jugadorObjetivo.IsDead())
        {
            Vector2Int patrullaAleatoria = CalcularCasillaAleatoria(miPosX, miPosY);
            SubmitAction(Acciones.Moverse, patrullaAleatoria.x, patrullaAleatoria.y);
            return;
        }

        int jugadorX = Mathf.RoundToInt(jugadorObjetivo.xPos);
        int jugadorY = Mathf.RoundToInt(jugadorObjetivo.yPos);
        int distReal = Mathf.Max(Mathf.Abs(miPosX - jugadorX), Mathf.Abs(miPosY - jugadorY));

        bool tieneLineaVision = !collisionChecker.HayMuroEnRuta(miPosX, miPosY, jugadorX, jugadorY);

        // El rangoVisionDinamico es el rango de visión afectado por el multiplicador de DungeonMaster
        int rangoVisionDinamico = Mathf.RoundToInt(rangoVision * DungeonMaster.EnemyDetectionRadiusMultiplier);

        if (distReal > rangoVisionDinamico || !tieneLineaVision)
        {
            Vector2Int patrullaAleatoria = CalcularCasillaAleatoria(miPosX, miPosY);
            SubmitAction(Acciones.Moverse, patrullaAleatoria.x, patrullaAleatoria.y);
            return;
        }

        // Cálculo de la lógica de escape y contraataque
        bool quiereHuir = hp <= vidaMaxima * (umbralHuida * DungeonMaster.EnemyEscapeMultiplier);
        bool valiente = quiereHuir && (UnityEngine.Random.value <= (probContraataque * DungeonMaster.EnemyAggressivenessMultiplier));

        if (quiereHuir && !valiente)
        {
            Vector2Int rutaEscape = CalcularSiguientePasoHuida(miPosX, miPosY, jugadorObjetivo);

            if (rutaEscape.x != -9999)
            {
                ProcesarPaso(rutaEscape);
                return;
            }
        }
        if (distReal <= 1)
        {
            SubmitAction(Acciones.Atacar, jugadorX, jugadorY);
            return;
        }

        // Si no está a una unidad del jugador, se acerca.
        Vector2Int rutaAvance = CalcularSiguientePasoAStar(miPosX, miPosY, jugadorX, jugadorY);

        if (rutaAvance.x != -9999) ProcesarPaso(rutaAvance);
        else
        {
            Vector2Int rutaAlternativa = CalcularCasillaAleatoria(miPosX, miPosY);

            ProcesarPaso(rutaAlternativa);
        }
    }

    private void ProcesarPaso(Vector2Int avance)
    {
        Entidad obstaculo = gameManager.ObtenerEntidadEnCasilla(avance.x, avance.y);

        if (obstaculo != null && obstaculo != this) SubmitAction(Acciones.Atacar, avance.x, avance.y);
        else SubmitAction(Acciones.Moverse, avance.x, avance.y);
    }

    private Vector2Int CalcularSiguientePasoAStar(int startX, int startY, int targetX, int targetY)
    {

        List<Vector2Int> path = Pathfinding.GetAStarPath(new Vector2Int(startX, startY), new Vector2Int(targetX, targetY), collisionChecker, gameManager);
        if (path.Count > 0) return path[0];

        // Equivalente a un "null"
        return new Vector2Int(-9999, -9999);
    }

    private Vector2Int CalcularSiguientePasoHuida(int origenX, int origenY, Entidad adversario)
    {
        int adversarioXPos = Mathf.RoundToInt(adversario.xPos);
        int adversarioYPos = Mathf.RoundToInt(adversario.yPos);

        List<Vector2Int> casillaValida = Pathfinding.GetValidAdjacent(new Vector2Int(origenX, origenY), 1, collisionChecker, gameManager);

        Vector2Int mejorCasilla = new Vector2Int(origenX, origenY);

        int mejorDist = -1;

        foreach (var casilla in casillaValida)
        {
            int dist = Mathf.Max(Mathf.Abs(casilla.x - adversarioXPos), Mathf.Abs(casilla.y - adversarioYPos));

            if (dist > mejorDist)
            {
                mejorDist = dist;
                mejorCasilla = casilla;
            }
        }
        return mejorCasilla;
    }

    private Vector2Int CalcularCasillaAleatoria(int origenX, int origenY) => Pathfinding.GetRandomValidTile(new Vector2Int(origenX, origenY), collisionChecker, gameManager);
}