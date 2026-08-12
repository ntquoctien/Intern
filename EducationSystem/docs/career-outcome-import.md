# CareerService — Import và kiểm duyệt CLO/PLO

## Phạm vi

Module chuyển tài liệu CLO/PLO dạng DOCX hoặc PDF thành dữ liệu có cấu trúc, cho quản trị viên kiểm duyệt và chỉ ghi vào bảng chính thức sau khi phê duyệt. Đây là dữ liệu đầu vào cho bộ lọc CLO/PLO–JD ở giai đoạn sau.

Trong phạm vi:

- upload DOCX hoặc PDF (PDF dùng OCR Gemini/Vault);
- tách block có thể truy vết;
- LLM trích xuất JSON theo schema;
- đối soát và kiểm tra bằng rule;
- sửa, kiểm tra lại, phê duyệt, từ chối, lưu trữ;
- ghi PLO, CLO và ma trận E/R/D theo cơ chế insert-only;
- sinh official JSON bất biến từ dữ liệu SQL đã phê duyệt;
- tính báo cáo chất lượng deterministic cho UI kiểm duyệt.

Ngoài phạm vi: embedding, vector index, JD matching, xếp hạng và sinh CV.

## Luồng xử lý

Trước khi upload, quản trị viên chọn **Ngành → Học phần → Phiên bản**:

- danh sách học phần ban đầu được lấy từ các kế hoạch đào tạo của ngành đã chọn;
- khi nhập mã hoặc tên học phần, UI tìm trong toàn bộ danh mục học phần;
- phiên bản là chuỗi do người dùng nhập; hệ thống dùng lại curriculum draft cùng ngành/phiên bản hoặc tạo snapshot mới;
- ID, mã và tên học phần đã chọn được lưu trong `OutcomeImportBatch`, gửi vào ngữ cảnh LLM và đối chiếu lại bằng rule deterministic.

```text
Uploaded
   |
   v
Processing
   |-- ParsingDocument
   |-- ExtractingProgram
   |-- ExtractingOutcomes
   |-- ExtractingMatrix
   |-- Reconciling
   `-- Validating
          |
          +--> PendingReview ----> Approved ----> Archived
          |          |
          |          `-----------> Rejected ----> Archived
          |
          `--> ValidationFailed --(sửa/validate)--> PendingReview

