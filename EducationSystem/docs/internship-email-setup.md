# Cấu hình môi trường Development – xác thực thực tập

Tài liệu này dành cho thành viên mới clone repository và chạy toàn bộ luồng:

1. Sinh viên đăng nhập và gửi yêu cầu thực tập.
2. CommunicationService gửi email cho Mentor.
3. Mentor xác nhận qua liên kết công khai.
4. Admin phê duyệt.
5. CommunicationService đồng bộ kỳ thực tập sang AcademicService.

Không commit mật khẩu database, SMTP password, JWT key hoặc internal API key vào Git.

## 1. Yêu cầu cài đặt

- .NET SDK 8
- SQL Server
- Node.js và npm
- Gmail đã bật xác minh hai bước nếu dùng Gmail SMTP
- Gmail App Password 16 ký tự; không dùng mật khẩu Gmail thông thường

Các lệnh dưới đây giả định terminal đang đứng tại thư mục chứa `EducationSystem`.

## 2. Cấu hình kết nối SQL Server

Sao chép file mẫu:

```powershell
Copy-Item `
  "EducationSystem/connectionstrings.Development.example.json" `
  "EducationSystem/connectionstrings.Development.json"
```

Mở `connectionstrings.Development.json` và thay:

- `YOUR_SQL_SERVER`: tên SQL Server instance.
- `YOUR_DATABASE`: tên database vật lý.
- Username/password của từng schema user.

File `connectionstrings.Development.json` đã được `.gitignore` và không được commit.

Chạy script:

```text
EducationSystem/scripts/20260728-ai-resume-builder-phase1.sql
```

trên đúng database đã cấu hình. Script có thể chạy bằng SSMS/Azure Data Studio hoặc:

```powershell
sqlcmd -S "YOUR_SQL_SERVER" -d "YOUR_DATABASE" -E -C `
  -i "EducationSystem/scripts/20260728-ai-resume-builder-phase1.sql"
```

## 3. Tạo khóa dùng chung tương thích Windows PowerShell

Không dùng cú pháp tĩnh:

```powershell
[Security.Cryptography.RandomNumberGenerator]::GetBytes(48)
```

vì một số phiên bản Windows PowerShell/.NET không hỗ trợ overload này.

Chạy nguyên khối sau trong **cùng một cửa sổ PowerShell**:

```powershell
function New-SecureBase64Key {
    $bytes = New-Object byte[] 48
    $rng = [Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $rng.GetBytes($bytes)
    }
    finally {
        $rng.Dispose()
    }
    [Convert]::ToBase64String($bytes)
}

$studentJwtKey = New-SecureBase64Key
$internalApiKey = New-SecureBase64Key

$studentJwtProjects = @(
    "EducationSystem/src/Services/IdentityService",
    "EducationSystem/src/Services/AcademicService",
    "EducationSystem/src/Services/ExamService",
    "EducationSystem/src/Services/CommunicationService"
)

