# Mô tả hệ thống EducationSystem

> Tài liệu được đối chiếu với mã nguồn hiện tại ngày 04/08/2026. Khi tài liệu và mã nguồn khác nhau, mã nguồn, migration và cấu hình môi trường triển khai là nguồn sự thật cuối cùng.

## 1. Tổng quan

EducationSystem là hệ thống quản lý giáo dục theo kiến trúc modular microservices và schema-per-service. Hệ thống gồm cổng sinh viên, cổng quản trị, các API nghiệp vụ học vụ/thi cử/truyền thông và CareerService phục vụ import chuẩn đầu ra CLO/PLO và tạo CV thông minh.

### 1.1. Công nghệ chính

| Thành phần | Công nghệ hiện tại |
| --- | --- |
| Backend | .NET 8, ASP.NET Core Web API |
| ORM và cơ sở dữ liệu | Entity Framework Core 8, SQL Server, schema-per-service |
| Frontend | React 19, TypeScript 6, Vite 8 |
| UI và dữ liệu phía client | Ant Design 6, TanStack Query 5, Axios |
| Kiểm thử frontend | Vitest, Testing Library |
| AI | CareerService gọi nhà cung cấp LLM qua HTTP |
| Semantic matching | Tích hợp tùy chọn qua VectorMatchService tại port 5006 |

### 1.2. Các dịch vụ đang có trong solution

| Dịch vụ | HTTP local | Schema | Trách nhiệm chính |
| --- | ---: | --- | --- |
| IdentityService | 5001 | `identity` | Tài khoản, đăng nhập sinh viên, thiết bị, reset mật khẩu, audit và cài đặt |
| AcademicService | 5002 | `academic` | Sinh viên, chương trình học, môn/lớp học phần, lịch, điểm danh, đánh giá, học phí, dữ liệu CV |
| ExamService | 5003 | `exam` | Ngân hàng câu hỏi, kỳ thi, lượt thi, đáp án và kết quả |
| CommunicationService | 5004 | `communication` | Thông báo, mẫu biểu và yêu cầu biểu mẫu |
| CareerService | 5005 | `career` | Import/kiểm duyệt CLO-PLO, chuẩn bị ngữ cảnh CV và tối ưu CV bằng AI |

VectorMatchService tại `http://localhost:5006` là một tích hợp tùy chọn mà CareerService có client để gọi. Mã nguồn triển khai dịch vụ Python và script khởi động riêng không có trong repository hiện tại. `Start-Dev.ps1` không khởi động port 5006. Khi dịch vụ này không hoạt động, chức năng CV vẫn dùng cơ chế fallback theo điểm môn học.

### 1.3. Kiến trúc kết nối

Frontend gọi trực tiếp từng service bằng URL cấu hình, chưa có API Gateway chung:

```text
React/Vite
  ├─ IdentityService      :5001
  ├─ AcademicService      :5002
  ├─ ExamService          :5003
  ├─ CommunicationService :5004
  └─ CareerService        :5005
        ├─ AcademicService :5002
        ├─ VectorMatchService :5006 (tùy chọn, có fallback)
        └─ LLM provider qua HTTPS
```

Các service .NET dùng chung SQL Server nhưng sở hữu schema và `DbContext` riêng. Giao tiếp liên service hiện chủ yếu dùng REST/HTTP; repository chưa triển khai message broker hoặc gRPC.

## 2. Người dùng và phạm vi giao diện

### 2.1. Cổng sinh viên

Sinh viên đăng nhập bằng JWT và chỉ được truy cập dữ liệu thuộc chính mình. Các màn hình hiện có:

- Dashboard, hồ sơ cá nhân và chương trình học.
- Môn/lớp học phần, lịch học, điểm danh và đánh giá.
- Kết quả thi.
- Thông báo và tài liệu môn học.
- Học phí và yêu cầu biểu mẫu.
- Xác minh thực tập.
- CV thông minh tại `/student/resume-builder`.

Route `/student/resume-builder-old` vẫn tồn tại để tham chiếu màn hình CV cũ; wizard mới là route chính.

### 2.2. Cổng quản trị

Giao diện quản trị hiện dùng ba nhãn vai trò điều hướng: `Administrator`, `AcademicManager` và `Viewer`.

