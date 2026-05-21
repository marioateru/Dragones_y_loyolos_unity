using UnityEngine;
using SQLite4Unity3d;

// TODO: Resto de tablas
[Table("Stats_base")]
public class Stats_baseSQL
{
    [PrimaryKey]
    public int id_stats_base {get; set;}
    public int hp {get; set;}
    public int ac {get; set;}
    public int fuerza {get; set;}
    public int destreza {get; set;}
    public int constitucion {get; set;}
    public int inteligencia {get; set;}
    public int sabiduria {get; set;}
    public int carisma {get; set;}
    public int velocidad {get; set;}
}

[Table("Acciones")]
public class AccionesSQL
{
    [PrimaryKey]
    public string id_acciones {get; set;}
}

[Table("Entidades")]
public class EntidadesSQL
{
    [PrimaryKey]
    public int id_entidades {get; set;}
}
