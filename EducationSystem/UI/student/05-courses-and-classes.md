# Student Portal — Môn và lớp học phần của tôi

**Route đề xuất:** `/student/subjects`  
**Mục tiêu:** tập hợp các lớp học phần mà sinh viên đã được ghi danh.

## Bố cục

- Desktop dùng table; mobile dùng course cards.
- Filter bar phía trên; click row/card mở drawer hoặc trang detail.
- Detail dùng tabs: Tổng quan, Giảng viên, Lịch học, Tài liệu, Liên quan.

## Danh sách hiển thị

- Mã và tên môn.
- Tên lớp học phần.
- Số tín chỉ, tổng buổi nếu có.
- Ngày bắt đầu/kết thúc.
- Phòng mặc định.
- Giảng viên chính/phụ nếu resolve được.
- Trạng thái thời gian trung tính: sắp diễn ra, đang trong khoảng, đã qua.

## Bộ lọc

- Search mã/tên môn hoặc tên lớp.
- Khoảng ngày; trạng thái thời gian.
- Không filter học kỳ/ngành nếu chưa có quan hệ được xác nhận.

## Detail

- Thông tin môn và lớp; danh sách giảng viên.
- Các buổi học và link tới thời khóa biểu.
- Tài liệu môn nếu được công bố.
- Ghi chú đặc biệt liên quan chính sinh viên nếu chính sách cho phép.
- Shortcut tới điểm danh, kỳ thi và kết quả của lớp.

## Nguồn dữ liệu

- `SubjectStudents`, `SubjectTeachings`, `Subjects`, `Rooms`.
- `SubjectTeachingTeachers`, `TeacherFaculties`, `Users`.
- `SubjectSchedules`, `SubjectDocuments`, `SubjectSpecialNotes`.

## Giới hạn

- `SubjectTeaching` là lớp học phần, không phải lớp hành chính.
- Không tự gắn một lớp học phần với ngành/học kỳ khi database chưa có quan hệ chắc chắn.

