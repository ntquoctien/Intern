# Student Portal — Thông báo

**Route đề xuất:** `/student/announcements`  
**Mục tiêu:** cung cấp inbox thông báo đúng người nhận và điều hướng an toàn tới nội dung liên quan.

## Bố cục

- Desktop: danh sách bên trái, nội dung detail bên phải.
- Mobile: list → detail page.
- Filter chips: Tất cả, Chưa đọc, Bắt buộc đọc; filter loại/thời gian nếu enum được map.

## List item

- Trạng thái đọc bằng dot/icon và text hỗ trợ screen reader.
- Loại thông báo.
- Preview message tối đa 2–3 dòng.
- Thời gian tạo.
- Badge “Bắt buộc đọc” khi `EnforceRead`.

## Detail

- Nội dung đã sanitize.
- Thời gian, loại, trạng thái.
- CTA tới deep link nội bộ nếu route nằm trong allowlist.
- Không render HTML/script không tin cậy.

## Nguồn dữ liệu và quyền

- `UserAnnouncements`: `UserIds`, `Message`, `CreationDate`, `Status`, `DeepLink`, `DeepLinkParam`, `EnforceRead`, `NotificationType`.
- Backend phải parse đúng chuẩn `UserIds` và lọc theo người dùng đã xác thực.
- Badge header lấy số lượng từ endpoint aggregate có quyền.
- Đánh dấu đã đọc là thao tác ghi, ngoài phase chỉ đọc hiện tại.

## Đối chiếu schema database (22/07/2026)

- `UserAnnouncements` (16.809): `Id`, `Type`, `UserIds`, `Message`, `CreationDate`, `Status`, `DeepLink`, `DeepLinkParam`, `EntityObjectId`, `EnforceRead`, `NotificationType`, `IsDeleted`.
- Backend parse `UserIds` và lọc đúng user; không hiện raw recipients/GUID entity. Deep link chỉ chạy khi khớp allowlist; sanitize message.

