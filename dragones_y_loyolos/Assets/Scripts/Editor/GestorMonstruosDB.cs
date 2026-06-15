using UnityEngine;
using UnityEditor;
using SQLite4Unity3d;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class GestorMonstruosDB : EditorWindow
{
    // Sistema de Pestañas
    private int pestannaActual = 0;
    private readonly string[] pestannas = { "Gestor de Monstruos", "Explorador de Tablas" };

    // ==========================================
    // VARIABLES: GESTOR DE MONSTRUOS
    // ==========================================
    private string nombreMonstruo = "Esqueleto";
    private int cantidadACrear = 50;
    private int salaInicial = 1;
    private int hp = 12, ac = 11;
    private int fue = 1, des = 2, con = 1, intel = -1, sab = 0, car = -2;

    private Vector2 scrollPosGestor;
    private List<MonstruoView> listaRelaciones = new List<MonstruoView>();

    private class MonstruoView 
    {
        public int id_entidades { get; set; }
        public string id_monstruos { get; set; }
        public int id_stats_base { get; set; }
        public int hp { get; set; }
        public int ac { get; set; }
        public int id_sala_proposito_contenido { get; set; }
    }

    // ==========================================
    // VARIABLES: EXPLORADOR DE TABLAS
    // ==========================================
    private Vector2 scrollListaTablas;
    private Vector2 scrollInfoTabla;
    private Vector2 scrollDatosVisor;
    
    private List<string> listaTablasBD = new List<string>();
    private string tablaSeleccionada = "";
    private int totalRegistrosTabla = 0;
    private List<ColumnaInfo> infoColumnas = new List<ColumnaInfo>();
    private List<FKInfo> infoFKs = new List<FKInfo>();
    private List<string> registrosTablaActual = new List<string>(); // Aquí guardaremos la muestra de datos

    // Clases molde para los PRAGMA de SQLite y la concatenación
    public class TablaNombre { public string name { get; set; } }
    public class ColumnaInfo { public int cid { get; set; } public string name { get; set; } public string type { get; set; } public int pk { get; set; } }
    public class FKInfo { public int id { get; set; } public string table { get; set; } public string from { get; set; } public string to { get; set; } }
    public class FilaGenerica { public string filaData { get; set; } }

    [MenuItem("Modding Tools/Gestor de Base de Datos")]
    public static void MostrarVentana()
    {
        GestorMonstruosDB ventana = GetWindow<GestorMonstruosDB>("Gestor de BD");
        ventana.minSize = new Vector2(600, 600);
        ventana.CargarBaseDeDatos();
        ventana.CargarListaTablas();
    }

    void OnGUI()
    {
        GUILayout.Space(10);
        pestannaActual = GUILayout.Toolbar(pestannaActual, pestannas, GUILayout.Height(30));
        GUILayout.Space(10);

        if (pestannaActual == 0) MostrarPestannaGestor();
        else if (pestannaActual == 1) MostrarPestannaExplorador();
    }

    private SQLiteConnection AbrirConexion()
    {
        string dbPath = Path.Combine(Application.streamingAssetsPath, "dragones_y_loyolos.db");
        if (!File.Exists(dbPath)) return null;
        return new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite);
    }

    // =========================================================================================
    //                            PESTAÑA 2: EXPLORADOR DE TABLAS Y DATOS
    // =========================================================================================
    private void MostrarPestannaExplorador()
    {
        EditorGUILayout.BeginHorizontal();

        // PANEL IZQUIERDO: Lista interactiva de Tablas
        EditorGUILayout.BeginVertical("box", GUILayout.Width(180));
        if (GUILayout.Button("Recargar Tablas", GUILayout.Height(25))) CargarListaTablas();
        
        GUILayout.Space(5);
        scrollListaTablas = EditorGUILayout.BeginScrollView(scrollListaTablas);
        
        foreach (string tabla in listaTablasBD)
        {
            GUI.backgroundColor = (tablaSeleccionada == tabla) ? new Color(0.6f, 0.8f, 1f) : Color.white;
            if (GUILayout.Button(tabla, EditorStyles.miniButton, GUILayout.Height(20)))
            {
                SeleccionarTabla(tabla);
            }
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        // PANEL DERECHO: Información y VISOR DE DATOS
        EditorGUILayout.BeginVertical("box");
        if (string.IsNullOrEmpty(tablaSeleccionada))
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label("Selecciona una tabla en el menú\nizquierdo para explorar sus datos.", EditorStyles.centeredGreyMiniLabel);
            GUILayout.FlexibleSpace();
        }
        else
        {
            scrollInfoTabla = EditorGUILayout.BeginScrollView(scrollInfoTabla);

            // 1. Cabecera general
            GUILayout.Label($"Tabla: {tablaSeleccionada}", new GUIStyle(EditorStyles.boldLabel) { fontSize = 16 });
            GUILayout.Label($"Total de registros almacenados: {totalRegistrosTabla}", EditorStyles.wordWrappedLabel);
            
            GUILayout.Space(10);

            // 2. Información de Columnas y FKs (Lado a lado)
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.BeginVertical("helpbox", GUILayout.Width(180));
            GUILayout.Label("Columnas:", EditorStyles.boldLabel);
            foreach (var col in infoColumnas)
            {
                string extra = col.pk == 1 ? " <color=yellow>[PK]</color>" : "";
                GUILayout.Label($"• <b>{col.name}</b>{extra}", new GUIStyle(EditorStyles.label) { richText = true });
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("helpbox");
            GUILayout.Label("Conexiones (Foreign Keys):", EditorStyles.boldLabel);
            if (infoFKs.Count == 0) GUILayout.Label("Sin conexiones.", EditorStyles.miniLabel);
            else
            {
                foreach (var fk in infoFKs)
                {
                    GUILayout.Label($"↳ Columna <b>'{fk.from}'</b> apunta a <b>'{fk.to}'</b> en <color=cyan><b>'{fk.table}'</b></color>", new GUIStyle(EditorStyles.label) { richText = true });
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);

            // 3. ¡LA MAGIA!: Visor Dinámico de Datos
            GUILayout.Label("Muestra de Datos (Últimos 50 registros):", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            
            // Un scroll extra horizontal por si la tabla tiene 20 columnas y no caben en pantalla
            scrollDatosVisor = EditorGUILayout.BeginScrollView(scrollDatosVisor, GUILayout.Height(200));

            if (totalRegistrosTabla == 0)
            {
                GUILayout.Label("La tabla está vacía.", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                // Pintamos la cabecera (Los nombres de las columnas separados por líneas)
                string cabecera = string.Join(" │ ", infoColumnas.Select(c => c.name));
                GUILayout.Label(cabecera, new GUIStyle(EditorStyles.boldLabel) { normal = new GUIStyleState() { textColor = new Color(0.3f, 0.8f, 1f) } });
                
                GUILayout.Space(5);

                // Pintamos todas las filas
                foreach (string fila in registrosTablaActual)
                {
                    GUILayout.Label(fila);
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    private void CargarListaTablas()
    {
        var conn = AbrirConexion();
        if (conn == null) return;
        try
        {
            string query = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name";
            listaTablasBD = conn.Query<TablaNombre>(query).Select(t => t.name).ToList();
            if (!listaTablasBD.Contains(tablaSeleccionada)) tablaSeleccionada = "";
        }
        catch (System.Exception e) { Debug.LogError("[Explorador] Error: " + e.Message); }
        finally { conn.Close(); }
    }

    private void SeleccionarTabla(string nombre)
    {
        var conn = AbrirConexion();
        if (conn == null) return;
        try
        {
            tablaSeleccionada = nombre;
            infoColumnas = conn.Query<ColumnaInfo>($"PRAGMA table_info('{nombre}')").ToList();
            infoFKs = conn.Query<FKInfo>($"PRAGMA foreign_key_list('{nombre}')").ToList();
            totalRegistrosTabla = conn.ExecuteScalar<int>($"SELECT COUNT(*) FROM {nombre}");

            // EXTRACTOR GENÉRICO DE DATOS (Concatena columnas en un solo string por fila)
            registrosTablaActual.Clear();
            if (infoColumnas.Count > 0 && totalRegistrosTabla > 0)
            {
                // Usamos COALESCE para evitar que si un campo es nulo (ej: el alineamiento de un monstruo), rompa toda la fila
                var partesColumna = infoColumnas.Select(c => $"COALESCE(CAST({c.name} AS TEXT), 'NULL')");
                string selector = string.Join(" || ' │ ' || ", partesColumna);
                
                string query = $"SELECT {selector} AS filaData FROM {nombre} LIMIT 50";
                
                var filasExtraidas = conn.Query<FilaGenerica>(query);
                foreach (var f in filasExtraidas)
                {
                    registrosTablaActual.Add(f.filaData);
                }
            }
        }
        catch (System.Exception e) { Debug.LogError("[Explorador] Error leyendo tabla: " + e.Message); }
        finally { conn.Close(); }
    }

    // =========================================================================================
    //                            PESTAÑA 1: GESTOR DE MONSTRUOS (Sin cambios)
    // =========================================================================================
    private void MostrarPestannaGestor()
    {
        GUIStyle subtituloStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 12 };

        EditorGUILayout.LabelField("1. Crear Nuevos Monstruos", subtituloStyle);
        EditorGUILayout.BeginVertical("box");
        
        nombreMonstruo = EditorGUILayout.TextField("Nombre (ID Visual)", nombreMonstruo);
        cantidadACrear = EditorGUILayout.IntSlider("Cantidad a instanciar", cantidadACrear, 1, 100);
        salaInicial = EditorGUILayout.IntField("Sala Inicial (Por defecto)", salaInicial);
        
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Bloque de Estadísticas Base:", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        hp = EditorGUILayout.IntField("HP", hp);
        ac = EditorGUILayout.IntField("AC", ac);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        fue = EditorGUILayout.IntField("FUE", fue);
        des = EditorGUILayout.IntField("DES", des);
        con = EditorGUILayout.IntField("CON", con);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        intel = EditorGUILayout.IntField("INT", intel);
        sab = EditorGUILayout.IntField("SAB", sab);
        car = EditorGUILayout.IntField("CAR", car);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Añadir a la Base de Datos", GUILayout.Height(30))) GenerarMonstruos();
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndVertical();

        GUILayout.Space(15);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("2. Visor de Conexiones (SQL)", subtituloStyle);
        if (GUILayout.Button("Refrescar Datos", GUILayout.Width(120))) CargarBaseDeDatos();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("box");
        scrollPosGestor = EditorGUILayout.BeginScrollView(scrollPosGestor, GUILayout.Height(150));

        if (listaRelaciones.Count == 0) EditorGUILayout.LabelField("No hay monstruos en la base de datos (Solo el jugador).");
        else
        {
            foreach (var mon in listaRelaciones)
            {
                EditorGUILayout.BeginVertical("helpbox");
                EditorGUILayout.LabelField($"ID Entidad: {mon.id_entidades} | Clase: {mon.id_monstruos}");
                EditorGUILayout.LabelField($" ↳ Usa Bloque de Stats: {mon.id_stats_base} (HP: {mon.hp}, AC: {mon.ac})");
                EditorGUILayout.LabelField($" ↳ Conectado a Sala: {mon.id_sala_proposito_contenido}");
                EditorGUILayout.EndVertical();
            }
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        GUILayout.Space(15);

        EditorGUILayout.LabelField("3. Zona de Peligro", subtituloStyle);
        EditorGUILayout.BeginVertical("box");
        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("Borrar TODOS los Monstruos y Stats", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Confirmar Borrado", "¿Seguro que quieres vaciar la tabla de monstruos? Esto no borrará al jugador.", "Sí, borrar", "Cancelar"))
            {
                LimpiarBaseDeDatos();
            }
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndVertical();
    }

    private void CargarBaseDeDatos()
    {
        listaRelaciones.Clear();
        var conn = AbrirConexion();
        if (conn == null) return;
        try
        {
            string query = @"
                SELECT m.id_entidades, m.id_monstruos, s.id_stats_base, s.hp, s.ac, e.id_sala_proposito_contenido
                FROM Monstruos m
                LEFT JOIN Stats_base_entidades s ON m.id_entidades = s.id_entidades
                LEFT JOIN Entidades_sala_proposito_contenido e ON m.id_entidades = e.id_entidades
                WHERE s.timestep = 1 AND e.timestep = 1
                ORDER BY m.id_entidades ASC";
            listaRelaciones = conn.Query<MonstruoView>(query).ToList();
        }
        catch (System.Exception e) { Debug.LogError("[Visor SQL] Error leyendo relaciones: " + e.Message); }
        finally { conn.Close(); }
    }

    private void GenerarMonstruos()
    {
        var conn = AbrirConexion();
        if (conn == null) return;
        conn.BeginTransaction();
        try
        {
            int maxEntidad = conn.ExecuteScalar<int>("SELECT MAX(id_entidades) FROM Entidades");
            int maxStats = conn.ExecuteScalar<int>("SELECT MAX(id_stats_base) FROM Stats_base");
            int proximoIdEntidad = (maxEntidad < 1) ? 2 : maxEntidad + 1;
            int proximoIdStats = (maxStats < 1) ? 2 : maxStats + 1;

            conn.Execute(@"INSERT INTO Stats_base 
                (id_stats_base, hp, ac, fuerza, destreza, constitucion, inteligencia, sabiduria, carisma, velocidad) 
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, 6)", proximoIdStats, hp, ac, fue, des, con, intel, sab, car);

            for (int i = 0; i < cantidadACrear; i++)
            {
                int idActual = proximoIdEntidad + i;
                conn.Execute("INSERT INTO Entidades (id_entidades) VALUES (?)", idActual);
                conn.Execute("INSERT INTO Monstruos (id_monstruos, id_entidades) VALUES (?, ?)", nombreMonstruo, idActual);
                conn.Execute(@"INSERT INTO Stats_base_entidades 
                    (timestep, subTimestep, id_entidades, id_stats_base, hp, ac, fuerza, destreza, constitucion, inteligencia, sabiduria, carisma) 
                    VALUES (1, 0, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", idActual, proximoIdStats, hp, ac, fue, des, con, intel, sab, car);
                conn.Execute(@"INSERT INTO Entidades_sala_proposito_contenido 
                    (timestep, subTimestep, id_entidades, id_sala_proposito_contenido, Xpos, Ypos) 
                    VALUES (1, 0, ?, ?, 0, 0)", idActual, salaInicial);
                try {
                    conn.Execute("INSERT INTO Acciones_entidades (id_entidades, id_acciones) VALUES (?, 'Moverse')", idActual);
                    conn.Execute("INSERT INTO Acciones_entidades (id_entidades, id_acciones) VALUES (?, 'Atacar')", idActual);
                } catch {}
            }
            conn.Commit();
            Debug.Log($"<color=green>[SQL]</color> Creados {cantidadACrear} '{nombreMonstruo}'. Usan los IDs del {proximoIdEntidad} al {proximoIdEntidad + cantidadACrear - 1}.");
        }
        catch (System.Exception e) { conn.Rollback(); Debug.LogError("[SQL] Error insertando datos: " + e.Message); }
        finally { conn.Close(); CargarBaseDeDatos(); CargarListaTablas(); }
    }

    private void LimpiarBaseDeDatos()
    {
        var conn = AbrirConexion();
        if (conn == null) return;
        conn.BeginTransaction();
        try
        {
            conn.Execute("DELETE FROM Monstruos");
            conn.Execute("DELETE FROM Entidades WHERE id_entidades > 1");
            conn.Execute("DELETE FROM Stats_base WHERE id_stats_base > 1");
            conn.Execute("DELETE FROM Stats_base_entidades WHERE id_entidades > 1");
            conn.Execute("DELETE FROM Entidades_sala_proposito_contenido WHERE id_entidades > 1");
            conn.Execute("DELETE FROM Tiempo_acciones_entidades WHERE id_entidades > 1");
            try { conn.Execute("DELETE FROM Acciones_entidades WHERE id_entidades > 1"); } catch {}
            conn.Commit();
            Debug.Log("<color=cyan>[SQL]</color> Tablas limpiadas con éxito. Solo queda el Jugador.");
        }
        catch (System.Exception e) { conn.Rollback(); Debug.LogError("[SQL] Error limpiando tablas: " + e.Message); }
        finally { conn.Close(); CargarBaseDeDatos(); CargarListaTablas(); }
    }
}