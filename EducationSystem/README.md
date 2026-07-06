# Education Management System

Backend skeleton cho dự án Education Management System theo kiến trúc Schema-per-Service / Modular Microservices.

Phase hiện tại chỉ tạo bộ khung ban đầu để chuẩn bị cho các phase backend CRUD và UI sau này. Dự án chưa kết nối database, chưa có DTO, chưa có Entity, chưa có DbContext, chưa có Repository, chưa có Migration và chưa có CRUD API cho các bảng nghiệp vụ.

## Công nghệ

- .NET 8
- ASP.NET Core Web API
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

- Chưa có database.
- Chưa có DTO.
- Chưa có Entity.
- Chưa có DbContext.
- Chưa có Repository.
- Chưa có Migration.
- Chưa có CRUD API thật cho 37 bảng.
- Chưa có RabbitMQ.
- Chưa có gRPC.
- Chưa có API Gateway.
- Chưa có Docker.
- Chưa có UI.

## Future scope

- Kết nối database theo hướng DB First.
- Scaffold EF Core theo từng schema/service.
- Tạo DTOs, Entities, DbContext, Repository nếu cần.
- Tạo CRUD APIs cho các bảng nghiệp vụ.
- Thêm JWT authentication/authorization.
- Tích hợp UI.
- Cân nhắc API Gateway khi cần một entry point chung.
- Cân nhắc RabbitMQ hoặc gRPC khi có nhu cầu giao tiếp liên service.
