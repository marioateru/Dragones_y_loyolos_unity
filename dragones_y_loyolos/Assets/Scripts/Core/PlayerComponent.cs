using UnityEngine;
using System.Collections.Generic;

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
        
        // Se habilita la bandera y el código termina. El GameManager se quedará esperando.
        esMiTurno = true;
        Debug.Log("🟩 [JUGADOR] Es mi turno de decisión. Esperando input de teclado o ratón...");
    }

    public bool EsSuTurno() => esMiTurno;

    public List<Acciones> DeterminarOpcionesCasilla(int targetX, int targetY)
    {
        List<Acciones> opciones = new List<Acciones>();
        Entidad entidadDestino = gameManager.ObtenerEntidadEnCasilla(targetX, targetY);
        PuertaMazmorra puerta = gameManager.salaActual.ObtenerPuerta(targetX, targetY);

        if (entidadDestino != null)
        {
            if (accionesPermitidas.Contains(Acciones.Atacar)) opciones.Add(Acciones.Atacar);
            if (accionesPermitidas.Contains(Acciones.Defender)) opciones.Add(Acciones.Defender);
            if (accionesPermitidas.Contains(Acciones.Interactuar)) opciones.Add(Acciones.Interactuar);
        }
        else if (puerta != null)
        {
            if (accionesPermitidas.Contains(Acciones.Moverse)) opciones.Add(Acciones.Moverse);
            if (accionesPermitidas.Contains(Acciones.Interactuar)) opciones.Add(Acciones.Interactuar);
        }
        else
        {
            if (accionesPermitidas.Contains(Acciones.Moverse)) opciones.Add(Acciones.Moverse);
        }

        if (accionesPermitidas.Contains(Acciones.Consumir)) opciones.Add(Acciones.Consumir);

        return opciones;
    }

    public bool ValidarIntencion(Acciones accion, int targetX, int targetY)
    {
        if (!accionesPermitidas.Contains(accion)) return false;

        int dist = Mathf.Max(Mathf.Abs(Mathf.RoundToInt(xPos) - targetX), Mathf.Abs(Mathf.RoundToInt(yPos) - targetY));
        int rangoPermitido = 1;

        if (accion == Acciones.Moverse) rangoPermitido = Mathf.Max(1, velocidad);
        
        if (dist > rangoPermitido) return false;

        if (accion == Acciones.Moverse)
        {
            if (collisionChecker.HayMuro(targetX, targetY)) return false;
            if (gameManager.ObtenerEntidadEnCasilla(targetX, targetY) != null) return false;
        }

        return true;
    }

    public void ConsumirTurno(Acciones accion, int targetX, int targetY)
    {
        esMiTurno = false;
        SubmitAction(accion, targetX, targetY);
    }
}