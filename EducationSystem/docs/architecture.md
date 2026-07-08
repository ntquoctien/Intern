# Architecture

## Tổng quan

Education Management System được khởi tạo theo hướng Modular Microservices với chiến lược Schema-per-Service. Mỗi service là một ASP.NET Core Web API project độc lập, có base route, port và boundary nghiệp vụ riêng.

Phase hiện tại vẫn là skeleton, nhưng đã có nền tảng persistence bằng EF Core cho từng service, cùng migration đầu tay cho từng schema. Messaging, gateway, authentication và UI vẫn chưa được triển khai.

## Schema-per-Service

Schema-per-Service nghĩa là mỗi service sẽ sở hữu một schema database riêng trong cùng hệ quản trị cơ sở dữ liệu hoặc trong hạ tầng database phù hợp ở phase sau.

Mục tiêu:

- Giữ boundary dữ liệu rõ ràng theo service.
- Tránh để một service phụ thuộc trực tiếp vào bảng thuộc service khác.
- Dễ scaffold DB First theo từng schema.
- Dễ mở rộng sang microservice độc lập hơn khi dự án lớn lên.

Trong phase này, schema đã được gắn vào từng service bằng `DbContext` riêng. Mỗi service dùng một default schema khác nhau trong cùng SQL Server.

Connection string không nằm trong repo. Dev có thể dùng user secrets hoặc biến môi trường `ConnectionStrings__SqlServer` để cấp giá trị cho từng service.

## Service boundary

### IdentityService

- Base route: `/api/identity`
- Schema tương lai: `identity`
- Quản lý tương lai: Users, UserDevices, PasswordResets, AuditLogs, Settings

IdentityService chịu trách nhiệm cho nền tảng người dùng, thiết bị đăng nhập, đặt lại mật khẩu, audit và cấu hình.

### AcademicService

- Base route: `/api/academic`
- Schema tương lai: `academic`
- Quản lý tương lai: Faculties, Majors, AcademicYears, Rooms, Students, TeacherFaculties, Subjects, SubjectDocuments, SubjectTeachings, SubjectTeachingTeachers, SubjectStudents, SubjectSchedules, SubjectSpecialNotes, Attendances, SemesterPlans, SemesterSubjects, SemesterTuitions, EvaluationCriterias, StudentEvaluations, StudentEvaluationDetails

AcademicService chịu trách nhiệm cho dữ liệu học vụ, sinh viên, môn học, lịch học, điểm danh, học kỳ, học phí và đánh giá.

### ExamService

- Base route: `/api/exam`
- Schema tương lai: `exam`
- Quản lý tương lai: QuestionSuites, Questions, QuestionAnswers, SubjectTeachingExams, ExamAttempts, ExamQuestionSelections, ExamQuestionAnswers, ExamResults

ExamService chịu trách nhiệm cho ngân hàng câu hỏi, đề thi, lượt làm bài, câu trả lời và kết quả thi.

### CommunicationService

- Base route: `/api/communication`
- Schema tương lai: `communication`
- Quản lý tương lai: FormTemplates, FormRequests, UserAnnouncements

CommunicationService chịu trách nhiệm cho biểu mẫu, yêu cầu và thông báo người dùng.

## Vì sao chưa dùng Database, RabbitMQ, gRPC, API Gateway

Database chưa được thêm vì phase hiện tại chỉ cần skeleton. Khi bước sang phase CRUD, database sẽ được thiết kế hoặc scaffold theo DB First.

RabbitMQ chưa được thêm vì chưa có luồng nghiệp vụ bất đồng bộ giữa các service.

gRPC chưa được thêm vì chưa có yêu cầu giao tiếp service-to-service hiệu năng cao hoặc contract nội bộ ổn định.

API Gateway chưa được thêm vì hiện tại mỗi service có thể chạy độc lập và Swagger riêng. Gateway nên được thêm khi cần một entry point chung, routing tập trung, authentication tập trung hoặc rate limiting.

## SharedKernel

`SharedKernel` là class library dùng cho các contract chung, ổn định và không phụ thuộc nghiệp vụ riêng của service nào.

Hiện tại `SharedKernel` có:

- `ApiResponse<T>`: format response chuẩn.
- `PagedResult<T>`: format phân trang chuẩn cho tương lai.

Không đưa entity, DbContext, repository hoặc service-specific logic vào `SharedKernel`.

## Health endpoint

Mỗi service có endpoint `/health` để xác nhận service đang chạy.

Ví dụ:

```http
GET /api/exam/health
```

Response có `success`, `message` và `data` gồm service name, status và UTC timestamp.

## Service Info endpoint

Mỗi service có endpoint `/info` để mô tả metadata hiện tại:

- Tên service.
- Schema tương lai.
- Domain.
- Trạng thái các tích hợp đang tắt.
- Danh sách trách nhiệm/bảng tương lai.

Endpoint này giúp kiểm tra boundary của từng service ngay cả khi chưa có database hoặc CRUD.
