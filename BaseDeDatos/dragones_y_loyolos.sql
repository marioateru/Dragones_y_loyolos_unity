-- CREATE SCHEMA dragones_y_loyolos;
-- USE dragones_y_loyolos;
-- DROP SCHEMA dragones_y_loyolos;

-- TODO: cambiar variables "tinytext" por tipos más sólidos, refactorizar para mayor limpieza, 
-- TODO: conectar la velocidad de lanzamiento de los hechizos a las acciones y añadir el sistema de prioridad.

CREATE TABLE Timestep (
PRIMARY KEY (timestep, subTimestep),

timestep INT,
subTimestep INT,
dificultad FLOAT
);

-- NAMESPACE: ENTE
CREATE TABLE Stats_base (
	id_stats_base INT PRIMARY KEY,
	hp INT,
	ac SMALLINT,
	fuerza SMALLINT,
	destreza SMALLINT,
	constitucion SMALLINT,
	inteligencia SMALLINT,
	sabiduria SMALLINT,
	carisma SMALLINT,
	velocidad SMALLINT -- Esto quizás lo quitemos y dejemos solo la destreza.
);

CREATE TABLE Acciones (
id_acciones VARCHAR(255) PRIMARY KEY -- Nombre de acción, implementada en C# por interfaces.
);

CREATE TABLE Entidades (
id_entidades INT PRIMARY KEY
);

CREATE TABLE Jugadores (
    id_entidades INT PRIMARY KEY,
    
	id_jugadores VARCHAR(255),
	raza TINYTEXT,
	clase SMALLINT,
	trasfondo SMALLINT,
		rasgo_de_personalidad SMALLINT,
		ideal SMALLINT,
		vinculo SMALLINT,
		defecto SMALLINT,
	nivel SMALLINT,
    
    FOREIGN KEY (id_entidades) REFERENCES Entidades (id_entidades)
);

CREATE TABLE Monstruos (
	id_entidades INT PRIMARY KEY,
    
	id_monstruos VARCHAR(255),
	tipo TINYTEXT,
    tamanno TINYTEXT,
    desafio SMALLINT,
    alineamiento TINYTEXT,
    
    FOREIGN KEY (id_entidades) REFERENCES Entidades (id_entidades)
);

CREATE TABLE Pnj (
	id_entidades INT PRIMARY KEY,
    
	id_pnj VARCHAR(255),
    apariencia TINYTEXT,
    caracteristica TINYTEXT,
		caracteristica_alta TINYTEXT,
        caracteristica_baja TINYTEXT,
	talento TINYTEXT,
    peculiaridad TINYTEXT,
    interaccion_con_los_demas TINYTEXT,
    ideal SMALLINT,
		ideal_bueno TINYTEXT,
        ideal_malo TINYTEXT,
        ideal_legal TINYTEXT,
        ideal_caotico TINYTEXT,
        ideal_neutral TINYTEXT,
        ideal_otro TINYTEXT,
	alineamiento Tinytext,
    vinculo TINYTEXT,
    defecto TINYTEXT,
	
    FOREIGN KEY (id_entidades) REFERENCES Entidades (id_entidades)

);

CREATE TABLE Hechizo_base (
 id_hechizo_base VARCHAR(255) PRIMARY KEY,
 nivel SMALLINT,
 prioridadLanzamiento SMALLINT,
 alcance SMALLINT,
 componentes TINYTEXT,
 duracion SMALLINT,
 escuela TINYTEXT,
 
 numero_objetivos SMALLINT,
 areaX1 int,
 areaX2 int,
 areaY1 int,
 areaY2 int,
 
 tirada_salvacion TINYTEXT
);

CREATE TABLE Objeto_base (
 id_objeto_base INT PRIMARY KEY ,
 nombre TINYTEXT,
 tipo TINYTEXT, -- Aquí es donde metemos el tema de si es mobiliario por ejemplo.
 rareza TINYTEXT,
 peso SMALLINT,
 capacidad SMALLINT,
 clase_armadura_objeto SMALLINT,
 valor SMALLINT,
 coste SMALLINT,
 
 armadura_annadida SMALLINT,
 requisito_fuerza SMALLINT,
 sigilo TINYTEXT,
 tiempo_para_poner_quitar SMALLINT,
 
 tipo_arma TINYTEXT,
 propiedad_arma TINYTEXT,
 danno SMALLINT,
 alcance SMALLINT,
 prioridadAtaque SMALLINT, -- Nuevo campo para cuando se ataque. 
 
 encantado BOOL,
 
 categoria_objeto_magico TINYTEXT,
 quien_fabrico_objeto TINYTEXT,
 historia_objeto TINYTEXT,
 propiedad_menor TINYTEXT,
 peculiaridad TINYTEXT,
 
 indestructible BOOL,
 
 beneficio_menor TINYTEXT,
 beneficio_mayor TINYTEXT,
 perjuicio_menor TINYTEXT,
 perjuicio_mayor TINYTEXT
);

