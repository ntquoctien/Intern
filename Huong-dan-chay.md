# Hướng dẫn thiết lập và khởi chạy EducationSystem

> Hướng dẫn này dành cho môi trường phát triển Windows/PowerShell và được đối chiếu với mã nguồn ngày 04/08/2026.

## 1. Thành phần được khởi chạy

| Thành phần | Địa chỉ local |
| --- | --- |
| Frontend React/Vite | `http://127.0.0.1:5173` |
| IdentityService | `http://localhost:5001` |
| AcademicService | `http://localhost:5002` |
| ExamService | `http://localhost:5003` |
| CommunicationService | `http://localhost:5004` |
| CareerService | `http://localhost:5005` |
| VectorMatchService | `http://localhost:5006` — tùy chọn, không được script hiện tại tự khởi động |

`Start-Dev.ps1` chạy năm service .NET và frontend. Nếu VectorMatchService không có, chức năng CV vẫn đề xuất môn theo điểm số.

## 2. Yêu cầu môi trường

Cài đặt các công cụ sau:

- Windows 10/11 và PowerShell 5.1 trở lên.
- .NET SDK 8.
- Node.js 20 trở lên và npm.
- SQL Server và công cụ quản trị như SQL Server Management Studio.
- Git nếu lấy mã nguồn từ repository.
- Tùy chọn: `sqlcmd` nếu muốn chạy script SQL bằng terminal.
- API key của nhà cung cấp AI nếu dùng tối ưu CV hoặc import CLO/PLO.

Kiểm tra nhanh:

```powershell
dotnet --version
node --version
npm --version
```

Các phiên bản đang dùng được xác nhận trên máy phát triển hiện tại là .NET SDK `8.0.423`, Node.js `22.19.0` và npm `10.9.3`. Không bắt buộc khớp tuyệt đối, nhưng backend phải dùng .NET 8 và frontend phải đáp ứng yêu cầu của Vite 8.

## 3. Thư mục làm việc

Mở PowerShell và chuyển vào repository:

```powershell
cd D:\_intern\Intern\EducationSystem
```

Nếu repository được đặt ở vị trí khác, thay đường dẫn trên bằng đường dẫn thực tế. Tất cả lệnh còn lại trong tài liệu này giả định terminal đang đứng tại thư mục chứa `EducationSystem.sln`.

## 4. Chuẩn bị cơ sở dữ liệu

### 4.1. Lưu ý quan trọng về migration

Các migration baseline của IdentityService, AcademicService, ExamService và CommunicationService hiện có phương thức `Up()` rỗng. Chúng đánh dấu một database đã tồn tại, không tạo toàn bộ bảng nghiệp vụ từ đầu.

Vì vậy, để chạy đầy đủ hệ thống cần một trong các nguồn sau:

- Database `TayDoV2` đã được cung cấp/seed sẵn; hoặc
- File backup của database và restore vào SQL Server; hoặc
- Bộ script tạo schema Identity, Academic, Exam và Communication từ người quản trị database.

Chỉ chạy `dotnet ef database update` trên database trống sẽ không đủ để tạo hệ thống.

Hai bảng phục vụ CV là `academic.StudentProjects` và `academic.StudentInternships` đã có trong EF model nhưng không có trong migration baseline hiện tại. Phải bảo đảm database đích đã có hai bảng này trước khi sử dụng CV thông minh.

### 4.2. Tạo file connection string dùng chung

Repository cung cấp file mẫu:

```text
connectionstrings.Development.example.json
```

Tạo bản local:

```powershell
Copy-Item .\connectionstrings.Development.example.json .\connectionstrings.Development.json
```

Sửa `connectionstrings.Development.json`. Ví dụ dùng Windows Authentication:

```json
{
  "ConnectionStrings": {
    "IdentityDb": "Server=localhost;Database=TayDoV2;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True",
    "AcademicDb": "Server=localhost;Database=TayDoV2;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True",
    "ExamDb": "Server=localhost;Database=TayDoV2;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True",
    "CommunicationDb": "Server=localhost;Database=TayDoV2;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True",
    "CareerDb": "Server=localhost;Database=TayDoV2;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
  }
}
```

