# Kiến trúc kỹ thuật hệ thống EducationSystem

> **Tên hệ thống:** EducationSystem — Hệ Thống Quản Lý Giáo Dục  
> **Vai trò tài liệu:** Nguồn sự thật kỹ thuật dùng cho phát triển, tích hợp, vận hành và rà soát kiến trúc  
> **Phạm vi:** Kiến trúc tổng thể, danh mục microservice, mô hình dữ liệu 47 bảng, phân quyền, giao diện, luồng CV AI và luồng nhập chuẩn đầu ra CLO/PLO  
> **Cập nhật theo mã nguồn:** 09/08/2026

## 1. Mục đích và nguyên tắc sử dụng tài liệu

Tài liệu này mô tả kiến trúc sản xuất của EducationSystem và là điểm khởi đầu bắt buộc cho thành viên mới. Khi có khác biệt, thứ tự ưu tiên xác minh là: mã nguồn đang triển khai, migration/cấu hình môi trường, tài liệu này, sau đó mới đến tài liệu phân tích cũ.

Các nguyên tắc xuyên suốt:

- Mỗi dịch vụ sở hữu một miền nghiệp vụ, một `DbContext` và một schema SQL riêng.
- Không tạo khóa ngoại vật lý xuyên schema; liên kết xuyên dịch vụ chỉ là định danh logic và phải được xác minh qua API hoặc logic ứng dụng.
- Frontend không được coi là biên bảo mật. Xác thực, phân quyền, quyền sở hữu dữ liệu và tính toàn vẹn phải được thực thi tại backend.
- Dữ liệu do LLM sinh ra không phải nguồn sự thật. Dữ liệu định danh và dữ kiện nghiệp vụ phải được khóa lại bằng bản ghi đã xác minh.
- Khóa API, JWT signing key và connection string không được commit hoặc đưa vào tài liệu.

## 2. Kiến trúc cốt lõi

### 2.1. Mô hình kiến trúc

EducationSystem áp dụng **modular microservices** với chiến lược **schema-per-service trên một SQL Server vật lý**. Năm dịch vụ .NET dùng chung database engine nhằm giảm chi phí vận hành nhưng tách biệt quyền sở hữu dữ liệu bằng năm schema: `identity`, `academic`, `exam`, `communication` và `career`.

Mô hình này đem lại:

- Ranh giới miền và `DbContext` rõ ràng; migration của một dịch vụ không trực tiếp quản lý bảng của dịch vụ khác.
- Khả năng tách schema thành database độc lập về sau mà không phải thiết kế lại toàn bộ miền.
- Tái sử dụng một SQL Server instance, connection pool và hạ tầng sao lưu trong giai đoạn hiện tại.
- Tránh kết dính dữ liệu bằng quy tắc **không có khóa ngoại xuyên schema**.

Đổi lại, tính nhất quán xuyên dịch vụ không thể dựa vào transaction SQL chung. Mỗi tích hợp phải chấp nhận lỗi mạng, timeout, dữ liệu đến trễ và triển khai kiểm tra tồn tại/ownership ở lớp ứng dụng.

### 2.2. Sơ đồ thành phần

```text
                         +-----------------------------+
                         | React 19 / TypeScript 6     |
                         | Vite 8 / Ant Design 6       |
                         | React Router 7 / Query 5    |
                         +---------------+-------------+
                                         |
                       HTTPS/JSON + JWT  |  (gọi trực tiếp từng service)
          +------------------------------+------------------------------+
          |              |               |              |               |
          v              v               v              v               v
   IdentityService AcademicService   ExamService CommunicationService CareerService
     :5001/:7001    :5002/:7002     :5003/:7003   :5004/:7004       :5005/:7005
          |              |               |              |               |
          +--------------+---------------+--------------+---------------+
                                         |
                         SQL Server vật lý dùng chung
              identity | academic | exam | communication | career
                                         |
                                         +---- không có FK xuyên schema

   CareerService --REST--> AcademicService :5002
        |             \--> VectorMatchService :5006 -- S-BERT --> FAISS
        \----------------> Gemini / Groq
```

Frontend hiện gọi trực tiếp từng service bằng URL cấu hình; repository chưa có API Gateway chung, message broker hay gRPC. `CareerService` đóng vai trò **BFF/Context Hydrator** riêng cho nghiệp vụ CV, không phải gateway toàn hệ thống.

### 2.3. Ngăn xếp công nghệ

| Lớp | Công nghệ | Vai trò |
| --- | --- | --- |
| Backend | .NET 8, ASP.NET Core Web API, C# | REST API, xác thực/phân quyền, orchestration nghiệp vụ |
| Truy cập dữ liệu | Entity Framework Core 8 | Mapping entity, LINQ, migration, transaction và optimistic concurrency |
| Cơ sở dữ liệu | Microsoft SQL Server | Một database engine vật lý, năm schema nghiệp vụ |
| Frontend | React 19, TypeScript 6, Vite 8 | SPA cho cổng sinh viên và cổng quản trị |
| Điều hướng/UI/data fetching | React Router 7, Ant Design 6, TanStack React Query 5, Axios | Route, component, cache trạng thái server và HTTP client |
| Vector matching | Python 3.10, FastAPI | API semantic matching độc lập tại cổng `5006` |
| Embedding | Sentence-BERT `multi-qa-MiniLM-L6-cos-v1` | Mã hóa JD và nội dung CLO thành dense vector cosine |
| Vector index | FAISS | Tìm kiếm top-K dense vector hiệu năng cao |
| LLM | Gemini (Gemini 3.5 Flash), Groq (Qwen_3.5_27B) | Trích xuất CLO/PLO và biên tập CV theo JSON nghiêm ngặt |

