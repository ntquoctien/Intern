# Hướng Dẫn Cài Đặt và Khởi Chạy Local (Bare-Metal Local Setup Guide)
## Dự Án EducationSystem (Hệ Thống Quản Lý Giáo Dục)

---

## 📌 1. Bối Cảnh & Lý Do Không Sử Dụng Docker (Context & Motivation)

Dự án **EducationSystem** bao gồm 6 dịch vụ Microservices (.NET 8 & Python FastAPI) và 1 ứng dụng React Frontend.

⚠️ **LÝ DO KHÔNG SỬ DỤNG DOCKER TRONG MÔI TRƯỜNG HIỆN TẠI:**
- Do các lỗi về encoding và thiếu font tiếng Việt trong môi trường Docker Container (Linux container cơ bản), các file PDF báo cáo/CV sinh viên và log hệ thống xuất ra bị **lỗi hiển thị font chữ tiếng Việt (corrupted fonts/encoding issues)**.
- Do đó, tài liệu này hướng dẫn chi tiết quy trình **Bare-Metal Local Setup** — cài đặt và vận hành toàn bộ hệ thống trực tiếp trên hệ điều hành host (Windows/macOS/Linux) nhằm đảm bảo hiển thị chuẩn tiếng Việt, hiệu năng tối ưu và dễ dàng debug trong quá trình phát triển.

---

## 🛠️ 2. Yêu Cầu Hệ Thống (Section A: Prerequisites)

Trước khi thực hiện setup, hãy đảm bảo máy tính cá nhân của bạn đã cài đặt đầy đủ các công cụ sau:

