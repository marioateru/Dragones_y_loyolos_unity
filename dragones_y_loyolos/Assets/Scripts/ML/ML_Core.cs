using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Collections.Generic;

public class ML_Core : MonoBehaviour
{
    public static ML_Core Instancia;
    public static bool IsMLMode { get; private set; } = false;

    [Header("Forzar ML en Editor")]
    public bool forzarModoML_EnEditor = false; 

    [Header("Velocidad de Simulación")]
    public float operacionesPorSegundo = -1f;
    [HideInInspector] public float acumuladorOperaciones = 0f;

    [Header("Configuración modo ML")]
    public int limiteEjecuciones = 50;
    public int ejecucionesActuales = 0;
    public int operacionesEnMemoria = 0;
    public int limiteOperacionesGuardado = 50000;
    public HashSet<int> salasVisitadas = new HashSet<int>();
    
    [Header("Condiciones de Parada")]
    [SerializeField] private float TIMEOUT_SEGUNDOS = 300f;
    [SerializeField] private float TIMEOUT_MATA_ENEMIGO = 120f; // NUEVO: 2 Minutos
    
    private GameManager gameManager;
    private float tiempoUltimoEnemigo;
    private float tiempoUltimaMuerteEnemigo; // NUEVO
    private string idInstanciaBase;

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
        if (Application.isEditor && forzarModoML_EnEditor) IsMLMode = true;

        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-mlmode") IsMLMode = true;
            if (args[i] == "-mlruns" && i + 1 < args.Length) int.TryParse(args[i + 1], out limiteEjecuciones);
        }

        if (IsMLMode)
        {
            idInstanciaBase = Guid.NewGuid().ToString().Substring(0, 8);
            GameSession.nombrePartidaActiva = "ML_PlayerBot_Simulation";
            AsignarNuevaBD();
            Debug.Log($"<color=magenta>[ML-CORE]</color> Instancia iniciada. Límite: {limiteEjecuciones}");
        }
    }

    private void AsignarNuevaBD()
    {
        GameSession.dbActiva = Application.isEditor ? $"ML_Save_EDITOR_{idInstanciaBase}_Run_{ejecucionesActuales}.db" : $"ML_Save_{idInstanciaBase}_Run_{ejecucionesActuales}.db";
        Debug.Log($"[ML-CORE] Nueva BD en uso: {GameSession.dbActiva}");
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

        float tiempoActual = Time.realtimeSinceStartup;

        // TIMEOUT 1: Sin contacto
        if (tiempoActual - tiempoUltimoEnemigo > TIMEOUT_SEGUNDOS)
        {
            Debug.LogWarning($"[ML-CORE] TIMEOUT: {TIMEOUT_SEGUNDOS/60} minutos sin ver enemigos. Reiniciando run.");
            gameManager.enabled = false;
            GestionarMuerteBot();
            return;
        }

        // TIMEOUT 2: Sin matar a nadie (NUEVO)
        if (tiempoActual - tiempoUltimaMuerteEnemigo > TIMEOUT_MATA_ENEMIGO)
        {
            Debug.LogWarning($"<color=red>[ML-CORE] TIMEOUT: {TIMEOUT_MATA_ENEMIGO/60} minutos sin matar a ningún enemigo. Bot atascado o débil. Reiniciando run.</color>");
            gameManager.enabled = false;
            GestionarMuerteBot();
            return;
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

    public void RegistrarContactoEnemigo() => tiempoUltimoEnemigo = Time.realtimeSinceStartup;
    
    public void RegistrarMuerteEnemigo() => tiempoUltimaMuerteEnemigo = Time.realtimeSinceStartup; // NUEVO

    public void GestionarMuerteBot()
    {
        salasVisitadas.Clear();
        Debug.Log($"<color=cyan>[ML-CORE]</color> Ejecución {ejecucionesActuales + 1}/{limiteEjecuciones} completada. Guardando SQL...");
        
        if (gameManager != null) gameManager.GuardarPartidaEnDisco();
        operacionesEnMemoria = 0;
        
        ejecucionesActuales++;

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
            AsignarNuevaBD();
            ResetearTimeout();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void ResetearTimeout() 
    {
        tiempoUltimoEnemigo = Time.realtimeSinceStartup;
        tiempoUltimaMuerteEnemigo = Time.realtimeSinceStartup;
    }

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