> **Trạng thái triển khai:** mã nguồn Python của `VectorMatchService` nằm tại `src/Services/VectorMatchService`; dịch vụ được tự động khởi chạy và tích hợp khi dùng Docker Compose tại cổng `5006`. `CareerService` có HTTP client và vẫn fallback theo điểm học phần khi dịch vụ vector không khả dụng.

### 2.4. Quy tắc sở hữu dữ liệu

| Quy tắc | Cách thực thi |
| --- | --- |
| Một schema, một chủ sở hữu | Chỉ service sở hữu được migration và ghi bảng trong schema đó |
| Không FK xuyên schema | ID ngoài miền chỉ là tham chiếu logic; xác minh bằng REST/logic ứng dụng |
| Không đọc chéo tùy tiện | Ngoại lệ VectorMatch chỉ đọc `academic` và `career`; không được ghi |
| Không transaction phân tán | Dùng idempotency, trạng thái quy trình, audit và retry có giới hạn |
| Contract trước implementation | Giao tiếp service qua DTO JSON được kiểm soát phiên bản |
| Source of truth theo miền | Identity sở hữu tài khoản; Academic sở hữu hồ sơ học tập; Career sở hữu CLO/PLO đã duyệt |

## 3. Danh bạ mạng microservice

| Dịch vụ | HTTP / HTTPS | Schema sở hữu | Trách nhiệm chính | Phụ thuộc chính |
| --- | --- | --- | --- | --- |
| **IdentityService** | `5001` / `7001` | `identity` | Xác thực, phát JWT, hồ sơ tài khoản, reset mật khẩu, audit, cài đặt và thiết bị đang hoạt động | SQL Server; AcademicService khi phân giải danh tính sinh viên |
| **AcademicService** | `5002` / `7002` | `academic` | Vòng đời sinh viên, khoa/ngành, môn/lớp học phần, phân công, lịch/phòng, điểm danh, đánh giá, kế hoạch/học phí, e-Portfolio và thực tập đã xác minh | SQL Server; IdentityService cho tham chiếu người dùng |
| **ExamService** | `5003` / `7003` | `exam` | Ngân hàng câu hỏi, bộ đề, kỳ thi, lượt thi, câu hỏi được phát, câu trả lời và tính kết quả | SQL Server; ID lớp/sinh viên là tham chiếu logic Academic |
| **CommunicationService** | `5004` / `7004` | `communication` | Mẫu biểu, yêu cầu biểu mẫu, luồng duyệt, thông báo và luồng xác minh thực tập | SQL Server; AcademicService cho đồng bộ thực tập đã duyệt |
| **CareerService** | `5005` / `7005` | `career` | BFF hydrate ngữ cảnh CV, tối ưu CV bằng LLM, nhập/kiểm duyệt/publish CLO-PLO từ DOCX | AcademicService, VectorMatchService, LLM, SQL Server |
| **VectorMatchService** | `5006` / không cấu hình | đọc `academic` + `career` | Mã hóa JD, truy vấn FAISS và trả các CLO/học phần phù hợp; chỉ đọc | Python/FastAPI, Sentence-BERT, FAISS |

Các port trên là cấu hình phát triển cục bộ. Môi trường triển khai nên dùng DNS/service discovery và HTTPS nội bộ thay vì hard-code `localhost`.

### 3.1. Hợp đồng giao tiếp và xử lý lỗi

- Giao tiếp đồng bộ dùng HTTP/JSON; timeout và `CancellationToken` phải được truyền xuyên chuỗi gọi.
- JWT sinh viên được phát bởi IdentityService và xác minh bằng signing key/issuer/audience nhất quán giữa các API liên quan.
- Lỗi nghiệp vụ dùng mã ổn định thay vì buộc client phân tích thông báo tự do.
- Vector matching là dependency có thể suy giảm: mất cổng `5006` phải trả `isFallbackMode: true` và xếp theo điểm, không làm mất toàn bộ context.
- Lỗi Academic context hoặc quyền sở hữu không hợp lệ là lỗi chặn; không được dùng dữ liệu của sinh viên khác làm fallback.

## 4. Vai trò người dùng và biên bảo mật

| Vai trò/Principal | Phạm vi | Quyền tiêu biểu | Kiểm soát bắt buộc |
| --- | --- | --- | --- |
| **Student** | Cổng sinh viên | Xem hồ sơ, chương trình, lịch, điểm danh, kết quả, học phí, biểu mẫu; tạo CV của chính mình | JWT policy sinh viên; `studentId` phải khớp claim; trả `404` khi truy cập tài nguyên không sở hữu |
| **Viewer** | Cổng quản trị, chỉ đọc | Xem dashboard và dữ liệu vận hành được cấp | API vẫn phải kiểm tra quyền; ẩn menu không phải phân quyền |
| **AcademicManager** | Quản trị học vụ | Quyền xem và chức năng học vụ/phân công theo chính sách | Kiểm tra role/policy tại API |
| **Administrator** | Quản trị chung | Tài khoản, cấu hình, danh mục và chức năng quản trị | Kiểm tra role/policy tại API; ghi audit |
| **SystemAdministrator** | API Career quản trị | Upload, xử lý, review, validate, approve/reject/archive CLO/PLO | Policy backend `SystemAdministrator`; môi trường production không dùng development bypass |
| **Service principal** | Gọi nội bộ | Hydrate context hoặc đồng bộ dữ liệu đã duyệt | Credential riêng, least privilege, timeout, audit và allow-list endpoint |

Thông tin PII như email, số điện thoại, địa chỉ và GitHub dùng để hiển thị CV được giữ ở frontend. Backend loại email, URL, số điện thoại, HTML/script và ký tự điều khiển khỏi nội dung đưa vào LLM. Không đưa secret, token hoặc connection string vào prompt hay log.

## 5. Bản thiết kế cơ sở dữ liệu — 47 bảng

### 5.1. Tổng hợp