Lỗi parser/LLM/runtime --> Failed --(retry)--> Uploaded
```

1. API kiểm tra extension, MIME, kích thước file, số ZIP entry, kích thước giải nén, đường dẫn nguy hiểm và tính hợp lệ OpenXML.
2. File được lưu ngoài web root; CSDL chỉ giữ `StorageKey` và SHA-256.
3. Worker claim batch `Uploaded` bằng transaction, parse heading/paragraph/table thành block ổn định.
4. Provider được chọn (Gemini/Groq) trích xuất JSON theo từng nhiệm vụ. Nếu Admin đã chọn học phần, tài liệu được xem là nguồn CLO cấp môn: hệ thống không gọi request PLO mà tạo `plos: []` cùng metadata curriculum từ CSDL. Request CLO chỉ nhận vùng `MỤC TIÊU MÔN HỌC`; tài liệu không có ma trận nhận mapping rỗng bằng rule deterministic.
5. Service chuẩn hoá mã, sinh draft ID còn thiếu, gộp bản trùng hoàn toàn và đánh dấu mâu thuẫn.
   Với vùng mục tiêu có các dấu `+` chính thức, service đếm trước số mục. Nếu provider gộp hoặc bỏ sót mục, danh sách review được dựng lại từ chính các dấu đầu dòng nguồn, giữ thứ tự/source reference và ghi warning `CLO_LIST_RECONCILED_FROM_FORMAL_OBJECTIVES`.
6. Rule engine kiểm tra metadata curriculum, mã trùng, confidence, source block, môn học từ AcademicService, E/R/D, tham chiếu mapping và xung đột dữ liệu chính thức.
7. Quản trị viên sửa draft bằng optimistic concurrency (`RowVersion`), xem source reference và kiểm tra lại.
8. Approve chạy trong serializable transaction, ghi toàn bộ PLO/CLO/mapping, đọc lại dữ liệu SQL, sinh `ApprovedOutcomeJsonDocument` hoặc rollback toàn bộ.

## Ranh giới LLM

Gemini hoặc Groq chỉ được gọi cho ba loại task nội bộ cố định: PLO, CLO và ma trận CLO–PLO. Với import đã chọn học phần, task PLO bị bỏ qua hoàn toàn. Provider từ chối task number khác trước khi gửi request. System instruction xác định block tài liệu là dữ liệu không đáng tin, không phải chỉ dẫn; không cấp tool, quyền thao tác website hoặc quyền ghi dữ liệu. Gemini dùng response schema; Groq `qwen/qwen3.6-27b` dùng JSON Object Mode kèm schema trong prompt và validation phía ứng dụng. Source-reference validation, rule engine và bước kiểm duyệt của admin vẫn là lớp quyết định.

## Trạng thái tầng JSON Document

Workflow hiện tại đã có JSON staging:

- `RawExtractionJson`: ba response thô theo schema từ provider;
- `ReviewedJson`: `OutcomeDraftDocument` đã reconcile, chứa curriculum, PLO, subject/CLO, mappings, source references và warnings;
- `ValidationJson`: kết quả rule deterministic.
- `QualityReportJson`: chỉ số phạm vi, nguồn dẫn, số lượng, exact-code evidence, mapping integrity và text overlap.

Các JSON staging nằm trong `career.OutcomeImportBatch`. Sau khi approve, dữ liệu chuẩn được publish vào các bảng quan hệ rồi được đọc lại để tạo `career.ApprovedOutcomeJsonDocument`. Official document có `SchemaVersion`, `DocumentVersion`, canonical `ContentJson`, SHA-256 `ContentHash`, provenance và trạng thái `Active`/`Superseded`; nội dung không được lấy bằng cách copy `ReviewedJson`.

Vector embedding vẫn là bước downstream được giữ trong thiết kế: nguồn vector tương lai phải là official document đã duyệt, không phải raw/reviewed JSON. Module hiện tại chưa tạo vector, index, embedding job hoặc gọi embedding model.

## Dữ liệu

Ba bảng workflow:

- `career.OutcomeImportBatch`: file, hash, snapshot học phần đã chọn, trạng thái, raw/reviewed/validation/quality JSON, actor, lỗi và rowversion.
- `career.OutcomeDocumentBlock`: block nguồn có `BlockId`, thứ tự, loại và content JSON.
- `career.OutcomeReviewLog`: lịch sử edit/remove/restore/validate/approve/reject/archive.

Bảng chính thức được ghi khi approve:

- `career.ProgramLearningOutcome`;
- `career.CourseLearningOutcome`;
- `career.CloPloMapping`.
- `career.ApprovedOutcomeJsonDocument`: official JSON bất biến được materialize từ các bảng chính thức.

Scripts nằm tại `database/scripts/career-import`:

```powershell
sqlcmd -S ".\SOFTWAREINTERN" -d TayDoV2 -E -f 65001 -i database\scripts\career-import\001_create_import_tables.sql
sqlcmd -S ".\SOFTWAREINTERN" -d TayDoV2 -E -f 65001 -i database\scripts\career-import\002_create_import_constraints.sql
sqlcmd -S ".\SOFTWAREINTERN" -d TayDoV2 -E -f 65001 -i database\scripts\career-import\003_create_import_indexes.sql
sqlcmd -S ".\SOFTWAREINTERN" -d TayDoV2 -E -f 65001 -i database\scripts\career-import\004_add_subject_selection.sql
sqlcmd -S ".\SOFTWAREINTERN" -d TayDoV2 -E -f 65001 -i database\scripts\career-import\006_add_approved_outcome_document.sql
sqlcmd -S ".\SOFTWAREINTERN" -d TayDoV2 -E -f 65001 -i database\scripts\career-import\005_verify_import_schema.sql
```

Rollback xoá official document trước các bảng workflow theo thứ tự phụ thuộc. Nó không tự xoá PLO/CLO đã phê duyệt:

```powershell
sqlcmd -S ".\SOFTWAREINTERN" -d TayDoV2 -E -f 65001 -i database\scripts\career-import\rollback_import_schema.sql
```

## Cấu hình

Connection string được đọc từ file dùng chung:

```json
{
  "ConnectionStrings": {
    "CareerDb": "Server=.\\SOFTWAREINTERN;Database=TayDoV2;Integrated Security=True;TrustServerCertificate=True"
  }
}
```

Không commit API key. Cấu hình local bằng user secrets hoặc biến môi trường:

```powershell
dotnet user-secrets set "LLM:Model" "YOUR_GEMINI_MODEL" --project src\Services\CareerService\CareerService.csproj
dotnet user-secrets set "LLM:ApiKey" "YOUR_GEMINI_API_KEY" --project src\Services\CareerService\CareerService.csproj
```

Chọn Groq mà không ghi secret vào repository:

```powershell
dotnet user-secrets set "LLM:Provider" "Groq" --project src\Services\CareerService\CareerService.csproj
dotnet user-secrets set "LLM:Model" "qwen/qwen3.6-27b" --project src\Services\CareerService\CareerService.csproj
dotnet user-secrets set "LLM:ApiKey" "YOUR_GROQ_API_KEY" --project src\Services\CareerService\CareerService.csproj
```

Các cấu hình đáng chú ý:

| Key | Mặc định | Ý nghĩa |
| --- | ---: | --- |
| `OutcomeStorage:RootPath` | `.data/outcome-imports` | thư mục private lưu DOCX/PDF |
| `OutcomeStorage:MaxFileSizeBytes` | 10 MB | giới hạn file nén |
| `OutcomeStorage:MaxExpandedSizeBytes` | 100 MB | giới hạn giải nén |
| `PdfOcr:Enabled` | `true` | bật OCR cho tài liệu PDF |
| `PdfOcr:Provider` | `Vault` | OCR PDF qua `Vault` hoặc `Gemini` |
| `PdfOcr:Model` | `gpt-5.6-sol` | model có khả năng đọc tệp PDF |
| `AcademicClient:BaseUrl` | `http://localhost:5002` | tra cứu môn học |
| `LLM:MaxRetries` | 2 | retry lỗi mạng/429/5xx |
| `LLM:TimeoutSeconds` | 180 | timeout cho một lượt trích xuất tài liệu dài |
| `LLM:MaxInputTokensPerRequest` | 12000 | giới hạn input xấp xỉ |
| `ManagementAuth:AllowDevelopmentBypass` | false | fake System Admin, chỉ có hiệu lực trong Development |

