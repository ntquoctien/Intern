# Student Portal — Thời khóa biểu

**Route đề xuất:** `/student/schedule`  
**Mục tiêu:** giúp sinh viên theo dõi lịch học theo tuần và danh sách.

## Bố cục

- Toolbar: tuần trước, tuần sau, “Hôm nay”, date picker, chuyển chế độ Tuần/Danh sách.
- Desktop: lưới 7 ngày, trục giờ theo dọc.
- Mobile: agenda theo ngày; không ép lưới tuần thu nhỏ.
- Màu event ổn định theo môn nhưng luôn kèm text/icon.

## Event hiển thị

- Giờ bắt đầu–kết thúc.
- Mã/tên môn, tên lớp học phần.
- Phòng.
- Giảng viên.
- Loại lịch đã map label và ghi chú.

## Tương tác

- Click event mở popover/drawer chi tiết.
- Link từ event tới lớp học phần, phòng hoặc giảng viên nếu người dùng có quyền.
- URL lưu tuần/ngày và chế độ xem.
- Trùng khoảng thời gian có thể được viền/cảnh báo trung tính.

## Trạng thái

- Empty: “Tuần này không có lịch học”.
- Missing room/teacher hiển thị “Chưa xếp phòng/Chưa xác định giảng viên”.
- Loading dùng skeleton theo vùng lịch, không spinner che toàn trang.

## Nguồn và quy tắc

- `SubjectStudents` → `SubjectSchedules`, nối `SubjectTeachings`, `Subjects`, `Rooms`, `TeacherFaculties/Users`.
- Dùng timezone Asia/Ho_Chi_Minh.
- Không suy diễn số tiết từ giờ nếu chưa có bảng quy đổi tiết học.