- Cả ba vai trò có thể xem dashboard, sinh viên, khoa/ngành, môn học, kế hoạch đào tạo, lớp học phần, giảng viên, lịch, điểm danh, kết quả, ngân hàng câu hỏi, đánh giá, thông báo và biểu mẫu.
- `Administrator` và `AcademicManager` có mục phân công giảng viên.
- Chỉ `Administrator` nhìn thấy mục tài khoản, hệ thống và CLO/PLO trên menu.
- API import CLO/PLO của CareerService bảo vệ bằng policy backend `SystemAdministrator`; cấu hình xác thực môi trường phải ánh xạ đúng role này.

Phân quyền hiển thị menu không thay thế kiểm tra quyền tại API.

## 3. Mô hình dữ liệu

Mã nguồn hiện khai báo 47 `DbSet`: Identity 5, Academic 22, Exam 8, Communication 3 và Career 9.

### 3.1. Schema `identity` — 5 thực thể

| Thực thể | Mục đích |
| --- | --- |
| `Users` | Tài khoản, vai trò và hồ sơ nhận dạng |
| `AuditLogs` | Nhật ký thao tác |
| `PasswordResets` | Yêu cầu và token đặt lại mật khẩu |
| `Settings` | Cấu hình hệ thống |
| `UserDevices` | Thiết bị và phiên liên quan đến người dùng |

### 3.2. Schema `academic` — 22 thực thể

| Nhóm | Thực thể |
| --- | --- |
| Cấu trúc đào tạo | `AcademicYears`, `Faculties`, `Majors`, `SemesterPlans`, `SemesterSubjects` |
| Con người | `Students`, `TeacherFaculties` |
| Môn và lớp học phần | `Subjects`, `SubjectTeachings`, `SubjectTeachingTeachers`, `SubjectStudents` |
| Lịch và cơ sở vật chất | `SubjectSchedules`, `Rooms` |
| Học tập | `Attendances`, `EvaluationCriterias`, `StudentEvaluations`, `StudentEvaluationDetails` |
| Tài liệu và ngoại lệ | `SubjectDocuments`, `SubjectSpecialNotes` |
| Tài chính | `SemesterTuitions` |
| Nguồn dữ liệu CV | `StudentProjects`, `StudentInternships` |

`StudentProjects` lưu dự án e-Portfolio: tên dự án, công nghệ, mô tả, URL mã nguồn, quy mô nhóm, vai trò, đóng góp và môn liên kết. `StudentInternships` lưu công ty, vị trí, thời gian và nhiệm vụ thực tập.

Hai thực thể CV đã có trong EF model hiện tại nhưng chưa xuất hiện trong migration baseline đang được lưu trong repository. Database đích phải có migration hoặc script tạo bảng tương ứng trước khi dùng dữ liệu thật.

### 3.3. Schema `exam` — 8 thực thể

| Thực thể | Mục đích |
| --- | --- |
| `Questions` | Ngân hàng câu hỏi |
| `QuestionAnswers` | Các đáp án của câu hỏi |
| `QuestionSuites` | Bộ câu hỏi |
| `SubjectTeachingExams` | Kỳ thi gắn với lớp học phần |
| `ExamAttempts` | Lượt làm bài của sinh viên |
| `ExamResults` | Điểm và trạng thái kết quả |
| `ExamQuestionSelections` | Câu hỏi được chọn cho lượt thi |
| `ExamQuestionAnswers` | Đáp án sinh viên đã chọn |

### 3.4. Schema `communication` — 3 thực thể

| Thực thể | Mục đích |
| --- | --- |
| `FormTemplates` | Danh mục mẫu biểu |
| `FormRequests` | Yêu cầu biểu mẫu của sinh viên và trạng thái xử lý |
| `UserAnnouncements` | Thông báo gửi đến người dùng |

### 3.5. Schema `career` — 9 thực thể

| Thực thể | Mục đích |
| --- | --- |
| `CurriculumVersions` | Phiên bản chương trình đào tạo |
| `ProgramLearningOutcomes` | PLO chính thức |
| `CourseLearningOutcomes` | CLO chính thức và snapshot học phần |
| `ProgressionLevels` | Mức E/R/D |
| `CloPloMappings` | Ánh xạ CLO–PLO |
| `OutcomeImportBatches` | Batch, file, trạng thái và JSON staging |
| `OutcomeDocumentBlocks` | Block nguồn trích từ tài liệu |
| `OutcomeReviewLogs` | Audit chỉnh sửa và kiểm duyệt |
| `ApprovedOutcomeJsonDocuments` | JSON chính thức có version, hash và provenance |

