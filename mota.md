# Mô Tả Hệ Thống EducationSystem

## 1. Tổng Quan Hệ Thống

### 1.1. Giới Thiệu
- **Tên Dự Án**: EducationSystem - Hệ Thống Quản Lý Giáo Dục
- **Mô Tả**: Nền tảng quản lý giáo dục toàn diện cho các trường đại học/cao đẳng
- **Kiến Trúc**: Microservices theo mô hình Schema-per-Service
- **Công Nghệ**: .NET 8, ASP.NET Core Web API, Entity Framework Core, SQL Server

### 1.2. Các Dịch Vụ (Services)
Hệ thống bao gồm 4 dịch vụ độc lập:

| Dịch Vụ | Port HTTP | Port HTTPS | Schema DB | Mô Tả |
|---------|-----------|-----------|----------|-------|
| IdentityService | 5001 | 7001 | identity | Quản lý xác thực, người dùng, audit |
| AcademicService | 5002 | 7002 | academic | Quản lý học tập, lớp, sinh viên, điểm danh |
| ExamService | 5003 | 7003 | exam | Quản lý thi, câu hỏi, kết quả thi |
| CommunicationService | 5004 | 7004 | communication | Quản lý yêu cầu biểu mẫu, thông báo |

### 1.3. Cơ Sở Dữ Liệu
- **Số Bảng Theo EF Model Hiện Tại**: 38 bảng
- **Phân Bổ**: 
  - Identity: 5 bảng
  - Academic: 22 bảng
  - Exam: 8 bảng
  - Communication: 3 bảng
- **Kết Nối**: 4 connection strings riêng biệt (một cho mỗi dịch vụ)
- **Lưu ý triển khai**: hai bảng phục vụ CV là `academic.StudentProjects` và
  `academic.StudentInternships` đã có trong EF model nhưng chưa xuất hiện trong
  migration baseline hiện có trong repository. Cần bảo đảm database đích đã chạy
  migration/script tạo hai bảng trước khi bật chế độ lấy dữ liệu CV thật.

---

## 2. Các Vai Trò (Roles) và Tính Năng (Features) Người Dùng

### 2.1. Vai Trò Hệ Thống

| Vai Trò | Mô Tả | Tính Năng Chính |
|---------|-------|-----------------|
| **Admin** | Quản trị viên hệ thống | Quản lý toàn bộ dữ liệu, cài đặt hệ thống, audit logs |
| **Giáo Viên** | Người giảng dạy | Quản lý lớp, điểm danh, tạo đề thi, nhập điểm |
| **Sinh Viên** | Học viên | Xem lịch học, điểm danh, kết quả học tập, nộp yêu cầu |
| **Nhân Viên Đào Tạo** | Quản lý chương trình đào tạo | Quản lý môn học, lớp, lịch học, khóa học |

### 2.2. Tính Năng Chính

#### 2.2.1. Quản Lý Tài Khoản Người Dùng
- Đăng nhập với username/password
- Quản lý hồ sơ người dùng
- Phân quyền theo vai trò
- Audit log hoạt động
- Xử lý reset mật khẩu
- Quản lý thiết bị đăng nhập

#### 2.2.2. Quản Lý Học Viên
- Tạo/cập nhật/xóa thông tin học viên
- Ghi danh học phần
- Xem lịch học
- Xem kết quả học tập
- Quản lý thông tin cá nhân

#### 2.2.3. Quản Lý Môn Học
- Quản lý danh mục môn học
- Quản lý khoa/bộ môn
- Quản lý tài liệu môn học
- Định nghĩa tiêu chí đánh giá

#### 2.2.4. Quản Lý Lớp Học Phần
- Tạo lớp học phần
- Gán giáo viên
- Quản lý lịch học
- Quản lý phòng học
- Ghi danh sinh viên

#### 2.2.5. Quản Lý Điểm Danh
- Ghi nhận điểm danh
- Xem lịch sử điểm danh
- Cảnh cáo vắng mặt
- Báo cáo tỉ lệ vắng

#### 2.2.6. Quản Lý Thi
- Tạo đề thi
- Quản lý câu hỏi
- Tạo bộ câu hỏi
- Quản lý kỳ thi
- Nhập kết quả thi
- Xem chi tiết kết quả thi

#### 2.2.7. Quản Lý Đánh Giá
- Đánh giá sinh viên theo tiêu chí
- Xem kết quả đánh giá
- Báo cáo thống kê

#### 2.2.8. Quản Lý Yêu Cầu Biểu Mẫu
- Tạo yêu cầu biểu mẫu
- Duyệt yêu cầu
- Theo dõi trạng thái yêu cầu

#### 2.2.9. Quản Lý Thông Báo
- Gửi thông báo cho người dùng
- Xem lịch sử thông báo

