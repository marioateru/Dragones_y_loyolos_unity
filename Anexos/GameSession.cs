using UnityEngine;

/// <summary>
/// Almacena los datos de la sesión de juego.
/// Mantiene un registro de la base de datos de la partida activa y el último timestep.
/// </summary>
public static class GameSession
{
    // Nombre de la bd activa.
    public static string dbActiva = ""; 
    
    // Nombre de la bd en UI.
    public static string nombrePartidaActiva = ""; 

    public static int UltimoGuardadoManualTimestep
    {
        get => PlayerPrefs.GetInt("ManualSave_" + dbActiva, 1);
        set => PlayerPrefs.SetInt("ManualSave_" + dbActiva, value);
    }
}