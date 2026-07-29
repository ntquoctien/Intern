/*
    Creates the career outcome-import persistence tables.
    Run with sqlcmd -f 65001 against TayDoV2.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'TayDoV2'
    THROW 51000, N'Refusing to create career import tables outside TayDoV2.', 1;

IF SCHEMA_ID(N'career') IS NULL
    THROW 51000, N'Schema career does not exist.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'career.OutcomeImportBatch', N'U') IS NULL
    BEGIN
        CREATE TABLE career.OutcomeImportBatch
        (
            Id BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_OutcomeImportBatch PRIMARY KEY,
            CurriculumVersionId BIGINT NOT NULL,
            SelectedSubjectExternalId UNIQUEIDENTIFIER NULL,
            SelectedSubjectCode NVARCHAR(100) NULL,
            SelectedSubjectName NVARCHAR(255) NULL,
            OriginalFileName NVARCHAR(255) NOT NULL,
            StorageKey NVARCHAR(500) NOT NULL,
            ContentType NVARCHAR(100) NOT NULL,
            FileSize BIGINT NOT NULL,
            FileHash CHAR(64) NOT NULL,
            Provider NVARCHAR(50) NULL,
            ModelName NVARCHAR(100) NULL,
            PromptVersion NVARCHAR(50) NULL,
            RawExtractionJson NVARCHAR(MAX) NULL,
            ReviewedJson NVARCHAR(MAX) NULL,
            ValidationJson NVARCHAR(MAX) NULL,
            Status NVARCHAR(30) NOT NULL,
            ProcessingStage NVARCHAR(40) NULL,
            ProgressPercent TINYINT NOT NULL
                CONSTRAINT DF_OutcomeImportBatch_ProgressPercent DEFAULT ((0)),
            UploadedByExternalId NVARCHAR(100) NOT NULL,
            UploadedByName NVARCHAR(200) NOT NULL,
            ReviewedByExternalId NVARCHAR(100) NULL,
            ReviewedByName NVARCHAR(200) NULL,
            ApprovedByExternalId NVARCHAR(100) NULL,
            ApprovedByName NVARCHAR(200) NULL,
            ProcessingStartedAt DATETIME2(7) NULL,
            ProcessingCompletedAt DATETIME2(7) NULL,
            ReviewedAt DATETIME2(7) NULL,
            ApprovedAt DATETIME2(7) NULL,
            ErrorCode NVARCHAR(100) NULL,
            ErrorMessage NVARCHAR(2000) NULL,
            CreatedAt DATETIME2(7) NOT NULL
                CONSTRAINT DF_OutcomeImportBatch_CreatedAt DEFAULT (SYSUTCDATETIME()),
            UpdatedAt DATETIME2(7) NOT NULL
                CONSTRAINT DF_OutcomeImportBatch_UpdatedAt DEFAULT (SYSUTCDATETIME()),
            RowVersion ROWVERSION NOT NULL
        );
    END;

    IF OBJECT_ID(N'career.OutcomeDocumentBlock', N'U') IS NULL
    BEGIN
        CREATE TABLE career.OutcomeDocumentBlock
        (
            Id BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_OutcomeDocumentBlock PRIMARY KEY,
            ImportBatchId BIGINT NOT NULL,
            BlockId NVARCHAR(100) NOT NULL,
            BlockType NVARCHAR(30) NOT NULL,
            Sequence INT NOT NULL,
            HeadingLevel TINYINT NULL,
            ContentJson NVARCHAR(MAX) NOT NULL,
            CreatedAt DATETIME2(7) NOT NULL
                CONSTRAINT DF_OutcomeDocumentBlock_CreatedAt DEFAULT (SYSUTCDATETIME())
        );
    END;

    IF OBJECT_ID(N'career.OutcomeReviewLog', N'U') IS NULL
    BEGIN
        CREATE TABLE career.OutcomeReviewLog
        (
            Id BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_OutcomeReviewLog PRIMARY KEY,
            ImportBatchId BIGINT NOT NULL,
            EntityType NVARCHAR(30) NOT NULL,
            EntityDraftId NVARCHAR(100) NOT NULL,
            FieldName NVARCHAR(100) NULL,
            OldValueJson NVARCHAR(MAX) NULL,
            NewValueJson NVARCHAR(MAX) NULL,
            Action NVARCHAR(30) NOT NULL,
            Note NVARCHAR(1000) NULL,
            ActorExternalId NVARCHAR(100) NOT NULL,
            ActorName NVARCHAR(200) NOT NULL,
            CreatedAt DATETIME2(7) NOT NULL
                CONSTRAINT DF_OutcomeReviewLog_CreatedAt DEFAULT (SYSUTCDATETIME())
        );
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