#### 2.2.10. Tạo CV Thông Minh
- Tổng hợp hồ sơ, ngành học, GPA và học phần nổi bật từ dữ liệu học vụ
- Chọn dự án e-Portfolio và lịch sử thực tập đã xác nhận
- Nhập vị trí mục tiêu và mô tả công việc để định hướng nội dung
- Tối ưu cách diễn đạt bằng AI nhưng không tự tạo kinh nghiệm
- Cho phép sinh viên kiểm tra, chỉnh sửa và in/xuất PDF

---

## 3. Chi Tiết 38 Bảng Dữ Liệu

### 3.1. Schema Identity (5 Bảng)

| STT | Tên Bảng | Entity Name | Số Cột | Mô Tả |
|-----|----------|-------------|--------|-------|
| 1 | Người Dùng | Users | 15+ | Lưu trữ thông tin người dùng, tài khoản đăng nhập, vai trò, hồ sơ cá nhân |
| 2 | Nhật Ký Kiểm Tra | AuditLogs | 8+ | Ghi lại tất cả các hoạt động của người dùng trong hệ thống (thay đổi dữ liệu, truy cập) |
| 3 | Reset Mật Khẩu | PasswordResets | 5+ | Quản lý yêu cầu reset mật khẩu, token xác thực, thời hạn hết hạn |
| 4 | Cài Đặt Hệ Thống | Settings | 5+ | Lưu trữ các cài đặt toàn cục của hệ thống (cấu hình, thông số) |
| 5 | Thiết Bị Người Dùng | UserDevices | 8+ | Theo dõi các thiết bị đăng nhập của người dùng (mobile, máy tính, push notification) |

### 3.2. Schema Academic (22 Bảng)

| STT | Tên Bảng | Entity Name | Số Cột | Mô Tả |
|-----|----------|-------------|--------|-------|
| 1 | Sinh Viên | Students | 30+ | Thông tin cá nhân sinh viên, khóa học, ngành học, trạng thái học tập |
| 2 | Năm Học | AcademicYears | 5+ | Định nghĩa các năm học (ví dụ: 2023-2024, 2024-2025) |
| 3 | Chuyên Ngành | Majors | 8+ | Danh sách chuyên ngành đào tạo, mã chuyên ngành, mô tả |
| 4 | Khoa/Bộ Môn | Faculties | 8+ | Danh sách các khoa hoặc bộ môn trong trường |
| 5 | Môn Học | Subjects | 10+ | Danh mục môn học (mã, tên, tín chỉ, giờ học, trạng thái) |
| 6 | Lớp Học Phần | SubjectTeachings | 12+ | Triển khai môn học thành các lớp (thời gian bắt đầu, kết thúc, số buổi, phòng) |
| 7 | Giáo Viên Lớp HP | SubjectTeachingTeachers | 5+ | Gắn giáo viên với lớp học phần (hỗ trợ nhiều giáo viên) |
| 8 | Ghi Danh Học Phần | SubjectStudents | 5+ | Bảng nối giữa sinh viên và lớp học phần (ghi danh/enrollment) |
| 9 | Lịch Học | SubjectSchedules | 10+ | Chi tiết từng buổi học (thời gian bắt đầu-kết thúc, phòng, giáo viên, ghi chú) |
| 10 | Phòng Học | Rooms | 8+ | Danh sách phòng học, sức chứa, vị trí, trang thiết bị |
| 11 | Điểm Danh | Attendances | 8+ | Ghi nhận điểm danh sinh viên tại các buổi học (có/vắng, cảnh cáo) |
| 12 | Tài Liệu Môn Học | SubjectDocuments | 8+ | Tài liệu liên quan đến môn học (giáo trình, slide, bài tập) |
| 13 | Ghi Chú Đặc Biệt | SubjectSpecialNotes | 8+ | Ghi chú đặc biệt cho sinh viên (miễn điểm danh, ngoại lệ, v.v.) |
| 14 | Tiêu Chí Đánh Giá | EvaluationCriterias | 10+ | Định nghĩa tiêu chí đánh giá cho môn học (điểm giữa kỳ, cuối kỳ, bài tập) |
| 15 | Đánh Giá Sinh Viên | StudentEvaluations | 12+ | Đánh giá tổng hợp sinh viên trong một môn (tổng điểm, trạng thái pass/fail) |
| 16 | Chi Tiết Đánh Giá | StudentEvaluationDetails | 8+ | Điểm chi tiết từng tiêu chí đánh giá (điểm giữa kỳ, cuối kỳ, bài tập) |
| 17 | Kế Hoạch Kỳ Học | SemesterPlans | 8+ | Kế hoạch môn học theo từng kỳ cho mỗi ngành (ngành + năm = các môn phải học) |
| 18 | Môn Trong Kế Hoạch | SemesterSubjects | 8+ | Danh sách môn trong kế hoạch kỳ học (môn nào, bắt buộc hay tự chọn) |
| 19 | Học Phí Theo Kỳ | SemesterTuitions | 8+ | Học phí của sinh viên từng kỳ học (số tiền, trạng thái đã đóng) |
| 20 | Giáo Viên-Khoa | TeacherFaculties | 5+ | Gắn giáo viên với khoa/bộ môn (mỗi giáo viên thuộc một khoa) |
| 21 | Dự Án Sinh Viên | StudentProjects | 10 | Lưu dự án e-Portfolio do sinh viên sở hữu, công nghệ, vai trò, đóng góp và môn học liên quan để làm nguồn tạo CV |
| 22 | Lịch Sử Thực Tập | StudentInternships | 8 | Lưu đợt thực tập đã được đồng bộ sau quy trình xác nhận/phê duyệt, dùng làm kinh nghiệm có nguồn kiểm chứng trên CV |

