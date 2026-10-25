-- Solo la base lógica DEV de trabajos; nunca db_object ni db_election.
-- Ejecutar una vez con el cliente MySQL y la identidad administrativa de migraciones.
CREATE DATABASE IF NOT EXISTS `db_file_jobs_dev` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `db_file_jobs_dev`;

ALTER DATABASE CHARACTER SET utf8mb4;


CREATE TABLE `file_job_audit` (
    `AuditId` varchar(36) CHARACTER SET utf8mb4 NOT NULL,
    `JobId` varchar(36) CHARACTER SET utf8mb4 NOT NULL,
    `AttemptId` varchar(36) CHARACTER SET utf8mb4 NULL,
    `Status` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `OccurredAt` datetime(6) NOT NULL,
    `Code` varchar(100) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_file_job_audit` PRIMARY KEY (`AuditId`)
) CHARACTER SET=utf8mb4;


CREATE TABLE `file_job_intents` (
    `Scope` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `RequestId` varchar(36) CHARACTER SET utf8mb4 NOT NULL,
    `Fingerprint` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `JobId` varchar(36) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_file_job_intents` PRIMARY KEY (`Scope`, `RequestId`)
) CHARACTER SET=utf8mb4;


CREATE TABLE `file_job_outbox` (
    `MessageId` varchar(36) CHARACTER SET utf8mb4 NOT NULL,
    `JobId` varchar(36) CHARACTER SET utf8mb4 NOT NULL,
    `Payload` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `AvailableAt` datetime(6) NOT NULL,
    `PublishedAt` datetime(6) NULL,
    `DispatchId` varchar(36) CHARACTER SET utf8mb4 NULL,
    `DispatchUntil` datetime(6) NULL,
    `Attempts` int NOT NULL,
    `Version` bigint NOT NULL,
    `DiscardedAt` datetime(6) NULL,
    CONSTRAINT `PK_file_job_outbox` PRIMARY KEY (`MessageId`)
) CHARACTER SET=utf8mb4;


CREATE TABLE `file_job_slots` (
    `Fingerprint` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `JobId` varchar(36) CHARACTER SET utf8mb4 NOT NULL,
    `Version` bigint NOT NULL,
    CONSTRAINT `PK_file_job_slots` PRIMARY KEY (`Fingerprint`)
) CHARACTER SET=utf8mb4;


CREATE TABLE `file_jobs` (
    `Sequence` bigint NOT NULL AUTO_INCREMENT,
    `JobId` varchar(36) CHARACTER SET utf8mb4 NOT NULL,
    `Environment` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `ChurchCode` varchar(36) CHARACTER SET utf8mb4 NOT NULL,
    `OwnerHash` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `ReportType` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `ContractVersion` int NOT NULL,
    `Producer` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `Fingerprint` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `ParametersJson` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Status` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    `DeadlineAt` datetime(6) NOT NULL,
    `GeneratedAt` datetime(6) NULL,
    `DataAsOf` datetime(6) NULL,
    `ExpiresAt` datetime(6) NULL,
    `AttemptCount` int NOT NULL,
    `AttemptId` varchar(36) CHARACTER SET utf8mb4 NULL,
    `LeaseHash` varchar(64) CHARACTER SET utf8mb4 NULL,
    `LeaseUntil` datetime(6) NULL,
    `AttemptDeadlineAt` datetime(6) NULL,
    `NextAttemptAt` datetime(6) NULL,
    `FailureCode` varchar(100) CHARACTER SET utf8mb4 NULL,
    `RetentionHours` int NULL,
    `ReuseSeconds` int NOT NULL,
    `Version` bigint NOT NULL,
    CONSTRAINT `PK_file_jobs` PRIMARY KEY (`Sequence`),
    CONSTRAINT `AK_file_jobs_JobId` UNIQUE (`JobId`)
) CHARACTER SET=utf8mb4;


CREATE TABLE `file_job_artifacts` (
    `ArtifactId` varchar(36) CHARACTER SET utf8mb4 NOT NULL,
    `JobId` varchar(36) CHARACTER SET utf8mb4 NOT NULL,
    `AttemptId` varchar(36) CHARACTER SET utf8mb4 NOT NULL,
    `Name` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `FileName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `StorageKey` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    `ContentType` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `SizeBytes` bigint NOT NULL,
    `Sha256` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `Published` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `DeletedAt` datetime(6) NULL,
    CONSTRAINT `PK_file_job_artifacts` PRIMARY KEY (`ArtifactId`),
    CONSTRAINT `FK_file_job_artifacts_file_jobs_JobId` FOREIGN KEY (`JobId`) REFERENCES `file_jobs` (`JobId`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE UNIQUE INDEX `IX_file_job_artifacts_JobId_AttemptId_Name` ON `file_job_artifacts` (`JobId`, `AttemptId`, `Name`);


CREATE INDEX `IX_file_job_audit_JobId_OccurredAt` ON `file_job_audit` (`JobId`, `OccurredAt`);


CREATE INDEX `IX_file_job_outbox_PublishedAt_AvailableAt_DispatchUntil` ON `file_job_outbox` (`PublishedAt`, `AvailableAt`, `DispatchUntil`);


CREATE INDEX `IX_file_jobs_Environment_ChurchCode_OwnerHash_Sequence` ON `file_jobs` (`Environment`, `ChurchCode`, `OwnerHash`, `Sequence`);


CREATE INDEX `IX_file_jobs_Environment_Status_LeaseUntil` ON `file_jobs` (`Environment`, `Status`, `LeaseUntil`);