| Schema | Service sở hữu | Số bảng | Quyền ghi |
| --- | --- | ---: | --- |
| `identity` | IdentityService | 5 | IdentityService |
| `academic` | AcademicService | 22 | AcademicService |
| `exam` | ExamService | 8 | ExamService |
| `communication` | CommunicationService | 3 | CommunicationService |
| `career` | CareerService | 9 | CareerService |
| **Tổng** |  | **47** |  |

Tên dưới đây ánh xạ theo EF Core entity/`DbSet`. “Quan hệ logic” mô tả hướng nghiệp vụ, không mặc nhiên khẳng định có FK vật lý xuyên schema.

### 5.2. Schema `identity` — 5 bảng

| # | Bảng | Chức năng/cấu trúc nghiệp vụ | Quan hệ chính |
| ---: | --- | --- | --- |
| 1 | `Users` | Tài khoản, credential đã băm, vai trò, trạng thái và hồ sơ nhận dạng | Gốc của `AuditLogs`, `PasswordResets`, `UserDevices`; ID được tham chiếu logic ở miền khác |
| 2 | `AuditLogs` | Dấu vết hành động: actor, loại thao tác, tài nguyên, thời điểm và metadata | Nhiều log thuộc một user/actor |
| 3 | `PasswordResets` | Token xác minh, hạn dùng, trạng thái sử dụng và yêu cầu đặt lại mật khẩu | Thuộc `Users`; token phải lưu/so sánh an toàn và dùng một lần |
| 4 | `Settings` | Cặp cấu hình hệ thống, giá trị, mô tả và trạng thái | Cấu hình toàn cục do quản trị viên quản lý |
| 5 | `UserDevices` | Thiết bị đăng nhập, thông tin phiên/push token, lần hoạt động và trạng thái | Nhiều thiết bị thuộc một `Users` |

### 5.3. Schema `academic` — 22 bảng

| # | Bảng | Chức năng/cấu trúc nghiệp vụ | Quan hệ chính |
| ---: | --- | --- | --- |
| 1 | `Students` | Hồ sơ nhập học, mã sinh viên, ngành, niên khóa, trạng thái cố vấn/học tập | Thuộc `Majors`/`AcademicYears`; gốc của đăng ký lớp, điểm, dự án, thực tập |
| 2 | `AcademicYears` | Danh mục năm/niên khóa, khoảng thời gian và trạng thái | Được sinh viên, kế hoạch và lớp học tham chiếu |
| 3 | `Majors` | Ngành/chương trình, mã ngành và thông số chương trình | Thuộc `Faculties`; có `Students`, `SemesterPlans` |
| 4 | `Faculties` | Khoa/đơn vị học thuật | Có `Majors` và liên kết giảng viên qua `TeacherFaculties` |
| 5 | `Subjects` | Danh mục học phần, mã, tên, tín chỉ và mô tả | Có lớp triển khai, kế hoạch học kỳ, tài liệu và CLO logic |
| 6 | `SubjectTeachings` | Lớp học phần/đợt giảng dạy theo học kỳ và môn | Nối `Subjects` với giảng viên, sinh viên, lịch và kỳ thi logic |
| 7 | `SubjectTeachingTeachers` | Bảng nối nhiều-nhiều giữa giảng viên và lớp học phần | Thuộc `SubjectTeachings`; user/teacher là ID logic |
| 8 | `SubjectStudents` | Danh sách sinh viên đăng ký mỗi lớp học phần | Nối `Students` và `SubjectTeachings` |
| 9 | `SubjectSchedules` | Ca/buổi học chi tiết, ngày giờ và phòng | Thuộc `SubjectTeachings`, tham chiếu `Rooms`; nguồn của điểm danh |
| 10 | `Rooms` | Phòng học, sức chứa, vị trí và trạng thái | Có nhiều `SubjectSchedules` |
| 11 | `Attendances` | Check-in/trạng thái có mặt, ghi chú và cảnh báo theo buổi | Nối sinh viên/lớp/lịch học |
| 12 | `SubjectDocuments` | Giáo trình, đề cương, slide, handout và metadata tệp | Thuộc `Subjects` hoặc ngữ cảnh lớp học phần |
| 13 | `SubjectSpecialNotes` | Ngoại lệ/ghi chú đặc biệt cho điểm danh hoặc vận hành học phần | Gắn với môn/lớp/sinh viên theo nghiệp vụ |
| 14 | `EvaluationCriterias` | Tiêu chí và trọng số đánh giá học phần | Thuộc môn/lớp; chi tiết điểm tham chiếu |
| 15 | `StudentEvaluations` | Điểm tổng hợp học phần, trạng thái đạt/rớt | Nối `Students` với môn/lớp; có nhiều chi tiết |
| 16 | `StudentEvaluationDetails` | Điểm thành phần theo từng tiêu chí | Thuộc `StudentEvaluations` và `EvaluationCriterias` |
| 17 | `SemesterPlans` | Kế hoạch đào tạo theo ngành/phiên bản và học kỳ | Thuộc `Majors`; có nhiều `SemesterSubjects` |
| 18 | `SemesterSubjects` | Môn trong kế hoạch học kỳ, cốt lõi/tự chọn và thứ tự | Nối `SemesterPlans` với `Subjects` |
| 19 | `SemesterTuitions` | Học phí kỳ, số tiền, hạn và trạng thái thanh toán | Thuộc `Students` và học kỳ/năm học |
| 20 | `TeacherFaculties` | Bảng nối giảng viên với khoa/đơn vị | Nối định danh giảng viên với `Faculties` |
| 21 | `StudentProjects` | Dự án e-Portfolio: tên, vai trò, công nghệ, mô tả, đóng góp, repository và môn liên kết | Thuộc `Students`; nguồn dữ kiện được khóa khi tạo CV |
| 22 | `StudentInternships` | Thực tập đã được trường xác minh: công ty, vị trí, nhiệm vụ, thời gian và trạng thái | Thuộc `Students`; được Communication đồng bộ sau duyệt và Career đọc cho CV |