### 3.3. Schema Exam (8 Bảng)

| STT | Tên Bảng | Entity Name | Số Cột | Mô Tả |
|-----|----------|-------------|--------|-------|
| 1 | Câu Hỏi | Questions | 8+ | Câu hỏi trong ngân hàng (nội dung, ảnh, mức độ khó) |
| 2 | Đáp Án Câu Hỏi | QuestionAnswers | 8+ | Các lựa chọn trả lời cho mỗi câu hỏi (đáp án, ảnh, đáp án đúng) |
| 3 | Bộ Câu Hỏi | QuestionSuites | 8+ | Nhóm câu hỏi theo chủ đề/môn học (tên bộ, môn, thời gian tạo) |
| 4 | Kỳ Thi Lớp HP | SubjectTeachingExams | 10+ | Kỳ thi cho một lớp học phần (bộ câu hỏi, phòng, thời gian, giáo viên coi thi) |
| 5 | Lần Thi | ExamAttempts | 8+ | Mỗi lần sinh viên tham gia thi (kỳ thi, thời gian thi, IP, trạng thái) |
| 6 | Kết Quả Thi | ExamResults | 10+ | Kết quả thi của sinh viên (điểm, lần thi, trạng thái pass/fail, ghi chú) |
| 7 | Câu Hỏi Trong Thi | ExamQuestionSelections | 8+ | Danh sách câu hỏi được chọn cho một lần thi (câu nào, thứ tự, điểm) |
| 8 | Đáp Án Trong Thi | ExamQuestionAnswers | 8+ | Đáp án sinh viên đã chọn khi làm bài thi (lần thi, câu hỏi, đáp án chọn) |

### 3.4. Schema Communication (3 Bảng)

| STT | Tên Bảng | Entity Name | Số Cột | Mô Tả |
|-----|----------|-------------|--------|-------|
| 1 | Mẫu Biểu Mẫu | FormTemplates | 8+ | Mẫu các biểu mẫu có thể yêu cầu (đơn xin học lại, xin miễn, v.v.) |
| 2 | Yêu Cầu Biểu Mẫu | FormRequests | 10+ | Yêu cầu từ sinh viên (loại mẫu, sinh viên, người duyệt, trạng thái) |
| 3 | Thông Báo Người Dùng | UserAnnouncements | 8+ | Thông báo hệ thống gửi đến người dùng (tiêu đề, nội dung, trạng thái đã đọc) |

---

## 4. Sơ Đồ Quan Hệ Dữ Liệu Chính

### Luồng Dữ Liệu Học Tập
```
Ngành (Major) 
  ↓
Kế Hoạch Kỳ Học (SemesterPlans)
  ↓
Môn Trong Kế Hoạch (SemesterSubjects)
  ↓
Môn Học (Subjects)
  ↓
Lớp Học Phần (SubjectTeachings)
  ↓
Ghi Danh Học Phần (SubjectStudents) ← Sinh Viên (Students)
  ↓
Lịch Học (SubjectSchedules)
  ↓
Điểm Danh (Attendances)
  ↓
Đánh Giá Sinh Viên (StudentEvaluations) → Kết Quả Thi (ExamResults)
```

### Luồng Dữ Liệu Thi Cử
```
Bộ Câu Hỏi (QuestionSuites)
  ↓
Câu Hỏi (Questions) + Đáp Án (QuestionAnswers)
  ↓
Kỳ Thi Lớp HP (SubjectTeachingExams)
  ↓
Lần Thi (ExamAttempts) ← Sinh Viên (Students)
  ↓
Kết Quả Thi (ExamResults)
  ↓
Câu Hỏi Trong Thi (ExamQuestionSelections) + Đáp Án Trong Thi (ExamQuestionAnswers)
```

### Luồng Dữ Liệu Yêu Cầu
```
Mẫu Biểu Mẫu (FormTemplates)
  ↓
Yêu Cầu Biểu Mẫu (FormRequests) ← Sinh Viên (Students) + Người Duyệt
  ↓
Trạng Thái (Đang Chờ / Phê Duyệt / Từ Chối)
```

---