### 3.6. Quan hệ nghiệp vụ chính

```text
Major → SemesterPlan → SemesterSubject → Subject
Subject → SubjectTeaching → SubjectStudent ← Student
SubjectTeaching → SubjectSchedule → Attendance
Student + Subject → StudentEvaluation → StudentEvaluationDetail

QuestionSuite → Question → QuestionAnswer
SubjectTeachingExam → ExamAttempt → ExamQuestionSelection/ExamQuestionAnswer
ExamAttempt → ExamResult

CurriculumVersion → PLO
CurriculumVersion + Subject snapshot → CLO
CLO + PLO + ProgressionLevel → CloPloMapping
```

Khóa ngoại xuyên service/schema không nên được giả định là ràng buộc vật lý ở mọi nơi. Các tích hợp cần kiểm tra ID, quyền sở hữu và dữ liệu tồn tại qua API hoặc logic ứng dụng.

## 4. Xác thực và an toàn dữ liệu

### 4.1. Sinh viên

- Đăng nhập qua `POST /api/auth/student/login` tại IdentityService.
- Frontend lưu token sinh viên trong `sessionStorage` và tự gắn `Authorization: Bearer ...` bằng Axios interceptor.
- Khi gặp `401`, frontend xóa phiên và chuyển về trang đăng nhập.
- Các API `student/me` và API CV kiểm tra claim sinh viên.
- `studentId` trong URL/body phải trùng sinh viên đang đăng nhập; nếu không, API trả `404` để không làm lộ tài nguyên của người khác.
- Năm service dùng chung signing key khi chạy local. Script backend sinh hoặc dùng lại key trong `.run/student-jwt-signing-key`.

### 4.2. Dữ liệu đưa vào AI

- CareerService loại HTML/active content, control characters và chuẩn hóa khoảng trắng.
- Email, URL và số điện thoại bị loại khỏi phần văn bản gửi cho LLM.
- `StudentId` và `StudentCode` được giữ ở server để kiểm tra quyền/khôi phục header nhưng được đánh dấu không serialize vào prompt.
- Dữ liệu trong JD và nội dung người dùng được xem là dữ liệu không tin cậy, không phải chỉ dẫn hệ thống.
- API key và connection string phải đến từ user secrets, biến môi trường hoặc file cấu hình local không commit; không ghi khóa thật vào tài liệu.

## 5. CV thông minh

### 5.1. Mục tiêu

Wizard tạo CV tiếng Việt thân thiện ATS, hướng tới một trang A4. AI chỉ biên tập cách diễn đạt dựa trên dữ liệu đã chọn, không được tự tạo công ty, dự án, vai trò, công nghệ, thời gian hoặc số liệu thành tích.

### 5.2. Luồng giao diện bốn bước

1. **Mục tiêu & JD**: nhập vị trí mục tiêu, mô tả công việc, career focus tag và thông tin liên hệ dùng để hiển thị trên CV.
2. **Học phần & CLO**: chọn trong tối đa 10 môn được đề xuất theo độ phù hợp JD hoặc theo điểm fallback.
3. **Dự án & Thực tập**: chọn/chỉnh sửa dự án e-Portfolio, thêm dự án cá nhân, chọn thực tập, chứng chỉ và hoạt động/giải thưởng.
4. **Tối ưu AI & Xuất PDF**: xem trước A4, gọi AI, chỉnh trực tiếp nội dung cho phép và mở hộp thoại in của trình duyệt.

State của wizard được lưu vào `sessionStorage` với key `tdu.resume-builder.wizard.v1`, nên F5 trong cùng phiên không làm mất dữ liệu. Đây không phải bản CV được lưu lâu dài trên server; đóng phiên/xóa storage có thể làm mất bản nháp.

### 5.3. Tải dữ liệu và phân tích JD

Khi người dùng bấm phân tích, frontend gọi song song:

```http
GET  http://localhost:5002/api/academic/resume/get-context-data/{studentId}
POST http://localhost:5005/api/career/resume/prepare-context
```