### 5.4. Schema `exam` — 8 bảng

| # | Bảng | Chức năng/cấu trúc nghiệp vụ | Quan hệ chính |
| ---: | --- | --- | --- |
| 1 | `Questions` | Ngân hàng câu hỏi trắc nghiệm/tự luận, nội dung, loại và độ khó | Có `QuestionAnswers`; được gom vào bộ đề |
| 2 | `QuestionAnswers` | Phương án trả lời/đáp án khóa và cờ đúng | Thuộc `Questions` |
| 3 | `QuestionSuites` | Gói/bộ câu hỏi phục vụ tạo đề | Gom các câu hỏi theo cấu hình nghiệp vụ |
| 4 | `SubjectTeachingExams` | Kỳ thi gắn lớp học phần, lịch, thời lượng và cấu hình | `SubjectTeachingId` là tham chiếu logic Academic; có lượt thi |
| 5 | `ExamAttempts` | Phiên làm bài của sinh viên, thời gian và trạng thái nộp | Thuộc kỳ thi; có selections, answers và result |
| 6 | `ExamResults` | Điểm, trạng thái chấm và phản hồi | Thường tương ứng một `ExamAttempts` |
| 7 | `ExamQuestionSelections` | Snapshot các câu hỏi được phát trong một lượt thi | Thuộc `ExamAttempts`, tham chiếu câu hỏi/bộ đề |
| 8 | `ExamQuestionAnswers` | Câu trả lời sinh viên nộp cho từng câu | Thuộc `ExamAttempts` và câu hỏi được phát |

### 5.5. Schema `communication` — 3 bảng

| # | Bảng | Chức năng/cấu trúc nghiệp vụ | Quan hệ chính |
| ---: | --- | --- | --- |
| 1 | `FormTemplates` | Mẫu biểu động, schema trường nhập, phiên bản và trạng thái | Được `FormRequests` lựa chọn |
| 2 | `FormRequests` | Đơn/yêu cầu, dữ liệu validate, trạng thái và nhật ký chuỗi duyệt | Thuộc template và người yêu cầu; có thể kích hoạt đồng bộ thực tập |
| 3 | `UserAnnouncements` | Thông báo hệ thống, đối tượng nhận, deep-link, lịch phát và trạng thái đọc/phát | Tham chiếu user bằng ID logic |

### 5.6. Schema `career` — 9 bảng

| # | Bảng | Chức năng/cấu trúc nghiệp vụ | Quan hệ chính |
| ---: | --- | --- | --- |
| 1 | `CurriculumVersions` | Snapshot phiên bản chương trình đào tạo, trạng thái hiệu lực | Gốc của PLO, CLO và tài liệu công bố |
| 2 | `ProgramLearningOutcomes` | PLO đã kiểm duyệt: mã, mô tả và thứ tự | Thuộc `CurriculumVersions`; đích của mapping CLO-PLO |
| 3 | `CourseLearningOutcomes` | CLO đã kiểm duyệt kèm mã học phần/snapshot | Thuộc phiên bản; nguồn của vector matching và mapping |
| 4 | `ProgressionLevels` | Danh mục mức E/R/D (Giới thiệu/Củng cố/Thành thạo) | Được `CloPloMappings` tham chiếu |
| 5 | `CloPloMappings` | Liên kết CLO–PLO và mức tiến triển E/R/D | Nối CLO, PLO và `ProgressionLevels` |
| 6 | `OutcomeImportBatches` | Batch staging: file/hash, trạng thái, raw/reviewed JSON, validation, `QualityReportJson`, actor, lỗi và rowversion | Gốc quy trình import; có blocks và review logs |
| 7 | `OutcomeDocumentBlocks` | Block OpenXML theo heading/paragraph/table, thứ tự và tọa độ trích dẫn | Thuộc batch; cho phép truy ngược kết quả AI về nguồn |
| 8 | `OutcomeReviewLogs` | Audit lịch sử sửa, validate, approve, reject, archive | Thuộc batch; ghi actor, action và thay đổi |
| 9 | `ApprovedOutcomeJsonDocuments` | Canonical JSON bất biến, phiên bản schema/document, SHA-256, provenance và trạng thái active/superseded | Sinh sau publication từ dữ liệu quan hệ chính thức |

### 5.7. Quan hệ nghiệp vụ tổng quát

```text
Faculty -> Major -> SemesterPlan -> SemesterSubject -> Subject
                           Student -> SubjectStudent <- SubjectTeaching <- Subject
                                                         |
                    TeacherFaculty -> SubjectTeachingTeacher
                                                         |
                     Room <- SubjectSchedule -> Attendance <- Student
                                                         |
Student + Subject -> StudentEvaluation -> StudentEvaluationDetail <- EvaluationCriteria
Student -> StudentProject
Student -> StudentInternship

QuestionSuite -> Question -> QuestionAnswer
SubjectTeachingExam -> ExamAttempt -> ExamQuestionSelection
                                \--> ExamQuestionAnswer
                                \--> ExamResult

CurriculumVersion -> ProgramLearningOutcome
CurriculumVersion -> CourseLearningOutcome
CourseLearningOutcome + ProgramLearningOutcome + ProgressionLevel -> CloPloMapping
OutcomeImportBatch -> OutcomeDocumentBlock
OutcomeImportBatch -> OutcomeReviewLog
OutcomeImportBatch --approve--> các bảng outcome chính thức -> ApprovedOutcomeJsonDocument
```

## 6. Luồng chuyên sâu A — Tối ưu CV thông minh theo thời gian thực

### 6.1. Mục tiêu và API

