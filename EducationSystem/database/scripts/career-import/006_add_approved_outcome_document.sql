/*
    Adds deterministic extraction-quality storage and immutable approved outcome JSON.
    Run with sqlcmd -f 65001 against TayDoV2.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;

IF DB_NAME() <> N'TayDoV2'
    THROW 51000, N'Refusing to alter career import tables outside TayDoV2.', 1;

IF SCHEMA_ID(N'career') IS NULL
    THROW 51000, N'Schema career does not exist.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    IF COL_LENGTH(N'career.OutcomeImportBatch', N'QualityReportJson') IS NULL
        EXEC(N'ALTER TABLE career.OutcomeImportBatch
            ADD QualityReportJson NVARCHAR(MAX) NULL;');

    IF OBJECT_ID(N'career.CK_OutcomeImportBatch_QualityReportJson', N'C') IS NULL
        EXEC(N'ALTER TABLE career.OutcomeImportBatch
            ADD CONSTRAINT CK_OutcomeImportBatch_QualityReportJson
                CHECK (QualityReportJson IS NULL OR ISJSON(QualityReportJson) = 1);');

    IF OBJECT_ID(N'career.ApprovedOutcomeJsonDocument', N'U') IS NULL
    BEGIN
        CREATE TABLE career.ApprovedOutcomeJsonDocument
        (
            Id BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_ApprovedOutcomeJsonDocument PRIMARY KEY,
            CurriculumVersionId BIGINT NOT NULL,
            ImportBatchId BIGINT NOT NULL,
            SchemaVersion NVARCHAR(20) NOT NULL,
            DocumentVersion INT NOT NULL,
            ContentJson NVARCHAR(MAX) NOT NULL,
            ContentHash CHAR(64) NOT NULL,
            Status NVARCHAR(20) NOT NULL,
            GeneratedAt DATETIME2(7) NOT NULL
                CONSTRAINT DF_ApprovedOutcomeJsonDocument_GeneratedAt DEFAULT (SYSUTCDATETIME()),
            GeneratedByExternalId NVARCHAR(100) NOT NULL,
            GeneratedByName NVARCHAR(200) NOT NULL,
            SupersededAt DATETIME2(7) NULL,
            RowVersion ROWVERSION NOT NULL
        );
    END;

    IF OBJECT_ID(N'career.CK_ApprovedOutcomeJsonDocument_ContentJson', N'C') IS NULL
        ALTER TABLE career.ApprovedOutcomeJsonDocument
            ADD CONSTRAINT CK_ApprovedOutcomeJsonDocument_ContentJson
                CHECK (ISJSON(ContentJson) = 1);

    IF OBJECT_ID(N'career.CK_ApprovedOutcomeJsonDocument_ContentHash', N'C') IS NULL
        ALTER TABLE career.ApprovedOutcomeJsonDocument
            ADD CONSTRAINT CK_ApprovedOutcomeJsonDocument_ContentHash
                CHECK
                (
                    LEN(ContentHash) = 64
                    AND ContentHash COLLATE Latin1_General_100_BIN2
                        NOT LIKE '%[^0-9a-f]%' COLLATE Latin1_General_100_BIN2
                );

    IF OBJECT_ID(N'career.CK_ApprovedOutcomeJsonDocument_DocumentVersion', N'C') IS NULL
        ALTER TABLE career.ApprovedOutcomeJsonDocument
            ADD CONSTRAINT CK_ApprovedOutcomeJsonDocument_DocumentVersion
                CHECK (DocumentVersion > 0);

    IF OBJECT_ID(N'career.CK_ApprovedOutcomeJsonDocument_Status', N'C') IS NULL
        ALTER TABLE career.ApprovedOutcomeJsonDocument
            ADD CONSTRAINT CK_ApprovedOutcomeJsonDocument_Status
                CHECK (Status IN (N'Active', N'Superseded'));

    IF OBJECT_ID(N'career.FK_ApprovedOutcomeJsonDocument_CurriculumVersion', N'F') IS NULL
        ALTER TABLE career.ApprovedOutcomeJsonDocument
            ADD CONSTRAINT FK_ApprovedOutcomeJsonDocument_CurriculumVersion
                FOREIGN KEY (CurriculumVersionId)
                REFERENCES career.CurriculumVersion(Id);

    IF OBJECT_ID(N'career.FK_ApprovedOutcomeJsonDocument_OutcomeImportBatch', N'F') IS NULL
        ALTER TABLE career.ApprovedOutcomeJsonDocument
            ADD CONSTRAINT FK_ApprovedOutcomeJsonDocument_OutcomeImportBatch
                FOREIGN KEY (ImportBatchId)
                REFERENCES career.OutcomeImportBatch(Id);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'career.ApprovedOutcomeJsonDocument')
          AND name = N'UQ_ApprovedOutcomeJsonDocument_ImportBatchId'
    )
        CREATE UNIQUE INDEX UQ_ApprovedOutcomeJsonDocument_ImportBatchId
            ON career.ApprovedOutcomeJsonDocument(ImportBatchId);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'career.ApprovedOutcomeJsonDocument')
          AND name = N'UQ_ApprovedOutcomeJsonDocument_CurriculumVersionId_DocumentVersion'
    )
        CREATE UNIQUE INDEX UQ_ApprovedOutcomeJsonDocument_CurriculumVersionId_DocumentVersion
            ON career.ApprovedOutcomeJsonDocument(CurriculumVersionId, DocumentVersion);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'career.ApprovedOutcomeJsonDocument')
          AND name = N'UQ_ApprovedOutcomeJsonDocument_Active'
    )
        CREATE UNIQUE INDEX UQ_ApprovedOutcomeJsonDocument_Active
            ON career.ApprovedOutcomeJsonDocument(CurriculumVersionId)
            WHERE Status = N'Active';

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'career.ApprovedOutcomeJsonDocument')
          AND name = N'IX_ApprovedOutcomeJsonDocument_CurriculumVersionId_ContentHash'
    )
        CREATE INDEX IX_ApprovedOutcomeJsonDocument_CurriculumVersionId_ContentHash
            ON career.ApprovedOutcomeJsonDocument(CurriculumVersionId, ContentHash);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