Tài khoản Groq có TPM thấp nên đặt `LLM:MaxInputTokensPerRequest` phù hợp với tier; cấu hình kiểm thử local hiện dùng 3000. Retry 429 tôn trọng `Retry-After` hoặc thời gian chờ trong response của Groq.

Frontend:

```env
VITE_CAREER_API_ORIGIN=http://localhost:5005
VITE_ACADEMIC_API_ORIGIN=http://localhost:5002
```

## API quản trị

Base path: `/api/management`.

| Method | Path | Mục đích |
| --- | --- | --- |
| GET/POST | `/curriculum-versions` | chọn hoặc tạo curriculum draft |
| GET/POST | `/outcome-imports` | danh sách hoặc upload |
| POST | `/outcome-imports/{id}/process` | retry |
| GET | `/outcome-imports/{id}` | trạng thái |
| GET/PATCH | `/outcome-imports/{id}/review` | đọc hoặc sửa draft |
| GET | `/outcome-imports/{id}/approved-document` | đọc official JSON đã phê duyệt |
| POST | `/outcome-imports/{id}/validate` | kiểm tra lại với rowversion |
| POST | `/outcome-imports/{id}/approve` | publish atomic |
| POST | `/outcome-imports/{id}/reject` | từ chối kèm lý do |
| POST | `/outcome-imports/{id}/archive` | lưu trữ batch kết thúc |

Lỗi trả về `application/problem+json` với `errorCode`, `detail` và `traceId`.

## UI

Các route:

- `/management/system/outcomes`;
- `/management/system/outcomes/import`;
- `/management/system/outcomes/{id}/review`.

Workspace hiển thị PLO, môn/CLO, ma trận E/R/D, confidence, source reference, quality metrics, lỗi chặn và cảnh báo. Batch đã approve hiển thị version, trạng thái, hash và nội dung official JSON. Nút approve bị khoá khi còn `BlockingError`.

## Bảo mật hiện tại

Tất cả endpoint dùng policy `SystemAdministrator`. Trong Development có thể bật bypass để tích hợp local. Ngoài Development, handler không tạo identity nên request không có cơ chế xác thực hợp lệ sẽ nhận 401.

Đây là mức bảo mật cơ bản theo phạm vi hiện tại. Trước production cần thay handler local bằng JWT/OIDC của management portal, đặt private storage phù hợp, quản lý secret tập trung và chốt retention.

## Chạy và kiểm thử

```powershell
dotnet build EducationSystem.sln
dotnet test tests\CareerService.Tests\CareerService.Tests.csproj

cd education-system-ui
npm run build
npm test
```

Khởi động local:

```powershell
.\scripts\Start-BackendServices.ps1
cd education-system-ui
npm run dev -- --host 127.0.0.1
```

- CareerService Swagger: `http://localhost:5005/swagger`
- Health: `http://localhost:5005/health`
