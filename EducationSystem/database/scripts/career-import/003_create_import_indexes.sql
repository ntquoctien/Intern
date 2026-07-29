/*
    Creates the career outcome-import query indexes.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'TayDoV2'
    THROW 51000, N'Refusing to create career import indexes outside TayDoV2.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'career.OutcomeImportBatch') AND name = N'IX_OutcomeImportBatch_CurriculumVersionId')
        CREATE INDEX IX_OutcomeImportBatch_CurriculumVersionId
            ON career.OutcomeImportBatch (CurriculumVersionId);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'career.OutcomeImportBatch') AND name = N'IX_OutcomeImportBatch_Status')
        CREATE INDEX IX_OutcomeImportBatch_Status
            ON career.OutcomeImportBatch (Status);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'career.OutcomeImportBatch') AND name = N'IX_OutcomeImportBatch_CreatedAt')
        CREATE INDEX IX_OutcomeImportBatch_CreatedAt
            ON career.OutcomeImportBatch (CreatedAt DESC);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'career.OutcomeImportBatch') AND name = N'IX_OutcomeImportBatch_CurriculumVersionId_FileHash')
        CREATE INDEX IX_OutcomeImportBatch_CurriculumVersionId_FileHash
            ON career.OutcomeImportBatch (CurriculumVersionId, FileHash);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'career.OutcomeImportBatch') AND name = N'IX_OutcomeImportBatch_Status_CreatedAt')
        CREATE INDEX IX_OutcomeImportBatch_Status_CreatedAt
            ON career.OutcomeImportBatch (Status, CreatedAt DESC);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'career.OutcomeDocumentBlock') AND name = N'IX_OutcomeDocumentBlock_ImportBatchId_BlockType')
        CREATE INDEX IX_OutcomeDocumentBlock_ImportBatchId_BlockType
            ON career.OutcomeDocumentBlock (ImportBatchId, BlockType);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'career.OutcomeReviewLog') AND name = N'IX_OutcomeReviewLog_ImportBatchId_CreatedAt')
        CREATE INDEX IX_OutcomeReviewLog_ImportBatchId_CreatedAt
            ON career.OutcomeReviewLog (ImportBatchId, CreatedAt DESC);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'career.OutcomeReviewLog') AND name = N'IX_OutcomeReviewLog_ImportBatchId_EntityDraftId')
        CREATE INDEX IX_OutcomeReviewLog_ImportBatchId_EntityDraftId
            ON career.OutcomeReviewLog (ImportBatchId, EntityDraftId);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'career.OutcomeReviewLog') AND name = N'IX_OutcomeReviewLog_ActorExternalId_CreatedAt')
        CREATE INDEX IX_OutcomeReviewLog_ActorExternalId_CreatedAt
            ON career.OutcomeReviewLog (ActorExternalId, CreatedAt DESC);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