## 5. Dữ Liệu Dùng Cho Tính Năng Tạo CV Tự Động

### 5.1. Phạm Vi Và Trạng Thái Hiện Tại

Tính năng nằm tại trang `/student/resume-builder`. Dữ liệu học vụ được chuẩn bị
bởi `AcademicService` qua API:

```http
GET /api/academic/resume/get-context-data/{studentId}
```

API yêu cầu token sinh viên và chỉ cho phép sinh viên đọc đúng dữ liệu thuộc
`studentId` trong token. Dữ liệu thực tập được giao diện tải thêm qua:

```http
GET /api/academic/students/{studentId}/internships
```

Khi tối ưu bằng AI ở chế độ thật, frontend dự kiến gửi dữ liệu đến:

```http
POST /api/ai/resume/optimize
```

Tuy nhiên, trong repository hiện tại chưa có AI Service/endpoint xử lý LLM.
`VITE_AI_OPTIMIZE_MODE` mặc định không phải `live`, nên nút **Tối ưu hóa CV
bằng AI** đang chạy dữ liệu mô phỏng ở frontend. Tương tự,
`VITE_RESUME_API_MODE` phải bằng `live` thì ngữ cảnh học vụ mới được tải từ
backend.

### 5.2. Luồng Dữ Liệu Tổng Quát

```text
identity.Users
   └─ họ tên, tên đăng nhập
             │
academic.Students ── Majors ── Faculties ── AcademicYears
             │
             ├─ StudentEvaluations ── SubjectTeachings ── Subjects
             │         └─ StudentEvaluationDetails ── EvaluationCriterias
             │                 → GPA + học phần đủ điều kiện + chuẩn đầu ra
             │
             ├─ StudentProjects
             │                 → dự án, công nghệ, vai trò, đóng góp
             │
             └─ StudentInternships
                       ↑ đồng bộ sau khi communication.FormRequests được duyệt
                               → kinh nghiệm thực tập đã xác nhận

Sinh viên nhập/chọn thêm: vị trí mục tiêu + JD + thông tin liên hệ
             ↓
Payload tối ưu CV
             ↓
AI chỉ diễn đạt lại → tóm tắt nghề nghiệp + nhóm kỹ năng
             ↓
Sinh viên kiểm tra/chỉnh sửa → in hoặc xuất PDF
```

### 5.3. Các Bảng Được Sử Dụng

#### 5.3.1. Bảng Được API Ngữ Cảnh CV Truy Vấn Trực Tiếp

| Schema | Bảng | Vai Trò Trong CV |
|---|---|---|
| `academic` | `Students` | Bảng gốc xác định chủ sở hữu CV, liên kết tài khoản, ngành và năm học |
| `identity` | `Users` | Cung cấp họ tên và tên đăng nhập qua internal API; chỉ lấy tài khoản đang hoạt động, chưa xóa |
| `academic` | `Majors` | Cung cấp mã và tên ngành học |
| `academic` | `Faculties` | Cung cấp tên khoa, có thể rỗng |
| `academic` | `AcademicYears` | Cung cấp tên khóa/năm học |
| `academic` | `StudentEvaluations` | Cung cấp `TotalScore`, là nguồn tính điểm môn và GPA |
| `academic` | `SubjectTeachings` | Nối một đánh giá với môn học cụ thể |
| `academic` | `Subjects` | Cung cấp mã môn, tên môn, số tín chỉ |
| `academic` | `StudentEvaluationDetails` | Cung cấp tên kết quả/tiêu chí chi tiết của học phần |
| `academic` | `EvaluationCriterias` | Cung cấp tên và mô tả tiêu chí để suy ra năng lực/chuẩn đầu ra |
| `academic` | `StudentProjects` | Cung cấp dự án e-Portfolio của sinh viên |
| `academic` | `StudentInternships` | Cung cấp lịch sử thực tập dùng trong phần kinh nghiệm |

#### 5.3.2. Bảng Nguồn Gián Tiếp Cho Thực Tập

| Schema | Bảng | Vai Trò |
|---|---|---|
| `communication` | `FormRequests` | Lưu khai báo thực tập, token xác nhận của doanh nghiệp, trạng thái xác nhận và phê duyệt. Khi nhà trường duyệt, dữ liệu cần thiết được sao chép sang `academic.StudentInternships` |

`communication.FormRequests` không được API CV join trực tiếp vì hệ thống dùng
mô hình schema-per-service. Hai service liên kết logic bằng
`StudentInternships.FormRequestID`, không có khóa ngoại EF xuyên service.
`FormTemplates` không tham gia trực tiếp vào luồng CV/thực tập hiện tại vì yêu
cầu thực tập được tạo bằng endpoint riêng và có thể không có `FormTemplateID`.

### 5.4. Chi Tiết Trường Dữ Liệu Database

#### 5.4.1. Hồ Sơ Sinh Viên

