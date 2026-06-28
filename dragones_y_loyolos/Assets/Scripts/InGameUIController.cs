using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using TMPro;

public class InGameUIController : MonoBehaviour
{
    public static InGameUIController Instancia;

    [Header("Paneles")]
    public GameObject panelPausa;
    public GameObject panelGameOver;

    [Header("Texto stats")]
    [SerializeField] private TextMeshProUGUI textoHp;
    
    [Header("Textos Dificultad Dinámica")]
    [SerializeField] private TextMeshProUGUI textoDificultad;
    [SerializeField] private TextMeshProUGUI textoRiesgo;
    [SerializeField] private TextMeshProUGUI textoDesglose;

    [Header("Avisos")]
    public GameObject avisoGuardado; 

    private GameManager gameManager;
    private SQLManager sqlManager;
    private PlayerComponent jugador;

    void Awake()
    {
        Instancia = this;
    }

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        sqlManager = FindFirstObjectByType<SQLManager>();

        Entidad.onEntityCreated += entidad_onEntityCreated;
        DungeonMaster.OnDifficultyChanged += ActualizarUIDificultad;
    }

    private void OnDestroy()
    {
        if (jugador != null)
        {
            jugador.onStatsChangedByAction -= jugador_onStatsChangedByAction;
        }

        Entidad.onEntityCreated -= entidad_onEntityCreated;
        DungeonMaster.OnDifficultyChanged -= ActualizarUIDificultad;
    }

    private void entidad_onEntityCreated(object sender, EventArgs e)
    {
        jugador = FindFirstObjectByType<PlayerComponent>();

        if (jugador != null)
        {
            jugador.onStatsChangedByAction += jugador_onStatsChangedByAction;
        }
    }
    
    private void jugador_onStatsChangedByAction(object sender, Entidad.onStatsChangedByActionArgs e)
    {
        if (textoHp != null) textoHp.text = $"HP: {e.entidad.hp}";
    }

    private void ActualizarUIDificultad(NivelDificultad nivel, float riesgo, string desglose)
    {
        if (textoDificultad != null) textoDificultad.text = $"Dificultad: {nivel}";
        if (textoRiesgo != null) textoRiesgo.text = $"Riesgo: {riesgo:F2}";
        if (textoDesglose != null) textoDesglose.text = desglose;
    }

    public void AlternarPausa(bool pausar)
    {
        if (panelPausa != null) panelPausa.SetActive(pausar);
        Time.timeScale = pausar ? 0 : 1; 

        if (avisoGuardado != null) avisoGuardado.SetActive(false);
        CancelInvoke(nameof(OcultarAvisoGuardado));
    }

    public void BotonGuardarManual()
    {
        gameManager.GuardarPartidaEnDisco();
        
        GameSession.UltimoGuardadoManualTimestep = sqlManager.ObtenerUltimoTimestep();
        PlayerPrefs.SetString("Date_" + GameSession.dbActiva, DateTime.Now.ToString("dd/MM/yyyy - HH:mm:ss"));
        PlayerPrefs.Save();
        
        if (avisoGuardado != null) 
        {
            avisoGuardado.SetActive(true);
            Invoke(nameof(OcultarAvisoGuardado), 2f);
        }
    }

    private void OcultarAvisoGuardado() 
    {
        if (avisoGuardado != null) avisoGuardado.SetActive(false);
    }

    public void MostrarGameOver()
    {
        if (panelGameOver != null) panelGameOver.SetActive(true);
        Time.timeScale = 0; 
    }

    public void CargarUltimoMomentoConVida()
    {
        int ultimoTimestepVivo = sqlManager.ObtenerUltimoTimestepConVida();
        OcultarYReanudar();
        gameManager.RecargarPartidaDesdeTimestep(ultimoTimestepVivo);
    }

    public void CargarUltimoGuardadoManual()
    {
        int timestepGuardadoManual = GameSession.UltimoGuardadoManualTimestep;
        OcultarYReanudar();
        gameManager.RecargarPartidaDesdeTimestep(timestepGuardadoManual);
    }

    public void IrAMenuPrincipal()
    {
        OcultarYReanudar();
        SceneManager.LoadScene("MainMenu");
    }

    public void SalirAlEscritorio()
    {
        Application.Quit();
    }

    private void OcultarYReanudar()
    {
        if (panelPausa != null) panelPausa.SetActive(false);
        if (panelGameOver != null) panelGameOver.SetActive(false);
        Time.timeScale = 1;
    }
}