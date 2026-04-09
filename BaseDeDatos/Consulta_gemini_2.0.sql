-- ==========================================
-- 1. TABLAS INDEPENDIENTES (Sin Foreign Keys)
-- ==========================================

INSERT INTO Timestep (timestep, dificultad) VALUES
(1, 1.0), (2, 1.2), (3, 1.5), (4, 1.8), (5, 2.0),
(6, 2.5), (7, 3.0), (8, 3.5), (9, 4.0), (10, 5.0);

INSERT INTO Stats_base (id_stats_base, hp, ac, fuerza, destreza, constitucion, inteligencia, sabiduria, carisma, velocidad) VALUES
(1, 10, 15, 16, 14, 15, 10, 12, 8, 30),
(2, 8, 12, 10, 16, 12, 18, 14, 10, 30),
(3, 12, 16, 18, 10, 16, 8, 10, 14, 25),
(4, 30, 14, 15, 12, 14, 6, 8, 6, 40),
(5, 6, 10, 8, 14, 10, 12, 16, 18, 30),
(6, 50, 18, 20, 10, 18, 10, 10, 10, 20),
(7, 40, 15, 14, 18, 14, 12, 14, 8, 50),
(8, 20, 11, 8, 12, 10, 20, 18, 16, 30),
(9, 15, 13, 12, 14, 12, 14, 12, 14, 30),
(10, 100, 20, 24, 14, 22, 16, 14, 20, 60);

INSERT INTO Acciones (id_acciones) VALUES
('Abrir'), ('Hablar'), ('Atacar'), ('Caminar'), ('Defenderse'),
('Saltar'), ('Correr'), ('Esquivar'), ('Examinar'), ('Usar');

INSERT INTO Jugadores (id_jugadores, nombre, raza, clase, trasfondo, rasgo_de_personalidad, ideal, vinculo, defecto, nivel) VALUES
(1, 'Gimli', 'Enano', 1, 2, 3, 1, 4, 5, 3),
(2, 'Legolas', 'Elfo', 2, 1, 2, 2, 3, 4, 3),
(3, 'Aragorn', 'Humano', 3, 4, 1, 3, 2, 1, 4),
(4, 'Gandalf', 'Maia', 4, 5, 4, 4, 1, 2, 10),
(5, 'Frodo', 'Mediano', 5, 3, 5, 5, 5, 3, 2),
(6, 'Sam', 'Mediano', 5, 3, 1, 1, 5, 2, 2),
(7, 'Boromir', 'Humano', 1, 4, 2, 3, 4, 1, 4),
(8, 'Gollum', 'Hobbit Corrompido', 6, 6, 5, 2, 1, 5, 5),
(9, 'Elrond', 'Medio Elfo', 7, 7, 3, 4, 2, 3, 12),
(10, 'Galadriel', 'Elfo', 8, 8, 4, 5, 3, 4, 15);

INSERT INTO Monstruos (id_monstruos, tipo, tamanno, desafio, alineamiento) VALUES
('Goblin', 'Humanoide', 'Pequeño', 1, 'Neutral Malvado'),
('Orco', 'Humanoide', 'Mediano', 2, 'Caótico Malvado'),
('Dragon Rojo', 'Dragón', 'Enorme', 17, 'Caótico Malvado'),
('Cubo Gelatinoso', 'Cieno', 'Grande', 4, 'No alineado'),
('Esqueleto', 'No muerto', 'Mediano', 1, 'Legal Malvado'),
('Lich', 'No muerto', 'Mediano', 21, 'Neutral Malvado'),
('Mimico', 'Monstruosidad', 'Mediano', 2, 'Neutral'),
('Beholder', 'Aberración', 'Grande', 13, 'Legal Malvado'),
('Vampiro', 'No muerto', 'Mediano', 13, 'Legal Malvado'),
('Troll', 'Gigante', 'Grande', 5, 'Caótico Malvado');

