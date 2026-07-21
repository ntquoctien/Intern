# Management Portal — Danh mục môn học

**Route đề xuất:** `/management/education/subjects`  
**Mục tiêu:** tra cứu môn học và đi tới kế hoạch, lớp học phần, tài liệu liên quan.

## Bố cục và danh sách

- Table gồm mã môn, tên, khoa, tín chỉ, tổng giờ, active.
- Search mã/tên; filter khoa, active, khoảng tín chỉ.
- Mã/tên được ghim; ghi chú không đưa nguyên văn lên table.

## Detail

- Header mã + tên + active.
- Tổng quan: khoa, tín chỉ, tổng giờ, note.
- Tabs: Kế hoạch có môn, Lớp học phần, Tài liệu, Ghi chú đặc biệt có quyền.
- Các số đếm liên kết tới list đã filter.

## Nguồn dữ liệu

- `Subjects`, `Faculties`, `SemesterSubjects`, `SubjectTeachings`, `SubjectDocuments`, `SubjectSpecialNotes`.

## Trạng thái

- `SubjectDocuments` hiện 0 dòng; tab hiển thị empty state thật.
- Không tự bổ sung CLO/PLO, chương, chủ đề hoặc tiên quyết vì chưa có data model.

