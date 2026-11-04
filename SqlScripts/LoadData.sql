-- Importiert die CSV Dateien in die Tabellen aus CreateDB.sql.
-- Vorher einmal CreateDB.sql ausführen, dieses Skript baut nur Daten rein.
--
-- LOAD DATA LOCAL INFILE braucht oft eine extra Einstellung, sonst blockt der
-- Server das aus Sicherheitsgründen. Vor dem Ausführen einmal pruefen/setzen:
--   SHOW VARIABLES LIKE 'local_infile';
--   SET GLOBAL local_infile = 1;
-- und beim Verbinden mit der mysql Konsole den Flag --local-infile=1 mitgeben:
--   mysql --local-infile=1 -u dein_user -p
--
-- Die Pfade unten sind absolut und zeigen auf meinen Projektordner.
-- Liegt das Projekt woanders, muss man sie vorher anpassen.
--
-- Leere Werte stehen in den CSVs als \N drin (ohne Anführungszeichen).
-- Das erkennt LOAD DATA von sich aus als NULL, da braucht es nichts extra.

USE `MonkeyArchive`;
SET GLOBAL local_infile = 1;
-- Lookup Tabellen zuerst, die haben keine Foreign Keys.

LOAD DATA LOCAL INFILE 'C:\\Desktop\\Schule\\Informatik\\3. Semester\\IPA\\MonkeyArchive\\SqlScripts\\TowerCategory.csv'
INTO TABLE `TowerCategory`
FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 LINES
(ID, Name);

LOAD DATA LOCAL INFILE 'C:\\Desktop\\Schule\\Informatik\\3. Semester\\IPA\\MonkeyArchive\\SqlScripts\\PathLabel.csv'
INTO TABLE `PathLabel`
FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 LINES
(ID, Name);

LOAD DATA LOCAL INFILE 'C:\\Desktop\\Schule\\Informatik\\3. Semester\\IPA\\MonkeyArchive\\SqlScripts\\BloonCategory.csv'
INTO TABLE `BloonCategory`
FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 LINES
(ID, Name);

LOAD DATA LOCAL INFILE 'C:\\Desktop\\Schule\\Informatik\\3. Semester\\IPA\\MonkeyArchive\\SqlScripts\\Immunity.csv'
INTO TABLE `Immunity`
FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 LINES
(ID, Name);

-- Tower braucht TowerCategory, also erst danach.

LOAD DATA LOCAL INFILE 'C:\\Desktop\\Schule\\Informatik\\3. Semester\\IPA\\MonkeyArchive\\SqlScripts\\Tower.csv'
INTO TABLE `Tower`
FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 LINES
(ID, CategoryId, Slug, Name, Description, ImageFileName, Cost, Damage, Pierce, `Range`, GameVersion);

-- TowerUpgradePaths braucht Tower und PathLabel.

LOAD DATA LOCAL INFILE 'C:\\Desktop\\Schule\\Informatik\\3. Semester\\IPA\\MonkeyArchive\\SqlScripts\\TowerUpgradePaths.csv'
INTO TABLE `TowerUpgradePaths`
FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 LINES
(ID, TowerId, PathLabelId, Name, Description, Price);

-- Bloon braucht BloonCategory.

LOAD DATA LOCAL INFILE 'C:\\Desktop\\Schule\\Informatik\\3. Semester\\IPA\\MonkeyArchive\\SqlScripts\\Bloon.csv'
INTO TABLE `Bloon`
FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 LINES
(ID, CategoryId, Slug, Name, Description, ImageFileName, ColorHex, ImageScale, Rbe, Speed, IsCamo, IsLead, Layer, GameVersion, PopsIntoNote);

-- Die zwei Verknuepfungstabellen brauchen Bloon (und Immunity), also zuletzt.

LOAD DATA LOCAL INFILE 'C:\\Desktop\\Schule\\Informatik\\3. Semester\\IPA\\MonkeyArchive\\SqlScripts\\BloonImmunities.csv'
INTO TABLE `BloonImmunities`
FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 LINES
(BloonId, ImmunityId);

LOAD DATA LOCAL INFILE 'C:\\Desktop\\Schule\\Informatik\\3. Semester\\IPA\\MonkeyArchive\\SqlScripts\\BloonPopsInto.csv'
INTO TABLE `BloonPopsInto`
FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 LINES
(ID, BloonId, ChildBloonId, Count);