AcademicService trả:

- Thông tin sinh viên, ngành, khoa và năm học.
- GPA tính từ điểm trung bình môn; ưu tiên trung bình có trọng số tín chỉ khi có tín chỉ.
- `eligibleCourses`: các môn có điểm trung bình từ `7.0` trở lên.
- Toàn bộ dự án thuộc sinh viên.
- Các đợt thực tập đã có trong nguồn dữ liệu CV.

CareerService đồng thời lấy Academic context và thử gọi VectorMatchService. Nếu semantic matching thành công, kết quả được giới hạn trong các môn đủ điều kiện của sinh viên rồi xếp theo similarity. Nếu port 5006 không kết nối được, timeout hoặc trả lỗi downstream, CareerService trả `isFallbackMode: true` và xếp môn theo điểm. Frontend vẫn hiển thị tối đa 10 môn; cảnh báo fallback không đồng nghĩa với mất dữ liệu môn học.

Nếu giao diện hiển thị `0 / 0` môn:

- Kiểm tra `GET .../get-context-data/{studentId}` có `eligibleCourses` hay không.
- Môn chỉ đủ điều kiện khi điểm trung bình từ 7.0.
- Kiểm tra JWT đúng sinh viên và AcademicService đang chạy bản build mới.
- VectorMatch hỏng không được làm danh sách về 0; bản hiện tại phải dùng score fallback.

### 5.4. Chuẩn bị context tại CareerService

`POST /api/career/resume/prepare-context` yêu cầu JWT sinh viên và nhận chung contract với endpoint optimize:

```json
{
  "studentId": "guid",
  "targetRole": "Frontend Developer",
  "jobDescription": "...",
  "careerFocusTag": "Frontend & UI",
  "selectedSubjectIds": [],
  "selectedInternshipIds": [],
  "uiProjects": [],
  "certifications": [],
  "awardsAndActivities": [],
  "currentSummaryDraft": null,
  "topK": 8,
  "similarityThreshold": 0.65
}
```

Giới hạn backend hiện tại:

| Trường | Giới hạn |
| --- | ---: |
| `targetRole` | 2–200 ký tự |
| `jobDescription` | 10–10.000 ký tự |
| `careerFocusTag` | tối đa 200 ký tự |
| `currentSummaryDraft` | tối đa 3.000 ký tự |
| `selectedSubjectIds` | tối đa 100 |
| `selectedInternshipIds` | tối đa 100 |
| `uiProjects` | tối đa 20 |
| `certifications` | tối đa 20 |
| `awardsAndActivities` | tối đa 20 |
| `topK` | 1–100 |
| `similarityThreshold` | 0,50–1,00 |

Quy tắc lựa chọn quan trọng:

- Frontend chỉ gửi dự án có `isSelectedForCv === true` và có tên.
- `uiProjects` khác `null` là lựa chọn có chủ đích của người dùng. Mảng `[]` nghĩa là không đưa dự án nào vào CV.
- CareerService chỉ fallback về toàn bộ dự án Academic khi `uiProjects` là `null`, không phải khi là mảng rỗng.
- `selectedInternshipIds` chỉ được hydrate nếu ID nằm trong danh sách thực tập của chính sinh viên; không chọn nghĩa là không đưa thực tập vào CV.
- Chứng chỉ và hoạt động/giải thưởng chỉ được gửi khi đã chọn.

Quy tắc này ngăn lỗi “chọn 2 dự án nhưng CV hiển thị toàn bộ dự án”.

### 5.5. Tối ưu CV bằng AI

Frontend gọi:

```http
POST http://localhost:5005/api/career/resume/optimize
Authorization: Bearer <student-jwt>
Content-Type: application/json
```

Timeout riêng phía frontend cho request này là 310 giây. CareerService hydrate lại toàn bộ context ở server, gọi LLM, parse JSON, áp dụng fact guard rồi mới tính quality metrics.

Cấu hình mặc định hiện tại trong `CareerService/appsettings.json`:

```json
{
  "ResumeLLM": {
    "Provider": "Vault",
    "Model": "gpt-5.6-sol",
    "BaseUrl": "https://newapi.vault.io.vn/v1",
    "TimeoutSeconds": 300,
    "MaxRetries": 2,
    "MaxInputTokensPerRequest": 12000
  }
}
```