INSERT INTO Pnj (id_pnj, nombre, apariencia, caracteristica, caracteristica_alta, caracteristica_baja, talento, peculiaridad, interaccion_con_los_demas, ideal, ideal_bueno, ideal_malo, ideal_legal, ideal_caotico, ideal_neutral, ideal_otro, alineamiento, vinculo, defecto) VALUES
(1, 'Bob Tabernero', 'Calvo', 'Amable', 'Carisma', 'Destreza', 'Cocina', 'Tartamudea', 'Hospitalario', 1, 'Paz', 'Codicia', 'Orden', 'Libertad', 'Equilibrio', 'Ninguno', 'Neutral Bueno', 'Taberna', 'Cobarde'),
(2, 'Lady Vesper', 'Alta', 'Distante', 'Inteligencia', 'Fuerza', 'Política', 'Usa abanico', 'Condescendiente', 2, 'Progreso', 'Poder', 'Tradición', 'Revolución', 'Naturaleza', 'Ninguno', 'Legal Neutral', 'Familia', 'Arrogante'),
(3, 'Thorek', 'Marcas forja', 'Gruñon', 'Fuerza', 'Carisma', 'Herrería', 'Escupe', 'Directo', 3, 'Justicia', 'Venganza', 'Honor', 'Anarquía', 'Supervivencia', 'Ninguno', 'Legal Bueno', 'Martillo', 'Terco'),
(4, 'Lira', 'Sucia', 'Curiosa', 'Destreza', 'Constitucion', 'Robar', 'Juega moneda', 'Desconfiada', 4, 'Amistad', 'Dolor', 'Ley', 'Caos', 'Independencia', 'Ninguno', 'Caótico Neutral', 'Hermana', 'Cleptómana'),
(5, 'Sacerdote Aldus', 'Anciano', 'Sereno', 'Sabiduría', 'Fuerza', 'Sanación', 'Reza', 'Paternal', 5, 'Luz', 'Oscuridad', 'Dogma', 'Herejía', 'Fe', 'Ninguno', 'Legal Bueno', 'Deidad', 'Ingenuo'),
(6, 'Capitán Rex', 'Tuerto', 'Firme', 'Constitucion', 'Inteligencia', 'Liderazgo', 'Cicatriz', 'Autoritario', 1, 'Protección', 'Crueldad', 'Deber', 'Descontrol', 'Status Quo', 'Ninguno', 'Legal Neutral', 'Guardia', 'Despiadado'),
(7, 'Mago Kael', 'Túnica roja', 'Misterioso', 'Inteligencia', 'Sabiduría', 'Arcanos', 'Olor ozono', 'Críptico', 2, 'Conocimiento', 'Destrucción', 'Reglas', 'Magia Salvaje', 'Indiferencia', 'Ninguno', 'Neutral', 'Gremio', 'Orgulloso'),
(8, 'Rufian', 'Encapuchado', 'Nervioso', 'Destreza', 'Carisma', 'Mentir', 'Mira atrás', 'Evasivo', 3, 'Caridad', 'Egoísmo', 'Contrato', 'Supervivencia', 'Lucro', 'Ninguno', 'Caótico Malvado', 'Oro', 'Traidor'),
(9, 'Niño Timmy', 'Pequeño', 'Alegre', 'Carisma', 'Fuerza', 'Jugar', 'Lleva peluche', 'Inocente', 4, 'Diversión', 'Miedo', 'Obediencia', 'Travesura', 'Crecimiento', 'Ninguno', 'Neutral Bueno', 'Padres', 'Llorón'),
(10, 'Herrero Bror', 'Musculoso', 'Trabajador', 'Fuerza', 'Inteligencia', 'Armas', 'Sordo', 'Ruidoso', 5, 'Creación', 'Guerra', 'Calidad', 'Caos', 'Comercio', 'Ninguno', 'Neutral', 'Fragua', 'Avaro');

INSERT INTO Hechizo_base (id_hechizo_base, nivel, tiempo_lanzamiento, alcance, componentes, duracion, escuela, objetivo, area_Efecto_forma, area_efecto_distancia, tirada_salvacion) VALUES
('Bola de Fuego', 3, 1, 150, 'V, S, M', 0, 'Evocación', 2, 'Esfera', 20, 'Destreza'),
('Curar Heridas', 1, 1, 0, 'V, S', 0, 'Evocación', 1, 'Ninguna', 0, 'Ninguna'),
('Escudo Mágico', 1, 1, 0, 'V, S', 1, 'Abjuración', 1, 'Ninguna', 0, 'Ninguna'),
('Proyectil Mágico', 1, 1, 120, 'V, S', 0, 'Evocación', 3, 'Ninguna', 0, 'Ninguna'),
('Invisibilidad', 2, 1, 0, 'V, S, M', 60, 'Ilusión', 1, 'Ninguna', 0, 'Ninguna'),
('Volar', 3, 1, 0, 'V, S, M', 100, 'Transmutación', 1, 'Ninguna', 0, 'Ninguna'),
('Deseo', 9, 1, 0, 'V', 0, 'Conjuración', 1, 'Ninguna', 0, 'Ninguna'),
('Muro de Fuego', 4, 1, 120, 'V, S, M', 10, 'Evocación', 0, 'Línea', 60, 'Destreza'),
('Rayo', 3, 1, 100, 'V, S, M', 0, 'Evocación', 0, 'Línea', 100, 'Destreza'),
('Teletransporte', 7, 1, 10, 'V', 0, 'Conjuración', 0, 'Círculo', 10, 'Ninguna');

