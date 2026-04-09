-- 1. TABLAS INDEPENDIENTES (Core)

-- Timestep (Fundamental para la integridad referencial temporal)
INSERT INTO Timestep (timestep, dificultad) VALUES 
(1, 1.0),
(2, 1.5),
(3, 2.0);

-- Stats_base
INSERT INTO Stats_base (id_stats_base, hp, ac, fuerza, destreza, constitucion, inteligencia, sabiduria, carisma, velocidad) VALUES 
(1, 10, 12, 10, 12, 14, 8, 10, 16, 30), -- Perfil Bardo/Hechicero
(2, 45, 18, 18, 10, 16, 8, 10, 8, 25),  -- Perfil Guerrero Tanque
(3, 20, 14, 8, 18, 12, 16, 14, 10, 35); -- Perfil Pícaro/Mago

-- Jugadores
INSERT INTO Jugadores (id_jugadores, nombre, raza, clase, trasfondo, rasgo_de_personalidad, ideal, vinculo, defecto, nivel) VALUES 
(1, 'Loyolo el Bravo', 'Humano', 1, 1, 1, 1, 1, 1, 3),
(2, 'Eldrin Sombra', 'Elfo', 2, 2, 2, 2, 2, 2, 3),
(3, 'Gimli Hacha', 'Enano', 3, 3, 3, 3, 3, 3, 4);

-- Monstruos (Ojo, PK es VARCHAR)
INSERT INTO Monstruos (id_monstruos, tipo, tamanno, desafio, alineamiento) VALUES 
('goblin_001', 'Humanoide', 'Pequeño', 1, 'Caótico Malvado'),
('dragon_rojo_joven', 'Dragón', 'Grande', 10, 'Caótico Malvado'),
('esqueleto_guerrero', 'No-muerto', 'Mediano', 2, 'Legal Malvado');

-- Pnj
INSERT INTO Pnj (id_pnj, nombre, apariencia, caracteristica, caracteristica_alta, caracteristica_baja, talento, peculiaridad, interaccion_con_los_demas, ideal, ideal_bueno, ideal_malo, ideal_legal, ideal_caotico, ideal_neutral, ideal_otro, alineamiento, vinculo, defecto) VALUES 
(1, 'Martha', 'Robusta y alegre', 'Amable', 'Fuerza', 'Inteligencia', 'Cocina', 'Tartamudea', 'Amigable', 1, 'Ayudar', NULL, NULL, NULL, NULL, NULL, 'Neutral Bueno', 'Su taberna', 'Demasiado confiada'),
(2, 'Viejo Tom', 'Encorvado', 'Gruñón', 'Sabiduría', 'Carisma', 'Historia', 'Cojo', 'Hostil', 2, NULL, NULL, 'Orden', NULL, NULL, NULL, 'Legal Neutral', 'La biblioteca', 'Avaro'),
(3, 'Sombra', 'Encapuchado', 'Misterioso', 'Destreza', 'Fuerza', 'Robar', 'Juega con moneda', 'Silencioso', 3, NULL, 'Poder', NULL, NULL, NULL, NULL, 'Neutral Maligno', 'Gremio', 'Cleptómano');

-- Hechizo_base (PK es VARCHAR)
INSERT INTO Hechizo_base (id_hechizo_base, nivel, tiempo_lanzamiento, alcance, componentes, duracion, escuela, objetivo, area_Efecto_forma, area_efecto_distancia, tirada_salvacion) VALUES 
('bola_fuego', 3, 1, 150, 'V,S,M', 0, 'Evocación', 1, 'Esfera', 20, 'Destreza'),
('curar_heridas', 1, 1, 0, 'V,S', 0, 'Evocación', 1, 'Toque', 0, 'Ninguna'),
('invisibilidad', 2, 1, 0, 'V,S,M', 60, 'Ilusión', 1, 'Toque', 0, 'Ninguna');

