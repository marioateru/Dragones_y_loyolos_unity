using UnityEngine;
using UnityEngine.Tilemaps;

public class TileCollisionChecker : MonoBehaviour
{
    private Tilemap tilemapMuros;
    
    public void AsignarMuros(Tilemap muros)
    {
        this.tilemapMuros = muros;
    }

    public bool HayMuro(int targetX, int targetY)
    {
        if (tilemapMuros == null) return false;

        // 1. Dónde está ese punto físicamente en el mundo según la matemática pura
        Vector3 posicionMundo = new Vector3(targetX + 0.5f, -targetY - 0.5f, 0f);

        // 2. LA MANERA "X": Le restamos el offset (Ese Y=-1 de tu captura) que ST2U le pone a la capa.
        // Esto alinea la matemática con el gráfico dibujado de forma automática y perfecta.
        Vector3 posicionLocalCapa = posicionMundo - tilemapMuros.transform.position;

        // 3. Ahora sí, le pedimos la celda exacta a Unity
        Vector3Int celdaReal = tilemapMuros.layoutGrid.WorldToCell(posicionLocalCapa);

        return tilemapMuros.HasTile(celdaReal);
    }

    // Algoritmo de trazado de línea (Bresenham) para evitar atravesar muros de lejos
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
            if (HayMuro(coordenadaActualX, coordenadaActualY)) return true; // Chocó con un muro
            
            if (coordenadaActualX == coordenadaDestinoX && coordenadaActualY == coordenadaDestinoY) break; // Llegó al destino

            // Constante matemática del algoritmo para calcular el diferencial sin usar floats (decimales)
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