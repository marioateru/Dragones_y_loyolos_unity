using UnityEngine;
using SuperTiled2Unity.Editor;
using SuperTiled2Unity;
using UnityEngine.Tilemaps;
using SQLite4Unity3d;
using System.IO;

[AutoCustomTmxImporter]
public class ImportadorMazmorras : CustomTmxImporter
{
    public override void TmxAssetImported(TmxAssetImportedArgs args)
    {
        GameObject mapaRaiz = args.ImportedSuperMap.gameObject;
        SuperCustomProperties propiedades = args.ImportedSuperMap.GetComponent<SuperCustomProperties>();
        
        ControladorSala controlador = mapaRaiz.AddComponent<ControladorSala>();
        int idSalaImportada = 1; 

        if (propiedades != null && propiedades.TryGetCustomProperty("idSala", out CustomProperty propiedadId))
        {
            if (int.TryParse(propiedadId.m_Value, out int idParseado)) 
            {
                controlador.idSalaActual = idParseado;
                idSalaImportada = idParseado;
            }
        }

        Tilemap[] tilemapsMapa = mapaRaiz.GetComponentsInChildren<Tilemap>();
        foreach (Tilemap tilemap in tilemapsMapa)
        {
            if (tilemap.gameObject.name == "Muros" || tilemap.gameObject.name == "Walls")
            {
                controlador.tilemapMuros = tilemap;
                break;
            }
        }

        string dbPath = Path.Combine(Application.streamingAssetsPath, "dragones_y_loyolos.db");
        SQLiteConnection connection = null;
        if (File.Exists(dbPath))
        {
            connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite);
            connection.BeginTransaction();
        }

        SuperObject[] objetosTiled = mapaRaiz.GetComponentsInChildren<SuperObject>();
        
        foreach (SuperObject obj in objetosTiled)
        {
            // 1. PUERTAS
            if (obj.m_Type == "Puerta" || obj.m_TiledName == "Puerta")
            {
                GameObject prefabPuerta = Resources.Load<GameObject>("Prefabs/Puerta_Generica");
                if (prefabPuerta != null)
                {
                    GameObject puertaInstanciada = GameObject.Instantiate(prefabPuerta, obj.transform.position, Quaternion.identity);
                    puertaInstanciada.transform.SetParent(obj.transform.parent, true);

                    PuertaMazmorra puertaScript = puertaInstanciada.GetComponent<PuertaMazmorra>();
                    SuperCustomProperties propiedadesPuerta = obj.GetComponent<SuperCustomProperties>();
                    
                    if (propiedadesPuerta != null)
                    {
                        if (propiedadesPuerta.TryGetCustomProperty("idSalaDestino", out CustomProperty pSala))
                            if (int.TryParse(pSala.m_Value, out int val)) puertaScript.idSalaDestino = val;
                        if (propiedadesPuerta.TryGetCustomProperty("destinoX", out CustomProperty pX))
                            if (int.TryParse(pX.m_Value, out int val)) puertaScript.destinoX = val;
                        if (propiedadesPuerta.TryGetCustomProperty("destinoY", out CustomProperty pY))
                            if (int.TryParse(pY.m_Value, out int val)) puertaScript.destinoY = val;
                            
                        puertaInstanciada.name = $"Puerta_A_Sala_{puertaScript.idSalaDestino}";
                    }
                    GameObject.DestroyImmediate(obj.gameObject);
                }
                continue; 
            }
            
            // 2. ENTIDADES
            int idEntidad = -1;
            string propValue = "";
            SuperCustomProperties propiedadesObj = obj.GetComponent<SuperCustomProperties>();
            if (propiedadesObj != null)
            {
                if (propiedadesObj.TryGetCustomProperty("id_entidades", out CustomProperty p1)) propValue = p1.m_Value;
                else if (propiedadesObj.TryGetCustomProperty("id_entidad", out CustomProperty p2)) propValue = p2.m_Value;
                else if (propiedadesObj.TryGetCustomProperty("idEntidad", out CustomProperty p3)) propValue = p3.m_Value;

                if (!string.IsNullOrEmpty(propValue))
                {
                    if (float.TryParse(propValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float fId)) 
                    {
                        idEntidad = Mathf.RoundToInt(fId);
                    }
                }
            }

            if (idEntidad > 0)
            {
                int logicX = Mathf.FloorToInt(obj.transform.position.x);
                int logicY = Mathf.FloorToInt(-obj.transform.position.y);

                if (connection != null)
                {
                    try 
                    {
                        // FIX: Timestep 0 instead of 1
                        int filasActualizadas = connection.Execute(@"UPDATE Entidades_sala_proposito_contenido 
                            SET Xpos = ?, Ypos = ?, id_sala_proposito_contenido = ? 
                            WHERE id_entidades = ? AND timestep = 0", 
                            logicX, logicY, idSalaImportada, idEntidad);

                        if (filasActualizadas > 0)
                        {
                            Debug.Log($"<color=green>[SQL-Tiled]</color> Entidad {idEntidad} movida a ({logicX}, {logicY}) - Sala {idSalaImportada}.");
                        }
                        else
                        {
                            // FIX: Timestep 0 instead of 1
                            connection.Execute(@"INSERT INTO Entidades_sala_proposito_contenido 
                                (timestep, subTimestep, id_entidades, id_sala_proposito_contenido, Xpos, Ypos) 
                                VALUES (0, 0, ?, ?, ?, ?)", 
                                idEntidad, idSalaImportada, logicX, logicY);
                                
                            Debug.Log($"<color=cyan>[SQL-Tiled]</color> Entidad {idEntidad} insertada por primera vez en ({logicX}, {logicY}) - Sala {idSalaImportada}.");
                        }
                    }
                    catch (System.Exception e) { Debug.LogError($"Error SQL en entidad {idEntidad}: {e.Message}"); }
                }

                // FIX: NO DESTROY HERE! Leave object so ModRebuilder can find it later.
                // GameObject.DestroyImmediate(obj.gameObject); 
            }
        }

        if (connection != null)
        {
            connection.Commit();
            connection.Close();
        }
    }
}