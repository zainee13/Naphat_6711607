
/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

/*!40000 DROP DATABASE IF EXISTS `csi402db`*/;

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `csi402db` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `csi402db`;
DROP TABLE IF EXISTS `booking`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `booking` (
  `BookingId` varchar(30) COLLATE utf8mb4_unicode_ci NOT NULL,
  `UserId` int NOT NULL,
  `RoomId` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `BookingDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `StartDate` date NOT NULL,
  `EndDate` date NOT NULL,
  `TotalRent` decimal(12,2) NOT NULL,
  `Deposit` decimal(12,2) NOT NULL,
  `PromoCode` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `DiscountAmount` decimal(12,2) DEFAULT NULL,
  `GoodsCategory` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Note` text COLLATE utf8mb4_unicode_ci,
  `BookingStatus` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`BookingId`),
  KEY `fk_booking_user` (`UserId`),
  KEY `fk_booking_room` (`RoomId`),
  CONSTRAINT `fk_booking_room` FOREIGN KEY (`RoomId`) REFERENCES `room` (`RoomId`),
  CONSTRAINT `fk_booking_user` FOREIGN KEY (`UserId`) REFERENCES `user` (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

LOCK TABLES `booking` WRITE;
/*!40000 ALTER TABLE `booking` DISABLE KEYS */;
INSERT INTO `booking` VALUES ('BK-2026-001',16,'M-02','2026-11-18 00:25:15','2026-11-18','2027-02-17',114000.00,38000.00,NULL,NULL,'อาหารและเครื่องดื่ม','ผู้ติดต่อ: kuy gg\nบริการวันย้ายเข้า: โฟล์คลิฟต์ยกพาเลท (ฟรี)','Active');
/*!40000 ALTER TABLE `booking` ENABLE KEYS */;
UNLOCK TABLES;
DROP TABLE IF EXISTS `invoice`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `invoice` (
  `InvoiceId` varchar(30) COLLATE utf8mb4_unicode_ci NOT NULL,
  `BookingId` varchar(30) COLLATE utf8mb4_unicode_ci NOT NULL,
  `RentAmount` decimal(12,2) NOT NULL,
  `DepositAmount` decimal(12,2) NOT NULL,
  `VatAmount` decimal(12,2) NOT NULL,
  `TotalAmount` decimal(12,2) NOT NULL,
  `IssuedDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `IssuedBy` int NOT NULL,
  PRIMARY KEY (`InvoiceId`),
  KEY `fk_invoice_booking` (`BookingId`),
  KEY `fk_invoice_issuedby` (`IssuedBy`),
  CONSTRAINT `fk_invoice_booking` FOREIGN KEY (`BookingId`) REFERENCES `booking` (`BookingId`),
  CONSTRAINT `fk_invoice_issuedby` FOREIGN KEY (`IssuedBy`) REFERENCES `user` (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

LOCK TABLES `invoice` WRITE;
/*!40000 ALTER TABLE `invoice` DISABLE KEYS */;
INSERT INTO `invoice` VALUES ('INV-2026-0001','BK-2026-001',38000.00,38000.00,0.00,76000.00,'2026-11-18 00:25:15',6);
/*!40000 ALTER TABLE `invoice` ENABLE KEYS */;
UNLOCK TABLES;
DROP TABLE IF EXISTS `labstudent`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `labstudent` (
  `StdId` varchar(10) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `StdPassword` varchar(30) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `StdName` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `StdLastname` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  PRIMARY KEY (`StdId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

LOCK TABLES `labstudent` WRITE;
/*!40000 ALTER TABLE `labstudent` DISABLE KEYS */;
INSERT INTO `labstudent` VALUES ('1341','1111','213123','12313');
/*!40000 ALTER TABLE `labstudent` ENABLE KEYS */;
UNLOCK TABLES;
DROP TABLE IF EXISTS `payment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `payment` (
  `PaymentId` int NOT NULL AUTO_INCREMENT,
  `BookingId` varchar(30) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Amount` decimal(12,2) NOT NULL,
  `PaymentMethod` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `PaymentDate` datetime DEFAULT NULL,
  `PaymentStatus` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `SlipImageUrl` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `DueDate` datetime DEFAULT NULL,
  `VerifiedBy` int DEFAULT NULL,
  `VerifiedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`PaymentId`),
  KEY `fk_payment_booking` (`BookingId`),
  KEY `fk_payment_verifiedby` (`VerifiedBy`),
  CONSTRAINT `fk_payment_booking` FOREIGN KEY (`BookingId`) REFERENCES `booking` (`BookingId`),
  CONSTRAINT `fk_payment_verifiedby` FOREIGN KEY (`VerifiedBy`) REFERENCES `user` (`UserId`)
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

LOCK TABLES `payment` WRITE;
/*!40000 ALTER TABLE `payment` DISABLE KEYS */;
INSERT INTO `payment` VALUES (10,'BK-2026-001',76000.00,'qr','2026-11-18 00:27:02','Approved','/uploads/slips/BK-2026-001-20261118002701.jpg','2026-11-25 00:25:15',6,'2026-11-18 00:28:16');
/*!40000 ALTER TABLE `payment` ENABLE KEYS */;
UNLOCK TABLES;
DROP TABLE IF EXISTS `promocode`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `promocode` (
  `Code` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `Description` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `DiscountType` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `DiscountValue` decimal(10,2) NOT NULL,
  `MinMonths` int NOT NULL DEFAULT '1',
  `StartDate` date DEFAULT NULL,
  `EndDate` date DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

LOCK TABLES `promocode` WRITE;
/*!40000 ALTER TABLE `promocode` DISABLE KEYS */;
INSERT INTO `promocode` VALUES ('MEGA35START','ลดค่าเช่าเดือนแรก 35% เมื่อทำสัญญา 6 เดือนขึ้นไป','Percent',35.00,6,NULL,NULL,1,'2026-11-18 00:02:49');
/*!40000 ALTER TABLE `promocode` ENABLE KEYS */;
UNLOCK TABLES;
DROP TABLE IF EXISTS `receipt`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `receipt` (
  `ReceiptId` varchar(30) COLLATE utf8mb4_unicode_ci NOT NULL,
  `PaymentId` int NOT NULL,
  `IssuedDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `IssuedBy` int NOT NULL,
  PRIMARY KEY (`ReceiptId`),
  KEY `fk_receipt_payment` (`PaymentId`),
  KEY `fk_receipt_issuedby` (`IssuedBy`),
  CONSTRAINT `fk_receipt_issuedby` FOREIGN KEY (`IssuedBy`) REFERENCES `user` (`UserId`),
  CONSTRAINT `fk_receipt_payment` FOREIGN KEY (`PaymentId`) REFERENCES `payment` (`PaymentId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

LOCK TABLES `receipt` WRITE;
/*!40000 ALTER TABLE `receipt` DISABLE KEYS */;
INSERT INTO `receipt` VALUES ('RCP-2026-0001',10,'2026-11-18 00:28:16',6);
/*!40000 ALTER TABLE `receipt` ENABLE KEYS */;
UNLOCK TABLES;
DROP TABLE IF EXISTS `refund`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `refund` (
  `RefundId` varchar(30) COLLATE utf8mb4_unicode_ci NOT NULL,
  `PaymentId` int NOT NULL,
  `RefundAmount` decimal(12,2) NOT NULL,
  `RefundReason` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `RefundMethod` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `RefundStatus` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `RefundDate` datetime DEFAULT NULL,
  `ProcessedBy` int NOT NULL,
  PRIMARY KEY (`RefundId`),
  KEY `fk_refund_payment` (`PaymentId`),
  KEY `fk_refund_processedby` (`ProcessedBy`),
  CONSTRAINT `fk_refund_payment` FOREIGN KEY (`PaymentId`) REFERENCES `payment` (`PaymentId`),
  CONSTRAINT `fk_refund_processedby` FOREIGN KEY (`ProcessedBy`) REFERENCES `user` (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

LOCK TABLES `refund` WRITE;
/*!40000 ALTER TABLE `refund` DISABLE KEYS */;
/*!40000 ALTER TABLE `refund` ENABLE KEYS */;
UNLOCK TABLES;
DROP TABLE IF EXISTS `review`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `review` (
  `ReviewId` int NOT NULL AUTO_INCREMENT,
  `BookingId` varchar(30) COLLATE utf8mb4_unicode_ci NOT NULL,
  `OverallRating` int NOT NULL,
  `Comment` text COLLATE utf8mb4_unicode_ci,
  `ReviewDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ReviewId`),
  KEY `fk_review_booking` (`BookingId`),
  CONSTRAINT `fk_review_booking` FOREIGN KEY (`BookingId`) REFERENCES `booking` (`BookingId`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

LOCK TABLES `review` WRITE;
/*!40000 ALTER TABLE `review` DISABLE KEYS */;
/*!40000 ALTER TABLE `review` ENABLE KEYS */;
UNLOCK TABLES;
DROP TABLE IF EXISTS `room`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `room` (
  `RoomId` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `SizeCat` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Description` text COLLATE utf8mb4_unicode_ci,
  `Size` int DEFAULT NULL,
  `Height` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Zone` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `MonthlyRate` decimal(12,2) NOT NULL,
  `Status` varchar(30) COLLATE utf8mb4_unicode_ci NOT NULL,
  `ImageUrl` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`RoomId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

LOCK TABLES `room` WRITE;
/*!40000 ALTER TABLE `room` DISABLE KEYS */;
INSERT INTO `room` VALUES ('CR-05','cold','ห้องเย็นฉนวน PIR หนา 100 มม. ไฟสำรองคู่ แจ้งเตือนเมื่ออุณหภูมิผิดปกติทาง SMS',200,'7 ม.','โซนห้องเย็น',40000.00,'Available','https://images.unsplash.com/photo-1741739813128-cb658a9a0f9a'),('L-01','l','รถพ่วง 18 ล้อเข้าได้ มี Dock Leveler ปรับระดับอัตโนมัติ 2 ช่องสำหรับเทียบตู้คอนเทนเนอร์',500,'10 ม.','โซน C1 (มี Dock)',75000.00,'Available','https://images.unsplash.com/photo-1780367261654-45395777b560'),('L-02','l','สำหรับสินค้ามูลค่าสูง ประตูนิรภัย 2 ชั้น กล้องบันทึกภาพย้อนหลัง 90 วัน',350,'10 ม.','โซน C2 (สินค้าปลอดภัย)',55000.00,'Available','https://images.unsplash.com/photo-1787363813793-5f7a380dce78'),('M-01','m','พื้นอีพ็อกซี่รับน้ำหนัก 3 ตัน/ตร.ม. โฟล์คลิฟต์เข้าได้ วางพาเลทได้ 60–120 ตัว',150,'8 ม.','โซน B1 (มาตรฐาน)',25000.00,'Available','https://images.unsplash.com/photo-1644079446600-219068676743'),('M-02','m','ติดตั้ง Racking ได้ 3 ชั้น วางพาเลทได้ 120 ตัว มีระบบระบายอากาศ 24 ชม.',250,'8 ม.','โซน B2 (พาเลท)',38000.00,'Occupied','https://images.unsplash.com/photo-1714650601435-67a4d51a0798'),('S-01','s','ห้องไซส์เล็กชั้น 1 ประตูม้วนกว้าง 3.5 ม. เหมาะกับเอกสารและสต็อกสินค้าพร้อมส่ง',60,'6 ม.','โซน A1 (ชั้น 1)',12000.00,'Available','https://images.unsplash.com/photo-1694601618351-dbbbb2b8934f'),('S-02','s','ห้องกะทัดรัด ประตูม้วนไฟฟ้า เหมาะกับธุรกิจ SME และร้านค้าออนไลน์',80,'6 ม.','โซน A2 (SME)',15000.00,'Available','https://images.unsplash.com/photo-1618438502398-195e47778d6c'),('XL-01','xl','คลังโล่งขนาดใหญ่พิเศษ เพดานสูง 12 ม. หลังคา Solar Roof',1200,'12 ม.','โซน D1 (เมกะ)',180000.00,'Available','https://images.unsplash.com/photo-1694885169342-909981fb408a');
/*!40000 ALTER TABLE `room` ENABLE KEYS */;
UNLOCK TABLES;
DROP TABLE IF EXISTS `stockitem`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `stockitem` (
  `StockId` int NOT NULL AUTO_INCREMENT,
  `BookingId` varchar(30) COLLATE utf8mb4_unicode_ci NOT NULL,
  `ItemName` varchar(200) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Category` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Quantity` int NOT NULL DEFAULT '0',
  `MinQuantity` int NOT NULL DEFAULT '0',
  `Unit` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `LastUpdated` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedBy` int NOT NULL,
  PRIMARY KEY (`StockId`),
  KEY `fk_stock_booking` (`BookingId`),
  KEY `fk_stock_updatedby` (`UpdatedBy`),
  CONSTRAINT `fk_stock_booking` FOREIGN KEY (`BookingId`) REFERENCES `booking` (`BookingId`),
  CONSTRAINT `fk_stock_updatedby` FOREIGN KEY (`UpdatedBy`) REFERENCES `user` (`UserId`)
) ENGINE=InnoDB AUTO_INCREMENT=17 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

LOCK TABLES `stockitem` WRITE;
/*!40000 ALTER TABLE `stockitem` DISABLE KEYS */;
INSERT INTO `stockitem` VALUES (14,'BK-2026-001','น้ำขวด','อาหารและเครื่องดื่ม',52,30,'pack','2026-11-18 00:31:05',7);
/*!40000 ALTER TABLE `stockitem` ENABLE KEYS */;
UNLOCK TABLES;
DROP TABLE IF EXISTS `supportticket`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `supportticket` (
  `TicketId` int NOT NULL AUTO_INCREMENT,
  `UserId` int NOT NULL,
  `BookingId` varchar(30) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Subject` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Description` text COLLATE utf8mb4_unicode_ci NOT NULL,
  `Priority` varchar(30) COLLATE utf8mb4_unicode_ci NOT NULL,
  `TicketStatus` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `AssignedTo` int DEFAULT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ResolvedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`TicketId`),
  KEY `fk_ticket_user` (`UserId`),
  KEY `fk_ticket_booking` (`BookingId`),
  KEY `fk_ticket_assignedto` (`AssignedTo`),
  CONSTRAINT `fk_ticket_assignedto` FOREIGN KEY (`AssignedTo`) REFERENCES `user` (`UserId`),
  CONSTRAINT `fk_ticket_booking` FOREIGN KEY (`BookingId`) REFERENCES `booking` (`BookingId`),
  CONSTRAINT `fk_ticket_user` FOREIGN KEY (`UserId`) REFERENCES `user` (`UserId`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

LOCK TABLES `supportticket` WRITE;
/*!40000 ALTER TABLE `supportticket` DISABLE KEYS */;
/*!40000 ALTER TABLE `supportticket` ENABLE KEYS */;
UNLOCK TABLES;
DROP TABLE IF EXISTS `user`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user` (
  `UserId` int NOT NULL AUTO_INCREMENT,
  `Username` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `PasswordHash` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Fullname` varchar(150) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Phone` varchar(20) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Email` varchar(150) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Role` varchar(30) COLLATE utf8mb4_unicode_ci NOT NULL,
  `CompanyName` varchar(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `TaxId` varchar(13) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Address` text COLLATE utf8mb4_unicode_ci,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`UserId`),
  UNIQUE KEY `Username` (`Username`)
) ENGINE=InnoDB AUTO_INCREMENT=18 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

LOCK TABLES `user` WRITE;
/*!40000 ALTER TABLE `user` DISABLE KEYS */;
INSERT INTO `user` VALUES (3,'123@gmail.com','123','ณภัทร นวหัสดินกุล34','0969382439',NULL,'Customer',NULL,NULL,NULL,1,'2026-11-10 14:58:31'),(5,'naphat.nav@spulive.net','123','NAPHAT NAVAHASDINKULL','0969382439','naphat.nav@spulive.net','Customer','67จำกัด','1111111111111','adad',1,'2026-11-10 19:51:41'),(6,'acc01','1234','พนักงานบัญชี',NULL,NULL,'Accounting',NULL,NULL,NULL,1,'2026-11-10 20:04:53'),(7,'inv01','1234','พนักงานคลังสินค้า',NULL,NULL,'Inventory',NULL,NULL,NULL,1,'2026-11-12 21:05:40'),(10,'cs01','1234','พนักงานบริการลูกค้า',NULL,NULL,'CustomerService',NULL,NULL,NULL,1,'2026-11-15 19:26:57'),(13,'manager01','1234','ผู้จัดการ',NULL,NULL,'Manager',NULL,NULL,NULL,1,'2026-11-17 23:59:43'),(14,'admin01','1234','ผู้ดูแลระบบ',NULL,NULL,'Admin',NULL,NULL,NULL,1,'2026-11-17 23:59:43'),(16,'kuy@gmail.com','123','kuy gg','022222222222','kuy@gmail.com','Customer',NULL,'1111111111111','aaaaaaa',1,'2026-11-18 00:14:56');
/*!40000 ALTER TABLE `user` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

