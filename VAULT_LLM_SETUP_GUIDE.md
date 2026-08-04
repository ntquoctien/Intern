# Hướng Dẫn Kết Nối Vault LLM API cho Tính Năng Tối Ưu Hóa CV

**Ngày cập nhật**: 2026-08-01  
**Mục đích**: Thiết lập Vault LLM để tối ưu hóa CV mà không lộ API key trong source code

---

## 1. Tổng Quan Kiến Trúc

### Flow Tối Ưu Hóa CV
```
Frontend (React)
    ↓
POST /api/career/resume/optimize
    ↓
ResumeOptimizationController
    ↓
ResumeContextHydrationService (lấy dữ liệu từ VectorMatch + Academic)
    ↓
LlmResumeGeneratorService (gọi Vault LLM)
    ↓
Vault API (https://newapi.vault.io.vn/v1/chat/completions)
    ↓
Trả về JSON CV đã tối ưu
```

### Cấu Trúc Cấu Hình
Hệ thống sử dụng **hai mức cấu hình**:

1. **ResumeLLM** (ưu tiên): Cấu hình riêng cho Resume Optimization
2. **VaultLLM** (fallback): Cấu hình dự phòng cho Vault khi ResumeLLM dùng provider Vault

---

## 2. Bước 1: Xác Minh API Vault có Hoạt Động

Trước khi cấu hình, hãy test API Vault trực tiếp:

```powershell
# Windows PowerShell - Test kết nối Vault LLM
$vaultUrl = "https://newapi.vault.io.vn/v1/chat/completions"
$vaultApiKey = "${VAULT_API_KEY}"  # Thay bằng key thực
$model = "gpt-5.6-sol"  # Hoặc model khác từ Vault

$headers = @{
    "Authorization" = "Bearer $vaultApiKey"
    "Content-Type" = "application/json"
}

$payload = @{
    model = $model
    messages = @(
        @{ role = "system"; content = "Bạn là trợ lý tiếng Việt." }
        @{ role = "user"; content = "Xin chào, hãy trả lời bằng tiếng Việt." }
    )
    temperature = 0
    response_format = @{ type = "json_object" }
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri $vaultUrl -Method POST -Headers $headers -Body $payload -TimeoutSec 120
}

---

## 4. Bước 3: Cấu Hình ResumeLLM trong appsettings.json

Mở `src/Services/CareerService/appsettings.json` và cấu hình:

### Tùy Chọn 1: Dùng Vault cho Resume Optimization (Khuyến Nghị)

```json
{
  "ResumeLLM": {
    "Provider": "Vault",
    "Model": "gpt-5.6-sol",
    "ApiKey": "",
    "BaseUrl": "https://newapi.vault.io.vn/v1",
    "TimeoutSeconds": 300,
    "MaxRetries": 2,
    "MaxInputTokensPerRequest": 12000
  }
}
```

**Giải thích**:
- `Provider`: "Vault" → gọi `SendOpenAiCompatibleAsync` với Vault endpoint
- `Model`: "kimi-k3" → model Vault (điều chỉnh theo model có sẵn)
- `ApiKey`: "" → **ĐỂ TRỐNG**, sẽ lấy từ user-secrets
- `BaseUrl`: Vault API endpoint
- `TimeoutSeconds`: 300 (tối ưu hóa CV có thể mất thời gian)
- `MaxRetries`: 2 (retry tự động nếu LLM tạm thời không khả dụng)

### Tùy Chọn 2: Dùng Provider Khác (Groq, Gemini, OpenAI)

Nếu muốn dùng provider khác thay vì Vault:

```json
{
  "ResumeLLM": {
    "Provider": "Groq",
    "Model": "mixtral-8x7b-32768",
    "ApiKey": "",
    "TimeoutSeconds": 60,
    "MaxRetries": 2,
    "MaxInputTokensPerRequest": 12000
  }
}
```

---

## 5. Bước 4: Xác Minh Cấu Hình trong Code

File `Program.cs` (dòng 69-110) tự động:
1. Đọc `ResumeLLM` từ appsettings.json
2. PostConfigure: ghi đè giá trị trống bằng user-secrets
3. Nếu Provider = "Vault", lấy thêm `VaultLLM:BaseUrl` và `VaultLLM:ApiKey`

**Không cần chỉnh sửa code** - hệ thống tự xử lý!

---

## 6. Bước 5: Build & Run Hệ Thống

```powershell
cd "d:\_intern\Intern\EducationSystem\src\Services\CareerService"
dotnet build
dotnet run --launch-profile "http"
```

**Kiểm tra**: Swagger accessible tại `http://localhost:5005/swagger`

