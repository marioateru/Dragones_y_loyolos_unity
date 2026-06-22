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

    // Comprueba si hay un muro en el tile xPos yPos
    public bool HayMuro(int xPos, int yPos)
    {
        if (tilemapMuros == null) return false;

        Vector3 posicionMundo = new Vector3(xPos + TILE_CENTER_OFFSET, -yPos - TILE_CENTER_OFFSET, 0f);

        Vector3 posicionLocalCapa = posicionMundo - tilemapMuros.transform.position;

        Vector3Int posRealCelda = tilemapMuros.layoutGrid.WorldToCell(posicionLocalCapa);

        return tilemapMuros.HasTile(posRealCelda);
    }

    // Algoritmo de Bresenham.
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
            if (HayMuro(coordenadaActualX, coordenadaActualY)) return true; 
            
            if (coordenadaActualX == coordenadaDestinoX && coordenadaActualY == coordenadaDestinoY) break;

            int dobleErrorAcumulado = 2 * errorAcumulado; 
            
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