`ResumeLLM` là cấu hình riêng cho tính năng CV; không dùng model trong section `LLM` của pipeline CLO/PLO. Với `Vault + gpt-5.6-sol`, CareerService gọi `/v1/responses`, bật streaming và ghép các sự kiện SSE `response.output_text.delta`. Những model Vault khác dùng `/chat/completions`. Các provider tương thích khác trong code gồm Gemini, Groq và OpenAI.

Log chuẩn để xác nhận model thực sự được dùng:

```text
Generating optimized resume with provider Vault and model gpt-5.6-sol.
```

Sau khi sửa cấu hình/mã nguồn, phải dừng và khởi động lại CareerService. Script start sẽ bỏ qua service còn sống, vì vậy chỉ bấm chạy lại `Start-Dev.ps1` có thể vẫn dùng process cũ.

### 5.6. Prompt và fact guard

Prompt hiện yêu cầu:

- Professional summary đúng 3 câu theo dạng elevator pitch.
- Nhóm kỹ năng thành `knowledgeDomain`, `functionalSkills`, `interpersonalSkills`.
- Mỗi kỹ năng có `skillName`, một trong ba mức `Thành thạo`/`Khá tốt`/`Nền tảng` và mảng `keywords`.
- Dự án và thực tập có 2–3 bullet theo STAR/XYZ, bắt đầu bằng động từ hành động.
- Không sao chép thô đóng góp/nhiệm vụ và không tạo số liệu mới.

Sau khi nhận JSON từ model, fact guard:

- Khôi phục header, giáo dục, tên dự án, vai trò, tech stack, công ty, vị trí và thời gian từ nguồn thật.
- Giữ chứng chỉ và giải thưởng/hoạt động từ payload đã chọn.
- Loại số không có trong bằng chứng và tham chiếu công ty không được xác minh.
- Chỉ chấp nhận summary đúng 3 câu và không có dữ kiện bị bịa; nếu không đạt sẽ dùng fallback an toàn.
- Chỉ giữ bullet có động từ hành động, không phải bản sao thô và không chứa dữ kiện mới; bổ sung bullet fallback khi cần.
- Lọc và khử trùng lặp keyword kỹ năng dựa trên bằng chứng từ môn, dự án, thực tập và JD.

Fact guard không đòi mọi câu biên tập phải trùng từ vựng với dữ liệu gốc; mục tiêu là cho phép viết lại tự nhiên nhưng khóa chặt fact định danh và số liệu.

### 5.7. Response tối ưu

```json
{
  "header": {
    "fullName": "...",
    "studentCode": "...",
    "majorName": "...",
    "gpa": 8.1,
    "targetRole": "..."
  },
  "education": {
    "institutionName": "...",
    "majorName": "...",
    "degreeName": "...",
    "gpa": 8.1,
    "durationText": "..."
  },
  "professionalSummary": "...",
  "skills": {
    "knowledgeDomain": [],
    "functionalSkills": [],
    "interpersonalSkills": []
  },
  "projects": [],
  "internships": [],
  "certifications": [],
  "awardsAndActivities": [],
  "qualityMetrics": {
    "jobAlignmentScore": 0,
    "contentPreservationScore": 0,
    "hasHallucinationWarning": false
  }
}
```

Frontend hiển thị kỹ năng thành ba dòng key–value; tên kỹ năng đứng trước dấu hai chấm, keyword ngăn cách bằng dấu phẩy và các mục kỹ năng ngăn cách bằng dấu chấm phẩy.

### 5.8. Xem trước, chỉnh sửa và in PDF

- A4 preview tự co theo chiều rộng panel; người dùng có thể zoom thủ công từ 30% đến 200%.
- Summary, keyword kỹ năng và bullet dự án/thực tập có thể chỉnh trực tiếp trên bản xem trước sau khi AI trả kết quả.
- CSS `@media print` ẩn toàn bộ navigation, nút và panel không liên quan; chỉ vùng CV được in.
- Nút in gọi `window.print()`. Xuất PDF dùng máy in ảo hoặc tùy chọn Save as PDF của trình duyệt, không phải dịch vụ sinh PDF ở backend.
- Khổ mong đợi là A4 portrait. Nếu preview bị cắt/thu nhỏ bất thường, kiểm tra paper size A4, scale mặc định và margin trong hộp thoại in.