---

## 7. Bước 6: Test Endpoints

### Test Resume Optimization

```powershell
$studentId = "YOUR_STUDENT_ID"
$jwtToken = "YOUR_STUDENT_JWT_TOKEN"

$headers = @{
    "Authorization" = "Bearer $jwtToken"
    "Content-Type" = "application/json"
}

$payload = @{
    StudentId = $studentId
    TargetRole = "Backend Developer"
    JobDescription = "Backend developer với C#, ASP.NET Core"
    SelectedSubjectIds = @()
    UiProjects = @()
    CurrentSummaryDraft = "Sinh viên kỹ sư phần mềm"
    Certifications = @("AWS Solutions Architect")
    AwardsAndActivities = @("Hackathon Winner 2024")
    TopK = 5
    SimilarityThreshold = 0.5
} | ConvertTo-Json

try {
    Write-Host "🔄 Calling Vault LLM for CV optimization..."
    $response = Invoke-WebRequest -Uri "http://localhost:5005/api/career/resume/optimize" `
        -Method POST -Headers $headers -Body $payload -TimeoutSec 120
    Write-Host "✅ Resume optimization successful!"
    Write-Host $response.Content | ConvertFrom-Json | ConvertTo-Json -Depth 5
} catch {
    Write-Host "❌ Error: $_"
}
```

---

## 8. Xử Lý Lỗi Thường Gặp

| Lỗi | Nguyên Nhân | Giải Pháp |
|-----|-----------|----------|
| `RESUME_LLM_NOT_CONFIGURED` | ApiKey hoặc Model bị trống | Kiểm tra `dotnet user-secrets list` |
| `RESUME_LLM_RATE_LIMIT` | Vault API quá nhiều request | Giảm số sinh viên, hoặc đợi |
| `RESUME_LLM_REQUEST_FAILED` | Vault endpoint không thể truy cập | Kiểm tra URL, firewall, API key |
| HTTP 500 Internal Server Error | Lỗi không được handle | Xem chi tiết trong server log |

---

## 9. Danh Sách Kiểm Tra (Checklist)

- [ ] API Vault được test thành công
- [ ] API Key lưu vào `dotnet user-secrets`
- [ ] `appsettings.json` cấu hình `ResumeLLM` với Provider = "Vault"
- [ ] Build & run không có lỗi
- [ ] Test optimize endpoint thành công
- [ ] CV được sinh ra bằng tiếng Việt
- [ ] Firewall cho phép truy cập Vault API

    Write-Host "✅ Vault LLM đang hoạt động!"
    Write-Host "Status Code: $($response.StatusCode)"
    $content = $response.Content | ConvertFrom-Json
    Write-Host "Response: $($content.choices[0].message.content)"
} catch {
    Write-Host "❌ Lỗi kết nối Vault: $_"
}
```

**Kết quả mong đợi**: Status 200 với JSON response hợp lệ

---

## 3. Bước 2: Lưu API Key vào User Secrets (Không Lộ trong Source Code)

### Phương Pháp A: Dùng dotnet user-secrets (Khuyến Nghị)

User Secrets lưu trữ API key ở mức máy local, không commit vào Git.

```powershell
# Mở PowerShell tại thư mục dự án
cd "d:\_intern\Intern\EducationSystem\src\Services\CareerService"

# Lưu Vault API Key vào user secrets
dotnet user-secrets set "VaultLLM:ApiKey" "YOUR_VAULT_API_KEY_HERE" --project CareerService.csproj

# (Tùy chọn) Lưu cấu hình Vault khác nếu cần
dotnet user-secrets set "VaultLLM:Model" "kimi-k3" --project CareerService.csproj
dotnet user-secrets set "VaultLLM:BaseUrl" "https://newapi.vault.io.vn/v1" --project CareerService.csproj

# Xác minh đã lưu
dotnet user-secrets list --project CareerService.csproj
```

**Kết quả mong đợi**:
```
VaultLLM:ApiKey = ***
VaultLLM:Model = kimi-k3
VaultLLM:BaseUrl = https://newapi.vault.io.vn/v1
```

### Phương Pháp B: Dùng appsettings.{Environment}.json (Development)

Chỉ dùng cho development. Trên production, luôn dùng environment variables.

1. Tạo/chỉnh sửa `appsettings.Development.json`:
```json
{
  "VaultLLM": {
    "ApiKey": "YOUR_VAULT_API_KEY_HERE"
  }
}
```

2. **Đừng commit file này vào Git** - thêm vào `.gitignore`



[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

