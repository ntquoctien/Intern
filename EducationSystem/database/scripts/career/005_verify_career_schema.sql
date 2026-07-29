/*
    Verifies the career schema, its metadata, seed data and important rejection rules.
    All functional test rows are inserted inside a transaction and rolled back.
*/
SET NOCOUNT ON;
SET XACT_ABORT OFF;

IF DB_NAME() <> N'TayDoV2'
    THROW 51000, N'Refusing to verify schema: the current database is not TayDoV2.', 1;

IF SCHEMA_ID(N'career') IS NULL
    THROW 51000, N'Verification failed: schema career does not exist.', 1;

DECLARE @RequiredTables TABLE (TableName SYSNAME PRIMARY KEY);
INSERT @RequiredTables (TableName)
VALUES
    (N'CurriculumVersion'),
    (N'ProgressionLevel'),
    (N'ProgramLearningOutcome'),
    (N'CourseLearningOutcome'),
    (N'CloPloMapping');

IF EXISTS
(
    SELECT 1
    FROM @RequiredTables AS required
    WHERE OBJECT_ID(N'career.' + QUOTENAME(required.TableName), N'U') IS NULL
)
    THROW 51000, N'Verification failed: one or more required career tables are missing.', 1;

IF (SELECT COUNT(*) FROM career.ProgressionLevel WHERE Code IN ('E', 'R', 'D')) <> 3
    THROW 51000, N'Verification failed: E, R and D have not all been seeded.', 1;

IF EXISTS
(
    SELECT expected.Code, expected.Name, expected.VietnameseName, expected.Rank
    FROM
    (
        VALUES
            (CAST('E' AS CHAR(1)), CAST(N'Enabling' AS NVARCHAR(50)), CAST(N'Nền tảng' AS NVARCHAR(50)), CAST(1 AS TINYINT)),
            (CAST('R' AS CHAR(1)), CAST(N'Reinforcing' AS NVARCHAR(50)), CAST(N'Củng cố' AS NVARCHAR(50)), CAST(2 AS TINYINT)),
            (CAST('D' AS CHAR(1)), CAST(N'Demonstrating' AS NVARCHAR(50)), CAST(N'Thể hiện' AS NVARCHAR(50)), CAST(3 AS TINYINT))
    ) AS expected(Code, Name, VietnameseName, Rank)
    LEFT JOIN career.ProgressionLevel actual ON actual.Code = expected.Code
    WHERE actual.Code IS NULL
       OR actual.Name <> expected.Name
       OR actual.VietnameseName <> expected.VietnameseName
       OR actual.Rank <> expected.Rank
       OR actual.IsActive <> 1
)
    THROW 51000, N'Verification failed: E-R-D seed values are incorrect.', 1;

IF
(
    SELECT COUNT(*)
    FROM sys.key_constraints kc
    JOIN sys.tables t ON t.object_id = kc.parent_object_id
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'career' AND kc.type = 'PK'
) <> 5
    THROW 51000, N'Verification failed: all five career tables must have primary keys.', 1;

DECLARE @RequiredForeignKeys TABLE (ConstraintName SYSNAME PRIMARY KEY);
INSERT @RequiredForeignKeys (ConstraintName)
VALUES
    (N'FK_ProgramLearningOutcome_CurriculumVersion'),
    (N'FK_CourseLearningOutcome_CurriculumVersion'),
    (N'FK_CloPloMapping_CurriculumVersion'),
    (N'FK_CloPloMapping_CourseLearningOutcome'),
    (N'FK_CloPloMapping_ProgramLearningOutcome'),
    (N'FK_CloPloMapping_ProgressionLevel');

IF EXISTS
(
    SELECT 1
    FROM @RequiredForeignKeys required
    LEFT JOIN sys.foreign_keys fk ON fk.name = required.ConstraintName
    WHERE fk.object_id IS NULL
)
    THROW 51000, N'Verification failed: an internal career foreign key is missing.', 1;

IF EXISTS
(
    SELECT 1
    FROM sys.foreign_keys fk
    WHERE OBJECT_SCHEMA_NAME(fk.parent_object_id) = N'career'
      AND OBJECT_SCHEMA_NAME(fk.referenced_object_id) <> N'career'
)
    THROW 51000, N'Verification failed: a cross-schema foreign key originates in career.', 1;

