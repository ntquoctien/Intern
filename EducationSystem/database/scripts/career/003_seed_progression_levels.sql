/*
    Seeds the controlled E-R-D progression-level catalogue.
    Existing rows are never updated or deleted.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'TayDoV2'
    THROW 51000, N'Refusing to seed data: the current database is not TayDoV2.', 1;

IF OBJECT_ID(N'career.ProgressionLevel', N'U') IS NULL
    THROW 51000, N'Table career.ProgressionLevel does not exist.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS (SELECT 1 FROM career.ProgressionLevel WHERE Code = 'E')
    BEGIN
        INSERT career.ProgressionLevel
            (Code, Name, VietnameseName, Description, Rank, IsActive)
        VALUES
            ('E', N'Enabling', N'Nền tảng',
             N'CLO đặt nền tảng kiến thức hoặc kỹ năng ban đầu cho PLO.', 1, 1);
    END;

    IF NOT EXISTS (SELECT 1 FROM career.ProgressionLevel WHERE Code = 'R')
    BEGIN
        INSERT career.ProgressionLevel
            (Code, Name, VietnameseName, Description, Rank, IsActive)
        VALUES
            ('R', N'Reinforcing', N'Củng cố',
             N'CLO củng cố, mở rộng và tăng cường năng lực liên quan đến PLO.', 2, 1);
    END;

    IF NOT EXISTS (SELECT 1 FROM career.ProgressionLevel WHERE Code = 'D')
    BEGIN
        INSERT career.ProgressionLevel
            (Code, Name, VietnameseName, Description, Rank, IsActive)
        VALUES
            ('D', N'Demonstrating', N'Thể hiện',
             N'CLO yêu cầu sinh viên tổng hợp và thể hiện năng lực của PLO.', 3, 1);
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
