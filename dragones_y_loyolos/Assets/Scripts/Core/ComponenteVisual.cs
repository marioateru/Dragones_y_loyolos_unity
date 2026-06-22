using UnityEngine;

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

        // Se carga el animator con el nombre indicado
        AnimatorOverrideController overrideController = Resources.Load<AnimatorOverrideController>($"Animaciones/{nombreVisual}");
        
        if (overrideController != null)
        {
            // Sustitución animator 
            animator.runtimeAnimatorController = overrideController;
            
            // Recarga del animator
            animator.Rebind();
            animator.Update(0f);

            Debug.Log($"[Visuales] Aspecto de '{nombreVisual}' cargadas en {gameObject.name}.");
        }
        else
        {
            Debug.LogWarning($"[Visuales] No se encontró 'Animaciones/{nombreVisual}'. Aspecto de seguridad (Error_Override).");
            
            AnimatorOverrideController errorController = Resources.Load<AnimatorOverrideController>("Animaciones/Error_Override");
            
            // Caso en el que no se encuentre el nombre de la entidad de juego en el SQL.
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