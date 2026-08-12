# Hướng Dẫn Cài Đặt Và Vận Hành Hệ Thống (Education System Setup Guide)
## Dự Án EducationSystem — Trường Cao Đẳng Tây Đô (Tay Do College - TDC)

---

## 📌 1. Tổng Quan Hệ Thống & Bối Cảnh Vận Hành

Hệ thống **EducationSystem** cho **Trường Cao Đẳng Tây Đô** bao gồm 6 dịch vụ Microservices (.NET 8 & Python FastAPI) và 1 ứng dụng React Frontend SPA:

1. **IdentityService (.NET 8 - Port 5001)**: Quản lý đăng nhập, cấp phát JWT token, phân quyền tài khoản (Sinh viên, Giảng viên, Quản trị).
2. **AcademicService (.NET 8 - Port 5002)**: Quản lý thông tin khoa, ngành học, chương trình đào tạo, sinh viên, môn học và điểm số.
3. **ExamService (.NET 8 - Port 5003)**: Quản lý ngân hàng câu hỏi, đề thi và tổ chức thi trực tuyến.
4. **CommunicationService (.NET 8 - Port 5004)**: Quản lý thông báo, mẫu biểu và tự động gửi email xác nhận thực tập qua SMTP Gmail.
5. **CareerService (.NET 8 / BFF - Port 5005)**: Quản lý lộ trình hướng nghiệp, tích hợp LLM (Groq Qwen 3.5 27B / Gemini 3.5 Flash) để tự động tạo và tối ưu CV sinh viên.
6. **VectorMatchService (Python 3.11 FastAPI - Port 5006)**: Dịch vụ trí tuệ nhân tạo khớp nối chuẩn đầu ra (CLO/PLO) và gợi ý môn học bằng thuật toán nhúng Vector (Sentence-BERT & FAISS).
7. **EducationSystem UI (React 19 / Vite 8 - Port 5173)**: Giao diện người dùng SPA cho sinh viên và ban quản lý.

---

## 🛠️ 2. Yêu Cầu Tiền Đề (Prerequisites)

Trước khi khởi chạy hệ thống, máy tính phát triển cần cài đặt đầy đủ các thành phần:

1. **.NET 8.0 SDK**:
   - Kiểm tra bằng lệnh: `dotnet --version` (Yêu cầu `8.0.x`)
2. **Node.js & npm**:
   - Kiểm tra bằng lệnh: `node -v` (Yêu cầu Node `v20 LTS` trở lên)
3. **Microsoft SQL Server**:
   - Đã cài đặt SQL Server local (Bản Developer hoặc Express Edition) và **đã restore CSDL `TayDoV2`**.
4. **(Tùy chọn) Python 3.11+**:
   - Dùng cho `VectorMatchService`. Nếu máy chưa cài Python, hệ thống sẽ tự động chuyển sang cơ chế **Fallback điểm số môn học** mà không làm gián đoạn các dịch vụ khác.

---

## ⚙️ 3. Cấu Hình Chuỗi Kết Nối CSDL (ConnectionStrings)

Tệp cấu hình kết nối CSDL nằm tại:
`EducationSystem/connectionstrings.Development.json`

Đảm bảo chuỗi kết nối trỏ chính xác về SQL Server local trên máy của bạn (Ví dụ: `TIENNGUYEN\SOFTWAREINTERN` hoặc `localhost`):

```json
{
  "ConnectionStrings": {
    "IdentityDb": "Server=TIENNGUYEN\\SOFTWAREINTERN;Database=TayDoV2;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;",
    "AcademicDb": "Server=TIENNGUYEN\\SOFTWAREINTERN;Database=TayDoV2;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;",
    "ExamDb": "Server=TIENNGUYEN\\SOFTWAREINTERN;Database=TayDoV2;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;",
    "CommunicationDb": "Server=TIENNGUYEN\\SOFTWAREINTERN;Database=TayDoV2;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;",
    "CareerDb": "Server=TIENNGUYEN\\SOFTWAREINTERN;Database=TayDoV2;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
  },
  "StudentJwt": {
    "SigningKey": "a5t1gKZ7udcVv/jY/ghdYGN7BUSbAvPzC7Ph2C9sbMvOmBhiBOeEi/VQ+XWy6urr"
  }
}
```

*Lưu ý: Thay đổi `"TIENNGUYEN\\SOFTWAREINTERN"` thành tên SQL Server Instance trên máy của bạn.*

---

## 🔑 4. Cấu Hình Biến Môi Trường & User Secrets

### A. Tệp `.env` cho VectorMatchService (Python AI)
Tệp nằm tại đường dẫn: `EducationSystem/src/Services/VectorMatchService/.env`

Nội dung mẫu `.env`:
```env
VECTOR_ACADEMIC_DB_CONNECTION_STRING=
VECTOR_CAREER_DB_CONNECTION_STRING=
VECTOR_MODEL_NAME=sentence-transformers/multi-qa-MiniLM-L6-cos-v1
VECTOR_MODEL_DEVICE=cpu
VECTOR_ELIGIBLE_GPA_THRESHOLD=7.0
VECTOR_INDEX_TTL_SECONDS=900
VECTOR_INTERNAL_API_KEY=
```

### B. Cấu hình User Secrets cho LLM AI (Groq & Gemini)
Dịch vụ `CareerService` hỗ trợ hai mô hình LLM chính: **Groq Qwen_3.5_27B** và **Gemini 3.5 Flash**.