INSERT INTO Objeto_base (id_objeto_base, nombre, tipo, rareza, peso, capacidad, clase_armadura_objeto, valor, coste, armadura_annadida, requisito_fuerza, sigilo, tiempo_para_poner_quitar, tipo_arma, propiedad_arma, danno, alcance, encantado, categoria_objeto_magico, quien_fabrico_objeto, historia_objeto, propiedad_menor, peculiaridad, indestructible, beneficio_menor, beneficio_mayor, perjuicio_menor, perjuicio_mayor) VALUES
(1, 'Espada Larga', 'Arma', 'Común', 3, 0, 15, 15, 15, 0, 13, 'Normal', 1, 'Marcial', 'Versátil', 8, 5, FALSE, 'Ninguna', 'Herrero local', 'Estándar', 'Ninguna', 'Mellada', FALSE, 'Ninguno', 'Ninguno', 'Ninguno', 'Ninguno'),
(2, 'Poción Curación', 'Consumible', 'Común', 1, 0, 5, 50, 50, 0, 0, 'Normal', 1, 'Ninguno', 'Ninguna', 0, 0, TRUE, 'Poción', 'Alquimista', 'Sabe a fresa', 'Cura 2d4+2', 'Roja', FALSE, 'Ninguno', 'Ninguno', 'Ninguno', 'Ninguno'),
(3, 'Cota de Mallas', 'Armadura', 'Poco Común', 55, 0, 16, 75, 75, 16, 13, 'Desventaja', 10, 'Ninguno', 'Ninguna', 0, 0, FALSE, 'Ninguna', 'Herrería enana', 'Pesada', 'Ninguna', 'Ruidosa', FALSE, 'Ninguno', 'Ninguno', 'Ninguno', 'Ninguno'),
(4, 'Varita Mágica', 'Foco', 'Raro', 1, 0, 10, 500, 500, 0, 0, 'Normal', 1, 'Ninguno', 'Ninguna', 0, 0, TRUE, 'Varita', 'Mago antiguo', 'De roble', '+1 hechizos', 'Brilla', FALSE, 'Ninguno', 'Ninguno', 'Ninguno', 'Ninguno'),
(5, 'Estatua Gárgola', 'Mobiliario', 'Común', 200, 0, 18, 100, 100, 0, 0, 'Normal', 0, 'Ninguno', 'Ninguna', 0, 0, FALSE, 'Ninguna', 'Cantero', 'Adorno', 'Ninguna', 'Pesada', FALSE, 'Ninguno', 'Ninguno', 'Ninguno', 'Ninguno'),
(6, 'Cofre Madera', 'Mobiliario', 'Común', 25, 50, 12, 5, 5, 0, 0, 'Normal', 0, 'Ninguno', 'Ninguna', 0, 0, FALSE, 'Ninguna', 'Carpintero', 'Guarda cosas', 'Ninguna', 'Rechina', FALSE, 'Ninguno', 'Ninguno', 'Ninguno', 'Ninguno'),
(7, 'Candelabro Plata', 'Tesoro', 'Poco Común', 5, 0, 10, 25, 25, 0, 0, 'Normal', 0, 'Ninguno', 'Ninguna', 0, 0, FALSE, 'Ninguna', 'Orfebre', 'Robado', 'Ninguna', 'Falta una vela', FALSE, 'Ninguno', 'Ninguno', 'Ninguno', 'Ninguno'),
(8, 'Anillo Invisibilidad', 'Accesorio', 'Legendario', 0, 0, 20, 5000, 5000, 0, 0, 'Normal', 1, 'Ninguno', 'Ninguna', 0, 0, TRUE, 'Anillo', 'Sauron', 'Peligroso', 'Invisibilidad', 'Corrompe', TRUE, 'Sigilo', 'Poder', 'Paranoia', 'Maldición'),
(9, 'Arco Largo', 'Arma', 'Común', 2, 0, 15, 50, 50, 0, 0, 'Normal', 1, 'Marcial', 'Dos manos', 8, 150, FALSE, 'Ninguna', 'Elfos', 'Madera flexible', 'Ninguna', 'Cuerda tensa', FALSE, 'Ninguno', 'Ninguno', 'Ninguno', 'Ninguno'),
(10, 'Libro Conjuros', 'Objeto', 'Raro', 3, 0, 12, 100, 100, 0, 0, 'Normal', 1, 'Ninguno', 'Ninguna', 0, 0, TRUE, 'Libro', 'Archimago', 'Páginas rotas', 'Almacena magia', 'Huele a moho', FALSE, 'Ninguno', 'Ninguno', 'Ninguno', 'Ninguno');

-- ==========================================
-- 2. TABLAS INTERMEDIAS: ENTE
-- ==========================================

INSERT INTO Stats_base_jugadores (id_stats_base_jugadores, timestep, id_jugadores, id_stats_base) VALUES
(1, 1, 1, 1), (2, 2, 2, 2), (3, 3, 3, 3), (4, 4, 4, 4), (5, 5, 5, 5),
(6, 6, 6, 1), (7, 7, 7, 3), (8, 8, 8, 2), (9, 9, 9, 8), (10, 10, 10, 8);

INSERT INTO Stats_base_monstruos (id_stats_base_monstruos, timestep, id_monstruos, id_stats_base) VALUES
(1, 1, 'Goblin', 5), (2, 2, 'Orco', 3), (3, 3, 'Dragon Rojo', 10), (4, 4, 'Cubo Gelatinoso', 6), (5, 5, 'Esqueleto', 2),
(6, 6, 'Lich', 8), (7, 7, 'Mimico', 3), (8, 8, 'Beholder', 8), (9, 9, 'Vampiro', 7), (10, 10, 'Troll', 6);

INSERT INTO Stats_base_pnj (id_stats_base_pnj, timestep, id_pnj, id_stats_base) VALUES
(1, 1, 1, 2), (2, 2, 2, 8), (3, 3, 3, 1), (4, 4, 4, 2), (5, 5, 5, 5),
(6, 6, 6, 3), (7, 7, 7, 8), (8, 8, 8, 2), (9, 9, 9, 5), (10, 10, 10, 1);

INSERT INTO Acciones_jugadores (id_acciones_jugadores, id_jugadores, id_acciones) VALUES
(1, 1, 'Atacar'), (2, 2, 'Caminar'), (3, 3, 'Defenderse'), (4, 4, 'Hablar'), (5, 5, 'Abrir'),
(6, 6, 'Correr'), (7, 7, 'Saltar'), (8, 8, 'Esquivar'), (9, 9, 'Examinar'), (10, 10, 'Usar');