Luồng tạo CV biên tập nội dung tiếng Việt thân thiện ATS dựa trên dữ liệu đã xác minh. AI được phép cải thiện diễn đạt theo STAR/XYZ và động từ hành động chuyên nghiệp, nhưng không được phát minh tên, điểm, công ty, thời gian, công nghệ hay thành tích.

```http
POST http://localhost:5005/api/career/resume/optimize
Authorization: Bearer <student-jwt>
Content-Type: application/json
```

Request `PrepareResumePayloadRequestDto` chứa các tham số như `studentId`, `targetRole`, `jobDescription`, `careerFocusTag`, `selectedSubjectIds`, `selectedInternshipIds`, dự án UI đã chọn, chứng chỉ, hoạt động/giải thưởng, `currentSummaryDraft`, `topK` và `similarityThreshold`. Email, điện thoại, địa chỉ và GitHub chỉ tồn tại ở frontend để render, không nằm trong payload gửi LLM.

### 6.2. Trình tự xử lý

```text
React            ResumeOptimizationController   Context Hydrator     Academic :5002
  | POST optimize          |                           |                    |
  |----------------------->| kiểm JWT/ownership       |                    |
  |                        |-------------------------->|---- hydrate ------>|
  |                        |                           |<-- verified data ---|
  |                        |                           |
  |                        |                           |--> Vector :5006 --> FAISS
  |                        |                           |<-- top-K/fallback --|
  |                        |<---- sanitized context ---|
  |                        |---- strict JSON prompt ------------------> LLM
  |                        |<--- generated JSON ------------------------|
  |                        | fact guard + metrics     |
  |<-----------------------| verified JSON response  |
  | render + ghép PII frontend-only                   |
```

#### Bước 1 — Frontend tạo yêu cầu

React thu thập vai trò mục tiêu, JD, học phần, dự án, thực tập, chứng chỉ, hoạt động và bản tóm tắt hiện tại. Chỉ mục đã được người dùng chọn mới được gửi. Mảng rỗng thể hiện chủ ý “không chọn”; `null` mới cho phép logic fallback tương ứng. JWT sinh viên đi trong `Authorization` header.

#### Bước 2 — Controller xác thực biên

`ResumeOptimizationController` tại route `api/career/resume` yêu cầu Bearer authentication và student policy. `studentId` phải khớp claim; sai ownership không được tiết lộ tài nguyên người khác. Nếu không tìm thấy context hợp lệ, API trả mã **`RESUME_CONTEXT_NOT_FOUND`**.

#### Bước 3 — Hydrate context song song

`ResumeContextHydrationService` điều phối hai nguồn:

1. `AcademicService :5002` trả hồ sơ sinh viên, ngành/khoa, GPA, học phần đạt điều kiện, e-Portfolio và thực tập đã duyệt.
2. `VectorMatchService :5006` mã hóa JD bằng `multi-qa-MiniLM-L6-cos-v1`, truy vấn FAISS trên CLO của các học phần có điểm trung bình **từ 7,0 trở lên**, rồi trả top-K outcome/học phần theo cosine similarity.

Kết quả vector phải được giao với tập học phần thực sự thuộc sinh viên. Khi VectorMatch timeout, từ chối kết nối hoặc lỗi downstream, service ghi warning, đặt `isFallbackMode: true` và xếp hạng theo điểm; đây không phải lỗi 502 bắt buộc.

#### Bước 4 — Làm sạch và sinh bằng LLM

`LlmResumeGeneratorService` xây prompt từ context đã hydrate. Trước khi gọi Gemini (Gemini 3.5 Flash), Groq (Qwen_3.5_27B) hoặc provider tương thích cấu hình, service:

- loại HTML, script/active content, ký tự điều khiển, email, URL và số điện thoại;
- coi JD/nội dung người dùng là dữ liệu không tin cậy, không phải system instruction;
- không serialize `StudentId`/`StudentCode` vào prompt;
- giới hạn kích thước đầu vào, timeout và retry;
- yêu cầu **Strict JSON Mode**, CV tiếng Việt, bullet STAR/XYZ và không thêm dữ kiện.

#### Bước 5 — Khóa dữ kiện phía server

Sau khi parse JSON, API không chuyển thẳng kết quả LLM cho client. Fact guard force-overwrite các trường cấu trúc bằng nguồn DB đã xác minh:

- `FullName`, `StudentCode`, `MajorName`, GPA;
- tên dự án, vai trò và technology stack;
- tên công ty, vị trí, ngày bắt đầu/kết thúc thực tập;
- tên/mã học phần và dữ kiện học thuật.

LLM chỉ sở hữu cách diễn đạt. Metadata lock ngăn hallucination ngay cả khi model trả JSON hợp lệ về cú pháp.

#### Bước 6 — Tính chất lượng độc lập

`ResumeMetricsEvaluator` tính lại phía server:

- **`JobAlignmentScore`**: mức token overlap giữa CV sau khóa dữ kiện và JD.
- **`ContentPreservationScore`**: mức token overlap giữa CV và dữ liệu nguồn DB/người dùng đã chọn.
- **Hallucination warning**: bật khi alignment cao nhưng preservation thấp, biểu hiện nội dung có vẻ hợp JD nhưng xa nguồn sự thật.

Điểm do model tự khai báo không được tin cậy. Metrics chỉ là tín hiệu chất lượng, không thay thế fact guard.

#### Bước 7 — Render frontend

API trả JSON đã xác minh. React ghép PII frontend-only, render bố cục CV gọn, các hàng văn bản và danh sách key-value có bullet riêng; tránh badge dày đặc hoặc dạng “đặt hàng” gây khó đọc. Bản nháp wizard trong phiên được giữ ở `sessionStorage`; không được hiểu là CV đã lưu lâu dài trên server.

### 6.3. Mã lỗi CV đáng chú ý

