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

        // Use AnimatorOverrideController. RuntimeAnimatorController no work for overrides well.
        AnimatorOverrideController overrideController = Resources.Load<AnimatorOverrideController>($"Animaciones/{nombreVisual}");
        
        if (overrideController != null)
        {
            animator.runtimeAnimatorController = overrideController;
            animator.Rebind();
            animator.Update(0f);
            Debug.Log($"[Visuales] Aspecto de '{nombreVisual}' cargadas con éxito en {gameObject.name}.");
        }
        else
        {
            Debug.LogWarning($"[Visuales] No se encontró 'Animaciones/{nombreVisual}'. Aspecto de seguridad (Error_Override).");
            
            AnimatorOverrideController errorController = Resources.Load<AnimatorOverrideController>("Animaciones/Error_Override");
            
            if (errorController != null)
            {
                animator.runtimeAnimatorController = errorController;
                animator.Rebind();
                animator.Update(0f);
            }
            else
            {
                Debug.LogError("[Visuales] No se ha encontrado 'Animaciones/Error_Override' en la carpeta.");
            }
        }
    }
}