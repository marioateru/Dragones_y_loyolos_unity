using SQLite4Unity3d;

[Table("Timestep")]
public class TimestepSQL {
    public int timestep { get; set; }
    public int subTimestep { get; set; }
    public float dificultad { get; set; }
}

[Table("Stats_base")]
public class StatsBaseSQL {
    [PrimaryKey] public int id_stats_base { get; set; }
    public int hp { get; set; }
    public short ac { get; set; }
    public short fuerza { get; set; }
    public short destreza { get; set; }
    public short constitucion { get; set; }
    public short inteligencia { get; set; }
    public short sabiduria { get; set; }
    public short carisma { get; set; }
    public short velocidad { get; set; }
}

[Table("Acciones")]
public class AccionesSQL {
    [PrimaryKey] public string id_acciones { get; set; }
}

[Table("Entidades")]
public class EntidadesSQL {
    [PrimaryKey] public int id_entidades { get; set; }
}

[Table("Jugadores")]
public class JugadoresSQL {
    [PrimaryKey] public int id_entidades { get; set; }
    public string id_jugadores { get; set; }
    public string raza { get; set; }
    public short clase { get; set; }
    public short trasfondo { get; set; }
    public short rasgo_de_personalidad { get; set; }
    public short ideal { get; set; }
    public short vinculo { get; set; }
    public short defecto { get; set; }
    public short nivel { get; set; }
}

[Table("Monstruos")]
public class MonstruosSQL {
    [PrimaryKey] public int id_entidades { get; set; }
    public string id_monstruos { get; set; }
    public string tipo { get; set; }
    public string tamanno { get; set; }
    public short desafio { get; set; }
    public string alineamiento { get; set; }
}

[Table("Pnj")]
public class PnjSQL {
    [PrimaryKey] public int id_entidades { get; set; }
    public string id_pnj { get; set; }
    public string apariencia { get; set; }
    public string caracteristica { get; set; }
    public string caracteristica_alta { get; set; }
    public string caracteristica_baja { get; set; }
    public string talento { get; set; }
    public string peculiaridad { get; set; }
    public string interaccion_con_los_demas { get; set; }
    public short ideal { get; set; }
    public string ideal_bueno { get; set; }
    public string ideal_malo { get; set; }
    public string ideal_legal { get; set; }
    public string ideal_caotico { get; set; }
    public string ideal_neutral { get; set; }
    public string ideal_otro { get; set; }
    public string alineamiento { get; set; }
    public string vinculo { get; set; }
    public string defecto { get; set; }
}