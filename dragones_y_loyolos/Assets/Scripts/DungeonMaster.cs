using System;
using UnityEngine;
using SQLite4Unity3d;

public enum NivelDificultad
{
    D1_asistido_muy_facil,
    D2_facil,
    D3_normal,
    D4_dificil
}

public class DungeonMaster : MonoBehaviour
{    
    public static float EnemyHitMultiplier { get; private set; } = 1.0f;
    public static float EnemyAttackMultiplier { get; private set; } = 1.0f;
    public static float EnemyDamageMultiplier { get; private set; } = 1.0f;
    public static float EnemyDetectionRadiusMultiplier { get; private set; } = 1.0f;
    public static float EnemyAggressivenessMultiplier { get; private set; } = 1.0f;
    public static float EnemyEscapeMultiplier { get; private set; } = 1.0f;
    public static float PlayerHealMultiplier { get; private set; } = 1.0f;

    private SQLManager sqlManager;
    private int lastEvaluatedTimestep = -1;
    private int timestepsToLookBackAt = 50;
    
    public static event Action<NivelDificultad, float, string> OnDifficultyChanged;

    void Start()
    {
        sqlManager = FindFirstObjectByType<SQLManager>();
        GameManager.OnGameStateSavedOrLoaded += GameManager_OnGameStateSavedOrLoaded;
        
        // Enviamos un nivel y desglose inicial para que el juego no se ejecute sobre nada
        ApplyMultipliers(NivelDificultad.D3_normal);
        OnDifficultyChanged?.Invoke(NivelDificultad.D3_normal, 0.25f, "Sin parametros");
    }

    void OnDestroy()
    {
        GameManager.OnGameStateSavedOrLoaded -= GameManager_OnGameStateSavedOrLoaded;
    }

    private void GameManager_OnGameStateSavedOrLoaded(object sender, int currentTimestep)
    {
        if (sqlManager == null || currentTimestep <= 0) return;
        
        if (currentTimestep % 5 != 0 || currentTimestep == lastEvaluatedTimestep) return;

        lastEvaluatedTimestep = currentTimestep;

        SQLiteConnection connection = sqlManager.GetConnection();
        if (connection == null) return;

        PlayerComponent player = FindFirstObjectByType<PlayerComponent>();
        if (player == null) return;

        int startTimestep = Mathf.Max(1, currentTimestep - timestepsToLookBackAt);

        float hpRatio = (float)player.hp / Mathf.Max(1, player.MaxHpT0);

        var hpHistory = connection.Query<StatsBaseEntidadesSQL>(
            "SELECT hp FROM Stats_base_entidades WHERE id_entidades = 1 AND timestep >= ? AND timestep <= ? ORDER BY timestep ASC, subTimestep ASC",
            startTimestep, currentTimestep);
        
        int recentDamage = 0;
        for (int i = 1; i < hpHistory.Count; i++)
        {
            if (hpHistory[i].hp < hpHistory[i - 1].hp) 
            {
                recentDamage += (hpHistory[i - 1].hp - hpHistory[i].hp);
            }
        }

        int heals = connection.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Tiempo_acciones_entidades WHERE id_entidades = 1 AND id_acciones = 'Consumir' AND timestep >= ?", 
            startTimestep);

        int kills = connection.ExecuteScalar<int>(
            "SELECT COUNT(DISTINCT id_entidades) FROM Stats_base_entidades WHERE id_entidades != 1 AND hp <= 0 AND timestep <= ?", 
            currentTimestep);

        int enemyAttacks = connection.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Tiempo_acciones_entidades WHERE id_entidades != 1 AND id_acciones = 'Atacar' AND timestep >= ?", 
            startTimestep);

        string desglose = $"Ratio Vida: {hpRatio:F2}\n" +
                          $"Danno Reciente: {recentDamage}\n" +
                          $"Curaciones ({timestepsToLookBackAt}T): {heals}\n" +
                          $"Bajas: {kills}\n" +
                          $"Ataques recibidos: {enemyAttacks}";

        float riskScore = 0f;

        // Factores que aumentan el riesgo
        riskScore += (1f - hpRatio) * 0.4f;               // Vida baja
        riskScore += (recentDamage > 30 ? 0.3f : 0f);     // Daño masivo reciente
        riskScore += (enemyAttacks > 5 ? 0.2f : 0f);      // Presión enemiga alta
        
        // Factores que reducen el riesgo
        riskScore -= (heals > 2 ? 0.1f : 0f);             // Curación activa
        riskScore -= (kills > 20 ? 0.2f : 0f);            // Dominio del jugador

        riskScore = Mathf.Clamp(riskScore, 0f, 1f);

        NivelDificultad currentDifficulty = NivelDificultad.D3_normal;
        if (riskScore >= 0.70f) currentDifficulty = NivelDificultad.D1_asistido_muy_facil;
        else if (riskScore >= 0.40f && riskScore < 0.70f) currentDifficulty = NivelDificultad.D2_facil;
        else if (riskScore >= 0.18f && riskScore < 0.40f) currentDifficulty = NivelDificultad.D3_normal;
        else currentDifficulty = NivelDificultad.D4_dificil;

        ApplyMultipliers(currentDifficulty);
        
        OnDifficultyChanged?.Invoke(currentDifficulty, riskScore, desglose);
        
        Debug.Log($"[DungeonMaster] Turno {currentTimestep} evaluado. Riesgo: {riskScore:F2}. Dificultad: {currentDifficulty}");
    }

    private void ApplyMultipliers(NivelDificultad level)
    {
        switch (level)
        {
            case NivelDificultad.D1_asistido_muy_facil:
                EnemyHitMultiplier = 0.70f; EnemyAttackMultiplier = 0.65f; EnemyDamageMultiplier = 0.70f;
                EnemyDetectionRadiusMultiplier = 0.75f; EnemyAggressivenessMultiplier = 0.65f;
                EnemyEscapeMultiplier = 1.20f; PlayerHealMultiplier = 1.30f;
                break;
            case NivelDificultad.D2_facil:
                EnemyHitMultiplier = 0.85f; EnemyAttackMultiplier = 0.85f; EnemyDamageMultiplier = 0.85f;
                EnemyDetectionRadiusMultiplier = 0.90f; EnemyAggressivenessMultiplier = 0.85f;
                EnemyEscapeMultiplier = 1.10f; PlayerHealMultiplier = 1.15f;
                break;
            case NivelDificultad.D3_normal:
                EnemyHitMultiplier = 1.00f; EnemyAttackMultiplier = 1.00f; EnemyDamageMultiplier = 1.00f;
                EnemyDetectionRadiusMultiplier = 1.00f; EnemyAggressivenessMultiplier = 1.00f;
                EnemyEscapeMultiplier = 1.00f; PlayerHealMultiplier = 1.00f;
                break;
            case NivelDificultad.D4_dificil:
                EnemyHitMultiplier = 1.15f; EnemyAttackMultiplier = 1.20f; EnemyDamageMultiplier = 1.15f;
                EnemyDetectionRadiusMultiplier = 1.15f; EnemyAggressivenessMultiplier = 1.20f;
                EnemyEscapeMultiplier = 0.85f; PlayerHealMultiplier = 0.90f;
                break;
        }
    }
}