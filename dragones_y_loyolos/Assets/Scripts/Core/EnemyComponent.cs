using UnityEngine;

[RequireComponent(typeof(TileCollisionChecker))] 
public class EnemyComponent : Entidad
{
    // Para que no explote la CPU
    private const int MAX_INTENTOS_BUSQUEDA = 10;
    private TileCollisionChecker collisionChecker;

    public override void Awake()
    {
        base.Awake(); 
        collisionChecker = GetComponent<TileCollisionChecker>();
    }

    public override void ChooseAction()
    {   
        if (!isRun || IsDead())
        {
            SubmitAction(Acciones.Moverse, Mathf.RoundToInt(xPos), Mathf.RoundToInt(yPos));
            return;
        }

        int intentos = 0;
        bool huecoEncontrado = false;
        int objetivoX = Mathf.RoundToInt(xPos);
        int objetivoY = Mathf.RoundToInt(yPos);

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

        SubmitAction(Acciones.Moverse, objetivoX, objetivoY);
    }
}