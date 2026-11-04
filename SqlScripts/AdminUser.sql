-- Legt den Admin Account an.
-- Nur dieser Account kommt auf /Admin, siehe AdminController.AdminUsername.
-- Einmal ausfuehren, nachdem CreateDB.sql gelaufen ist.
--
-- Login: Benutzername admin, Passwort admin, Antwort auf die Sicherheitsfrage banana.
-- In der DB stehen Passwort und Antwort nicht im Klartext, sondern als Hash
-- (PBKDF2 mit SHA256, siehe Helpers/PasswordHelper.cs). Die zwei Werte unten
-- sind genau diese Hashes. Nach dem ersten Login das Passwort auf /Admin/Users
-- aendern.
--
-- INSERT IGNORE: wenn es den Account schon gibt, passiert gar nichts. Ein
-- bestehendes Passwort wird also nicht ueberschrieben, wenn man das Skript
-- zweimal laufen laesst.

USE `MonkeyArchive`;

INSERT IGNORE INTO `User` (`Username`, `Password`, `SecurityQuestion`, `Answer`)
VALUES ('admin',
        'PBKDF2$600000$keMf5jzIOSuQLJYwDThpoQ==$Kaa3qWwvy/IjchXkaKtfFzwjijE1/Icw+iagEG+dpnQ=',
        'What was your first pet\'s name?',
        'PBKDF2$600000$pxl8WrKgc/MfhDUjMZf7vg==$Kp06IrLcwj/O83cBvufoAk4WGycRRjGGOdsTcuh8raE=');

SELECT `ID`, `Username`, `SecurityQuestion` FROM `User` WHERE `Username` = 'admin';
