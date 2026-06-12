using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ComponenteVisual : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void InicializarVisuales(string nombreVisual)
    {
        if (animator == null) return;

        RuntimeAnimatorController overrideController = Resources.Load<RuntimeAnimatorController>($"Animaciones/{nombreVisual}");
        
        if (overrideController != null)
        {
            animator.runtimeAnimatorController = overrideController;
            Debug.Log($"[Visuales] Aspecto de '{nombreVisual}' cargadas con éxito en {gameObject.name}.");
        }
        else
        {
            Debug.LogWarning($"[Visuales] No se encontró 'Animaciones/{nombreVisual}'. Aspecto de seguridad (Error_Override).");
            
            RuntimeAnimatorController errorController = Resources.Load<RuntimeAnimatorController>("Animaciones/Error_Override");
            
            if (errorController != null)
            {
                animator.runtimeAnimatorController = errorController;
            }
            else
            {
                Debug.LogError("[Visuales] No se ha encontrado 'Animaciones/Error_Override' en la carpeta.");
            }
        }
    }
}