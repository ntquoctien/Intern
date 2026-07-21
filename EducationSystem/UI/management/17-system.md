# Management Portal — Hệ thống có giới hạn

**Route đề xuất:** `/management/system`  
**Mục tiêu:** cung cấp các trang hỗ trợ kỹ thuật được kiểm soát, không trộn với nghiệp vụ đào tạo.

## Bố cục

- Chỉ hiển thị menu cho System Admin.
- Tabs/cards: Cấu hình, Audit, Thiết bị tổng hợp, Hỗ trợ reset mật khẩu.
- Mỗi khu vực có banner cảnh báo về dữ liệu nhạy cảm và phạm vi sử dụng.

## Cấu hình

- Key, giá trị đã mask/phân loại, mô tả nếu có.
- Trước khi hiển thị cần inventory key nào chứa secret; deny-by-default.

## Audit

- Filter actor/action/entity/time nếu schema thực tế hỗ trợ.
- `AuditLogs` hiện 0 dòng: empty state không được diễn giải là “không có sự kiện”.

## Thiết bị và reset mật khẩu

- Chỉ dùng aggregate sức khỏe thiết bị khi có yêu cầu; không lộ PushId/token.
- Không tạo list reset password nghiệp vụ chung; màn hình hỗ trợ phải mask và audit.

## Nguồn và giới hạn

- `Settings` có 11 dòng, `UserDevices` có 115 dòng; `AuditLogs`, `PasswordResets` đang 0 dòng.
- Không hiển thị password hash/salt, reset secret, push identifier hay dữ liệu định danh đầy đủ.
- Phase hiện tại chỉ đọc, không sửa settings hoặc thực hiện reset.

