using UnityEngine;

public static class DnD_Rules
{
    public static int LanzarD20(bool tieneVentaja = false, bool tieneDesventaja = false)
    {
        int tirada1 = Random.Range(1, 21);
        int tirada2 = Random.Range(1, 21);

        if (tieneVentaja && !tieneDesventaja) return Mathf.Max(tirada1, tirada2);
        if (tieneDesventaja && !tieneVentaja) return Mathf.Min(tirada1, tirada2);
        
        return tirada1;
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
}