# Management Portal — Đánh giá sinh viên

**Route đề xuất:** `/management/assessment/evaluations`  
**Mục tiêu:** tra cứu đánh giá, nhận xét và breakdown đang lưu cho sinh viên.

## Danh sách

- Sinh viên/mã SV.
- Học kỳ, lớp học phần, kỳ thi, câu hỏi, giảng viên nếu resolve được.
- Type đã map, total score, creation/update date.
- Comment được clamp và chỉ hiện đầy đủ trong detail.

## Bộ lọc

- Sinh viên, lớp, học kỳ, giảng viên, type, khoảng ngày và khoảng điểm.
- Các lookup lớn dùng autocomplete server-side.

## Detail

- Context links tới sinh viên/lớp/kỳ thi.
- Nhận xét đầy đủ.
- Table breakdown: tên tiêu chí snapshot, student score, score.
- Hiển thị cờ dữ liệu nếu criteria master không resolve được.

## Nguồn và giới hạn

- `StudentEvaluations`, `StudentEvaluationDetails`, `EvaluationCriterias` và relation Academic.
- `EvaluationCriterias` đang 0 dòng; detail phải hoạt động với ID null/snapshot name.
- Không tự tạo xếp loại hay so sánh bộ tiêu chí chưa đồng nhất.

