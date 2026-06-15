using UnityEngine;
using SQLite4Unity3d;
using System.Collections.Generic;
using System.Linq;

public class SQLManager : MonoBehaviour
{
    private SQLiteConnection connection;

    [Header("Reiniciar DB")]
    [Tooltip("Activar para forzar a hacer una base de datos limpia desde cero en streamingassets")]
    public bool forzarReinicioBD = true;

    void Awake()
    {
        if (string.IsNullOrEmpty(GameSession.dbActiva)) GameSession.dbActiva = "Partida_Test_Debug.db";

        string dbPath = string.Format("{0}/{1}", Application.streamingAssetsPath, "dragones_y_loyolos.db"); // Tu plantilla original
        string filepath = string.Format("{0}/{1}", Application.persistentDataPath, GameSession.dbActiva);
        
        #if UNITY_EDITOR
        if (forzarReinicioBD && System.IO.File.Exists(filepath) && GameSession.dbActiva == "Partida_Test_Debug.db")
        {
            System.IO.File.Delete(filepath);
            Debug.Log("[SQLManager] BD de test borrada. Cargando limpia.");
        }
        #endif

        if (!System.IO.File.Exists(filepath)) {
            System.IO.File.Copy(dbPath, filepath);
        }

        connection = new SQLiteConnection(filepath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        connection.Execute("CREATE INDEX IF NOT EXISTS idx_entidades_sala ON Entidades_sala_proposito_contenido (id_sala_proposito_contenido, timestep);");
        connection.Execute("CREATE INDEX IF NOT EXISTS idx_entidades_tiempo ON Entidades_sala_proposito_contenido (id_entidades, timestep, subTimestep);");
    }

    // Para evitar que se corrompan datos al cerrar el juego.
    void OnDestroy()
    {
        if (connection != null)
        {
            connection.Close();
        }
    }

    public int ObtenerUltimoTimestep()
    {
        var ultimoTimestep = connection.Query<EntidadesSalaPropositoContenidoSQL>(
            "SELECT timestep FROM Entidades_sala_proposito_contenido ORDER BY timestep DESC LIMIT 1"
        ).FirstOrDefault();
        
        return ultimoTimestep != null ? ultimoTimestep.timestep : 0;
    }

    // NUEVA FUNCIÓN: Lee directamente de la tabla intermedia para encontrar la sala del jugador
    public int ObtenerSalaDelJugador(int timestep, int salaPorDefecto)
    {
        try
        {
            // 1. Obtenemos el ID de la entidad que corresponde al jugador
            var jugador = connection.Query<JugadoresSQL>("SELECT id_entidades FROM Jugadores LIMIT 1").FirstOrDefault();
            
            if (jugador != null)
            {
                // 2. Buscamos en la tabla intermedia en qué sala estaba esa entidad en este timestep
                var ubicacion = connection.Query<EntidadesSalaPropositoContenidoSQL>(
                    "SELECT id_sala_proposito_contenido FROM Entidades_sala_proposito_contenido WHERE id_entidades = ? AND timestep <= ? ORDER BY timestep DESC, subTimestep DESC LIMIT 1",
                    jugador.id_entidades, timestep).FirstOrDefault();

                if (ubicacion != null)
                {
                    return ubicacion.id_sala_proposito_contenido;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[SQLManager] No se pudo obtener la sala del jugador en el timestep. Se usará la por defecto. Error: " + e.Message);
        }
        
        return salaPorDefecto;
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

    public void MoverEntidadASala(int id_entidad, int id_sala, int destX, int destY, int timestep)
    {
        string queryPos = @"INSERT INTO Entidades_sala_proposito_contenido (timestep, subTimestep, id_entidades, id_sala_proposito_contenido, Xpos, Ypos) VALUES (?, ?, ?, ?, ?, ?)";
        connection.Execute(queryPos, timestep, 0, id_entidad, id_sala, destX, destY);
    }

    public void GuardarEstadoMundoActual(List<Entidad> entidades, int idSala, int timestep)
    {
        connection.BeginTransaction();
        try
        {
            foreach (var entidad in entidades)
            {
                if (EsJugador(entidad.id_entidades)) continue;

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
        
        string query = "SELECT id_acciones FROM Acciones_entidades WHERE id_entidades = ?";
        
        try
        {
            var resultados = connection.Query<AccionesEntidadesSQL>(query, idEntidad);

            foreach (var fila in resultados)
            {
                string nombreAccion = fila.id_acciones;
                
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

        // Si la tabla intermedia de acciones está vacía, le damos a la entidad las básicas.
        if (acciones.Count == 0)
        {
            acciones.Add(Acciones.Moverse);
            acciones.Add(Acciones.Atacar);
            acciones.Add(Acciones.Defender);
        }

        return acciones;
    }

    public int ObtenerVidaMaximaDeEntidad(int id_entidad)
    {
        try 
        {
            // Buscamos el HP que tenía la entidad en el momento exacto de su creación (el timestep más antiguo)
            var registro = connection.Query<StatsBaseEntidadesSQL>(
                "SELECT hp FROM Stats_base_entidades WHERE id_entidades = ? ORDER BY timestep ASC, subTimestep ASC LIMIT 1", 
                id_entidad).FirstOrDefault();
                
            if (registro != null) return registro.hp;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SQLManager] Error al obtener la vida máxima de la entidad {id_entidad}: {e.Message}");
        }
        
        return 10;
    }

    public void RollbackATimestep(int targetTimestep)
    {
        connection.Execute("DELETE FROM Entidades_sala_proposito_contenido WHERE timestep > ?", targetTimestep);
        connection.Execute("DELETE FROM Tiempo_acciones_entidades WHERE timestep > ?", targetTimestep);
        connection.Execute("DELETE FROM Stats_base_entidades WHERE timestep > ?", targetTimestep);
    }

    // Busca automáticamente el último turno en el que el jugador estaba vivo
    public int ObtenerUltimoTimestepConVida()
    {
        var jugador = connection.Query<JugadoresSQL>("SELECT id_entidades FROM Jugadores LIMIT 1").FirstOrDefault();
        if (jugador != null)
        {
            var registro = connection.Query<StatsBaseEntidadesSQL>(
                "SELECT timestep FROM Stats_base_entidades WHERE id_entidades = ? AND hp > 0 ORDER BY timestep DESC, subTimestep DESC LIMIT 1",
                jugador.id_entidades).FirstOrDefault();
                
            if (registro != null) return registro.timestep;
        }
        return 1;
    }
}