| Bảng.Trường | Bắt Buộc | Được Chuyển Thành | Ý Nghĩa Cho CV/AI |
|---|---:|---|---|
| `Students.Id` | Có | `student.studentId` | Định danh sinh viên, dùng để giới hạn quyền sở hữu và truy vấn tất cả dữ liệu liên quan |
| `Students.UserId` | Có | `student.userId` | Khóa liên kết logic đến `identity.Users.Id` |
| `Students.MajorId` | Có | Quan hệ đến `Majors` | Xác định ngành học |
| `Students.AcademicYearId` | Có | Quan hệ đến `AcademicYears` | Xác định khóa/năm học |
| `Students.IsDeleted` | Có | Bộ lọc | Chỉ lấy sinh viên chưa bị xóa mềm |
| `Users.FullName` | Có | `student.fullName` | Họ tên hiển thị ở đầu CV |
| `Users.UserName` | Có | `student.userName` | Hiện được backend trả về; có thể dùng làm mã sinh viên nếu nghiệp vụ bảo đảm hai giá trị là một |
| `Users.IsActived` | Có | Bộ lọc | Chỉ cho phép hồ sơ tài khoản đang hoạt động |
| `Users.IsDeleted` | Có | Bộ lọc | Loại tài khoản đã xóa mềm |
| `Majors.Code` | Có | `student.majorCode` | Mã ngành, hữu ích cho chuẩn hóa/phân tích |
| `Majors.Name` | Có | `student.majorName` | Tên ngành hiển thị trong phần học vấn và cung cấp ngữ cảnh chuyên môn cho AI |
| `Majors.FacultyId` | Không | Quan hệ đến `Faculties` | Xác định khoa nếu có |
| `Faculties.Name` | Không | `student.facultyName` | Tên khoa; backend trả về nhưng frontend hiện chưa dùng |
| `AcademicYears.Name` | Có | `student.academicYear` | Tên khóa/năm học; backend trả về nhưng frontend hiện chưa dùng |

Các trường nhạy cảm khác trong `Students`/`Users` như mật khẩu, salt, số giấy tờ,
thông tin cha mẹ, tôn giáo, dân tộc hoặc dữ liệu đoàn/đảng **không được truy vấn
và không được gửi cho AI**.

#### 5.4.2. Điểm, GPA Và Học Phần Tiêu Biểu

| Bảng.Trường | Được Chuyển Thành | Quy Tắc Hiện Tại | Ý Nghĩa Cho CV/AI |
|---|---|---|---|
| `StudentEvaluations.StudentId` | Bộ lọc | Chỉ lấy đánh giá của sinh viên đang tạo CV | Bảo đảm đúng chủ sở hữu |
| `StudentEvaluations.TotalScore` | `eligibleCourses[].score` và `gpa` | Chỉ nhận điểm khác `null`, từ 0 đến 10; nếu một môn có nhiều dòng thì lấy trung bình | Bằng chứng định lượng cho năng lực học thuật |
| `StudentEvaluations.IsDeleted` | Bộ lọc | Phải bằng `false` | Không dùng dữ liệu đã xóa |
| `StudentEvaluations.SubjectTeachingId` | Quan hệ | Phải nối được đến lớp học phần chưa xóa | Tìm môn tương ứng |
| `SubjectTeachings.SubjectId` | Quan hệ đến `Subjects` | Lớp học phần phải chưa xóa | Gom các lần đánh giá theo cùng môn |
| `Subjects.Id` | `eligibleCourses[].subjectId` | Dùng làm định danh lựa chọn | Tham chiếu ổn định cho frontend |
| `Subjects.SubjectCode` | `eligibleCourses[].subjectCode` | Sắp xếp tăng dần khi điểm bằng nhau | Hiển thị mã môn |
| `Subjects.Name` | `eligibleCourses[].subjectName` | Môn phải chưa xóa | Hiển thị tên môn và tạo ngữ cảnh kỹ năng |
| `Subjects.CreditPoint` | `eligibleCourses[].creditPoint` | Dùng làm trọng số GPA | Phản ánh khối lượng học tập |
| `StudentEvaluationDetails.EvaluationName` | `courseOutcomes[].name` | Ưu tiên nếu có; chi tiết phải chưa xóa | Tên năng lực/kết quả đánh giá |
| `StudentEvaluationDetails.EvaluationCriteriaId` | Quan hệ | Dùng khi `EvaluationName` rỗng hoặc cần mô tả | Nối đến tiêu chí chuẩn |
| `EvaluationCriterias.Name` | `courseOutcomes[].name` | Giá trị dự phòng cho `EvaluationName` | Nhãn năng lực có thể đưa vào prompt |
| `EvaluationCriterias.Description` | `courseOutcomes[].description` | Lấy mô tả không rỗng đầu tiên của cùng một tên | Cung cấp ngữ nghĩa chi tiết cho AI |

Quy tắc tính hiện tại:

- Điểm của một môn = trung bình tất cả `TotalScore` hợp lệ của môn đó, làm tròn
  2 chữ số.
- GPA = trung bình có trọng số theo `CreditPoint`; nếu tổng tín chỉ bằng 0 thì
  dùng trung bình cộng; kết quả làm tròn 2 chữ số.
- Chỉ môn có điểm trung bình từ `7.0/10` trở lên được đưa vào
  `eligibleCourses`.
- Học phần đủ điều kiện được sắp xếp theo điểm giảm dần, sau đó theo mã môn.
- Chuẩn đầu ra trùng tên được gộp không phân biệt chữ hoa/chữ thường.

Đây là GPA nội bộ được tính từ các dòng đánh giá hợp lệ mà service đọc được,
không phải trường GPA chính thức đã lưu sẵn và cũng chưa có quy tắc chọn lần
học/lần thi tốt nhất.

#### 5.4.3. Dự Án Sinh Viên (`academic.StudentProjects`)

| Trường | Kiểu/Ràng Buộc EF | Trường API | Mục Đích Cho CV/AI |
|---|---|---|---|
| `ProjectID` | `int`, identity, PK | `projectId` | Định danh dự án |
| `StudentID` | `uniqueidentifier`, FK `Students`, cascade delete | Không cần hiển thị | Xác định chủ sở hữu |
| `ProjectName` | `nvarchar(255)`, bắt buộc | `projectName` | Tên dự án |
| `TechStack` | `varchar(255)`, bắt buộc | `techStack` | Công nghệ/kỹ thuật đã sử dụng |
| `ProjectDescription` | `nvarchar(max)`, có thể rỗng | `projectDescription` | Bối cảnh, mục tiêu và phạm vi dự án |
| `SourceCodeUrl` | `varchar(255)`, có thể rỗng | `sourceCodeUrl` | Link GitHub/source code để minh chứng |
| `TeamSize` | `int`, mặc định 1, phải `>= 1` | `teamSize` | Phân biệt dự án cá nhân/nhóm |
| `MyRole` | `nvarchar(100)`, có thể rỗng | `myRole` | Vai trò thực tế của sinh viên |
| `MyContributions` | `nvarchar(max)`, có thể rỗng | `myContributions` | Phần việc/kết quả cá nhân; là dữ liệu quan trọng nhất để AI viết bullet không bịa đặt |
| `MappedCourseID` | `uniqueidentifier`, FK `Subjects`, có thể rỗng, set null khi môn bị xóa | `mappedCourseId`, `mappedCourseCode`, `mappedCourseName` | Chứng minh dự án liên quan đến học phần nào |

#### 5.4.4. Thực Tập (`academic.StudentInternships`)

| Trường | Kiểu/Ràng Buộc EF | Trường API | Mục Đích Cho CV/AI |
|---|---|---|---|
| `InternshipID` | `int`, identity, PK | `internshipId` | Định danh đợt thực tập |
| `StudentID` | `uniqueidentifier`, FK `Students`, cascade delete | `studentId` ở API danh sách | Xác định chủ sở hữu |
| `CompanyName` | `nvarchar(255)`, bắt buộc | `companyName` | Tên doanh nghiệp |
| `Position` | `nvarchar(100)`, bắt buộc | `position` | Vị trí thực tập |
| `StartDate` | `date`, bắt buộc | `startDate` | Thời điểm bắt đầu |
| `EndDate` | `date`, có thể rỗng và không nhỏ hơn `StartDate` | `endDate` | Thời điểm kết thúc hoặc “Hiện tại” |
| `TaskDescription` | `nvarchar(max)`, có thể rỗng | `taskDescription` | Nhiệm vụ thực tế để AI diễn đạt thành kinh nghiệm |
| `FormRequestID` | `uniqueidentifier`, có thể rỗng, index | `formRequestId` ở API danh sách | Truy vết yêu cầu thực tập đã được duyệt bên CommunicationService |

Luồng tạo dữ liệu thực tập:

1. Sinh viên khai báo `CompanyName`, `Position`, `MentorEmail`, `StartDate`,
   `EndDate`, `TaskDescription`.
2. Dữ liệu được lưu trong `communication.FormRequests.VerificationData` dưới
   dạng JSON và gửi token xác nhận cho doanh nghiệp.
3. Doanh nghiệp xác nhận thông tin, chấm điểm và có thể ghi nhận xét.
4. Nhà trường duyệt yêu cầu.
5. CommunicationService gọi internal API để sao chép các trường cần cho CV sang
   `academic.StudentInternships`; `MentorEmail`, điểm và nhận xét của doanh
   nghiệp không được sao chép sang CV.

### 5.5. Hợp Đồng Dữ Liệu API Ngữ Cảnh CV

API Academic hiện trả về cấu trúc logic sau:

