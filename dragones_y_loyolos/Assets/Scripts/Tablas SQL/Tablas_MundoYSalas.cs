using SQLite4Unity3d;

[Table("Asentamiento_generador")]
public class AsentamientoGeneradorSQL {
    [PrimaryKey] public int id_asentamiento_generador { get; set; }
    public int dimMaxX { get; set; }
    public int dimMaxY { get; set; }
    public bool isDungeon { get; set; }
    public string ubicacion { get; set; }
    public string ubicacion_exotica { get; set; }
    public string creador { get; set; }
    public string secta_grupo_religioso { get; set; }
    public string alineamiento_creador { get; set; }
    public string clase_creador { get; set; }
    public string proposito_asentamiento { get; set; }
    public string historia_asentamiento { get; set; }
    public string relaciones_entre_razas { get; set; }
    public string gobernante { get; set; }
    public string caracs_destacadas { get; set; }
    public string conocido_por { get; set; }
    public string desgracia_actual { get; set; }
    public short cantidad_edificios_aleatorios { get; set; }
    public short tipo_edificios_aleatorios { get; set; }
    public string residencia { get; set; }
    public string edificio_religioso { get; set; }
    public string taberna { get; set; }
    public string primer_nombre_taberna { get; set; }
    public string segundo_nombre_taberna { get; set; }
    public string almacen { get; set; }
    public string tienda { get; set; }
    public string mazmorra { get; set; }
    public string encuentros_urbanos_aleatorios { get; set; }
}

[Table("Sala_generada")]
public class SalaGeneradaSQL {
    [PrimaryKey] public int id_sala_generada { get; set; }
    public int shapeX { get; set; }
    public int shapeY { get; set; }
    public int num_pisos { get; set; }
    public int num_salas { get; set; }
    public int num_pasillos { get; set; }
    public int num_puertas { get; set; }
    public string zona_inicial { get; set; }
    public string tipo_pasillo { get; set; }
    public int ancho_pasillo { get; set; }
    public string tipo_puerta { get; set; }
    public string tras_puerta { get; set; }
    public string sala { get; set; }
    public string tipo_sala { get; set; }
    public int tipos_salida_sala { get; set; }
    public string sala_normal { get; set; }
    public string sala_grande { get; set; }
    public int coordX_salida { get; set; }
    public int coordY_salida { get; set; }
    public string tipo_salida { get; set; }
    public string tipo_escaleras { get; set; }
}

[Table("Evento_asentamiento")]
public class EventoAsentamientoSQL {
    [PrimaryKey] public int id_evento_asentamiento { get; set; }
    public bool tipo { get; set; }
    public short gravedad { get; set; }
    public short activador { get; set; }
    public string efecto { get; set; }
    public short cd { get; set; }
    public short bonificador_ataque { get; set; }
    public short gravedad_danno_nivel_personaje { get; set; }
    public int timestep_duration { get; set; }
    public int subTimestep_duration { get; set; }
}

[Table("Sala_proposito_contenido")]
public class SalaPropositoContenidoSQL {
    [PrimaryKey] public int id_sala_proposito_contenido { get; set; }
    public int proposito_sala { get; set; }
    public string almacen_tesoros { get; set; }
    public string fortaleza { get; set; }
    public string guarida { get; set; }
    public string laberinto { get; set; }
    public string mausoleo { get; set; }
    public string mina { get; set; }
    public string portal { get; set; }
    public string templo { get; set; }
    public string trampa { get; set; }
    public string salas_genericas { get; set; }
    public string estado_sala { get; set; }
    public string contenido_sala { get; set; }
    public int mobiliario { get; set; }
    public string objetos_en_contenedor { get; set; }
    public string peligro_aleatorio { get; set; }
    public string obstaculo { get; set; }
    public string objeto_atimania { get; set; }
    public string artimania { get; set; }
    public string libros_pergaminos_etc { get; set; }
    public string ruidos { get; set; }
    public string aire { get; set; }
    public string aroma { get; set; }
    public string detalle_general { get; set; }
}

[Table("Asentamiento_generador_sala_generada")]
public class AsentamientoGeneradorSalaGeneradaSQL {
    [PrimaryKey] public int id_asentamiento_generador_sala_generada { get; set; }
    public int id_sala_generada { get; set; }
    public int id_asentamiento_generador { get; set; }
}

[Table("Sala_generada_sala_proposito_contenido")]
public class SalaGeneradaSalaPropositoContenidoSQL {
    [PrimaryKey] public int id_sala_generada_sala_proposito_contenido { get; set; }
    public int id_sala_generada { get; set; }
    public int id_sala_proposito_contenido { get; set; }
}