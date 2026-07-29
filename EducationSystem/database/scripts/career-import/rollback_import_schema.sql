/*
    Removes the official document and outcome-import tables, in dependency order.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'TayDoV2'
    THROW 51000, N'Refusing career import rollback outside TayDoV2.', 1;

BEGIN TRY
    BEGIN TRANSACTION;
    DROP TABLE IF EXISTS career.ApprovedOutcomeJsonDocument;
    DROP TABLE IF EXISTS career.OutcomeReviewLog;
    DROP TABLE IF EXISTS career.OutcomeDocumentBlock;
    DROP TABLE IF EXISTS career.OutcomeImportBatch;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