INSERT INTO Acciones_monstruos (id_acciones_monstruos, id_monstruos, id_acciones) VALUES
(1, 'Goblin', 'Atacar'), (2, 'Orco', 'Caminar'), (3, 'Dragon Rojo', 'Defenderse'), (4, 'Cubo Gelatinoso', 'Caminar'), (5, 'Esqueleto', 'Atacar'),
(6, 'Lich', 'Usar'), (7, 'Mimico', 'Atacar'), (8, 'Beholder', 'Examinar'), (9, 'Vampiro', 'Esquivar'), (10, 'Troll', 'Atacar');

INSERT INTO Acciones_pnj (id_acciones_pnj, id_pnj, id_acciones) VALUES
(1, 1, 'Hablar'), (2, 2, 'Hablar'), (3, 3, 'Atacar'), (4, 4, 'Abrir'), (5, 5, 'Caminar'),
(6, 6, 'Defenderse'), (7, 7, 'Usar'), (8, 8, 'Correr'), (9, 9, 'Saltar'), (10, 10, 'Examinar');

INSERT INTO Tiempo_acciones_jugadores (id_tiempo_acciones_jugadores, timestep, id_jugadores, id_acciones) VALUES
(1, 1, 1, 'Atacar'), (2, 2, 2, 'Caminar'), (3, 3, 3, 'Defenderse'), (4, 4, 4, 'Hablar'), (5, 5, 5, 'Abrir'),
(6, 6, 6, 'Correr'), (7, 7, 7, 'Saltar'), (8, 8, 8, 'Esquivar'), (9, 9, 9, 'Examinar'), (10, 10, 10, 'Usar');

INSERT INTO Tiempo_acciones_monstruos (id_tiempo_acciones_monstruos, timestep, id_monstruos, id_acciones) VALUES
(1, 1, 'Goblin', 'Atacar'), (2, 2, 'Orco', 'Caminar'), (3, 3, 'Dragon Rojo', 'Defenderse'), (4, 4, 'Cubo Gelatinoso', 'Caminar'), (5, 5, 'Esqueleto', 'Atacar'),
(6, 6, 'Lich', 'Usar'), (7, 7, 'Mimico', 'Atacar'), (8, 8, 'Beholder', 'Examinar'), (9, 9, 'Vampiro', 'Esquivar'), (10, 10, 'Troll', 'Atacar');

INSERT INTO Tiempo_acciones_pnj (id_tiempo_acciones_pnj, timestep, id_pnj, id_acciones) VALUES
(1, 1, 1, 'Hablar'), (2, 2, 2, 'Hablar'), (3, 3, 3, 'Atacar'), (4, 4, 4, 'Abrir'), (5, 5, 5, 'Caminar'),
(6, 6, 6, 'Defenderse'), (7, 7, 7, 'Usar'), (8, 8, 8, 'Correr'), (9, 9, 9, 'Saltar'), (10, 10, 10, 'Examinar');

-- ==========================================
-- 3. TABLAS INTERMEDIAS: HECHIZOS
-- ==========================================

INSERT INTO Jugadores_hechizo_base (id_jugadores_hechizo_base, timestep, id_jugadores, id_hechizo_base) VALUES
(1, 1, 4, 'Bola de Fuego'), (2, 2, 4, 'Escudo Mágico'), (3, 3, 2, 'Curar Heridas'), (4, 4, 3, 'Proyectil Mágico'), (5, 5, 5, 'Invisibilidad'),
(6, 6, 4, 'Volar'), (7, 7, 9, 'Teletransporte'), (8, 8, 10, 'Deseo'), (9, 9, 4, 'Rayo'), (10, 10, 9, 'Muro de Fuego');

INSERT INTO Monstruos_hechizo_base (id_monstruos_hechizo_base, timestep, id_monstruos, id_hechizo_base) VALUES
(1, 1, 'Dragon Rojo', 'Bola de Fuego'), (2, 2, 'Goblin', 'Invisibilidad'), (3, 3, 'Esqueleto', 'Escudo Mágico'), (4, 4, 'Orco', 'Proyectil Mágico'), (5, 5, 'Dragon Rojo', 'Escudo Mágico'),
(6, 6, 'Lich', 'Deseo'), (7, 7, 'Lich', 'Muro de Fuego'), (8, 8, 'Vampiro', 'Volar'), (9, 9, 'Beholder', 'Rayo'), (10, 10, 'Lich', 'Teletransporte');

INSERT INTO Pnj_hechizo_base (id_pnj_hechizo_base, timestep, id_pnj, id_hechizo_base) VALUES
(1, 1, 5, 'Curar Heridas'), (2, 2, 2, 'Proyectil Mágico'), (3, 3, 5, 'Escudo Mágico'), (4, 4, 4, 'Invisibilidad'), (5, 5, 3, 'Bola de Fuego'),
(6, 6, 7, 'Volar'), (7, 7, 7, 'Rayo'), (8, 8, 7, 'Teletransporte'), (9, 9, 5, 'Muro de Fuego'), (10, 10, 2, 'Deseo');

INSERT INTO Jugadores_objeto_base_hechizo_base (id_jugadores_objeto_base_hechizo_base, timestep, id_jugadores, id_objeto_base, id_hechizo_base) VALUES
(1, 1, 4, 4, 'Bola de Fuego'), (2, 2, 2, 2, 'Curar Heridas'), (3, 3, 3, 1, 'Escudo Mágico'), (4, 4, 5, 8, 'Invisibilidad'), (5, 5, 1, 3, 'Escudo Mágico'),
(6, 6, 9, 10, 'Teletransporte'), (7, 7, 10, 8, 'Deseo'), (8, 8, 4, 4, 'Rayo'), (9, 9, 6, 2, 'Curar Heridas'), (10, 10, 2, 9, 'Volar');