| Mã | Ý nghĩa | Xử lý client/vận hành |
| --- | --- | --- |
| `RESUME_CONTEXT_NOT_FOUND` | Không có context hợp lệ, sai ownership hoặc nguồn Academic không trả dữ liệu | Không retry mù; kiểm tra JWT, student ID và dữ liệu học vụ |
| `RESUME_LLM_NOT_CONFIGURED` | Provider/model/key chưa cấu hình | Chặn optimize; sửa secret/config môi trường |
| `RESUME_LLM_INPUT_TOO_LARGE` | Prompt vượt giới hạn | Giảm JD/nội dung đã chọn; không gửi lại nguyên payload |
| `RESUME_LLM_RATE_LIMIT` | Provider giới hạn tần suất | Backoff có jitter và thông báo người dùng thử lại |
| `RESUME_LLM_REQUEST_FAILED` | Lỗi HTTP/timeout/provider | Retry hữu hạn; correlation ID và log không chứa PII |
| `RESUME_LLM_INVALID_RESPONSE` | Phản hồi không parse được theo contract JSON | Không render dữ liệu chưa khóa; ghi diagnostic đã làm sạch |
| `RESUME_LLM_PROVIDER_UNSUPPORTED` | Tên provider không được implementation hỗ trợ | Sửa cấu hình triển khai |

### 6.4. Bất biến bảo mật của CV

- Không tin `studentId` từ body nếu không đối chiếu claim.
- Không gửi PII/secret vào LLM; không log prompt đầy đủ ở production.
- Không nhận tên dự án, công ty, ngày tháng hoặc điểm từ LLM làm source of truth.
- Chỉ internship thuộc sinh viên và đã được nguồn học vụ chấp nhận mới được hydrate.
- Không để lỗi VectorMatch xóa danh sách học phần đủ điều kiện.
- Không trả nội dung AI khi parse/fact guard thất bại.

## 7. Luồng chuyên sâu B — Trích xuất và phê duyệt CLO/PLO từ DOCX

### 7.1. Mục tiêu và biên API

Quy trình biến tài liệu chuẩn chương trình DOCX thành dữ liệu quan hệ đã kiểm duyệt và canonical JSON có thể truy vết. Base route là `/api/management`, toàn bộ endpoint yêu cầu policy **`SystemAdministrator`**.

Upload bắt đầu tại:

```http
POST http://localhost:5005/api/management/outcome-imports
Content-Type: multipart/form-data
```

Các endpoint vòng đời gồm list/detail, `process`, `review`, `validate`, `approve`, `reject`, `archive` và đọc `approved-document`.

### 7.2. Máy trạng thái khái niệm

```text
Uploaded -> Queued -> Processing -> Extracted -> UnderReview -> Validated -> Approved
                         |               |             |             |
                         +-> Failed      +-> Rejected  +-> Rejected  +-> Archived/Superseded
```

Tên trạng thái vật lý phải theo enum/contract hiện hành; sơ đồ thể hiện ý nghĩa vòng đời và các nhánh kiểm soát.

### 7.3. Sáu giai đoạn xử lý

#### Giai đoạn 1 — Upload và xác minh tệp

Quản trị viên tải DOCX tại màn hình `/management/system/outcomes/import`. Backend kiểm MIME/phần mở rộng, giới hạn kích thước và cấu trúc ZIP. Tệp được lưu ngoài web root; DB giữ storage key, metadata và SHA-256 thay vì coi tên file do client gửi là đường dẫn tin cậy.

`DocxFileValidator` mở container bằng `ZipArchive`, giới hạn tổng số entry (mặc định `MaxZipEntries = 5000`) cùng các ngưỡng kích thước/expansion để giảm nguy cơ path traversal, malformed archive và ZIP bomb.

#### Giai đoạn 2 — Parse và chia block OpenXML

`OutcomeImportWorker` là hosted background service nhận batch đang chờ. `OpenXmlOutcomeDocumentParser` bảo toàn thứ tự tài liệu và chuyển heading, paragraph, table thành `OutcomeDocumentBlocks`. Mỗi block mang định danh/vị trí đủ để UI và reviewer truy ngược kết quả trích xuất về nguồn.

Worker giúp request upload kết thúc nhanh và cô lập tác vụ LLM dài. Trạng thái, lỗi và thời điểm phải được ghi vào `OutcomeImportBatches` để quy trình có thể quan sát và phục hồi.

#### Giai đoạn 3 — Trích xuất LLM theo ba nhiệm vụ cô lập

Gemini hoặc Groq được gọi theo ba contract độc lập:

1. Metadata chương trình và danh sách Program Learning Outcomes.
2. Mã học phần và Course Learning Outcomes.
3. Ma trận CLO-to-PLO kèm mức tiến triển E/R/D.

Chia task làm giảm context không liên quan và cho phép validate từng phần. Kết quả raw/staging chưa phải dữ liệu chính thức, kể cả khi JSON hợp lệ.

#### Giai đoạn 4 — Reconciliation và validation xác định

`OutcomeReconciliationService`, `OutcomeValidationService` và quality evaluator:

- chuẩn hóa mã học phần, mã CLO/PLO và mức E/R/D;
- đối chiếu mã học phần với AcademicService/database Academic theo hợp đồng tích hợp;
- phát hiện duplicate, tham chiếu thiếu, mapping mồ côi và cấu trúc ma trận không nhất quán;
- phân loại **blocking errors** và **warnings**;
- ghi báo cáo vào `QualityReportJson` và contract validation.

Blocking error ngăn approve. Warning phải hiển thị để reviewer quyết định nhưng không mặc nhiên chặn publication nếu policy cho phép.

#### Giai đoạn 5 — Không gian review của quản trị viên

