-- MySQL dump 10.13  Distrib 8.0.43, for Win64 (x86_64)
--
-- Host: foodhub-db.cglwkqe4o19a.us-east-1.rds.amazonaws.com    Database: foodhub
-- ------------------------------------------------------
-- Server version	8.0.42

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;
SET @MYSQLDUMP_TEMP_LOG_BIN = @@SESSION.SQL_LOG_BIN;
SET @@SESSION.SQL_LOG_BIN= 0;

--
-- GTID state at the beginning of the backup 
--

SET @@GLOBAL.GTID_PURGED=/*!80000 '+'*/ '';

--
-- Table structure for table `AspNetRoleClaims`
--

DROP TABLE IF EXISTS `AspNetRoleClaims`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `AspNetRoleClaims` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `RoleId` varchar(450) NOT NULL,
  `ClaimType` varchar(256) DEFAULT NULL,
  `ClaimValue` varchar(256) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `AspNetRoleClaims`
--

LOCK TABLES `AspNetRoleClaims` WRITE;
/*!40000 ALTER TABLE `AspNetRoleClaims` DISABLE KEYS */;
/*!40000 ALTER TABLE `AspNetRoleClaims` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `AspNetRoles`
--

DROP TABLE IF EXISTS `AspNetRoles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `AspNetRoles` (
  `Id` varchar(450) NOT NULL,
  `Name` varchar(256) DEFAULT NULL,
  `NormalizedName` varchar(256) DEFAULT NULL,
  `ConcurrencyStamp` text,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `AspNetRoles`
--

LOCK TABLES `AspNetRoles` WRITE;
/*!40000 ALTER TABLE `AspNetRoles` DISABLE KEYS */;
INSERT INTO `AspNetRoles` VALUES ('0835722a-b6eb-4419-b096-92de6fb3dded','Customer','CUSTOMER',NULL),('a0a70753-441a-498d-a5ac-a1f1031693b2','Admin','ADMIN',NULL);
/*!40000 ALTER TABLE `AspNetRoles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `AspNetUserClaims`
--

DROP TABLE IF EXISTS `AspNetUserClaims`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `AspNetUserClaims` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` varchar(450) NOT NULL,
  `ClaimType` varchar(256) DEFAULT NULL,
  `ClaimValue` varchar(256) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `AspNetUserClaims`
--

LOCK TABLES `AspNetUserClaims` WRITE;
/*!40000 ALTER TABLE `AspNetUserClaims` DISABLE KEYS */;
/*!40000 ALTER TABLE `AspNetUserClaims` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `AspNetUserLogins`
--

DROP TABLE IF EXISTS `AspNetUserLogins`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `AspNetUserLogins` (
  `LoginProvider` varchar(128) NOT NULL,
  `ProviderKey` varchar(128) NOT NULL,
  `ProviderDisplayName` varchar(256) DEFAULT NULL,
  `UserId` varchar(450) NOT NULL,
  PRIMARY KEY (`LoginProvider`,`ProviderKey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `AspNetUserLogins`
--

LOCK TABLES `AspNetUserLogins` WRITE;
/*!40000 ALTER TABLE `AspNetUserLogins` DISABLE KEYS */;
/*!40000 ALTER TABLE `AspNetUserLogins` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `AspNetUserRoles`
--

DROP TABLE IF EXISTS `AspNetUserRoles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `AspNetUserRoles` (
  `UserId` varchar(191) NOT NULL,
  `RoleId` varchar(191) NOT NULL,
  PRIMARY KEY (`UserId`,`RoleId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `AspNetUserRoles`
--

LOCK TABLES `AspNetUserRoles` WRITE;
/*!40000 ALTER TABLE `AspNetUserRoles` DISABLE KEYS */;
INSERT INTO `AspNetUserRoles` VALUES ('5a633362-9110-4a2f-b159-f79f0a893fd4','0835722a-b6eb-4419-b096-92de6fb3dded'),('6125f118-a5a6-4f36-9a35-40fe7d396191','0835722a-b6eb-4419-b096-92de6fb3dded'),('e2fad164-b03f-43bf-a7ed-b47f19d4527c','a0a70753-441a-498d-a5ac-a1f1031693b2'),('e6f5533f-d83e-4622-85e7-d7ab8fbe494b','0835722a-b6eb-4419-b096-92de6fb3dded'),('f753db3f-65b6-4030-8176-b8a033f2fc5c','0835722a-b6eb-4419-b096-92de6fb3dded');
/*!40000 ALTER TABLE `AspNetUserRoles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `AspNetUserTokens`
--

DROP TABLE IF EXISTS `AspNetUserTokens`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `AspNetUserTokens` (
  `UserId` varchar(450) NOT NULL,
  `LoginProvider` varchar(128) NOT NULL,
  `Name` varchar(128) NOT NULL,
  `Value` text,
  PRIMARY KEY (`UserId`,`LoginProvider`,`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `AspNetUserTokens`
--

LOCK TABLES `AspNetUserTokens` WRITE;
/*!40000 ALTER TABLE `AspNetUserTokens` DISABLE KEYS */;
/*!40000 ALTER TABLE `AspNetUserTokens` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `AspNetUsers`
--

DROP TABLE IF EXISTS `AspNetUsers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `AspNetUsers` (
  `Id` varchar(450) NOT NULL,
  `UserName` varchar(256) NOT NULL,
  `NormalizedUserName` varchar(256) DEFAULT NULL,
  `Email` varchar(256) DEFAULT NULL,
  `NormalizedEmail` varchar(256) DEFAULT NULL,
  `EmailConfirmed` tinyint(1) DEFAULT '0',
  `PasswordHash` text,
  `SecurityStamp` text,
  `ConcurrencyStamp` text,
  `PhoneNumber` varchar(50) DEFAULT NULL,
  `PhoneNumberConfirmed` tinyint(1) DEFAULT '0',
  `TwoFactorEnabled` tinyint(1) DEFAULT '0',
  `LockoutEnd` datetime DEFAULT NULL,
  `LockoutEnabled` tinyint(1) DEFAULT '0',
  `AccessFailedCount` int DEFAULT '0',
  `FullName` varchar(256) DEFAULT NULL,
  `Address` varchar(500) DEFAULT NULL,
  `ProfileImage` varchar(500) DEFAULT NULL,
  `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `AspNetUsers`
--

LOCK TABLES `AspNetUsers` WRITE;
/*!40000 ALTER TABLE `AspNetUsers` DISABLE KEYS */;
INSERT INTO `AspNetUsers` VALUES ('5a633362-9110-4a2f-b159-f79f0a893fd4','presh1989@gmail.com','PRESH1989@GMAIL.COM','presh1989@gmail.com','PRESH1989@GMAIL.COM',0,'AQAAAAIAAYagAAAAELpRqtE3MUFM44UGDhsb2Dk3e1hasYtyd4eIhTFJmxzxjOGH+Q3cPbL004wQVl+gYQ==','E7IHIQVGM7UCMHFMWACWO6WTNHA5VOMB','11418821-744a-4532-9bbf-705ee61e16df',NULL,0,0,NULL,1,0,'Prasantha Amarasinghe','','','2026-01-22 01:22:13'),('6125f118-a5a6-4f36-9a35-40fe7d396191','kiara@gmail.com','KIARA@GMAIL.COM','kiara@gmail.com','KIARA@GMAIL.COM',0,'AQAAAAIAAYagAAAAENlIP75gviAf6/l8p8sTFOTAm/DnkteeqVQDC9o1IaIL0pC5MiL7QDF9PNv1QrGXtw==','EO7DPFLB4E7ACS5QR4QF73L7PLTEM32D','5b027349-3ce1-4d8b-9406-969d2d846b4d',NULL,0,0,NULL,1,0,'Kiara Fernando','','','2026-01-22 02:30:32'),('e2fad164-b03f-43bf-a7ed-b47f19d4527c','durangirosa001@gmail.com','DURANGIROSA001@GMAIL.COM','durangirosa001@gmail.com','DURANGIROSA001@GMAIL.COM',0,'AQAAAAIAAYagAAAAECkiuWgKtg9hRYaZGKrdzKYS/01iJX4fNFCyWKQNplQf6BMmSKslCCU8yPrAOVNSnw==','2FB4FPHOSP7QOVOUN3WKGUZEQ4EY2IAF','0807a171-d427-4f13-9633-da63403f2ca4',NULL,0,0,NULL,1,0,'Durangi Rosa','','','2025-11-20 11:35:27'),('e6f5533f-d83e-4622-85e7-d7ab8fbe494b','annacat@gmail.com','ANNACAT@GMAIL.COM','annacat@gmail.com','ANNACAT@GMAIL.COM',0,'AQAAAAIAAYagAAAAEKFQwnTXa1RyohaYvVRUdmAdNr3tlY5aAc5FbjRdbXsKw1kK/WPh8Uj1Cfe8OV4GwA==','PEYBYPZUO475D3KOMJKNI5VCW6S4H3XN','7e7d3489-5969-4153-8772-5b532f1d37fd',NULL,0,0,NULL,1,0,'Anna Cat','','','2025-11-07 20:50:36'),('f753db3f-65b6-4030-8176-b8a033f2fc5c','amayap99@gmail.com','AMAYAP99@GMAIL.COM','amayap99@gmail.com','AMAYAP99@GMAIL.COM',0,'AQAAAAIAAYagAAAAEFBroVq/U3IfYWLB0wbjjTSuPWZGZKK91Eo1nUBZAHPlFFCRQiwzQhP9K9p29DTXag==','ZY6VSZMPMCUZPAR6YB7IPKHCLJHYUNK3','e11109c6-e7ee-4049-97e4-d9dd74d92bbe',NULL,0,0,NULL,1,0,'Amaya Perera','','','2026-01-21 23:33:45');
/*!40000 ALTER TABLE `AspNetUsers` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Beverages`
--

DROP TABLE IF EXISTS `Beverages`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Beverages` (
  `Id` varchar(10) NOT NULL,
  `Name` varchar(150) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `ImageName` varchar(255) DEFAULT NULL,
  `Price` decimal(10,2) NOT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Beverages`
--

LOCK TABLES `Beverages` WRITE;
/*!40000 ALTER TABLE `Beverages` DISABLE KEYS */;
INSERT INTO `Beverages` VALUES ('BVG-0001','Coca Cola','Classic refreshing cola drink.','cocacola.jpeg',2.49,'2025-10-02 17:59:52'),('BVG-0002','Fanta','Fruity orange-flavored soft drink.','fanta.jpg',2.29,'2025-10-02 17:59:52'),('BVG-0003','Iced Tea','Chilled refreshing iced tea with a hint of lemon.','icedtea.jpg',2.19,'2025-10-02 17:59:52'),('BVG-0004','Orange Juice','Fresh and tangy orange juice.','orangeJuice.jpg',2.59,'2025-10-02 17:59:52'),('BVG-0005','Sprite','Crisp lemon-lime soda for instant refreshment.','sprite.jpg',2.39,'2025-10-02 17:59:52'),('BVG-0006','Pepsi','Pepsi. The Choice of a New Generation','pepsi.jpg',2.00,'2025-11-11 15:12:43');
/*!40000 ALTER TABLE `Beverages` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `CartItems`
--

DROP TABLE IF EXISTS `CartItems`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `CartItems` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Code` varchar(45) NOT NULL,
  `ProductId` varchar(10) NOT NULL,
  `ProductName` varchar(255) DEFAULT NULL,
  `Type` varchar(100) DEFAULT NULL,
  `Quantity` int DEFAULT '1',
  `Price` decimal(10,2) DEFAULT NULL,
  `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=21 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `CartItems`
--

LOCK TABLES `CartItems` WRITE;
/*!40000 ALTER TABLE `CartItems` DISABLE KEYS */;
INSERT INTO `CartItems` VALUES (3,'FEX-20260120-42198','PZ-0002','BBQ Chicken','Thin Crust',1,10.99,'2026-01-21 01:24:10','2026-01-20 19:54:10'),(4,'FEX-20260120-42198','BVG-0002','Fanta','beverage',1,2.29,'2026-01-21 01:24:10','2026-01-20 19:54:10'),(12,'FEX-20260121-79132','PZ-0003','Pepperoni','Thin Crust',1,9.99,'2026-01-21 23:34:29','2026-01-21 18:04:30'),(13,'FEX-20260121-79132','PZ-0001','Margherita','Thin Crust',1,8.99,'2026-01-21 23:34:29','2026-01-21 18:04:30'),(14,'FEX-20260121-79132','BVG-0001','Coca Cola','beverage',1,2.49,'2026-01-21 23:34:29','2026-01-21 18:04:30'),(17,'FEX-20260121-39749','PZ-0005','Hawaiian','Thin Crust',1,9.49,'2026-01-22 01:22:38','2026-01-21 19:52:39'),(18,'FEX-20260121-39749','BVG-0003','Iced Tea','beverage',1,2.19,'2026-01-22 01:22:38','2026-01-21 19:52:39'),(20,'FEX-20260121-32887','PZ-0004','Buffalo Chicken','Thin Crust',2,11.49,'2026-01-22 02:31:09','2026-01-21 21:01:11');
/*!40000 ALTER TABLE `CartItems` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Carts`
--

DROP TABLE IF EXISTS `Carts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Carts` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` varchar(450) NOT NULL,
  `Code` varchar(45) NOT NULL,
  `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `Status` varchar(50) DEFAULT 'Active',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Carts`
--

LOCK TABLES `Carts` WRITE;
/*!40000 ALTER TABLE `Carts` DISABLE KEYS */;
INSERT INTO `Carts` VALUES (1,'e6f5533f-d83e-4622-85e7-d7ab8fbe494b','FEX-20260120-42198','2026-01-21 01:24:06','2026-01-20 20:04:33','Inactive'),(2,'f753db3f-65b6-4030-8176-b8a033f2fc5c','FEX-20260121-79132','2026-01-21 23:34:11','2026-01-21 18:04:58','Inactive'),(3,'5a633362-9110-4a2f-b159-f79f0a893fd4','FEX-20260121-39749','2026-01-22 01:22:30','2026-01-21 19:52:50','Inactive'),(4,'6125f118-a5a6-4f36-9a35-40fe7d396191','FEX-20260121-32887','2026-01-22 02:31:06','2026-01-21 21:01:27','Inactive');
/*!40000 ALTER TABLE `Carts` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `DeliveryAttendance`
--

DROP TABLE IF EXISTS `DeliveryAttendance`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `DeliveryAttendance` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `DeliveryPersonId` varchar(50) NOT NULL,
  `Date` date NOT NULL,
  `IsPresent` tinyint(1) DEFAULT '0',
  `CheckInTime` datetime DEFAULT NULL,
  `CheckOutTime` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `idx_attendance_date` (`Date`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `DeliveryAttendance`
--

LOCK TABLES `DeliveryAttendance` WRITE;
/*!40000 ALTER TABLE `DeliveryAttendance` DISABLE KEYS */;
INSERT INTO `DeliveryAttendance` VALUES (1,'EmpDel002','2026-01-21',1,'2026-01-21 01:25:22',NULL),(2,'EmpDel002','2026-01-22',1,'2026-01-22 00:02:37','2026-01-22 14:34:05'),(3,'EmpDel003','2026-01-22',1,'2026-01-22 14:45:32','2026-01-22 14:45:49');
/*!40000 ALTER TABLE `DeliveryAttendance` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `DeliveryInfo`
--

DROP TABLE IF EXISTS `DeliveryInfo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `DeliveryInfo` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Code` varchar(45) NOT NULL,
  `Name` varchar(255) DEFAULT NULL,
  `Email` varchar(255) DEFAULT NULL,
  `Phone` varchar(50) DEFAULT NULL,
  `Address` text,
  `DeliveryNotes` text,
  `DeliveryStatus` varchar(50) DEFAULT NULL,
  `DeliveredAt` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `DeliveryInfo`
--

LOCK TABLES `DeliveryInfo` WRITE;
/*!40000 ALTER TABLE `DeliveryInfo` DISABLE KEYS */;
INSERT INTO `DeliveryInfo` VALUES (1,'FEX-20260120-42198','Eliorah Rosa','maheesharosa@gmail.com','0764836004','176 Seeduwa North, Seeduwa',NULL,'Delivered','2026-01-21 21:06:37'),(2,'FEX-20260121-79132','Amaya Perera','maheesharosa@gmail.com','0764836004',' Kotugoda Road, Seeduwa 11410, Sri Lanka.',NULL,'Delivered','2026-01-22 00:00:00'),(3,'FEX-20260121-39749','Sisira Rosa','maheesharosa@gmail.com','0764836004',' SLIIT Malabe Campus, New Kandy Rd, Malabe 10115',NULL,'Delivered','2026-01-21 21:11:32'),(4,'FEX-20260121-32887','Melisha Fernando','maheesharosa@gmail.com','0764836004','176 Seeduwa North, Seeduwa',NULL,'Delivered','2026-01-22 00:00:00');
/*!40000 ALTER TABLE `DeliveryInfo` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `DeliveryOrderAssignments`
--

DROP TABLE IF EXISTS `DeliveryOrderAssignments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `DeliveryOrderAssignments` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `OrderCode` varchar(50) NOT NULL,
  `DeliveryPersonId` varchar(50) NOT NULL,
  `AssignedAt` datetime NOT NULL,
  `PickedUpAt` datetime DEFAULT NULL,
  `DeliveredAt` datetime DEFAULT NULL,
  `Status` varchar(30) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `idx_order_code` (`OrderCode`),
  KEY `idx_driver_id` (`DeliveryPersonId`),
  KEY `idx_status` (`Status`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `DeliveryOrderAssignments`
--

LOCK TABLES `DeliveryOrderAssignments` WRITE;
/*!40000 ALTER TABLE `DeliveryOrderAssignments` DISABLE KEYS */;
INSERT INTO `DeliveryOrderAssignments` VALUES (1,'FEX-20260120-42198','EmpDel002','2026-01-22 00:00:00',NULL,NULL,'Assigned'),(2,'FEX-20260121-79132','EmpDel002','2026-01-22 00:00:00',NULL,NULL,'Assigned'),(3,'FEX-20260121-39749','EmpDel002','2026-01-22 00:00:00',NULL,NULL,'Assigned'),(4,'FEX-20260121-32887','EmpDel002','2026-01-22 00:00:00',NULL,NULL,'Assigned');
/*!40000 ALTER TABLE `DeliveryOrderAssignments` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `DeliveryPerson`
--

DROP TABLE IF EXISTS `DeliveryPerson`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `DeliveryPerson` (
  `id` varchar(50) NOT NULL,
  `Name` varchar(100) DEFAULT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `NIC` varchar(20) DEFAULT NULL,
  `Password` varchar(255) DEFAULT NULL,
  `PhoneNumber` varchar(20) DEFAULT NULL,
  `IsActive` tinyint DEFAULT NULL,
  `FingerprintEnabled` tinyint DEFAULT NULL,
  `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `DeliveryPerson`
--

LOCK TABLES `DeliveryPerson` WRITE;
/*!40000 ALTER TABLE `DeliveryPerson` DISABLE KEYS */;
INSERT INTO `DeliveryPerson` VALUES ('EmpDel001','Sithum Fernando','sithumf@gmail.com','95017590086v','AQAAAAIAAYagAAAAELRetqUGMBLTY5md1Wlp2hvjaLfqycbSRQaz+6QX4xOTXZt/Z6xZ8VV1ABuG8RYnDQ==','0764836084',1,1,'2025-12-09 05:56:18','2026-01-05 07:01:52'),('EmpDel002','Akila Abeyratne','akila95@gmail.com','950217590086v','AQAAAAIAAYagAAAAEJIYvA5Hhh/Lu99j/EKlMPSx+VI/+lEtF3CGCQkDrQsPYL6ZlVePv62pbxZs7sD19g==','0764836004',1,1,'2025-12-05 07:37:27','2026-01-05 07:02:40'),('EmpDel003','Pasan Prasanna','pasan123@gmail.com','95000090086v','AQAAAAIAAYagAAAAEF2K3ZNh8fV+QvDrJ65QACLBQswb6FAgt3ebvFFHPG1a9FCcqHT25yqZwnA5n4odzQ==','0764836004',1,1,'2026-01-05 09:52:03','2026-01-05 09:52:00'),('EmpDel004','Kalum Sampath','kalum12@gmail.com','95055590086v','AQAAAAIAAYagAAAAECd4iOIjCoiCKlFjGKHSCo7XHDudan+eTiGTazWuz4CNGXvM4EeRYfi8tQGAPEaKGw==','0764836004',1,1,'2026-01-05 09:53:35','2026-01-05 09:53:33');
/*!40000 ALTER TABLE `DeliveryPerson` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `FoodItems`
--

DROP TABLE IF EXISTS `FoodItems`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `FoodItems` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(150) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `Price` decimal(10,2) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `FoodItems`
--

LOCK TABLES `FoodItems` WRITE;
/*!40000 ALTER TABLE `FoodItems` DISABLE KEYS */;
/*!40000 ALTER TABLE `FoodItems` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `OrderItems`
--

DROP TABLE IF EXISTS `OrderItems`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `OrderItems` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Code` varchar(45) NOT NULL,
  `ProductId` varchar(10) NOT NULL,
  `ProductName` varchar(255) DEFAULT NULL,
  `ProductType` varchar(100) DEFAULT NULL,
  `Quantity` int DEFAULT NULL,
  `UnitPrice` decimal(10,2) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `OrderItems`
--

LOCK TABLES `OrderItems` WRITE;
/*!40000 ALTER TABLE `OrderItems` DISABLE KEYS */;
INSERT INTO `OrderItems` VALUES (1,'FEX-20260120-42198','PZ-0002','BBQ Chicken','Thin Crust',1,10.99),(2,'FEX-20260120-42198','BVG-0002','Fanta','beverage',1,2.29),(3,'FEX-20260121-79132','PZ-0003','Pepperoni','Thin Crust',1,9.99),(4,'FEX-20260121-79132','PZ-0001','Margherita','Thin Crust',1,8.99),(5,'FEX-20260121-79132','BVG-0001','Coca Cola','beverage',1,2.49),(6,'FEX-20260121-39749','PZ-0005','Hawaiian','Thin Crust',1,9.49),(7,'FEX-20260121-39749','BVG-0003','Iced Tea','beverage',1,2.19),(8,'FEX-20260121-32887','PZ-0004','Buffalo Chicken','Thin Crust',2,11.49);
/*!40000 ALTER TABLE `OrderItems` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Orders`
--

DROP TABLE IF EXISTS `Orders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Orders` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` varchar(450) NOT NULL,
  `Code` varchar(45) NOT NULL,
  `TotalAmount` decimal(10,2) DEFAULT NULL,
  `Status` varchar(50) DEFAULT 'Pending',
  `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
  `DeliveryPersonId` varchar(50) DEFAULT NULL,
  `AssignedAt` datetime DEFAULT NULL,
  `DeliveredAt` datetime DEFAULT NULL,
  `DeliveryStatus` varchar(30) DEFAULT 'WAITING',
  PRIMARY KEY (`Id`),
  KEY `idx_orders_status` (`DeliveryStatus`),
  KEY `idx_orders_driver` (`DeliveryPersonId`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Orders`
--

LOCK TABLES `Orders` WRITE;
/*!40000 ALTER TABLE `Orders` DISABLE KEYS */;
INSERT INTO `Orders` VALUES (1,'e6f5533f-d83e-4622-85e7-d7ab8fbe494b','FEX-20260120-42198',13.28,'Completed','2026-01-22 01:34:30',NULL,NULL,'2026-01-21 21:06:37','Completed'),(2,'f753db3f-65b6-4030-8176-b8a033f2fc5c','FEX-20260121-79132',21.47,'Completed','2026-01-22 01:34:30',NULL,NULL,'2026-01-22 00:00:00','Completed'),(3,'5a633362-9110-4a2f-b159-f79f0a893fd4','FEX-20260121-39749',11.68,'Completed','2026-01-22 01:22:47',NULL,NULL,'2026-01-21 21:11:32','Completed'),(4,'6125f118-a5a6-4f36-9a35-40fe7d396191','FEX-20260121-32887',22.98,'Completed','2026-01-22 02:31:25',NULL,NULL,'2026-01-22 00:00:00','Completed');
/*!40000 ALTER TABLE `Orders` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Payments`
--

DROP TABLE IF EXISTS `Payments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Payments` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Code` varchar(45) NOT NULL,
  `PaymentMethod` varchar(100) DEFAULT NULL,
  `TransactionId` varchar(255) DEFAULT NULL,
  `Amount` decimal(10,2) DEFAULT NULL,
  `PaymentStatus` varchar(50) DEFAULT 'Pending',
  `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Payments_OrderId` (`Code`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Payments`
--

LOCK TABLES `Payments` WRITE;
/*!40000 ALTER TABLE `Payments` DISABLE KEYS */;
INSERT INTO `Payments` VALUES (1,'FEX-20260120-42198','Cash','Not Required',13.28,'Paid','2026-01-21 01:35:07'),(2,'FEX-20260121-79132','Cash','Not Required',21.47,'Paid','2026-01-21 23:36:26'),(3,'FEX-20260121-39749','Cash','Not Required',11.68,'Paid','2026-01-22 01:26:41'),(4,'FEX-20260121-32887','Cash','Not Required',22.98,'Paid','2026-01-22 02:32:06');
/*!40000 ALTER TABLE `Payments` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `PizzaCrustCategory`
--

DROP TABLE IF EXISTS `PizzaCrustCategory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `PizzaCrustCategory` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `CategoryName` varchar(100) NOT NULL,
  `ExtraCharge` decimal(10,2) DEFAULT NULL,
  `PercentageIncrease` decimal(5,2) DEFAULT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `PizzaCrustCategory`
--

LOCK TABLES `PizzaCrustCategory` WRITE;
/*!40000 ALTER TABLE `PizzaCrustCategory` DISABLE KEYS */;
INSERT INTO `PizzaCrustCategory` VALUES (1,'Thin Crust',NULL,NULL,'2025-10-02 23:12:48'),(2,'Pan Crust',2.00,NULL,'2025-10-02 23:12:58'),(3,'Thick Crust',2.50,NULL,'2025-10-02 23:13:38');
/*!40000 ALTER TABLE `PizzaCrustCategory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `PizzaPrices`
--

DROP TABLE IF EXISTS `PizzaPrices`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `PizzaPrices` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `PizzaId` varchar(10) NOT NULL,
  `CrustId` int NOT NULL,
  `Price` decimal(10,2) NOT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=25 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `PizzaPrices`
--

LOCK TABLES `PizzaPrices` WRITE;
/*!40000 ALTER TABLE `PizzaPrices` DISABLE KEYS */;
INSERT INTO `PizzaPrices` VALUES (1,'PZ-0001',1,8.99,'2025-10-02 23:52:32'),(2,'PZ-0001',2,10.99,'2025-10-02 23:52:32'),(3,'PZ-0001',3,11.49,'2025-10-02 23:52:32'),(4,'PZ-0002',1,10.99,'2025-10-02 23:53:20'),(5,'PZ-0002',2,12.99,'2025-10-02 23:53:20'),(6,'PZ-0002',3,13.49,'2025-10-02 23:53:20'),(7,'PZ-0003',1,9.99,'2025-10-02 23:54:30'),(8,'PZ-0003',2,11.99,'2025-10-02 23:54:30'),(9,'PZ-0003',3,12.49,'2025-10-02 23:54:30'),(10,'PZ-0004',1,11.49,'2025-10-02 23:55:15'),(11,'PZ-0004',2,13.49,'2025-10-02 23:55:15'),(12,'PZ-0004',3,13.99,'2025-10-02 23:55:15'),(13,'PZ-0005',1,9.49,'2025-10-02 23:57:16'),(14,'PZ-0005',2,11.49,'2025-10-02 23:57:16'),(15,'PZ-0005',3,11.99,'2025-10-02 23:57:16'),(16,'PZ-0006',1,11.99,'2025-10-02 23:59:13'),(17,'PZ-0006',2,13.99,'2025-10-02 23:59:13'),(18,'PZ-0006',3,14.49,'2025-10-02 23:59:13'),(19,'PZ-0007',1,9.79,'2025-10-02 23:59:54'),(20,'PZ-0007',2,11.79,'2025-10-02 23:59:54'),(21,'PZ-0007',3,12.29,'2025-10-02 23:59:54'),(22,'PZ-0008',1,16.81,'2025-11-11 14:22:11'),(23,'PZ-0008',2,18.81,'2025-11-11 14:22:11'),(24,'PZ-0008',3,19.31,'2025-11-11 14:22:11');
/*!40000 ALTER TABLE `PizzaPrices` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Pizzas`
--

DROP TABLE IF EXISTS `Pizzas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Pizzas` (
  `Id` varchar(10) NOT NULL,
  `Name` varchar(150) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `ImageName` varchar(255) DEFAULT NULL,
  `BasePrice` decimal(10,2) NOT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Pizzas`
--

LOCK TABLES `Pizzas` WRITE;
/*!40000 ALTER TABLE `Pizzas` DISABLE KEYS */;
INSERT INTO `Pizzas` VALUES ('PZ-0001','Margherita','Classic pizza topped with fresh mozzarella, tomatoes, and basil.','Margherita.jpg',8.99,'2025-10-02 23:52:30'),('PZ-0002','BBQ Chicken','Smoky BBQ sauce topped with grilled chicken, onions, and mozzarella.','BBQ.jpg',10.99,'2025-10-02 23:53:18'),('PZ-0003','Pepperoni','Cheesy pizza loaded with spicy pepperoni slices.','Pepperoni.jpg',9.99,'2025-10-02 23:54:28'),('PZ-0004','Buffalo Chicken','Tangy buffalo sauce, chicken, and mozzarella for a spicy kick.','BuffaloChicken.jpg',11.49,'2025-10-02 23:55:13'),('PZ-0005','Hawaiian','Sweet pineapple chunks and savoury ham on a cheesy base.','pineapple.jpg',9.49,'2025-02-10 23:57:00'),('PZ-0006','Sausage & Pepperoni','Loaded with Italian sausage and pepperoni for meat lovers.','SausagePepperoni.jpg',11.99,'2025-10-02 23:59:11'),('PZ-0007','Veggie Delight','Topped with bell peppers, onions, mushrooms, and olives.','vegPizza.jpg',9.79,'2025-10-02 23:59:52'),('PZ-0008','New-York Style ','Large, thin, and foldable crust that is crisp on the outside and chewy inside, topped with a sweet and tangy tomato sauce and low-moisture mozzarella cheese.','newYorkStyle.jpeg',16.81,'2025-11-11 14:22:09');
/*!40000 ALTER TABLE `Pizzas` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `SpecialItem`
--

DROP TABLE IF EXISTS `SpecialItem`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `SpecialItem` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `SpecialId` varchar(10) DEFAULT NULL,
  `ItemType` varchar(45) DEFAULT NULL,
  `ItemId` varchar(10) DEFAULT NULL,
  `Quantity` int DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `SpecialItem`
--

LOCK TABLES `SpecialItem` WRITE;
/*!40000 ALTER TABLE `SpecialItem` DISABLE KEYS */;
INSERT INTO `SpecialItem` VALUES (1,'SPC-0001','Pizza','PZ-0002',2),(2,'SPC-0001','Beverage','BVG-0001',1),(3,'SPC-0002','Pizza','PZ-0006',1),(6,'SPC-0002','Beverage','BVG-0001',1),(7,'SPC-0003','Pizza','PZ-0007',1),(8,'SPC-0003','Beverage','BVG-0001',1);
/*!40000 ALTER TABLE `SpecialItem` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Specials`
--

DROP TABLE IF EXISTS `Specials`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Specials` (
  `Id` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Title` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Description` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `TotalPrice` decimal(10,2) DEFAULT NULL,
  `DiscountType` varchar(45) DEFAULT NULL,
  `DiscountValue` decimal(10,2) DEFAULT NULL,
  `FinalPrice` decimal(10,2) DEFAULT NULL,
  `ImageName` varchar(255) DEFAULT NULL,
  `CreatedAt` datetime DEFAULT NULL,
  `IsActive` tinyint DEFAULT NULL,
  `StartDate` datetime DEFAULT NULL,
  `EndDate` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Specials`
--

LOCK TABLES `Specials` WRITE;
/*!40000 ALTER TABLE `Specials` DISABLE KEYS */;
INSERT INTO `Specials` VALUES ('SPC-0001','The BBQ Boss Bundle','BBQ Chicken Pizza 1 + Cool Code Medium',24.47,'Percentage',17.00,20.31,'BBQDeal.jpg','2025-11-11 23:54:33',0,'2025-11-01 00:00:00','2025-11-28 00:00:00'),('SPC-0002','The Classic Meat & Pepsi Combo','1 Sausage & Pepperoni Pizza + 1 Coke',14.48,'Percentage',15.00,12.31,'b4711010-2bdb-4a03-8246-a46421a830e0.jpg',NULL,0,'2025-11-14 00:00:00','2025-11-28 00:00:00'),('SPC-0003','Garden Feast with Coke','1 Veggie Pizza + 1 Coke',12.28,'Percentage',10.00,11.05,'VegDeal.jpg',NULL,0,'2025-11-11 00:00:00','2025-11-28 00:00:00');
/*!40000 ALTER TABLE `Specials` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `__EFMigrationsHistory`
--

DROP TABLE IF EXISTS `__EFMigrationsHistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `__EFMigrationsHistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `__EFMigrationsHistory`
--

LOCK TABLES `__EFMigrationsHistory` WRITE;
/*!40000 ALTER TABLE `__EFMigrationsHistory` DISABLE KEYS */;
INSERT INTO `__EFMigrationsHistory` VALUES ('20251107143201_InitialCreate','9.0.9'),('20251107151028_AddCartIdToCartItems','9.0.9'),('20251108201235_CleanSchemaSync','9.0.9'),('20251109050514_RemoveOrderId','9.0.9'),('20251109050942_SyncSchema','9.0.9'),('20251109051253_RemoveOrderIdFinal','9.0.9'),('20251109074117_InitialCreate','9.0.9'),('20251109075520_Baseline','9.0.9'),('20251109084422_Baseline','9.0.9');
/*!40000 ALTER TABLE `__EFMigrationsHistory` ENABLE KEYS */;
UNLOCK TABLES;
SET @@SESSION.SQL_LOG_BIN = @MYSQLDUMP_TEMP_LOG_BIN;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-01-28  0:38:15