| Nhóm | Trường | Nguồn | Tác Dụng Dự Kiến |
|---|---|---|---|
| `student` | `studentId`, `userId` | `Students` | Định danh và truy vết, không cần đưa vào nội dung prompt tự do |
| `student` | `fullName`, `userName` | `identity.Users` | Họ tên và mã/tên đăng nhập |
| `student` | `majorCode`, `majorName` | `Majors` | Ngữ cảnh ngành học |
| `student` | `facultyName` | `Faculties` | Ngữ cảnh khoa |
| `student` | `academicYear` | `AcademicYears` | Khóa/năm học |
| root | `gpa` | Giá trị tính từ đánh giá | Điểm trung bình 0–10 hoặc `null` |
| `eligibleCourses[]` | `subjectId`, `subjectCode`, `subjectName`, `creditPoint`, `score` | Đánh giá + lớp học phần + môn | Danh sách môn nổi bật |
| `eligibleCourses[].courseOutcomes[]` | `name`, `description` | Chi tiết đánh giá + tiêu chí | Nguồn suy ra kiến thức/kỹ năng có căn cứ |
| `projects[]` | Toàn bộ trường ở mục 5.4.3 | `StudentProjects` + môn được map | Dự án thực tế |
| `approvedInternships[]` | `internshipId`, `companyName`, `position`, `startDate`, `endDate`, `taskDescription` | `StudentInternships` | Kinh nghiệm thực tập |

### 5.6. Dữ Liệu Frontend Đang Gửi Cho Endpoint AI

Khi `VITE_AI_OPTIMIZE_MODE=live`, frontend gửi toàn bộ đối tượng `ResumeData`:

| Nhóm Payload | Trường | Nguồn Hiện Tại | AI Nên Làm Gì |
|---|---|---|---|
| `studentInfo` | `fullName` | API Academic khi live; mock khi không live | Giữ nguyên, không tự sửa |
| `studentInfo` | `studentCode` | Hiện là dữ liệu mẫu/edit tay; `student.userName` từ API chưa được map vào | Giữ nguyên sau khi nguồn được chuẩn hóa |
| `studentInfo` | `email`, `phone`, `location` | Hiện là dữ liệu mẫu/edit tay, chưa có trong API ngữ cảnh | Chỉ định dạng, không suy đoán |
| `studentInfo` | `major` | `student.majorName` khi live | Dùng làm ngữ cảnh chuyên môn |
| `studentInfo` | `university` | Hiện là chuỗi mẫu/edit tay | Giữ nguyên; nên chuyển thành cấu hình trường |
| `studentInfo` | `gpa` | API Academic khi live | Giữ nguyên số liệu |
| root | `selectedCourses` | Mảng `subjectId` được sinh viên chọn | Xác định môn được phép dùng |
| `projects[]` | `name`, `techStack`, `githubUrl`, `scale`, `role`, `contribution` | API dự án và chỉnh sửa ở frontend | Viết lại bullet từ dữ kiện thật |
| `internships[]` | `id`, `company`, `position`, `startDate`, `endDate`, `duration`, `responsibilities`, `source` | Các đợt thực tập sinh viên chọn | Viết lại trách nhiệm, không thêm kinh nghiệm mới |
| `aiContext` | `targetRole` | Sinh viên nhập | Xác định chức danh mục tiêu |
| `aiContext` | `jobDescription` | Sinh viên dán JD, tối đa 1.500 ký tự | Trích kỹ năng/từ khóa để căn chỉnh CV |
| `cvOutput` | `professionalSummary` | Nội dung hiện có hoặc AI tạo trước đó | Tạo/viết lại tóm tắt |
| `cvOutput.skills` | `knowledge`, `functional`, `interpersonal` | Nội dung hiện có hoặc AI tạo trước đó | Phân nhóm và viết lại kỹ năng |

### 5.7. Các Khoảng Trống Cần Xử Lý Trước Khi Áp Dụng AI Thật

1. **Endpoint AI chưa tồn tại trong repository**: cần tạo service thực thi
   `/api/ai/resume/optimize`, xác thực người dùng, giới hạn payload, timeout,
   logging an toàn và schema output cố định.
2. **Payload chỉ gửi ID học phần**: `selectedCourses` hiện chỉ chứa
   `subjectId`; tên môn, điểm, tín chỉ và `courseOutcomes` không nằm trong
   `ResumeData` gửi cho AI. AI sẽ không hiểu các ID này nếu không tự gọi lại
   AcademicService. Nên gửi các object học phần đã chọn đầy đủ.
3. **Một số trường backend chưa được frontend dùng**:
   `student.userId`, `majorCode`, `facultyName`, `academicYear`,
   `eligibleCourses.creditPoint`, `projects.projectDescription`,
   `mappedCourseId`, `mappedCourseCode`, `mappedCourseName` và danh sách
   `approvedInternships` trong context.
