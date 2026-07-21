# Student Portal — Đánh giá học tập

**Route đề xuất:** `/student/evaluations`  
**Mục tiêu:** hiển thị các đánh giá và nhận xét đang được lưu cho chính sinh viên.

## Bố cục

- Filter theo khoảng ngày, lớp học phần, giảng viên và loại sau khi enum được map.
- Timeline hoặc list card theo thời gian.
- Detail mở breakdown dạng table hai cột.

## Nội dung hiển thị

- Ngày tạo/cập nhật.
- Lớp học phần, học kỳ, kỳ thi/câu hỏi liên quan nếu resolve được.
- Tên giảng viên snapshot hoặc label được resolve.
- Loại đánh giá đã map.
- Tổng điểm raw và nhận xét.
- Breakdown: tên tiêu chí snapshot, điểm sinh viên, điểm tối đa/raw score.

## Trạng thái dữ liệu

- Nếu không resolve được quan hệ, ẩn block không cần thiết thay vì hiện GUID.
- Nếu `EvaluationCriteriaId` null, vẫn dùng `EvaluationName` snapshot.
- Empty: “Chưa có đánh giá được công bố”.

## Nguồn và giới hạn

- `StudentEvaluations`, `StudentEvaluationDetails`, `SemesterPlans`, `SubjectTeachings`, `TeacherFaculties`.
- `EvaluationCriterias` đang 0 dòng.
- Không tự đặt tên thang đo, xếp loại hoặc so sánh giữa các bộ tiêu chí chưa đồng nhất.

