using UnityEngine;
using System.Collections.Generic;

public class EnemyComponent : Entidad
{
    [Header("Configuración IA")]
    [Tooltip("Distancia a la que el enemigo detecta al jugador si no hay muros de por medio.")]
    public int rangoVision = 8; 
    
    [Tooltip("Porcentaje de vida (0.0 a 1.0) al que el enemigo huirá.")]
    [Range(0f, 1f)] 
    public float umbralHuida = 0.3f;
    
    private PlayerComponent jugadorObjetivo;
    private TileCollisionChecker collisionChecker;
    private SQLManager sqlManager;
    
    private int vidaMaxima = -1;
    private bool estaHuyendo = false;

    public override void Awake()
    {
        base.Awake();
        collisionChecker = FindFirstObjectByType<TileCollisionChecker>();
        sqlManager = FindFirstObjectByType<SQLManager>();
    }

    public override void ChooseAction()
    {
        if (IsDead()) 
        {
            SubmitAction(Acciones.Moverse, xPos, yPos); 
            return;
        }

        if (vidaMaxima <= 0) 
        {
            vidaMaxima = sqlManager.ObtenerVidaMaximaDeEntidad(this.id_entidades);
        }

        if (jugadorObjetivo == null) jugadorObjetivo = FindFirstObjectByType<PlayerComponent>();

        if (jugadorObjetivo == null || jugadorObjetivo.IsDead())
        {
            Vector2Int patrullaCiega = CalcularCasillaAleatoria(Mathf.RoundToInt(xPos), Mathf.RoundToInt(yPos));
            SubmitAction(Acciones.Moverse, patrullaCiega.x, patrullaCiega.y);
            return;
        }

        int miX = Mathf.RoundToInt(xPos);
        int miY = Mathf.RoundToInt(yPos);
        int jugadorX = Mathf.RoundToInt(jugadorObjetivo.xPos);
        int jugadorY = Mathf.RoundToInt(jugadorObjetivo.yPos);

        int distancia = Mathf.Max(Mathf.Abs(miX - jugadorX), Mathf.Abs(miY - jugadorY));

        if (hp <= (vidaMaxima * umbralHuida))
        {
            estaHuyendo = true;
        }

        bool hayLineaDeVision = !collisionChecker.HayMuroEnRuta(miX, miY, jugadorX, jugadorY);

        if (estaHuyendo)
        {
            Debug.Log($"<color=orange>[IA Enemiga]</color> {gameObject.name} entra en pánico (HP: {hp}/{vidaMaxima}). ¡Huye!");
            Vector2Int casillaEscape = CalcularMejorCasilla(miX, miY, jugadorX, jugadorY, huir: true);
            SubmitAction(Acciones.Moverse, casillaEscape.x, casillaEscape.y);
            return;
        }

        if (hayLineaDeVision && distancia <= rangoVision)
        {
            if (distancia <= 1) 
            {
                Debug.Log($"<color=red>[IA Enemiga]</color> {gameObject.name} ataca al jugador en ({jugadorX},{jugadorY}).");
                SubmitAction(Acciones.Atacar, jugadorX, jugadorY);
            }
            else 
            {
                Debug.Log($"<color=yellow>[IA Enemiga]</color> {gameObject.name} persigue al jugador.");
                Vector2Int casillaAcercamiento = CalcularMejorCasilla(miX, miY, jugadorX, jugadorY, huir: false);
                SubmitAction(Acciones.Moverse, casillaAcercamiento.x, casillaAcercamiento.y);
            }
        }
        else
        {
            Debug.Log($"<color=grey>[IA Enemiga]</color> {gameObject.name} no ve al jugador. Patrulla aleatoriamente.");
            Vector2Int casillaRandom = CalcularCasillaAleatoria(miX, miY);
            SubmitAction(Acciones.Moverse, casillaRandom.x, casillaRandom.y);
        }
    }

    /// <summary>
    /// Escanea las 8 casillas adyacentes y escoge la mejor según si quiere acercarse o alejarse.
    /// </summary>
    private Vector2Int CalcularMejorCasilla(int origenX, int origenY, int targetX, int targetY, bool huir)
    {
        Vector2Int mejorCasilla = new Vector2Int(origenX, origenY);
        int mejorDistancia = huir ? -1 : int.MaxValue; 

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue; 

                int checkX = origenX + x;
                int checkY = origenY + y;

                // FIX ANTI-ATASCOS: Evita atravesar esquinas
                if (collisionChecker.HayMuroEnRuta(origenX, origenY, checkX, checkY)) continue;
                if (gameManager.ObtenerEntidadEnCasilla(checkX, checkY) != null) continue;
                
                // FIX OUT OF BOUNDS: Las puertas se consideran muros para los enemigos
                if (gameManager.salaActual.ObtenerPuerta(checkX, checkY) != null) continue;

                int distAlJugador = Mathf.Max(Mathf.Abs(checkX - targetX), Mathf.Abs(checkY - targetY));

                if (huir)
                {
                    if (distAlJugador > mejorDistancia)
                    {
                        mejorDistancia = distAlJugador;
                        mejorCasilla = new Vector2Int(checkX, checkY);
                    }
                }
                else 
                {
                    if (distAlJugador < mejorDistancia)
                    {
                        mejorDistancia = distAlJugador;
                        mejorCasilla = new Vector2Int(checkX, checkY);
                    }
                }
            }
        }

        return mejorCasilla;
    }

    /// <summary>
    /// Escanea las 8 casillas adyacentes, descarta los obstáculos y devuelve una aleatoria.
    /// </summary>
    private Vector2Int CalcularCasillaAleatoria(int origenX, int origenY)
    {
        List<Vector2Int> casillasValidas = new List<Vector2Int>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = origenX + x;
                int checkY = origenY + y;

                // FIX ANTI-ATASCOS
                if (!collisionChecker.HayMuroEnRuta(origenX, origenY, checkX, checkY) && gameManager.ObtenerEntidadEnCasilla(checkX, checkY) == null)
                {
                    // FIX OUT OF BOUNDS: Jamás elegir una casilla con puerta
                    if (gameManager.salaActual.ObtenerPuerta(checkX, checkY) == null)
                    {
                        casillasValidas.Add(new Vector2Int(checkX, checkY));
                    }
                }
            }
        }

        if (casillasValidas.Count > 0)
        {
            int indexAleatorio = Random.Range(0, casillasValidas.Count);
            return casillasValidas[indexAleatorio];
        }

        return new Vector2Int(origenX, origenY);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.6f);

        float tamanoLado = (rangoVision * 2) + 1; 
        
        Gizmos.DrawWireCube(transform.position, new Vector3(tamanoLado, tamanoLado, 0f));
    }
}