-- Objeto_base
INSERT INTO Objeto_base (id_objeto_base, nombre, tipo, rareza, peso, capacidad, clase_armadura_objeto, valor, coste, armadura_annadida, requisito_fuerza, sigilo, tiempo_para_poner_quitar, tipo_arma, propiedad_arma, danno, alcance, encantado, categoria_objeto_magico, quien_fabrico_objeto, historia_objeto, propiedad_menor, peculiaridad, indestructible, beneficio_menor, beneficio_mayor, perjuicio_menor, perjuicio_mayor) VALUES 
(1, 'Espada Larga', 'Arma', 'Común', 3, 0, 0, 15, 15, 0, 10, 'Normal', 1, 'Marcial', 'Versátil', 8, 5, 0, NULL, 'Herrero Local', NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL),
(2, 'Poción de Curación', 'Poción', 'Común', 1, 0, 0, 50, 50, 0, 0, 'Normal', 1, NULL, NULL, 0, 0, 1, 'Poción', 'Alquimista', NULL, NULL, 'Sabe a fresa', 0, 'Cura 2d4+2', NULL, NULL, NULL),
(3, 'Anillo de Protección', 'Anillo', 'Raro', 0, 0, 1, 500, 500, 1, 0, 'Normal', 1, NULL, NULL, 0, 0, 1, 'Anillo', 'Mago Antiguo', 'Encontrado en ruinas', 'Brilla', NULL, 1, '+1 CA', NULL, NULL, NULL);

-- Asentamiento_lore
INSERT INTO Asentamiento_lore (id_asentamiento_lore, isDungeon, ubicacion, ubicacion_exotica, creador, secta_grupo_religioso, alineamiento_creador, clase_creador, proposito_asentamiento, historia_asentamiento) VALUES 
(1, 0, 'Valle Verde', NULL, 'Humanos', NULL, 'Neutral', 'Plebeyos', 'Comercio', 'Fundado hace 100 años.'),
(2, 1, 'Montaña Negra', 'Volcán', 'Cultistas', 'El Ojo Rojo', 'Caótico Malvado', 'Brujo', 'Adoración', 'Guarida secreta.'),
(3, 1, 'Bosque Sombrío', 'Árbol Gigante', 'Elfos Oscuros', NULL, 'Neutral Maligno', 'Druida', 'Refugio', 'Abandonado tras la plaga.');

