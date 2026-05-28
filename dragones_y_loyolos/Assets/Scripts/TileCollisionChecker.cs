using UnityEngine;
using UnityEngine.Tilemaps;

public class TileCollisionChecker : MonoBehaviour
{
    private const float TILE_CENTER_OFFSET = 0.5f; 
    private const float TAMANNO_CRUZ_DEBUG = 0.2f; 
    private const float DURACION_RAYO_DEBUG = 1f;

    [Header("Tilemap de muro")]
    [SerializeField] private Tilemap tilemapMuros;

    public bool HayMuro(int targetX, int targetY)
    {
        if (tilemapMuros == null) 
        {
            Debug.LogWarning("[Colisión] Tilemap de muros no asignado.");
            return false;
        }

        Vector3Int celdaObjetivo = new Vector3Int(targetX, targetY, 0);

        float centroX = targetX + TILE_CENTER_OFFSET;
        float centroY = targetY + TILE_CENTER_OFFSET;
        
        // Vector3 cruzSupIzq = new Vector3(centroX - TAMANNO_CRUZ_DEBUG, centroY + TAMANNO_CRUZ_DEBUG, 0);
        // Vector3 cruzInfDer = new Vector3(centroX + TAMANNO_CRUZ_DEBUG, centroY - TAMANNO_CRUZ_DEBUG, 0);
        
        // Vector3 cruzSupDer = new Vector3(centroX + TAMANNO_CRUZ_DEBUG, centroY + TAMANNO_CRUZ_DEBUG, 0);
        // Vector3 cruzInfIzq = new Vector3(centroX - TAMANNO_CRUZ_DEBUG, centroY - TAMANNO_CRUZ_DEBUG, 0);

        // Debug.DrawLine(cruzSupIzq, cruzInfDer, Color.magenta, DURACION_RAYO_DEBUG);
        // Debug.DrawLine(cruzSupDer, cruzInfIzq, Color.magenta, DURACION_RAYO_DEBUG);

        if (tilemapMuros.HasTile(celdaObjetivo))
        {
            Debug.Log($"[Colisión] Muro detectado en la celda ({targetX}, {targetY}). Movimiento denegado.");
            return true;
        }
        
        return false;
    }
}