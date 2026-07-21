# Student Portal — Điểm danh

**Route đề xuất:** `/student/attendance`  
**Mục tiêu:** cho sinh viên xem bản ghi điểm danh của chính mình theo buổi học.

## Bố cục

- Filter bar: môn/lớp, khoảng ngày, trạng thái.
- Summary row chỉ dùng bộ đếm theo status sau khi enum được xác nhận.
- Desktop table; mobile list theo ngày.

## Nội dung hiển thị

- Ngày và giờ buổi học.
- Mã/tên môn, lớp học phần.
- Phòng.
- Trạng thái điểm danh bằng tag có text.
- Ghi chú liên quan sinh viên nếu được phép.
- Thời điểm ghi nhận khi có ý nghĩa nghiệp vụ.

## Tương tác và trạng thái

- Click row mở detail buổi học.
- Filter lưu trên URL; có nút xóa toàn bộ filter.
- Empty theo filter: “Không có bản ghi phù hợp”.
- Nếu status chưa được map, Student Portal không dùng nhãn phỏng đoán.

## Nguồn dữ liệu

- `Attendances`, `SubjectSchedules`, `SubjectTeachings`, `Subjects`, `Rooms`.
- Backend bắt buộc filter theo `studentId` trong token trước paging/projection.

## Giới hạn bắt buộc

- Dữ liệu hiện có `Attendance.Status` 1–4 nhưng chưa xác nhận ý nghĩa.
- Không gán Có mặt/Vắng/Muộn/Có phép và không tính tỷ lệ chuyên cần trước khi chốt data dictionary và mẫu số.

