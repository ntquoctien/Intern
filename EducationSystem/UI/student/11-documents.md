# Student Portal — Tài liệu môn học

**Route đề xuất:** `/student/documents`  
**Mục tiêu:** tập trung tài liệu đã được công bố cho các môn có liên quan tới sinh viên.

## Bố cục

- Sidebar/select môn học; khu vực chính là list tài liệu.
- Search theo tên; filter loại tài liệu sau khi enum được xác nhận.
- Mobile dùng select môn ở đầu trang và document cards.

## Nội dung hiển thị

- Tên tài liệu.
- Môn học.
- Loại và mô tả/chi tiết.
- Ngày tạo/cập nhật.
- Nút “Xem/Tải xuống” có icon định dạng khi xác định được.

## Trạng thái và an toàn

- Database hiện có 0 `SubjectDocuments`; empty state chính thức: “Môn học chưa có tài liệu được công bố”.
- Không tạo dữ liệu mẫu trong database cố định.
- URL phải được kiểm tra scheme/domain hoặc đi qua download proxy.
- Không tự tải file khi mở page; file lỗi có thông báo và retry.

## Nguồn dữ liệu

- `SubjectDocuments`, `Subjects`.
- Quyền truy cập dựa trên các môn/lớp liên quan qua `SubjectStudents`.

## Đối chiếu schema database (22/07/2026)

- `SubjectDocuments` hiện 0 dòng: `Id`, `SubjectId`, `Type`, `Name`, `Detail`, `Url`, `CreateById`, `UserId`, `CreationDate`, `UpdateDate`.
- Resolve subject và creator; kiểm tra URL scheme/domain; không hiện GUID người tạo và không giả dữ liệu.

