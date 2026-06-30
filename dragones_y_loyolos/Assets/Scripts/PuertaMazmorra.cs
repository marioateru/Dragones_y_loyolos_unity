using UnityEngine;

public class PuertaMazmorra : MonoBehaviour
{
    private const float TILE_CENTER_OFFSET = 0;
    public int idSalaDestino;
    public int destinoX;
    public int destinoY;
    public int xPos { get; private set; }
    public int yPos { get; private set; }

    void Start()
    {
        ControladorSala sala = GetComponentInParent<ControladorSala>();
        if (sala != null)
        {
            Collider2D collider2D = GetComponent<Collider2D>();

            // Ajusta el pivote del prefab puerta al centro para alinearse con la cuadrícula.
            if (collider2D != null)
            {
                Vector3 localCenter = sala.transform.InverseTransformPoint(collider2D.bounds.center);

                xPos = Mathf.FloorToInt(localCenter.x);
                yPos = Mathf.FloorToInt(-(localCenter.y + TILE_CENTER_OFFSET));
            }
            else
            {
                xPos = Mathf.FloorToInt(transform.localPosition.x + 0.1f);
                yPos = Mathf.RoundToInt(Mathf.Abs(transform.localPosition.y)) - 1;
            }
            
            sala.RegistrarPuerta(xPos, yPos, this);

            Debug.Log($"<color=cyan>[Puerta]</color> Registrada en casilla: ({xPos}, {yPos})");
        }

        Collider2D collider2DToDestroy = GetComponent<Collider2D>();
        
        if (collider2DToDestroy != null) Destroy(collider2DToDestroy);
    }
}