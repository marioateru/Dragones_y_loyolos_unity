using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class Entidad
{
    public int id {get; private set;}

    public int hp {get; private set;}
    public int ac {get; private set;}
    public int fuerza {get; private set;}
    public int destreza {get; private set;}
    public int constitucion {get; private set;}
    public int inteligencia {get; private set;}
    public int sabiduria {get; private set;}
    public int carisma {get; private set;}

    public float xPos {get; private set;}
    public float yPos {get; private set;}

    public void ChooseAction(ref Enum Action, ref float xActionPos, ref float yActionPos)
    {
        if (IsDead())
        {
            Debug.Log("Entidad muerta, saltando acción");
            return;
        }

        Vector2 interactionPosition = new Vector2(xActionPos, yActionPos);
        float distanceToChosenPoint = interactionPosition.magnitude;

        // TODO: implementar rango y el resto de la función.
    }

    private bool IsDead()
    {
        if (hp < 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
