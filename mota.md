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
- **Số Bảng Tổng Cộng**: 36 bảng
- **Phân Bổ**: 
  - Identity: 5 bảng
  - Academic: 20 bảng
  - Exam: 8 bảng
  - Communication: 3 bảng
- **Kết Nối**: 4 connection strings riêng biệt (một cho mỗi dịch vụ)

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

---

## 3. Chi Tiết 36 Bảng Dữ Liệu

### 3.1. Schema Identity (5 Bảng)

| STT | Tên Bảng | Entity Name | Số Cột | Mô Tả |
|-----|----------|-------------|--------|-------|
| 1 | Người Dùng | Users | 15+ | Lưu trữ thông tin người dùng, tài khoản đăng nhập, vai trò, hồ sơ cá nhân |
| 2 | Nhật Ký Kiểm Tra | AuditLogs | 8+ | Ghi lại tất cả các hoạt động của người dùng trong hệ thống (thay đổi dữ liệu, truy cập) |
| 3 | Reset Mật Khẩu | PasswordResets | 5+ | Quản lý yêu cầu reset mật khẩu, token xác thực, thời hạn hết hạn |
| 4 | Cài Đặt Hệ Thống | Settings | 5+ | Lưu trữ các cài đặt toàn cục của hệ thống (cấu hình, thông số) |
| 5 | Thiết Bị Người Dùng | UserDevices | 8+ | Theo dõi các thiết bị đăng nhập của người dùng (mobile, máy tính, push notification) |

### 3.2. Schema Academic (20 Bảng)

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

## 5. Tóm Tắt

**Tổng Số Bảng:** 36 bảng  
**Tổng Số Người Dùng:** Admin, Giáo Viên, Sinh Viên, Nhân Viên Đào Tạo  
**Phạm Vi Quản Lý:** Từ tuyển sinh → học tập → thi cử → công bố kết quả