Nếu dùng SQL Authentication, thay bằng `User Id` và `Password` tương ứng. Không commit file chứa mật khẩu. File `connectionstrings.Development.json` đã được loại khỏi Git bởi `.gitignore` của workspace.

Mỗi service tự tìm file này từ thư mục chứa `EducationSystem.sln` khi chạy với môi trường `Development`.

### 4.3. Tạo schema Career

Các script Career chỉ chấp nhận database có tên chính xác `TayDoV2`. Với database mới đã có các schema nền, chạy theo đúng thứ tự sau.

Nhóm bảng chuẩn đầu ra:

```text
database/scripts/career/001_create_career_schema.sql
database/scripts/career/002_create_career_tables.sql
database/scripts/career/003_seed_progression_levels.sql
database/scripts/career/004_create_career_indexes.sql
database/scripts/career/005_verify_career_schema.sql
```

Nhóm bảng import/kiểm duyệt:

```text
database/scripts/career-import/001_create_import_tables.sql
database/scripts/career-import/002_create_import_constraints.sql
database/scripts/career-import/003_create_import_indexes.sql
database/scripts/career-import/004_add_subject_selection.sql
database/scripts/career-import/005_verify_import_schema.sql
database/scripts/career-import/006_add_approved_outcome_document.sql
```

Có thể mở từng file trong SSMS và chạy theo thứ tự. Nếu dùng Windows Authentication và `sqlcmd`:

```powershell
sqlcmd -S "localhost" -d "TayDoV2" -E -b -f 65001 -i ".\database\scripts\career\001_create_career_schema.sql"
sqlcmd -S "localhost" -d "TayDoV2" -E -b -f 65001 -i ".\database\scripts\career\002_create_career_tables.sql"
sqlcmd -S "localhost" -d "TayDoV2" -E -b -f 65001 -i ".\database\scripts\career\003_seed_progression_levels.sql"
sqlcmd -S "localhost" -d "TayDoV2" -E -b -f 65001 -i ".\database\scripts\career\004_create_career_indexes.sql"
sqlcmd -S "localhost" -d "TayDoV2" -E -b -f 65001 -i ".\database\scripts\career\005_verify_career_schema.sql"

sqlcmd -S "localhost" -d "TayDoV2" -E -b -f 65001 -i ".\database\scripts\career-import\001_create_import_tables.sql"
sqlcmd -S "localhost" -d "TayDoV2" -E -b -f 65001 -i ".\database\scripts\career-import\002_create_import_constraints.sql"
sqlcmd -S "localhost" -d "TayDoV2" -E -b -f 65001 -i ".\database\scripts\career-import\003_create_import_indexes.sql"
sqlcmd -S "localhost" -d "TayDoV2" -E -b -f 65001 -i ".\database\scripts\career-import\004_add_subject_selection.sql"
sqlcmd -S "localhost" -d "TayDoV2" -E -b -f 65001 -i ".\database\scripts\career-import\005_verify_import_schema.sql"
sqlcmd -S "localhost" -d "TayDoV2" -E -b -f 65001 -i ".\database\scripts\career-import\006_add_approved_outcome_document.sql"
```

Không chạy các file `rollback_*.sql` trong quá trình setup. Chúng có tính phá hủy và chỉ dùng khi chủ động gỡ schema.

## 5. Cấu hình AI cho CareerService

### 5.1. Tối ưu CV

Cấu hình mặc định hiện tại:

```text
Provider: Vault
Model: gpt-5.6-sol
Base URL: https://newapi.vault.io.vn/v1
```

Không ghi API key thật vào `appsettings.json`. Trong cửa sổ PowerShell sẽ chạy hệ thống, đặt biến môi trường:

```powershell
$env:ResumeLLM__Provider = "Vault"
$env:ResumeLLM__Model = "gpt-5.6-sol"
$env:ResumeLLM__BaseUrl = "https://newapi.vault.io.vn/v1"
$env:ResumeLLM__ApiKey = "YOUR_API_KEY"
```

Các biến trên chỉ tồn tại trong cửa sổ PowerShell hiện tại. Phải chạy `Start-Dev.ps1` trong cùng cửa sổ để process CareerService kế thừa chúng.

### 5.2. Import CLO/PLO

Pipeline CLO/PLO dùng section `LLM`, độc lập với `ResumeLLM`. Ví dụ dùng Groq theo cấu hình mặc định:

