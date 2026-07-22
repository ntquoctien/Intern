# Management Portal — Tài khoản người dùng

**Route đề xuất:** `/management/people/users`  
**Mục tiêu:** tra cứu danh tính và trạng thái tài khoản ở phạm vi quản trị được phép.

## Bố cục

- Table mật độ cao với filter role/active.
- Detail drawer cho thông tin cơ bản; relation cards tới Student/Teacher nếu resolve được.

## Danh sách

- Username, họ tên, mã nội bộ.
- Role đã map label, trạng thái active.
- Loại liên kết: Sinh viên/Giảng viên/Khác khi backend xác định được.
- Search username, họ tên, mã nội bộ; filter role/active.

## Detail

- Profile picture, họ tên, ngày sinh khi cần.
- Mobile được mask; identification number/date chỉ cho quyền đặc biệt.
- Thời điểm enforce announcement read nếu có ý nghĩa hỗ trợ.
- Link tới hồ sơ sinh viên/giảng viên tương ứng.

## Bảo mật

- Không bao giờ trả/hiển thị `PasswordHash`, `PasswordSalt`.
- Không hiển thị PushId, token hoặc dữ liệu reset mật khẩu.
- Role là enum cần data dictionary; không để người dùng nhập mã số filter.

## Đối chiếu schema database (22/07/2026)

- `Users` (665): `Id`, `UserName`, `FullName`, `BirthDate`, `IdentificationDate`, `IdentificationNumber`, `UserInternalId`, `Mobile`, `ProfilePicUrl`, `Role`, `IsActived`, `LastEnforceAnnouncementRead`, `IsDeleted`.
- Detail liên kết `Students(UserId)`, `TeacherFaculties(UserId)` và aggregate `UserDevices(UserId, UserRole, DeviceType)`.
- Cấm trả `PasswordHash`, `PasswordSalt`, `UserDevices.Identifier`, `UserDevices.PushId`; mask identification/mobile.