### 5.9. Quality metrics

CareerService tự tính metrics sau fact guard, không tin số do LLM trả về:

- `jobAlignmentScore`: mức độ khớp với JD dựa trên nội dung đã sinh.
- `contentPreservationScore`: mức bảo toàn dữ liệu nguồn.
- `hasHallucinationWarning`: cờ cảnh báo khi phát hiện dấu hiệu không được nguồn hỗ trợ.

Các chỉ số hỗ trợ người dùng rà soát, không phải cam kết chất lượng tuyển dụng.

## 6. Import và kiểm duyệt CLO/PLO

### 6.1. Phạm vi

CareerService cho phép quản trị viên tạo/chọn phiên bản chương trình, upload tài liệu chuẩn đầu ra, xử lý nền, rà soát draft, validate, approve/reject/archive và tải JSON chính thức.

Luồng tổng quát:

```text
Chọn ngành/học phần/curriculum
  → Upload DOCX hoặc PDF
  → Kiểm tra loại file, kích thước và hash
  → Tách block nguồn/OCR khi cần
  → LLM trích PLO, CLO và mapping
  → Reconcile + rule validation + đối chiếu AcademicService
  → PendingReview hoặc ValidationFailed
  → Admin sửa/remove/restore và validate lại
  → Approve / Reject / Archive
  → Publish bảng chính thức + ApprovedOutcomeJsonDocuments
```

### 6.2. API quản trị chính

Base route: `/api/management`, yêu cầu policy `SystemAdministrator`.

| Method | Route | Mục đích |
| --- | --- | --- |
| GET | `/curriculum-versions` | Danh sách curriculum |
| POST | `/curriculum-versions` | Tạo curriculum |
| GET | `/outcome-imports` | Tìm/lọc/phân trang batch |
| POST | `/outcome-imports` | Upload batch |
| POST | `/outcome-imports/{id}/process` | Yêu cầu xử lý/retry |
| GET | `/outcome-imports/{id}` | Chi tiết batch |
| GET | `/outcome-imports/{id}/review` | Dữ liệu review |
| PATCH | `/outcome-imports/{id}/review` | Sửa draft |
| POST | `/outcome-imports/{id}/validate` | Validate lại |
| POST | `/outcome-imports/{id}/approve` | Phê duyệt và publish |
| POST | `/outcome-imports/{id}/reject` | Từ chối |
| POST | `/outcome-imports/{id}/archive` | Lưu trữ |
| GET | `/outcome-imports/{id}/approved-document` | Lấy JSON đã duyệt |

### 6.3. Lưu trữ và kiểm soát chất lượng

- Giới hạn file mặc định 10 MB; giới hạn nội dung giải nén 100 MB và 5.000 entry.
- Block nguồn được lưu để truy vết heading, paragraph hoặc table.
- Worker nền nhận batch chờ xử lý.
- Reconciliation chuẩn hóa mã và nối kết quả giữa nhiều lượt/model response.
- Validation kiểm tra mã trùng, tham chiếu CLO/PLO, progression E/R/D và học phần với AcademicService.
- Approve là thao tác publish dữ liệu chính thức và tạo JSON có version/hash/provenance.
- `rowversion` và review log hỗ trợ kiểm soát ghi đồng thời và audit.

### 6.4. Cấu hình AI cho CLO/PLO

Pipeline CLO/PLO dùng section `LLM`, độc lập với `ResumeLLM`. Code hỗ trợ provider `Vault`, `Gemini` hoặc `Groq`. Cấu hình OCR chỉ chấp nhận `Gemini` hoặc `Vault`; giá trị khác sẽ được xử lý như Gemini ở bước đăng ký service và không nên dùng. Cần đồng bộ `PdfOcr:Provider` với validation trong `Program.cs` trước khi bật OCR PDF.

## 7. API và route quan trọng

### 7.1. Health và Swagger

| Service | Health | Swagger local |
| --- | --- | --- |
| Identity | `/api/identity/health` | `http://localhost:5001/swagger` |
| Academic | `/api/academic/health` | `http://localhost:5002/swagger` |
| Exam | `/api/exam/health` | `http://localhost:5003/swagger` |
| Communication | `/api/communication/health` | `http://localhost:5004/swagger` |
| Career | `/health` | `http://localhost:5005/swagger` |

