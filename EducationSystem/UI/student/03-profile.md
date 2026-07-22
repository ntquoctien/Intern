# Student Portal — Hồ sơ của tôi

**Route đề xuất:** `/student/profile`  
**Mục tiêu:** cho sinh viên tra cứu hồ sơ cá nhân và học vụ đang lưu trong hệ thống.

## Bố cục

- Page header có avatar, họ tên, mã sinh viên và status tag.
- Nội dung dùng các section card; desktop hai cột, mobile một cột.
- Tabs đề xuất: “Học vụ”, “Cá nhân”, “Liên hệ”, “Thông tin bổ sung”.

## Nội dung hiển thị

### Học vụ

- Mã sinh viên chính thức, mã thư viện.
- Khoa, ngành, loại đào tạo, khóa/năm học.
- Trạng thái học, trạng thái tốt nghiệp, trạng thái tài khoản.

### Cá nhân và liên hệ

- Họ tên, ngày sinh, giới tính, nickname nếu có ý nghĩa nghiệp vụ.
- Số điện thoại, số định danh đã mask, ngày cấp nếu được phép.
- Nơi sinh, quê quán, địa chỉ thường trú/tạm trú.
- Dân tộc, tôn giáo, trình độ văn hóa khi chính sách cho phép.

### Bổ sung có giới hạn

- Thông tin gia đình/người thân, đoàn/đảng.
- Cờ vấn đề và mô tả chỉ khi sinh viên được phép xem.

## Quy tắc UI

- Field trống hiện “Chưa cập nhật”, không hiện `null` hoặc chuỗi rỗng.
- Thông tin dài wrap đúng; không đưa địa chỉ/gia đình lên page overview.
- Số định danh và mobile được mask theo chính sách.
- Phase chỉ đọc không có nút lưu; nếu cần chỉnh sửa, hiển thị hướng dẫn liên hệ.

## Nguồn dữ liệu

- `identity.Users`, `academic.Students`, `Majors`, `Faculties`, `AcademicYears`.
- Không trả password hash/salt hoặc dữ liệu thiết bị.

## Đối chiếu schema database (22/07/2026)

- `Users`: `FullName`, `BirthDate`, `IdentificationDate`, `IdentificationNumber`, `UserInternalId`, `Mobile`, `ProfilePicUrl`, `Role`, `IsActived`; mask số định danh/mobile.
- `Students`: `StudyStatus`, `Gender`, `Nickname`, nơi sinh/địa chỉ/dân tộc/tôn giáo/trình độ, gia đình, chính sách, nghề nghiệp, ngày Đảng/Đoàn, `IsGraduated`, `HasIssue`, `IssueDescription`, `LibraryId`; chia section và RBAC.
- Resolve `AcademicYears(Name, Year, StartDate, EndDate)`, `Majors(Code, Name, TrainingType)`, `Faculties(Code, Name)`.

