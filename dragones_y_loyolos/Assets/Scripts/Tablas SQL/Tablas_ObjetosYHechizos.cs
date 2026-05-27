using SQLite4Unity3d;

[Table("Hechizo_base")]
public class HechizoBaseSQL {
    [PrimaryKey] public string id_hechizo_base { get; set; }
    public short nivel { get; set; }
    public short prioridadLanzamiento { get; set; }
    public short alcance { get; set; }
    public string componentes { get; set; }
    public short duracion { get; set; }
    public string escuela { get; set; }
    public short numero_objetivos { get; set; }
    public int? areaX1 { get; set; }
    public int? areaX2 { get; set; }
    public int? areaY1 { get; set; }
    public int? areaY2 { get; set; }
    public string tirada_salvacion { get; set; }
}

[Table("Objeto_base")]
public class ObjetoBaseSQL {
    [PrimaryKey] public int id_objeto_base { get; set; }
    public string nombre { get; set; }
    public string tipo { get; set; }
    public string rareza { get; set; }
    public short peso { get; set; }
    public short capacidad { get; set; }
    public short clase_armadura_objeto { get; set; }
    public short valor { get; set; }
    public short coste { get; set; }
    public short armadura_annadida { get; set; }
    public short requisito_fuerza { get; set; }
    public string sigilo { get; set; }
    public short tiempo_para_poner_quitar { get; set; }
    public string tipo_arma { get; set; }
    public string propiedad_arma { get; set; }
    public short danno { get; set; }
    public short alcance { get; set; }
    public short prioridadAtaque { get; set; }
    public bool encantado { get; set; }
    public string categoria_objeto_magico { get; set; }
    public string quien_fabrico_objeto { get; set; }
    public string historia_objeto { get; set; }
    public string propiedad_menor { get; set; }
    public string peculiaridad { get; set; }
    public bool indestructible { get; set; }
    public string beneficio_menor { get; set; }
    public string beneficio_mayor { get; set; }
    public string perjuicio_menor { get; set; }
    public string perjuicio_mayor { get; set; }
}