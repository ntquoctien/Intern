# AI Resume Builder và luồng xác thực thực tập

## 1. Tổng quan

Trong chuỗi công việc này, tính năng **AI-Powered Resume Builder** đã được xây dựng xuyên suốt từ tầng cơ sở dữ liệu, API ASP.NET Core đến giao diện React. Trọng tâm là tái sử dụng phân hệ `FormRequests` của `CommunicationService` để xác thực thực tập, đồng bộ dữ liệu đã được nhà trường duyệt sang `AcademicService`, sau đó cho phép sinh viên chọn các đợt thực tập đó để đưa lên CV.

Kiến trúc được giữ theo mô hình Modular Microservices và Schema-per-Service:

- `AcademicService`: cổng `5002`, schema `academic`.
- `CommunicationService`: cổng `5004`, schema `communication`.
- Frontend: React 19, TypeScript, Vite, Ant Design 6, TanStack React Query và CSS/Tailwind utility classes.
- Response API tiếp tục sử dụng `ApiResponse<T>` của `SharedKernel`.

## 2. Giai đoạn 1 – Database và EF Core

### 2.1. Kho dự án e-Portfolio

Đã tạo entity `StudentProject` trong namespace:

```text
EducationSystem.Services.Academic.Domain.Entities
```

Entity được ánh xạ vào bảng `academic.StudentProjects`, gồm:

- `ProjectId`: khóa chính, identity.
- `StudentId`: khóa ngoại tới `academic.Students`.
- `ProjectName`.
- `TechStack`.
- `ProjectDescription`.
- `SourceCodeUrl`.
- `TeamSize`, mặc định bằng `1`.
- `MyRole`.
- `MyContributions`.
- `MappedCourseId`: liên kết tùy chọn tới `academic.Subjects`.

Các cấu hình bổ sung:

- Check constraint bảo đảm `TeamSize >= 1`.
- Index theo `StudentId`.
- Index theo `MappedCourseId`.
- Cấu hình đầy đủ độ dài và kiểu dữ liệu SQL Server.
- Không tạo liên kết xuyên schema/service ngoài phạm vi `AcademicService`.

### 2.2. Lịch sử thực tập chính thức

Đã tạo entity `StudentInternship`, ánh xạ vào `academic.StudentInternships`, gồm:

- `InternshipId`: khóa chính, identity.
- `StudentId`: khóa ngoại tới sinh viên.
- `CompanyName`.
- `Position`.
- `StartDate`.
- `EndDate`.
- `TaskDescription`.
- `FormRequestId`: ID đối soát từ `communication.FormRequests`.

Do khóa chính hiện tại của `Students` và `FormRequests` trong dự án là `uniqueidentifier`, mã triển khai sử dụng `Guid` thay vì `VARCHAR/INT` như bản đặc tả ban đầu.

Các cấu hình quan trọng:

- Check constraint bảo đảm `EndDate` không nhỏ hơn `StartDate`.
- Index theo `StudentId`.
- Unique filtered index theo `FormRequestId`.
- Unique index giúp thao tác đồng bộ liên service có tính idempotent, tránh tạo hai dòng thực tập từ cùng một biểu mẫu.
- `FormRequestId` chỉ đóng vai trò correlation ID, không tạo EF navigation xuyên service/schema.

### 2.3. Mở rộng FormRequest

Entity `FormRequest` của `CommunicationService` đã được bổ sung:

- `EmployerToken: Guid?`.
- `EmployerVerifiedStatus: int`, mặc định `0`.
- `VerificationData: string?`, ánh xạ `NVARCHAR(MAX)`.

Fluent API đã cấu hình:

- Giá trị mặc định của `EmployerVerifiedStatus` là `0`.
- Check constraint chỉ cho phép trạng thái `0`, `1`, `2`.
- Unique filtered index cho `EmployerToken` khi token khác `NULL`.
- Token có thể được dùng như mã xác thực một lần.

### 2.4. SQL migration thô

Đã tạo script:

```text
EducationSystem/scripts/20260728-ai-resume-builder-phase1.sql
```

Script bao gồm:

- Bổ sung các cột xác thực doanh nghiệp vào `communication.FormRequests`.
- Tạo `academic.StudentProjects`.
- Tạo `academic.StudentInternships`.
- Tạo khóa chính, khóa ngoại, index, filtered unique index, default constraint và check constraint tương ứng.

