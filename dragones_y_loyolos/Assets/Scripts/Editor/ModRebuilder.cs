using UnityEngine;
using UnityEditor;
using SQLite4Unity3d;
using SuperTiled2Unity;
using System.IO;

public class ModRebuilder : EditorWindow
{
    [MenuItem("Modding Tools/Reconstruir Base de Datos de Niveles")]
    public static void RebuildDatabase()
    {
        Debug.Log("[Rebuilder] Iniciando compilación de mapas...");

        string dbPath = Application.streamingAssetsPath + "/dragones_y_loyolos.db";
        var connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite);
        
        connection.Execute("DELETE FROM Entidades_sala_proposito_contenido WHERE timestep = 0");
        connection.Execute("DELETE FROM Sala_proposito_contenido");

        GameObject[] mapas = Resources.LoadAll<GameObject>("Mapas");
        bool jugadorEncontrado = false;
        int entidadesColocadas = 0;
        int salasRegistradas = 0;

        foreach (var mapa in mapas)
        {
            ControladorSala salaImportada = mapa.GetComponent<ControladorSala>();
            if (salaImportada == null) continue;

            connection.Execute(
                "INSERT OR REPLACE INTO Sala_proposito_contenido (id_sala_proposito_contenido, proposito_sala) VALUES (?, ?)", 
                salaImportada.idSalaActual, 
                0 
            );
            salasRegistradas++;
            Debug.Log($"[Rebuilder] Sala {salaImportada.idSalaActual} construida en el SQL.");

            SuperObject[] objetosSalaImportada = mapa.GetComponentsInChildren<SuperObject>();
            
            foreach (var objeto in objetosSalaImportada)
            {
                var propiedadesObjeto = objeto.GetComponent<SuperCustomProperties>();
                if (propiedadesObjeto != null)
                {
                    int idEntidad = -1;
                    
                    if (propiedadesObjeto.TryGetCustomProperty("id_entidades", out CustomProperty p1)) int.TryParse(p1.m_Value, out idEntidad);
                    else if (propiedadesObjeto.TryGetCustomProperty("id_entidad", out CustomProperty p2)) int.TryParse(p2.m_Value, out idEntidad);
                    else if (propiedadesObjeto.TryGetCustomProperty("idEntidad", out CustomProperty p3)) int.TryParse(p3.m_Value, out idEntidad);

                    if (idEntidad > 0)
                    {
                        if (idEntidad == 1) 
                        {
                            if (jugadorEncontrado) 
                            {
                                Debug.LogWarning($"[Rebuilder] Jugador duplicado encontrado en Sala {salaImportada.idSalaActual}. Ignorando...");
                                continue; 
                            }
                            jugadorEncontrado = true;
                        }

                        int gridX = Mathf.FloorToInt(objeto.transform.localPosition.x);
                        int gridY = Mathf.FloorToInt(-objeto.transform.localPosition.y);

                        string query = @"INSERT INTO Entidades_sala_proposito_contenido 
                                        (timestep, subTimestep, id_entidades, id_sala_proposito_contenido, Xpos, Ypos) 
                                        VALUES (?, ?, ?, ?, ?, ?)";
                        connection.Execute(query, 0, 0, idEntidad, salaImportada.idSalaActual, gridX, gridY);
                        
                        entidadesColocadas++;
                    }
                }
            }
        }

        connection.Close();

        string persistentPath = Application.persistentDataPath + "/dragones_y_loyolos.db";
        File.Copy(dbPath, persistentPath, true);

        if (!jugadorEncontrado)
        {
            Debug.LogWarning("<color=red><b>[Rebuilder ERROR] ¡No se encontró ningún Jugador (id_entidades = 1) en ningún mapa!</b></color>\n(Comprueba en Tiled que realmente haya un objeto con la propiedad id_entidades a 1)");
        }
        else
        {
            Debug.Log($"<color=green><b>[Rebuilder ÉXITO] Base de datos reconstruida. Se crearon {salasRegistradas} salas y se colocaron {entidadesColocadas} entidades.</b></color>");
        }
    }
}