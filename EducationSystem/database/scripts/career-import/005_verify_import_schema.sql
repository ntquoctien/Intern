/*
    Verifies metadata and rejection rules. Test data is always rolled back.
*/
SET NOCOUNT ON;
SET XACT_ABORT OFF;
SET QUOTED_IDENTIFIER ON;

IF DB_NAME() <> N'TayDoV2'
    THROW 51000, N'Refusing to verify career import schema outside TayDoV2.', 1;

IF OBJECT_ID(N'career.OutcomeImportBatch', N'U') IS NULL
   OR OBJECT_ID(N'career.OutcomeDocumentBlock', N'U') IS NULL
   OR OBJECT_ID(N'career.OutcomeReviewLog', N'U') IS NULL
   OR OBJECT_ID(N'career.ApprovedOutcomeJsonDocument', N'U') IS NULL
    THROW 51000, N'Verification failed: import tables are missing.', 1;

IF COL_LENGTH(N'career.OutcomeImportBatch', N'SelectedSubjectExternalId') IS NULL
   OR COL_LENGTH(N'career.OutcomeImportBatch', N'SelectedSubjectCode') IS NULL
   OR COL_LENGTH(N'career.OutcomeImportBatch', N'SelectedSubjectName') IS NULL
   OR COL_LENGTH(N'career.OutcomeImportBatch', N'QualityReportJson') IS NULL
    THROW 51000, N'Verification failed: selected-subject snapshot columns are missing.', 1;

IF
(
    SELECT COUNT(*)
    FROM sys.key_constraints
    WHERE type = 'PK'
      AND OBJECT_SCHEMA_NAME(parent_object_id) = N'career'
      AND OBJECT_NAME(parent_object_id) IN
          (N'OutcomeImportBatch', N'OutcomeDocumentBlock', N'OutcomeReviewLog',
           N'ApprovedOutcomeJsonDocument')
) <> 4
    THROW 51000, N'Verification failed: import primary keys are missing.', 1;

IF
(
    SELECT COUNT(*)
    FROM sys.foreign_keys
    WHERE OBJECT_SCHEMA_NAME(parent_object_id) = N'career'
      AND OBJECT_NAME(parent_object_id) IN
          (N'OutcomeImportBatch', N'OutcomeDocumentBlock', N'OutcomeReviewLog',
           N'ApprovedOutcomeJsonDocument')
) <> 5
    THROW 51000, N'Verification failed: import foreign keys are missing.', 1;

