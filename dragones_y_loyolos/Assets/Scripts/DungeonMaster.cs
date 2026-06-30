using System;
using System.IO;
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
    public static float EnemyAttackMultiplier { get; private set; } = 1.0f; // En desuso. DamageMultiplier ya se encarga.
    public static float EnemyDamageMultiplier { get; private set; } = 1.0f;
    public static float EnemyDetectionRadiusMultiplier { get; private set; } = 1.0f;
    public static float EnemyAggressivenessMultiplier { get; private set; } = 1.0f;
    public static float EnemyEscapeMultiplier { get; private set; } = 1.0f;
    public static float PlayerHealMultiplier { get; private set; } = 1.0f;

    private SQLManager sqlManager;
    private GameManager gameManager;
    private LectorRedBayesiana redBayesiana; 
    private int lastEvaluatedTimestep = -1;

    [SerializeField]
    private int timestepsToLookBackAt = 50; 
    [SerializeField]
    private int turnsUntilReevaluate = 4;  
    
    public static event Action<NivelDificultad, float, string> OnDifficultyChanged;

    private readonly string[] estadosRiesgo = { "safe", "moderate_damage", "high_damage", "death" };

    void Start()
    {
        sqlManager = FindFirstObjectByType<SQLManager>();
        gameManager = FindFirstObjectByType<GameManager>();
        GameManager.OnGameStateSavedOrLoaded += GameManager_OnGameStateSavedOrLoaded;
        
        string jsonPath = Path.Combine(Application.streamingAssetsPath, "modelo_red_bayesiana_dificultad.json");
        if (File.Exists(jsonPath))
        {
            string jsonContent = File.ReadAllText(jsonPath);
            redBayesiana = new LectorRedBayesiana(jsonContent);
        }
        else
        {
            Debug.LogError("[DungeonMaster] No se encontró el modelo bayesiano en: " + jsonPath);
        }

        ApplyMultipliers(NivelDificultad.D3_normal);
        OnDifficultyChanged?.Invoke(NivelDificultad.D3_normal, 0.25f, "Esperando turnos para evaluar...");
    }

    void OnDestroy()
    {
        GameManager.OnGameStateSavedOrLoaded -= GameManager_OnGameStateSavedOrLoaded;
    }

    // Evento lanzado cuando el GameManager finaliza un turno T.Recalcula las probabilidades para ajustar el nivel de dificultad.
    private void GameManager_OnGameStateSavedOrLoaded(object sender, int currentTimestep)
    {
        if (sqlManager == null || redBayesiana == null || currentTimestep <= 0) return;
        if (currentTimestep % turnsUntilReevaluate != 0 || currentTimestep == lastEvaluatedTimestep) return;

        lastEvaluatedTimestep = currentTimestep;

        // Nos conectamos
        SQLiteConnection connection = sqlManager.GetConnection();
        if (connection == null) return;

        PlayerComponent player = FindFirstObjectByType<PlayerComponent>();
        if (player == null) return;

        // Seleccionamos el timestep t-50
        int startTimestep = Mathf.Max(0, currentTimestep - timestepsToLookBackAt);
        
        // Obtenemos Banda de Vida
        float hpRatio = (float)player.hp / Mathf.Max(1, player.MaxHpT0);
        string hpBand = "high_81_100";
        if (hpRatio <= 0.20f) hpBand = "critical_1_20";
        else if (hpRatio <= 0.50f) hpBand = "low_21_50";
        else if (hpRatio <= 0.80f) hpBand = "medium_51_80";

        // Obtenemos Danno reciente
        int hpInicioVentana = connection.ExecuteScalar<int>(
            "SELECT hp FROM Stats_base_entidades WHERE id_entidades = 1 AND timestep <= ? ORDER BY timestep DESC LIMIT 1", 
            startTimestep);
        
        if (hpInicioVentana <= 0) hpInicioVentana = player.MaxHpT0;
        
        int recentDamage = Mathf.Max(0, hpInicioVentana - player.hp);
        
        string damageBand = "none";
        if (recentDamage >= 1 && recentDamage <= 10) damageBand = "low_1_10";
        else if (recentDamage >= 11 && recentDamage <= 30) damageBand = "medium_11_30";
        else if (recentDamage >= 31) damageBand = "high_31_plus";

        // Buscamos los enemigos que en total ha eliminado el jugador
        int kills = connection.ExecuteScalar<int>(
            "SELECT COUNT(DISTINCT id_entidades) FROM Stats_base_entidades WHERE id_entidades != 1 AND hp <= 0 AND id_entidades IN (SELECT id_entidades FROM Stats_base_entidades WHERE hp > 0)");
        
        string killsBand = "0";
        if (kills >= 1 && kills <= 5) killsBand = "1_5";
        else if (kills >= 6 && kills <= 25) killsBand = "6_25";
        else if (kills >= 26 && kills <= 75) killsBand = "26_75";
        else if (kills >= 76) killsBand = "76_plus";

        // Miramos en qué fase de la partida se encuentra el jugador temporalmente.
        string progressBand = "start_0_50";
        if (currentTimestep > 50 && currentTimestep <= 250) progressBand = "early_51_250";
        else if (currentTimestep > 250 && currentTimestep <= 1000) progressBand = "mid_251_1000";
        else if (currentTimestep > 1000 && currentTimestep <= 2500) progressBand = "late_1001_2500";
        else if (currentTimestep > 2500) progressBand = "very_late_2501_plus";

        // Miramos la sala en la que se encuentra el jugador.
        int salaId = gameManager.salaActual != null ? gameManager.salaActual.idSalaActual : 0;
        string roomBand = "room_0_1";
        if (salaId == 2) roomBand = "room_2";
        else if (salaId == 3) roomBand = "room_3";
        else if (salaId == 4) roomBand = "room_4";
        else if (salaId >= 5) roomBand = "room_5_plus";

        int lastTurn = currentTimestep - 1;

        // Miramos cuántos enemigos han atacado en el turno anterior
        int enemyAttacks = connection.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Tiempo_acciones_entidades WHERE id_entidades != 1 AND id_acciones = 'Atacar' AND timestep = ?", 
            lastTurn);
        string pressureBand = "none";
        if (enemyAttacks == 1) pressureBand = "one";
        else if (enemyAttacks == 2 || enemyAttacks == 3) pressureBand = "two_three";
        else if (enemyAttacks >= 4) pressureBand = "four_plus";

        // Miramos cuántas acciones han tomado los enemigos en el turno anterior
        int enemyActivity = connection.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Tiempo_acciones_entidades WHERE id_entidades != 1 AND timestep = ?", 
            lastTurn);
        string activityBand = "none";
        if (enemyActivity >= 1 && enemyActivity <= 5) activityBand = "low_1_5";
        else if (enemyActivity >= 6 && enemyActivity <= 15) activityBand = "medium_6_15";
        else if (enemyActivity >= 16) activityBand = "high_16_plus";

        // Obtenemos las veces que el jugador se ha curado
        int playerConsume = connection.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Tiempo_acciones_entidades WHERE id_entidades = 1 AND id_acciones = 'Consumir' AND timestep = ?", 
            lastTurn);
        string consumptionBand = playerConsume > 0 ? "consume" : "no_consume";

        // Resolvemos la ecuación de Bayes.        
        double[] numeradorProbabilidades = new double[4];
        double denominadorSumaTotal = 0.0;

        for (int i = 0; i < estadosRiesgo.Length; i++)
        {
            string riesgo = estadosRiesgo[i];

            double probConjunta = redBayesiana.ObtenerPrior(riesgo);
            
            probConjunta *= redBayesiana.ObtenerProbabilidad("player_hp_band", riesgo, hpBand);
            probConjunta *= redBayesiana.ObtenerProbabilidad("recent_damage_band", riesgo, damageBand);
            probConjunta *= redBayesiana.ObtenerProbabilidad("kills_so_far_band", riesgo, killsBand);
            probConjunta *= redBayesiana.ObtenerProbabilidad("progress_band", riesgo, progressBand);
            probConjunta *= redBayesiana.ObtenerProbabilidad("room_band", riesgo, roomBand);
            probConjunta *= redBayesiana.ObtenerProbabilidad("enemy_attack_pressure_band", riesgo, pressureBand);
            probConjunta *= redBayesiana.ObtenerProbabilidad("enemy_activity_band", riesgo, activityBand);
            probConjunta *= redBayesiana.ObtenerProbabilidad("player_consumption_band", riesgo, consumptionBand);

            numeradorProbabilidades[i] = probConjunta;
            denominadorSumaTotal += probConjunta; 
        }

        // Calculamos las probabilidades
        double pSafe = numeradorProbabilidades[0] / denominadorSumaTotal;
        double pModerate = numeradorProbabilidades[1] / denominadorSumaTotal;
        double pHigh = numeradorProbabilidades[2] / denominadorSumaTotal;
        double pDeath = numeradorProbabilidades[3] / denominadorSumaTotal;

        // Aplicamos la ecucación
        float riskScore = (float)(pDeath + pHigh + (0.5 * pModerate));

        // Seleccionamos la dificultad
        NivelDificultad currentDifficulty;
        if (riskScore >= 0.70f) currentDifficulty = NivelDificultad.D1_asistido_muy_facil;
        else if (riskScore >= 0.40f && riskScore < 0.70f) currentDifficulty = NivelDificultad.D2_facil;
        else if (riskScore >= 0.18f && riskScore < 0.40f) currentDifficulty = NivelDificultad.D3_normal;
        else currentDifficulty = NivelDificultad.D4_dificil;

        // Aplicamos los multiplicadores
        ApplyMultipliers(currentDifficulty);
        
        string desglose = $"<b>Probabilidades Naive Bayes:</b>\n" +
                          $"Muerte: {(pDeath*100):F2}%\n" +
                          $"Danno Alto: {(pHigh*100):F2}%\n" +
                          $"Danno Mod: {(pModerate*100):F2}%\n" +
                          $"Seguro: {(pSafe*100):F2}%\n" +
                          $"---------------------\n" +
                          $"<b>Bandas Extraidas:</b>\n" +
                          $"HP: {hpBand}\n" +
                          $"Danno 50T: {damageBand}\n" +
                          $"Bajas: {killsBand}";

        OnDifficultyChanged?.Invoke(currentDifficulty, riskScore, desglose);
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