-- Tablas intermedias Ente

-- Stats base
CREATE TABLE Stats_base_entidades (
 PRIMARY KEY (id_stats_base_entidades, timestep, subTimestep),
 
 id_stats_base_entidades INT,
 timestep INT,
 subTimestep INT,
 
 id_entidades INT,
 id_stats_base INT,
 
 hp INT,
 ac SMALLINT,
 fuerza SMALLINT,
 destreza SMALLINT,
 constitucion SMALLINT,
 inteligencia SMALLINT,
 sabiduria SMALLINT,
 carisma SMALLINT,
    
 FOREIGN KEY (timestep, subTimestep) REFERENCES Timestep (timestep, subTimestep),
 FOREIGN KEY (id_entidades) REFERENCES Entidades (id_entidades),
 FOREIGN KEY (id_stats_base) REFERENCES Stats_base(id_stats_base)
);

CREATE TABLE Acciones_entidades (
 PRIMARY KEY (id_acciones_entidades, timestep, subTimestep),

 id_acciones_entidades INT,
 timestep INT, 
 subTimestep INT,

 id_entidades INT,
 id_acciones VARCHAR(255), 

 FOREIGN KEY (timestep, subTimestep) REFERENCES Timestep (timestep, subTimestep),
 FOREIGN KEY (id_entidades) REFERENCES Entidades(id_entidades),
 FOREIGN KEY (id_acciones) REFERENCES Acciones(id_acciones)
);

CREATE TABLE Tiempo_acciones_entidades (
 PRIMARY KEY (id_tiempo_acciones_entidades, timestep, subTimestep), -- Podemos gestionar el subtimestep por C#. Se procesan las acciones en unity -> se vuelcan a SQL.
 
 id_tiempo_acciones_entidades INT,
 timestep INT,
 subTimestep INT, 

 id_entidades INT,
 id_acciones VARCHAR(255),
 
 id_objeto_usado INT NULL,
 id_hechizo_usado VARCHAR(255) NULL,

 objetivoX_1 INT,
 objetivoY_1 INT,
 objetivoX_2 INT,
 objetivoY_2 INT,

 FOREIGN KEY (timestep, subTimestep) REFERENCES Timestep (timestep, subTimestep),
 FOREIGN KEY (id_entidades) REFERENCES Entidades (id_entidades),
 FOREIGN KEY (id_acciones) REFERENCES Acciones(id_acciones),
 
 FOREIGN KEY (id_objeto_usado) REFERENCES Objeto_base(id_objeto_base),
 FOREIGN KEY (id_hechizo_usado) REFERENCES Hechizo_base(id_hechizo_base)
);


-- Hechizos base

CREATE TABLE Entidades_hechizo_base (
 PRIMARY KEY (id_entidades_hechizo_base, timestep, subTimestep),
 
 id_entidades_hechizo_base INT ,
 timestep INT, 
 subTimestep INT, 
 
 id_entidades INT, 
 id_hechizo_base VARCHAR(255), 
 
 FOREIGN KEY (timestep, subTimestep) REFERENCES Timestep (timestep, subTimestep),
 FOREIGN KEY (id_entidades) REFERENCES Entidades (id_entidades),
 FOREIGN KEY (id_hechizo_base) REFERENCES Hechizo_base (id_hechizo_base)
);

-- Objeto-Hechizo base

CREATE TABLE Entidades_objeto_base_hechizo_base (
 PRIMARY KEY(id_entidades_objeto_base_hechizo_base, timestep, subTimestep),
 
 id_entidades_objeto_base_hechizo_base INT,
 timestep INT, 
 subTimestep INT,
 
 id_entidades INT, 
 id_objeto_base INT,
 id_hechizo_base VARCHAR(255),
 
 FOREIGN KEY (timestep, subTimestep) REFERENCES Timestep (timestep, subTimestep),
 FOREIGN KEY (id_entidades) REFERENCES Entidades (id_entidades),
 FOREIGN KEY (id_objeto_base) REFERENCES Objeto_base (id_objeto_base),
 FOREIGN KEY (id_hechizo_base) REFERENCES Hechizo_base (id_hechizo_base)
);

