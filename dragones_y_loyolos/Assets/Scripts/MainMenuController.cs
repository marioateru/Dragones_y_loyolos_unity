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
    public TMP_InputField inputNombrePartida; // Si usas TMPro Input, cámbialo a TMPro.TMP_InputField
    
    [Header("UI Cargar")]
    public Transform contenedorPartidas;
    public GameObject prefabBotonPartida;
    
    [Tooltip("Arrastra aquí el botón de 'Cargar Partida' del panel principal para ocultarlo si no hay partidas")]
    public Button btnAbrirMenuCargar; 

    private string prefijoGuardado = "Save_";

    void Start()
    {
        Time.timeScale = 1; 
        
        string[] archivosBD = Directory.GetFiles(Application.persistentDataPath, prefijoGuardado + "*.db");
        if (btnAbrirMenuCargar != null)
        {
            btnAbrirMenuCargar.gameObject.SetActive(archivosBD.Length > 0);
        }
    }

    public void MostrarNuevaPartida()
    {
        panelPrincipal.SetActive(false);
        panelNuevaPartida.SetActive(true);
    }

    public void ConfirmarNuevaPartida()
    {
        string nombre = inputNombrePartida.text;
        if (string.IsNullOrWhiteSpace(nombre)) 
        {
            nombre = "D&L " + DateTime.Now.ToString("yyyy-MM-dd");
        }

        string nombreFisicoBD = prefijoGuardado + DateTime.Now.Ticks + ".db";
        string plantillaPath = Path.Combine(Application.streamingAssetsPath, "dragones_y_loyolos.db");
        string nuevoPath = Path.Combine(Application.persistentDataPath, nombreFisicoBD);
        
        File.Copy(plantillaPath, nuevoPath);

        PlayerPrefs.SetString("DisplayName_" + nombreFisicoBD, nombre);
        PlayerPrefs.SetString("Date_" + nombreFisicoBD, DateTime.Now.ToString("dd/MM/yyyy - HH:mm:ss"));
        PlayerPrefs.Save();

        GameSession.dbActiva = nombreFisicoBD;
        GameSession.nombrePartidaActiva = nombre;
        SceneManager.LoadScene("GameScene"); 
    }

    public void MostrarCargarPartida()
    {
        panelPrincipal.SetActive(false);
        panelCargarPartida.SetActive(true);

        // Desconectamos los hijos viejos antes de destruirlos para evitar que la UI se solape
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
            
            // LA MAGIA ANTICRASHES: Soportamos tanto UI clásica como TextMeshPro
            string textoFinal = $"{nombreMostrar}\n<size=12>{fechaGuardado}</size>";
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
            
            // Botón principal de Cargar
            Button btnCargar = btnObjeto.GetComponent<Button>();
            if (btnCargar != null)
            {
                btnCargar.onClick.AddListener(() => {
                    GameSession.dbActiva = nombreArchivo;
                    GameSession.nombrePartidaActiva = nombreMostrar;
                    SceneManager.LoadScene("GameScene");
                });
            }

            // Sub-Botón de Borrar
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
                            if (btnAbrirMenuCargar != null) btnAbrirMenuCargar.gameObject.SetActive(false);
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