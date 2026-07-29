/*
    Removes only the objects introduced by the career schema scripts.
    This script is intentionally destructive and must be run against TayDoV2 explicitly.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'TayDoV2'
    THROW 51000, N'Refusing rollback: the current database is not TayDoV2.', 1;

IF SCHEMA_ID(N'career') IS NOT NULL
   AND EXISTS
   (
       SELECT 1
       FROM sys.objects
       WHERE schema_id = SCHEMA_ID(N'career')
         AND type IN ('U', 'V', 'P', 'FN', 'IF', 'TF', 'SO', 'SN')
         AND name NOT IN
         (
             N'CurriculumVersion',
             N'ProgressionLevel',
             N'ProgramLearningOutcome',
             N'CourseLearningOutcome',
             N'CloPloMapping'
         )
   )
    THROW 51000, N'Refusing rollback: schema career contains unexpected objects.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    DROP TABLE IF EXISTS career.CloPloMapping;
    DROP TABLE IF EXISTS career.CourseLearningOutcome;
    DROP TABLE IF EXISTS career.ProgramLearningOutcome;
    DROP TABLE IF EXISTS career.ProgressionLevel;
    DROP TABLE IF EXISTS career.CurriculumVersion;

    IF SCHEMA_ID(N'career') IS NOT NULL
        EXEC(N'DROP SCHEMA [career];');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