IF EXISTS
(
    SELECT 1
    FROM sys.foreign_keys fk
    WHERE OBJECT_SCHEMA_NAME(fk.parent_object_id) = N'career'
      AND fk.delete_referential_action <> 0
)
    THROW 51000, N'Verification failed: a career foreign key uses a cascading delete action.', 1;

DECLARE @RequiredUniqueConstraints TABLE (ConstraintName SYSNAME PRIMARY KEY);
INSERT @RequiredUniqueConstraints (ConstraintName)
VALUES
    (N'UQ_CurriculumVersion_MajorCode_CurriculumCode_Version'),
    (N'UQ_ProgressionLevel_Rank'),
    (N'UQ_ProgramLearningOutcome_CurriculumVersionId_PloCode'),
    (N'UQ_ProgramLearningOutcome_Id_CurriculumVersionId'),
    (N'UQ_CourseLearningOutcome_CurriculumVersionId_SubjectCode_CloCode'),
    (N'UQ_CourseLearningOutcome_Id_CurriculumVersionId'),
    (N'UQ_CloPloMapping_CurriculumVersionId_CloId_PloId');

IF EXISTS
(
    SELECT 1
    FROM @RequiredUniqueConstraints required
    LEFT JOIN sys.key_constraints kc
        ON kc.name = required.ConstraintName AND kc.type = 'UQ'
    WHERE kc.object_id IS NULL
)
    THROW 51000, N'Verification failed: a required unique constraint is missing.', 1;

DECLARE @RequiredStatusChecks TABLE (ConstraintName SYSNAME PRIMARY KEY);
INSERT @RequiredStatusChecks (ConstraintName)
VALUES
    (N'CK_CurriculumVersion_Status'),
    (N'CK_ProgramLearningOutcome_Status'),
    (N'CK_CourseLearningOutcome_Status');

IF EXISTS
(
    SELECT 1
    FROM @RequiredStatusChecks required
    LEFT JOIN sys.check_constraints cc ON cc.name = required.ConstraintName
    WHERE cc.object_id IS NULL OR cc.is_disabled = 1 OR cc.is_not_trusted = 1
)
    THROW 51000, N'Verification failed: a required trusted status check constraint is missing.', 1;

DECLARE @RequiredIndexes TABLE (TableName SYSNAME, IndexName SYSNAME, PRIMARY KEY (TableName, IndexName));
INSERT @RequiredIndexes (TableName, IndexName)
VALUES
    (N'ProgramLearningOutcome', N'IX_ProgramLearningOutcome_CurriculumVersionId'),
    (N'ProgramLearningOutcome', N'IX_ProgramLearningOutcome_Status'),
    (N'ProgramLearningOutcome', N'IX_ProgramLearningOutcome_CurriculumVersionId_Status'),
    (N'CourseLearningOutcome', N'IX_CourseLearningOutcome_CurriculumVersionId'),
    (N'CourseLearningOutcome', N'IX_CourseLearningOutcome_SubjectCode'),
    (N'CourseLearningOutcome', N'IX_CourseLearningOutcome_Status'),
    (N'CourseLearningOutcome', N'IX_CourseLearningOutcome_CurriculumVersionId_SubjectCode'),
    (N'CourseLearningOutcome', N'IX_CourseLearningOutcome_CurriculumVersionId_Status'),
    (N'CloPloMapping', N'IX_CloPloMapping_CloId'),
    (N'CloPloMapping', N'IX_CloPloMapping_PloId'),
    (N'CloPloMapping', N'IX_CloPloMapping_CurriculumVersionId'),
    (N'CloPloMapping', N'IX_CloPloMapping_ProgressionLevelCode'),
    (N'CloPloMapping', N'IX_CloPloMapping_CurriculumVersionId_ProgressionLevelCode'),
    (N'CloPloMapping', N'IX_CloPloMapping_PloId_ProgressionLevelCode');

