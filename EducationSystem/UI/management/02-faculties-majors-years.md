# Management Portal — Khoa, ngành và năm học

**Route đề xuất:** `/management/education/structure`  
**Mục tiêu:** tra cứu cấu trúc tổ chức đào tạo làm điểm bắt đầu cho các luồng drill-down.

## Bố cục

- Tabs: Khoa, Ngành, Năm học/Khóa.
- Mỗi tab có filter bar, table và detail drawer/page.
- Breadcrumb detail giữ nguyên tab/filter khi quay lại.

## Tab Khoa

- Mã, tên, số ngành, số môn, số giảng viên liên kết.
- Detail: danh sách ngành, môn, giảng viên; link tới từng nhóm.

## Tab Ngành

- Mã, tên, khoa, loại đào tạo, số sinh viên, số kế hoạch học kỳ.
- Filter khoa/loại; search mã/tên.
- Detail: sinh viên, kế hoạch học kỳ và thông tin khoa.

## Tab Năm học/Khóa

- Tên, năm, ngày bắt đầu/kết thúc, số sinh viên.
- Filter khoảng năm; detail link tới sinh viên và kế hoạch.

## Nguồn dữ liệu

- `Faculties`, `Majors`, `AcademicYears` và aggregate từ `Subjects`, `TeacherFaculties`, `Students`, `SemesterPlans`.
- Số đếm phải tôn trọng `IsDeleted` và phạm vi quyền.

## Đối chiếu schema database (22/07/2026)

- `Faculties` (9): `Id`, `Name`, `Code`, `IsDeleted`; detail aggregate ngành, môn, giảng viên, sinh viên.
- `Majors` (21): `Id`, `FacultyId`, `Name`, `Code`, `TrainingType`, `IsDeleted`; giữ TrainingType raw nếu chưa có enum.
- `AcademicYears` (6): `Id`, `Name`, `Year`, `StartDate`, `EndDate`, `IsDeleted`; detail aggregate sinh viên và kế hoạch.

