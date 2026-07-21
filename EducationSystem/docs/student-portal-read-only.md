# Student Portal read-only development contract

This change treats the existing database as immutable. Application code, tests, scripts, and local setup must not create or modify schema objects, migrations, seed data, or persisted rows.

Rules for implementation:

- Student and management data access uses `AsNoTracking` and projects directly to response DTOs.
- Student ownership is derived only from the validated JWT `studentId` claim and is applied in the database query before projection or paging.
- No feature code may call EF write APIs (`SaveChanges*`, `Add*`, `Update*`, `Remove*`, `ExecuteUpdate*`, or `ExecuteDelete*`) or modifying raw SQL.
- The only non-GET action in scope is `POST /api/auth/student/login`; it performs lookup and stateless token issuance only.
- Tests use mocks/in-memory query sources or a SELECT-only connection. They must never seed the fixed database.
- Do not run `dotnet ef migrations add`, `dotnet ef database update`, or schema tooling for this change.

Run `powershell -File scripts/verify-read-only.ps1` before every release. The script compares the checked-in migration baseline and rejects forbidden write APIs/actions in the feature scope. Runtime database registrations also use a command interceptor that rejects non-read commands.
