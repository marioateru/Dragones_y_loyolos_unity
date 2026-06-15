using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class ControladorSala : MonoBehaviour
{
    public int idSalaActual; 
    public Tilemap tilemapMuros; 

    private Dictionary<Vector2Int, PuertaMazmorra> puertasRegistradas = new Dictionary<Vector2Int, PuertaMazmorra>();

    public void RegistrarPuerta(int x, int y, PuertaMazmorra puerta)
    {
        Vector2Int pos = new Vector2Int(x, y);
        if (!puertasRegistradas.ContainsKey(pos))
        {
            puertasRegistradas.Add(pos, puerta);
        }
    }

    public PuertaMazmorra ObtenerPuerta(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);
        if (puertasRegistradas.TryGetValue(pos, out PuertaMazmorra puerta))
        {
            return puerta;
        }
        return null;
    }

    public PuertaMazmorra ObtenerPuertaMasCercana(int x, int y)
    {
        PuertaMazmorra mejorPuerta = null;
        float mejorDistancia = float.MaxValue;

        foreach (var kvp in puertasRegistradas)
        {
            Vector2Int posPuerta = kvp.Key;
            
            float dist = Mathf.Max(Mathf.Abs(x - posPuerta.x), Mathf.Abs(y - posPuerta.y));
            
            if (dist < mejorDistancia)
            {
                mejorDistancia = dist;
                mejorPuerta = kvp.Value;
            }
        }
        return mejorPuerta;
    }

    public List<PuertaMazmorra> ObtenerTodasLasPuertas()
    {
        return new List<PuertaMazmorra>(puertasRegistradas.Values);
    }
}