```powershell
$env:LLM__Provider = "Groq"
$env:LLM__Model = "qwen/qwen3.6-27b"
$env:LLM__ApiKey = "YOUR_API_KEY"
```

Nếu xử lý PDF bằng OCR, code hiện chỉ hỗ trợ `Gemini` hoặc `Vault` cho `PdfOcr:Provider`. Không đặt provider OCR là `Groq` dù file cấu hình mặc định cũ còn giá trị đó.

Ví dụ:

```powershell
$env:PdfOcr__Enabled = "true"
$env:PdfOcr__Provider = "Vault"
$env:PdfOcr__Model = "gpt-5.6-sol"
$env:VaultLLM__ApiKey = "YOUR_API_KEY"
```

Nếu chọn `PdfOcr__Provider = "Gemini"`, cấu hình khóa bằng
`PdfOcr__ApiKey` thay cho `VaultLLM__ApiKey`.

Nếu không dùng import CLO/PLO hoặc OCR trong phiên chạy, có thể bỏ qua các biến của mục này.

## 6. Cài dependency

### 6.1. Backend

Khôi phục NuGet packages và build solution:

```powershell
dotnet restore .\EducationSystem.sln
dotnet build .\EducationSystem.sln --no-restore
```

`Start-BackendServices.ps1` build với `--no-restore`, vì vậy lần setup đầu tiên cần chạy `dotnet restore` trước.

### 6.2. Frontend

```powershell
cd .\education-system-ui
npm ci
cd ..
```

`npm ci` cài đúng dependency theo `package-lock.json`.

## 7. Cấu hình frontend

Tạo file `education-system-ui/.env.local` nếu cần ghi đè URL:

```dotenv
VITE_IDENTITY_API_BASE=http://localhost:5001/api/identity
VITE_ACADEMIC_API_BASE=http://localhost:5002/api/academic
VITE_EXAM_API_BASE=http://localhost:5003/api/exam
VITE_COMMUNICATION_API_BASE=http://localhost:5004/api/communication

VITE_IDENTITY_API_ORIGIN=http://localhost:5001
VITE_ACADEMIC_API_ORIGIN=http://localhost:5002
VITE_EXAM_API_ORIGIN=http://localhost:5003
VITE_COMMUNICATION_API_ORIGIN=http://localhost:5004
VITE_CAREER_API_ORIGIN=http://localhost:5005
VITE_AI_API_ORIGIN=http://localhost:5005
```

Các URL mặc định trong code đã trỏ đến các port trên, nên môi trường local chuẩn có thể chạy mà không cần `.env.local`.

Không cần cấu hình `VITE_RESUME_API_MODE` hoặc `VITE_AI_OPTIMIZE_MODE`; các cờ mock này trong `.env.example` là nội dung cũ và không còn điều khiển wizard CV hiện tại.

Sau khi sửa file `.env.local`, phải restart Vite.

## 8. Khởi chạy hệ thống

### 8.1. Cách khuyến nghị: chạy tất cả

Từ thư mục `EducationSystem`, sau khi đặt các biến API key cần thiết:

```powershell
.\scripts\Start-Dev.ps1
```

Script sẽ:

1. Tạo thư mục `.run`.
2. Sinh hoặc dùng lại signing key JWT local trong `.run/student-jwt-signing-key`.
3. Build và chạy năm service .NET trên port 5001–5005.
4. Ghi PID, stdout và stderr vào `.run`.
5. Chạy frontend tại `http://127.0.0.1:5173`.

Cửa sổ PowerShell này sẽ tiếp tục chạy Vite. Dùng `Ctrl+C` để dừng frontend; các service backend vẫn chạy nền.

### 8.2. Chỉ chạy backend

```powershell
.\scripts\Start-BackendServices.ps1
```

Nếu chắc chắn binary đã được build và chỉ muốn khởi động nhanh:

```powershell
.\scripts\Start-BackendServices.ps1 -NoBuild
```

Không dùng `-NoBuild` sau khi vừa sửa code backend.

### 8.3. Chỉ chạy frontend

```powershell
cd .\education-system-ui
npm run dev -- --host 127.0.0.1
```

## 9. Kiểm tra sau khi khởi động

### 9.1. Swagger và health

Mở các địa chỉ:

