# Management Portal — Giảng viên

**Route đề xuất:** `/management/people/teachers`  
**Mục tiêu:** tra cứu giảng viên theo khoa và các lớp/lịch được phân công.

## Bố cục

- Search/filter ở đầu, table bên dưới.
- Detail có tabs: Hồ sơ, Khoa, Lớp phụ trách, Lịch giảng.

## Danh sách

- Họ tên, mã tài khoản/mã nội bộ.
- Khoa.
- Tag trưởng khoa từ `IsHeadOfFaculty`.
- Trạng thái tài khoản.
- Số lớp được phân công; số lớp làm giảng viên chính nếu aggregate được.
- Filter khoa, trưởng khoa, active; search tên/mã.

## Detail

- Thông tin user cơ bản đã mask.
- Khoa liên kết.
- Lớp học phần: môn, tên lớp, vai trò chính/phụ, thời gian.
- Lịch giảng: thời gian, phòng, lớp/môn.

## Nguồn và giới hạn

- `TeacherFaculties`, `Users`, `Faculties`, `SubjectTeachingTeachers`, `SubjectTeachings`, `Subjects`, `SubjectSchedules`.
- Không hiển thị học hàm, học vị hoặc chức danh vì database không có.

