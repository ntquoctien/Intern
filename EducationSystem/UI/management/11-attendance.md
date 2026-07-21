# Management Portal — Điểm danh

**Route đề xuất:** `/management/teaching/attendance`  
**Mục tiêu:** tìm và đối chiếu bản ghi điểm danh theo sinh viên, lớp và buổi học.

## Bố cục

- Filter bar nâng cao có thể collapse.
- Table server-side với cột sinh viên, buổi học và lớp được ghim.
- Detail drawer liên kết tới sinh viên, lịch và lớp.

## Nội dung hiển thị

- Mã/họ tên sinh viên.
- Mã/tên môn, lớp học phần.
- Bắt đầu/kết thúc, phòng.
- Status tag đã map, notes.
- Người tạo/ghi nhận đã resolve và creation date khi cần audit nghiệp vụ.

## Bộ lọc

- Khoa qua môn, môn, lớp, sinh viên, phòng.
- Khoảng ngày, status.
- Search tên/mã sinh viên; autocomplete server-side.

## Nguồn và giới hạn

- `Attendances` composition `Students/Users`, `SubjectSchedules`, `SubjectTeachings`, `Subjects`, `Rooms`.
- Dataset 93.865 dòng: bắt buộc paging/filter backend.
- Chưa phát hành tỷ lệ chuyên cần hoặc nhãn status cho đến khi xác nhận semantics 1–4 và mẫu số.

