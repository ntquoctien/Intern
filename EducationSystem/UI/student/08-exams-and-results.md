# Student Portal — Kỳ thi và kết quả

**Route đề xuất:** `/student/exam-results`  
**Mục tiêu:** cho sinh viên xem lịch/kỳ thi liên quan và các kết quả raw được phép công bố.

## Bố cục

- Tabs: “Kỳ thi” và “Kết quả”.
- Filter: môn/lớp, khoảng thời gian; học kỳ chỉ dùng khi quan hệ được xác nhận.
- Desktop table, mobile result cards.
- Detail drawer/page dùng section thay vì dump tất cả field.

## Tab Kỳ thi

- Tên kỳ thi, môn, lớp học phần.
- Loại và phương thức thi nếu enum đã map.
- Thời gian bắt đầu/kết thúc, phòng, giảng viên.
- Ghi chú được phép công bố.

## Tab Kết quả

- Môn/lớp/kỳ thi.
- Thời điểm draft/nộp của attempt.
- `Result` và `CombinedResult` với nhãn “Giá trị ghi nhận” khi chưa có quy tắc chính thức.
- Mô tả và ghi chú được phép công bố.
- Không chọn tùy ý một bản ghi trong trường hợp có duplicate/null.

## Nguồn dữ liệu

- `SubjectTeachingExams`, `ExamAttempts`, `ExamResults`.
- Composition với `SubjectTeachings`, `Subjects`, `Rooms`, teacher labels.

## Giới hạn

- Không tự hiển thị đạt/rớt, điểm chữ, GPA hoặc điểm chính thức.
- Không cho sinh viên xem câu hỏi/đáp án nếu chưa có chính sách khảo thí và thời điểm công bố.