INSERT INTO Monstruos_objeto_base_hechizo_base (id_monstruos_objeto_base_hechizo_base, timestep, id_monstruos, id_objeto_base, id_hechizo_base) VALUES
(1, 1, 'Goblin', 2, 'Curar Heridas'), (2, 2, 'Orco', 1, 'Proyectil Mágico'), (3, 3, 'Esqueleto', 3, 'Escudo Mágico'), (4, 4, 'Dragon Rojo', 4, 'Bola de Fuego'), (5, 5, 'Cubo Gelatinoso', 5, 'Invisibilidad'),
(6, 6, 'Lich', 10, 'Deseo'), (7, 7, 'Mimico', 6, 'Invisibilidad'), (8, 8, 'Vampiro', 8, 'Volar'), (9, 9, 'Troll', 1, 'Rayo'), (10, 10, 'Lich', 4, 'Muro de Fuego');

INSERT INTO Pnj_objeto_base_hechizo_base (id_pnj_objeto_base_hechizo_base, timestep, id_pnj, id_objeto_base, id_hechizo_base) VALUES
(1, 1, 5, 4, 'Curar Heridas'), (2, 2, 2, 4, 'Proyectil Mágico'), (3, 3, 4, 2, 'Invisibilidad'), (4, 4, 3, 1, 'Bola de Fuego'), (5, 5, 1, 2, 'Escudo Mágico'),
(6, 6, 7, 10, 'Teletransporte'), (7, 7, 6, 1, 'Muro de Fuego'), (8, 8, 8, 8, 'Invisibilidad'), (9, 9, 5, 7, 'Curar Heridas'), (10, 10, 7, 4, 'Rayo');

-- ==========================================
-- 4. TABLAS DE ASENTAMIENTO / GENERACIÓN MAPA
-- ==========================================

INSERT INTO Asentamiento_generador (id_asentamiento_generador, dimMaxX, dimMaxY, isDungeon, ubicacion, ubicacion_exotica, creador, secta_grupo_religioso, alineamiento_creador, clase_creador, proposito_asentamiento, historia_asentamiento, relaciones_entre_razas, gobernante, caracs_destacadas, conocido_por, desgracia_actual, cantidad_edificios_aleatorios, tipo_edificios_aleatorios, residencia, edificio_religioso, taberna, primer_nombre_taberna, segundo_nombre_taberna, almacen, tienda, mazmorra, encuentros_urbanos_aleatorios) VALUES
(1, 100, 100, TRUE, 'Subterráneo', 'Volcán', 'Enanos', 'Culto Fuego', 'Neutral', 'Artífice', 'Mina', 'Mina abandonada', 'Hostil', 'Rey Orco', 'Lava', 'Gemas', 'Infestación', 0, 0, 'No', 'No', 'No', 'La', 'Mina', 'Sí', 'No', 'Sí', 'Emboscada goblin'),
(2, 50, 50, FALSE, 'Bosque', 'Cráter', 'Elfos', 'Druídico', 'Bueno', 'Druida', 'Pueblo', 'Pueblo pacífico', 'Amigable', 'Anciano', 'Árboles', 'Magia', 'Plaga', 10, 1, 'Sí', 'Sí', 'Sí', 'El', 'Poni', 'Sí', 'Sí', 'No', 'Mercader'),
(3, 200, 200, TRUE, 'Montaña', 'Nubes', 'Magos', 'Orden Arcana', 'Legal', 'Mago', 'Prisión', 'Prisión mágica', 'Tensas', 'Guardián', 'Cristales', 'Peligro', 'Fuga', 0, 0, 'No', 'No', 'No', 'El', 'Mago', 'No', 'No', 'Sí', 'Elemental'),
(4, 150, 150, TRUE, 'Desierto', 'Oasis', 'Faraón', 'Muerte', 'Malvado', 'Clérigo', 'Tumba', 'Descanso', 'Aislado', 'Momia', 'Arena', 'Tesoros', 'Maldición', 0, 0, 'No', 'Sí', 'No', 'La', 'Duna', 'No', 'No', 'Sí', 'Escorpiones'),
(5, 80, 80, FALSE, 'Costa', 'Acantilado', 'Humanos', 'Panteón', 'Neutral', 'Guerrero', 'Puerto', 'Ciudad comercial', 'Abierta', 'Alcalde', 'Barcos', 'Comercio', 'Piratas', 20, 2, 'Sí', 'Sí', 'Sí', 'El', 'Ancla', 'Sí', 'Sí', 'No', 'Pelea'),
(6, 300, 300, TRUE, 'Pantano', 'Cueva Gaseosa', 'Hombres Lagarto', 'Dios Cieno', 'Caótico', 'Bárbaro', 'Templo', 'Ruinas hundidas', 'Odio', 'Sacerdote', 'Lodo', 'Veneno', 'Inundación', 0, 0, 'No', 'Sí', 'No', 'El', 'Sapo', 'No', 'No', 'Sí', 'Limo oscuro'),
(7, 120, 120, FALSE, 'Pradera', 'Ruina Flotante', 'Gnomos', 'Gremio Inventores', 'Bueno', 'Artífice', 'Laboratorio', 'Experimento fallido', 'Curiosos', 'Maestro', 'Engranajes', 'Inventos', 'Explosiones', 15, 3, 'Sí', 'No', 'Sí', 'La', 'Tuerca', 'Sí', 'Sí', 'Sí', 'Robot loco'),
(8, 400, 400, TRUE, 'Bajo Tierra', 'Abismo', 'Drow', 'Araña', 'Malvado', 'Pícaro', 'Ciudadela', 'Hogar oscuro', 'Esclavitud', 'Matrona', 'Telarañas', 'Asesinatos', 'Guerra Civil', 50, 4, 'Sí', 'Sí', 'Sí', 'La', 'Sombra', 'Sí', 'Sí', 'Sí', 'Patrulla drow'),
(9, 60, 60, FALSE, 'Valle', 'Lago Cristalino', 'Hadas', 'Corte Verano', 'Caótico', 'Bardo', 'Refugio', 'Fiesta eterna', 'Bromistas', 'Reina', 'Flores', 'Música', 'Apatía', 5, 1, 'Sí', 'No', 'No', 'El', 'Lirio', 'No', 'No', 'No', 'Duendes'),
(10, 250, 250, TRUE, 'Pico Helado', 'Glaciar', 'Gigantes', 'Dios Trueno', 'Neutral', 'Guerrero', 'Fortaleza', 'Castillo hielo', 'Aislacionistas', 'Jarl', 'Hielo', 'Frío', 'Avalanchas', 0, 0, 'No', 'Sí', 'No', 'El', 'Cuerno', 'Sí', 'No', 'Sí', 'Lobos invernales');