React hiển thị dữ liệu trích xuất cạnh block nguồn/trích dẫn. Reviewer có thể sửa inline PLO, CLO, mã học phần và mapping. Mọi ghi sửa dùng optimistic concurrency thông qua **rowversion**; request mang phiên bản cũ phải bị từ chối thay vì ghi đè thay đổi mới.

`OutcomeReviewLogs` lưu actor, hành động, thời điểm và dấu vết thay đổi. UI phải refresh dữ liệu khi gặp concurrency conflict và yêu cầu người dùng hòa giải, không tự động ghi đè.

#### Giai đoạn 6 — Publication nguyên tử và provenance

Approve chạy trong transaction có isolation **Serializable**:

1. Kiểm tra lại rowversion, trạng thái và không còn blocking error.
2. Ghi `CurriculumVersions`, PLO, CLO, progression/mapping vào bảng quan hệ chính thức.
3. Đọc lại dữ liệu chính thức để dựng canonical JSON; không copy trực tiếp `ReviewedJson`.
4. Chuẩn hóa serialization ổn định và tính SHA-256 lowercase trên UTF-8 `ContentJson`.
5. Ghi `ApprovedOutcomeJsonDocuments` với schema version, document version, provenance và trạng thái active.
6. Commit toàn bộ hoặc rollback toàn bộ.

Canonical JSON là snapshot bất biến. Khi phát hành phiên bản mới, phiên bản cũ được đánh dấu superseded/archived theo chính sách, không sửa âm thầm nội dung đã ký hash.

### 7.4. Kiểm soát an toàn và khả năng phục hồi

| Rủi ro | Kiểm soát |
| --- | --- |
| ZIP bomb hoặc archive độc hại | Giới hạn entry/kích thước/expansion, parser an toàn, lưu ngoài web root |
| Prompt injection trong DOCX | Tài liệu là dữ liệu không tin cậy; system instruction và JSON schema do server kiểm soát |
| AI trích xuất sai | Validation xác định, đối chiếu Academic, reviewer và citation về block nguồn |
| Hai admin sửa đồng thời | `rowversion`, optimistic concurrency và review log |
| Publish dở dang | Transaction `Serializable`, rollback toàn bộ |
| Canonical JSON bị sửa | SHA-256, version/provenance và tính bất biến |
| Worker bị dừng | Trạng thái batch bền vững, retry/idempotency theo phase |

## 8. Danh mục màn hình UI

### 8.1. Các route nghiệp vụ được yêu cầu

| Route | Màn hình/chức năng | Trạng thái điều hướng hiện tại |
| --- | --- | --- |
| `/academic/students` | Danh sách sinh viên, lọc và trạng thái cố vấn | Redirect sang `/management/people/students`, giữ query string |
| `/academic/subjects` | Danh mục học phần | Redirect sang `/management/education/subjects` |
| `/academic/subject-teachings` | Lớp học phần, phân công và timeline | Redirect sang `/management/teaching/classes` |
| `/academic/subject-students` | Danh sách lớp/ánh xạ sinh viên | Redirect sang `/management/teaching/assignments` |
| `/academic/subject-schedules` | Lịch đại học chi tiết | Redirect sang `/management/teaching/schedule` |
| `/academic/attendances` | Điểm danh thời gian thực và cảnh báo | Redirect sang `/management/teaching/attendance` |
| `/exam/exam-results` | Tìm điểm và nhật ký lượt thi | Redirect sang `/management/assessment/results` |
| `/exam/questions` | Ngân hàng/bộ câu hỏi trắc nghiệm và tự luận | Redirect sang `/management/assessment/question-suites` |
| `/identity/users` | Quản lý tài khoản, role và audit | Redirect sang `/management/people/users` |
| `/communication/form-requests` | Yêu cầu biểu mẫu của sinh viên | Route cổng sinh viên dùng `StudentFormRequestsPage`; quản trị dùng `/management/forms/requests` |
| `/management/system/outcomes/import` | Upload/khởi tạo batch CLO-PLO | Route hiện hành, chỉ dành quyền quản trị phù hợp |

### 8.2. Route quản trị hiện hành liên quan

| Nhóm | Route chính |
| --- | --- |
| Tổng quan | `/management/overview` |
| Cơ cấu/kế hoạch/môn học | `/management/education/structure`, `/management/education/plans`, `/management/education/subjects` |
| Con người | `/management/people/students`, `/management/people/teachers`, `/management/people/users` |
| Giảng dạy | `/management/teaching/classes`, `/management/teaching/assignments`, `/management/teaching/schedule`, `/management/teaching/attendance` |
| Đánh giá | `/management/assessment/results`, `/management/assessment/question-suites`, `/management/assessment/evaluations` |
| Truyền thông/biểu mẫu | `/management/communication/announcements`, `/management/forms/requests` |
| Hệ thống/CLO-PLO | `/management/system`, `/management/system/outcomes`, `/management/system/outcomes/import`, `/management/system/outcomes/:id/review` |

### 8.3. Cổng sinh viên liên quan đến kiến trúc

| Route | Chức năng |
| --- | --- |
| `/student/login` | Đăng nhập và nhận JWT sinh viên |
| `/student/dashboard` | Tổng quan cá nhân |
| `/student/profile`, `/student/program` | Hồ sơ và chương trình đào tạo |
| `/student/subjects`, `/student/schedule` | Học phần và thời khóa biểu |
| `/student/attendance`, `/student/evaluations`, `/student/exam-results` | Điểm danh, đánh giá và kết quả thi |
| `/student/documents`, `/student/announcements` | Tài liệu và thông báo |
| `/student/tuition`, `/student/form-requests` | Học phí và yêu cầu biểu mẫu |
| `/student/resume-builder` | Wizard CV AI hiện hành |
| `/student/resume-builder-old` | Màn hình CV cũ, chỉ giữ để tham chiếu |

## 9. API và quy ước vận hành quan trọng

### 9.1. Endpoint Career trọng yếu

