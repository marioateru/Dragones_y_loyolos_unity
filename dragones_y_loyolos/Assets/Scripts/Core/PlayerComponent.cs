using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TileCollisionChecker))] // Obligamos a que el GameObject tenga el colisionador
public class PlayerComponent : Entidad
{
    private bool esMiTurno = false;
    private TileCollisionChecker collisionChecker;

    public override void Awake()
    {
        base.Awake(); // Llamamos al Awake del padre para que coja el GameManager
        collisionChecker = GetComponent<TileCollisionChecker>();
    }

    public override void ChooseAction() 
    {
        if (IsDead()) 
        {
            SubmitAction(Acciones.Moverse, xPos, yPos); 
            return;
        }
        esMiTurno = true;
    }

    void Update()
    {
        if (!esMiTurno) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        int dx = 0, dy = 0;

        if (keyboard.wKey.wasPressedThisFrame) dy = 1;
        else if (keyboard.sKey.wasPressedThisFrame) dy = -1;
        else if (keyboard.aKey.wasPressedThisFrame) dx = -1;
        else if (keyboard.dKey.wasPressedThisFrame) dx = 1;

        if (dx != 0 || dy != 0)
        {
            // 1. Calculamos a qué coordenada exacta intenta moverse
            int objetivoX = Mathf.RoundToInt(xPos) + dx;
            int objetivoY = Mathf.RoundToInt(yPos) + dy;

            // 2. Le preguntamos al checker si ese Tile está ocupado por un muro
            if (collisionChecker.HayMuro(objetivoX, objetivoY)) 
            {
                return; // Si hay muro, bloqueamos el proceso y el jugador no pierde su turno
            }

            // 3. Si no hay muro, bloqueamos el input y mandamos la acción al GameManager
            esMiTurno = false;
            SubmitAction(Acciones.Moverse, objetivoX, objetivoY);
        }
    }
}