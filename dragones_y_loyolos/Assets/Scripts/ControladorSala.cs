using UnityEngine;
using UnityEngine.Tilemaps;

public class ControladorSala : MonoBehaviour
{
    public int idSalaActual = 0; 
    
    [Tooltip("Arrastra aquí el Tilemap de los muros de esta sala concreta")]
    public Tilemap tilemapMuros;
}