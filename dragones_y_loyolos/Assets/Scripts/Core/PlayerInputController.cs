using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PlayerInputController : MonoBehaviour
{

    private const float ENTITY_CENTER_OFFSET = 0.5f;
    [HideInInspector] public PlayerComponent jugador; 
    public GridInteractionUI uiInteractiva;

    private Grid mapGrid;

    void Start()
    {
        if (uiInteractiva != null) uiInteractiva.Inicializar();
    }

    void Update()
    {
        // No debe entrar la interacción si el juego se pausa.
        if (Time.timeScale == 0f) return;

        if (jugador == null || !jugador.EsSuTurno()) return;

        var keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.wKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame))
        {
            if (uiInteractiva != null) uiInteractiva.OcultarTodo();

            int distX = 0, distY = 0;
            if (keyboard.wKey.wasPressedThisFrame) distY = -1;
            else if (keyboard.sKey.wasPressedThisFrame) distY = 1;
            else if (keyboard.aKey.wasPressedThisFrame) distX = -1;
            else if (keyboard.dKey.wasPressedThisFrame) distX = 1;

            if (distX != 0 || distY != 0)
            {
                int tecladoX = Mathf.RoundToInt(jugador.xPos) + distX;
                int tecladoY = Mathf.RoundToInt(jugador.yPos) + distY;

                if (jugador.ValidarIntencion(Acciones.Moverse, tecladoX, tecladoY))
                {
                    jugador.ConsumirTurno(Acciones.Moverse, tecladoX, tecladoY);
                }
            }
            return;
        }

        var mouse = Mouse.current;
        if (mouse == null || uiInteractiva == null) return;
        
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (mapGrid == null) mapGrid = FindFirstObjectByType<Grid>();
        if (mapGrid == null) return;

        Vector2 mousePosScreen = mouse.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosScreen.x, mousePosScreen.y, Mathf.Abs(Camera.main.transform.position.z)));
        
        int objetivoX = Mathf.FloorToInt(mouseWorldPos.x);
        int objetivoY = Mathf.FloorToInt(-mouseWorldPos.y); 

        if (mouse.leftButton.wasPressedThisFrame)
        {
            uiInteractiva.OcultarTodo(); 
            bool esAccionValida = jugador.ValidarIntencion(Acciones.Moverse, objetivoX, objetivoY);
            uiInteractiva.ActivarSeleccionIzquierda(objetivoX, objetivoY, !esAccionValida);
        }

        if (mouse.rightButton.wasPressedThisFrame)
        {
            uiInteractiva.OcultarTodo();
            
            Vector2 origenVisual = new Vector2(jugador.xPos + ENTITY_CENTER_OFFSET, -jugador.yPos - ENTITY_CENTER_OFFSET);
            List<Acciones> opcionesAMostrar = jugador.DeterminarOpcionesCasilla(objetivoX, objetivoY);
            
            if (opcionesAMostrar.Count > 0)
            {
                bool rangoInvalido = true;
                
                foreach (var accion in opcionesAMostrar)
                {
                    if (jugador.ValidarIntencion(accion, objetivoX, objetivoY)) 
                    {
                        rangoInvalido = false;
                        break;
                    }
                }

                uiInteractiva.ActivarMenuDerecho(objetivoX, objetivoY, origenVisual, opcionesAMostrar, rangoInvalido, (accionElegida) => {
                    if (jugador.ValidarIntencion(accionElegida, objetivoX, objetivoY))
                    {
                        uiInteractiva.OcultarTodo();
                        jugador.ConsumirTurno(accionElegida, objetivoX, objetivoY);
                    }
                    else
                    {
                        uiInteractiva.MarcarFueraDeRango();
                    }
                });
            }
            else
            {
                uiInteractiva.ActivarSeleccionIzquierda(objetivoX, objetivoY, true);
                uiInteractiva.DibujarLinea(origenVisual, new Vector2(objetivoX + 0.5f, -objetivoY - 0.5f), true);
            }
        }
    }
}