# Student Portal — Học phí

**Route đề xuất:** `/student/tuition`  
**Mục tiêu:** dành chỗ cho dữ liệu học phí học kỳ tối giản mà schema hiện có thể hỗ trợ.

## Bố cục

- Intro banner nói rõ phạm vi thông tin.
- Danh sách theo học kỳ; detail card số tiền raw và ngày thanh toán.
- Không thiết kế dashboard công nợ đầy đủ ở phase hiện tại.

## Nội dung có thể hiển thị

- Học kỳ/kế hoạch học kỳ.
- `Amount` với đơn vị tiền tệ chỉ sau khi chính sách xác nhận.
- `PaidDate` nếu có.
- Link hướng dẫn liên hệ phòng tài chính, nếu trường cung cấp nội dung tĩnh.

## Empty state hiện tại

- `SemesterTuitions` đang 0 dòng.
- Hiện “Chưa có dữ liệu học phí trong hệ thống”, không kết luận “Đã hoàn tất nghĩa vụ”.

## Không được suy diễn

- Không có hạn thanh toán, giao dịch, khoản thu, miễn giảm, số đã trả/còn nợ và currency.
- Không tạo nhãn “Đã đóng/Chưa đóng” chỉ từ `PaidDate` nếu chưa có quy tắc.
- Muốn có cổng tài chính đầy đủ cần mở rộng data model và workflow riêng.