INSERT INTO Sala_generada (id_sala_generada, shapeX, shapeY, num_pisos, num_salas, num_pasillos, num_puertas, zona_inicial, tipo_pasillo, ancho_pasillo, tipo_puerta, tras_puerta, sala, tipo_sala, tipos_salida_sala, sala_normal, sala_grande, coordX_salida, coordY_salida, tipo_salida, tipo_escaleras) VALUES
(1, 10, 10, 1, 5, 3, 4, 'Entrada', 'Piedra', 2, 'Madera', 'Pasillo', 'Guardia', 'Cuadrada', 2, 'Sí', 'No', 5, 10, 'Puerta', 'Madera'),
(2, 20, 15, 1, 8, 5, 6, 'Pasillo', 'Tierra', 1, 'Hierro', 'Trampa', 'Tesoro', 'Rectangular', 1, 'No', 'Sí', 20, 7, 'Arco', 'Piedra'),
(3, 5, 5, 2, 2, 1, 1, 'Escaleras', 'Ladrillo', 2, 'Secreta', 'Monstruo', 'Celda', 'Redonda', 1, 'Sí', 'No', 2, 5, 'Agujero', 'Caracol'),
(4, 15, 15, 1, 4, 2, 3, 'Santuario', 'Hielo', 3, 'Mágica', 'Vacío', 'Altar', 'Hexagonal', 3, 'No', 'Sí', 7, 0, 'Doble', 'Cristal'),
(5, 12, 12, 3, 10, 8, 12, 'Comedor', 'Mármol', 2, 'Acero', 'Tesoro', 'Jefe', 'Irregular', 2, 'No', 'Sí', 12, 6, 'Portón', 'Granito'),
(6, 8, 8, 1, 3, 1, 2, 'Armería', 'Piedra', 1, 'Reja', 'Pasillo', 'Cuartel', 'Cuadrada', 1, 'Sí', 'No', 4, 8, 'Reja', 'Piedra'),
(7, 25, 25, 1, 12, 6, 8, 'Caverna', 'Tierra', 4, 'Ninguna', 'Trampa', 'Nido', 'Irregular', 4, 'No', 'Sí', 25, 12, 'Túnel', 'Ninguna'),
(8, 6, 6, 4, 1, 0, 1, 'Torre', 'Madera', 1, 'Escotilla', 'Monstruo', 'Mirador', 'Redonda', 1, 'Sí', 'No', 3, 3, 'Escalera', 'Mano'),
(9, 30, 10, 1, 5, 4, 5, 'Puente', 'Cuerda', 1, 'Doble', 'Jefe', 'Trono', 'Rectangular', 1, 'No', 'Sí', 30, 5, 'Portón', 'Ninguna'),
(10, 14, 14, 2, 6, 3, 4, 'Biblioteca', 'Ladrillo', 2, 'Secreta', 'Tesoro', 'Estudio', 'Octogonal', 2, 'Sí', 'No', 7, 14, 'Librería', 'Caracol');

-- Eventos obligatoriamente de TIPO TRAMPA (gravedades, activadores y CDs surtidos)
INSERT INTO Evento_asentamiento (id_evento_asentamiento, tipo, gravedad, activador, efecto, cd, bonificador_ataque, gravedad_danno_nivel_personaje, duracion) VALUES
(1, TRUE, 2, 1, 'Foso con pinchos', 12, 5, 1, 0),
(2, TRUE, 3, 2, 'Dardo venenoso', 14, 6, 2, 3),
(3, TRUE, 4, 3, 'Roca rodante', 15, 8, 3, 0),
(4, TRUE, 5, 1, 'Gas venenoso', 16, 0, 4, 5),
(5, TRUE, 3, 4, 'Runa explosiva', 15, 7, 2, 0),
(6, TRUE, 2, 2, 'Red cayendo techo', 13, 4, 1, 2),
(7, TRUE, 4, 3, 'Llamarada pared', 16, 6, 3, 0),
(8, TRUE, 5, 1, 'Foso de ácido', 17, 0, 4, 0),
(9, TRUE, 3, 2, 'Cuchilla péndulo', 14, 8, 2, 0),
(10, TRUE, 4, 4, 'Estatua dispara rayo', 15, 9, 3, 0);