## 3. Giai đoạn 2 – API xác thực và đồng bộ thực tập

### 3.1. CommunicationService

Đã triển khai `InternshipFormController` tại route:

```text
/api/communication/form-requests
```

#### Gửi yêu cầu xác nhận thực tập

```http
POST /api/communication/form-requests/submit-internship-request
```

Luồng xử lý:

- Yêu cầu đăng nhập với policy sinh viên.
- Lấy `StudentId` từ claim/session hiện tại, không tin tưởng ID do client tự truyền.
- Tiếp nhận công ty, vị trí, thời gian, mô tả nhiệm vụ và email mentor.
- Kiểm tra tính hợp lệ của khoảng thời gian.
- Tạo `FormRequest` trạng thái chờ duyệt.
- Sinh `EmployerToken` mới bằng `Guid`.
- Đặt `EmployerVerifiedStatus = 0`.
- Lưu dữ liệu khai báo thực tập dạng JSON để phục vụ xác thực và đồng bộ.
- Gọi `IInternshipVerificationEmailSender`.
- SMTP sender gửi email HTML thật đến mentor, trong đó chứa đường dẫn:

```text
{InternshipEmail:PortalBaseUrl}/verify-internship?token={EmployerToken}
```

- SMTP host, port, TLS, người gửi và tài khoản được đọc từ cấu hình `InternshipEmail`.
- Mật khẩu SMTP không lưu trong source code; môi trường development dùng .NET User Secrets hoặc environment variables.
- Cấu hình được kiểm tra khi service khởi động bằng `ValidateOnStart`.

#### Doanh nghiệp xác thực

```http
POST /api/communication/form-requests/verify-by-employer
```

Đặc điểm:

- Cho phép truy cập công khai bằng `[AllowAnonymous]`.
- Tìm yêu cầu theo token chưa sử dụng.
- Ghi nhận xác nhận hoặc từ chối.
- Trạng thái `1`: doanh nghiệp xác nhận.
- Trạng thái `2`: doanh nghiệp từ chối.
- Lưu điểm số, nhận xét và thời điểm phản hồi trong `VerificationData` dạng JSON.
- Token chỉ được xử lý khi trạng thái còn chờ, ngăn gửi phản hồi lần hai.

#### Admin phê duyệt

```http
POST /api/communication/form-requests/admin-approve/{requestId}
```

Luồng xử lý:

- Bảo vệ bằng policy `AdminOnly`.
- Chỉ cho duyệt khi doanh nghiệp đã xác nhận.
- Chuyển `FormRequest.Status` sang trạng thái Approved.
- Ghi nhận người duyệt.
- Ánh xạ dữ liệu biểu mẫu sang DTO đồng bộ thực tập.
- Gọi `AcademicService` qua typed service sử dụng `IHttpClientFactory`.

### 3.2. Kết nối nội bộ CommunicationService → AcademicService

Đã đăng ký named `HttpClient` với tên `AcademicService`.

Client gọi:

```http
POST http://localhost:5002/api/academic/internships/sync-approved
```

Request nội bộ gắn header:

```text
X-Internal-Api-Key
```

Khóa nội bộ được lấy từ cấu hình `InternalApi:Key`. Phía `AcademicService` so sánh khóa bằng `CryptographicOperations.FixedTimeEquals` để hạn chế timing attack.

`AcademicInternshipClient` sử dụng:

- `IHttpClientFactory`.
- `PostAsJsonAsync`.
- `EnsureSuccessStatusCode`.
- Đọc kết quả theo `ApiResponse<int>`.

### 3.3. AcademicService

Đã triển khai `StudentInternshipsController`.

#### Nhận dữ liệu đã duyệt

```http
POST /api/academic/internships/sync-approved
```

Luồng xử lý:

- Chỉ chấp nhận request có internal API key hợp lệ.
- Kiểm tra `FormRequestId`.
- Kiểm tra khoảng thời gian.
- Kiểm tra sinh viên tồn tại.
- Nếu `FormRequestId` đã được đồng bộ thì trả lại bản ghi hiện tại, không tạo trùng.
- Nếu chưa có thì tạo một `StudentInternship` mới.

#### Lấy lịch sử thực tập của sinh viên

```http
GET /api/academic/students/{studentId}/internships
```

Luồng xử lý:

- Yêu cầu policy sinh viên.
- So sánh `{studentId}` với sinh viên đang đăng nhập.
- Không cho phép sinh viên đọc lịch sử của người khác.
- Truy vấn bằng `AsNoTracking`.
- Trả danh sách theo DTO tinh gọn trong `ApiResponse<IReadOnlyList<StudentInternshipDto>>`.

### 3.4. API tổng hợp dữ liệu CV

Đã triển khai:

```http
GET /api/academic/resume/get-context-data/{studentId}
```

`ResumeDataService` tổng hợp:

- Thông tin định danh sinh viên.
- GPA.
- Các học phần đạt từ `7.0` trở lên.
- Chuẩn đầu ra/CLO của học phần.
- Các dự án trong `StudentProjects`.
- Các đợt thực tập chính thức trong `StudentInternships`.

Các truy vấn đọc sử dụng `AsNoTracking` và ánh xạ sang Contextual JSON gọn nhẹ để chuẩn bị cho Resume Builder hoặc dịch vụ AI sau này.

## 4. Giai đoạn 3 – Giao diện yêu cầu và xác thực thực tập

### 4.1. Trang Yêu cầu biểu mẫu của sinh viên

Đã mở rộng:

```text
/student/form-requests
/communication/form-requests
```

Khi chọn mẫu “Đơn xác nhận thực tập doanh nghiệp”, giao diện hiển thị:

- Tên doanh nghiệp.
- Dropdown vị trí: Backend, Frontend, Fullstack, Mobile, QA, UI/UX.
- Email mentor.
- Khoảng thời gian thực tập bằng Ant Design `RangePicker`.
- Mô tả nhiệm vụ thực tế.
- Nút gửi yêu cầu Navy, có loading.

Danh sách đơn có Smart Badge/Progress theo trạng thái:

- `0`: Chờ Doanh Nghiệp Xác Thực.
- `1`: Chờ Nhà Trường Duyệt.
- `2`: Đã Phê Duyệt.
- `3`: Bị Từ Chối.

Giao diện sử dụng React Query/Axios và luôn gọi API thật của `CommunicationService`; nhánh tạo request giả đã được loại bỏ.

### 4.2. Cổng xác thực công khai cho doanh nghiệp

Đã tạo route:

```text
/verify-internship?token={token}
```

Trang này:

- Không sử dụng sidebar/header của portal sinh viên.
- Có giao diện Card trắng, Navy, logo/nhận diện Đại học Tây Đô.
- Đọc token từ query string.
- Hiển thị công ty, sinh viên, MSSV, vị trí và nhiệm vụ khai báo.
- Cho mentor xác nhận thông tin.
- Cho nhập điểm và nhận xét.
- Có nút xác nhận và nút từ chối.
- Hiển thị trạng thái thành công/thất bại sau khi phản hồi.
- Thông báo rõ token không thể tái sử dụng.

## 5. Giai đoạn 3–4 – AI CV Builder Workspace

### 5.1. Bố cục và state tập trung

Đã tạo `AICVBuilderWorkspace.tsx` với bố cục:

- Panel trái khoảng 45% cho dữ liệu đầu vào.
- Panel phải khoảng 55% cho A4 Preview.
- Giao diện Navy `#002140/#0B3A60`.
- Card trắng, viền xám nhạt, `rounded-2xl`, `shadow-sm`.
- Ant Design `Collapse`, `Checkbox`, `Select`, `Progress`, `Button`, `Spin`, `Tooltip`.

Toàn bộ nội dung CV được quản lý trong state:

```ts
resumeData = {
  studentInfo,
  selectedCourses,
  projects,
  internships,
  aiContext,
  cvOutput
}
```

Các handler đã có:

- `handleSubjectSelectionChange`.
- `handleProjectChange`.
- `handleProjectSubmit`.
- `handleInlineEdit`.
- `handleInternshipSelectionChange`.
- `handleInternshipInlineEdit`.
- `handleAIOptimize` ở chế độ mock/scaffold.

### 5.2. Nội dung CV

Workspace hỗ trợ:

- Nhập vị trí và JD mục tiêu.
- Chọn học phần đạt chuẩn.
- Thêm/xóa dự án.
- Phân biệt dự án solo và dự án nhóm.
- Nhập vai trò và đóng góp cá nhân.
- Chỉnh sửa trực tiếp nội dung CV bằng `contentEditable`.
- Đồng bộ lại state tại sự kiện `onBlur`.

