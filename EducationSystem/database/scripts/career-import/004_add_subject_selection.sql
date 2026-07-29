/*
    Adds the academic subject snapshot selected for an outcome import.
    Columns remain nullable so batches created before this change stay valid.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;

IF DB_NAME() <> N'TayDoV2'
    THROW 51000, N'Refusing to alter career import tables outside TayDoV2.', 1;

IF OBJECT_ID(N'career.OutcomeImportBatch', N'U') IS NULL
    THROW 51000, N'career.OutcomeImportBatch does not exist.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    IF COL_LENGTH(N'career.OutcomeImportBatch', N'SelectedSubjectExternalId') IS NULL
        ALTER TABLE career.OutcomeImportBatch
            ADD SelectedSubjectExternalId UNIQUEIDENTIFIER NULL;

    IF COL_LENGTH(N'career.OutcomeImportBatch', N'SelectedSubjectCode') IS NULL
        ALTER TABLE career.OutcomeImportBatch
            ADD SelectedSubjectCode NVARCHAR(100) NULL;

    IF COL_LENGTH(N'career.OutcomeImportBatch', N'SelectedSubjectName') IS NULL
        ALTER TABLE career.OutcomeImportBatch
            ADD SelectedSubjectName NVARCHAR(255) NULL;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'career.OutcomeImportBatch')
          AND name = N'IX_OutcomeImportBatch_CurriculumVersionId_SelectedSubjectCode'
    )
        EXEC
        (
            N'CREATE INDEX IX_OutcomeImportBatch_CurriculumVersionId_SelectedSubjectCode
              ON career.OutcomeImportBatch (CurriculumVersionId, SelectedSubjectCode)
              WHERE SelectedSubjectCode IS NOT NULL;'
        );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
