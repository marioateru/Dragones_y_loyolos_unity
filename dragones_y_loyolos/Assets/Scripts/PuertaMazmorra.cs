using UnityEngine;

public class PuertaMazmorra : MonoBehaviour
{
    [Header("Destino del Viaje")]
    [Tooltip("El ID de la sala en Sala_proposito_contenido a la que vamos")]
    public int idSalaDestino;
    
    [Tooltip("Coordenada X en la nueva sala")]
    public int destinoX;
    
    [Tooltip("Coordenada Y en la nueva sala")]
    public int destinoY;

    // Se activa cuando el jugador camina hacia la casilla de la puerta
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Comprobamos que el que ha pisado la puerta es el jugador
        if (collision.TryGetComponent(out PlayerComponent jugador))
        {
            Debug.Log($"[Puerta] El jugador ha cruzado. Viajando a la sala {idSalaDestino}...");
            FindFirstObjectByType<GameManager>().ViajarAUbicacion(jugador, idSalaDestino, destinoX, destinoY);
        }
    }
}