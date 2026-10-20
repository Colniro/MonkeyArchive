-- MonkeyArchive Datenbank Schema
-- Nur Struktur (CREATE TABLE), keine Daten.
-- Dialekt: MySQL / MariaDB

CREATE DATABASE IF NOT EXISTS `MonkeyArchive`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE `MonkeyArchive`;

-- Reihenfolge der DROPs ist umgekehrt zu den CREATEs, wegen Foreign Keys.
DROP TABLE IF EXISTS `BloonPopsInto`;
DROP TABLE IF EXISTS `BloonImmunities`;
DROP TABLE IF EXISTS `Immunity`;
DROP TABLE IF EXISTS `Bloon`;
DROP TABLE IF EXISTS `BloonCategory`;
DROP TABLE IF EXISTS `FavouriteTowers`;
DROP TABLE IF EXISTS `User`;
DROP TABLE IF EXISTS `TowerUpgradePaths`;
DROP TABLE IF EXISTS `PathLabel`;
DROP TABLE IF EXISTS `Tower`;
DROP TABLE IF EXISTS `TowerCategory`;

-- Towers -------------------------------------------------------------

CREATE TABLE `TowerCategory` (
    `ID`   INT AUTO_INCREMENT PRIMARY KEY,
    `Name` VARCHAR(50) NOT NULL
) ENGINE=InnoDB;

CREATE TABLE `Tower` (
    `ID`            INT AUTO_INCREMENT PRIMARY KEY,
    `CategoryId`    INT NOT NULL,
    `Slug`          VARCHAR(100) NOT NULL UNIQUE,
    `Name`          VARCHAR(100) NOT NULL,
    `Description`   TEXT,
    `ImageFileName` VARCHAR(255),
    `Cost`          INT NOT NULL,
    `Damage`        INT NOT NULL,
    `Pierce`        INT NOT NULL,
    `Range`         INT,
    `GameVersion`   VARCHAR(20),
    FOREIGN KEY (`CategoryId`) REFERENCES `TowerCategory`(`ID`)
) ENGINE=InnoDB;

CREATE TABLE `PathLabel` (
    `ID`   INT AUTO_INCREMENT PRIMARY KEY,
    `Name` VARCHAR(20) NOT NULL
) ENGINE=InnoDB;

CREATE TABLE `TowerUpgradePaths` (
    `ID`          INT AUTO_INCREMENT PRIMARY KEY,
    `TowerId`     INT NOT NULL,
    `PathLabelId` INT NOT NULL,
    `Name`        VARCHAR(100) NOT NULL,
    `Description` TEXT,
    `Price`       INT NOT NULL,
    FOREIGN KEY (`TowerId`) REFERENCES `Tower`(`ID`),
    FOREIGN KEY (`PathLabelId`) REFERENCES `PathLabel`(`ID`)
) ENGINE=InnoDB;

-- User + Favoriten -----------------------------------------------------

CREATE TABLE `User` (
    `ID`               INT AUTO_INCREMENT PRIMARY KEY,
    `Username`         VARCHAR(50) NOT NULL UNIQUE,
    `Password`         VARCHAR(255) NOT NULL,
    `SecurityQuestion` VARCHAR(255) NOT NULL,
    `Answer`           VARCHAR(255) NOT NULL
) ENGINE=InnoDB;

CREATE TABLE `FavouriteTowers` (
    `UserID`  INT NOT NULL,
    `TowerID` INT NOT NULL,
    PRIMARY KEY (`UserID`, `TowerID`),
    FOREIGN KEY (`UserID`) REFERENCES `User`(`ID`),
    FOREIGN KEY (`TowerID`) REFERENCES `Tower`(`ID`)
) ENGINE=InnoDB;

-- Bloons ---------------------------------------------------------------

CREATE TABLE `BloonCategory` (
    `ID`   INT AUTO_INCREMENT PRIMARY KEY,
    `Name` VARCHAR(50) NOT NULL
) ENGINE=InnoDB;

CREATE TABLE `Bloon` (
    `ID`            INT AUTO_INCREMENT PRIMARY KEY,
    `CategoryId`    INT NOT NULL,
    `Slug`          VARCHAR(100) NOT NULL UNIQUE,
    `Name`          VARCHAR(100) NOT NULL,
    `Description`   TEXT,
    `ImageFileName` VARCHAR(255),
    `ColorHex`      VARCHAR(50),
    `ImageScale`    DECIMAL(4,2) NOT NULL DEFAULT 1.0,
    `Rbe`           INT NOT NULL,
    `Speed`         DECIMAL(6,2) NOT NULL,
    `IsCamo`        BOOLEAN NOT NULL DEFAULT FALSE,
    `IsLead`        BOOLEAN NOT NULL DEFAULT FALSE,
    `Layer`         INT NOT NULL,
    `GameVersion`   VARCHAR(20),
    `PopsIntoNote`  VARCHAR(255),
    FOREIGN KEY (`CategoryId`) REFERENCES `BloonCategory`(`ID`)
) ENGINE=InnoDB;

CREATE TABLE `Immunity` (
    `ID`   INT AUTO_INCREMENT PRIMARY KEY,
    `Name` VARCHAR(50) NOT NULL
) ENGINE=InnoDB;

CREATE TABLE `BloonImmunities` (
    `BloonId`     INT NOT NULL,
    `ImmunityId`  INT NOT NULL,
    PRIMARY KEY (`BloonId`, `ImmunityId`),
    FOREIGN KEY (`BloonId`) REFERENCES `Bloon`(`ID`),
    FOREIGN KEY (`ImmunityId`) REFERENCES `Immunity`(`ID`)
) ENGINE=InnoDB;

CREATE TABLE `BloonPopsInto` (
    `ID`           INT AUTO_INCREMENT PRIMARY KEY,
    `BloonId`      INT NOT NULL,
    `ChildBloonId` INT NOT NULL,
    `Count`        INT NOT NULL,
    FOREIGN KEY (`BloonId`) REFERENCES `Bloon`(`ID`),
    FOREIGN KEY (`ChildBloonId`) REFERENCES `Bloon`(`ID`)
) ENGINE=InnoDB;
