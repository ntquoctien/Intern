# Management Portal — Kỳ thi, lượt thi và kết quả

**Route đề xuất:** `/management/assessment/results`  
**Mục tiêu:** tra cứu kỳ thi, lượt làm bài và kết quả raw trong ngữ cảnh môn/lớp/sinh viên.

## Bố cục

- Tabs: Kỳ thi, Lượt thi, Kết quả.
- Filter bar theo tab; detail page dùng tabs phụ.
- Các bảng lớn luôn server-side.

## Kỳ thi

- Tên, môn/lớp, loại, question suite, thời gian, phòng, giảng viên.
- Số lượng/cấu hình câu hỏi, method, allow notify, note.

## Lượt thi và kết quả

- Sinh viên, mã SV, kỳ thi/môn/lớp.
- Draft date, submit date.
- Result, combined result raw, description/note.
- Detail khảo thí: số câu selection/answer snapshot; nội dung chỉ cho role phù hợp.

## Filter

- Môn, lớp, kỳ thi, loại, ngày, sinh viên, khoảng điểm.

## Nguồn và giới hạn

- `SubjectTeachingExams`, `ExamAttempts`, `ExamResults`, `ExamQuestionSelections`, `ExamQuestionAnswers`, composition Academic/Identity.
- `ExamResults` 31.079 và answers 176.092 dòng: không tải toàn bộ.
- Không tự kết luận điểm chính thức, đạt/rớt, điểm chữ hoặc GPA.

## Đối chiếu schema database (22/07/2026)

- `SubjectTeachingExams` (1.528): lớp, suite, tên, thời gian, phòng, teacher, notes, type, Count/NumOfEasy/Normal/Hard/Practice, Method, AllowNotifyStudent, IsDeleted.
- `ExamAttempts` (1.587): student/exam/draft/submit; `ExamResults` (31.079): exam/student/attempt, Result, CombinedResult, Notes, ExamResultDesc, ExamResultDetail.
- Có thể đối chiếu ExamQuestionSelections/ExamQuestionAnswers theo quyền; không suy diễn đạt/rớt.

