-- CREATE SCHEMA dbTest;
-- USE dbTest;
-- DROP SCHEMA dbTest;

CREATE TABLE Log (
 log_timestamp DOUBLE PRIMARY KEY,
 log_difficulty int
);

CREATE TABLE Stat (
 id_stats INT PRIMARY KEY,
 strength INT,
 dexterity INT,
 wisdom INT,
 armor_class INT,
 mana INT
);

CREATE TABLE Player (
id_player INT PRIMARY KEY,
race TINYTEXT,
class TINYTEXT
);

CREATE TABLE Monster (
id_monster INT PRIMARY KEY,
monster_name TINYTEXT,
challenge INT
);

CREATE TABLE Npc (
id_npc INT PRIMARY KEY,
npc_name TINYTEXT,
-- npc_type SET // definir los oficios de npc
npc_dialogue TINYTEXT
);

CREATE TABLE Object (
id_object INT PRIMARY KEY,
object_name TINYTEXT,
damage INT,
armor_class INT,
-- rarity ENUM // definir tipos de rareza
object_value INT,
object_description TINYTEXT
);

/*
CREATE TABLE Spell (
id_spell VARCHAR(255) PRIMARY KEY,
spell_level INT,
mana_requirement INT,
-- area_type ENUM // hay que definir los tipos en el enum
area_a_distance INT,
area_b_distance INT
);
*/
-- Tablas intermedias

CREATE TABLE Stat_player(
PRIMARY KEY (id_stat_player, log_timestamp),

log_timestamp DOUBLE, FOREIGN KEY (log_timestamp) REFERENCES Log (log_timestamp),
id_stat_player INT

);

CREATE TABLE Stat_monster(
PRIMARY KEY (id_stat_monster, log_timestamp),

log_timestamp DOUBLE, FOREIGN KEY (log_timestamp) REFERENCES Log (log_timestamp),
id_stat_monster INT
);

CREATE TABLE Stat_npc(
PRIMARY KEY (id_stat_npc, log_timestamp),

log_timestamp DOUBLE, FOREIGN KEY (log_timestamp) REFERENCES Log (log_timestamp),
id_stat_npc INT
);

/*
CREATE TABLE Spell_player(
);

CREATE TABLE Spell_monster(
);

CREATE TABLE Spell_npc(
);
*/
CREATE TABLE Object_player(
PRIMARY KEY (id_object_player, log_timestamp),

log_timestamp DOUBLE, FOREIGN KEY (log_timestamp) REFERENCES Log (log_timestamp),
id_object_player INT
);

CREATE TABLE Object_monster(
PRIMARY KEY (id_object_monster, log_timestamp),

log_timestamp DOUBLE, FOREIGN KEY (log_timestamp) REFERENCES Log (log_timestamp),
id_object_monster INT
);

CREATE TABLE Object_npc(
PRIMARY KEY (id_object_npc, log_timestamp),

log_timestamp DOUBLE, FOREIGN KEY (log_timestamp) REFERENCES Log (log_timestamp),
id_object_npc INT
);

-- PLACE tables
CREATE TABLE Place (
id_place VARCHAR(255) PRIMARY KEY
);

CREATE TABLE Dungeon (
PRIMARY KEY(id_dungeon, dungeon_name),

id_dungeon INT,
dungeon_name VARCHAR(255)
);

CREATE TABLE Settlement (

);

-- Interconnection tables

CREATE TABLE Place_dungeon (

);

CREATE TABLE Place_settlement (

);

CREATE TABLE Place_player(

);

CREATE TABLE Place_monster(

);

CREATE TABLE Place_npc(

);

CREATE TABLE Place_object(

);