SET NOCOUNT ON;

IF DATABASE_PRINCIPAL_ID(N'vector_match_reader') IS NULL
    THROW 51000, N'Database role vector_match_reader does not exist.', 1;

DECLARE @Expected TABLE (SchemaName sysname, TableName sysname);
INSERT @Expected (SchemaName, TableName)
VALUES
    (N'academic', N'StudentEvaluations'),
    (N'academic', N'SubjectTeachings'),
    (N'academic', N'Subjects'),
    (N'academic', N'StudentEvaluationDetails'),
    (N'academic', N'EvaluationCriterias'),
    (N'academic', N'StudentProjects'),
    (N'academic', N'StudentInternships'),
    (N'career', N'CourseLearningOutcome'),
    (N'career', N'CloPloMapping');

IF EXISTS
(
    SELECT 1
    FROM @Expected expected
    WHERE OBJECT_ID(
        QUOTENAME(expected.SchemaName) + N'.' + QUOTENAME(expected.TableName),
        N'U'
    ) IS NULL
)
    THROW 51000, N'A required vector match table does not exist.', 1;

IF EXISTS
(
    SELECT 1
    FROM @Expected expected
    INNER JOIN sys.schemas schemaInfo
        ON schemaInfo.name = expected.SchemaName
    INNER JOIN sys.objects objectInfo
        ON objectInfo.schema_id = schemaInfo.schema_id
       AND objectInfo.name = expected.TableName
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM sys.database_permissions permissionInfo
        WHERE permissionInfo.grantee_principal_id =
              DATABASE_PRINCIPAL_ID(N'vector_match_reader')
          AND permissionInfo.class = 1
          AND permissionInfo.major_id = objectInfo.object_id
          AND permissionInfo.minor_id = 0
          AND permissionInfo.permission_name = N'SELECT'
          AND permissionInfo.state IN (N'G', N'W')
    )
)
    THROW 51000, N'vector_match_reader is missing a required SELECT grant.', 1;

SELECT
    rolePrincipal.name AS RoleName,
    memberPrincipal.name AS MemberName
FROM sys.database_role_members membership
INNER JOIN sys.database_principals rolePrincipal
    ON rolePrincipal.principal_id = membership.role_principal_id
INNER JOIN sys.database_principals memberPrincipal
    ON memberPrincipal.principal_id = membership.member_principal_id
WHERE rolePrincipal.name = N'vector_match_reader'
ORDER BY memberPrincipal.name;

SELECT N'Vector match read-only permissions verified.' AS Message;

