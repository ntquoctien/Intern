# Education Management System (EducationSystem)
## Hướng Dẫn Cài Đặt và Khởi Chạy Local (Bare-Metal Local Setup Guide - Non-Docker)

---

## 📌 1. Bối Cảnh & Lý Do Không Sử Dụng Docker (Context & Motivation)

⚠️ **LÝ DO KHÔNG SỬ DỤNG DOCKER TRONG MÔI TRƯỜNG HIỆN TẠI:**
Do các lỗi về encoding và thiếu font tiếng Việt trong môi trường Docker Container (Linux container cơ bản), các file PDF báo cáo/CV sinh viên và log hệ thống xuất ra bị **lỗi hiển thị font chữ tiếng Việt (corrupted fonts/encoding issues)**. 
Do đó, tài liệu này hướng dẫn chi tiết quy trình **Bare-Metal Local Setup** — cài đặt và vận hành toàn bộ hệ thống trực tiếp trên hệ điều hành host (Windows/macOS/Linux) nhằm đảm bảo hiển thị chuẩn tiếng Việt, hiệu năng tối ưu và dễ dàng debug trong quá trình phát triển.

---

## 🛠️ 2. Yêu Cầu Hệ Thống (Prerequisites)

1. **Runtime .NET:** .NET SDK 8.0 (`dotnet --version`)
2. **NodeJS:** Node.js v20 LTS hoặc v22 (`node --version`, `npm --version`)
3. **Python:** Python 3.10+ (cho dịch vụ VectorMatchService)
4. **Database:** Microsoft SQL Server 2022 (Developer hoặc Express Edition) cài đặt cục bộ + Microsoft ODBC Driver 18 for SQL Server.

---

## 🚀 3. Quy Trình Cài Đặt Chi Tiết

### Bước 1: Khởi Tạo Cơ Sở Dữ Liệu Cục Bộ (Local Database Ingestion)
- Đảm bảo file `connectionstrings.Development.json` tại thư mục solution kết nối đến SQL Server:
  `Server=localhost;Database=TayDoV2;Trusted_Connection=True;TrustServerCertificate=True;`
- Thực thi tuần tự lệnh EF Core Migration cho 5 DbContext hoặc nạp các script SQL trong `database/scripts/`:
  ```powershell
  dotnet ef database update --project src/Services/IdentityService/IdentityService.csproj
  dotnet ef database update --project src/Services/AcademicService/AcademicService.csproj
  dotnet ef database update --project src/Services/ExamService/ExamService.csproj
  dotnet ef database update --project src/Services/CommunicationService/CommunicationService.csproj
  dotnet ef database update --project src/Services/CareerService/CareerService.csproj
  ```

### Bước 2: Cài Đặt và Chạy VectorMatchService (Python / FastAPI)
- Di chuyển vào `src/Services/VectorMatchService`, tạo và kích hoạt `venv`:
  ```powershell
  python -m venv .venv
  .\.venv\Scripts\Activate.ps1
  pip install -r requirements.txt
  ```
- Tạo tệp `.env` cấu hình ODBC:
  `DRIVER={ODBC Driver 18 for SQL Server};SERVER=127.0.0.1,1433;DATABASE=TayDoV2;UID=sa;PWD=...`
- Khởi chạy trên cổng `http://localhost:5006`.

### Bước 3: Cấu Hình và Khởi Chạy 5 Dịch Vụ .NET 8 (Backend Services)
Cổng HTTP các dịch vụ:
- IdentityService -> `http://localhost:5001`
- AcademicService -> `http://localhost:5002`
- ExamService -> `http://localhost:5003`
- CommunicationService -> `http://localhost:5004`
- CareerService -> `http://localhost:5005`

Đảm bảo cấu hình BFF / Client endpoints trỏ về `localhost` thay vì DNS container (`http://academic-service:5002` ➔ `http://localhost:5002`).

### Bước 4: Cài Đặt và Khởi Động React Frontend
- Di chuyển vào `education-system-ui`, cài đặt `npm ci`.
- Cấu hình `.env.local`: `VITE_RESUME_API_MODE=live`, `VITE_AI_OPTIMIZE_MODE=live`, trỏ các base URL về các cổng localhost 5001-5005 tương ứng.
- Chạy môi trường dev: `npm run dev -- --host 127.0.0.1` (`http://127.0.0.1:5173`).

### Bước 5: Khởi Động Tự Động Tất Cả (One-Click Startup)
Thực thi script tiện ích có sẵn trong repository:
```powershell
.\scripts\Start-Dev.ps1
```
*(Script sẽ tự động khởi chạy 5 services .NET, 1 service Python VectorMatchService và React Frontend).*

Dừng các service backend:
```powershell
.\scripts\Stop-BackendServices.ps1
```

---

## 📊 4. Bảng Cổng Kết Nối (Port Mapping)

| Dịch Vụ | Port Local | URL Swagger / Health |
| :--- | :--- | :--- |
| **React Frontend** | `5173` | `http://127.0.0.1:5173` |
| **IdentityService** | `5001` | `http://localhost:5001/swagger` |
| **AcademicService** | `5002` | `http://localhost:5002/swagger` |
| **ExamService** | `5003` | `http://localhost:5003/swagger` |
| **CommunicationService** | `5004` | `http://localhost:5004/swagger` |
| **CareerService** | `5005` | `http://localhost:5005/swagger` |
| **VectorMatchService** | `5006` | `http://localhost:5006/health` |
| **SQL Server** | `1433` | Database `TayDoV2` |
