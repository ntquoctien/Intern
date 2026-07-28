/*
    EducationSystem - AI Resume Builder, Phase 1
    Targets:
      - academic.StudentProjects
      - academic.StudentInternships
      - communication.FormRequests

    Important:
      The physical academic.Students.Id and academic.Subjects.Id columns are
      UNIQUEIDENTIFIER in the current EducationSystem database. StudentID and
      MappedCourseID therefore use UNIQUEIDENTIFIER so SQL Server can enforce
      the requested foreign keys.
*/

SET XACT_ABORT ON;
SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF SCHEMA_ID(N'academic') IS NULL
        EXEC(N'CREATE SCHEMA [academic]');

    IF SCHEMA_ID(N'communication') IS NULL
        EXEC(N'CREATE SCHEMA [communication]');

    IF OBJECT_ID(N'[academic].[Students]', N'U') IS NULL
        THROW 50001, 'Required table academic.Students does not exist.', 1;

    IF OBJECT_ID(N'[academic].[Subjects]', N'U') IS NULL
        THROW 50002, 'Required table academic.Subjects does not exist.', 1;

    IF OBJECT_ID(N'[communication].[FormRequests]', N'U') IS NULL
        THROW 50003, 'Required table communication.FormRequests does not exist.', 1;

    IF OBJECT_ID(N'[academic].[StudentProjects]', N'U') IS NULL
    BEGIN
        CREATE TABLE [academic].[StudentProjects]
        (
            [ProjectID] INT IDENTITY(1, 1) NOT NULL,
            [StudentID] UNIQUEIDENTIFIER NOT NULL,
            [ProjectName] NVARCHAR(255) NOT NULL,
            [TechStack] VARCHAR(255) NOT NULL,
            [ProjectDescription] NVARCHAR(MAX) NULL,
            [SourceCodeUrl] VARCHAR(255) NULL,
            [TeamSize] INT NOT NULL
                CONSTRAINT [DF_StudentProjects_TeamSize] DEFAULT (1),
            [MyRole] NVARCHAR(100) NULL,
            [MyContributions] NVARCHAR(MAX) NULL,
            [MappedCourseID] UNIQUEIDENTIFIER NULL,

            CONSTRAINT [PK_StudentProjects]
                PRIMARY KEY CLUSTERED ([ProjectID]),
            CONSTRAINT [CK_StudentProjects_TeamSize]
                CHECK ([TeamSize] >= 1),
            CONSTRAINT [FK_StudentProjects_Students_StudentID]
                FOREIGN KEY ([StudentID])
                REFERENCES [academic].[Students] ([Id])
                ON DELETE CASCADE,
            CONSTRAINT [FK_StudentProjects_Subjects_MappedCourseID]
                FOREIGN KEY ([MappedCourseID])
                REFERENCES [academic].[Subjects] ([Id])
                ON DELETE SET NULL
        );

        CREATE INDEX [IX_StudentProjects_StudentID]
            ON [academic].[StudentProjects] ([StudentID]);

        CREATE INDEX [IX_StudentProjects_MappedCourseID]
            ON [academic].[StudentProjects] ([MappedCourseID]);
    END;

    IF OBJECT_ID(N'[academic].[StudentInternships]', N'U') IS NULL
    BEGIN
        CREATE TABLE [academic].[StudentInternships]
        (
            [InternshipID] INT IDENTITY(1, 1) NOT NULL,
            [StudentID] UNIQUEIDENTIFIER NOT NULL,
            [CompanyName] NVARCHAR(255) NOT NULL,
            [Position] NVARCHAR(100) NOT NULL,
            [StartDate] DATE NOT NULL,
            [EndDate] DATE NULL,
            [TaskDescription] NVARCHAR(MAX) NULL,
            [FormRequestID] UNIQUEIDENTIFIER NULL,

            CONSTRAINT [PK_StudentInternships]
                PRIMARY KEY CLUSTERED ([InternshipID]),
            CONSTRAINT [CK_StudentInternships_DateRange]
                CHECK ([EndDate] IS NULL OR [EndDate] >= [StartDate]),
            CONSTRAINT [FK_StudentInternships_Students_StudentID]
                FOREIGN KEY ([StudentID])
                REFERENCES [academic].[Students] ([Id])
                ON DELETE CASCADE
        );

        CREATE INDEX [IX_StudentInternships_StudentID]
            ON [academic].[StudentInternships] ([StudentID]);

        CREATE INDEX [IX_StudentInternships_FormRequestID]
            ON [academic].[StudentInternships] ([FormRequestID]);
    END;

    IF COL_LENGTH(N'academic.StudentInternships', N'FormRequestID') IS NULL
        ALTER TABLE [academic].[StudentInternships]
            ADD [FormRequestID] UNIQUEIDENTIFIER NULL;

    IF EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE [name] = N'UX_StudentInternships_FormRequestID'
          AND [object_id] = OBJECT_ID(N'[academic].[StudentInternships]')
    )
        DROP INDEX [UX_StudentInternships_FormRequestID]
            ON [academic].[StudentInternships];

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE [name] = N'IX_StudentInternships_FormRequestID'
          AND [object_id] = OBJECT_ID(N'[academic].[StudentInternships]')
    )
    BEGIN
        CREATE INDEX [IX_StudentInternships_FormRequestID]
            ON [academic].[StudentInternships] ([FormRequestID]);
    END;

    IF COL_LENGTH(N'communication.FormRequests', N'EmployerToken') IS NULL
        ALTER TABLE [communication].[FormRequests]
            ADD [EmployerToken] UNIQUEIDENTIFIER NULL;

    IF COL_LENGTH(N'communication.FormRequests', N'EmployerVerifiedStatus') IS NULL
        ALTER TABLE [communication].[FormRequests]
            ADD [EmployerVerifiedStatus] INT NOT NULL
                CONSTRAINT [DF_FormRequests_EmployerVerifiedStatus] DEFAULT (0);

    IF COL_LENGTH(N'communication.FormRequests', N'VerificationData') IS NULL
        ALTER TABLE [communication].[FormRequests]
            ADD [VerificationData] NVARCHAR(MAX) NULL;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE [name] = N'UX_FormRequests_EmployerToken'
          AND [object_id] = OBJECT_ID(N'[communication].[FormRequests]')
    )
    BEGIN
        -- Dynamic SQL defers column binding until after ALTER TABLE statements
        -- in this batch have completed.
        EXEC(N'
            CREATE UNIQUE INDEX [UX_FormRequests_EmployerToken]
                ON [communication].[FormRequests] ([EmployerToken])
                WHERE [EmployerToken] IS NOT NULL;
        ');
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.check_constraints
        WHERE [name] = N'CK_FormRequests_EmployerVerifiedStatus'
          AND [parent_object_id] = OBJECT_ID(N'[communication].[FormRequests]')
    )
    BEGIN
        EXEC(N'
            ALTER TABLE [communication].[FormRequests] WITH CHECK
                ADD CONSTRAINT [CK_FormRequests_EmployerVerifiedStatus]
                CHECK ([EmployerVerifiedStatus] IN (0, 1, 2));

            ALTER TABLE [communication].[FormRequests]
                CHECK CONSTRAINT [CK_FormRequests_EmployerVerifiedStatus];
        ');
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
