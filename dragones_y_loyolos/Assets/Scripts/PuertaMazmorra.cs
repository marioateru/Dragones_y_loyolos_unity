using UnityEngine;

public class PuertaMazmorra : MonoBehaviour
{
    private const float TILE_CENTER_OFFSET = 0;
    public int idSalaDestino;
    public int destinoX;
    public int destinoY;

    void Start()
    {
        ControladorSala sala = GetComponentInParent<ControladorSala>();
        if (sala != null)
        {
            Collider2D col = GetComponent<Collider2D>();
            int gridX = 0, gridY = 0;

            if (col != null)
            {
                Vector3 localCenter = sala.transform.InverseTransformPoint(col.bounds.center);
                gridX = Mathf.FloorToInt(localCenter.x);
                gridY = Mathf.FloorToInt(-(localCenter.y + TILE_CENTER_OFFSET));
            }
            else
            {
                gridX = Mathf.FloorToInt(transform.localPosition.x + 0.1f);
                gridY = Mathf.RoundToInt(Mathf.Abs(transform.localPosition.y)) - 1;
            }
            
            sala.RegistrarPuerta(gridX, gridY, this);
            Debug.Log($"<color=cyan>[Puerta]</color> Registrada EXACTAMENTE en casilla lógica: ({gridX}, {gridY})");
        }

        Collider2D colToDestroy = GetComponent<Collider2D>();
        if (colToDestroy != null) Destroy(colToDestroy);
        
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) Destroy(rb);
    }
}