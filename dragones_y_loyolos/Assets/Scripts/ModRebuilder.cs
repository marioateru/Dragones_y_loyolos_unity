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
                if (propiedadesObjeto != null && propiedadesObjeto.TryGetCustomProperty("id_entidades", out CustomProperty pId))
                {
                    if (int.TryParse(pId.m_Value, out int idEntidad))
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

                        // CORRECCIÓN: Conversión correcta del espacio local negativo de ST2U a enteros positivos de Grid
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
            Debug.LogError("<color=red><b>[Rebuilder ERROR] ¡No se encontró ningún Jugador (id_entidades = 1) en ningún mapa!</b></color>");
        }
        else
        {
            Debug.Log($"<color=green><b>[Rebuilder ÉXITO] Base de datos reconstruida. Se crearon {salasRegistradas} salas y se colocaron {entidadesColocadas} entidades.</b></color>");
        }
    }
}