using UnityEngine;
using SQLite4Unity3d; // Usamos el plugin

// Mapeamos la tabla del jugador para la prueba
[Table("Jugadores")]
public class DatosJugadorSQL {
    [PrimaryKey]
    public int id_entidades { get; set; }
    public string id_jugadores { get; set; }
    public string raza { get; set; }
    public short nivel { get; set; }
}

public class PruebaBaseDatos : MonoBehaviour
{
    void Start()
    {
        // 1. Conectamos a la base de datos que acabas de meter
        string dbPath = string.Format("{0}/{1}", Application.streamingAssetsPath, "dragones_y_loyolos.db");
        
        // Copiamos la DB al almacenamiento persistente si es necesario (como vimos en tu DataService)
        string filepath = string.Format("{0}/{1}", Application.persistentDataPath, "dragones_y_loyolos.db");
        if (!System.IO.File.Exists(filepath)) {
            System.IO.File.Copy(dbPath, filepath);
        }
        Debug.Log(filepath);

        // Abrimos conexión
        var connection = new SQLiteConnection(filepath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

        // 2. Extraemos el jugador que metiste en el script (id_entidades = 1)
        var miJugador = connection.Table<DatosJugadorSQL>().Where(j => j.id_entidades == 1).FirstOrDefault();

        // 3. Comprobamos si ha funcionado
        if (miJugador != null)
        {
            Debug.Log("¡ÉXITO! Base de datos conectada.");
            Debug.Log($"He cargado al jugador: {miJugador.id_jugadores}, Raza: {miJugador.raza}, Nivel: {miJugador.nivel}");
        }
        else
        {
            Debug.LogError("No he encontrado al jugador. Algo ha fallado en la lectura.");
        }
    }
}