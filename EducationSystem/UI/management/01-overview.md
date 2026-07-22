# Management Portal — Tổng quan điều hành

**Route đề xuất:** `/management/overview`  
**Mục tiêu:** cung cấp bức tranh dữ liệu tổng thể và điểm vào các luồng tra cứu quản trị.

## Bố cục

- Header: tiêu đề, phạm vi đơn vị đang xem, thời điểm dữ liệu.
- Hàng KPI 4–6 cards; tiếp theo là chart/grid hai cột.
- Cuối trang: hoạt động gần đây, cảnh báo chất lượng dữ liệu và shortcut.
- Tablet xếp KPI 2 cột; chart chuyển một cột.

## Nội dung hiển thị

- Tổng sinh viên, người dùng active, khoa, ngành, môn, lớp học phần, giảng viên.
- Tổng lịch, bản ghi điểm danh, kỳ thi và kết quả.
- Cơ cấu sinh viên theo khoa/ngành/khóa/trạng thái bằng bar/donut phù hợp.
- Lớp theo khoảng thời gian; hoạt động thi theo thời gian.
- Cảnh báo: thiếu label cross-service, orphan, kết quả duplicate/null, bảng nghiệp vụ rỗng.

## Tương tác

- Click KPI/segment mở đúng list với filter trên URL.
- Chọn phạm vi khoa/đơn vị chỉ xuất hiện theo quyền.
- Widget tải/lỗi độc lập; skeleton đúng kích thước.

## Nguồn dữ liệu

- Aggregate có kiểm soát từ `Students`, `Users`, `Faculties`, `Majors`, `Subjects`, `SubjectTeachings`, `TeacherFaculties`, `SubjectSchedules`, `Attendances`, `SubjectTeachingExams` và `ExamResults`.
- Mỗi KPI phải dùng cùng điều kiện `IsDeleted`, phạm vi quyền và định nghĩa với page đích.

## Không được hiển thị

- GPA trung bình, tỷ lệ đạt, tốt nghiệp, chuyên cần hay công nợ khi chưa có định nghĩa.
- KPI phải aggregate tại backend, không tải raw dataset về client.

## Đối chiếu schema database (22/07/2026)

- Aggregate từ `Students`, `Users`, `Faculties`, `Majors`, `AcademicYears`, `Subjects`, `SubjectTeachings`, `TeacherFaculties`, `SubjectSchedules`, `Attendances`, `SubjectTeachingExams`, `ExamResults`, `UserAnnouncements`.
- Mọi count lọc `IsDeleted`; database không có bảng lịch sử snapshot nên không tính phần trăm tăng trưởng theo tháng.
