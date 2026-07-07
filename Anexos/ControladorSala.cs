using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class ControladorSala : MonoBehaviour
{
    public int idSalaActual; 
    public Tilemap tilemapMuros; 

    // Diccionario para almacenar todas las salas del juego.
    private Dictionary<Vector2Int, PuertaMazmorra> puertasRegistradas = new Dictionary<Vector2Int, PuertaMazmorra>();

    public void RegistrarPuerta(int xPos, int yPos, PuertaMazmorra puerta)
    {
        Vector2Int posicionPuerta = new Vector2Int(xPos, yPos);

        if (!puertasRegistradas.ContainsKey(posicionPuerta))
        {
            puertasRegistradas.Add(posicionPuerta, puerta);
        }
    }

    public PuertaMazmorra ObtenerPuerta(int xPos, int yPos)
    {
        Vector2Int posicionPuerta = new Vector2Int(xPos, yPos);

        if (puertasRegistradas.TryGetValue(posicionPuerta, out PuertaMazmorra puerta))
        {
            return puerta;
        }
        return null;
    }

    // En desuso. Calcula la puerta cuya posición es más cercana a las posiciones xPos, yPos
    public PuertaMazmorra ObtenerPuertaMasCercana(int xPos, int yPos)
    {
        PuertaMazmorra mejorPuerta = null;

        float mejorDistancia = float.MaxValue;

        foreach (var keyValuePair in puertasRegistradas)
        {
            Vector2Int posicionPuerta = keyValuePair.Key;
            
            float dist = Mathf.Max(Mathf.Abs(xPos - posicionPuerta.x), Mathf.Abs(yPos - posicionPuerta.y));
            
            if (dist < mejorDistancia)
            {
                mejorDistancia = dist;
                mejorPuerta = keyValuePair.Value;
            }
        }
        return mejorPuerta;
    }

    public List<PuertaMazmorra> ObtenerTodasLasPuertas()
    {
        return new List<PuertaMazmorra>(puertasRegistradas.Values);
    }
}