Mock data gồm sinh viên Nguyễn Trọng Nghĩa, GPA `7.54`, học phần Lập trình Web, Cơ sở dữ liệu và các dữ liệu dự án phù hợp.

### 5.3. Metrics Dashboard

Phía trên A4 Preview đã có:

- `Content Preservation`: mặc định `95%`.
- `Job Alignment`: mặc định `65%`, mock tăng lên `88%` khi tối ưu.
- Progress bar có animation.
- Tooltip giải thích Token Space Overlap và Latent Space Cosine Similarity.

### 5.4. In CV/Xuất PDF

Đã bổ sung CSS in A4:

- `@page { size: A4 portrait; margin: 1cm; }`.
- Ẩn sidebar, header, footer, panel điều khiển, metrics và các nút.
- Ẩn outline/background của `contentEditable`.
- Ngăn ngắt trang trong header, section và từng entry.
- Điều chỉnh font, gap và margin dành riêng cho bản in.
- Tính toán `--print-content-scale` trước khi gọi `window.print()`.
- Ép nội dung vào vùng in `190mm × 277mm`, tương ứng một trang A4 sau lề.

## 6. Giai đoạn 4 – Đồng bộ thực tập đã duyệt lên CV

Đây là phần hoàn thiện gần nhất.

### 6.1. Load dữ liệu bằng React Query

`AICVBuilderWorkspace.tsx` gọi:

```http
GET /api/academic/students/{studentId}/internships
```

Trong đó:

- `studentId` lấy từ `useStudentAuth()`.
- Query key chứa student ID.
- Có `staleTime` để tránh gọi lại không cần thiết.
- Axios client dùng chung tự động gắn JWT.
- Không còn dữ liệu thực tập giả; lỗi API và danh sách rỗng được thể hiện bằng trạng thái UI riêng.

### 6.2. Trạng thái loading, lỗi và rỗng

Tab “Thực tập & Kinh nghiệm” xử lý:

- Loading bằng Ant Design `Spin`.
- Lỗi tải dữ liệu.
- Empty State nếu chưa có thực tập được duyệt.
- Liên kết sinh viên về phần “Yêu cầu biểu mẫu”.

### 6.3. Checkbox chọn thực tập

Mỗi thực tập được hiển thị dưới dạng Card:

```text
[ ] Thực tập sinh {Position} tại {CompanyName}
    {StartDate} – {EndDate}
```

Card còn hiển thị:

- Mô tả nhiệm vụ rút gọn.
- Biểu tượng xác nhận của nhà trường.
- Trạng thái selected với màu Navy.
- Animation hover và shadow nhẹ.

State lựa chọn được quản lý bằng `selectedInternshipIds`.

Khi check:

- `internshipId` được thêm vào danh sách chọn.
- DTO từ API được ánh xạ sang `resumeData.internships`.
- Mục kinh nghiệm xuất hiện ngay trên A4.

Khi uncheck:

- ID bị loại khỏi danh sách chọn.
- Bản ghi bị loại khỏi `resumeData.internships`.
- Mục tương ứng trên A4 biến mất ngay.
- Nếu không còn bản ghi nào, toàn bộ section “KINH NGHIỆM LÀM VIỆC” được ẩn.

### 6.4. Inline editing theo ID ổn định

Các trường sau hỗ trợ sửa trực tiếp:

- `position`.
- `company`.
- `duration`.
- `responsibilities`.

Handler:

```ts
handleInternshipInlineEdit(id, field, value)
```

Handler tìm phần tử bằng `internshipId`, thay vì sử dụng index. Vì vậy dữ liệu vẫn cập nhật đúng khi người dùng chọn hoặc bỏ chọn các thực tập theo thứ tự bất kỳ.

Nội dung chỉnh sửa được ghi ngược vào `resumeData.internships`, sẵn sàng cho:

- In PDF.
- Lưu bản nháp.
- Gửi sang backend.
- Gửi sang LLM trong giai đoạn sau.

Phần thực tập hiện chỉ hiển thị dữ liệu thô mà sinh viên chọn. Không có thao tác LLM tự sửa nội dung thực tập.

## 7. Cấu hình môi trường frontend

Tệp `.env.example` đã được bổ sung:

