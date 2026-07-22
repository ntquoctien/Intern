# Management Portal — Mẫu biểu và yêu cầu

**Route đề xuất:** `/management/forms/requests`  
**Mục tiêu:** tra cứu mẫu biểu và yêu cầu dịch vụ sinh viên khi có dữ liệu.

## Bố cục

- Tabs: Mẫu biểu, Yêu cầu.
- Mẫu biểu dùng table/card; yêu cầu dùng table server-side và detail.

## Mẫu biểu

- Tên, document URL.
- Detail có link xem/tải đã validate.

## Yêu cầu

- Sinh viên/mã SV, tên mẫu.
- Creation/update date, status.
- Người duyệt, approval name và note.
- Detail trình bày các mốc hiện có; không dựng timeline giả nếu schema không có history.

## Bộ lọc

- Mẫu, sinh viên, người duyệt, status, khoảng ngày.
- Resolve cross-service label, không hiện GUID.

## Trạng thái và giới hạn

- `FormTemplates` và `FormRequests` đang 0 dòng; mỗi tab có empty state riêng.
- Không seed dữ liệu demo vào database.
- Tạo/sửa/duyệt/từ chối là workflow ghi cần proposal, authorization và audit riêng.

## Đối chiếu schema database (22/07/2026)

- `FormTemplates`: `Id`, `Name`, `DocumentUrl`, `IsDeleted`; `FormRequests`: `Id`, CreationDate, UpdateDate, StudentId, FormTemplateId, ApprovalId, ApprovalName, Note, Status, IsDeleted. Cả hai hiện 0 dòng.
- Detail resolve sinh viên/mẫu, ưu tiên ApprovalName; UI empty state thật và không giả workflow.