| Method | Endpoint | Policy | Ý nghĩa |
| --- | --- | --- | --- |
| `POST` | `/api/career/resume/prepare-context` | Student JWT | Hydrate và xếp hạng context trước khi tạo CV |
| `POST` | `/api/career/resume/optimize` | Student JWT | Hydrate lại, gọi LLM, fact guard và tính metrics |
| `GET` | `/api/management/curriculum-versions` | `SystemAdministrator` | Liệt kê phiên bản chương trình |
| `POST` | `/api/management/curriculum-versions` | `SystemAdministrator` | Tạo metadata phiên bản |
| `GET/POST` | `/api/management/outcome-imports` | `SystemAdministrator` | Danh sách/tạo batch import |
| `POST` | `/api/management/outcome-imports/{id}/process` | `SystemAdministrator` | Đưa batch vào xử lý |
| `GET` | `/api/management/outcome-imports/{id}/review` | `SystemAdministrator` | Lấy workspace review và citation |
| `POST` | `/api/management/outcome-imports/{id}/validate` | `SystemAdministrator` | Chạy lại validation với rowversion |
| `POST` | `/api/management/outcome-imports/{id}/approve` | `SystemAdministrator` | Publication nguyên tử |
| `POST` | `/api/management/outcome-imports/{id}/reject` | `SystemAdministrator` | Từ chối batch |
| `POST` | `/api/management/outcome-imports/{id}/archive` | `SystemAdministrator` | Lưu trữ batch/tài liệu theo policy |

### 9.2. Quan sát hệ thống

Mỗi request xuyên service nên mang correlation ID và log có cấu trúc gồm service, endpoint, status, duration và downstream dependency. Tuyệt đối không log JWT, API key, prompt chứa PII hoặc toàn bộ tài liệu người dùng ở production.

Các tín hiệu cần giám sát:

- tỷ lệ `401/403/404` theo endpoint và lỗi ownership;
- latency/error rate của Academic, VectorMatch và LLM;
- tỷ lệ VectorMatch fallback và phân phối similarity;
- `RESUME_LLM_RATE_LIMIT`, invalid JSON và hallucination warning;
- độ dài hàng đợi, thời gian mỗi phase và số retry của `OutcomeImportWorker`;
- concurrency conflict, blocking validation và rollback khi approve;
- hash mismatch hoặc nhiều canonical document active ngoài dự kiến.

### 9.3. Cấu hình và bí mật

- Dùng user secrets, secret manager hoặc biến môi trường cho API key/connection string/JWT signing key.
- Cấu hình URL downstream theo môi trường; không hard-code URL production.
- Đặt timeout riêng cho HTTP client; retry chỉ với lỗi tạm thời và operation an toàn/idempotent.
- CORS chỉ cho phép origin đã biết; HTTPS bắt buộc ngoài local.
- Development authentication bypass của Career management không được bật ngoài Development.

## 10. Quy tắc mở rộng và checklist cho lập trình viên mới

### 10.1. Khi thêm dữ liệu hoặc chức năng

1. Xác định đúng bounded context và service sở hữu.
2. Thêm entity/`DbSet`/migration chỉ trong schema đó.
3. Không tạo FK sang schema khác; thêm client contract và validation logic nếu cần.
4. Tạo DTO riêng, không trả thẳng EF entity.
5. Áp dụng authentication, authorization và ownership ở controller/application layer.
6. Thêm pagination/filter cho danh sách; truyền cancellation và giới hạn payload.
7. Viết test cho happy path, forbidden ownership, lỗi downstream và concurrency.
8. Cập nhật tài liệu này khi bảng, endpoint, route, port hoặc invariant thay đổi.

### 10.2. Definition of Done kiến trúc

- Build backend/frontend thành công và migration được review.
- Không có secret trong source, log, fixture hoặc tài liệu.
- Không phát sinh cross-schema FK hay truy cập ghi trái quyền sở hữu.
- API trả mã lỗi ổn định và không lộ stack trace/PII.
- Các fallback không làm sai ownership hoặc biến dữ liệu chưa xác minh thành sự thật.
- Luồng LLM có sanitize, schema validation, fact guard và metrics phía server.
- Luồng import có citation, deterministic validation, rowversion, audit và publication nguyên tử.
- Route cũ nếu còn hỗ trợ phải redirect rõ ràng và giữ query string cần thiết.

## 11. Các quyết định kiến trúc cần ghi nhớ

| Quyết định | Lý do | Hệ quả |
| --- | --- | --- |
| Một SQL Server, schema-per-service | Tối ưu chi phí nhưng vẫn giữ bounded context | Phải cấm FK/truy vấn ghi xuyên schema |
| REST đồng bộ giữa service | Đơn giản, phù hợp quy mô hiện tại | Phải quản lý timeout, retry, partial failure |
| CareerService làm Context Hydrator | Gom context đáng tin trước khi gọi AI | Không biến Career thành gateway chung |
| VectorMatch là dependency suy giảm được | CV vẫn hữu dụng khi FAISS/Python ngừng | Luôn duy trì score fallback và cảnh báo |
| AI chỉ biên tập, server khóa metadata | Ngăn hallucination gây sai hồ sơ | Cần contract JSON và force-overwrite sau generation |
| CLO/PLO qua staging và human review | Tài liệu/AI có thể mơ hồ | Publication chỉ sau validation và approve |
| Canonical JSON dựng từ bảng chính thức | Tránh ký hash trên staging chưa chuẩn | Cần transaction Serializable và deterministic serialization |

---

Tài liệu này phải được cập nhật trong cùng thay đổi khi kiến trúc, schema, endpoint, role, route hoặc workflow AI thay đổi. Mọi ngoại lệ đối với ranh giới schema, fact guard hay quy trình publication phải có quyết định kiến trúc được phê duyệt trước khi triển khai.