Để thiết lập API Key bảo mật qua `dotnet user-secrets`, mở PowerShell tại thư mục `EducationSystem`:

```powershell
# Cấu hình Groq LLM (Qwen 3.5 27B)
dotnet user-secrets set "LLM:Provider" "Groq" --project .\src\Services\CareerService\CareerService.csproj
dotnet user-secrets set "LLM:Model" "qwen/qwen3.5-27b" --project .\src\Services\CareerService\CareerService.csproj
dotnet user-secrets set "LLM:ApiKey" "<API_KEY_GROQ_CỦA_BẠN>" --project .\src\Services\CareerService\CareerService.csproj

# (Tùy chọn) Cấu hình Gemini 3.5 Flash
dotnet user-secrets set "Gemini:ApiKey" "<API_KEY_GEMINI_CỦA_BẠN>" --project .\src\Services\CareerService\CareerService.csproj
```

### C. Cấu hình Gmail SMTP cho Dịch Vụ Email
Thiết lập tài khoản gửi Email thông báo thực tập sinh viên cho `CommunicationService`:

```powershell
dotnet user-secrets set "InternshipEmail:SmtpHost" "smtp.gmail.com" --project .\src\Services\CommunicationService\CommunicationService.csproj
dotnet user-secrets set "InternshipEmail:SmtpPort" "587" --project .\src\Services\CommunicationService\CommunicationService.csproj
dotnet user-secrets set "InternshipEmail:Username" "email_cua_ban@gmail.com" --project .\src\Services\CommunicationService\CommunicationService.csproj
dotnet user-secrets set "InternshipEmail:Password" "<MA_UNG_DUNG_GMAIL>" --project .\src\Services\CommunicationService\CommunicationService.csproj
dotnet user-secrets set "InternshipEmail:FromName" "Cổng sinh viên Cao Đẳng Tây Đô" --project .\src\Services\CommunicationService\CommunicationService.csproj
```

---

## 🚀 5. Quy Trình Khởi Chạy Hệ Thống

Mở cửa sổ **PowerShell** và di chuyển vào thư mục `EducationSystem`:

```powershell
cd D:\Intern_IT\Intern\EducationSystem
```

### A. Khởi chạy TOÀN BỘ Hệ thống (Backend + Frontend)
Sử dụng script PowerShell tự động hóa:

```powershell
.\scripts\Start-Dev.ps1
```

**Script `Start-Dev.ps1` sẽ tự động:**
1. Khởi tạo khóa ký JWT dùng chung tại `.run/student-jwt-signing-key`.
2. Khởi chạy dịch vụ Python Vector Match (Port 5006).
3. Biên dịch và khởi chạy 5 dịch vụ .NET Microservices (Ports 5001 - 5005).
4. Khởi chạy Frontend React Vite tại địa chỉ `http://127.0.0.1:5173`.

### B. Chỉ khởi chạy Backend Microservices
```powershell
.\scripts\Start-BackendServices.ps1
```

### C. Tắt toàn bộ các dịch vụ Backend đang chạy
```powershell
.\scripts\Stop-BackendServices.ps1
```

---

## 🌐 6. Bảng Cổng Kết Nối & Địa Chỉ Truy Cập

| Dịch Vụ / Thành Phần | Mô Tả | Địa Chỉ Endpoint / Swagger |
| :--- | :--- | :--- |
| **React Frontend SPA** | Giao diện quản lý học tập & thực tập | **`http://127.0.0.1:5173`** |
| **IdentityService** | Xác thực JWT & Quản lý Tài khoản | `http://localhost:5001/swagger` |
| **AcademicService** | Quản lý Đào tạo, Môn học & Sinh viên | `http://localhost:5002/swagger` |
| **ExamService** | Khảo thí & Ngân hàng câu hỏi | `http://localhost:5003/swagger` |
| **CommunicationService** | Thông báo & Gửi Email tự động | `http://localhost:5004/swagger` |
| **CareerService** | Tối ưu CV AI & Hướng nghiệp | `http://localhost:5005/swagger` |
| **VectorMatchService** | Dịch vụ AI Vector Search | `http://127.0.0.1:5006/docs` |
| **SQL Server Database** | Cơ sở dữ liệu Cục bộ `TayDoV2` | `127.0.0.1:1433` |

---

## 🛠️ 7. Xử Lý Lỗi Thường Gặp (Troubleshooting)

1. **Lỗi `Error 26` / `Error 40` (Không thể kết nối CSDL)**:
   - Kiểm tra xem dịch vụ SQL Server (`MSSQLSERVER` hoặc `SOFTWAREINTERN`) trên Windows Services đã ở trạng thái **Running** chưa.
   - Kiểm tra tên Server trong `connectionstrings.Development.json` đã khớp với tên máy local chưa.

2. **Lỗi `Port in use` (Cổng 5001-5005 bị chiếm dụng)**:
   - Chạy script giải phóng cổng: `.\scripts\Stop-BackendServices.ps1`

3. **Lỗi chưa cài Python 3.11**:
   - `Start-Dev.ps1` đã tích hợp cơ chế tự động bắt ngoại lệ. Nếu máy chưa cài Python, hệ thống tự động cảnh báo và chuyển sang cơ chế **Fallback theo điểm môn học** giúp hệ thống vẫn vận hành bình thường.
