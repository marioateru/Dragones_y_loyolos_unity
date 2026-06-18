using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class ML_Core : MonoBehaviour
{
    public static ML_Core Instancia;
    public static bool IsMLMode { get; private set; } = false;

    [Header("Forzar ML en Editor")]
    [Tooltip("Marcar para forzar el modo ML a correr en el editor")]
    public bool forzarModoML_EnEditor = false; 

    [Header("Velocidad de Simulación")]
    [Tooltip("Controla la velocidad de la simulación en operaciones por segundo. -1 = todo lo rápido que dé el procesador.")]
    public float operacionesPorSegundo = -1f;
    [HideInInspector] public float acumuladorOperaciones = 0f;

    [Header("Configuración modo ML")]
    [Tooltip("Límite de veces que el jugador va a morir y continuar la partida")]
    public int limiteEjecuciones = 50;
    public int ejecucionesActuales = 0;
    public int operacionesEnMemoria = 0;
    [Tooltip("Al alcanzar este umbral, el ML_Core guardará en disco el progreso.")]
    public int limiteOperacionesGuardado = 50000;
    public HashSet<int> salasVisitadas = new HashSet<int>();
    [Tooltip("Tiempo (en segundos) que el jugador debe estar sin encontrarse a un enemigo para terminar la simulación.")]
    [SerializeField] private float TIMEOUT_SEGUNDOS = 300f;
    
    private GameManager gameManager;
    private float tiempoUltimoEnemigo;

    void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject);
            ComprobarArgumentosConsola();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void ComprobarArgumentosConsola()
    {
        if (Application.isEditor && forzarModoML_EnEditor)
        {
            IsMLMode = true;
        }

        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-mlmode") IsMLMode = true;
            if (args[i] == "-mlruns" && i + 1 < args.Length) int.TryParse(args[i + 1], out limiteEjecuciones);
        }

        if (IsMLMode)
        {
            string idInstancia = Guid.NewGuid().ToString().Substring(0, 8);
            
            GameSession.dbActiva = Application.isEditor ? $"ML_Save_EDITOR_{idInstancia}.db" : $"ML_Save_{idInstancia}.db";
            GameSession.nombrePartidaActiva = "ML_PlayerBot_Simulation";
            
            Debug.Log($"<color=magenta>[ML-CORE]</color> Instancia iniciada. BD asignada: {GameSession.dbActiva}. Límite: {limiteEjecuciones}");
        }
    }

    void Start()
    {
        if (!IsMLMode) return;
        Application.wantsToQuit += FlushSeguridadAlCerrar;
        ResetearTimeout();
    }

    void Update()
    {
        if (!IsMLMode) return;

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager == null) return;
        }

        if (Time.realtimeSinceStartup - tiempoUltimoEnemigo > TIMEOUT_SEGUNDOS)
        {
            Debug.LogWarning($"[ML-CORE] TIMEOUT: {TIMEOUT_SEGUNDOS/60} minutos sin actividad hostil. Guardando y abortando instancia.");
            gameManager.GuardarPartidaEnDisco();
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }

    public void RegistrarOperacionIA()
    {
        operacionesEnMemoria++;
        if (operacionesEnMemoria >= limiteOperacionesGuardado)
        {
            Debug.Log($"[ML-CORE] Alcanzadas {operacionesEnMemoria} operaciones. Volcando a disco duro...");
            if (gameManager != null) gameManager.GuardarPartidaEnDisco();
            operacionesEnMemoria = 0;
        }
    }

    public void RegistrarContactoEnemigo()
    {
        tiempoUltimoEnemigo = Time.realtimeSinceStartup;
    }

    public void GestionarMuerteBot()
    {
        salasVisitadas.Clear();
        ejecucionesActuales++;
        Debug.Log($"<color=cyan>[ML-CORE]</color> Bot eliminado. Ejecución {ejecucionesActuales}/{limiteEjecuciones} completada. Guardando SQL...");
        
        if (gameManager != null) gameManager.GuardarPartidaEnDisco();
        operacionesEnMemoria = 0;

        if (ejecucionesActuales >= limiteEjecuciones)
        {
            Debug.Log("<color=green>[ML-CORE]</color> Límite de ejecuciones alcanzado. Cerrando instancia exitosamente.");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        else
        {
            if (gameManager != null) gameManager.RecargarPartidaDesdeTimestep(1); 
            ResetearTimeout();
        }
    }

    public void ResetearTimeout() => tiempoUltimoEnemigo = Time.realtimeSinceStartup;

    private bool FlushSeguridadAlCerrar()
    {
        if (gameManager != null)
        {
            Debug.Log("[ML-CORE] Comando STOP recibido. Volcando memoria RAM al SQL antes de morir...");
            gameManager.GuardarPartidaEnDisco();
        }
        return true; 
    }
}