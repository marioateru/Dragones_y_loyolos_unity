using UnityEngine;
using SuperTiled2Unity.Editor;
using SuperTiled2Unity;
using UnityEngine.Tilemaps;

[AutoCustomTmxImporter]
public class ImportadorMazmorras : CustomTmxImporter
{
    public override void TmxAssetImported(TmxAssetImportedArgs args)
    {
        GameObject mapaRaiz = args.ImportedSuperMap.gameObject;
        SuperCustomProperties propiedades = args.ImportedSuperMap.GetComponent<SuperCustomProperties>();
        
        if (propiedades != null)
        {
            if (propiedades.TryGetCustomProperty("idSala", out CustomProperty propiedadId))
            {
                ControladorSala controlador = mapaRaiz.AddComponent<ControladorSala>();
                if (int.TryParse(propiedadId.m_Value, out int idParseado)) controlador.idSalaActual = idParseado;

                Tilemap[] tilemapsMapa = mapaRaiz.GetComponentsInChildren<Tilemap>();
                foreach (Tilemap tilemap in tilemapsMapa)
                {
                    if (tilemap.gameObject.name == "Muros" || tilemap.gameObject.name == "Walls")
                    {
                        controlador.tilemapMuros = tilemap;
                        break;
                    }
                }
            }
        }

        SuperObject[] objetosTiled = mapaRaiz.GetComponentsInChildren<SuperObject>();
        
        foreach (SuperObject obj in objetosTiled)
        {
            if (obj.m_Type == "Puerta" || obj.m_TiledName == "Puerta")
            {
                // A) Buscamos la plantilla visual en el disco duro
                GameObject prefabPuerta = Resources.Load<GameObject>("Prefabs/Puerta_Generica");
                
                if (prefabPuerta != null)
                {
                    GameObject puertaInstanciada = GameObject.Instantiate(prefabPuerta, obj.transform.position, Quaternion.identity);
                    
                    puertaInstanciada.transform.SetParent(obj.transform.parent, true);
                    //instanciaPuerta.name = "Puerta_A_Sala_Desconocida";

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
                    
                    // D) Destruimos el cascarón vacío e invisible que había generado Tiled
                    GameObject.DestroyImmediate(obj.gameObject);
                    
                    Debug.Log($"[Auto-Importador] Puerta Visual instanciada hacia Sala {puertaScript.idSalaDestino}.");
                }
                else
                {
                    Debug.LogError("[Auto-Importador] CRÍTICO: No se ha encontrado el prefab de la puerta en 'Resources/Prefabs/Puerta_Generica'.");
                }
            }
        }
    }
}