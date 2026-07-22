# Management Portal — Phòng và lịch giảng dạy

**Route đề xuất:** `/management/teaching/schedule`  
**Mục tiêu:** tra cứu việc sử dụng phòng và lịch của lớp/giảng viên.

## Bố cục

- Tabs: Lịch tuần, Danh sách lịch, Danh mục phòng.
- Toolbar dùng chung: ngày/tuần, phòng, môn/lớp, giảng viên, loại lịch.

## Lịch tuần/danh sách

- Event/row: bắt đầu–kết thúc, môn, lớp, phòng, giảng viên, loại, note.
- Click mở detail và liên kết tới lớp, phòng, giảng viên.
- Trùng lịch được highlight như cảnh báo dữ liệu, không tự kết luận vi phạm.

## Danh mục phòng

- Tên phòng, số chỗ, số lịch trong phạm vi đang lọc.
- Detail hiển thị lịch sử dụng theo ngày/tuần.

## Nguồn và quy tắc

- `Rooms`, `SubjectSchedules`, `SubjectTeachings`, `Subjects`, `TeacherFaculties/Users`.
- Dùng timezone thống nhất; không suy diễn tiết học.
- Cảnh báo vượt sức chứa chỉ khi số chỗ và sĩ số đều hợp lệ.

## Đối chiếu schema database (22/07/2026)

- `Rooms` (24): `Id`, `Name`, `NumberOfSeats`, `IsDeleted`.
- `SubjectSchedules` (6.382): `Id`, `SubjectTeachingId`, `RoomId`, `TeacherId`, `StartDateTime`, `EndDateTime`, `ScheduleType`, `Note`, `IsDeleted`.
- Resolve lớp/môn/giảng viên; chỉ cảnh báo sức chứa khi NumberOfSeats và count SubjectStudents hợp lệ.

