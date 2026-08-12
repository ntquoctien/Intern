# Cấu hình email xác thực thực tập

Luồng xác thực thực tập không còn sử dụng mock sender. `CommunicationService`
phải có SMTP hợp lệ trước khi khởi động.

## Development với Gmail SMTP

Tài khoản Gmail cần bật xác minh hai bước và sử dụng App Password, không dùng
mật khẩu đăng nhập thông thường.

Chạy các lệnh sau từ thư mục gốc repository và thay giá trị ví dụ:

```powershell
dotnet user-secrets set "InternshipEmail:SmtpHost" "smtp.gmail.com" --project EducationSystem/src/Services/CommunicationService
dotnet user-secrets set "InternshipEmail:SmtpPort" "587" --project EducationSystem/src/Services/CommunicationService
dotnet user-secrets set "InternshipEmail:EnableSsl" "true" --project EducationSystem/src/Services/CommunicationService
dotnet user-secrets set "InternshipEmail:FromAddress" "your-account@gmail.com" --project EducationSystem/src/Services/CommunicationService
dotnet user-secrets set "InternshipEmail:Username" "your-account@gmail.com" --project EducationSystem/src/Services/CommunicationService
dotnet user-secrets set "InternshipEmail:Password" "your-16-character-app-password" --project EducationSystem/src/Services/CommunicationService
```

Trong development, link trong email mặc định trỏ tới:

```text
http://localhost:5173/verify-internship?token=...
```

## Khóa gọi nội bộ giữa hai service

Tạo một chuỗi ngẫu nhiên mạnh và cấu hình cùng một giá trị cho cả hai service:

```powershell
$internalApiKey = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
dotnet user-secrets set "InternalApi:Key" $internalApiKey --project EducationSystem/src/Services/CommunicationService
dotnet user-secrets set "InternalApi:Key" $internalApiKey --project EducationSystem/src/Services/AcademicService
```

Khóa này bảo vệ:

- Đọc snapshot tên/MSSV từ AcademicService trước khi gửi email.
- Đồng bộ đợt thực tập chính thức sau khi Admin duyệt.

## Frontend

Tạo hoặc cập nhật `EducationSystem/education-system-ui/.env.local`:

```env
VITE_COMMUNICATION_API_ORIGIN=http://localhost:5004
VITE_ACADEMIC_API_ORIGIN=http://localhost:5002
VITE_AI_OPTIMIZE_MODE=mock
```

Không còn biến `VITE_INTERNSHIP_API_MODE`; các màn hình thực tập luôn gọi API thật.

## Khởi động

Khởi động tối thiểu các service theo thứ tự:

```powershell
dotnet run --project EducationSystem/src/Services/IdentityService --launch-profile http
dotnet run --project EducationSystem/src/Services/AcademicService --launch-profile http
dotnet run --project EducationSystem/src/Services/CommunicationService --launch-profile http
npm run dev --prefix EducationSystem/education-system-ui
```

Nếu thiếu SMTP, `CommunicationService` sẽ dừng ngay khi khởi động với lỗi cấu
hình thay vì âm thầm ghi email giả ra console.

## Cấu hình production

Không commit password vào `appsettings.json`. Sử dụng environment variables hoặc
secret store của môi trường triển khai:

```text
InternshipEmail__PortalBaseUrl=https://portal.tdu.edu.vn
InternshipEmail__SmtpHost=<smtp-host>
InternshipEmail__SmtpPort=587
InternshipEmail__EnableSsl=true
InternshipEmail__FromAddress=<sender-address>
InternshipEmail__FromName=Cổng sinh viên Cao Đắng Tây Đô
InternshipEmail__Username=<smtp-user>
InternshipEmail__Password=<smtp-secret>
InternalApi__Key=<shared-internal-key>
```

Sau khi thay đổi User Secrets hoặc environment variables, cần khởi động lại
`CommunicationService`.
