# Management Portal — Lớp học phần và ghi danh

**Route đề xuất:** `/management/teaching/classes`  
**Mục tiêu:** tra cứu lớp học phần và toàn bộ sinh viên/giảng viên/lịch/thi liên quan.

## Danh sách

- Tên lớp, mã/tên môn, khoa lấy từ môn.
- Ngày bắt đầu/kết thúc, tổng số buổi.
- Phòng mặc định, giảng viên chính.
- Sĩ số từ `SubjectStudents`.
- Filter môn, khoa, giảng viên, phòng, khoảng ngày; search lớp/môn.

## Detail tabs

- Tổng quan: thông tin lớp, môn, phòng, khoảng thời gian.
- Sinh viên: mã, họ tên, ngành, khóa; server paging.
- Giảng viên: chính/phụ và khoa.
- Lịch: các buổi theo thời gian/phòng.
- Điểm danh: summary/raw records sau khi chốt enum.
- Kỳ thi/kết quả: kỳ thi của lớp và link drill-down.

## Nguồn dữ liệu

- `SubjectTeachings`, `Subjects`, `SubjectStudents`, `SubjectTeachingTeachers`, `TeacherFaculties`, `Rooms`, `SubjectSchedules`, các bảng exam.

## Giới hạn

- Không gọi là lớp hành chính.
- Không gán lớp duy nhất cho ngành/học kỳ nếu schema chưa có quan hệ xác định.

