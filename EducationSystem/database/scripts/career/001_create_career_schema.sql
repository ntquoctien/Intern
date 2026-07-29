/*
    Creates the isolated career schema for Academic-to-Career Mapping.
    Deployment target: TayDoV2.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'TayDoV2'
    THROW 51000, N'Refusing to create schema: the current database is not TayDoV2.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    IF SCHEMA_ID(N'career') IS NULL
        EXEC(N'CREATE SCHEMA [career] AUTHORIZATION [dbo];');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