foreach ($project in $studentJwtProjects) {
    dotnet user-secrets set "StudentJwt:SigningKey" $studentJwtKey `
        --project $project
}

dotnet user-secrets set "InternalApi:Key" $internalApiKey `
    --project "EducationSystem/src/Services/AcademicService"

dotnet user-secrets set "InternalApi:Key" $internalApiKey `
    --project "EducationSystem/src/Services/CommunicationService"
```

Quy tắc bắt buộc:

- `StudentJwt:SigningKey` phải giống nhau ở cả 4 service.
- `InternalApi:Key` phải giống nhau ở AcademicService và CommunicationService.
- Mỗi máy developer có thể dùng bộ khóa riêng.
- Thay đổi User Secrets xong phải restart service.

## 4. Cấu hình Gmail SMTP

Thay các giá trị ví dụ trước khi chạy:

```powershell
$communicationProject = "EducationSystem/src/Services/CommunicationService"

dotnet user-secrets set "InternshipEmail:PortalBaseUrl" `
  "http://localhost:5173" --project $communicationProject

dotnet user-secrets set "InternshipEmail:SmtpHost" `
  "smtp.gmail.com" --project $communicationProject

dotnet user-secrets set "InternshipEmail:SmtpPort" `
  "587" --project $communicationProject

dotnet user-secrets set "InternshipEmail:EnableSsl" `
  "true" --project $communicationProject

dotnet user-secrets set "InternshipEmail:FromAddress" `
  "YOUR_GMAIL@gmail.com" --project $communicationProject

dotnet user-secrets set "InternshipEmail:FromName" `
  "Cổng sinh viên Đại học Tây Đô" --project $communicationProject

dotnet user-secrets set "InternshipEmail:Username" `
  "YOUR_GMAIL@gmail.com" --project $communicationProject

dotnet user-secrets set "InternshipEmail:Password" `
  "YOUR_16_CHARACTER_APP_PASSWORD" --project $communicationProject
```

`PortalBaseUrl` quyết định domain của liên kết trong email:

```text
http://localhost:5173/verify-internship?token=...
```

## 5. Cấu hình Frontend

```powershell
Copy-Item `
  "EducationSystem/education-system-ui/.env.example" `
  "EducationSystem/education-system-ui/.env.local"
```

Giá trị Development đề nghị:

```env
VITE_IDENTITY_API_ORIGIN=http://localhost:5001
VITE_ACADEMIC_API_ORIGIN=http://localhost:5002
VITE_EXAM_API_ORIGIN=http://localhost:5003
VITE_COMMUNICATION_API_ORIGIN=http://localhost:5004
VITE_RESUME_API_MODE=live
VITE_AI_OPTIMIZE_MODE=mock
```

Sau khi đổi `.env.local`, restart Vite.

## 6. Build và khởi động

Build một lần:

```powershell
dotnet build "EducationSystem/EducationSystem.sln" -c Debug
npm install --prefix "EducationSystem/education-system-ui"
```

Mở 5 terminal và chạy:

```powershell
dotnet run --project "EducationSystem/src/Services/IdentityService" `
  -c Debug --launch-profile http
```

```powershell
dotnet run --project "EducationSystem/src/Services/AcademicService" `
  -c Debug --launch-profile http
```

```powershell
dotnet run --project "EducationSystem/src/Services/ExamService" `
  -c Debug --launch-profile http
```

```powershell
dotnet run --project "EducationSystem/src/Services/CommunicationService" `
  -c Debug --launch-profile http
```

```powershell
npm run dev --prefix "EducationSystem/education-system-ui" -- --host 127.0.0.1
```

Thứ tự khuyến nghị:

1. IdentityService `:5001`
2. AcademicService `:5002`
3. ExamService `:5003`
4. CommunicationService `:5004`
5. Frontend `:5173`

## 7. Kiểm tra nhanh

Mở Swagger:

- `http://localhost:5001/swagger`
- `http://localhost:5002/swagger`
- `http://localhost:5003/swagger`
- `http://localhost:5004/swagger`

Tài khoản sinh viên dữ liệu hiện tại:

```text
MSSV: SV000001
Mật khẩu mặc định: 1
```

Nếu đăng nhập xong bị trả về `reason=unauthorized`, kiểm tra lại
`StudentJwt:SigningKey` ở cả 4 service.

Nếu Admin duyệt nhận `502`, kiểm tra:

- AcademicService đang chạy ở `http://localhost:5002`.
- `InternalApi:Key` của Academic và Communication giống nhau.
- Đã chạy script SQL đúng database.
- Hai service đã được restart sau khi đổi User Secrets.

## 8. Production

Không sử dụng User Secrets trên server. Dùng secret manager hoặc environment variables:

```text
StudentJwt__SigningKey=<shared-jwt-key>
InternalApi__Key=<shared-internal-key>
InternshipEmail__PortalBaseUrl=https://portal.tdu.edu.vn
InternshipEmail__SmtpHost=<smtp-host>
InternshipEmail__SmtpPort=587
InternshipEmail__EnableSsl=true
InternshipEmail__FromAddress=<sender-address>
InternshipEmail__FromName=Cổng sinh viên Đại học Tây Đô
InternshipEmail__Username=<smtp-user>
InternshipEmail__Password=<smtp-secret>
```

`StudentJwt__SigningKey` phải được cấp cho mọi service xác thực JWT.
`InternalApi__Key` chỉ cấp cho AcademicService và CommunicationService.
