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
            System.IO.File.Copy(dbPath, filepath);
        }

        connection = new SQLiteConnection(filepath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

        connection.Execute("CREATE INDEX IF NOT EXISTS idx_entidades_sala ON Entidades_sala_proposito_contenido (id_sala_proposito_contenido, timestep);");
        connection.Execute("CREATE INDEX IF NOT EXISTS idx_entidades_tiempo ON Entidades_sala_proposito_contenido (id_entidades, timestep, subTimestep);");
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
            "SELECT Xpos, Ypos FROM Entidades_sala_proposito_contenido WHERE id_entidades = ? AND timestep <= ? ORDER BY timestep DESC, subTimestep DESC LIMIT 1", 
            id_entidad, timestepInicial).FirstOrDefault();

        int posX = posBD != null ? posBD.Xpos : 0;
        int posY = posBD != null ? posBD.Ypos : 0;

        entidad.InicializarDatosSQL(id_entidad, estadoStats.id_stats_base, estadoStats, posX, posY);
    }

    public int ObtenerArquetipoDeEntidad(int id_entidad, int timestep)
    {
        var vinculo = connection.Query<StatsBaseEntidadesSQL>(
            "SELECT id_stats_base FROM Stats_base_entidades WHERE id_entidades = ? AND timestep <= ? ORDER BY timestep DESC, subTimestep DESC LIMIT 1", 
            id_entidad, timestep).FirstOrDefault();

        return vinculo != null ? vinculo.id_stats_base : 1; 
    }

    public List<EntidadesSalaPropositoContenidoSQL> ObtenerEntidadesEnSala(int idSala, int timestep)
    {
        string query = @"
            SELECT e.* FROM Entidades_sala_proposito_contenido e
            INNER JOIN (
                SELECT id_entidades, MAX(timestep * 10000 + subTimestep) as max_time
                FROM Entidades_sala_proposito_contenido
                WHERE timestep <= ?
                GROUP BY id_entidades
            ) latest ON e.id_entidades = latest.id_entidades 
            AND (e.timestep * 10000 + e.subTimestep) = latest.max_time
            WHERE e.id_sala_proposito_contenido = ?";

        return connection.Query<EntidadesSalaPropositoContenidoSQL>(query, timestep, idSala).ToList();
    }

    public bool EsJugador(int id_entidad)
    {
        var jugador = connection.Query<JugadoresSQL>("SELECT id_entidades FROM Jugadores WHERE id_entidades = ? LIMIT 1", id_entidad).FirstOrDefault();
        return jugador != null;
    }

    public string ObtenerNombreEntidad(int id_entidad, bool esJugador)
    {
        if (esJugador) 
        {
            var jug = connection.Query<JugadoresSQL>("SELECT id_jugadores FROM Jugadores WHERE id_entidades = ?", id_entidad).FirstOrDefault();
            return jug != null ? jug.id_jugadores : "JugadorBase";
        } 
        else 
        {
            var mon = connection.Query<MonstruosSQL>("SELECT id_monstruos FROM Monstruos WHERE id_entidades = ?", id_entidad).FirstOrDefault();
            return mon != null ? mon.id_monstruos : "EnemigoBase";
        }
    }

    // CORRECCIÓN: Ahora pide el idSalaActual por parámetro en vez de forzar el 0
    public void GuardarHistorialDeAcciones(List<AccionEnMemoria> colaDeAcciones, int idSalaActual)
    {
        connection.BeginTransaction();
        try
        {
            foreach (var accion in colaDeAcciones)
            {
                string queryAccion = @"INSERT INTO Tiempo_acciones_entidades (timestep, subTimestep, id_entidades, id_acciones, objetivoX_1, objetivoY_1) VALUES (?, ?, ?, ?, ?, ?)";
                connection.Execute(queryAccion, accion.timestep, accion.subTimestep, accion.entidad.id_entidades, accion.tipoAccion.ToString(), accion.objetivoX, accion.objetivoY);
                
                string queryPos = @"INSERT INTO Entidades_sala_proposito_contenido (timestep, subTimestep, id_entidades, id_sala_proposito_contenido, Xpos, Ypos) VALUES (?, ?, ?, ?, ?, ?)";
                connection.Execute(queryPos, accion.timestep, accion.subTimestep, accion.entidad.id_entidades, idSalaActual, Mathf.RoundToInt(accion.entidad.xPos), Mathf.RoundToInt(accion.entidad.yPos));

                string queryStats = @"INSERT INTO Stats_base_entidades (timestep, subTimestep, id_entidades, id_stats_base, hp, ac, fuerza, destreza, constitucion, inteligencia, sabiduria, carisma) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
                connection.Execute(queryStats, accion.timestep, accion.subTimestep, accion.entidad.id_entidades, accion.entidad.id_stats_base, accion.entidad.hp, accion.entidad.ac, accion.entidad.fuerza, accion.entidad.destreza, accion.entidad.constitucion, accion.entidad.inteligencia, accion.entidad.sabiduria, accion.entidad.carisma);
            }
            connection.Commit();
        }
        catch (System.Exception e)
        {
            connection.Rollback();
            Debug.LogError("[SQLManager] Error al guardar los datos: " + e.Message);
        }
    }

    // NUEVO: Movimiento instantáneo entre salas
    public void MoverEntidadASala(int id_entidad, int id_sala, int destX, int destY, int timestep)
    {
        string queryPos = @"INSERT INTO Entidades_sala_proposito_contenido (timestep, subTimestep, id_entidades, id_sala_proposito_contenido, Xpos, Ypos) VALUES (?, ?, ?, ?, ?, ?)";
        connection.Execute(queryPos, timestep, 0, id_entidad, id_sala, destX, destY);
    }

    // NUEVO: Fotografía de la mazmorra que dejas atrás
    public void GuardarEstadoMundoActual(List<Entidad> entidades, int idSala, int timestep)
    {
        connection.BeginTransaction();
        try
        {
            foreach (var entidad in entidades)
            {
                if (EsJugador(entidad.id_entidades)) continue; // El jugador se guarda con MoverEntidadASala

                string queryPos = @"INSERT INTO Entidades_sala_proposito_contenido (timestep, subTimestep, id_entidades, id_sala_proposito_contenido, Xpos, Ypos) VALUES (?, ?, ?, ?, ?, ?)";
                connection.Execute(queryPos, timestep, 0, entidad.id_entidades, idSala, Mathf.RoundToInt(entidad.xPos), Mathf.RoundToInt(entidad.yPos));

                string queryStats = @"INSERT INTO Stats_base_entidades (timestep, subTimestep, id_entidades, id_stats_base, hp, ac, fuerza, destreza, constitucion, inteligencia, sabiduria, carisma) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
                connection.Execute(queryStats, timestep, 0, entidad.id_entidades, entidad.id_stats_base, entidad.hp, entidad.ac, entidad.fuerza, entidad.destreza, entidad.constitucion, entidad.inteligencia, entidad.sabiduria, entidad.carisma);
            }
            connection.Commit();
        }
        catch (System.Exception e)
        {
            connection.Rollback();
            Debug.LogError("[SQLManager] Error al congelar el estado de la mazmorra: " + e.Message);
        }
    }

    public List<Acciones> ObtenerAccionesPermitidas(int idEntidad)
    {
        List<Acciones> acciones = new List<Acciones>();
        
        // Consultamos la tabla usando el molde de datos que YA tienes en Tablas_Intermedias.cs
        string query = "SELECT id_acciones FROM Acciones_entidades WHERE id_entidades = ?";
        
        try
        {
            // Usamos tu clase AccionesEntidadesSQL nativa
            var resultados = connection.Query<AccionesEntidadesSQL>(query, idEntidad);

            foreach (var fila in resultados)
            {
                string nombreAccion = fila.id_acciones;
                
                // Convertimos el texto de la base de datos al Enum de C# de forma segura
                if (System.Enum.TryParse(nombreAccion, true, out Acciones accionParseada))
                {
                    if (!acciones.Contains(accionParseada)) 
                    {
                        acciones.Add(accionParseada);
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SQLManager] Error al cargar acciones de la entidad {idEntidad}: {e.Message}");
        }

        // Fallback de seguridad: si la tabla intermedia está vacía, le damos acciones básicas
        if (acciones.Count == 0)
        {
            acciones.Add(Acciones.Moverse);
            acciones.Add(Acciones.Atacar);
            acciones.Add(Acciones.Defender);
        }

        return acciones;
    }
}