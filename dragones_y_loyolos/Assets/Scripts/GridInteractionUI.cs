using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GridInteractionUI : MonoBehaviour
{
    [Header("Referencias Visuales (Mundo)")]
    public GameObject cursorPrefabAnimado; 
    public Material materialLinea;

    [Header("Referencias UI (Canvas)")]
    public GameObject panelMenuUI; 
    public Transform contenedorBotones; 
    public GameObject botonPlantilla; 

    private GameObject instanciaCursor;
    private LineRenderer lineaRuta;
    private List<GameObject> botonesInstanciados = new List<GameObject>();

    public void Inicializar()
    {
        // 1. SALVAVIDAS: Si no hay EventSystem, los botones de Unity NO FUNCIONAN. Lo creamos.
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            GameObject evtObj = new GameObject("EventSystem_GeneradoAutomaticamente");
            evtObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            evtObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        if (cursorPrefabAnimado != null && instanciaCursor == null)
        {
            instanciaCursor = Instantiate(cursorPrefabAnimado);
            instanciaCursor.SetActive(false);
            SpriteRenderer sr = instanciaCursor.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = 50;
        }

        if (lineaRuta == null)
        {
            GameObject lineaObj = new GameObject("LineaRuta_UI");
            lineaRuta = lineaObj.AddComponent<LineRenderer>();
            lineaRuta.positionCount = 2;
            lineaRuta.startWidth = 0.05f;
            lineaRuta.endWidth = 0.05f;
            if (materialLinea != null) lineaRuta.material = materialLinea;
            lineaRuta.sortingOrder = 50;
            lineaRuta.gameObject.SetActive(false);
        }

        if (botonPlantilla != null) botonPlantilla.SetActive(false); 
        if (panelMenuUI != null) panelMenuUI.SetActive(false);
    }

    public void ActivarSeleccionIzquierda(int x, int y, bool esInvalido)
    {
        if (instanciaCursor != null)
        {
            instanciaCursor.SetActive(true);
            instanciaCursor.transform.position = new Vector3(x + 0.5f, -y + 0.5f, 0f);
            SpriteRenderer sr = instanciaCursor.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = esInvalido ? Color.red : Color.white;
        }
        if (lineaRuta) lineaRuta.gameObject.SetActive(false);
        if (panelMenuUI) panelMenuUI.SetActive(false);
    }

    public void ActivarMenuDerecho(int x, int y, Vector2 origenVisual, List<Acciones> acciones, bool esInvalido, System.Action<Acciones> callback)
    {
        if (instanciaCursor != null)
        {
            instanciaCursor.SetActive(true);
            instanciaCursor.transform.position = new Vector3(x + 0.5f, -y + 0.5f, 0f);
            SpriteRenderer sr = instanciaCursor.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = esInvalido ? Color.red : Color.white;
        }

        if (lineaRuta != null)
        {
            lineaRuta.gameObject.SetActive(true);
            lineaRuta.SetPosition(0, new Vector3(origenVisual.x, origenVisual.y, 0f));
            lineaRuta.SetPosition(1, new Vector3(x + 0.5f, -y + 0.5f, 0f));
            lineaRuta.startColor = esInvalido ? Color.red : Color.white;
            lineaRuta.endColor = esInvalido ? Color.red : Color.white;
        }

        if (panelMenuUI == null || botonPlantilla == null) return;

        // 2. POSICIONAMIENTO PERFECTO: Da igual cómo tengas tu Canvas configurado. Esto lo clava al ratón.
        panelMenuUI.SetActive(true);
        Canvas canvasRaiz = panelMenuUI.GetComponentInParent<Canvas>();
        RectTransform panelRect = panelMenuUI.GetComponent<RectTransform>();
        panelRect.pivot = new Vector2(0, 1);

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Camera camaraUI = canvasRaiz.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            panelMenuUI.transform.parent.GetComponent<RectTransform>(),
            mouseScreenPos,
            camaraUI,
            out Vector2 localPoint
        );
        panelRect.localPosition = localPoint;

        // 3. LIMPIEZA Y GENERACIÓN DE BOTONES
        foreach (var b in botonesInstanciados) Destroy(b);
        botonesInstanciados.Clear();

        foreach (var acc in acciones)
        {
            Acciones accionFijada = acc; // CRÍTICO: Previene un bug de memoria de C# al generar los botones
            
            GameObject nuevoBoton = Instantiate(botonPlantilla, contenedorBotones);
            nuevoBoton.SetActive(true);

            var textoUI = nuevoBoton.GetComponentInChildren<Text>();
            if (textoUI != null) textoUI.text = accionFijada.ToString();
            else
            {
                var tmpro = nuevoBoton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (tmpro != null) tmpro.text = accionFijada.ToString();
            }

            Button btnComponent = nuevoBoton.GetComponent<Button>();
            if (btnComponent != null)
            {
                // Ahora sí o sí enviará la acción que acabas de clicar al GameManager
                btnComponent.onClick.AddListener(() => {
                    callback?.Invoke(accionFijada);
                });
            }

            botonesInstanciados.Add(nuevoBoton);
        }
    }

    public void DibujarLinea(Vector2 inicio, Vector2 fin, bool esInvalido)
    {
        if (lineaRuta != null)
        {
            lineaRuta.gameObject.SetActive(true);
            lineaRuta.SetPosition(0, new Vector3(inicio.x, inicio.y, 0f));
            lineaRuta.SetPosition(1, new Vector3(fin.x, fin.y, 0f));
            lineaRuta.startColor = esInvalido ? Color.red : Color.white;
            lineaRuta.endColor = esInvalido ? Color.red : Color.white;
        }
    }

    public void MarcarFueraDeRango()
    {
        if (instanciaCursor != null)
        {
            SpriteRenderer sr = instanciaCursor.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.red;
        }
        if (lineaRuta != null)
        {
            lineaRuta.startColor = Color.red;
            lineaRuta.endColor = Color.red;
        }
        if (panelMenuUI != null) panelMenuUI.SetActive(false); 
    }

    public void OcultarTodo()
    {
        if (instanciaCursor) instanciaCursor.SetActive(false);
        if (lineaRuta) lineaRuta.gameObject.SetActive(false);
        if (panelMenuUI) panelMenuUI.SetActive(false);
    }
}