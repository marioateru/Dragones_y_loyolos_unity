using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TileCollisionChecker))]
public class PlayerComponent : Entidad
{
    private bool esMiTurno = false;
    private TileCollisionChecker collisionChecker;

    public override void Awake()
    {
        base.Awake();
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

        if (keyboard.wKey.wasPressedThisFrame) dy = -1;
        else if (keyboard.sKey.wasPressedThisFrame) dy = 1;
        else if (keyboard.aKey.wasPressedThisFrame) dx = -1;
        else if (keyboard.dKey.wasPressedThisFrame) dx = 1;

        if (dx != 0 || dy != 0)
        {
            int objetivoX = Mathf.RoundToInt(xPos) + dx;
            int objetivoY = Mathf.RoundToInt(yPos) + dy;

            if (gameManager != null && gameManager.salaActual != null)
            {
                PuertaMazmorra puertaEnObjetivo = gameManager.salaActual.ObtenerPuerta(objetivoX, objetivoY);
                if (puertaEnObjetivo != null)
                {
                    esMiTurno = false;
                    SubmitAction(Acciones.Moverse, objetivoX, objetivoY);
                    gameManager.ViajarAUbicacion(this, puertaEnObjetivo.idSalaDestino, puertaEnObjetivo.destinoX, puertaEnObjetivo.destinoY);
                    return;
                }
            }

            if (collisionChecker.HayMuro(objetivoX, objetivoY)) 
            {
                Debug.Log("No puedes moverte, hay un muro en medio.");
                return; 
            }

            if (gameManager != null)
            {
                Entidad entidadBloqueante = gameManager.ObtenerEntidadEnCasilla(objetivoX, objetivoY);
                if (entidadBloqueante != null)
                {
                    Debug.Log($"No puedes moverte, hay otra entidad ({entidadBloqueante.gameObject.name}) en medio.");
                    return; 
                }
            }

            esMiTurno = false;
            SubmitAction(Acciones.Moverse, objetivoX, objetivoY);
        }
    }
}