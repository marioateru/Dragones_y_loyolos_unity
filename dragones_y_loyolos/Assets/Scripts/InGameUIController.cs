using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class InGameUIController : MonoBehaviour
{
    public static InGameUIController Instancia;

    [Header("Paneles")]
    public GameObject panelPausa;
    public GameObject panelGameOver;
    
    [Header("Avisos")]
    public GameObject avisoGuardado; 

    private GameManager gameManager;
    private SQLManager sqlManager;

    void Awake()
    {
        Instancia = this;
        gameManager = FindFirstObjectByType<GameManager>();
        sqlManager = FindFirstObjectByType<SQLManager>();
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
        int timestepVivo = sqlManager.ObtenerUltimoTimestepConVida();
        OcultarYReanudar();
        gameManager.RecargarPartidaDesdeTimestep(timestepVivo);
    }

    public void CargarUltimoGuardadoManual()
    {
        int timestepManual = GameSession.UltimoGuardadoManualTimestep;
        OcultarYReanudar();
        gameManager.RecargarPartidaDesdeTimestep(timestepManual);
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