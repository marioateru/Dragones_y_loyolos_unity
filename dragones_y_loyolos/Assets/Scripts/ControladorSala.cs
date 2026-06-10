using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class ControladorSala : MonoBehaviour
{
    public int idSalaActual;
    public Tilemap tilemapMuros;
    [SerializeField] private Dictionary<Vector2Int, PuertaMazmorra> puertas = new Dictionary<Vector2Int, PuertaMazmorra>();

    public void RegistrarPuerta(int x, int y, PuertaMazmorra puerta)
    {
        puertas[new Vector2Int(x, y)] = puerta;
    }

    public PuertaMazmorra ObtenerPuerta(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);
        if (puertas.TryGetValue(pos, out PuertaMazmorra puertaEncontrada))
        {
            return puertaEncontrada;
        }
        return null;
    }
}