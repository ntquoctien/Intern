# Student Portal — Tổng quan

**Route đề xuất:** `/student/dashboard`  
**Mục tiêu:** giúp sinh viên biết nhanh hồ sơ học vụ, lịch gần nhất và nội dung cần chú ý.

## Bố cục

- Header chào theo họ tên, kèm ngày hiện tại.
- Hàng đầu: thẻ hồ sơ học vụ và các shortcut quan trọng.
- Khu vực chính hai cột: lịch sắp tới bên trái, thông báo bên phải.
- Hàng cuối: lớp học phần đang liên quan, kỳ thi/kết quả gần đây.
- Mobile xếp một cột theo thứ tự ưu tiên: lịch → thông báo → lớp → kết quả.

## Nội dung hiển thị

- Họ tên, mã sinh viên đã chốt, ngành, khoa, khóa/năm học, trạng thái học.
- “Lịch tiếp theo”: giờ, môn, lớp học phần, phòng, giảng viên.
- Tối đa 3–5 lịch sắp tới và nút “Xem thời khóa biểu”.
- Thông báo mới/bắt buộc đọc: loại, preview, thời gian.
- Lớp học phần: mã/tên môn, tên lớp, khoảng thời gian.
- Kỳ thi/kết quả gần nhất: tên kỳ thi, môn, thời gian hoặc điểm raw được phép công bố.

## Tương tác và trạng thái

- Mỗi card liên kết thẳng tới page chi tiết và giữ đúng ngữ cảnh.
- Widget lỗi độc lập, không làm toàn dashboard trắng.
- Skeleton theo từng vùng; empty state cụ thể như “Không có lịch sắp tới”.
- Nếu thiếu label cross-service, hiện “Chưa xác định” thay vì GUID.

## Nguồn dữ liệu

- `Users`, `Students`, `Majors`, `Faculties`, `AcademicYears`.
- `SubjectStudents`, `SubjectTeachings`, `Subjects`, `SubjectSchedules`, `Rooms`.
- `SubjectTeachingExams`, `ExamResults`, `UserAnnouncements`.

## Không được hiển thị khi chưa có quy tắc

- GPA, xếp loại, đạt/rớt, tín chỉ tích lũy.
- Tỷ lệ chuyên cần hoặc cảnh báo vắng học.
- Công nợ học phí.

## Đối chiếu schema database (22/07/2026)

- Hồ sơ: `Users(Id, FullName, ProfilePicUrl, IsActived)`, `Students(Id, UserId, AcademicYearId, MajorId, Nickname, StudyStatus, IsGraduated, HasIssue)`, `Majors`, `Faculties`, `AcademicYears`.
- Học tập: `SubjectStudents`, `SubjectTeachings(Id, SubjectId, Name, StartDate, EndDate, TotalSessions, RoomIdDefault)`, `Subjects`, `SubjectSchedules`, `Rooms`, `TeacherFaculties`.
- Liên lạc/thi: `UserAnnouncements(Id, Type, Message, CreationDate, Status, DeepLink, DeepLinkParam, EnforceRead, NotificationType)`, `SubjectTeachingExams`, `ExamResults`.

