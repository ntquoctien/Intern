# Education Management System

Backend skeleton cho dự án Education Management System theo kiến trúc Schema-per-Service / Modular Microservices.

Phase hiện tại vẫn là skeleton, nhưng đã có nền tảng EF Core để chuẩn bị cho CRUD. Mỗi service đã có `DbContext` riêng, default schema riêng và migration đầu tay riêng theo schema-per-service.

## Công nghệ

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- REST API
- Swagger/OpenAPI
- Modular Microservices
- Schema-per-Service
- Clean Architecture ở mức khung thư mục
- SharedKernel

## Kiến trúc

Solution `EducationSystem` gồm 4 service độc lập. Mỗi service đại diện cho một boundary nghiệp vụ và sẽ sở hữu một schema database riêng trong phase sau.

```text
EducationSystem/
├── EducationSystem.sln
├── src/
│   ├── Services/
│   │   ├── IdentityService/
│   │   ├── AcademicService/
│   │   ├── ExamService/
│   │   └── CommunicationService/
│   └── BuildingBlocks/
│       └── SharedKernel/
├── docs/
│   ├── architecture.md
│   └── api-convention.md
└── README.md
```

## Danh sách service

| Service | Base route | Schema tương lai | HTTP | HTTPS |
| --- | --- | --- | --- | --- |
| IdentityService | `/api/identity` | `identity` | `5001` | `7001` |
| AcademicService | `/api/academic` | `academic` | `5002` | `7002` |
| ExamService | `/api/exam` | `exam` | `5003` | `7003` |
| CommunicationService | `/api/communication` | `communication` | `5004` | `7004` |

## Cách chạy

Thiết lập connection string bằng user secrets hoặc biến môi trường trước khi chạy migration/runtime:

```powershell
dotnet user-secrets set "ConnectionStrings:SqlServer" "Server=localhost;Database=EducationSystem;User Id=sa;Password=Your_password123;TrustServerCertificate=True;MultipleActiveResultSets=True" --project src\Services\IdentityService\IdentityService.csproj
```

Hoặc đặt biến môi trường:

```powershell
$env:ConnectionStrings__SqlServer = "Server=localhost;Database=EducationSystem;User Id=sa;Password=Your_password123;TrustServerCertificate=True;MultipleActiveResultSets=True"
```

Chạy build toàn solution:

```powershell
cd D:\Intern_IT\Intern\EducationSystem
dotnet build
```

Chạy từng service:

```powershell
dotnet run --project src\Services\IdentityService\IdentityService.csproj --launch-profile http
dotnet run --project src\Services\AcademicService\AcademicService.csproj --launch-profile http
dotnet run --project src\Services\ExamService\ExamService.csproj --launch-profile http
dotnet run --project src\Services\CommunicationService\CommunicationService.csproj --launch-profile http
```

Swagger:

- IdentityService: `http://localhost:5001/swagger`
- AcademicService: `http://localhost:5002/swagger`
- ExamService: `http://localhost:5003/swagger`
- CommunicationService: `http://localhost:5004/swagger`

## Endpoint hiện có

Health:

- `GET /api/identity/health`
- `GET /api/academic/health`
- `GET /api/exam/health`
- `GET /api/communication/health`

Service info:

- `GET /api/identity/info`
- `GET /api/academic/info`
- `GET /api/exam/info`
- `GET /api/communication/info`

## Trạng thái hiện tại

- Đã có EF Core DbContext cho từng service.
- Đã có migration đầu tay cho từng schema.
- Chưa có DTO.
- Chưa có Entity.
- Chưa có Repository.
- Chưa có CRUD API thật cho 37 bảng.
- Chưa có RabbitMQ.
- Chưa có gRPC.
- Chưa có API Gateway.
- Chưa có Docker.
- Chưa có UI.

## Future scope

- Scaffold EF Core theo từng schema/service.
- Tạo DTOs, Entities, Repository nếu cần.
- Tạo CRUD APIs cho các bảng nghiệp vụ.
- Thêm JWT authentication/authorization.
- Tích hợp UI.
- Cân nhắc API Gateway khi cần một entry point chung.
- Cân nhắc RabbitMQ hoặc gRPC khi có nhu cầu giao tiếp liên service.

## Read-only Student Portal and management UI

The Student Portal is available at `/student/login` and uses MSSV-only access for demo/internal environments. It is not production-grade identity verification: anyone who knows a valid student code can impersonate that student. Restrict the deployment to a controlled network and apply rate limiting at the edge.

### Required configuration

Set the same JWT signing key (at least 32 UTF-8 bytes) for all four services. Do not commit the value:

```powershell
$env:StudentJwt__SigningKey = "replace-with-a-secret-of-at-least-32-bytes"
```

The issuer is `EducationSystem.IdentityService`, the audience is `EducationSystem.StudentPortal`, and the default access-token lifetime is 15 minutes (maximum 30). The initial student-code source is `Students.Nickname`. To select an existing alternative without changing the database:

```powershell
$env:StudentLogin__CodeSource = "Nickname" # Nickname | UserInternalId | UserName
```

Frontend environment variables:

```powershell
$env:VITE_IDENTITY_API_ORIGIN = "http://localhost:5001"
$env:VITE_ACADEMIC_API_ORIGIN = "http://localhost:5002"
$env:VITE_EXAM_API_ORIGIN = "http://localhost:5003"
$env:VITE_COMMUNICATION_API_ORIGIN = "http://localhost:5004"
$env:VITE_STUDENT_SESSION_STORAGE = "false" # optional demo reload continuity
```

Run the four services as listed above, then run the UI:

```powershell
cd education-system-ui
npm install
npm run dev
```

### Security and deployment boundary

- Student logout only clears the browser state. A copied JWT remains valid until its short expiry because no session, refresh token, or revocation record is persisted.
- In-memory token storage is the default. Optional `sessionStorage` improves demo reload continuity but retains XSS exposure; LocalStorage is never used.
- The public Student ingress must expose only `POST /api/auth/student/login` and `/api/student/me/*`. Use `deploy/nginx/student-portal.conf` as the allow-list reference.
- Direct service ports, `/api/management/*`, `/api/academic/*`, `/api/exam/*`, `/api/identity/*`, `/api/communication/*`, and `/api/internal/*` must remain internal/VPN-only. Management authentication and RBAC are out of scope.

### Immutable database and truthful metrics

No schema object, migration, seed, row, session, refresh token, or audit value is created or changed. All new queries use `AsNoTracking`, ownership predicates, DTO projection, and a runtime SELECT-only command interceptor. Verify the guardrails and optional live row counts with:

```powershell
powershell -File scripts/verify-read-only.ps1
powershell -File scripts/verify-read-only.ps1 -VerifyDatabase
```

The UI intentionally does not calculate GPA, pass/fail, earned or required credits, completion percentage, attendance rate, administrative class, curriculum version, or required/elective status. Raw result, combined result, attendance status, schedule type, and study status are shown without unconfirmed semantic labels.
