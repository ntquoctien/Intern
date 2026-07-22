# Management Portal — Ngân hàng câu hỏi

**Route đề xuất:** `/management/assessment/question-suites`  
**Mục tiêu:** tra cứu bộ câu hỏi, câu hỏi và đáp án theo quyền khảo thí.

## Bố cục

- Master–detail: suite list → question list → question detail.
- Search/filter cố định; question content trong table được clamp 2–3 dòng.
- Ảnh dùng thumbnail, click mở preview an toàn.

## Bộ câu hỏi

- Tên suite, môn/mã môn, người cập nhật nếu resolve được.
- Số câu tổng và breakdown theo level.

## Câu hỏi

- Suite, môn, level, preview text/image.
- Filter suite, subject, level; search text có giới hạn.
- Detail: nội dung đầy đủ, ảnh, danh sách đáp án, cờ `IsAnswer`, metadata.

## Nguồn và bảo mật

- `QuestionSuites`, `Questions`, `QuestionAnswers`; subject là liên kết cross-service.
- Answers đúng chỉ cho role được cấp; sanitize text và validate image URL.
- Không tạo filter Chapter/Topic/CLO/PLO/Skill/status vì database chưa có.

## Đối chiếu schema database (22/07/2026)

- `QuestionSuites` (319): `Id`, `SubjectId`, `Name`, `UpdatedById`, `CreationTime`.
- `Questions` (6.652): `Id`, `QuestionSuiteId`, `QuestionText`, `Level`, `ImageUrl`; `QuestionAnswers` (25.516): `Id`, `QuestionId`, `AnswerText`, `ImageUrl`, `IsAnswer`.
- Resolve subject/updater; đáp án đúng chỉ dành cho role khảo thí được phép.