-- El MOBILIARIO enlaza con los IDs (1 al 10) creados en Objeto_base
INSERT INTO Sala_proposito_contenido (id_sala_proposito_contenido, proposito_sala, almacen_tesoros, fortaleza, guarida, laberinto, mausoleo, mina, portal, templo, trampa, salas_genericas, estado_sala, contenido_sala, mobiliario, mobiliario_general, arte, mobiliario_mago, utensilios_personales, objetos_en_contenedor, peligro_aleatorio, obstaculo, objeto_atimanna, artimanna, libros_pergaminos_etc, ruidos, aire, aroma, detalle_general) VALUES
(1, 1, 'Gemas', 'No', 'No', 'No', 'No', 'Sí', 'No', 'No', 'No', 'Armería', 'Polvo', 'Armas', 1, 'Estante', 'Ninguno', 'Ninguno', 'Ropa', 'Oro', 'Moho', 'Escombros', 'Engranaje', 'Cuerda', 'Diario', 'Goteo', 'Viciado', 'Humedad', 'Sangre'),
(2, 2, 'No', 'Sí', 'No', 'No', 'No', 'No', 'No', 'No', 'No', 'Cuartel', 'Ordenado', 'Camas', 5, 'Cama', 'Tapiz', 'Ninguno', 'Plato', 'Plata', 'Ninguno', 'Mesa', 'Llave', 'Polea', 'Mapa', 'Silencio', 'Frío', 'Sudor', 'Garras'),
(3, 3, 'No', 'No', 'Sí', 'No', 'No', 'No', 'No', 'No', 'No', 'Cueva', 'Húmedo', 'Huesos', 3, 'Cofre', 'Ninguno', 'Alambique', 'Taza', 'Poción', 'Hongos', 'Roca', 'Espejo', 'Palanca', 'Libro conjuros', 'Viento', 'Corriente', 'Azufre', 'Telarañas'),
(4, 4, 'No', 'No', 'No', 'No', 'Sí', 'No', 'No', 'No', 'No', 'Cripta', 'Oscuro', 'Ataúdes', 4, 'Altar', 'Estatua', 'Ninguno', 'Anillo', 'Gemas', 'Niebla', 'Ataúd', 'Vela', 'Balanza', 'Pergamino', 'Susurros', 'Helado', 'Polvo', 'Runas'),
(5, 5, 'No', 'No', 'No', 'No', 'No', 'No', 'No', 'No', 'Sí', 'Pasillo', 'Intacto', 'Estatuas', 2, 'Silla', 'Cuadro', 'Telescopio', 'Espejo', 'Nada', 'Ácido', 'Estatua', 'Botón', 'Placa', 'Notas', 'Crujido', 'Calor', 'Humo', 'Quemaduras'),
(6, 6, 'Oro', 'No', 'No', 'Sí', 'No', 'No', 'No', 'No', 'No', 'Cámara', 'Derrumbado', 'Monedas', 6, 'Mesa', 'Jarrón', 'Ninguno', 'Cuchara', 'Joyas', 'Fuego', 'Pilar', 'Gema', 'Resorte', 'Facturas', 'Eco', 'Seco', 'Metálico', 'Grietas'),
(7, 7, 'No', 'No', 'No', 'No', 'No', 'No', 'Sí', 'No', 'No', 'Círculo', 'Brillante', 'Runas', 7, 'Atril', 'Mosaico', 'Orbe', 'Pluma', 'Polvo', 'Radiación', 'Foso', 'Cristal', 'Espejo', 'Grimorio', 'Zumbido', 'Ozono', 'Eléctrico', 'Luz astral'),
(8, 8, 'No', 'No', 'No', 'No', 'No', 'No', 'No', 'Sí', 'No', 'Nave', 'Sagrado', 'Bancos', 8, 'Pila', 'Vidriera', 'Ninguno', 'Rosario', 'Agua bendita', 'Luz cegadora', 'Altar', 'Campana', 'Incensario', 'Biblia', 'Cánticos', 'Puro', 'Incienso', 'Paz'),
(9, 9, 'Armas', 'Sí', 'No', 'No', 'No', 'No', 'No', 'No', 'No', 'Calabozo', 'Sangriento', 'Celdas', 9, 'Potro', 'Ninguno', 'Ninguno', 'Cadenas', 'Huesos', 'Enfermedad', 'Rejas', 'Garfio', 'Cadena', 'Confesiones', 'Gritos', 'Pútrido', 'Óxido', 'Miedo'),
(10, 10, 'Arte', 'No', 'No', 'No', 'No', 'No', 'No', 'No', 'Sí', 'Galería', 'Lujoso', 'Cuadros', 10, 'Pedestal', 'Escultura', 'Lupa', 'Pincel', 'Pintura', 'Ilusión', 'Vitrina', 'Marco', 'Láser', 'Catálogo', 'Clásica', 'Fresco', 'Pintura', 'Trampas mágicas');