public class LectorRedBayesiana
{
    private string contenidoJson;

    public LectorRedBayesiana(string json)
    {
        this.contenidoJson = json;
    }

    public double ObtenerPrior(string estadoRiesgo)
    {
        int indicePrior = contenidoJson.IndexOf("\"prior\"");
        int indiceRiesgo = contenidoJson.IndexOf($"\"{estadoRiesgo}\"", indicePrior);
        return ExtraerNumero(indiceRiesgo);
    }

    public double ObtenerProbabilidad(string variable, string estadoRiesgo, string bandaValor)
    {
        int indiceCPT = contenidoJson.IndexOf("\"cpts_feature_given_risk\"");
        int indiceVariable = contenidoJson.IndexOf($"\"{variable}\"", indiceCPT);
        int indiceRiesgo = contenidoJson.IndexOf($"\"{estadoRiesgo}\"", indiceVariable);
        int indiceBanda = contenidoJson.IndexOf($"\"{bandaValor}\"", indiceRiesgo);
        return ExtraerNumero(indiceBanda);
    }

    private double ExtraerNumero(int indiceInicial)
    {
        if (indiceInicial == -1) return 0.0001; 
        
        int dosPuntos = contenidoJson.IndexOf(":", indiceInicial);
        int coma = contenidoJson.IndexOf(",", dosPuntos);
        int llave = contenidoJson.IndexOf("}", dosPuntos);
        
        int final = (coma != -1 && coma < llave) ? coma : llave;
        
        string numeroBruto = contenidoJson.Substring(dosPuntos + 1, final - (dosPuntos + 1));
        string numeroLimpio = "";
        
        foreach (char c in numeroBruto)
        {
            if (char.IsDigit(c) || c == '.' || c == 'e' || c == 'E' || c == '-' || c == '+')
            {
                numeroLimpio += c;
            }
        }

        if (double.TryParse(numeroLimpio, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double valor))
        {
            return valor;
        }
        
        return 0.0001;
    }
}