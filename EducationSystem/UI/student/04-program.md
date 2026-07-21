# Student Portal — Chương trình và kế hoạch học tập

**Route đề xuất:** `/student/program`  
**Mục tiêu:** hiển thị kế hoạch các học kỳ và môn học tham chiếu của ngành/khóa sinh viên.

## Bố cục

- Header nêu ngành, khoa và khóa/năm học.
- Thanh chọn học kỳ/năm học; trên mobile dùng select, desktop có thể dùng segmented tabs.
- Nội dung mỗi học kỳ là table hoặc accordion môn học.
- Summary chỉ hiển thị số môn và tổng tín chỉ kế hoạch có thể cộng trực tiếp.

## Nội dung hiển thị

- Học kỳ, ngày bắt đầu/kết thúc, trạng thái active.
- Mã môn snapshot, tên môn snapshot.
- Số tín chỉ, tổng số giờ từ danh mục môn khi resolve được.
- Link tới trang môn/lớp liên quan nếu có quan hệ chắc chắn.
- Cảnh báo dữ liệu khi `SubjectId` thiếu hoặc snapshot khác danh mục hiện tại.

## Bộ lọc và tương tác

- Chọn học kỳ; search mã/tên môn trong học kỳ.
- Expand row để xem ghi chú môn nếu có.
- URL lưu học kỳ đang chọn.

## Nguồn dữ liệu

- `Students.AcademicYearId`, `Students.MajorId`.
- `SemesterPlans`, `SemesterSubjects`, tham chiếu `Subjects`.

## Giới hạn nghiệp vụ

- Gọi là “Kế hoạch học tập tham chiếu”, không khẳng định phiên bản chương trình.
- Không phân loại bắt buộc/tự chọn hoặc tiên quyết vì schema chưa có.
- Không tính tín chỉ đã đạt, tín chỉ còn thiếu hay tiến độ tốt nghiệp.