```env
VITE_ACADEMIC_API_ORIGIN=http://localhost:5002
VITE_RESUME_API_MODE=mock

VITE_AI_API_ORIGIN=http://localhost:5005
VITE_AI_OPTIMIZE_MODE=mock

VITE_COMMUNICATION_API_ORIGIN=http://localhost:5004
```

Để đọc dữ liệu thật từ `AcademicService` nhưng chưa gọi LLM:

```env
VITE_ACADEMIC_API_ORIGIN=http://localhost:5002
VITE_RESUME_API_MODE=live
VITE_AI_OPTIMIZE_MODE=mock
```

Chế độ Academic API và AI Optimization đã được tách thành hai biến độc lập. Vì vậy có thể chạy toàn bộ luồng thực tập thật mà không vô tình gọi dịch vụ LLM.

## 8. Các tệp chính đã tạo hoặc cập nhật

### Backend

```text
EducationSystem/src/Services/AcademicService/Domain/Entities/StudentProject.cs
EducationSystem/src/Services/AcademicService/Domain/Entities/StudentInternship.cs
EducationSystem/src/Services/AcademicService/Infrastructure/Persistence/AcademicDbContext.cs
EducationSystem/src/Services/AcademicService/Controllers/StudentInternshipsController.cs
EducationSystem/src/Services/AcademicService/Controllers/ResumeDataController.cs
EducationSystem/src/Services/AcademicService/Application/Services/StudentInternshipService.cs
EducationSystem/src/Services/AcademicService/Application/Services/ResumeDataService.cs
EducationSystem/src/Services/CommunicationService/Infrastructure/Persistence/Entities/FormRequest.cs
EducationSystem/src/Services/CommunicationService/Infrastructure/Persistence/CommunicationDbContext.cs
EducationSystem/src/Services/CommunicationService/Controllers/InternshipFormController.cs
EducationSystem/src/Services/CommunicationService/Application/Services/InternshipVerificationService.cs
EducationSystem/src/Services/CommunicationService/Application/Services/InternshipIntegrationServices.cs
EducationSystem/scripts/20260728-ai-resume-builder-phase1.sql
```

### Frontend

```text
EducationSystem/education-system-ui/src/features/student/pages/form-requests/FormRequests.tsx
EducationSystem/education-system-ui/src/features/internship/VerifyInternshipPage.tsx
EducationSystem/education-system-ui/src/features/student/pages/resume-builder/AICVBuilderWorkspace.tsx
EducationSystem/education-system-ui/src/app/router.tsx
EducationSystem/education-system-ui/src/features/student/StudentLayout.tsx
EducationSystem/education-system-ui/src/index.css
EducationSystem/education-system-ui/.env.example
```

### Kiểm thử

```text
EducationSystem/tests/StudentAccess.Tests/InternshipVerificationWorkflowTests.cs
EducationSystem/tests/StudentAccess.Tests/StudentInternshipSyncTests.cs
```

## 9. Kết quả kiểm tra

Các kiểm tra đã chạy trong quá trình triển khai:

- Backend: toàn bộ `19/19` test liên quan vượt qua.
- Frontend: `12/12` test vượt qua.
- TypeScript build và Vite production build thành công.
- Lint không phát sinh lỗi mới từ phần AI Resume Builder.
- Vite còn cảnh báo bundle JavaScript lớn hơn `500 kB`; đây là cảnh báo tối ưu code splitting, không làm build thất bại.
- Các cảnh báo lint còn lại thuộc các tệp có sẵn như `router.tsx`, `studentAuth.tsx` và `StudentPages.tsx`.

## 10. Trạng thái hiện tại và bước tiếp theo

Luồng hiện tại đã hoàn chỉnh và sử dụng email SMTP thật:

```text
Sinh viên gửi yêu cầu
→ Email token cho mentor
→ Doanh nghiệp xác nhận/từ chối
→ Nhà trường phê duyệt
→ CommunicationService đồng bộ sang AcademicService
→ StudentInternships lưu hồ sơ chính thức
→ CV Builder tải danh sách đã duyệt
→ Sinh viên chọn thực tập
→ A4 Preview cập nhật và cho phép chỉnh sửa trực tiếp
→ In/Xuất PDF
```

Phần chưa triển khai thực tế là LLM tối ưu nội dung. Scaffold API vẫn được giữ lại nhưng mặc định bị tắt bằng `VITE_AI_OPTIMIZE_MODE=mock`.