INSERT INTO Asentamiento_generador_sala_generada (id_asentamiento_generador_sala_generada, id_sala_generada, id_asentamiento_generador) VALUES
(1, 1, 1), (2, 2, 2), (3, 3, 3), (4, 4, 4), (5, 5, 5),
(6, 6, 6), (7, 7, 7), (8, 8, 8), (9, 9, 9), (10, 10, 10);

INSERT INTO Sala_generada_sala_proposito_contenido (id_sala_generada_sala_proposito_contenido, id_sala_generada, id_sala_proposito_contenido) VALUES
(1, 1, 1), (2, 2, 2), (3, 3, 3), (4, 4, 4), (5, 5, 5),
(6, 6, 6), (7, 7, 7), (8, 8, 8), (9, 9, 9), (10, 10, 10);

INSERT INTO Sala_proposito_contenido_evento_asentamiento (id_sala_proposito_contenido_evento_asentamiento, timestep, id_sala_proposito_contenido, id_evento_asentamiento) VALUES
(1, 1, 1, 1), (2, 2, 2, 2), (3, 3, 3, 3), (4, 4, 4, 4), (5, 5, 5, 5),
(6, 6, 6, 6), (7, 7, 7, 7), (8, 8, 8, 8), (9, 9, 9, 9), (10, 10, 10, 10);

-- ==========================================
-- 5. TABLAS DE INTERCONEXIÓN: ENTE Y ASENTAMIENTO
-- (NOTA: Xpos e Ypos como INT según el esquema)
-- ==========================================

INSERT INTO Jugadores_sala_proposito_contenido (id_jugadores_sala_proposito_contenido, timestep, Xpos, Ypos, id_jugadores, id_sala_proposito_contenido) VALUES
(1, 1, 5, 10, 1, 1), (2, 2, 12, 8, 2, 2), (3, 3, 2, 3, 3, 3), (4, 4, 7, 7, 4, 4), (5, 5, 1, 1, 5, 5),
(6, 6, 4, 4, 6, 6), (7, 7, 20, 15, 7, 7), (8, 8, 3, 2, 8, 8), (9, 9, 15, 5, 9, 9), (10, 10, 8, 8, 10, 10);

INSERT INTO Monstruos_sala_proposito_contenido (id_monstruos_sala_proposito_contenido, timestep, Xpos, Ypos, id_monstruos, id_sala_proposito_contenido) VALUES
(1, 1, 6, 9, 'Goblin', 1), (2, 2, 10, 10, 'Orco', 2), (3, 3, 4, 4, 'Dragon Rojo', 3), (4, 4, 8, 8, 'Cubo Gelatinoso', 4), (5, 5, 2, 2, 'Esqueleto', 5),
(6, 6, 5, 5, 'Lich', 6), (7, 7, 12, 12, 'Mimico', 7), (8, 8, 2, 2, 'Beholder', 8), (9, 9, 10, 4, 'Vampiro', 9), (10, 10, 7, 7, 'Troll', 10);

INSERT INTO Pnj_sala_proposito_contenido (id_pnj_sala_proposito_contenido, timestep, Xpos, Ypos, id_pnj, id_sala_proposito_contenido) VALUES
(1, 1, 5, 5, 1, 1), (2, 2, 8, 4, 2, 2), (3, 3, 3, 7, 3, 3), (4, 4, 6, 6, 4, 4), (5, 5, 9, 9, 5, 5),
(6, 6, 2, 6, 6, 6), (7, 7, 18, 18, 7, 7), (8, 8, 1, 1, 8, 8), (9, 9, 14, 6, 9, 9), (10, 10, 5, 12, 10, 10);

INSERT INTO Hechizo_base_sala_proposito_contenido (id_hechizo_base_sala_proposito_contenido, timestep, Xpos, Ypos, id_hechizo_base, id_sala_proposito_contenido) VALUES
(1, 1, 5, 9, 'Bola de Fuego', 1), (2, 2, 11, 9, 'Curar Heridas', 2), (3, 3, 3, 3, 'Escudo Mágico', 3), (4, 4, 7, 8, 'Proyectil Mágico', 4), (5, 5, 1, 1, 'Invisibilidad', 5),
(6, 6, 6, 6, 'Volar', 6), (7, 7, 10, 10, 'Teletransporte', 7), (8, 8, 2, 3, 'Deseo', 8), (9, 9, 20, 5, 'Rayo', 9), (10, 10, 4, 4, 'Muro de Fuego', 10);

INSERT INTO Objeto_base_hechizo_base_sala_proposito_contenido (id_objeto_base_hechizo_base_sala_proposito_contenido, timestep, Xpos, Ypos, id_objeto_base, id_hechizo_base, id_sala_proposito_contenido) VALUES
(1, 1, 4, 4, 4, 'Bola de Fuego', 1), (2, 2, 5, 5, 2, 'Curar Heridas', 2), (3, 3, 6, 6, 3, 'Escudo Mágico', 3), (4, 4, 7, 7, 1, 'Proyectil Mágico', 4), (5, 5, 8, 8, 5, 'Invisibilidad', 5),
(6, 6, 2, 2, 10, 'Volar', 6), (7, 7, 15, 15, 8, 'Teletransporte', 7), (8, 8, 1, 2, 6, 'Deseo', 8), (9, 9, 22, 6, 9, 'Rayo', 9), (10, 10, 6, 6, 7, 'Muro de Fuego', 10);