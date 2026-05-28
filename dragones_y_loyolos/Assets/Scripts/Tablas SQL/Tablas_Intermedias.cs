using SQLite4Unity3d;

[Table("Stats_base_entidades")]
public class StatsBaseEntidadesSQL {
    public int id_stats_base_entidades { get; set; }
    public int timestep { get; set; }
    public int subTimestep { get; set; }
    public int id_entidades { get; set; }
    public int id_stats_base { get; set; }
    public int hp { get; set; }
    public int ac { get; set; }
    public int fuerza { get; set; }
    public int destreza { get; set; }
    public int constitucion { get; set; }
    public int inteligencia { get; set; }
    public int sabiduria { get; set; }
    public int carisma { get; set; }
}

[Table("Acciones_entidades")]
public class AccionesEntidadesSQL {
    public int id_acciones_entidades { get; set; }
    public int timestep { get; set; }
    public int subTimestep { get; set; }
    public int id_entidades { get; set; }
    public string id_acciones { get; set; }
    public int? id_objeto_usado { get; set; }
    public string id_hechizo_usado { get; set; }
}

[Table("Tiempo_acciones_entidades")]
public class TiempoAccionesEntidadesSQL {
    public int id_tiempo_acciones_entidades { get; set; }
    public int timestep { get; set; }
    public int subTimestep { get; set; }
    public int id_entidades { get; set; }
    public string id_acciones { get; set; }
    public int? id_objeto_usado { get; set; }
    public string id_hechizo_usado { get; set; }
    public int? objetivoX_1 { get; set; }
    public int? objetivoY_1 { get; set; }
    public int? objetivoX_2 { get; set; }
    public int? objetivoY_2 { get; set; }
}

[Table("Entidades_hechizo_base")]
public class EntidadesHechizoBaseSQL {
    public int id_entidades_hechizo_base { get; set; }
    public int timestep { get; set; }
    public int subTimestep { get; set; }
    public int id_entidades { get; set; }
    public string id_hechizo_base { get; set; }
}

[Table("Entidades_objeto_base_hechizo_base")]
public class EntidadesObjetoBaseHechizoBaseSQL {
    public int id_entidades_objeto_base_hechizo_base { get; set; }
    public int timestep { get; set; }
    public int subTimestep { get; set; }
    public int id_entidades { get; set; }
    public int id_objeto_base { get; set; }
    public string id_hechizo_base { get; set; }
}

[Table("Sala_proposito_contenido_evento_asentamiento")]
public class SalaPropositoContenidoEventoAsentamientoSQL {
    public int id_sala_proposito_contenido_evento_asentamiento { get; set; }
    public int timestep { get; set; }
    public int subTimestep { get; set; }
    public int id_sala_proposito_contenido { get; set; }
    public int id_evento_asentamiento { get; set; }
}

[Table("Entidades_sala_proposito_contenido")]
public class EntidadesSalaPropositoContenidoSQL {
    public int id_entidades_sala_proposito_contenido { get; set; }
    public int timestep { get; set; }
    public int subTimestep { get; set; }
    public int id_entidades { get; set; }
    public int id_sala_proposito_contenido { get; set; }
    public int Xpos { get; set; }
    public int Ypos { get; set; }
}

[Table("Hechizo_base_sala_proposito_contenido")]
public class HechizoBaseSalaPropositoContenidoSQL {
    public int id_hechizo_base_sala_proposito_contenido { get; set; }
    public int timestep { get; set; }
    public int subTimestep { get; set; }
    public string id_hechizo_base { get; set; }
    public int id_sala_proposito_contenido { get; set; }
    public int Xpos { get; set; }
    public int Ypos { get; set; }
}

[Table("Objeto_base_hechizo_base_sala_proposito_contenido")]
public class ObjetoBaseHechizoBaseSalaPropositoContenidoSQL {
    public int id_objeto_base_hechizo_base_sala_proposito_contenido { get; set; }
    public int timestep { get; set; }
    public int subTimestep { get; set; }
    public int id_objeto_base { get; set; }
    public string id_hechizo_base { get; set; }
    public int id_sala_proposito_contenido { get; set; }
    public int Xpos { get; set; }
    public int Ypos { get; set; }
}