4. **Một số trường trên CV vẫn là mock**: `studentCode`, `email`, `phone`,
   `location`, `university` và khoảng thời gian học `2023–2027`. Cần bổ sung
   nguồn dữ liệu chính thức hoặc bắt sinh viên xác nhận trước khi gửi AI.
5. **Dự án chỉnh sửa/thêm trên giao diện chưa được ghi về database**:
   giao diện chỉ thay đổi React state; chưa có API CRUD `StudentProjects`.
6. **Bản CV chưa được lưu bền vững**: nhãn “Đã lưu” hiện không tương ứng với
   API/local storage. Reload trang sẽ mất chỉnh sửa trong phiên.
7. **Thông tin thực tập bị tải hai lần**: context đã trả
   `approvedInternships`, nhưng frontend lại gọi endpoint danh sách thực tập
   riêng. Nên chọn một nguồn để tránh lệch dữ liệu.
8. **Tên `approvedInternships` phụ thuộc quy trình ghi dữ liệu**:
   `ResumeDataService` đọc toàn bộ `StudentInternships` theo sinh viên, không
   kiểm tra `FormRequestID` khác `null` hay gọi CommunicationService để xác
   nhận trạng thái. Hiện tính “đã duyệt” được bảo đảm bởi luồng đồng bộ, không
   phải bởi điều kiện truy vấn.
9. **Migration chưa đồng bộ EF model**: snapshot/baseline migration hiện chưa
   chứa `StudentProjects` và `StudentInternships`.

### 5.8. Payload Khuyến Nghị Cho AI

Nên tạo một DTO riêng, chỉ chứa dữ liệu sinh viên đã chọn và đã xác nhận:

```json
{
  "targetRole": "Frontend Developer Intern",
  "jobDescription": "...",
  "profile": {
    "fullName": "...",
    "studentCode": "...",
    "email": "...",
    "phone": "...",
    "location": "...",
    "majorName": "...",
    "facultyName": "...",
    "academicYear": "...",
    "university": "...",
    "gpa": 8.12
  },
  "selectedCourses": [
    {
      "subjectCode": "CT312",
      "subjectName": "Lập trình Web",
      "creditPoint": 3,
      "score": 8.2,
      "outcomes": [
        {
          "name": "Phát triển ứng dụng web",
          "description": "..."
        }
      ]
    }
  ],
  "projects": [
    {
      "name": "...",
      "description": "...",
      "techStack": ["React", "TypeScript"],
      "teamSize": 4,
      "role": "Frontend Developer",
      "contributions": ["..."],
      "sourceCodeUrl": "...",
      "mappedCourse": "CT312"
    }
  ],
  "internships": [
    {
      "companyName": "...",
      "position": "...",
      "startDate": "2026-01-01",
      "endDate": "2026-04-30",
      "taskDescription": "...",
      "source": "school-approved"
    }
  ]
}
```

Output của AI nên là JSON có schema cố định và chỉ chứa nội dung được phép viết
lại, ví dụ `professionalSummary`, `skills` và các bullet đề xuất cho từng
`projectId`/`internshipId`. Các trường định danh, họ tên, liên hệ, điểm, ngày,
công ty và URL phải được copy từ dữ liệu nguồn, không cho model tự sinh.

### 5.9. Nguyên Tắc An Toàn Và Chất Lượng Dữ Liệu AI

- Chỉ gửi các trường đã whitelist trong DTO, không serialize trực tiếp entity
  database.
- Không gửi `PasswordHash`, `PasswordSalt`, `IdentificationNumber`,
  `EmployerToken`, `MentorEmail` hoặc dữ liệu gia đình/chính trị/tôn giáo.
- Cho sinh viên xem và xác nhận dữ liệu nguồn trước khi gọi AI.
- Prompt phải yêu cầu không tạo thêm kinh nghiệm, công nghệ, thành tích hoặc số
  liệu không có trong nguồn.
- Lưu `sourceId` và phiên bản dữ liệu cho mỗi câu/bullet AI sinh ra để có thể
  truy vết.
- Validate output bằng JSON schema; giới hạn độ dài, loại HTML/script và URL
  không an toàn.
- Tách log kỹ thuật khỏi nội dung CV; không ghi nguyên JD/CV vào log nếu không
  có mục đích và thời hạn lưu rõ ràng.
- Chỉ số `Content Preservation` và `Job Alignment` trên giao diện hiện là giá
  trị mock/fallback. Khi triển khai thật cần định nghĩa mô hình embedding,
  ngưỡng, phiên bản và cách tính có thể kiểm thử.

---

## 6. Tóm Tắt

**Tổng Số Bảng Theo EF Model:** 38 bảng

**Tổng Số Người Dùng:** Admin, Giáo Viên, Sinh Viên, Nhân Viên Đào Tạo

**Phạm Vi Quản Lý:** Từ tuyển sinh → học tập → thi cử → công bố kết quả → tạo CV từ dữ liệu học vụ và trải nghiệm đã xác nhận
