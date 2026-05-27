using UnityEngine;

[RequireComponent(typeof(TileCollisionChecker))] 
public class EnemyComponent : Entidad
{
    // === CONSTANTES ===
    // Límite de seguridad para evitar bucles infinitos (cuelgues) si la IA se queda acorralada entre muros.
    private const int MAX_INTENTOS_BUSQUEDA = 10;

    private TileCollisionChecker collisionChecker;

    public override void Awake()
    {
        base.Awake(); 
        collisionChecker = GetComponent<TileCollisionChecker>();
    }

    public override void ChooseAction()
    {
        if (IsDead()) 
        {
            SubmitAction(Acciones.Moverse, Mathf.RoundToInt(xPos), Mathf.RoundToInt(yPos));
            return;
        }

        int intentos = 0;
        bool huecoEncontrado = false;
        
        int objetivoX = Mathf.RoundToInt(xPos);
        int objetivoY = Mathf.RoundToInt(yPos);

        // Limitamos el estrés de la CPU utilizando nuestra constante de seguridad
        while (!huecoEncontrado && intentos < MAX_INTENTOS_BUSQUEDA)
        {
            int dx = Random.Range(-1, 2);
            int dy = Random.Range(-1, 2);
            
            int intentoX = Mathf.RoundToInt(xPos) + dx;
            int intentoY = Mathf.RoundToInt(yPos) + dy;

            if (!collisionChecker.HayMuro(intentoX, intentoY))
            {
                objetivoX = intentoX;
                objetivoY = intentoY;
                huecoEncontrado = true;
            }
            
            intentos++;
        }

        if (!huecoEncontrado)
        {
            objetivoX = Mathf.RoundToInt(xPos);
            objetivoY = Mathf.RoundToInt(yPos);
            Debug.Log($"[IA Colisiones] {gameObject.name} agotó sus intentos y pasa turno.");
        }

        SubmitAction(Acciones.Moverse, objetivoX, objetivoY);
    }
}