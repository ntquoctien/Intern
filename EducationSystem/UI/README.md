# UI Specification — University Information Portals

Thư mục này tách mô tả UI thành **một file cho mỗi page** trong phạm vi website hoàn chỉnh. Nội dung được dẫn xuất từ [PRD](../docs/PRD_UNIVERSITY_INFORMATION_PORTALS.md) và database hiện tại.

## Quy ước trạng thái dữ liệu

- **Sẵn sàng:** có bảng, quan hệ và bản ghi để xây read UI.
- **Schema rỗng:** có bảng nhưng snapshot hiện tại 0 dòng; page phải dùng empty state thật.
- **Thiếu quy tắc:** có giá trị raw nhưng chưa đủ căn cứ gán nhãn/tính chỉ số.
- **Chưa có dữ liệu:** không được tạo nội dung chỉ để giống website đại học khác.

## Student Portal — 13 pages

| # | Page | Route đề xuất | File |
|---:|---|---|---|
| 1 | Đăng nhập | `/student/login` | [01-login.md](student/01-login.md) |
| 2 | Tổng quan | `/student/dashboard` | [02-overview.md](student/02-overview.md) |
| 3 | Hồ sơ của tôi | `/student/profile` | [03-profile.md](student/03-profile.md) |
| 4 | Chương trình/kế hoạch | `/student/program` | [04-program.md](student/04-program.md) |
| 5 | Môn và lớp học phần | `/student/subjects` | [05-courses-and-classes.md](student/05-courses-and-classes.md) |
| 6 | Thời khóa biểu | `/student/schedule` | [06-timetable.md](student/06-timetable.md) |
| 7 | Điểm danh | `/student/attendance` | [07-attendance.md](student/07-attendance.md) |
| 8 | Kỳ thi và kết quả | `/student/exam-results` | [08-exams-and-results.md](student/08-exams-and-results.md) |
| 9 | Đánh giá học tập | `/student/evaluations` | [09-evaluations.md](student/09-evaluations.md) |
| 10 | Thông báo | `/student/announcements` | [10-announcements.md](student/10-announcements.md) |
| 11 | Tài liệu môn học | `/student/documents` | [11-documents.md](student/11-documents.md) |
| 12 | Học phí | `/student/tuition` | [12-tuition.md](student/12-tuition.md) |
| 13 | Biểu mẫu và yêu cầu | `/student/form-requests` | [13-forms-and-requests.md](student/13-forms-and-requests.md) |

## Management Portal — 17 pages

| # | Page | Route đề xuất | File |
|---:|---|---|---|
| 1 | Tổng quan điều hành | `/management/overview` | [01-overview.md](management/01-overview.md) |
| 2 | Khoa, ngành và năm học | `/management/education/structure` | [02-faculties-majors-years.md](management/02-faculties-majors-years.md) |
| 3 | Kế hoạch đào tạo | `/management/education/plans` | [03-study-plans.md](management/03-study-plans.md) |
| 4 | Sinh viên | `/management/people/students` | [04-students.md](management/04-students.md) |
| 5 | Giảng viên | `/management/people/teachers` | [05-teachers.md](management/05-teachers.md) |
| 6 | Tài khoản | `/management/people/users` | [06-users.md](management/06-users.md) |
| 7 | Danh mục môn | `/management/education/subjects` | [07-subjects.md](management/07-subjects.md) |
| 8 | Lớp học phần và ghi danh | `/management/teaching/classes` | [08-course-classes.md](management/08-course-classes.md) |
| 9 | Phân công giảng viên | `/management/teaching/assignments` | [09-teaching-assignments.md](management/09-teaching-assignments.md) |
| 10 | Phòng và lịch | `/management/teaching/schedule` | [10-rooms-and-schedules.md](management/10-rooms-and-schedules.md) |
| 11 | Điểm danh | `/management/teaching/attendance` | [11-attendance.md](management/11-attendance.md) |
| 12 | Kỳ thi, lượt thi và kết quả | `/management/assessment/results` | [12-exams-and-results.md](management/12-exams-and-results.md) |
| 13 | Ngân hàng câu hỏi | `/management/assessment/question-suites` | [13-question-bank.md](management/13-question-bank.md) |
| 14 | Đánh giá sinh viên | `/management/assessment/evaluations` | [14-student-evaluations.md](management/14-student-evaluations.md) |
| 15 | Thông báo | `/management/communication/announcements` | [15-announcements.md](management/15-announcements.md) |
| 16 | Mẫu biểu và yêu cầu | `/management/forms/requests` | [16-forms-and-requests.md](management/16-forms-and-requests.md) |
| 17 | Hệ thống có giới hạn | `/management/system` | [17-system.md](management/17-system.md) |

## App shell dùng chung

### Header

- Logo và tên trường; tên cổng hiện tại.
- Tìm kiếm trong đúng phạm vi quyền.
- Thông báo, avatar và menu tài khoản.
- Không hiển thị nút chuyển cổng nếu người dùng không có quyền ở cổng còn lại.

### Navigation

- Student: ưu tiên 4–5 tác vụ chính trên mobile; phần còn lại trong menu.
- Management: sidebar nhóm Cơ cấu đào tạo, Con người, Giảng dạy, Khảo thí, Truyền thông, Hệ thống.
- Route, filter, sort, tab và paging được lưu trong URL.

### List/detail pattern

- Mọi list lớn phân trang/lọc/sắp xếp phía server.
- ID quan hệ được đổi thành mã/tên; không dùng GUID làm nội dung cho người dùng.
- List chỉ có field cần scan; detail chứa các section/tabs liên quan.
- Loading dùng skeleton; empty, error, retry và permission denied là các trạng thái riêng.

### Responsive và accessibility

- Student Portal ưu tiên mobile từ 360 px; table chuyển card/list khi cần.
- Management Portal tối ưu desktop 1280 px, vẫn dùng được ở tablet 768 px.
- Mục tiêu WCAG 2.1 AA; keyboard/focus/label/contrast đầy đủ.
- Trạng thái không chỉ biểu diễn bằng màu.

## Hàng rào dữ liệu bắt buộc

- Không tính GPA, điểm chữ, đạt/rớt hoặc điểm chính thức từ kết quả raw.
- Không tính tỷ lệ chuyên cần khi chưa chốt semantics `Attendance.Status` 1–4 và mẫu số.
- Không tính tín chỉ tích lũy/tiến độ tốt nghiệp từ kế hoạch học kỳ.
- Không gọi lớp học phần là lớp hành chính.
- Không tạo học hàm/học vị giảng viên, curriculum version, môn tiên quyết, CLO/PLO hoặc công nợ khi database chưa có.
- Không trả password hash/salt, push token hoặc thông tin định danh không cần thiết.

