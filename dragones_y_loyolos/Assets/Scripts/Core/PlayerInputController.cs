using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PlayerInputController : MonoBehaviour
{
    [HideInInspector] public PlayerComponent jugador; 
    public GridInteractionUI uiInteractiva;

    private Grid mapGrid;

    void Start()
    {
        if (uiInteractiva != null) uiInteractiva.Inicializar();
    }

    void Update()
    {
        if (jugador == null || !jugador.EsSuTurno()) return;

        var keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.wKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame))
        {
            if (uiInteractiva != null) uiInteractiva.OcultarTodo();

            int dx = 0, dy = 0;
            if (keyboard.wKey.wasPressedThisFrame) dy = -1;
            else if (keyboard.sKey.wasPressedThisFrame) dy = 1;
            else if (keyboard.aKey.wasPressedThisFrame) dx = -1;
            else if (keyboard.dKey.wasPressedThisFrame) dx = 1;

            if (dx != 0 || dy != 0)
            {
                int tecladoX = Mathf.RoundToInt(jugador.xPos) + dx;
                int tecladoY = Mathf.RoundToInt(jugador.yPos) + dy;

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
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());

        Vector3Int cellPos = mapGrid.WorldToCell(mouseWorldPos);
        int objetivoX = Mathf.FloorToInt(mouseWorldPos.x);
        int objetivoY = Mathf.FloorToInt(-mouseWorldPos.y);

        if (mouse.leftButton.wasPressedThisFrame)
        {
            uiInteractiva.OcultarTodo(); 
            bool esValido = jugador.ValidarIntencion(Acciones.Moverse, objetivoX, objetivoY);
            uiInteractiva.ActivarSeleccionIzquierda(objetivoX, objetivoY, !esValido);
        }

        if (mouse.rightButton.wasPressedThisFrame)
        {
            uiInteractiva.OcultarTodo();
            
            Vector2 origenVisual = new Vector2(jugador.xPos + 0.5f, -jugador.yPos - 0.5f);
            List<Acciones> opciones = jugador.DeterminarOpcionesCasilla(objetivoX, objetivoY);
            
            if (opciones.Count > 0)
            {
                bool rangoInvalido = true;
                foreach (var acc in opciones)
                {
                    if (jugador.ValidarIntencion(acc, objetivoX, objetivoY)) 
                    {
                        rangoInvalido = false;
                        break;
                    }
                }

                uiInteractiva.ActivarMenuDerecho(objetivoX, objetivoY, origenVisual, opciones, rangoInvalido, (accionElegida) => {
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