Swagger chỉ được bật trong môi trường Development.

### 7.2. Một số endpoint theo cổng người dùng

| Nhóm | Endpoint đại diện |
| --- | --- |
| Phiên sinh viên | `POST /api/auth/student/login`, `GET /api/student/me/session` |
| Hồ sơ/học tập | Các endpoint `/api/student/me/...` và `/api/academic/...` |
| CV context | `GET /api/academic/resume/get-context-data/{studentId}` |
| Phân tích CV | `POST /api/career/resume/prepare-context` |
| Tối ưu CV | `POST /api/career/resume/optimize` |
| Quản trị tổng hợp | Các endpoint `/api/management/...` |

Response phần lớn theo wrapper `ApiResponse<T>`; client phải kiểm tra cả HTTP status và trường lỗi trong body.

## 8. Cấu hình và chạy local

### 8.1. Yêu cầu

- .NET SDK 8.
- Node.js/npm tương thích Vite 8.
- SQL Server có schema/bảng đúng với migration và dữ liệu seed cần thiết.
- API key của provider AI nếu dùng CLO/PLO hoặc tối ưu CV.
- VectorMatchService riêng nếu muốn semantic matching; không bắt buộc cho score fallback.

### 8.2. Khởi động toàn bộ ứng dụng

Từ thư mục `EducationSystem`:

```powershell
.\scripts\Start-Dev.ps1
```

Script thực hiện:

1. Tạo thư mục `.run` nếu chưa có.
2. Sinh hoặc dùng lại signing key JWT local.
3. Build và chạy năm service .NET trên port 5001–5005.
4. Ghi PID và log vào `.run`.
5. Chạy Vite tại `http://127.0.0.1:5173`.

Service đang chạy và còn phản hồi Swagger sẽ được bỏ qua, không build lại. Để chắc chắn nạp thay đổi backend:

```powershell
.\scripts\Stop-BackendServices.ps1
.\scripts\Start-BackendServices.ps1
```

Sau đó chạy frontend nếu chưa chạy:

```powershell
cd .\education-system-ui
npm run dev -- --host 127.0.0.1
```

### 8.3. Biến môi trường frontend

| Biến | Mặc định |
| --- | --- |
| `VITE_IDENTITY_API_BASE` | `http://localhost:5001/api/identity` |
| `VITE_ACADEMIC_API_BASE` | `http://localhost:5002/api/academic` |
| `VITE_EXAM_API_BASE` | `http://localhost:5003/api/exam` |
| `VITE_COMMUNICATION_API_BASE` | `http://localhost:5004/api/communication` |
| `VITE_ACADEMIC_API_ORIGIN` | `http://localhost:5002` cho wizard CV |
| `VITE_CAREER_API_ORIGIN` | `http://localhost:5005` |
| `VITE_AI_API_ORIGIN` | Nếu có, ưu tiên thay `VITE_CAREER_API_ORIGIN` cho API CV |

Các cờ mock cũ như `VITE_AI_OPTIMIZE_MODE` và `VITE_RESUME_API_MODE` không còn điều khiển wizard CV hiện tại.

### 8.4. Log và chẩn đoán CareerService

```powershell
.\scripts\Watch-CareerServiceLogs.ps1
```

Các file thường dùng:

```text
.run/CareerService.out.log
.run/CareerService.err.log
.run/CareerService.pid
.run/CareerService.monitor.<timestamp>.log
```

Thông báo `Failed to determine the https port for redirect` trong local HTTP đến từ `UseHttpsRedirection()` khi launch profile không có HTTPS port. Đây là cảnh báo cấu hình redirect, không phải nguyên nhân trực tiếp của lỗi AI.

## 9. Xử lý lỗi thường gặp

### 9.1. `502 Bad Gateway` khi tối ưu CV

`502` tại `/api/career/resume/optimize` thường có nghĩa CareerService không nhận được phản hồi hợp lệ từ downstream LLM hoặc AcademicService.

Kiểm tra theo thứ tự:

1. CareerService và AcademicService có phản hồi Swagger/health không.
2. `ResumeLLM:Provider`, `Model`, `BaseUrl` và `ApiKey` có được process đang chạy nạp đúng không.
3. Log có dòng provider/model đúng `Vault` và `gpt-5.6-sol` không.
4. Process CareerService đã được restart sau khi sửa cấu hình chưa.
5. Response upstream là timeout, `401/403`, rate limit hay JSON sai schema.

VectorMatch bị từ chối kết nối sẽ tạo warning và score fallback; bản thân warning này không nên làm endpoint optimize trả 502.

### 9.2. Timeout phía trình duyệt

- Frontend chờ optimize tối đa 310 giây.
- `ResumeLLM:TimeoutSeconds` mặc định 300 giây và retry tối đa 2 lần cho lỗi phù hợp.
- Không giảm timeout frontend xuống thấp hơn backend nếu muốn nhận kết quả đầy đủ.
- Với `gpt-5.6-sol`, phải dùng luồng Responses API hiện tại; chat-completions compatibility đã từng timeout.

### 9.3. CV chứa sai số dự án đã chọn

Trong Network tab, kiểm tra `uiProjects` của request optimize:

- Chỉ các project được tick phải xuất hiện.
- Không chọn project nào phải gửi `[]`, không bỏ hẳn field.
- Nếu backend nhận `null`, nó được phép fallback sang project từ AcademicService.

### 9.4. Giao diện vừa báo thất bại vừa giữ thông báo thành công cũ

Kết quả thành công cũ có thể còn trong state để người dùng không mất CV. Giao diện hiện tại phải ưu tiên trạng thái lỗi của mutation đang chạy và không hiển thị success alert cũ cùng lúc. Có thể bấm thử lại sau khi xử lý downstream; không cần xóa bản CV cũ chỉ để chẩn đoán.

### 9.5. Trang in bị thiếu hoặc hiển thị cả giao diện

- Dùng nút in trong wizard để CSS gắn đúng vùng CV.
- Chọn A4, Portrait và kiểm tra Scale/Margins.
- Không in toàn bộ trang từ một route cũ hoặc khi stylesheet chưa tải.
- Nếu trình duyệt cache CSS cũ, hard reload trước khi mở lại print preview.

## 10. Trạng thái và giới hạn hiện tại

- Chưa có API Gateway, RabbitMQ, gRPC hoặc Docker orchestration trong repository.
- VectorMatchService là dependency tùy chọn nhưng implementation không nằm trong source tree hiện tại.
- Wizard lưu bản nháp theo browser session, chưa lưu phiên bản CV vào database.
- Xuất PDF dựa vào tính năng in của trình duyệt.
- `StudentProjects` và `StudentInternships` cần được đồng bộ migration ở môi trường mới.
- Tài liệu README gốc ở root vẫn mô tả skeleton bốn service và đã lỗi thời; không nên dùng nó để xác định trạng thái hiện tại.
- Các URL, provider và model trong `appsettings.json` là giá trị mặc định; biến môi trường có thể ghi đè khi deploy.

## 11. Nguồn đối chiếu trong repository

| Nội dung | Nguồn chính |
| --- | --- |
| Service/port và cách start | `scripts/Start-BackendServices.ps1`, `scripts/Start-Dev.ps1` |
| Data model | Các `*DbContext.cs`, entity configuration và migrations của từng service |
| Route frontend | `education-system-ui/src/app/router.tsx` |
| Menu và vai trò quản trị | `education-system-ui/src/app/managementNavigation.tsx` |
| CV context học vụ | `AcademicService/Application/Services/ResumeDataService.cs` |
| Hydrate/fallback/chọn dự án | `CareerService/Infrastructure/ResumeContextHydrationService.cs` |
| Prompt, provider, fact guard | `CareerService/Infrastructure/LlmResumeGeneratorService.cs` |
| Cấu hình model CV | `CareerService/appsettings.json`, section `ResumeLLM` |
| Payload optimize frontend | `resume-builder/hooks/useOptimizeResume.ts` |
| State wizard | `resume-builder/hooks/useResumeStore.tsx` |
| Preview/in A4 | `A4PaperPreview.tsx`, `resume-builder.css`, `printA4Resume.ts` |

Khi bổ sung tính năng, cần cập nhật tài liệu này cùng thay đổi code, đặc biệt với contract API, migration, provider/model AI, script vận hành và chính sách phân quyền.
