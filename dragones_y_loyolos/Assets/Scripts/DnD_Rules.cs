using UnityEngine;

public static class DnD_Rules
{
    // TODO: tenemos que encontrar una forma de hacer bypass para permitir que el algoritmo DM manipule los dados
    // FIXME: ya lo he hecho, pero está bastante guarro.
    public static int LanzarD20(bool tieneVentaja = false, bool tieneDesventaja = false)
    {
        int tirada1 = Random.Range(1, 21);
        int tirada2 = Random.Range(1, 21);

        if (tieneVentaja && !tieneDesventaja) return Mathf.Max(tirada1, tirada2);
        if (tieneDesventaja && !tieneVentaja) return Mathf.Min(tirada1, tirada2);
        
        return tirada1;
    }

    // Trucada para funcionar con el multiplicador de redes bayesianas
    public static int LanzarD20(bool tieneVentaja = false, bool tieneDesventaja = false, float multiplicadorProbabilidadTirada = 0.01f)
    {
        int tirada1 = Random.Range(1, 21);
        int tirada2 = Random.Range(1, 21);

        if (tieneVentaja && !tieneDesventaja) return Mathf.Max(tirada1, tirada2);
        if (tieneDesventaja && !tieneVentaja) return Mathf.Min(tirada1, tirada2);
        
        if (multiplicadorProbabilidadTirada <= 0) multiplicadorProbabilidadTirada = 0.01f; 
        return Mathf.FloorToInt(tirada1 * multiplicadorProbabilidadTirada);
    }

    public static int LanzarDados(int cantidad, int caras)
    {
        int total = 0;
        for (int i = 0; i < cantidad; i++)
        {
            total += Random.Range(1, caras + 1);
        }
        return total;
    }

    // Trucada para funcionar con el multiplicador de redes bayesianas
    public static int LanzarDados(int cantidad, int caras, float multiplicadorProbabilidadTirada = 0.01f)
    {
        int total = 0;
        
        if (multiplicadorProbabilidadTirada <= 0) multiplicadorProbabilidadTirada = 0.01f;
        
        for (int i = 0; i < cantidad; i++)
        {
            total += Mathf.FloorToInt(Random.Range(1, caras + 1) * multiplicadorProbabilidadTirada);
        }
        return total;
    }
}