# VectorMatchService

FastAPI microservice tìm các CLO/tiêu chí học thuật gần nghĩa với mô tả công
việc bằng Sentence-BERT và FAISS cosine similarity.

## Nguồn dữ liệu

Service chỉ đọc dữ liệu có nguồn trong SQL Server:

- các môn sinh viên có điểm trung bình từ `7.0`;
- CLO trạng thái `Approved` trong `career.CourseLearningOutcome`;
- mức E/R/D từ mapping đã duyệt;
- `academic.EvaluationCriterias` gắn với kết quả đánh giá của sinh viên;
- dự án và thực tập của chính sinh viên cho nhánh fallback.

Mỗi vector CLO ghép metadata môn, CLO và các tiêu chí của môn. Nếu một môn chưa
có CLO đã duyệt, từng criteria được index riêng. `bigint` CLO ID được dùng trực
tiếp làm FAISS ID; UUID criteria dùng một ID `int64` âm ổn định và được ánh xạ
ngược qua metadata.

## Thuật toán

1. Encode bằng `sentence-transformers/multi-qa-MiniLM-L6-cos-v1` (384 chiều).
2. Chuẩn hóa L2 bằng `faiss.normalize_L2`.
3. Index bằng `IndexIDMap2(IndexFlatIP(384))`.
4. Encode và chuẩn hóa JD bằng cùng model.
5. Search top-K và giữ kết quả đạt threshold yêu cầu.
6. Nếu không có kết quả, thử ngưỡng `0.50`, đồng thời tìm bằng chứng dự
   án/thực tập; response đặt `isFallback=true`.

Index được cache theo `studentId + selectedCourseIds` trong 15 phút mặc định.
Endpoint reindex cho phép xóa cache và dựng lại sau khi dữ liệu được duyệt.

## Cài đặt trên Windows

Yêu cầu:

- Python 3.11;
- Microsoft ODBC Driver 18 for SQL Server;
- database đã có schema `academic` và `career`.

Tạo file cấu hình:

```powershell
Copy-Item .env.example .env
```

Sửa `VECTOR_DB_CONNECTION_STRING` trong `.env`, sau đó từ thư mục
`EducationSystem` chạy:

```powershell
.\scripts\Start-VectorMatchService.ps1 -Install
```

Model Sentence-BERT được tải từ Hugging Face trong lần chạy đầu. Các lần sau
dùng model cache trên máy.

Hoặc chạy cùng môi trường development của toàn hệ thống:

```powershell
.\scripts\Start-Dev.ps1
```

Script tự tạo virtual environment và cài dependency khi chưa có hoặc khi
`requirements.txt` thay đổi. Dùng `-InstallPythonDependencies` khi muốn ép cài
lại thủ công.

### Cấp quyền database tối thiểu

Chạy role script bằng tài khoản quản trị database:

```powershell
sqlcmd -S "YOUR_SQL_SERVER" -d TayDoV2 -E -f 65001 `
  -i database\scripts\vector-match\001_create_vector_match_reader_role.sql
```

Tạo login/user riêng và thêm vào role:

```sql
USE master;
CREATE LOGIN [vector_match_user]
WITH PASSWORD = N'THAY_BANG_MAT_KHAU_MANH';
GO

USE TayDoV2;
CREATE USER [vector_match_user] FOR LOGIN [vector_match_user];
ALTER ROLE [vector_match_reader] ADD MEMBER [vector_match_user];
GO
```

Nếu user đã tồn tại, chỉ cần chạy `ALTER ROLE`. Không dùng `career_user` mặc
định nếu tài khoản đó chưa được cấp quyền đọc các bảng `academic`; service sẽ
nhận lỗi SQL Server 229 (`SELECT permission was denied`).

Chạy trực tiếp:

```powershell
cd src\Services\VectorMatchService
python -m venv .venv
.\.venv\Scripts\python.exe -m pip install -r requirements.txt
.\.venv\Scripts\python.exe run.py
```

Swagger: `http://localhost:5006/docs`.

## API

### Match CLO

```http
POST /api/ai/vector-match/clos
Content-Type: application/json
X-Internal-Api-Key: <VECTOR_INTERNAL_API_KEY>
```

```json
{
  "studentId": "11111111-1111-1111-1111-111111111111",
  "jobDescription": "Thiết kế và tối ưu cơ sở dữ liệu SQL...",
  "selectedCourseIds": [
    "22222222-2222-2222-2222-222222222222"
  ],
  "topK": 10,
  "similarityThreshold": 0.65
}
```

Ví dụ response:

```json
{
  "studentId": "11111111-1111-1111-1111-111111111111",
  "targetJobMatched": true,
  "isFallback": false,
  "appliedSimilarityThreshold": 0.65,
  "matchedOutcomeCount": 1,
  "topMatchedOutcomes": [
    {
      "cloId": 12,
      "criteriaId": null,
      "sourceType": "CLO",
      "subjectCode": "INT1234",
      "subjectName": "Cơ sở dữ liệu",
      "cloCode": "CLO1",
      "description": "Thiết kế mô hình ERD và chuẩn hóa cơ sở dữ liệu",
      "similarityScore": 0.87,
      "progressionLevel": "D"
    }
  ],
  "fallbackEvidence": [],
  "warnings": []
}
```

### Dựng lại index

```http
POST /api/ai/vector-match/students/{studentId}/reindex
```

Có thể truyền nhiều `selected_course_ids` trong query string. Nếu không truyền,
service index tất cả môn đủ điều kiện.

### Health

```http
GET /health
```

## Bảo mật

Đặt `VECTOR_INTERNAL_API_KEY` ngoài môi trường local. Khi key có giá trị, hai
endpoint mutation/search bắt buộc header `X-Internal-Api-Key`. Tài khoản SQL
nên là user read-only và chỉ được `SELECT` trên các bảng cần thiết. Service
không ghi database và không gửi nội dung học tập/JD sang LLM sinh nội dung.
Không expose service này trực tiếp ra Internet; backend/BFF đã xác thực sinh
viên phải kiểm tra quyền sở hữu `studentId` rồi mới gọi bằng internal API key.

## Test

```powershell
python -m pip install -r requirements-dev.txt
python -m pytest -q
```