```text
http://localhost:5001/swagger
http://localhost:5002/swagger
http://localhost:5003/swagger
http://localhost:5004/swagger
http://localhost:5005/swagger
```

Health endpoints:

```text
http://localhost:5001/api/identity/health
http://localhost:5002/api/academic/health
http://localhost:5003/api/exam/health
http://localhost:5004/api/communication/health
http://localhost:5005/health
```

Kiểm tra bằng PowerShell:

```powershell
Invoke-WebRequest http://localhost:5001/api/identity/health -UseBasicParsing
Invoke-WebRequest http://localhost:5002/api/academic/health -UseBasicParsing
Invoke-WebRequest http://localhost:5003/api/exam/health -UseBasicParsing
Invoke-WebRequest http://localhost:5004/api/communication/health -UseBasicParsing
Invoke-WebRequest http://localhost:5005/health -UseBasicParsing
```

### 9.2. Kiểm tra frontend

Mở:

```text
http://127.0.0.1:5173
```

Đăng nhập sinh viên tại:

```text
http://127.0.0.1:5173/student/login
```

CV thông minh:

```text
http://127.0.0.1:5173/student/resume-builder
```

Tài khoản sinh viên phải tồn tại trong `identity.Users` và ánh xạ được tới `academic.Students`; hệ thống không tự sinh tài khoản demo khi startup.

### 9.3. Kiểm tra model CV

Sau khi bấm tối ưu CV, log CareerService phải có:

```text
Generating optimized resume with provider Vault and model gpt-5.6-sol.
```

Nếu vẫn thấy model cũ, process CareerService chưa được restart hoặc biến môi trường/cấu hình khác đang ghi đè.

## 10. Theo dõi log

Theo dõi lỗi CareerService:

```powershell
.\scripts\Watch-CareerServiceLogs.ps1
```

Xem cả warning:

```powershell
.\scripts\Watch-CareerServiceLogs.ps1 -Level Warning
```

Xem toàn bộ log:

```powershell
.\scripts\Watch-CareerServiceLogs.ps1 -Level All
```

Log của từng service nằm trong `.run`:

```text
.run/IdentityService.out.log
.run/IdentityService.err.log
.run/AcademicService.out.log
.run/AcademicService.err.log
.run/ExamService.out.log
.run/ExamService.err.log
.run/CommunicationService.out.log
.run/CommunicationService.err.log
.run/CareerService.out.log
.run/CareerService.err.log
```

## 11. Dừng và khởi động lại

Dừng năm backend service:

```powershell
.\scripts\Stop-BackendServices.ps1
```

Script này không dừng frontend Vite. Dừng Vite bằng `Ctrl+C` trong cửa sổ đang chạy frontend.

Sau khi thay đổi backend hoặc cấu hình AI, thực hiện restart sạch:

```powershell
.\scripts\Stop-BackendServices.ps1
.\scripts\Start-BackendServices.ps1
```

`Start-BackendServices.ps1` bỏ qua service đang chạy và còn phản hồi Swagger. Vì vậy, chạy lại script mà không dừng process cũ có thể khiến code/cấu hình mới chưa được nạp.

## 12. Lỗi thường gặp

### 12.1. Build báo thiếu package

```powershell
dotnet restore .\EducationSystem.sln
dotnet build .\EducationSystem.sln --no-restore
```

Với frontend:

```powershell
cd .\education-system-ui
npm ci
npm run build
```

### 12.2. Service không kết nối được SQL Server

Kiểm tra:

- SQL Server service đang chạy.
- Tên server/instance trong `connectionstrings.Development.json` đúng.
- Database `TayDoV2` tồn tại.
- Tài khoản có quyền đọc schema tương ứng; CareerService cần quyền ghi schema `career` khi import/approve.
- `TrustServerCertificate=True` có trong cấu hình local nếu chứng chỉ SQL Server chưa được tin cậy.
- Các bảng nền đã có; migration baseline không tự tạo chúng.

### 12.3. Port 5001–5005 đã được sử dụng

Xem process giữ port:

```powershell
Get-NetTCPConnection -LocalPort 5001,5002,5003,5004,5005 -State Listen |
  Select-Object LocalPort,OwningProcess
```

Trước tiên chạy script dừng của dự án:

```powershell
.\scripts\Stop-BackendServices.ps1
```