-- NAMESPACE: Asentamiento

CREATE TABLE Asentamiento_generador(
 id_asentamiento_generador INT PRIMARY KEY,

 dimMaxX INT,
 dimMaxY INT,

-- Bloque Lore
 isDungeon BOOL,
 ubicacion TINYTEXT,
	ubicacion_exotica TINYTEXT,
 creador TINYTEXT,
	secta_grupo_religioso TINYTEXT,
 alineamiento_creador TINYTEXT,
 clase_creador TINYTEXT,
 proposito_asentamiento TINYTEXT,
 historia_asentamiento TEXT,
 
-- Bloque Asentamiento
 relaciones_entre_razas TINYTEXT,
 gobernante TINYTEXT, -- ¿esto es como en C#? ¿puedo hacer que se coma un tipo de dato NPC?
 caracs_destacadas TINYTEXT,
 conocido_por TINYTEXT,
 desgracia_actual TINYTEXT,
 cantidad_edificios_aleatorios SMALLINT,
 tipo_edificios_aleatorios SMALLINT,
	residencia TINYTEXT,
    edificio_religioso TINYTEXT,
    taberna TINYTEXT,
		primer_nombre_taberna TINYTEXT,
		segundo_nombre_taberna TINYTEXT,
	almacen TINYTEXT,
    tienda TINYTEXT,
    mazmorra TINYTEXT,
 encuentros_urbanos_aleatorios TEXT
);

CREATE TABLE Sala_generada(
 id_sala_generada INT PRIMARY KEY,

 shapeX INT,
 shapeY INT,

-- Bloque Mazmorra

-- coordX INT,
-- coordY INT,
 num_pisos INT,
 num_salas INT,
 num_pasillos INT,
 num_puertas INT,
 zona_inicial TINYTEXT,

 tipo_pasillo TINYTEXT,
	ancho_pasillo INT,
 tipo_puerta TINYTEXT,
	tras_puerta TINYTEXT,
    
 sala TINYTEXT,
	tipo_sala TINYTEXT,
 tipos_salida_sala INT,
	sala_normal TINYTEXT,
    sala_grande TINYTEXT,
    
    coordX_salida INT,
    coordY_salida INT,
    tipo_salida TINYTEXT,
    
    tipo_escaleras TINYTEXT
-- Atención, podemos darle una serie de puntos para que construya la sala. Más complejo, pero más chulo.
);

CREATE TABLE Evento_asentamiento(
 id_evento_asentamiento INT PRIMARY KEY ,
 tipo BOOL,
 gravedad SMALLINT,
 activador SMALLINT,
 efecto TINYTEXT,
 cd SMALLINT,
 bonificador_ataque SMALLINT,
 gravedad_danno_nivel_personaje SMALLINT,
 timestep_duration INT, -- 1 timestep = 1 turno
	subTimestep_duration INT -- Introducido para ir acorde a nuestro sistema de turnos
);

CREATE TABLE Sala_proposito_contenido(
 id_sala_proposito_contenido INT PRIMARY KEY,

-- Bloque Area proposito sala
    proposito_sala INT,
	almacen_tesoros TEXT,
    fortaleza TEXT,
    guarida TEXT,
    laberinto TEXT,
    mausoleo TEXT,
    mina TEXT,
    portal TEXT,
    templo TEXT,
    trampa TEXT,
 salas_genericas TEXT,
 estado_sala TEXT,
 contenido_sala TEXT, -- La tabla pregunta a la tabla de objetos por el mobiliario. No puede colocar un objeto que no existe. Esto se hace en runtime
	mobiliario INT, -- Foreign key a objeto para que se asegure que haya un tipo de mobiliario en objetos
	objetos_en_contenedor TEXT,
	peligro_aleatorio TEXT,
	obstaculo TEXT,
	objeto_atimania TEXT,
    artimania TEXT,
    libros_pergaminos_etc TEXT,
 ruidos TEXT,
 aire TEXT,
 aroma TEXT,
 detalle_general TEXT
);

-- Tablas intermedias Asentamiento

CREATE TABLE Asentamiento_generador_sala_generada(
 id_asentamiento_generador_sala_generada INT PRIMARY KEY,

 id_sala_generada INT, 
 id_asentamiento_generador INT, 

 FOREIGN KEY (id_sala_generada) REFERENCES Sala_generada (id_sala_generada),
 FOREIGN KEY (id_asentamiento_generador) REFERENCES Asentamiento_generador (id_asentamiento_generador)
);

