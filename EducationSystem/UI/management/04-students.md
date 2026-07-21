# Management Portal — Sinh viên

**Route đề xuất:** `/management/people/students`  
**Mục tiêu:** tìm kiếm và xem hồ sơ học tập tổng hợp của sinh viên theo quyền.

## Bố cục

- Filter bar nhiều điều kiện nhưng có chế độ thu gọn.
- Table server-side; cột nhận diện được ghim trái.
- Trang detail có header và tabs nghiệp vụ.

## Danh sách

- Mã sinh viên chính thức, họ tên.
- Khoa, ngành, khóa/năm học.
- Trạng thái học, tốt nghiệp, có vấn đề; trạng thái tài khoản.
- Search tên/mã/username.
- Filter khoa, ngành, khóa, trạng thái học, tốt nghiệp, có vấn đề.

## Detail tabs

- Tổng quan và hồ sơ cá nhân có masking.
- Lớp học phần, lịch, điểm danh.
- Kỳ thi/kết quả, đánh giá.
- Học phí, biểu mẫu/yêu cầu, ghi chú đặc biệt khi có quyền/dữ liệu.

## Quy tắc riêng tư

- Không đưa địa chỉ, số định danh, người thân lên list/export mặc định.
- Không trả hash/salt hoặc thông tin thiết bị.
- Truy cập detail phải theo RBAC/phạm vi đơn vị và có audit khi chính sách yêu cầu.

## Nguồn dữ liệu

- `Students` composition `Users`, `Majors`, `Faculties`, `AcademicYears` và các bảng học tập liên quan.

