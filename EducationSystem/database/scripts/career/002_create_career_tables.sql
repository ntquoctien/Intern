/*
    Creates the five relational tables owned by the career schema.
    No existing table is altered and no cross-schema foreign key is created.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'TayDoV2'
    THROW 51000, N'Refusing to create tables: the current database is not TayDoV2.', 1;

IF SCHEMA_ID(N'career') IS NULL
    THROW 51000, N'Schema career does not exist. Run 001_create_career_schema.sql first.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'career.CurriculumVersion', N'U') IS NULL
    BEGIN
        CREATE TABLE career.CurriculumVersion
        (
            Id BIGINT IDENTITY(1,1) NOT NULL,
            MajorExternalId UNIQUEIDENTIFIER NULL,
            MajorCode NVARCHAR(100) NOT NULL,
            MajorName NVARCHAR(255) NOT NULL,
            CurriculumCode NVARCHAR(100) NOT NULL,
            CurriculumName NVARCHAR(255) NULL,
            Version NVARCHAR(50) NOT NULL,
            EffectiveFrom DATE NULL,
            EffectiveTo DATE NULL,
            Status NVARCHAR(20) NOT NULL
                CONSTRAINT DF_CurriculumVersion_Status DEFAULT (N'Draft'),
            Description NVARCHAR(MAX) NULL,
            CreatedAt DATETIME2(7) NOT NULL
                CONSTRAINT DF_CurriculumVersion_CreatedAt DEFAULT (SYSUTCDATETIME()),
            UpdatedAt DATETIME2(7) NOT NULL
                CONSTRAINT DF_CurriculumVersion_UpdatedAt DEFAULT (SYSUTCDATETIME()),
            RowVersion ROWVERSION NOT NULL,
            CONSTRAINT PK_CurriculumVersion PRIMARY KEY (Id),
            CONSTRAINT UQ_CurriculumVersion_MajorCode_CurriculumCode_Version
                UNIQUE (MajorCode, CurriculumCode, Version),
            CONSTRAINT CK_CurriculumVersion_MajorCode_NotBlank
                CHECK (LEN(LTRIM(RTRIM(MajorCode))) > 0),
            CONSTRAINT CK_CurriculumVersion_MajorName_NotBlank
                CHECK (LEN(LTRIM(RTRIM(MajorName))) > 0),
            CONSTRAINT CK_CurriculumVersion_CurriculumCode_NotBlank
                CHECK (LEN(LTRIM(RTRIM(CurriculumCode))) > 0),
            CONSTRAINT CK_CurriculumVersion_Version_NotBlank
                CHECK (LEN(LTRIM(RTRIM(Version))) > 0),
            CONSTRAINT CK_CurriculumVersion_Status
                CHECK (Status IN (N'Draft', N'PendingReview', N'Approved', N'Archived')),
            CONSTRAINT CK_CurriculumVersion_EffectiveDateRange
                CHECK (EffectiveTo IS NULL OR EffectiveFrom IS NULL OR EffectiveTo >= EffectiveFrom)
        );
    END;

    IF OBJECT_ID(N'career.ProgressionLevel', N'U') IS NULL
    BEGIN
        CREATE TABLE career.ProgressionLevel
        (
            Code CHAR(1) NOT NULL,
            Name NVARCHAR(50) NOT NULL,
            VietnameseName NVARCHAR(50) NOT NULL,
            Description NVARCHAR(1000) NOT NULL,
            Rank TINYINT NOT NULL,
            IsActive BIT NOT NULL
                CONSTRAINT DF_ProgressionLevel_IsActive DEFAULT ((1)),
            CreatedAt DATETIME2(7) NOT NULL
                CONSTRAINT DF_ProgressionLevel_CreatedAt DEFAULT (SYSUTCDATETIME()),
            CONSTRAINT PK_ProgressionLevel PRIMARY KEY (Code),
            CONSTRAINT UQ_ProgressionLevel_Rank UNIQUE (Rank),
            CONSTRAINT CK_ProgressionLevel_Code CHECK (Code IN ('E', 'R', 'D')),
            CONSTRAINT CK_ProgressionLevel_Rank CHECK (Rank IN (1, 2, 3)),
            CONSTRAINT CK_ProgressionLevel_Name_NotBlank
                CHECK (LEN(LTRIM(RTRIM(Name))) > 0),
            CONSTRAINT CK_ProgressionLevel_VietnameseName_NotBlank
                CHECK (LEN(LTRIM(RTRIM(VietnameseName))) > 0)
        );
    END;

    IF OBJECT_ID(N'career.ProgramLearningOutcome', N'U') IS NULL
    BEGIN
        CREATE TABLE career.ProgramLearningOutcome
        (
            Id BIGINT IDENTITY(1,1) NOT NULL,
            CurriculumVersionId BIGINT NOT NULL,
            PloCode NVARCHAR(50) NOT NULL,
            Description NVARCHAR(MAX) NOT NULL,
            OriginalDescription NVARCHAR(MAX) NULL,
            NormalizedDescription NVARCHAR(MAX) NULL,
            SortOrder INT NOT NULL
                CONSTRAINT DF_ProgramLearningOutcome_SortOrder DEFAULT ((0)),
            Status NVARCHAR(20) NOT NULL
                CONSTRAINT DF_ProgramLearningOutcome_Status DEFAULT (N'Draft'),
            CreatedAt DATETIME2(7) NOT NULL
                CONSTRAINT DF_ProgramLearningOutcome_CreatedAt DEFAULT (SYSUTCDATETIME()),
            UpdatedAt DATETIME2(7) NOT NULL
                CONSTRAINT DF_ProgramLearningOutcome_UpdatedAt DEFAULT (SYSUTCDATETIME()),
            RowVersion ROWVERSION NOT NULL,
            CONSTRAINT PK_ProgramLearningOutcome PRIMARY KEY (Id),
            CONSTRAINT UQ_ProgramLearningOutcome_CurriculumVersionId_PloCode
                UNIQUE (CurriculumVersionId, PloCode),
            CONSTRAINT UQ_ProgramLearningOutcome_Id_CurriculumVersionId
                UNIQUE (Id, CurriculumVersionId),
            CONSTRAINT FK_ProgramLearningOutcome_CurriculumVersion
                FOREIGN KEY (CurriculumVersionId)
                REFERENCES career.CurriculumVersion (Id)
                ON DELETE NO ACTION,
            CONSTRAINT CK_ProgramLearningOutcome_PloCode_NotBlank
                CHECK (LEN(LTRIM(RTRIM(PloCode))) > 0),
            CONSTRAINT CK_ProgramLearningOutcome_Description_NotBlank
                CHECK (LEN(LTRIM(RTRIM(Description))) > 0),
            CONSTRAINT CK_ProgramLearningOutcome_SortOrder
                CHECK (SortOrder >= 0),
            CONSTRAINT CK_ProgramLearningOutcome_Status
                CHECK (Status IN (N'Draft', N'PendingReview', N'Approved', N'Rejected', N'Archived'))
        );
    END;

    IF OBJECT_ID(N'career.CourseLearningOutcome', N'U') IS NULL
    BEGIN
        CREATE TABLE career.CourseLearningOutcome
        (
            Id BIGINT IDENTITY(1,1) NOT NULL,
            CurriculumVersionId BIGINT NOT NULL,
            SubjectExternalId UNIQUEIDENTIFIER NULL,
            SubjectCode NVARCHAR(100) NOT NULL,
            SubjectName NVARCHAR(255) NOT NULL,
            Credits INT NULL,
            CloCode NVARCHAR(50) NOT NULL,
            Description NVARCHAR(MAX) NOT NULL,
            OriginalDescription NVARCHAR(MAX) NULL,
            NormalizedDescription NVARCHAR(MAX) NULL,
            SortOrder INT NOT NULL
                CONSTRAINT DF_CourseLearningOutcome_SortOrder DEFAULT ((0)),
            Status NVARCHAR(20) NOT NULL
                CONSTRAINT DF_CourseLearningOutcome_Status DEFAULT (N'Draft'),
            CreatedAt DATETIME2(7) NOT NULL
                CONSTRAINT DF_CourseLearningOutcome_CreatedAt DEFAULT (SYSUTCDATETIME()),
            UpdatedAt DATETIME2(7) NOT NULL
                CONSTRAINT DF_CourseLearningOutcome_UpdatedAt DEFAULT (SYSUTCDATETIME()),
            RowVersion ROWVERSION NOT NULL,
            CONSTRAINT PK_CourseLearningOutcome PRIMARY KEY (Id),
            CONSTRAINT UQ_CourseLearningOutcome_CurriculumVersionId_SubjectCode_CloCode
                UNIQUE (CurriculumVersionId, SubjectCode, CloCode),
            CONSTRAINT UQ_CourseLearningOutcome_Id_CurriculumVersionId
                UNIQUE (Id, CurriculumVersionId),
            CONSTRAINT FK_CourseLearningOutcome_CurriculumVersion
                FOREIGN KEY (CurriculumVersionId)
                REFERENCES career.CurriculumVersion (Id)
                ON DELETE NO ACTION,
            CONSTRAINT CK_CourseLearningOutcome_SubjectCode_NotBlank
                CHECK (LEN(LTRIM(RTRIM(SubjectCode))) > 0),
            CONSTRAINT CK_CourseLearningOutcome_SubjectName_NotBlank
                CHECK (LEN(LTRIM(RTRIM(SubjectName))) > 0),
            CONSTRAINT CK_CourseLearningOutcome_CloCode_NotBlank
                CHECK (LEN(LTRIM(RTRIM(CloCode))) > 0),
            CONSTRAINT CK_CourseLearningOutcome_Description_NotBlank
                CHECK (LEN(LTRIM(RTRIM(Description))) > 0),
            CONSTRAINT CK_CourseLearningOutcome_Credits
                CHECK (Credits IS NULL OR Credits > 0),
            CONSTRAINT CK_CourseLearningOutcome_SortOrder
                CHECK (SortOrder >= 0),
            CONSTRAINT CK_CourseLearningOutcome_Status
                CHECK (Status IN (N'Draft', N'PendingReview', N'Approved', N'Rejected', N'Archived'))
        );
    END;

    IF OBJECT_ID(N'career.CloPloMapping', N'U') IS NULL
    BEGIN
        CREATE TABLE career.CloPloMapping
        (
            Id BIGINT IDENTITY(1,1) NOT NULL,
            CurriculumVersionId BIGINT NOT NULL,
            CloId BIGINT NOT NULL,
            PloId BIGINT NOT NULL,
            ProgressionLevelCode CHAR(1) NOT NULL,
            IsApproved BIT NOT NULL
                CONSTRAINT DF_CloPloMapping_IsApproved DEFAULT ((0)),
            Note NVARCHAR(1000) NULL,
            CreatedAt DATETIME2(7) NOT NULL
                CONSTRAINT DF_CloPloMapping_CreatedAt DEFAULT (SYSUTCDATETIME()),
            UpdatedAt DATETIME2(7) NOT NULL
                CONSTRAINT DF_CloPloMapping_UpdatedAt DEFAULT (SYSUTCDATETIME()),
            RowVersion ROWVERSION NOT NULL,
            CONSTRAINT PK_CloPloMapping PRIMARY KEY (Id),
            CONSTRAINT UQ_CloPloMapping_CurriculumVersionId_CloId_PloId
                UNIQUE (CurriculumVersionId, CloId, PloId),
            CONSTRAINT FK_CloPloMapping_CurriculumVersion
                FOREIGN KEY (CurriculumVersionId)
                REFERENCES career.CurriculumVersion (Id)
                ON DELETE NO ACTION,
            CONSTRAINT FK_CloPloMapping_CourseLearningOutcome
                FOREIGN KEY (CloId, CurriculumVersionId)
                REFERENCES career.CourseLearningOutcome (Id, CurriculumVersionId)
                ON DELETE NO ACTION,
            CONSTRAINT FK_CloPloMapping_ProgramLearningOutcome
                FOREIGN KEY (PloId, CurriculumVersionId)
                REFERENCES career.ProgramLearningOutcome (Id, CurriculumVersionId)
                ON DELETE NO ACTION,
            CONSTRAINT FK_CloPloMapping_ProgressionLevel
                FOREIGN KEY (ProgressionLevelCode)
                REFERENCES career.ProgressionLevel (Code)
                ON DELETE NO ACTION
        );
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
