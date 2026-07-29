/*
    Creates query indexes required by curriculum, outcome and matrix lookups.
    Unique indexes created by table constraints are not duplicated.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'TayDoV2'
    THROW 51000, N'Refusing to create indexes: the current database is not TayDoV2.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'career.ProgramLearningOutcome', N'U') IS NULL
       OR OBJECT_ID(N'career.CourseLearningOutcome', N'U') IS NULL
       OR OBJECT_ID(N'career.CloPloMapping', N'U') IS NULL
        THROW 51000, N'Career tables are missing. Run 002_create_career_tables.sql first.', 1;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'career.ProgramLearningOutcome') AND name = N'IX_ProgramLearningOutcome_CurriculumVersionId')
        CREATE INDEX IX_ProgramLearningOutcome_CurriculumVersionId
            ON career.ProgramLearningOutcome (CurriculumVersionId);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'career.ProgramLearningOutcome') AND name = N'IX_ProgramLearningOutcome_Status')
        CREATE INDEX IX_ProgramLearningOutcome_Status
            ON career.ProgramLearningOutcome (Status);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'career.ProgramLearningOutcome') AND name = N'IX_ProgramLearningOutcome_CurriculumVersionId_Status')
        CREATE INDEX IX_ProgramLearningOutcome_CurriculumVersionId_Status
            ON career.ProgramLearningOutcome (CurriculumVersionId, Status);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'career.CourseLearningOutcome') AND name = N'IX_CourseLearningOutcome_CurriculumVersionId')
        CREATE INDEX IX_CourseLearningOutcome_CurriculumVersionId
            ON career.CourseLearningOutcome (CurriculumVersionId);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'career.CourseLearningOutcome') AND name = N'IX_CourseLearningOutcome_SubjectCode')
        CREATE INDEX IX_CourseLearningOutcome_SubjectCode
            ON career.CourseLearningOutcome (SubjectCode);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'career.CourseLearningOutcome') AND name = N'IX_CourseLearningOutcome_Status')
        CREATE INDEX IX_CourseLearningOutcome_Status
            ON career.CourseLearningOutcome (Status);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'career.CourseLearningOutcome') AND name = N'IX_CourseLearningOutcome_CurriculumVersionId_SubjectCode')
        CREATE INDEX IX_CourseLearningOutcome_CurriculumVersionId_SubjectCode
            ON career.CourseLearningOutcome (CurriculumVersionId, SubjectCode);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'career.CourseLearningOutcome') AND name = N'IX_CourseLearningOutcome_CurriculumVersionId_Status')
        CREATE INDEX IX_CourseLearningOutcome_CurriculumVersionId_Status
            ON career.CourseLearningOutcome (CurriculumVersionId, Status);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'career.CloPloMapping') AND name = N'IX_CloPloMapping_CloId')
        CREATE INDEX IX_CloPloMapping_CloId
            ON career.CloPloMapping (CloId);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'career.CloPloMapping') AND name = N'IX_CloPloMapping_PloId')
        CREATE INDEX IX_CloPloMapping_PloId
            ON career.CloPloMapping (PloId);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'career.CloPloMapping') AND name = N'IX_CloPloMapping_CurriculumVersionId')
        CREATE INDEX IX_CloPloMapping_CurriculumVersionId
            ON career.CloPloMapping (CurriculumVersionId);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'career.CloPloMapping') AND name = N'IX_CloPloMapping_ProgressionLevelCode')
        CREATE INDEX IX_CloPloMapping_ProgressionLevelCode
            ON career.CloPloMapping (ProgressionLevelCode);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'career.CloPloMapping') AND name = N'IX_CloPloMapping_CurriculumVersionId_ProgressionLevelCode')
        CREATE INDEX IX_CloPloMapping_CurriculumVersionId_ProgressionLevelCode
            ON career.CloPloMapping (CurriculumVersionId, ProgressionLevelCode);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'career.CloPloMapping') AND name = N'IX_CloPloMapping_PloId_ProgressionLevelCode')
        CREATE INDEX IX_CloPloMapping_PloId_ProgressionLevelCode
            ON career.CloPloMapping (PloId, ProgressionLevelCode);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