IF EXISTS
(
    SELECT 1
    FROM @RequiredIndexes required
    LEFT JOIN sys.tables t
        ON t.name = required.TableName AND SCHEMA_NAME(t.schema_id) = N'career'
    LEFT JOIN sys.indexes i
        ON i.object_id = t.object_id AND i.name = required.IndexName
    WHERE i.index_id IS NULL
)
    THROW 51000, N'Verification failed: a required query index is missing.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Token NVARCHAR(32) = REPLACE(CONVERT(NVARCHAR(36), NEWID()), N'-', N'');
    DECLARE @CurriculumVersionId BIGINT;
    DECLARE @OtherCurriculumVersionId BIGINT;
    DECLARE @Plo1Id BIGINT;
    DECLARE @Plo2Id BIGINT;
    DECLARE @OtherPloId BIGINT;
    DECLARE @Clo1Id BIGINT;
    DECLARE @Clo2Id BIGINT;
    DECLARE @WasRejected BIT;

    INSERT career.CurriculumVersion
        (MajorCode, MajorName, CurriculumCode, CurriculumName, Version,
         EffectiveFrom, EffectiveTo, Status)
    VALUES
        (N'VERIFY-' + @Token, N'Ngành kiểm thử',
         N'CUR-' + @Token, N'Chương trình kiểm thử', N'v1',
         '2026-01-01', '2029-12-31', N'Approved');
    SET @CurriculumVersionId = SCOPE_IDENTITY();

    INSERT career.ProgramLearningOutcome
        (CurriculumVersionId, PloCode, Description, SortOrder, Status)
    VALUES
        (@CurriculumVersionId, N'PLO1', N'PLO kiểm thử thứ nhất.', 1, N'Approved'),
        (@CurriculumVersionId, N'PLO2', N'PLO kiểm thử thứ hai.', 2, N'Approved');

    SELECT @Plo1Id = Id
    FROM career.ProgramLearningOutcome
    WHERE CurriculumVersionId = @CurriculumVersionId AND PloCode = N'PLO1';

    SELECT @Plo2Id = Id
    FROM career.ProgramLearningOutcome
    WHERE CurriculumVersionId = @CurriculumVersionId AND PloCode = N'PLO2';

    INSERT career.CourseLearningOutcome
        (CurriculumVersionId, SubjectCode, SubjectName, Credits,
         CloCode, Description, SortOrder, Status)
    VALUES
        (@CurriculumVersionId, N'SUB-A', N'Môn kiểm thử A', 3,
         N'CLO1', N'CLO kiểm thử thứ nhất.', 1, N'Approved'),
        (@CurriculumVersionId, N'SUB-A', N'Môn kiểm thử A', 3,
         N'CLO2', N'CLO kiểm thử thứ hai.', 2, N'Approved');

    SELECT @Clo1Id = Id
    FROM career.CourseLearningOutcome
    WHERE CurriculumVersionId = @CurriculumVersionId
      AND SubjectCode = N'SUB-A' AND CloCode = N'CLO1';

    SELECT @Clo2Id = Id
    FROM career.CourseLearningOutcome
    WHERE CurriculumVersionId = @CurriculumVersionId
      AND SubjectCode = N'SUB-A' AND CloCode = N'CLO2';

    INSERT career.CloPloMapping
        (CurriculumVersionId, CloId, PloId, ProgressionLevelCode, IsApproved)
    VALUES
        (@CurriculumVersionId, @Clo1Id, @Plo1Id, 'E', 1),
        (@CurriculumVersionId, @Clo1Id, @Plo2Id, 'R', 1),
        (@CurriculumVersionId, @Clo2Id, @Plo2Id, 'D', 1);

    IF
    (
        SELECT COUNT(*)
        FROM career.CloPloMapping
        WHERE CurriculumVersionId = @CurriculumVersionId
          AND ProgressionLevelCode IN ('E', 'R', 'D')
    ) <> 3
        THROW 51000, N'Verification failed: valid E-R-D mappings could not be inserted.', 1;

    SET @WasRejected = 0;
    BEGIN TRY
        INSERT career.CloPloMapping
            (CurriculumVersionId, CloId, PloId, ProgressionLevelCode)
        VALUES
            (@CurriculumVersionId, @Clo2Id, @Plo1Id, 'X');
    END TRY
    BEGIN CATCH
        SET @WasRejected = 1;
    END CATCH;
    IF @WasRejected = 0
        THROW 51000, N'Verification failed: an invalid progression code was accepted.', 1;

    SET @WasRejected = 0;
    BEGIN TRY
        INSERT career.ProgramLearningOutcome
            (CurriculumVersionId, PloCode, Description)
        VALUES
            (@CurriculumVersionId, N'PLO1', N'Mã PLO bị trùng.');
    END TRY
    BEGIN CATCH
        SET @WasRejected = 1;
    END CATCH;
    IF @WasRejected = 0
        THROW 51000, N'Verification failed: a duplicate PLO code was accepted.', 1;

    SET @WasRejected = 0;
    BEGIN TRY
        INSERT career.CourseLearningOutcome
            (CurriculumVersionId, SubjectCode, SubjectName, Credits, CloCode, Description)
        VALUES
            (@CurriculumVersionId, N'SUB-A', N'Môn kiểm thử A', 3, N'CLO1', N'Mã CLO bị trùng.');
    END TRY
    BEGIN CATCH
        SET @WasRejected = 1;
    END CATCH;
    IF @WasRejected = 0
        THROW 51000, N'Verification failed: a duplicate CLO code was accepted.', 1;

    SET @WasRejected = 0;
    BEGIN TRY
        INSERT career.CloPloMapping
            (CurriculumVersionId, CloId, PloId, ProgressionLevelCode)
        VALUES
            (@CurriculumVersionId, @Clo1Id, @Plo1Id, 'E');
    END TRY
    BEGIN CATCH
        SET @WasRejected = 1;
    END CATCH;
    IF @WasRejected = 0
        THROW 51000, N'Verification failed: a duplicate CLO-PLO mapping was accepted.', 1;

    INSERT career.CurriculumVersion
        (MajorCode, MajorName, CurriculumCode, Version, Status)
    VALUES
        (N'VERIFY-' + @Token, N'Ngành kiểm thử',
         N'CUR-' + @Token, N'v2', N'Approved');
    SET @OtherCurriculumVersionId = SCOPE_IDENTITY();

    INSERT career.ProgramLearningOutcome
        (CurriculumVersionId, PloCode, Description, Status)
    VALUES
        (@OtherCurriculumVersionId, N'PLO1', N'PLO thuộc phiên bản khác.', N'Approved');
    SET @OtherPloId = SCOPE_IDENTITY();

    SET @WasRejected = 0;
    BEGIN TRY
        INSERT career.CloPloMapping
            (CurriculumVersionId, CloId, PloId, ProgressionLevelCode)
        VALUES
            (@CurriculumVersionId, @Clo2Id, @OtherPloId, 'E');
    END TRY
    BEGIN CATCH
        SET @WasRejected = 1;
    END CATCH;
    IF @WasRejected = 0
        THROW 51000, N'Verification failed: a cross-curriculum mapping was accepted.', 1;

    SET @WasRejected = 0;
    BEGIN TRY
        INSERT career.CurriculumVersion
            (MajorCode, MajorName, CurriculumCode, Version, EffectiveFrom, EffectiveTo)
        VALUES
            (N'BAD-' + @Token, N'Ngành kiểm thử',
             N'BAD-DATE-' + @Token, N'v1', '2027-01-01', '2026-01-01');
    END TRY
    BEGIN CATCH
        SET @WasRejected = 1;
    END CATCH;
    IF @WasRejected = 0
        THROW 51000, N'Verification failed: an invalid effective date range was accepted.', 1;

    SET @WasRejected = 0;
    BEGIN TRY
        INSERT career.CourseLearningOutcome
            (CurriculumVersionId, SubjectCode, SubjectName, Credits, CloCode, Description)
        VALUES
            (@CurriculumVersionId, N'SUB-B', N'Môn kiểm thử B', 0, N'CLO1', N'CLO có số tín chỉ sai.');
    END TRY
    BEGIN CATCH
        SET @WasRejected = 1;
    END CATCH;
    IF @WasRejected = 0
        THROW 51000, N'Verification failed: non-positive credits were accepted.', 1;

    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT
    N'PASS' AS VerificationStatus,
    DB_NAME() AS DatabaseName,
    N'career' AS SchemaName,
    5 AS VerifiedTableCount,
    N'All metadata and transactional rejection tests passed.' AS Message;