IF EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE OBJECT_SCHEMA_NAME(parent_object_id) = N'career'
      AND OBJECT_NAME(parent_object_id) IN
          (N'OutcomeImportBatch', N'OutcomeDocumentBlock', N'OutcomeReviewLog',
           N'ApprovedOutcomeJsonDocument')
      AND
      (
          OBJECT_SCHEMA_NAME(referenced_object_id) <> N'career'
          OR delete_referential_action <> 0
      )
)
    THROW 51000, N'Verification failed: cross-schema or cascading FK found.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Token NVARCHAR(32) = REPLACE(CONVERT(NVARCHAR(36), NEWID()), N'-', N'');
    DECLARE @CurriculumId BIGINT;
    DECLARE @BatchId BIGINT;
    DECLARE @Rejected BIT;

    INSERT career.CurriculumVersion
        (MajorCode, MajorName, CurriculumCode, Version, Status)
    VALUES
        (N'VERIFY-' + @Token, N'Ngành kiểm thử',
         N'IMPORT-' + @Token, N'v1', N'Draft');
    SET @CurriculumId = SCOPE_IDENTITY();

    INSERT career.OutcomeImportBatch
        (CurriculumVersionId, SelectedSubjectExternalId, SelectedSubjectCode,
         SelectedSubjectName, OriginalFileName, StorageKey, ContentType,
         FileSize, FileHash, Status, ProcessingStage,
         UploadedByExternalId, UploadedByName)
    VALUES
        (@CurriculumId, NEWID(), N'VERIFY101', N'Học phần kiểm thử',
         N'verify.docx', N'outcome-imports/verify/source.docx',
         N'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
         100, REPLICATE('a', 64), N'Uploaded', N'Waiting',
         N'verifier', N'Verifier');
    SET @BatchId = SCOPE_IDENTITY();

    INSERT career.OutcomeDocumentBlock
        (ImportBatchId, BlockId, BlockType, Sequence, ContentJson)
    VALUES
        (@BatchId, N'paragraph-1', N'Paragraph', 0, N'{"text":"valid"}');

    INSERT career.OutcomeReviewLog
        (ImportBatchId, EntityType, EntityDraftId, Action,
         ActorExternalId, ActorName)
    VALUES
        (@BatchId, N'Batch', N'batch', N'Validate', N'verifier', N'Verifier');

    INSERT career.ApprovedOutcomeJsonDocument
        (CurriculumVersionId, ImportBatchId, SchemaVersion, DocumentVersion,
         ContentJson, ContentHash, Status, GeneratedByExternalId, GeneratedByName)
    VALUES
        (@CurriculumId, @BatchId, N'1.0', 1, N'{"schemaVersion":"1.0"}',
         REPLICATE('a', 64), N'Active', N'verifier', N'Verifier');

    SET @Rejected = 0;
    BEGIN TRY
        INSERT career.OutcomeDocumentBlock
            (ImportBatchId, BlockId, BlockType, Sequence, ContentJson)
        VALUES
            (@BatchId, N'paragraph-2', N'Paragraph', 0, N'{"text":"duplicate"}');
    END TRY
    BEGIN CATCH
        SET @Rejected = 1;
    END CATCH;
    IF @Rejected = 0
        THROW 51000, N'Verification failed: duplicate sequence accepted.', 1;

    SET @Rejected = 0;
    BEGIN TRY
        INSERT career.OutcomeReviewLog
            (ImportBatchId, EntityType, EntityDraftId, Action,
             ActorExternalId, ActorName)
        VALUES
            (-1, N'Batch', N'batch', N'Validate', N'verifier', N'Verifier');
    END TRY
    BEGIN CATCH
        SET @Rejected = 1;
    END CATCH;
    IF @Rejected = 0
        THROW 51000, N'Verification failed: orphan review log accepted.', 1;

    SET @Rejected = 0;
    BEGIN TRY
        UPDATE career.OutcomeImportBatch
        SET ReviewedJson = N'not-json'
        WHERE Id = @BatchId;
    END TRY
    BEGIN CATCH
        SET @Rejected = 1;
    END CATCH;
    IF @Rejected = 0
        THROW 51000, N'Verification failed: invalid JSON accepted.', 1;

    SET @Rejected = 0;
    BEGIN TRY
        UPDATE career.OutcomeImportBatch
        SET QualityReportJson = N'not-json'
        WHERE Id = @BatchId;
    END TRY
    BEGIN CATCH
        SET @Rejected = 1;
    END CATCH;
    IF @Rejected = 0
        THROW 51000, N'Verification failed: invalid quality JSON accepted.', 1;

    SET @Rejected = 0;
    BEGIN TRY
        INSERT career.ApprovedOutcomeJsonDocument
            (CurriculumVersionId, ImportBatchId, SchemaVersion, DocumentVersion,
             ContentJson, ContentHash, Status, GeneratedByExternalId, GeneratedByName)
        VALUES
            (@CurriculumId, @BatchId, N'1.0', 2, N'{}',
             REPLICATE('b', 64), N'Active', N'verifier', N'Verifier');
    END TRY
    BEGIN CATCH
        SET @Rejected = 1;
    END CATCH;
    IF @Rejected = 0
        THROW 51000, N'Verification failed: duplicate import document accepted.', 1;

    DECLARE @AtomicCurriculumId BIGINT;
    DECLARE @AtomicBatchId BIGINT;
    INSERT career.CurriculumVersion
        (MajorCode, MajorName, CurriculumCode, Version, Status)
    VALUES
        (N'ATOMIC-' + @Token, N'Ngành kiểm thử transaction',
         N'ATOMIC-' + @Token, N'v1', N'Draft');
    SET @AtomicCurriculumId = SCOPE_IDENTITY();

    INSERT career.OutcomeImportBatch
        (CurriculumVersionId, OriginalFileName, StorageKey, ContentType,
         FileSize, FileHash, Status, UploadedByExternalId, UploadedByName)
    VALUES
        (@AtomicCurriculumId, N'atomic.docx', N'outcome-imports/verify/atomic.docx',
         N'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
         100, REPLICATE('c', 64), N'PendingReview', N'verifier', N'Verifier');
    SET @AtomicBatchId = SCOPE_IDENTITY();

    SAVE TRANSACTION VerifyAtomicApproval;
    BEGIN TRY
        INSERT career.ProgramLearningOutcome
            (CurriculumVersionId, PloCode, Description, SortOrder, Status)
        VALUES
            (@AtomicCurriculumId, N'PLO1', N'Atomic outcome', 1, N'Approved');

        INSERT career.ApprovedOutcomeJsonDocument
            (CurriculumVersionId, ImportBatchId, SchemaVersion, DocumentVersion,
             ContentJson, ContentHash, Status, GeneratedByExternalId, GeneratedByName)
        VALUES
            (@AtomicCurriculumId, @AtomicBatchId, N'1.0', 1, N'{}',
             REPLICATE('x', 64), N'Active', N'verifier', N'Verifier');

        THROW 51000, N'Verification failed: invalid content hash accepted.', 1;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION VerifyAtomicApproval;
    END CATCH;

    IF EXISTS
    (
        SELECT 1 FROM career.ProgramLearningOutcome
        WHERE CurriculumVersionId = @AtomicCurriculumId
    )
       OR EXISTS
    (
        SELECT 1 FROM career.ApprovedOutcomeJsonDocument
        WHERE ImportBatchId = @AtomicBatchId
    )
        THROW 51000, N'Verification failed: failed document materialization did not roll back canonical data.', 1;

    SET @Rejected = 0;
    BEGIN TRY
        UPDATE career.OutcomeImportBatch
        SET Status = N'InvalidStatus'
        WHERE Id = @BatchId;
    END TRY
    BEGIN CATCH
        SET @Rejected = 1;
    END CATCH;
    IF @Rejected = 0
        THROW 51000, N'Verification failed: invalid status accepted.', 1;

    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT N'PASS' AS VerificationStatus,
       N'Career import schema and transactional rejection tests passed.' AS Message;
