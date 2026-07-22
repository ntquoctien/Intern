# Management Portal — Phân công giảng viên

**Route đề xuất:** `/management/teaching/assignments`  
**Mục tiêu:** xem quan hệ giảng viên–lớp học phần và vai trò chính/phụ.

## Bố cục

- Toggle “Theo giảng viên” / “Theo lớp”.
- Table server-side; nhóm row hoặc expandable row để xem quan hệ.
- Summary theo khoa chỉ dùng số đếm có căn cứ.

## Nội dung hiển thị

- Giảng viên, mã nội bộ, khoa.
- Môn, lớp học phần.
- Vai trò “Giảng viên chính” từ `IsMainTeacher`, ngược lại dùng nhãn phụ đã phê duyệt.
- Khoảng thời gian lớp.

## Bộ lọc

- Khoa, giảng viên, môn, lớp, vai trò, khoảng ngày.
- Autocomplete server-side cho giảng viên/môn/lớp.

## Nguồn dữ liệu

- `SubjectTeachingTeachers`, `TeacherFaculties`, `Users`, `SubjectTeachings`, `Subjects`.
- Missing user label hiển thị “Không xác định” kèm cờ dữ liệu ở quản trị, không dùng GUID làm tên.

## Đối chiếu schema database (22/07/2026)

- `SubjectTeachingTeachers` (524): `Id`, `SubjectTeachingId`, `TeacherId`, `IsMainTeacher`, `IsDeleted`.
- Resolve lớp/môn từ SubjectTeachings/Subjects; giảng viên từ TeacherFaculties và Users; khoa từ Faculties.
- Ghi danh là bảng riêng `SubjectStudents(Id, SubjectTeachingId, StudentId)`, không trộn nghĩa với phân công.

