using UnityEngine;
using UnityEditor;
using SuperTiled2Unity.Editor;
using SuperTiled2Unity;
using UnityEngine.Tilemaps;

[AutoCustomTmxImporter]
public class ImportadorMazmorras : CustomTmxImporter
{
    public override void TmxAssetImported(TmxAssetImportedArgs args)
    {
        // 1. Obtenemos el GameObject raíz del mapa que ST2U acaba de generar
        GameObject mapaRaiz = args.ImportedSuperMap.gameObject;

        // 2. Leemos las propiedades personalizadas
        SuperCustomProperties propiedades = args.ImportedSuperMap.GetComponent<SuperCustomProperties>();
        
        if (propiedades != null)
        {
            // CORRECCIÓN 1: Usamos el método nativo de ST2U para buscar la propiedad
            if (propiedades.TryGetCustomProperty("idSala", out CustomProperty propiedadId))
            {
                // ¡Bingo! Es una mazmorra. Le añadimos el script automáticamente
                ControladorSala controlador = mapaRaiz.AddComponent<ControladorSala>();
                
                // CORRECCIÓN 2: ST2U guarda el valor como string en 'm_Value'. Lo parseamos a Int.
                if (int.TryParse(propiedadId.m_Value, out int idParseado))
                {
                    controlador.idSalaActual = idParseado;
                }
                else
                {
                    Debug.LogWarning($"[Auto-Importador] Cuidado: La propiedad 'idSala' en '{mapaRaiz.name}' no es un número válido. Se asignará 0 por defecto.");
                }

                // 3. Buscamos automáticamente la capa de colisiones para asignarla
                Tilemap[] todosLosTilemaps = mapaRaiz.GetComponentsInChildren<Tilemap>();
                foreach (Tilemap tilemap in todosLosTilemaps)
                {
                    if (tilemap.gameObject.name == "Walls")
                    {
                        controlador.tilemapMuros = tilemap;
                        break;
                    }
                }

                Debug.Log($"[Auto-Importador] El mapa '{mapaRaiz.name}' se configuró automáticamente como la Sala {controlador.idSalaActual}.");
            }
        }
    }
}