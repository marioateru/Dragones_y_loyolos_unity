using UnityEngine;
using UnityEngine.Tilemaps;

public class TileCollisionChecker : MonoBehaviour
{
    private const float TILE_CENTER_OFFSET = 0.5f; 
    private const float TAMANNO_CRUZ_DEBUG = 0.2f; 
    private const float DURACION_RAYO_DEBUG = 1f;

    private Tilemap tilemapMuros;
    public void AsignarMuros(Tilemap muros)
    {
        this.tilemapMuros = muros;
    }

    public bool HayMuro(int targetX, int targetY)
    {
        if (tilemapMuros == null) return false;

        Vector3Int celdaObjetivo = new Vector3Int(targetX, -targetY - 1, 0);

        return tilemapMuros.HasTile(celdaObjetivo);
    }
}