Không kết thúc process không thuộc EducationSystem khi chưa xác định rõ.

### 12.4. `Failed to determine the https port for redirect`

Đây là cảnh báo do service chạy profile HTTP nhưng middleware HTTPS redirection vẫn được bật. Nếu Swagger và health HTTP hoạt động thì cảnh báo này không phải nguyên nhân của lỗi database hoặc AI.

### 12.5. VectorMatchService báo connection refused

Port 5006 không được `Start-Dev.ps1` khởi động. Trong repository hiện tại không có implementation có thể chạy của VectorMatchService.

- CareerService sẽ ghi warning và dùng danh sách môn theo điểm.
- Sinh viên vẫn phải thấy tối đa 10 môn đủ điều kiện có điểm từ 7.0.
- Không cần port 5006 để gọi model tối ưu CV.

### 12.6. Tối ưu CV trả `502 Bad Gateway`

Kiểm tra:

1. `ResumeLLM__ApiKey` đã được đặt trong đúng cửa sổ PowerShell trước khi start.
2. Log xác nhận `Vault / gpt-5.6-sol`.
3. CareerService đã restart sau khi đổi model/API key.
4. AcademicService đang chạy và JWT sinh viên còn hạn.
5. Provider không trả `401`, rate limit, timeout hoặc JSON sai schema.

Frontend chờ tối đa 310 giây; backend mặc định chờ LLM tối đa 300 giây.

### 12.7. Không thấy môn học trong CV Builder

- Kiểm tra AcademicService trả `eligibleCourses` tại API resume context.
- Chỉ môn có điểm trung bình từ 7.0 mới đủ điều kiện.
- Bảo đảm database có dữ liệu `StudentEvaluations` và liên kết môn đúng.
- Restart AcademicService nếu vừa thay code.
- VectorMatch lỗi phải chuyển sang score fallback, không được làm danh sách môn về 0.

### 12.8. CV hiển thị toàn bộ dự án thay vì dự án đã chọn

Trong Network tab, request `/api/career/resume/optimize` phải gửi `uiProjects` chỉ gồm dự án đã tick. Nếu không chọn dự án nào, field phải là `[]`, không phải `null` hoặc bị bỏ khỏi payload.

### 12.9. Frontend không gọi được API hoặc lỗi CORS

- Truy cập frontend bằng `localhost` hoặc `127.0.0.1`; CORS Development cho phép hai host này.
- Kiểm tra URL trong `.env.local`.
- Restart Vite sau khi sửa biến môi trường.
- Kiểm tra từng Swagger trước khi debug frontend.

## 13. Kiểm tra trước khi bàn giao

- [ ] SQL Server và database `TayDoV2` hoạt động.
- [ ] Có đủ schema/bảng Identity, Academic, Exam và Communication.
- [ ] Schema Career và bảng import đã được tạo/verify.
- [ ] Có `academic.StudentProjects` và `academic.StudentInternships` nếu dùng CV.
- [ ] `connectionstrings.Development.json` đã cấu hình và không commit.
- [ ] Đã chạy `dotnet restore` và build solution thành công.
- [ ] Đã chạy `npm ci` và frontend build thành công.
- [ ] Năm Swagger/health endpoint phản hồi.
- [ ] Frontend mở được tại port 5173.
- [ ] Sinh viên đăng nhập được và token dùng chung giữa các service.
- [ ] Log tối ưu CV xác nhận `Vault / gpt-5.6-sol`.
- [ ] Đã kiểm tra fallback môn học khi port 5006 không hoạt động.

## 14. Lệnh nhanh cho những lần chạy sau

```powershell
cd D:\_intern\Intern\EducationSystem

$env:ResumeLLM__Provider = "Vault"
$env:ResumeLLM__Model = "gpt-5.6-sol"
$env:ResumeLLM__BaseUrl = "https://newapi.vault.io.vn/v1"
$env:ResumeLLM__ApiKey = "YOUR_API_KEY"

.\scripts\Start-Dev.ps1
```

Dừng backend:

```powershell
cd D:\_intern\Intern\EducationSystem
.\scripts\Stop-BackendServices.ps1
```

Không lưu API key thật vào file hướng dẫn, lịch sử Git hoặc ảnh chụp màn hình log.
