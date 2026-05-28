using UnityEngine;
using SQLite4Unity3d;
using System.Collections.Generic;
using System.Linq;

public class SQLManager : MonoBehaviour
{
    private SQLiteConnection connection;

    void Awake()
    {
        string dbPath = string.Format("{0}/{1}", Application.streamingAssetsPath, "dragones_y_loyolos.db");
        string filepath = string.Format("{0}/{1}", Application.persistentDataPath, "dragones_y_loyolos.db");
        
        if (!System.IO.File.Exists(filepath)) {
            Debug.Log(filepath);
            System.IO.File.Copy(dbPath, filepath);
        }

        connection = new SQLiteConnection(filepath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
    }

    public int ObtenerUltimoTimestep()
    {
        var ultimoTimestep = connection.Query<EntidadesSalaPropositoContenidoSQL>(
            "SELECT timestep FROM Entidades_sala_proposito_contenido ORDER BY timestep DESC LIMIT 1"
        ).FirstOrDefault();
        
        return ultimoTimestep != null ? ultimoTimestep.timestep : 0;
    }

    public void CargarDatosDeEntidad(Entidad entidad, int id_entidad, int timestepInicial)
    {
        var estadoStats = connection.Query<StatsBaseEntidadesSQL>(
            "SELECT * FROM Stats_base_entidades WHERE id_entidades = ? AND timestep <= ? ORDER BY timestep DESC, subTimestep DESC LIMIT 1", 
            id_entidad, timestepInicial).FirstOrDefault();

        if (estadoStats == null) {
            Debug.LogWarning($"[SQLManager] Entidad {id_entidad} sin registro en Stats_base_entidades. Se inyecta un molde vacío por seguridad.");
            estadoStats = new StatsBaseEntidadesSQL();
        }

        var posBD = connection.Query<EntidadesSalaPropositoContenidoSQL>(
            "SELECT Xpos, Ypos FROM Entidades_sala_proposito_contenido WHERE id_entidades = ? AND id_sala_proposito_contenido = 0 ORDER BY timestep DESC, subTimestep DESC LIMIT 1", 
            id_entidad).FirstOrDefault();

        int posX = posBD != null ? posBD.Xpos : 0;
        int posY = posBD != null ? posBD.Ypos : 0;

        entidad.InicializarDatosSQL(id_entidad, estadoStats.id_stats_base, estadoStats, posX, posY);
    }

    public void GuardarHistorialDeAcciones(List<AccionEnMemoria> colaDeAcciones)
    {
        connection.BeginTransaction();
        
        try
        {
            foreach (var accion in colaDeAcciones)
            {
                string queryAccion = @"INSERT INTO Tiempo_acciones_entidades 
                                 (timestep, subTimestep, id_entidades, id_acciones, objetivoX_1, objetivoY_1) 
                                 VALUES (?, ?, ?, ?, ?, ?)";
                connection.Execute(queryAccion, accion.timestep, accion.subTimestep, accion.entidad.id_entidades, accion.tipoAccion.ToString(), accion.objetivoX, accion.objetivoY);
                
                string queryPos = @"INSERT INTO Entidades_sala_proposito_contenido 
                                 (timestep, subTimestep, id_entidades, id_sala_proposito_contenido, Xpos, Ypos) 
                                 VALUES (?, ?, ?, ?, ?, ?)";
                connection.Execute(queryPos, accion.timestep, accion.subTimestep, accion.entidad.id_entidades, 0, accion.entidad.xPos, accion.entidad.yPos);

                string queryStats = @"INSERT INTO Stats_base_entidades 
                                 (timestep, subTimestep, id_entidades, id_stats_base, hp, ac, fuerza, destreza, constitucion, inteligencia, sabiduria, carisma) 
                                 VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
                connection.Execute(queryStats, 
                    accion.timestep, accion.subTimestep, accion.entidad.id_entidades, accion.entidad.id_stats_base,
                    accion.entidad.hp, accion.entidad.ac, accion.entidad.fuerza, accion.entidad.destreza, 
                    accion.entidad.constitucion, accion.entidad.inteligencia, accion.entidad.sabiduria, accion.entidad.carisma);
            }
            
            connection.Commit();
            Debug.Log("[SQLManager] Datos guardados.");
        }
        catch (System.Exception e)
        {
            connection.Rollback();
            Debug.LogError("[SQLManager] Error al guardar los datos: " + e.Message);
        }
    }
}