using UnityEngine;

public class PuertaMazmorra : MonoBehaviour
{
    public int idSalaDestino;
    public int destinoX;
    public int destinoY;

    void Start()
    {
        ControladorSala sala = GetComponentInParent<ControladorSala>();
        if (sala != null)
        {
            int gridX = Mathf.FloorToInt(transform.localPosition.x);
            int gridY = Mathf.FloorToInt(-transform.localPosition.y); // Se invierte para la lógica
            
            sala.RegistrarPuerta(gridX, gridY, this);
        }
    }
}