-- Area_proposito_sala
INSERT INTO Area_proposito_sala (id_area_proposito_sala, proposito_sala, almacen_tesoros, fortaleza, guarida, laberinto, mausoleo, mina, portal, templo, trampa, salas_genericas, estado_sala, contenido_sala, mobiliario, mobiliario_general, arte, mobiliario_mago, utensilios_personales, objetos_en_contenedor, peligro_aleatorio, obstaculo, objeto_atimanna, artimanna, libros_pergaminos_etc, ruidos, aire, aroma, detalle_general) VALUES 
(1, 1, NULL, 'Barracones', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Dormitorio', 'Ordenado', 'Camas', 1, 'Literas', NULL, NULL, 'Ropa', 'Cofres', NULL, NULL, NULL, NULL, NULL, 'Ronquidos', 'Viciado', 'Sudor', 'Literas de madera'),
(2, 2, 'Tesoro', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Cámara', 'Polvoriento', 'Cofre', 1, 'Estanterías', 'Estatuas', NULL, NULL, 'Oro', 'Trampa de flechas', 'Escombros', NULL, NULL, NULL, 'Silencio', 'Frío', 'Metálico', 'Brillo dorado'),
(3, 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Altar', NULL, 'Santuario', 'Limpio', 'Altar', 1, 'Bancos', 'Tapices', NULL, 'Velas', 'Ofrendas', NULL, NULL, NULL, NULL, 'Libro sagrado', 'Cánticos', 'Incienso', 'Mirra', 'Luces tenues');

-- Evento_asentamiento
INSERT INTO Evento_asentamiento (id_evento_asentamiento, tipo, gravedad, activador, efecto, cd, bonificador_ataque, gravedad_danno_nivel_personaje, duracion) VALUES 
(1, 1, 1, 1, 'Pinchos suelo', 12, 5, 1, 0),
(2, 1, 2, 2, 'Nube tóxica', 14, 0, 2, 3),
(3, 0, 0, 0, 'Ninguno', 0, 0, 0, 0);

-- 2. TABLAS INTERMEDIAS (Dependen de las anteriores)

-- Asentamiento_generador
INSERT INTO Asentamiento_generador VALUES 
(1, 1, 'Amistosa', 'Duque', 'Murallas altas', 'Comercio', 'Sequía', 10, 1, 'Adobe', 'Piedra', 'El Pony Pisador', 'Pony', 'Pisador', 'Grande', 'General', 'Sótanos', 'Robo', 0, 0, 0, 0, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, NULL, NULL),
(2, 2, 'Hostil', 'Sumo Sacerdote', 'Oscuridad', 'Terror', 'Invasión', 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 10, 10, 3, 10, 20, 15, 'Entrada Cueva', 'Piedra', 2, 'Madera reforzada', 'Pasillo', 'Guardia', 'Cuadrada', 2, 'Sala común', 'Sala trono', 12, 12, 'Arco', 'Espiral'),
(3, 3, 'Neutral', 'Reina Araña', 'Telarañas', 'Magia', 'Monstruos', 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 50, 50, 1, 5, 5, 2, 'Claro bosque', 'Tierra', 1, 'Rama tejida', 'Sala', 'Almacén', 'Redonda', 1, 'Nido', 'Almacén huevos', 55, 55, 'Agujero', 'Rampa');

-- Sala_generada
INSERT INTO Sala_generada (id_sala_generada, id_asentamiento_generador, id_area_proposito_sala) VALUES 
(1, 2, 1),
(2, 2, 2),
(3, 3, 3);

-- Sala_proposito_contenido (Intermedia clave para posicionamiento)
INSERT INTO Sala_proposito_contenido (id_sala_proposito_contenido, id_sala_generada, id_evento_asentamiento) VALUES 
(1, 1, 3), -- Sala 1 sin evento
(2, 2, 1), -- Sala 2 con pinchos
(3, 3, 2); -- Sala 3 con gas

-- 3. TABLAS DE RELACIÓN ENTE-STATS (Con Timestep)

-- Stats_base_jugadores
INSERT INTO Stats_base_jugadores (id_stats_base_jugadores, timestep, id_jugadores, id_stats_base) VALUES 
(1, 1, 1, 1), -- Jugador 1, stats 1 en t1
(2, 1, 2, 2), -- Jugador 2, stats 2 en t1
(3, 1, 3, 3); -- Jugador 3, stats 3 en t1

-- Stats_base_monstruos
INSERT INTO Stats_base_monstruos (id_stats_base_monstruos, timestep, id_monstruos, id_stats_base) VALUES 
(1, 1, 'goblin_001', 3), 
(2, 1, 'dragon_rojo_joven', 2),
(3, 1, 'esqueleto_guerrero', 1);

-- Stats_base_pnj
INSERT INTO Stats_base_pnj (id_stats_base_pnj, timestep, id_pnj, id_stats_base) VALUES 
(1, 1, 1, 1),
(2, 1, 2, 1),
(3, 1, 3, 3);

-- 4. TABLAS DE RELACIÓN ENTE-HECHIZO/OBJETO

-- Jugadores_hechizo_base
INSERT INTO Jugadores_hechizo_base (id_jugadores_hechizo_base, timestep, id_jugadores, id_hechizo_base) VALUES 
(1, 1, 1, 'bola_fuego'),
(2, 1, 2, 'curar_heridas'),
(3, 2, 1, 'invisibilidad'); -- Aprendido en timestep 2

-- Monstruos_hechizo_base
INSERT INTO Monstruos_hechizo_base (id_monstruos_hechizo_base, timestep, id_monstruos, id_hechizo_base) VALUES 
(1, 1, 'dragon_rojo_joven', 'bola_fuego'),
(2, 1, 'esqueleto_guerrero', 'curar_heridas');

-- Pnj_hechizo_base
INSERT INTO Pnj_hechizo_base (id_pnj_hechizo_base, timestep, id_pnj, id_hechizo_base) VALUES 
(1, 1, 2, 'curar_heridas'),
(2, 1, 3, 'invisibilidad'),
(3, 1, 1, 'bola_fuego'); -- Martha la tabernera es peligrosa

-- 5. TABLAS DE RELACIÓN TRIPLE (Objeto-Hechizo-Ente)
-- Asumo que estas tablas representan que un ente tiene un objeto que lanza un hechizo (ej. varita)

INSERT INTO Jugadores_objeto_base_hechizo_base (id_jugadores_objeto_base_hechizo_base, timestep, id_jugadores, id_objeto_base, id_hechizo_base) VALUES 
(1, 1, 1, 3, 'invisibilidad'), -- Anillo que da invisibilidad
(2, 1, 2, 2, 'curar_heridas'), -- Pocion que cura
(3, 1, 3, 1, 'bola_fuego'); -- Espada que lanza fuego

INSERT INTO Monstruos_objeto_base_hechizo_base (id_monstruos_objeto_base_hechizo_base, timestep, id_monstruos, id_objeto_base, id_hechizo_base) VALUES 
(1, 1, 'goblin_001', 2, 'curar_heridas'),
(2, 1, 'esqueleto_guerrero', 1, 'bola_fuego'),
(3, 1, 'dragon_rojo_joven', 3, 'invisibilidad');

INSERT INTO Pnj_objeto_base_hechizo_base (id_pnj_objeto_base_hechizo_base, timestep, id_pnj, id_objeto_base, id_hechizo_base) VALUES 
(1, 1, 1, 2, 'curar_heridas'),
(2, 1, 2, 2, 'curar_heridas'),
(3, 1, 3, 3, 'invisibilidad');


-- 6. POSICIONAMIENTO EN MAPA (Lo más complejo, coordenadas X/Y)

-- Jugadores en sala
INSERT INTO Jugadores_sala_proposito_contenido (id_jugadores_sala_proposito_contenido, timestep, Xpos, Ypos, id_jugadores, id_sala_proposito_contenido) VALUES 
(1, 1, 10.5, 20.0, 1, 1),
(2, 1, 12.0, 21.0, 2, 1),
(3, 2, 15.0, 25.0, 1, 2); -- Jugador 1 se movió a sala 2 en timestep 2

-- Monstruos en sala
INSERT INTO Monstruos_sala_proposito_contenido (id_monstruos_sala_proposito_contenido, timestep, Xpos, Ypos, id_monstruos, id_sala_proposito_contenido) VALUES 
(1, 1, 50.0, 50.0, 'dragon_rojo_joven', 2),
(2, 1, 5.0, 5.0, 'goblin_001', 1),
(3, 2, 6.0, 6.0, 'goblin_001', 1); -- El goblin se movió un poco

-- Pnj en sala
INSERT INTO Pnj_sala_proposito_contenido (id_pnj_sala_proposito_contenido, timestep, Xpos, Ypos, id_pnj, id_sala_proposito_contenido) VALUES 
(1, 1, 0.0, 0.0, 1, 1),
(2, 1, 1.0, 1.0, 2, 1),
(3, 1, 100.0, 100.0, 3, 3);

-- Objetos tirados en el suelo (Hechizos y Objetos-Hechizos)
INSERT INTO Hechizo_base_sala_proposito_contenido (id_hechizo_base_sala_proposito_contenido, timestep, Xpos, Ypos, id_hechizo_base, id_sala_proposito_contenido) VALUES 
(1, 1, 10.0, 10.0, 'bola_fuego', 2), -- Trampa mágica o efecto persistente
(2, 1, 20.0, 20.0, 'invisibilidad', 3),
(3, 2, 10.0, 10.0, 'bola_fuego', 2); 

INSERT INTO Objeto_base_hechizo_base_sala_proposito_contenido (id_objeto_base_hechizo_base_sala_proposito_contenido, timestep, Xpos, Ypos, id_objeto_base, id_hechizo_base, id_sala_proposito_contenido) VALUES 
(1, 1, 5.0, 5.0, 2, 'curar_heridas', 1), -- Una poción en el suelo
(2, 1, 6.0, 6.0, 1, 'bola_fuego', 2), -- Espada mágica en el suelo
(3, 2, 5.0, 5.0, 2, 'curar_heridas', 1); -- Sigue ahí en el tiempo 2INSERT INTO Objeto_base (id_objeto_base, nombre, tipo, rareza, peso, capacidad, clase_armadura_objeto, valor, coste, armadura_annadida, requisito_fuerza, sigilo, tiempo_para_poner_quitar, tipo_arma, propiedad_arma, danno, alcance, encantado, categoria_objeto_magico, quien_fabrico_objeto, historia_objeto, propiedad_menor, peculiaridad, indestructible, beneficio_menor, beneficio_mayor, perjuicio_menor, perjuicio_mayor) VALUES  (1, 'Espada Larga', 'Arma', 'Común', 3, 0, 0, 15, 15, 0, 10, 'Normal', 1, 'Marcial', 'Versátil', 8, 5, 0, NULL, 'Herrero Local', NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL), (2, 'Poción de Curación', 'Poción', 'Común', 1, 0, 0, 50, 50, 0, 0, 'Normal', 1, NULL, NULL, 0, 0, 1, 'Poción', 'Alquimista', NULL, NULL, 'Sabe a fresa', 0, 'Cura 2d4+2', NULL, NULL, NULL), (3, 'Anillo de Protección', 'Anillo', 'Raro', 0, 0, 1, 500, 500, 1, 0, 'Normal', 1, NULL, NULL, 0, 0, 1, 'Anillo', 'Mago Antiguo', 'Encontrado en ruinas', 'Brilla', NULL, 1, '+1 CA', NULL, NULL, NULL)
