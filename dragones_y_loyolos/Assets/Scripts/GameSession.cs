using UnityEngine;

public static class GameSession
{
    // Nombre del archivo físico (ej: "save_12345.db")
    public static string dbActiva = ""; 
    
    // Nombre para mostrar en UI (ej: "D&L 12/05/2026")
    public static string nombrePartidaActiva = ""; 

    public static int UltimoGuardadoManualTimestep
    {
        get => PlayerPrefs.GetInt("ManualSave_" + dbActiva, 1);
        set => PlayerPrefs.SetInt("ManualSave_" + dbActiva, value);
    }
}