using UnityEngine;

/// <summary>
/// Comprueba si hay diferentes entidades en área
/// WARNING: este script está obsoleto y en desuso
/// </summary>
public class EnemyInArea : MonoBehaviour
{
    Collider2D highPriorityArea;
    Collider2D lowPriorityArea;
    
    // REVIEW: Puede que ni necesitemos estos métodos, con quizás comprobar distancias sencillas ya lo tenemos todo hecho.
    // REVIEW: le ponemos un debug circle con radio areaDeDetección y ya.
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Entidad entidad))
        {
            if (collision.collider == lowPriorityArea)
            {
                // isRun = true;
            }
            if (collision.collider == highPriorityArea)
            {
              // isRun = true;
              // isHighPriority = true;   
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Entidad entidad))
        {
            if (collision.collider == lowPriorityArea)
            {
                // isRun = false;
            }
            if (collision.collider == highPriorityArea)
            {
              // isRun = true; <-- Revisar esto porque puede que no haga falta.
              // isHighPriority = false;   
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Entidad entidad))
        {
            if (collision.collider == lowPriorityArea)
            {
                // isRun = true;
                // isHighPriority = false;   
            }
            if (collision.collider == highPriorityArea)
            {
              // isRun = true;
              // isHighPriority = false;   
            }
        }
    }
}
