/*
 Navicat Premium Data Transfer

 Source Server         : Localhost
 Source Server Type    : MySQL
 Source Server Version : 100131
 Source Host           : localhost:3306
 Source Schema         : converse

 Target Server Type    : MySQL
 Target Server Version : 100131
 File Encoding         : 65001

 Date: 08/12/2018 21:53:00
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for __efmigrationshistory
-- ----------------------------
DROP TABLE IF EXISTS `__efmigrationshistory`;
CREATE TABLE `__efmigrationshistory`  (
  `MigrationId` varchar(95) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  PRIMARY KEY (`MigrationId`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Compact;

-- ----------------------------
-- Table structure for blockedusers
-- ----------------------------
DROP TABLE IF EXISTS `blockedusers`;
CREATE TABLE `blockedusers`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `UserId` int(11) NOT NULL,
  `Address` longtext CHARACTER SET latin1 COLLATE latin1_swedish_ci,
  `BlockedAddress` longtext CHARACTER SET latin1 COLLATE latin1_swedish_ci,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `IX_BlockedUsers_UserId`(`UserId`) USING BTREE,
  CONSTRAINT `FK_BlockedUsers_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Compact;

-- ----------------------------
-- Table structure for chatmessages
-- ----------------------------
DROP TABLE IF EXISTS `chatmessages`;
CREATE TABLE `chatmessages`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `InternalId` int(11) NOT NULL,
  `ChatId` int(11) NOT NULL,
  `UserId` int(11) NOT NULL,
  `Address` longtext CHARACTER SET latin1 COLLATE latin1_swedish_ci,
  `Message` longtext CHARACTER SET latin1 COLLATE latin1_swedish_ci,
  `BlockId` bigint(20) NOT NULL,
  `TransactionHash` longtext CHARACTER SET latin1 COLLATE latin1_swedish_ci,
  `BlockCreatedAt` datetime(6) NOT NULL,
  `TransactionCreatedAt` datetime(6) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `IX_ChatMessages_ChatId`(`ChatId`) USING BTREE,
  INDEX `IX_ChatMessages_UserId`(`UserId`) USING BTREE,
  CONSTRAINT `FK_ChatMessages_Chats_ChatId` FOREIGN KEY (`ChatId`) REFERENCES `chats` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT `FK_ChatMessages_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Compact;

-- ----------------------------
-- Table structure for chats
-- ----------------------------
DROP TABLE IF EXISTS `chats`;
CREATE TABLE `chats`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `IsGroup` bit(1) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Compact;

-- ----------------------------
-- Table structure for chatsettings
-- ----------------------------
DROP TABLE IF EXISTS `chatsettings`;
CREATE TABLE `chatsettings`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ChatId` int(11) NOT NULL,
  `Address` longtext CHARACTER SET latin1 COLLATE latin1_swedish_ci,
  `Name` longtext CHARACTER SET latin1 COLLATE latin1_swedish_ci,
  `Description` longtext CHARACTER SET latin1 COLLATE latin1_swedish_ci,
  `PictureUrl` longtext CHARACTER SET latin1 COLLATE latin1_swedish_ci,
  `IsPublic` bit(1) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  UNIQUE INDEX `IX_ChatSettings_ChatId`(`ChatId`) USING BTREE,
  CONSTRAINT `FK_ChatSettings_Chats_ChatId` FOREIGN KEY (`ChatId`) REFERENCES `chats` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Compact;

-- ----------------------------
-- Table structure for chatusers
-- ----------------------------
DROP TABLE IF EXISTS `chatusers`;
CREATE TABLE `chatusers`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ChatId` int(11) NOT NULL,
  `UserId` int(11) NOT NULL,
  `Address` longtext CHARACTER SET latin1 COLLATE latin1_swedish_ci,
  `IsAdmin` bit(1) NOT NULL,
  `JoinedAt` datetime(6) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `IX_ChatUsers_ChatId`(`ChatId`) USING BTREE,
  INDEX `IX_ChatUsers_UserId`(`UserId`) USING BTREE,
  CONSTRAINT `FK_ChatUsers_Chats_ChatId` FOREIGN KEY (`ChatId`) REFERENCES `chats` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT `FK_ChatUsers_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Compact;

-- ----------------------------
-- Table structure for settings
-- ----------------------------
DROP TABLE IF EXISTS `settings`;
CREATE TABLE `settings`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Key` longtext CHARACTER SET latin1 COLLATE latin1_swedish_ci,
  `Value` longtext CHARACTER SET latin1 COLLATE latin1_swedish_ci,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Compact;

-- ----------------------------
-- Table structure for userreceivedtokens
-- ----------------------------
DROP TABLE IF EXISTS `userreceivedtokens`;
CREATE TABLE `userreceivedtokens`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `UserId` int(11) NOT NULL,
  `Ip` longtext CHARACTER SET latin1 COLLATE latin1_swedish_ci,
  `ReceivedTokens` int(11) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `IX_UserReceivedTokens_UserId`(`UserId`) USING BTREE,
  CONSTRAINT `FK_UserReceivedTokens_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Compact;

-- ----------------------------
-- Table structure for users
-- ----------------------------
DROP TABLE IF EXISTS `users`;
CREATE TABLE `users`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Address` longtext CHARACTER SET latin1 COLLATE latin1_swedish_ci,
  `Nickname` longtext CHARACTER SET latin1 COLLATE latin1_swedish_ci,
  `ProfilePictureUrl` longtext CHARACTER SET latin1 COLLATE latin1_swedish_ci,
  `Status` longtext CHARACTER SET latin1 COLLATE latin1_swedish_ci,
  `StatusUpdatedAt` datetime(6) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Compact;

SET FOREIGN_KEY_CHECKS = 1;