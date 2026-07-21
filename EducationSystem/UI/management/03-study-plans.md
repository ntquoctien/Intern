# Management Portal — Kế hoạch đào tạo

**Route đề xuất:** `/management/education/plans`  
**Mục tiêu:** xem kế hoạch học kỳ theo ngành và khóa/năm học.

## Bố cục

- Filter bar cố định phía trên table.
- Table kế hoạch; click row mở detail page.
- Detail header + summary + table môn trong kế hoạch.

## Danh sách kế hoạch

- Ngành, khoa, khóa/năm học, học kỳ.
- Ngày bắt đầu/kết thúc, active.
- Số môn và tổng tín chỉ kế hoạch.
- Filter: khoa, ngành, khóa, học kỳ, active; search theo mã/tên ngành.

## Detail kế hoạch

- Thông tin plan và timeline học kỳ.
- Mã/tên môn snapshot, tín chỉ, subject catalog được resolve.
- Cảnh báo khi `SubjectId` null hoặc snapshot code/name lệch danh mục.
- Link từ môn tới danh mục môn và lớp học phần liên quan nếu có quan hệ chắc chắn.

## Nguồn và giới hạn

- `SemesterPlans`, `SemesterSubjects`, `Majors`, `Faculties`, `AcademicYears`, `Subjects`.
- Không gọi đây là curriculum version; chưa có loại môn bắt buộc/tự chọn/tiên quyết.

