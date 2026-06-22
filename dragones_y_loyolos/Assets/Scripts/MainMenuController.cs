using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Elementos")]
    public GameObject panelPrincipal;
    public GameObject panelNuevaPartida;
    public GameObject panelCargarPartida;
    public TMP_InputField inputNombrePartida; 
    
    [Header("UI Cargar")]
    public Transform contenedorPartidas;
    public GameObject prefabBotonPartida;
    
    [Tooltip("Arrastra aquí el botón de 'Cargar Partida' del panel principal para ocultarlo si no hay partidas")]
    public Button botonAbrirMenuCargar; 

    private string prefijoGuardado = "Save_";

    void Start()
    {
        Time.timeScale = 1; 
        
        string[] archivosBD = Directory.GetFiles(Application.persistentDataPath, prefijoGuardado + "*.db");
        if (botonAbrirMenuCargar != null)
        {
            botonAbrirMenuCargar.gameObject.SetActive(archivosBD.Length > 0);
        }

        if (ML_Core.IsMLMode)
        {
            Debug.Log("<color=yellow>[Menu Principal]</color> Modo ML. Iniciando simulación automáticamente...");
            ConfirmarNuevaPartida();
        }
    }

    public void MostrarNuevaPartida()
    {
        panelPrincipal.SetActive(false);
        panelNuevaPartida.SetActive(true);
    }

    public void ConfirmarNuevaPartida()
    {
        string nombreEnCarpetaBD = "";
        string nombreEnJuego = "";

        if (!ML_Core.IsMLMode)
        {
            nombreEnJuego = inputNombrePartida.text;
            if (string.IsNullOrWhiteSpace(nombreEnJuego)) 
            {
                nombreEnJuego = "D&L " + DateTime.Now.ToString("yyyy-MM-dd");
            }

            nombreEnCarpetaBD = prefijoGuardado + DateTime.Now.Ticks + ".db";
            
            PlayerPrefs.SetString("DisplayName_" + nombreEnCarpetaBD, nombreEnJuego);
            PlayerPrefs.SetString("Date_" + nombreEnCarpetaBD, DateTime.Now.ToString("dd/MM/yyyy - HH:mm:ss"));
            PlayerPrefs.Save();

            GameSession.dbActiva = nombreEnCarpetaBD;
            GameSession.nombrePartidaActiva = nombreEnJuego;
        }
        else
        {
            nombreEnCarpetaBD = GameSession.dbActiva;
            nombreEnJuego = GameSession.nombrePartidaActiva;
        }

        string plantillaPath = Path.Combine(Application.streamingAssetsPath, "dragones_y_loyolos.db");
        string nuevoPath = Path.Combine(Application.persistentDataPath, nombreEnCarpetaBD);
        
        File.Copy(plantillaPath, nuevoPath, true);
        
        SceneManager.LoadScene("GameScene"); 
    }

    public void MostrarCargarPartida()
    {
        panelPrincipal.SetActive(false);
        panelCargarPartida.SetActive(true);

        foreach (Transform child in contenedorPartidas) 
        {
            child.SetParent(null);
            Destroy(child.gameObject);
        }

        string[] archivosBD = Directory.GetFiles(Application.persistentDataPath, prefijoGuardado + "*.db");
        
        foreach (string archivo in archivosBD)
        {
            string nombreArchivo = Path.GetFileName(archivo);
            string nombreMostrar = PlayerPrefs.GetString("DisplayName_" + nombreArchivo, "Partida Desconocida");
            string fechaGuardado = PlayerPrefs.GetString("Date_" + nombreArchivo, "");

            GameObject btnObjeto = Instantiate(prefabBotonPartida, contenedorPartidas);
            
            string textoFinal = $"{nombreMostrar} -- <size=36>{fechaGuardado}</size>";
            var textoLegacy = btnObjeto.GetComponentInChildren<Text>();
            if (textoLegacy != null) 
            {
                textoLegacy.text = textoFinal;
            }
            else
            {
                var textoTMP = btnObjeto.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (textoTMP != null) textoTMP.text = textoFinal;
            }
            
            Button btnCargar = btnObjeto.GetComponent<Button>();
            if (btnCargar != null)
            {
                btnCargar.onClick.AddListener(() => {
                    GameSession.dbActiva = nombreArchivo;
                    GameSession.nombrePartidaActiva = nombreMostrar;
                    SceneManager.LoadScene("GameScene");
                });
            }

            Transform hijoBorrar = btnObjeto.transform.Find("BtnBorrar");
            if (hijoBorrar != null)
            {
                Button btnBorrar = hijoBorrar.GetComponent<Button>();
                if (btnBorrar != null)
                {
                    btnBorrar.onClick.AddListener(() => {
                        if (File.Exists(archivo)) File.Delete(archivo);
                        PlayerPrefs.DeleteKey("DisplayName_" + nombreArchivo);
                        PlayerPrefs.DeleteKey("Date_" + nombreArchivo);
                        PlayerPrefs.Save();
                        
                        Destroy(btnObjeto); 
                        
                        if (Directory.GetFiles(Application.persistentDataPath, prefijoGuardado + "*.db").Length == 0)
                        {
                            if (botonAbrirMenuCargar != null) botonAbrirMenuCargar.gameObject.SetActive(false);
                            VolverAlMenuPrincipal();
                        }
                    });
                }
            }
        }
    }

    public void VolverAlMenuPrincipal()
    {
        panelNuevaPartida.SetActive(false);
        panelCargarPartida.SetActive(false);
        panelPrincipal.SetActive(true);
    }

    public void SalirDelJuego()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}