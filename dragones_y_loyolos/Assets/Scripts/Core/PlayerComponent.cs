using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(TileCollisionChecker))]
public class PlayerComponent : Entidad
{
    [Header("Estadísticas de jugador")]
    [field: SerializeField] public string id_jugadores { get; private set; }
    [field: SerializeField] public short clase { get; private set; }
    [field: SerializeField] public short nivel { get; private set; }

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

        int origenX = Mathf.RoundToInt(xPos);
        int origenY = Mathf.RoundToInt(yPos);

        int dist = Mathf.Max(Mathf.Abs(origenX - targetX), Mathf.Abs(origenY - targetY));
        int rangoPermitido = 1;

        if (accion == Acciones.Moverse) rangoPermitido = Mathf.Max(1, velocidad);
        
        if (dist > rangoPermitido) return false;

        if (accion == Acciones.Moverse)
        {
            if (collisionChecker.HayMuroEnRuta(origenX, origenY, targetX, targetY)) return false;
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