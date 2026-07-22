# Management Portal — Thông báo

**Route đề xuất:** `/management/communication/announcements`  
**Mục tiêu:** tra cứu thông báo đã tồn tại, đối tượng nhận và liên kết liên quan.

## Bố cục

- Table server-side + filter bar; detail drawer có preview giống phía sinh viên.
- Nội dung table clamp; không render HTML trong cell.

## Nội dung hiển thị

- Type, notification type và status đã map.
- Creation date.
- Preview message.
- Người/nhóm nhận được parse và biểu diễn an toàn.
- Enforce-read tag.
- Deep link/entity relation trong detail.

## Bộ lọc

- Type, notification type, status, enforce-read, khoảng ngày, người nhận.
- Search nội dung có giới hạn và phải cân nhắc hiệu năng.

## Nguồn và an toàn

- `UserAnnouncements` có 16.809 dòng; bắt buộc paging server.
- Sanitize message; deep link dùng allowlist route nội bộ.
- Không để lộ raw serialized `UserIds` trên list.
- Gửi/sửa/xóa thông báo là phase write-enabled riêng.

## Đối chiếu schema database (22/07/2026)

- `UserAnnouncements` (16.809): `Id`, `Type`, `UserIds`, `Message`, `CreationDate`, `Status`, `DeepLink`, `DeepLinkParam`, `EntityObjectId`, `EnforceRead`, `NotificationType`, `IsDeleted`.
- Detail parse/resolve recipients, sanitize content và kiểm tra deep-link allowlist; không đưa raw UserIds lên list.

