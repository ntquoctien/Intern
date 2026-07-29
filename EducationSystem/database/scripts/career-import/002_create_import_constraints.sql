/*
    Adds internal foreign keys, unique constraints and validation checks.
    No foreign key leaves the career schema and no cascade delete is used.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'TayDoV2'
    THROW 51000, N'Refusing to create career import constraints outside TayDoV2.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'career.OutcomeImportBatch', N'U') IS NULL
       OR OBJECT_ID(N'career.OutcomeDocumentBlock', N'U') IS NULL
       OR OBJECT_ID(N'career.OutcomeReviewLog', N'U') IS NULL
        THROW 51000, N'Career import tables are missing.', 1;

    IF OBJECT_ID(N'career.FK_OutcomeImportBatch_CurriculumVersion', N'F') IS NULL
        ALTER TABLE career.OutcomeImportBatch WITH CHECK
        ADD CONSTRAINT FK_OutcomeImportBatch_CurriculumVersion
            FOREIGN KEY (CurriculumVersionId)
            REFERENCES career.CurriculumVersion (Id)
            ON DELETE NO ACTION;

    IF OBJECT_ID(N'career.FK_OutcomeDocumentBlock_OutcomeImportBatch', N'F') IS NULL
        ALTER TABLE career.OutcomeDocumentBlock WITH CHECK
        ADD CONSTRAINT FK_OutcomeDocumentBlock_OutcomeImportBatch
            FOREIGN KEY (ImportBatchId)
            REFERENCES career.OutcomeImportBatch (Id)
            ON DELETE NO ACTION;

    IF OBJECT_ID(N'career.FK_OutcomeReviewLog_OutcomeImportBatch', N'F') IS NULL
        ALTER TABLE career.OutcomeReviewLog WITH CHECK
        ADD CONSTRAINT FK_OutcomeReviewLog_OutcomeImportBatch
            FOREIGN KEY (ImportBatchId)
            REFERENCES career.OutcomeImportBatch (Id)
            ON DELETE NO ACTION;

    IF OBJECT_ID(N'career.UQ_OutcomeDocumentBlock_ImportBatchId_BlockId', N'UQ') IS NULL
        ALTER TABLE career.OutcomeDocumentBlock
        ADD CONSTRAINT UQ_OutcomeDocumentBlock_ImportBatchId_BlockId
            UNIQUE (ImportBatchId, BlockId);

    IF OBJECT_ID(N'career.UQ_OutcomeDocumentBlock_ImportBatchId_Sequence', N'UQ') IS NULL
        ALTER TABLE career.OutcomeDocumentBlock
        ADD CONSTRAINT UQ_OutcomeDocumentBlock_ImportBatchId_Sequence
            UNIQUE (ImportBatchId, Sequence);

    IF OBJECT_ID(N'career.CK_OutcomeImportBatch_FileSize', N'C') IS NULL
        ALTER TABLE career.OutcomeImportBatch
        ADD CONSTRAINT CK_OutcomeImportBatch_FileSize CHECK (FileSize > 0);

    IF OBJECT_ID(N'career.CK_OutcomeImportBatch_ProgressPercent', N'C') IS NULL
        ALTER TABLE career.OutcomeImportBatch
        ADD CONSTRAINT CK_OutcomeImportBatch_ProgressPercent
            CHECK (ProgressPercent BETWEEN 0 AND 100);

    IF OBJECT_ID(N'career.CK_OutcomeImportBatch_FileHash', N'C') IS NULL
        ALTER TABLE career.OutcomeImportBatch
        ADD CONSTRAINT CK_OutcomeImportBatch_FileHash
            CHECK
            (
                LEN(FileHash) = 64
                AND FileHash COLLATE Latin1_General_100_BIN2
                    NOT LIKE '%[^0-9a-f]%' COLLATE Latin1_General_100_BIN2
            );

    IF OBJECT_ID(N'career.CK_OutcomeImportBatch_Status', N'C') IS NULL
        ALTER TABLE career.OutcomeImportBatch
        ADD CONSTRAINT CK_OutcomeImportBatch_Status
            CHECK (Status IN
            (
                N'Uploaded', N'Processing', N'PendingReview',
                N'ValidationFailed', N'Approved', N'Rejected',
                N'Failed', N'Archived'
            ));

    IF OBJECT_ID(N'career.CK_OutcomeImportBatch_ProcessingStage', N'C') IS NULL
        ALTER TABLE career.OutcomeImportBatch
        ADD CONSTRAINT CK_OutcomeImportBatch_ProcessingStage
            CHECK
            (
                ProcessingStage IS NULL
                OR ProcessingStage IN
                (
                    N'Waiting', N'ParsingDocument', N'ExtractingProgram',
                    N'ExtractingOutcomes', N'ExtractingMatrix',
                    N'Reconciling', N'Validating', N'Completed'
                )
            );

    IF OBJECT_ID(N'career.CK_OutcomeImportBatch_RawExtractionJson', N'C') IS NULL
        ALTER TABLE career.OutcomeImportBatch
        ADD CONSTRAINT CK_OutcomeImportBatch_RawExtractionJson
            CHECK (RawExtractionJson IS NULL OR ISJSON(RawExtractionJson) = 1);

    IF OBJECT_ID(N'career.CK_OutcomeImportBatch_ReviewedJson', N'C') IS NULL
        ALTER TABLE career.OutcomeImportBatch
        ADD CONSTRAINT CK_OutcomeImportBatch_ReviewedJson
            CHECK (ReviewedJson IS NULL OR ISJSON(ReviewedJson) = 1);

    IF OBJECT_ID(N'career.CK_OutcomeImportBatch_ValidationJson', N'C') IS NULL
        ALTER TABLE career.OutcomeImportBatch
        ADD CONSTRAINT CK_OutcomeImportBatch_ValidationJson
            CHECK (ValidationJson IS NULL OR ISJSON(ValidationJson) = 1);

    IF OBJECT_ID(N'career.CK_OutcomeDocumentBlock_BlockType', N'C') IS NULL
        ALTER TABLE career.OutcomeDocumentBlock
        ADD CONSTRAINT CK_OutcomeDocumentBlock_BlockType
            CHECK (BlockType IN (N'Heading', N'Paragraph', N'Table', N'TableRow', N'TableCell'));

    IF OBJECT_ID(N'career.CK_OutcomeDocumentBlock_Sequence', N'C') IS NULL
        ALTER TABLE career.OutcomeDocumentBlock
        ADD CONSTRAINT CK_OutcomeDocumentBlock_Sequence CHECK (Sequence >= 0);

    IF OBJECT_ID(N'career.CK_OutcomeDocumentBlock_HeadingLevel', N'C') IS NULL
        ALTER TABLE career.OutcomeDocumentBlock
        ADD CONSTRAINT CK_OutcomeDocumentBlock_HeadingLevel
            CHECK (HeadingLevel IS NULL OR HeadingLevel BETWEEN 1 AND 9);

    IF OBJECT_ID(N'career.CK_OutcomeDocumentBlock_ContentJson', N'C') IS NULL
        ALTER TABLE career.OutcomeDocumentBlock
        ADD CONSTRAINT CK_OutcomeDocumentBlock_ContentJson CHECK (ISJSON(ContentJson) = 1);

    IF OBJECT_ID(N'career.CK_OutcomeReviewLog_Action', N'C') IS NULL
        ALTER TABLE career.OutcomeReviewLog
        ADD CONSTRAINT CK_OutcomeReviewLog_Action
            CHECK (Action IN
            (
                N'Edit', N'Remove', N'Restore', N'ResolveWarning',
                N'Validate', N'Approve', N'Reject', N'Archive'
            ));

    IF OBJECT_ID(N'career.CK_OutcomeReviewLog_OldValueJson', N'C') IS NULL
        ALTER TABLE career.OutcomeReviewLog
        ADD CONSTRAINT CK_OutcomeReviewLog_OldValueJson
            CHECK (OldValueJson IS NULL OR ISJSON(OldValueJson) = 1);

    IF OBJECT_ID(N'career.CK_OutcomeReviewLog_NewValueJson', N'C') IS NULL
        ALTER TABLE career.OutcomeReviewLog
        ADD CONSTRAINT CK_OutcomeReviewLog_NewValueJson
            CHECK (NewValueJson IS NULL OR ISJSON(NewValueJson) = 1);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
