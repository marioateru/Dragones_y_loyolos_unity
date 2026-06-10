using UnityEngine;
using UnityEngine.Tilemaps;

public class TileCollisionChecker : MonoBehaviour
{
    private const float TILE_CENTER_OFFSET = 0.5f;
    private Tilemap tilemapMuros;
    
    public void AsignarMuros(Tilemap muros)
    {
        this.tilemapMuros = muros;
    }

    public bool HayMuro(int targetX, int targetY)
    {
        if (tilemapMuros == null) return false;

        Vector3 posicionMundo = new Vector3(targetX + TILE_CENTER_OFFSET, -targetY + TILE_CENTER_OFFSET, 0);

        Vector3Int celdaReal = tilemapMuros.layoutGrid.WorldToCell(posicionMundo);

        return tilemapMuros.HasTile(celdaReal);
    }
}