1. **.NET SDK 8.0:**
   - Phiên bản: `.NET 8.0 SDK` (Ví dụ: `8.0.4xx`).
   - Tải về: [Microsoft .NET 8.0 Download](https://dotnet.microsoft.com/download/dotnet/8.0)
   - Kiểm tra lệnh: `dotnet --version`

2. **Node.js & npm:**
   - Phiên bản: `Node.js v20 LTS` hoặc `v22` (npm v10+).
   - Tải về: [Node.js Official Site](https://nodejs.org/)
   - Kiểm tra lệnh: `node --version`, `npm --version`

3. **Python 3.10+ (Cho VectorMatchService):**
   - Phiên bản: `Python 3.10` trở lên.
   - Kiểm tra lệnh: `python --version` hoặc `python3 --version`

4. **Microsoft SQL Server 2022 & Microsoft ODBC Driver:**
   - Phiên bản: `SQL Server 2022` (Bản Developer hoặc Express Edition) cài đặt cục bộ.
   - Yêu cầu thêm: **Microsoft ODBC Driver 18 for SQL Server** (dùng cho kết nối pyodbc từ Python VectorMatchService).
   - Công cụ quản trị: `SQL Server Management Studio (SSMS)` hoặc `sqlcmd`.

---

## 🚀 3. Quy Trình Cài Đặt Chi Tiết (Section B: Step-by-Step Local Ingestion Workflow)

---

### 🗄️ Bước 1: Khởi Tạo Cơ Sở Dữ Liệu Cục Bộ (Local Database Ingestion)

Hệ thống quản lý 47 bảng dữ liệu phân bổ trên 5 Schema chính: `identity`, `academic`, `exam`, `communication`, `career` thuộc CSDL **`TayDoV2`** (hoặc `EducationDb`).

#### 1.1. Cấu hình Connection String
Tạo tệp cấu hình local `connectionstrings.Development.json` tại thư mục `EducationSystem/`:
```powershell
cd D:\_intern\Intern\EducationSystem
Copy-Item .\connectionstrings.Development.example.json .\connectionstrings.Development.json
```

Cập nhật chuỗi kết nối trong `connectionstrings.Development.json` (dùng Windows Authentication hoặc SQL Authentication):
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
*(Hoặc dùng .NET User Secrets / `appsettings.Development.json` cho từng dự án).*

#### 1.2. Chạy Migration EF Core & SQL Scripts
Thực thi tuần tự lệnh EF Core Migration cho 5 DbContext:
```powershell
dotnet ef database update --project src/Services/IdentityService/IdentityService.csproj
dotnet ef database update --project src/Services/AcademicService/AcademicService.csproj
dotnet ef database update --project src/Services/ExamService/ExamService.csproj
dotnet ef database update --project src/Services/CommunicationService/CommunicationService.csproj
dotnet ef database update --project src/Services/CareerService/CareerService.csproj
```

**Hoặc chạy bộ Script khôi phục CSDL trong `database/scripts/` bằng SSMS hoặc `sqlcmd`:**
```powershell
# Chạy nhóm Script Career Schema
sqlcmd -S "localhost" -d "TayDoV2" -E -b -f 65001 -i ".\database\scripts\career\001_create_career_schema.sql"
sqlcmd -S "localhost" -d "TayDoV2" -E -b -f 65001 -i ".\database\scripts\career\002_create_career_tables.sql"
sqlcmd -S "localhost" -d "TayDoV2" -E -b -f 65001 -i ".\database\scripts\career\003_seed_progression_levels.sql"
sqlcmd -S "localhost" -d "TayDoV2" -E -b -f 65001 -i ".\database\scripts\career\004_create_career_indexes.sql"
sqlcmd -S "localhost" -d "TayDoV2" -E -b -f 65001 -i ".\database\scripts\career\005_verify_career_schema.sql"

# Chạy nhóm Script Import & Kiểm duyệt
sqlcmd -S "localhost" -d "TayDoV2" -E -b -f 65001 -i ".\database\scripts\career-import\001_create_import_tables.sql"
sqlcmd -S "localhost" -d "TayDoV2" -E -b -f 65001 -i ".\database\scripts\career-import\002_create_import_constraints.sql"
sqlcmd -S "localhost" -d "TayDoV2" -E -b -f 65001 -i ".\database\scripts\career-import\003_create_import_indexes.sql"
sqlcmd -S "localhost" -d "TayDoV2" -E -b -f 65001 -i ".\database\scripts\career-import\004_add_subject_selection.sql"
sqlcmd -S "localhost" -d "TayDoV2" -E -b -f 65001 -i ".\database\scripts\career-import\005_verify_import_schema.sql"
sqlcmd -S "localhost" -d "TayDoV2" -E -b -f 65001 -i ".\database\scripts\career-import\006_add_approved_outcome_document.sql"
```

---

### 🐍 Bước 2: Cài Đặt và Chạy VectorMatchService (Python / FastAPI)

Dịch vụ **VectorMatchService** phục vụ cho việc khớp nối dữ liệu và tìm kiếm vector (Vector Search), chạy trên **Port 5006**.

1. **Di chuyển vào thư mục dịch vụ Vector:**
   ```powershell
   cd D:\_intern\Intern\EducationSystem\src\Services\VectorMatchService
   ```

2. **Khởi tạo và kích hoạt môi trường ảo Python (`venv`):**
   ```powershell
   python -m venv .venv
   .\.venv\Scripts\Activate.ps1
   ```

3. **Cài đặt thư viện phụ thuộc:**
   ```powershell
   pip install -r requirements.txt
   ```

4. **Tạo tệp cấu hình `.env`:**
   Tạo tệp `src\Services\VectorMatchService\.env` từ tệp `.env.example`:
   ```env
   DRIVER={ODBC Driver 18 for SQL Server}
   SERVER=127.0.0.1,1433
   DATABASE=TayDoV2
   UID=sa
   PWD=YourPassword123
   TrustServerCertificate=yes
   Encrypt=no
   ```

5. **Khởi chạy Dịch vụ Python (Port 5006):**
   Chạy thủ công:
   ```powershell
   uvicorn main:app --host 127.0.0.1 --port 5006 --reload
   ```
   *(Hoặc sử dụng script có sẵn: `..\..\..\scripts\Start-VectorMatchService.ps1`)*.

---

### ⚙️ Bước 3: Cấu Hình và Khởi Chạy 5 Dịch Vụ .NET 8 (Backend Services)

Danh sách các Microservices .NET 8 và cổng HTTP tương ứng:

| Dịch Vụ Backend | Base Route | Port HTTP Cục Bộ |
| :--- | :--- | :--- |
| **IdentityService** | `/api/identity` | `http://localhost:5001` |
| **AcademicService** | `/api/academic` | `http://localhost:5002` |
| **ExamService** | `/api/exam` | `http://localhost:5003` |
| **CommunicationService** | `/api/communication` | `http://localhost:5004` |
| **CareerService** | `/api/career` | `http://localhost:5005` |

#### 3.1. Cập nhật endpoint gọi nội bộ (BFF Client Configuration)
Trong môi trường Bare-Metal local, đảm bảo các endpoint giao tiếp giữa các service (như trong `CareerService/appsettings.json` hoặc biến môi trường) được trỏ về **`localhost`** thay vì DNS container:
- Đổi `http://academic-service:5002` ➔ `http://localhost:5002`
- Đổi `http://identity-service:5001` ➔ `http://localhost:5001`
- Đổi `http://vector-service:5006` ➔ `http://localhost:5006`

#### 3.2. Lệnh khởi chạy cục bộ từng dịch vụ
Đứng tại thư mục `EducationSystem`:
```powershell
dotnet run --project src/Services/IdentityService/IdentityService.csproj --launch-profile http
dotnet run --project src/Services/AcademicService/AcademicService.csproj --launch-profile http
dotnet run --project src/Services/ExamService/ExamService.csproj --launch-profile http
dotnet run --project src/Services/CommunicationService/CommunicationService.csproj --launch-profile http
dotnet run --project src/Services/CareerService/CareerService.csproj --launch-profile http
```

---

### 💻 Bước 4: Cài Đặt và Khởi Động React Frontend

1. **Di chuyển vào thư mục Frontend:**
   ```powershell
   cd D:\_intern\Intern\EducationSystem\education-system-ui
   ```

2. **Cài đặt các gói Dependency:**
   ```powershell
   npm ci
   ```

3. **Cấu hình tệp `.env.local` / `.env.development`:**
   Tạo tệp `education-system-ui/.env.local` để trỏ trực tiếp về cổng backend localhost:
   ```dotenv
   VITE_RESUME_API_MODE=live
   VITE_AI_OPTIMIZE_MODE=live

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

4. **Khởi chạy ứng dụng Frontend React (Vite):**
   ```powershell
   npm run dev -- --host 127.0.0.1
   ```
   Ứng dụng sẽ chạy tại địa chỉ: **`http://127.0.0.1:5173`**.

---

### 🚀 Bước 5: Khởi Động Tự Động Toàn Bộ Bằng Script Tiện Ích (One-Click Startup)

Repository cung cấp script PowerShell tiện ích `Start-Dev.ps1` để tự động hóa toàn bộ việc khởi chạy 5 dịch vụ .NET, 1 dịch vụ Python và Frontend chỉ với 1 câu lệnh:

```powershell
cd D:\_intern\Intern\EducationSystem

# (Tùy chọn) Gán API key AI nếu dùng tính năng CV/CLO-PLO
$env:ResumeLLM__Provider = "Vault"
$env:ResumeLLM__Model = "gpt-5.6-sol"
$env:ResumeLLM__BaseUrl = "https://newapi.vault.io.vn/v1"
$env:ResumeLLM__ApiKey = "YOUR_API_KEY"

# Thực thi script khởi chạy toàn bộ
.\scripts\Start-Dev.ps1
```

**Script `Start-Dev.ps1` sẽ tự động thực hiện:**
- Khởi tạo thư mục `.run/` chứa log và PID.
- Tạo hoặc sử dụng lại khóa ký JWT cục bộ tại `.run/student-jwt-signing-key`.
- Khởi động 5 dịch vụ .NET 8 (Ports 5001 - 5005).
- Tự động gọi `Start-VectorMatchService.ps1` để chạy dịch vụ Python Vector (Port 5006).
- Mở Frontend React Vite tại cổng `5173`.

Để dừng toàn bộ dịch vụ backend đang chạy ẩn:
```powershell
.\scripts\Stop-BackendServices.ps1
```

---

## 📊 4. Tổng Kết Bảng Cổng Kết Nối (Port Mapping Table)

| Thành Phần | Công Nghệ | Endpoint / Port Local |
| :--- | :--- | :--- |
| **React Frontend** | React + Vite | `http://127.0.0.1:5173` |
| **IdentityService** | .NET 8 Web API | `http://localhost:5001/swagger` |
| **AcademicService** | .NET 8 Web API | `http://localhost:5002/swagger` |
| **ExamService** | .NET 8 Web API | `http://localhost:5003/swagger` |
| **CommunicationService** | .NET 8 Web API | `http://localhost:5004/swagger` |
| **CareerService** | .NET 8 Web API | `http://localhost:5005/swagger` |
| **VectorMatchService** | Python FastAPI | `http://localhost:5006/health` |
| **Database SQL Server** | MSSQL 2022 | `127.0.0.1:1433` (Database `TayDoV2`) |

---
*Hoàn tất quy trình Bare-Metal Local Setup. Hệ thống đảm bảo hiển thị chuẩn 100% tiếng Việt trên các bản in PDF và log!*
