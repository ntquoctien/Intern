# Student Portal — Biểu mẫu và yêu cầu

**Route đề xuất:** `/student/form-requests`  
**Mục tiêu:** hiển thị mẫu biểu và lịch sử yêu cầu của chính sinh viên.

## Bố cục

- Tabs: “Mẫu biểu” và “Yêu cầu của tôi”.
- Mẫu biểu dùng cards; yêu cầu dùng table/list theo trạng thái.
- Detail request có header status và các section thông tin.

## Tab Mẫu biểu

- Tên mẫu, link tài liệu.
- Mô tả tĩnh chỉ khi có nguồn nội dung được quản trị.

## Tab Yêu cầu của tôi

- Tên mẫu, ngày tạo/cập nhật.
- Trạng thái đã map label.
- Người duyệt/approval name nếu có.
- Ghi chú.
- Detail có liên kết tài liệu mẫu và thông tin duyệt.

## Trạng thái hiện tại

- `FormTemplates` và `FormRequests` đều 0 dòng.
- Hai tab phải có empty state riêng, không tạo request giả.
- Tạo/gửi/hủy yêu cầu, upload file và chuyển trạng thái là chức năng ghi ngoài phase hiện tại.

## Nguồn và quyền

- `FormTemplates`, `FormRequests`; resolve student/approver qua service phù hợp.
- Student Portal chỉ truy vấn request thuộc `studentId` trong token.

## Đối chiếu schema database (22/07/2026)

- `FormTemplates` hiện 0 dòng: `Id`, `Name`, `DocumentUrl`, `IsDeleted`.
- `FormRequests` hiện 0 dòng: `Id`, `CreationDate`, `UpdateDate`, `StudentId`, `FormTemplateId`, `ApprovalId`, `ApprovalName`, `Note`, `Status`, `IsDeleted`.
- Không hiện ApprovalId dạng GUID; ưu tiên ApprovalName; kiểm tra an toàn DocumentUrl.

