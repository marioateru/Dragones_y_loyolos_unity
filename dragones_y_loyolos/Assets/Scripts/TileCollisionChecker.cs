using UnityEngine;
using UnityEngine.Tilemaps;

public class TileCollisionChecker : MonoBehaviour
{
    [SerializeField] private const float TILE_PIVOT_POSITION = 0.5f;
    private Tilemap tilemapMuros;
    
    public void AsignarMuros(Tilemap muros)
    {
        this.tilemapMuros = muros;
    }

    public bool HayMuro(int targetX, int targetY)
    {
        if (tilemapMuros == null) return false;

        Vector3 posicionMundo = new Vector3(targetX + TILE_PIVOT_POSITION, -targetY - TILE_PIVOT_POSITION, 0);
        Vector3Int celdaReal = tilemapMuros.layoutGrid.WorldToCell(posicionMundo);

        return tilemapMuros.HasTile(celdaReal);
    }

    // Sucolega el brasenham
    public bool HayMuroEnRuta(int coordenadaInicioX, int coordenadaInicioY, int coordenadaDestinoX, int coordenadaDestinoY)
    {
        if (tilemapMuros == null) return false;

        int distanciaX = Mathf.Abs(coordenadaDestinoX - coordenadaInicioX);
        int distanciaY = Mathf.Abs(coordenadaDestinoY - coordenadaInicioY);
        
        int pasoEnDireccionX = coordenadaInicioX < coordenadaDestinoX ? 1 : -1;
        int pasoEnDireccionY = coordenadaInicioY < coordenadaDestinoY ? 1 : -1;

        int errorAcumulado = distanciaX - distanciaY;

        int coordenadaActualX = coordenadaInicioX;
        int coordenadaActualY = coordenadaInicioY;

        while (true)
        {
            int dobleErrorAcumulado = 2 * errorAcumulado;

            if (HayMuro(coordenadaActualX, coordenadaActualY)) return true;

            if (coordenadaActualX == coordenadaDestinoX && coordenadaActualY == coordenadaDestinoY) break;

            if (dobleErrorAcumulado > -distanciaY)
            {
                errorAcumulado -= distanciaY;
                coordenadaActualX += pasoEnDireccionX;
            }

            if (dobleErrorAcumulado < distanciaX)
            {
                errorAcumulado += distanciaX;
                coordenadaActualY += pasoEnDireccionY;
            }
        }

        return false; 
    }
}