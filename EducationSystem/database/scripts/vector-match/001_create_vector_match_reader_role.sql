/*
    Creates the least-privilege database role used by VectorMatchService.
    Run in the EducationSystem database (TayDoV2), then add the chosen database
    user to [vector_match_reader].
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF DATABASE_PRINCIPAL_ID(N'vector_match_reader') IS NULL
        CREATE ROLE [vector_match_reader] AUTHORIZATION [dbo];

    DECLARE @RequiredObjects TABLE
    (
        ObjectName nvarchar(300) NOT NULL PRIMARY KEY
    );

    INSERT @RequiredObjects (ObjectName)
    VALUES
        (N'academic.StudentEvaluations'),
        (N'academic.SubjectTeachings'),
        (N'academic.Subjects'),
        (N'academic.StudentEvaluationDetails'),
        (N'academic.EvaluationCriterias'),
        (N'academic.StudentProjects'),
        (N'academic.StudentInternships'),
        (N'career.CourseLearningOutcome'),
        (N'career.CloPloMapping');

    IF EXISTS
    (
        SELECT 1
        FROM @RequiredObjects required
        WHERE OBJECT_ID(required.ObjectName, N'U') IS NULL
    )
    BEGIN
        DECLARE @Missing nvarchar(max) =
        (
            SELECT STRING_AGG(required.ObjectName, N', ')
            FROM @RequiredObjects required
            WHERE OBJECT_ID(required.ObjectName, N'U') IS NULL
        );
        DECLARE @MissingMessage nvarchar(2048) =
            CONCAT(N'Vector match required tables are missing: ', @Missing);
        THROW 51000, @MissingMessage, 1;
    END;

    GRANT SELECT ON OBJECT::academic.StudentEvaluations
        TO [vector_match_reader];
    GRANT SELECT ON OBJECT::academic.SubjectTeachings
        TO [vector_match_reader];
    GRANT SELECT ON OBJECT::academic.Subjects
        TO [vector_match_reader];
    GRANT SELECT ON OBJECT::academic.StudentEvaluationDetails
        TO [vector_match_reader];
    GRANT SELECT ON OBJECT::academic.EvaluationCriterias
        TO [vector_match_reader];
    GRANT SELECT ON OBJECT::academic.StudentProjects
        TO [vector_match_reader];
    GRANT SELECT ON OBJECT::academic.StudentInternships
        TO [vector_match_reader];
    GRANT SELECT ON OBJECT::career.CourseLearningOutcome
        TO [vector_match_reader];
    GRANT SELECT ON OBJECT::career.CloPloMapping
        TO [vector_match_reader];

    COMMIT TRANSACTION;
    SELECT N'Created vector_match_reader role and read-only grants.' AS Message;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

