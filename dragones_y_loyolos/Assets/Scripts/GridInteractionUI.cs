using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GridInteractionUI : MonoBehaviour
{
    [Header("Referencias Visuales (Mundo)")]
    public GameObject cursorPrefabAnimado; 
    public Material materialLinea;

    [Header("Referencias UI (Prefabricados)")]
    public Canvas canvasPrincipal; 
    public GameObject prefabMenuContenedor; 
    public GameObject prefabBoton; 

    private GameObject instanciaCursor;
    private LineRenderer lineaRuta;
    
    private GameObject menuInstanciado; 
    private Transform contenedorBotonesInstanciado;

    [SerializeField]
    private List<GameObject> poolBotones = new List<GameObject>();

    public void Inicializar()
    {
        // Si no hay EventSystem, lo creamos
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            GameObject objetoEventSystem = new GameObject("EventSystem_GeneradoAutomaticamente");

            objetoEventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();

            objetoEventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Si detecta un cursor animado, lo instancia y accede al SpriteRenderer.
        if (cursorPrefabAnimado != null && instanciaCursor == null)
        {
            instanciaCursor = Instantiate(cursorPrefabAnimado);
            
            instanciaCursor.SetActive(false);
            
            SpriteRenderer spriteRenderer = instanciaCursor.GetComponent<SpriteRenderer>();

            if (spriteRenderer != null) spriteRenderer.sortingOrder = 50;
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

        if (canvasPrincipal != null && prefabMenuContenedor != null)
        {
            menuInstanciado = Instantiate(prefabMenuContenedor, canvasPrincipal.transform);

            menuInstanciado.SetActive(false);
            
            var layout = menuInstanciado.GetComponentInChildren<VerticalLayoutGroup>();

            contenedorBotonesInstanciado = layout != null ? layout.transform : menuInstanciado.transform;
        }
    }

    // Selecciona una casilla al hacer clic izquierdo.
    public void ActivarSeleccionIzquierda(int x, int y, bool esInvalido)
    {
        if (instanciaCursor != null)
        {
            instanciaCursor.SetActive(true);

            instanciaCursor.transform.position = new Vector3(x + 0.5f, -y - 0.5f, 0f);

            SpriteRenderer sr = instanciaCursor.GetComponent<SpriteRenderer>();

            if (sr != null) sr.color = esInvalido ? Color.red : Color.white;
        }
        if (lineaRuta) lineaRuta.gameObject.SetActive(false);
        
        OcultarMenu();
    }

    // Muestra las acciones al seleccionar con clic derecho. 
    public void ActivarMenuDerecho(int x, int y, Vector2 origenVisual, List<Acciones> acciones, bool esInvalido, System.Action<Acciones> callback)
    {
        if (instanciaCursor != null)
        {
            instanciaCursor.SetActive(true);

            instanciaCursor.transform.position = new Vector3(x + 0.5f, -y - 0.5f, 0f);

            SpriteRenderer spriteRenderer = instanciaCursor.GetComponent<SpriteRenderer>();

            if (spriteRenderer != null) spriteRenderer.color = esInvalido ? Color.red : Color.white;
        }

        if (lineaRuta != null)
        {
            lineaRuta.gameObject.SetActive(true);

            lineaRuta.SetPosition(0, new Vector3(origenVisual.x, origenVisual.y, 0f));
            lineaRuta.SetPosition(1, new Vector3(x + 0.5f, -y - 0.5f, 0f));

            lineaRuta.startColor = esInvalido ? Color.red : Color.white;
            lineaRuta.endColor = esInvalido ? Color.red : Color.white;
        }

        if (menuInstanciado == null || prefabBoton == null) return;

        menuInstanciado.SetActive(true);

        RectTransform rectTransformMenu = menuInstanciado.GetComponent<RectTransform>();

        // El menú con los botones nace desde el lugar donde hace clic el jugador hacia abajo.
        rectTransformMenu.pivot = new Vector2(0, 1);
        rectTransformMenu.position = Mouse.current.position.ReadValue();

        foreach (var boton in poolBotones) boton.SetActive(false);

        // Instancia los botones automáticamente, tomando solo una plantilla
        for (int i = 0; i < acciones.Count; i++)
        {
            Acciones accionFijada = acciones[i];

            GameObject botonActual;

            if (i < poolBotones.Count) botonActual = poolBotones[i];
            else
            {
                botonActual = Instantiate(prefabBoton, contenedorBotonesInstanciado);
                poolBotones.Add(botonActual);
            }

            botonActual.SetActive(true);

            var textoUI = botonActual.GetComponentInChildren<Text>();
            
            if (textoUI != null) textoUI.text = accionFijada.ToString();
            else
            {
                var tmpro = botonActual.GetComponentInChildren<TMPro.TextMeshProUGUI>();

                if (tmpro != null) tmpro.text = accionFijada.ToString();
            }

            Button btnComponent = botonActual.GetComponent<Button>();
            if (btnComponent != null)
            {
                btnComponent.onClick.RemoveAllListeners(); 

                // Cuando se presiona el botón, manda la acción.
                btnComponent.onClick.AddListener(() => {
                    callback?.Invoke(accionFijada);
                });
            }
        }
    }

    // Dibuja una línea de un punto A a uno B. Si es válido: blanco; si no: rojo.
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
    }

    public void OcultarTodo()
    {
        if (instanciaCursor) instanciaCursor.SetActive(false);
        if (lineaRuta) lineaRuta.gameObject.SetActive(false);

        OcultarMenu();
    }

    private void OcultarMenu()
    {
        if (menuInstanciado != null) menuInstanciado.SetActive(false);
    }
}