CREATE TABLE Sala_generada_sala_proposito_contenido(
 id_sala_generada_sala_proposito_contenido INT PRIMARY KEY,

 id_sala_generada INT, 
 id_sala_proposito_contenido INT, 

 FOREIGN KEY (id_sala_generada) REFERENCES Sala_generada (id_sala_generada),
 FOREIGN KEY (id_sala_proposito_contenido) REFERENCES Sala_proposito_contenido (id_sala_proposito_contenido)
);

CREATE TABLE Sala_proposito_contenido_evento_asentamiento(
 PRIMARY KEY(id_sala_proposito_contenido_evento_asentamiento, timestep, subTimestep),

 id_sala_proposito_contenido_evento_asentamiento INT,
 timestep INT,
 subTimestep INT,

 id_sala_proposito_contenido INT, 
 id_evento_asentamiento INT, 

 FOREIGN KEY (timestep, subTimestep) REFERENCES Timestep (timestep, subTimestep),
 FOREIGN KEY (id_sala_proposito_contenido) REFERENCES Sala_proposito_contenido (id_sala_proposito_contenido),
 FOREIGN KEY (id_evento_asentamiento) REFERENCES Evento_asentamiento (id_evento_asentamiento)
);

-- Tablas para Interconectar Ente y Asentamiento

CREATE TABLE Entidades_sala_proposito_contenido(
 PRIMARY KEY(id_entidades_sala_proposito_contenido, timestep, subTimestep),

 id_entidades_sala_proposito_contenido INT,
 timestep INT, 
 subTimestep INT,
 -- Arriba, clave primera compuesta 
 
 id_entidades INT, 
 id_sala_proposito_contenido INT, 

 Xpos INT,
 Ypos INT,

 FOREIGN KEY (timestep, subTimestep) REFERENCES Timestep (timestep, subTimestep),
 FOREIGN KEY (id_entidades) REFERENCES Entidades (id_entidades),
 FOREIGN KEY (id_sala_proposito_contenido) REFERENCES Sala_proposito_contenido(id_sala_proposito_contenido)
);

/* 
¿Por qué estas tablas de abajo, a pesar de ser objetos que se colocan al crearse una mazmorra, tienen timestep?
Muy sencillo, ¿y si un jugador suelta un hechizo o un objeto en el mapa? Ese objeto entonces se coloca en el mapa
en el timestep T, lo que significa que si viajamos atrás en el tiempo, ese objeto no estará presente en vez de ser
perpetuo, que sería el caso si no contáramos CUÁNDO se coloca el objeto.
*/

CREATE TABLE Hechizo_base_sala_proposito_contenido(
 PRIMARY KEY(id_hechizo_base_sala_proposito_contenido, timestep, subTimestep),

 id_hechizo_base_sala_proposito_contenido INT,
 timestep INT, 
 subTimestep INT, 
 -- Arriba, clave primera compuesta 
 
 id_hechizo_base VARCHAR(255), 
 id_sala_proposito_contenido INT, 
 
 Xpos INT,
 Ypos INT,

 FOREIGN KEY (timestep, subTimestep) REFERENCES Timestep (timestep, subTimestep),
 FOREIGN KEY (id_hechizo_base) REFERENCES Hechizo_base (id_hechizo_base),
 FOREIGN KEY (id_sala_proposito_contenido) REFERENCES Sala_proposito_contenido (id_sala_proposito_contenido)
);

CREATE TABLE Objeto_base_hechizo_base_sala_proposito_contenido(
 PRIMARY KEY(id_objeto_base_hechizo_base_sala_proposito_contenido, timestep, subTimestep),

 id_objeto_base_hechizo_base_sala_proposito_contenido INT,
 timestep INT, 
 subTimestep INT, 
 -- Arriba, clave primera compuesta 
 
 id_objeto_base INT, 
 id_hechizo_base VARCHAR(255), 
 id_sala_proposito_contenido INT, 
 
 Xpos INT,
 Ypos INT,

 FOREIGN KEY (timestep, subTimestep) REFERENCES Timestep (timestep, subTimestep),
 FOREIGN KEY (id_objeto_base) REFERENCES Objeto_base (id_objeto_base),
 FOREIGN KEY (id_hechizo_base) REFERENCES Hechizo_base (id_hechizo_base),
 FOREIGN KEY (id_sala_proposito_contenido) REFERENCES Sala_proposito_contenido (id_sala_proposito_contenido)
);

