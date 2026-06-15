using UnityEngine;

public class PuertaMazmorra : MonoBehaviour
{
    private const float TILE_CENTER_OFFSET = 0;
    public int idSalaDestino;
    public int destinoX;
    public int destinoY;
    public int logicX { get; private set; }
    public int logicY { get; private set; }

    void Start()
    {
        ControladorSala sala = GetComponentInParent<ControladorSala>();
        if (sala != null)
        {
            Collider2D col = GetComponent<Collider2D>();

            if (col != null)
            {
                Vector3 localCenter = sala.transform.InverseTransformPoint(col.bounds.center);
                logicX = Mathf.FloorToInt(localCenter.x);
                logicY = Mathf.FloorToInt(-(localCenter.y + TILE_CENTER_OFFSET));
            }
            else
            {
                logicX = Mathf.FloorToInt(transform.localPosition.x + 0.1f);
                logicY = Mathf.RoundToInt(Mathf.Abs(transform.localPosition.y)) - 1;
            }
            
            sala.RegistrarPuerta(logicX, logicY, this);
            Debug.Log($"<color=cyan>[Puerta]</color> Registrada en casilla: ({logicX}, {logicY})");
        }

        Collider2D colToDestroy = GetComponent<Collider2D>();
        if (colToDestroy != null) Destroy(colToDestroy);
        
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) Destroy(rb);
    }
}