using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ComponenteVisual : MonoBehaviour
{
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    // Recibe el nombre desde el GameManager y se encarga de todo lo gráfico
    public void InicializarVisuales(string nombreVisual)
    {
        if (anim == null) return;

        // 1. Intentamos cargar el controlador de animación específico
        RuntimeAnimatorController overrideController = Resources.Load<RuntimeAnimatorController>($"Animaciones/{nombreVisual}");
        
        if (overrideController != null)
        {
            anim.runtimeAnimatorController = overrideController;
            Debug.Log($"[Visuales] Animaciones de '{nombreVisual}' cargadas con éxito en {gameObject.name}.");
        }
        else
        {
            // 2. Fallback: No existe en Resources, cargamos el de error
            Debug.LogWarning($"[Visuales] No se encontró 'Animaciones/{nombreVisual}'. Cargando el override de seguridad (Error_Override).");
            
            RuntimeAnimatorController errorController = Resources.Load<RuntimeAnimatorController>("Animaciones/Error_Override");
            
            if (errorController != null)
            {
                anim.runtimeAnimatorController = errorController;
            }
            else
            {
                Debug.LogError("[Visuales] ERROR CRÍTICO: Tampoco se ha encontrado 'Animaciones/Error_Override' en la carpeta Resources. ¡Créalo!");
            }
        }
    }
}