# Read-only Student Portal and management UI completion report

## Delivered access and API surface

- Identity: `POST /api/auth/student/login`, `GET /api/student/me/session`.
- Academic Student: `GET /api/student/me/profile`, `/program`, `/subjects`, `/schedule`, `/attendance`.
- Exam Student: `GET /api/student/me/exam-results` and `/exam-results/{id}`.
- Communication Student: `GET /api/student/me/form-requests` and `/form-requests/{id}`.
- Internal lookup: exact Nickname/UserId student resolution and minimal safe User summaries.
- Management Academic: dashboard, student summary/detail, teacher, plan, class, weekly schedule, and raw attendance projections.
- Management Exam: raw result list/statistics and QuestionSuite → Question → Answer projections.

All Student detail/list queries derive `studentId` from the validated principal. Foreign IDs return the same not-found contract as missing IDs.

## Delivered frontend routes

Student routes: login, dashboard, profile, program/semester plans, subjects/classes, responsive schedule, raw exam results, raw attendance, and existing form requests.

Management routes are grouped under Overview, Education, People, Teaching, Assessment, and Forms/Communication. All ten previous business URLs redirect to their new route and preserve the query string. Supported list search, paging, page size, and sort state are URL-backed.

## Known data and rule gaps

- `Students.Nickname` is the configured demo code source, not a formally approved database rename or MSSV definition.
- No password-verification algorithm is present, so MSSV-only access is demo/internal-only.
- Attendance status values have no confirmed semantic mapping.
- Exam results have no confirmed official-result or pass/fail rule.
- Semester plans have no curriculum version or required/elective semantics.
- SubjectTeaching is a course offering, not an administrative class.
- GPA, earned/required credits, completion percentage, pass rate, and attendance rate remain unavailable.

## Read-only evidence

- The migration manifest contains the original 12 migration/snapshot files and rejects any delta.
- Static verification rejects EF write methods, modifying SQL, and non-login mutation actions in the feature scope.
- Every service DbContext registers `ReadOnlyCommandInterceptor`, which rejects non-SELECT/CTE commands.
- Ownership tests cover Academic, Exam, and Communication cross-student access.
- Response-contract tests reject password, salt, identification, address, family, mobile, push-token, and device-token fields.
- The SELECT-only database snapshot records 36 table counts. Critical pre-change observations remain `identity.Users=665` and `academic.Students=577`; the full post-verification snapshot matches exactly.
- Public ingress tests allow only student login/self-service paths and reject generic/internal/management paths.
