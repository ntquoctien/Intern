# PRD — Cổng thông tin sinh viên và cổng quản trị đào tạo

| Thuộc tính | Giá trị |
|---|---|
| Phiên bản | 1.0 |
| Trạng thái | Draft để xác nhận nghiệp vụ |
| Phạm vi | Website tra cứu, ưu tiên chỉ đọc trên database hiện hữu |
| Đối tượng | Sinh viên; cán bộ quản lý/đào tạo được phân quyền |
| Nguồn sự thật | 36 bảng thuộc `academic`, `exam`, `identity`, `communication` |
| Ngày lập | 21/07/2026 |

## 1. Tóm tắt sản phẩm

Xây dựng một website thông tin đại học gồm hai không gian sử dụng độc lập nhưng dùng chung nguồn dữ liệu:

1. **Cổng sinh viên (Student Portal):** sinh viên tự tra cứu hồ sơ, chương trình học, môn/lớp đang học, thời khóa biểu, điểm danh, kỳ thi, kết quả thi, thông báo và các dịch vụ biểu mẫu liên quan đến chính mình.
2. **Cổng quản trị (University Management Portal):** cán bộ tra cứu tổng thể và đi sâu theo khoa, ngành, khóa, học kỳ, sinh viên, giảng viên, môn học, lớp học phần, lịch, điểm danh, thi và truyền thông.

Sản phẩm phải trình bày dữ liệu theo ngôn ngữ nghiệp vụ đại học, không theo cấu trúc bảng kỹ thuật. GUID, `IsDeleted`, hash/salt và các trường kỹ thuật không được xuất hiện như thông tin chính trên UI.

## 2. Bối cảnh và vấn đề

Database hiện có độ phủ dữ liệu tốt ở hoạt động học tập, nhưng UI hiện tại chủ yếu là các danh sách tài nguyên rời rạc. Người dùng phải tự nối sinh viên, môn, lớp học phần, buổi học và kết quả bằng mã kỹ thuật; điều này chưa tương đương trải nghiệm của một cổng thông tin đại học chính quy.

Các vấn đề cần giải quyết:

- Thiếu kiến trúc thông tin theo hành trình của sinh viên và cán bộ đào tạo.
- Thiếu master–detail, breadcrumb và drill-down giữa các thực thể liên quan.
- Một số danh sách chỉ hiển thị ID thay vì mã/tên có nghĩa.
- Bộ lọc chưa phản ánh khoa, ngành, khóa, học kỳ, môn, lớp, giảng viên và khoảng thời gian.
- Dữ liệu cá nhân có nguy cơ bị overfetch hoặc hiển thị sai phạm vi quyền hạn.
- Một số chức năng quen thuộc của website đại học như GPA, xếp loại, đạt/rớt, công nợ hoặc lớp hành chính chưa có đủ dữ liệu/quy tắc để kết luận.

## 3. Mục tiêu và tiêu chí thành công

### 3.1. Mục tiêu

- Sinh viên tìm được thông tin quan trọng của mình trong tối đa 3 thao tác từ trang chủ.
- Cán bộ đi được trọn chuỗi tra cứu: khoa/ngành → kế hoạch học kỳ → môn → lớp học phần → sinh viên/giảng viên → lịch/điểm danh/kỳ thi/kết quả.
- Mọi giá trị trên UI có nguồn dữ liệu hoặc quy tắc được phê duyệt; không tự suy diễn nghiệp vụ.
- Tất cả danh sách lớn dùng truy vấn phía server, phân trang, lọc và sắp xếp có kiểm soát.
- Hai cổng có cùng ngôn ngữ thiết kế nhưng khác navigation, mật độ thông tin và quyền truy cập.

### 3.2. Chỉ số thành công dự kiến

- ≥ 90% tác vụ tra cứu cốt lõi hoàn thành mà không cần nhập GUID.
- ≥ 95% màn hình có trạng thái loading, empty, error và retry rõ ràng.
- P95 tải danh sách ≤ 2 giây trong mạng nội bộ ở kích thước trang mặc định.
- Không có dữ liệu của sinh viên khác trong response Student Portal.
- Không có password hash/salt, push token hoặc thông tin định danh nhạy cảm trong list response.
- 100% field tổng hợp có định nghĩa phép tính và nguồn dữ liệu đã phê duyệt.

## 4. Phạm vi dữ liệu và mức độ sẵn sàng

### 4.1. Quy ước

| Mức | Ý nghĩa | Cách xử lý trong sản phẩm |
|---|---|---|
| **A — Sẵn sàng** | Có bảng, quan hệ và dữ liệu thực tế | Có thể xây UI tra cứu sau khi có read model/API phù hợp |
| **B — Có mô hình nhưng rỗng** | Bảng tồn tại nhưng snapshot có 0 dòng | Thiết kế màn hình và empty state; không demo bằng dữ liệu giả |
| **C — Thiếu quy tắc/quan hệ** | Có giá trị thô nhưng chưa đủ kết luận nghiệp vụ | Chỉ hiển thị raw value có nhãn trung tính hoặc ẩn tính năng |
| **D — Chưa có dữ liệu** | Không có bảng/cột/quan hệ cần thiết | Đưa vào backlog mở rộng dữ liệu, ngoài phạm vi bản chỉ đọc |

### 4.2. Tóm tắt 36 bảng

| Nhóm | Dữ liệu chính | Hiện trạng đáng chú ý |
|---|---|---|
| Học vụ | năm học, khoa, ngành, kế hoạch học kỳ, môn, lớp học phần, phòng, lịch, ghi danh, giảng viên, điểm danh | Dữ liệu phong phú; `Attendances` 93.865 dòng, `SubjectStudents` 11.365, `SubjectSchedules` 6.382 |
| Đánh giá học tập | đánh giá sinh viên và chi tiết | 3.423 đánh giá và 23.032 chi tiết; danh mục tiêu chí hiện rỗng nên phải dùng snapshot name/score thận trọng |
| Thi | kỳ thi, lượt thi, kết quả, bộ câu hỏi, câu hỏi và đáp án | Khối lượng lớn; `ExamResults` 31.079, `ExamQuestionAnswers` 176.092 |
| Danh tính | người dùng, thiết bị, cấu hình, reset mật khẩu, audit | 665 người dùng; dữ liệu kỹ thuật/nhạy cảm phải hạn chế |
| Truyền thông | thông báo, mẫu biểu, yêu cầu biểu mẫu | 16.809 thông báo; mẫu biểu và yêu cầu biểu mẫu đang rỗng |
| Học phí/tài liệu | học phí học kỳ, tài liệu môn | Có schema nhưng đều đang rỗng |

## 5. Người dùng và quyền

### 5.1. Persona

| Persona | Nhu cầu chính | Phạm vi dữ liệu |
|---|---|---|
| Sinh viên | Xem nhanh hôm nay học gì, kết quả, điểm danh, hồ sơ và thông báo | Chỉ bản ghi thuộc `studentId` đã xác thực |
| Cán bộ đào tạo | Theo dõi chương trình, lớp học phần, lịch, ghi danh, kết quả | Theo đơn vị/phạm vi được cấp |
| Cán bộ khoa | Tra cứu ngành, môn, giảng viên và lớp thuộc khoa | Theo `FacultyId` được cấp |
| Quản trị hệ thống | Xem danh tính, cấu hình vận hành và audit được phép | Quyền đặc biệt, tách khỏi nghiệp vụ học vụ |
| Ban giám hiệu/điều hành | Xem số liệu tổng quan và đi sâu theo đơn vị | Dữ liệu tổng hợp, có masking theo chính sách |

### 5.2. Ma trận quyền tối thiểu

| Khả năng | Sinh viên | Cán bộ đào tạo/khoa | Quản trị hệ thống |
|---|---:|---:|---:|
| Xem dữ liệu cá nhân của chính mình | Có | Theo quyền | Theo quyền đặc biệt |
| Xem danh sách toàn bộ sinh viên | Không | Có giới hạn phạm vi | Có |
| Xem dữ liệu giảng viên/lớp/môn | Chỉ dữ liệu liên quan | Có | Có |
| Xem dữ liệu thi chi tiết/câu trả lời | Chỉ kết quả được công bố | Theo quyền khảo thí | Theo quyền đặc biệt |
| Xem hash/salt/push token | Không | Không | Không hiển thị trên UI |
| Sửa dữ liệu | Ngoài phạm vi phiên bản này | Ngoài phạm vi phiên bản này | Ngoài phạm vi phiên bản này |

> Xác thực bằng mã sinh viên đơn thuần không đủ an toàn cho production. Cần cơ chế xác thực đã được xác minh (mật khẩu đúng thuật toán, SSO hoặc IdP) trước khi công khai Student Portal.

## 6. Nguyên tắc UI/UX

1. **Nghiệp vụ trước dữ liệu kỹ thuật:** dùng “Lớp học phần”, “Giảng viên”, “Học kỳ”, không dùng tên bảng/GUID.
2. **Overview → list → detail → related records:** mọi đối tượng chính có đường đi sâu và quay lại giữ nguyên bộ lọc.
3. **Một nguồn, một nhãn:** mã sinh viên chính thức phải được chốt; không trộn `Nickname`, `UserInternalId` và `UserName` như cùng một khái niệm.
4. **Không bịa chỉ số:** không tự tính GPA, đạt/rớt, tỷ lệ chuyên cần hoặc tín chỉ tích lũy khi chưa có quy tắc.
5. **Progressive disclosure:** list chỉ chứa field cần scan; thông tin riêng tư/chi tiết nằm trong trang detail có quyền.
6. **URL là trạng thái:** tab, trang, search, filter và sort nằm trong query string để reload/chia sẻ/back không mất ngữ cảnh.
7. **Mobile-first cho sinh viên, desktop-first cho quản trị:** Student Portal ưu tiên thẻ và lịch gọn; Management Portal ưu tiên bảng, filter bar và drill-down.
8. **Trạng thái trung thực:** phân biệt “chưa có dữ liệu”, “không thuộc phạm vi”, “không có quyền” và “không tải được”.

## 7. Kiến trúc thông tin

```text
Website Đại học
├── Cổng sinh viên
│   ├── Đăng nhập
│   ├── Tổng quan
│   ├── Hồ sơ của tôi
│   ├── Chương trình học
│   ├── Môn và lớp học phần
│   ├── Thời khóa biểu
│   ├── Điểm danh
│   ├── Kỳ thi và kết quả
│   ├── Đánh giá học tập
│   ├── Thông báo
│   ├── Tài liệu môn học
│   ├── Học phí
│   └── Biểu mẫu/yêu cầu
└── Cổng quản trị
    ├── Tổng quan điều hành
    ├── Cơ cấu đào tạo
    │   ├── Khoa, ngành, khóa/năm học
    │   └── Kế hoạch học kỳ và môn trong kế hoạch
    ├── Con người
    │   ├── Sinh viên
    │   ├── Giảng viên
    │   └── Tài khoản
    ├── Giảng dạy
    │   ├── Danh mục môn
    │   ├── Lớp học phần và ghi danh
    │   ├── Phân công giảng viên
    │   ├── Phòng và lịch
    │   └── Điểm danh
    ├── Khảo thí và đánh giá
    │   ├── Kỳ thi, lượt thi, kết quả
    │   ├── Bộ câu hỏi, câu hỏi, đáp án
    │   └── Đánh giá sinh viên
    ├── Truyền thông và dịch vụ
    │   ├── Thông báo
    │   ├── Mẫu biểu
    │   └── Yêu cầu biểu mẫu
    └── Hệ thống có giới hạn
        ├── Cấu hình
        └── Audit/thiết bị theo quyền
```

## 8. Yêu cầu chung cho mọi màn hình

### 8.1. App shell

- Header: logo/tên trường, tên cổng, ô tìm kiếm theo phạm vi, thông báo, avatar/menu tài khoản.
- Sidebar desktop và drawer/bottom navigation mobile; nhóm menu theo nghiệp vụ.
- Breadcrumb ở trang detail; tiêu đề, mô tả ngắn, thời điểm dữ liệu cập nhật nếu có.
- Không dùng cùng một menu cho hai cổng; chuyển cổng chỉ xuất hiện khi tài khoản có quyền.

### 8.2. List pattern

- Search có debounce 300–500 ms; filter có label nghiệp vụ; select/auto-complete thay cho nhập mã enum.
- Server-side pagination; mặc định 20–50 dòng, tùy dataset.
- Cột ghim cho mã/tên; người dùng được chọn cột nhưng default phải hữu dụng.
- Row click mở detail; các liên kết liên quan mở đúng tab/ngữ cảnh.
- Có tổng số bản ghi, xóa filter, skeleton, empty state, lỗi và retry.
- Không tải toàn bộ lookup lớn; autocomplete phải search/paginate phía server.

### 8.3. Detail pattern

- Header nhận diện: mã + tên + status có nhãn đã xác minh.
- Các tab: Tổng quan, thông tin liên quan, lịch sử/bản ghi; chỉ hiện tab có dữ liệu/quyền.
- ID kỹ thuật chỉ được phép trong khu vực “Thông tin kỹ thuật” dành cho hỗ trợ, không phải nội dung mặc định.
- Trường nhạy cảm được mask; thao tác reveal nếu có phải có quyền và audit.

## 9. Cổng sinh viên — yêu cầu chi tiết

### STU-01. Đăng nhập

**Mục đích:** xác thực sinh viên và tạo phiên truy cập an toàn.

- UI: logo/tên trường, ô mã sinh viên/tài khoản, phương thức xác thực được phê duyệt, ghi nhớ phiên nếu chính sách cho phép, trợ giúp đăng nhập.
- Không tiết lộ tài khoản có tồn tại qua thông báo lỗi.
- Chặn rate-limit/brute force; session timeout; logout xóa token cục bộ.
- `Nickname`, `UserInternalId` và `UserName` phải được BA/chủ dữ liệu chọn một trường làm mã đăng nhập chính thức.

### STU-02. Tổng quan

**Mục đích:** trả lời nhanh “tôi là ai, hôm nay có gì, điều gì cần chú ý”.

**Hiển thị:** thẻ hồ sơ gọn (họ tên, mã SV đã chốt, ngành, khóa/năm tuyển sinh, trạng thái học); lịch học gần nhất; kỳ thi gần nhất; thông báo mới/bắt buộc đọc; các môn/lớp đang liên quan; shortcut tới điểm danh và kết quả.

**Nguồn:** `Users`, `Students`, `Majors`, `AcademicYears`, `SubjectStudents`, `SubjectTeachings`, `SubjectSchedules`, `SubjectTeachingExams`, `UserAnnouncements`.

**Giới hạn:** không hiển thị GPA, công nợ, tín chỉ đạt hoặc cảnh báo chuyên cần nếu chưa có quy tắc được phê duyệt.

### STU-03. Hồ sơ của tôi

**Các khối:** ảnh/họ tên và mã; thông tin học vụ (ngành, khoa, khóa, trạng thái học); thông tin cá nhân; liên hệ/địa chỉ; thông tin gia đình/đoàn-đảng nếu chính sách cho phép; thông tin thư viện và ghi chú vấn đề chỉ khi được phép.

**Nguồn:** `Users`, `Students`, `Majors`, `Faculties`, `AcademicYears`.

**Quy tắc:** mask số định danh và số điện thoại; không trả password fields; field trống hiện “Chưa cập nhật”, không hiện `null`.

### STU-04. Chương trình học

**Hiển thị:** chọn năm học/học kỳ; danh sách học kỳ; môn theo kế hoạch với mã, tên, số tín chỉ, tổng giờ; tổng tín chỉ theo học kỳ chỉ được cộng trực tiếp từ `SemesterSubjects.CreditPoint`; ghi chú rõ đây là “kế hoạch tham chiếu”.

**Nguồn:** `Students.AcademicYearId/MajorId` → `SemesterPlans` → `SemesterSubjects`, tham chiếu `Subjects` khi có.

**Giới hạn:** database chưa có phiên bản chương trình, môn bắt buộc/tự chọn, tiên quyết và mapping chắc chắn môn đã đạt; không được tạo progress “đã hoàn thành x/y tín chỉ”.

### STU-05. Môn và lớp học phần của tôi

**List:** mã/tên môn, tên lớp học phần, số tín chỉ, ngày bắt đầu–kết thúc, phòng mặc định, giảng viên chính nếu có, trạng thái thời gian trung tính như sắp diễn ra/đang trong khoảng/đã qua.

**Detail:** thông tin môn; danh sách giảng viên; lịch; tài liệu; ghi chú đặc biệt của chính sinh viên nếu chính sách cho phép; link tới điểm danh và kết quả liên quan.

**Nguồn:** `SubjectStudents`, `SubjectTeachings`, `Subjects`, `SubjectTeachingTeachers`, `TeacherFaculties`, `Users`, `Rooms`, `SubjectDocuments`, `SubjectSpecialNotes`.

### STU-06. Thời khóa biểu

**UI:** chế độ tuần mặc định, chuyển tuần, “Hôm nay”, list view cho mobile; mỗi block gồm giờ, môn, lớp, phòng, giảng viên, loại lịch và ghi chú.

**Nguồn:** các lớp thuộc `SubjectStudents` → `SubjectSchedules`, nối `SubjectTeachings`, `Subjects`, `Rooms`, `TeacherFaculties/Users`.

**Quy tắc:** timezone Asia/Ho_Chi_Minh; không suy diễn tiết học từ giờ nếu chưa có bảng quy đổi tiết; phát hiện trùng lịch chỉ là cảnh báo trình bày dựa trên khoảng thời gian.

### STU-07. Điểm danh

**UI:** bộ lọc môn/lớp/khoảng ngày/trạng thái; list theo buổi với thời gian, phòng, trạng thái, ghi chú; summary đếm từng trạng thái chỉ sau khi enum được xác nhận.

**Nguồn:** `Attendances`, `SubjectSchedules`, `SubjectTeachings`, `Subjects`, `Rooms` và student ownership.

**Giới hạn bắt buộc:** giá trị `Attendance.Status` thực tế nằm trong 1–4 nhưng ý nghĩa chưa được xác nhận. Không gán nhãn Có mặt/Vắng/Muộn/Phép hoặc tính tỷ lệ chuyên cần trước khi chốt enum và mẫu số.

### STU-08. Kỳ thi và kết quả

**UI:** tab “Lịch/kỳ thi” và “Kết quả”; filter học kỳ/môn; card/list gồm môn, lớp, tên kỳ thi, loại, thời gian, lần nộp, điểm raw, điểm kết hợp raw, mô tả/ghi chú được phép công bố.

**Nguồn:** `SubjectTeachingExams`, `ExamAttempts`, `ExamResults`, `SubjectTeachings`, `Subjects`.

**Giới hạn:** không hiển thị đạt/rớt, điểm chữ, GPA, điểm chính thức hoặc chọn một dòng “kết quả cuối” khi chưa có quy tắc xử lý duplicate/null và ngưỡng điểm. Không công khai đáp án/câu trả lời nếu chưa có chính sách khảo thí.

### STU-09. Đánh giá học tập

**UI:** timeline/danh sách đánh giá theo học kỳ/lớp/giảng viên; tổng điểm raw, nhận xét và breakdown theo snapshot `EvaluationName`, `StudentScore`, `Score`.

**Nguồn:** `StudentEvaluations`, `StudentEvaluationDetails`, `SemesterPlans`, `SubjectTeachings`, `TeacherFaculties`.

**Giới hạn:** `EvaluationCriterias` đang rỗng; UI không được tự đặt tên thang đo, xếp loại hoặc so sánh giữa các bộ tiêu chí chưa đồng nhất.

### STU-10. Thông báo

**UI:** inbox có chưa đọc/bắt buộc đọc, loại thông báo, thời gian, nội dung an toàn, deep link được allowlist; detail modal/page; badge số lượng.

**Nguồn:** `UserAnnouncements` với `UserIds`, `Message`, `CreationDate`, `Status`, `DeepLink`, `EnforceRead`, `NotificationType`.

**Quy tắc:** backend phải lọc đúng người nhận; sanitize message; deep link chỉ tới route nội bộ hợp lệ. Nếu cập nhật đã đọc làm thay đổi DB thì thuộc phase write-enabled riêng.

### STU-11. Tài liệu môn học

**UI:** theo môn/lớp, gồm tên, loại, mô tả, ngày cập nhật và link tải an toàn.

**Nguồn:** `SubjectDocuments` (hiện 0 dòng).

**Empty state:** “Môn học chưa có tài liệu được công bố”; không tạo tài liệu giả. Link ngoài phải được kiểm tra scheme/domain hoặc proxy tải xuống.

### STU-12. Học phí

**UI:** học kỳ, số tiền raw, ngày thanh toán và trạng thái chỉ khi có quy tắc chính thức.

**Nguồn:** `SemesterTuitions` (hiện 0 dòng).

**Giới hạn:** schema chỉ có `Amount` và `PaidDate`, chưa có hạn thanh toán, miễn giảm, giao dịch, số tiền đã trả/còn nợ hoặc đơn vị tiền tệ. Không xây màn hình công nợ đầy đủ từ schema này.

### STU-13. Biểu mẫu và yêu cầu

**UI:** danh mục mẫu (tên, tài liệu); yêu cầu của tôi (mẫu, ngày tạo/cập nhật, trạng thái, người duyệt, ghi chú); detail timeline nếu có dữ liệu lịch sử.

**Nguồn:** `FormTemplates`, `FormRequests` (đều 0 dòng).

**Giới hạn:** phase chỉ đọc chỉ hiển thị empty state. Tạo/gửi/hủy yêu cầu và chuyển trạng thái là phase write-enabled, cần authorization, audit và workflow riêng.

## 10. Cổng quản trị — yêu cầu chi tiết

### MGT-01. Tổng quan điều hành

**KPI được phép:** tổng sinh viên hiện hữu, người dùng active, khoa, ngành, môn, lớp học phần, giảng viên, lịch, bản ghi điểm danh, kỳ thi và kết quả; breakdown theo các field có thật.

**Biểu đồ/khối:** cơ cấu sinh viên theo khoa/ngành/khóa/trạng thái; lớp theo thời gian; hoạt động thi; thông báo gần đây; cảnh báo chất lượng dữ liệu (thiếu label, orphan cross-service, duplicate kết quả).

**Không được phép:** tỷ lệ tốt nghiệp, tỷ lệ đạt, GPA trung bình, tỷ lệ chuyên cần hoặc công nợ nếu chưa có định nghĩa.

### MGT-02. Khoa, ngành và năm học

- Danh sách khoa: mã, tên, số ngành, số môn, số giảng viên liên kết.
- Detail khoa: ngành, môn, giảng viên; link drill-down.
- Danh sách ngành: mã, tên, khoa, loại đào tạo, số sinh viên, số kế hoạch học kỳ.
- Năm học/khóa: tên, năm, bắt đầu/kết thúc, số sinh viên.
- Nguồn: `Faculties`, `Majors`, `AcademicYears`, quan hệ đếm tương ứng.

### MGT-03. Kế hoạch đào tạo

- Filter: khoa, ngành, khóa/năm học, học kỳ, active.
- List plan: ngành, khóa, học kỳ, thời gian, trạng thái, số môn/tổng tín chỉ kế hoạch.
- Detail: môn trong kế hoạch với code/name snapshot, credit; cảnh báo khi `SubjectId` null hoặc snapshot lệch danh mục môn.
- Nguồn: `SemesterPlans`, `SemesterSubjects`, `Majors`, `AcademicYears`, `Subjects`.
- Không gọi đây là “chương trình chuẩn phiên bản X” khi chưa có `CurriculumVersion`.

### MGT-04. Sinh viên

- List mặc định: mã SV chính thức, họ tên, ngành, khoa, khóa, trạng thái học, giới tính nếu cần nghiệp vụ, trạng thái tài khoản.
- Search: tên, mã SV, username; filter: khoa, ngành, khóa, trạng thái học, tốt nghiệp, có vấn đề.
- Detail tabs: Tổng quan; hồ sơ cá nhân có masking; lớp học phần; lịch; điểm danh; kỳ thi/kết quả; đánh giá; học phí; yêu cầu biểu mẫu; ghi chú đặc biệt.
- Nguồn: `Students` composition với `Users` và các bảng liên quan.
- Không đưa địa chỉ, người thân, định danh lên list hoặc export mặc định.

### MGT-05. Giảng viên

- List: họ tên, mã tài khoản/nội bộ, khoa, trưởng khoa hay không, trạng thái tài khoản, số lớp được phân công.
- Filter: khoa, vai trò trưởng khoa, trạng thái; search tên/mã.
- Detail: hồ sơ cơ bản; khoa; lớp phụ trách; lịch giảng; môn liên quan.
- Nguồn: `TeacherFaculties`, `Users`, `Faculties`, `SubjectTeachingTeachers`, `SubjectTeachings`, `SubjectSchedules`.
- `TeacherFaculty` là liên kết người dùng–khoa, không chứng minh học hàm/học vị/chức danh; không tự hiển thị các thông tin đó.

### MGT-06. Tài khoản

- List tối thiểu: username, họ tên, mã nội bộ, role raw đã map label, active, ngày sinh chỉ khi cần và được phép.
- Detail có thể có mobile, profile picture, lần enforce-read; identification phải mask.
- Không bao giờ trả/hiển thị `PasswordHash`, `PasswordSalt`, push ID hoặc token.
- Nguồn: `Users`; liên kết Student/Teacher nếu có.

### MGT-07. Danh mục môn học

- List: mã, tên, khoa, tín chỉ, tổng giờ, active; search mã/tên; filter khoa/active/số tín chỉ.
- Detail: mô tả/ghi chú, kế hoạch có môn, lớp học phần, tài liệu và ghi chú đặc biệt có quyền.
- Nguồn: `Subjects`, `Faculties`, `SemesterSubjects`, `SubjectTeachings`, `SubjectDocuments`, `SubjectSpecialNotes`.

### MGT-08. Lớp học phần và ghi danh

- List: tên lớp, môn, khoa từ môn, thời gian, phòng mặc định, giảng viên chính, số sinh viên, số buổi.
- Filter: môn, khoa, giảng viên, phòng, khoảng ngày; không filter trực tiếp theo ngành khi chưa có quan hệ xác định.
- Detail tabs: thông tin; sinh viên; giảng viên; lịch; điểm danh; kỳ thi/kết quả.
- Nguồn: `SubjectTeachings`, `Subjects`, `SubjectStudents`, `SubjectTeachingTeachers`, `TeacherFaculties`, `Rooms`.
- Không gọi lớp học phần là “lớp hành chính” và không gán duy nhất một ngành/học kỳ nếu schema chưa liên kết.

### MGT-09. Phân công giảng viên

- Ma trận/lists theo giảng viên hoặc lớp; nhãn giảng viên chính/phụ từ `IsMainTeacher`.
- Filter: khoa, giảng viên, môn, lớp, thời gian.
- Nguồn: `SubjectTeachingTeachers`, `TeacherFaculties`, `Users`, `SubjectTeachings`, `Subjects`.

### MGT-10. Phòng và lịch giảng dạy

- Danh mục phòng: tên, sức chứa, số lịch liên quan.
- Lịch tuần/tháng/list: lớp, môn, phòng, giảng viên, bắt đầu/kết thúc, loại, ghi chú.
- Filter: phòng, môn/lớp, giảng viên, khoảng thời gian, loại lịch.
- Nguồn: `Rooms`, `SubjectSchedules`, `SubjectTeachings`, `Subjects`, `TeacherFaculties/Users`.
- Cảnh báo vượt sức chứa chỉ được tính khi sĩ số lớp và `NumberOfSeats` hợp lệ; trình bày như kiểm tra dữ liệu, không kết luận vi phạm.

### MGT-11. Điểm danh

- List server-side: sinh viên, mã SV, môn/lớp, buổi học, phòng, status, ghi chú, người tạo, ngày ghi nhận.
- Filter: khoa (qua môn), môn, lớp, sinh viên, phòng, ngày, status.
- Detail liên kết tới sinh viên, buổi học và lớp.
- Nguồn: `Attendances` và read model composition.
- Không phát hành dashboard tỷ lệ cho đến khi enum/status và quy tắc mẫu số được phê duyệt.

### MGT-12. Kỳ thi, lượt thi và kết quả

- Kỳ thi list: tên, lớp/môn, loại, suite, thời gian, phòng, giảng viên, số lượng/cấu hình câu hỏi, method, cho phép thông báo.
- Kết quả list: sinh viên, mã SV, môn/lớp/kỳ thi, lượt thi, ngày draft/submit, result và combined result raw.
- Detail: mô tả/chi tiết kết quả, timeline attempt, số câu/đáp án đã snapshot; dữ liệu đáp án chỉ cho vai trò khảo thí.
- Filter: môn, lớp, kỳ thi, loại, ngày, sinh viên, score range.
- Nguồn: `SubjectTeachingExams`, `ExamAttempts`, `ExamResults`, `ExamQuestionSelections`, `ExamQuestionAnswers` và cross-service read model.

### MGT-13. Ngân hàng câu hỏi

- Suite list/detail: môn, tên, người cập nhật nếu resolve được, số câu theo level.
- Question list: suite, môn, level, text đã clamp, image preview; filter suite/môn/level.
- Question detail: nội dung, ảnh, các đáp án và `IsAnswer`, metadata liên quan.
- Nguồn: `QuestionSuites`, `Questions`, `QuestionAnswers`.
- Chưa có Chapter/Topic/CLO/PLO/Skill/approval status; không tạo các filter/biểu đồ này nếu chưa mở rộng schema.

### MGT-14. Đánh giá sinh viên

- List: sinh viên, học kỳ/lớp/kỳ thi/câu hỏi/giảng viên liên quan nếu resolve được, loại raw đã map, tổng điểm, ngày, nhận xét.
- Detail: breakdown từng tiêu chí snapshot.
- Nguồn: `StudentEvaluations`, `StudentEvaluationDetails`, `EvaluationCriterias`.
- UI phải chịu được `EvaluationCriteriaId` null và danh mục tiêu chí rỗng.

### MGT-15. Thông báo

- List: loại, trạng thái, thời gian, nhóm/người nhận được parse an toàn, preview nội dung, enforce-read, notification type.
- Detail: nội dung sanitized, deep link, đối tượng liên quan.
- Nguồn: `UserAnnouncements`.
- Gửi/sửa/xóa/đánh dấu đọc nằm ngoài phase chỉ đọc.

### MGT-16. Mẫu biểu và yêu cầu

- Mẫu biểu: tên, document URL; empty state vì hiện chưa có dữ liệu.
- Yêu cầu: sinh viên, mẫu, ngày tạo/cập nhật, trạng thái, người duyệt, ghi chú; empty state hiện tại.
- Cần resolve cross-service Student/User thay vì hiện GUID.
- Workflow phê duyệt là phase write-enabled riêng.

### MGT-17. Hệ thống có giới hạn

- `Settings`: chỉ quản trị hệ thống; key/value cần phân loại secret trước khi hiển thị.
- `AuditLogs`: hiện rỗng; chỉ hiển thị khi xác định schema và retention; không coi là nhật ký đầy đủ.
- `UserDevices`: dữ liệu nhạy cảm; chỉ dashboard sức khỏe thiết bị đã aggregate nếu có yêu cầu, không lộ PushId.
- `PasswordResets`: không tạo list nghiệp vụ chung; truy cập hỗ trợ phải đặc quyền và mask.

## 11. Ma trận dữ liệu → UI

| Bảng | Student Portal | Management Portal | Trạng thái |
|---|---|---|---|
| AcademicYears | Hồ sơ, chương trình | Cơ cấu, sinh viên, kế hoạch | A |
| Faculties, Majors | Hồ sơ, chương trình | Cơ cấu, người, môn | A |
| SemesterPlans, SemesterSubjects | Chương trình | Kế hoạch đào tạo | A; thiếu version/type môn |
| Students, Users | Hồ sơ của tôi | Sinh viên/tài khoản | A; cần chốt mã SV |
| TeacherFaculties | Giảng viên lớp/lịch | Giảng viên | A; cross-service User |
| Subjects | Chương trình/môn | Danh mục môn | A |
| SubjectTeachings, SubjectStudents | Lớp của tôi | Lớp/ghi danh | A |
| SubjectTeachingTeachers | Giảng viên lớp | Phân công | A |
| Rooms, SubjectSchedules | Thời khóa biểu | Phòng/lịch | A |
| Attendances | Điểm danh của tôi | Điểm danh | C; enum chưa chốt |
| SubjectSpecialNotes | Detail môn của tôi nếu được phép | Detail môn/sinh viên | A; nhạy cảm theo chính sách |
| SubjectDocuments | Tài liệu | Detail môn | B; 0 dòng |
| StudentEvaluations/Details | Đánh giá của tôi | Đánh giá | A/C; criteria master rỗng |
| EvaluationCriterias | Breakdown | Danh mục/đánh giá | B; 0 dòng |
| SemesterTuitions | Học phí | Detail sinh viên | B/C; 0 dòng, schema tối giản |
| SubjectTeachingExams | Kỳ thi | Kỳ thi | A |
| ExamAttempts, ExamResults | Kết quả của tôi | Lượt thi/kết quả | A/C; chưa có rule official/pass |
| QuestionSuites, Questions, QuestionAnswers | Không mặc định | Ngân hàng câu hỏi | A; quyền khảo thí |
| ExamQuestionSelections/Answers | Không mặc định | Detail lượt thi | A; dữ liệu nhạy cảm |
| UserAnnouncements | Thông báo | Thông báo | A |
| FormTemplates, FormRequests | Biểu mẫu | Dịch vụ sinh viên | B; 0 dòng |
| Settings | Không | Hệ thống có giới hạn | A; cần phân loại secret |
| UserDevices | Không | Không mặc định | A; nhạy cảm |
| AuditLogs, PasswordResets | Không | Hệ thống có giới hạn | B; 0 dòng |

## 12. Những nội dung UI không được vượt quá database

| Nội dung quen thuộc ở website đại học | Database hiện tại | Quyết định PRD |
|---|---|---|
| GPA hệ 4/hệ 10, điểm chữ | Chỉ có result raw/combined raw | Không tính/hiển thị cho tới khi có công thức và rule chọn kết quả |
| Đạt/rớt, xếp loại | Không có ngưỡng/quy tắc | Không suy diễn |
| Tín chỉ tích lũy/tiến độ tốt nghiệp | Có credit kế hoạch nhưng không có mapping hoàn thành chuẩn | Chỉ hiện tín chỉ kế hoạch; không có progress tốt nghiệp |
| Môn bắt buộc/tự chọn/tiên quyết | Không có field/quan hệ | Không hiển thị; cần mở rộng schema |
| Phiên bản chương trình | Không có curriculum version | Gọi là kế hoạch học kỳ tham chiếu |
| Lớp hành chính/cố vấn học tập | Không có mô hình xác định | Không đồng nhất với lớp học phần |
| Học hàm/học vị/chức danh GV | Không có | Không tạo nội dung giả |
| Chuyên cần % | Status chưa có semantics và chưa có denominator rule | Chỉ raw status sau khi map được phê duyệt |
| Công nợ/miễn giảm/hạn đóng | `SemesterTuitions` quá tối giản và rỗng | Chỉ empty state; cần mô hình tài chính riêng |
| CLO/PLO/kỹ năng/chủ đề/chương | Không có schema | Ngoài phạm vi, backlog dữ liệu |
| Thông báo đã đọc | Có status/enforce-read nhưng write không thuộc phase | Có thể xem; cập nhật đọc để phase sau |
| Nộp/duyệt biểu mẫu | Có schema nhưng rỗng; là thao tác ghi | Thiết kế empty/read view; workflow phase sau |

## 13. Yêu cầu read model/API

UI không được tự join nhiều danh sách đầy đủ ở client. Backend cần cung cấp read model theo use case:

- `StudentSummary`: student + user + major + faculty + academic year.
- `StudentProgram`: semester plan + semester subjects + catalog subject khi có.
- `StudentTeaching`: enrollment + subject teaching + subject + teachers + room.
- `ScheduleEvent`: subject/class/room/teacher labels và thời gian.
- `AttendanceRecord`: student + schedule + subject/class/room labels.
- `ExamResultRecord`: student + exam + attempt + class/subject labels.
- `TeacherSummary`: teacher-faculty + user + teaching count.
- `TeachingDetail`: subject + teachers + enrollments + schedules + exams.
- `ManagementOverview`: aggregate bằng query có kiểm soát, không kéo raw dataset về client.

Mọi endpoint list cần `page`, `pageSize`, `search`, filter đặc thù, `sort` allowlist và response gồm `items`, `page`, `pageSize`, `totalCount`. Cross-service composition phải xử lý missing label bằng “Không xác định” và cờ chất lượng dữ liệu, không hiện GUID thay thế.

## 14. Quy tắc dữ liệu và trình bày

- Dùng `IsDeleted = false` ở mọi query nghiệp vụ thích hợp.
- Thời gian hiển thị `dd/MM/yyyy HH:mm`, xử lý timezone nhất quán.
- Số điểm không tự làm tròn làm thay đổi nghĩa; hiển thị precision theo quy tắc đã chốt.
- Enum phải có data dictionary được phê duyệt. Nếu chưa có, dùng “Mã trạng thái: n” tại màn hình quản trị hạn chế; Student Portal phải ẩn hoặc dùng nhãn trung tính.
- Snapshot fields như `SemesterSubject.SubjectName/SubjectCode` được ưu tiên cho lịch sử; nếu lệch danh mục hiện tại, UI có thể cảnh báo nhưng không tự thay thế.
- Dữ liệu rỗng khác lỗi. Empty state phải nói rõ chưa có bản ghi, không nói “hệ thống lỗi”.
- Không export trường nhạy cảm mặc định; mọi export phải áp dụng cùng filter/quyền như list và có giới hạn số dòng.

## 15. Bảo mật và riêng tư

- Student ownership phải lấy từ token đã xác thực, áp dụng trong database query trước projection/paging; không nhận `studentId` tùy ý từ client.
- Management Portal cần RBAC/ABAC theo role và phạm vi khoa/đơn vị; deny-by-default.
- TLS, security headers, CORS allowlist, rate limiting và session expiry bắt buộc khi triển khai.
- Không log token, mật khẩu, hash/salt, push ID, số định danh đầy đủ hoặc dữ liệu thi nhạy cảm.
- Chống XSS cho announcement, note, question/answer text; validate link và image URL.
- Audit cho truy cập nhạy cảm và export cần một thiết kế riêng; bảng `AuditLogs` rỗng không chứng minh audit hiện hữu.
- Phiên bản hiện tại giữ nguyên database: GET/read query, ngoại trừ login phát hành token không ghi DB. Mọi đánh dấu đã đọc, tạo form hay quản lý dữ liệu phải qua change proposal khác.

## 16. Phi chức năng

### Hiệu năng

- Không `SELECT *`; projection đúng field theo list/detail.
- Danh sách attendance/result/question/answer bắt buộc server pagination.
- Index/filter plan phải được đánh giá bằng query plan trên bản sao hoặc kết nối chỉ đọc; không thay schema trong scope này.
- Cache ngắn cho danh mục ít thay đổi (khoa, ngành, phòng, năm học); dữ liệu cá nhân/điểm không cache dùng chung.
- Hủy request cũ khi người dùng đổi filter nhanh; tránh N+1 và lookup toàn bảng.

### Khả dụng và lỗi

- Có correlation ID hỗ trợ; thông báo lỗi thân thiện không lộ stack trace.
- Một service phụ lỗi không được khiến toàn bộ dashboard trắng; widget có fallback riêng.
- Retry chỉ cho request đọc idempotent; không retry vô hạn.

### Accessibility và responsive

- Mục tiêu WCAG 2.1 AA: keyboard navigation, focus rõ, contrast, label/form error, screen-reader text.
- Không truyền đạt status chỉ bằng màu; có text/icon.
- Student Portal hỗ trợ tốt từ 360 px; bảng chuyển card/list hoặc horizontal scroll có cột ưu tiên.
- Management Portal tối ưu từ 1280 px, usable từ tablet 768 px.

## 17. Acceptance criteria cấp sản phẩm

1. Sinh viên đăng nhập và chỉ nhận dữ liệu thuộc chính mình trên mọi endpoint Student Portal.
2. Dashboard sinh viên dẫn tới lịch, điểm danh, kết quả và thông báo trong tối đa 3 thao tác.
3. Cán bộ có thể mở một sinh viên và xem các bản ghi lớp/lịch/điểm danh/thi liên quan mà không sao chép GUID.
4. Cán bộ có thể đi từ khoa/ngành tới kế hoạch và môn; từ môn tới lớp học phần; từ lớp tới giảng viên/sinh viên/lịch/kỳ thi.
5. Search/filter/sort/paging nằm trong URL và được thực thi phía server ở danh sách lớn.
6. Mỗi list/detail có loading, empty, error, retry và permission-denied state phù hợp.
7. Không có password hash/salt, push ID hoặc dữ liệu định danh đầy đủ trong network response không cần thiết.
8. Attendance không dùng nhãn hoặc tỷ lệ chưa được phê duyệt.
9. Exam result không tự tạo GPA, đạt/rớt, điểm chữ hoặc kết quả chính thức.
10. Các bảng rỗng hiển thị empty state thật, không seed hay giả lập dữ liệu trên database cố định.
11. Tất cả 36 bảng được phân loại: có UI nghiệp vụ, chỉ dùng làm lookup/detail, hoặc chủ động không hiển thị vì kỹ thuật/nhạy cảm.
12. Bộ kiểm tra read-only xác nhận không có migration, seed hay write API trong phạm vi release.

## 18. Lộ trình đề xuất

### Phase 0 — Chốt nghiệp vụ và an toàn

- Chốt mã sinh viên chính thức và cơ chế đăng nhập production.
- Ban hành data dictionary cho Role/Status/Type/Level, đặc biệt attendance và exam.
- Chốt RBAC/phạm vi dữ liệu quản trị và masking.
- Xác nhận quy tắc công bố kết quả, đánh giá, thông báo và dữ liệu khảo thí.

### Phase 1 — Nền tảng tra cứu cốt lõi

- App shell, auth, overview hai cổng.
- Hồ sơ, cơ cấu đào tạo, sinh viên, giảng viên, môn, kế hoạch, lớp, lịch.
- Read models, server filters, master–detail và URL state.

### Phase 2 — Điểm danh, thi, đánh giá và truyền thông

- Điểm danh sau khi chốt enum.
- Kỳ thi/kết quả raw và ngân hàng câu hỏi theo quyền.
- Đánh giá sinh viên và thông báo.

### Phase 3 — Module có schema nhưng đang rỗng

- Tài liệu môn, học phí tối giản, mẫu biểu/yêu cầu khi có dữ liệu thật và owner nghiệp vụ.

### Phase 4 — Nghiệp vụ ghi và mở rộng dữ liệu

- Workflow biểu mẫu, đã đọc thông báo, quản lý dữ liệu.
- GPA/đạt-rớt/tích lũy, curriculum version, môn bắt buộc/tự chọn/tiên quyết.
- Lớp hành chính, cố vấn, chuẩn đầu ra CLO/PLO và mô hình tài chính đầy đủ.

## 19. Rủi ro và câu hỏi cần quyết định

| Ưu tiên | Câu hỏi/quyết định |
|---|---|
| P0 | Trường nào là mã sinh viên chính thức: `Nickname`, `UserInternalId` hay `UserName`? |
| P0 | Thuật toán xác thực/SSO production là gì? |
| P0 | Ý nghĩa chính xác của các giá trị attendance status 1–4? |
| P0 | Quy tắc chọn kết quả thi chính thức, ngưỡng đạt, điểm chữ và công bố điểm? |
| P0 | Các role và phạm vi khoa/đơn vị của Management Portal? |
| P1 | `SubjectTeaching` được gắn học kỳ/ngành bằng quan hệ nào, hay cần bổ sung schema? |
| P1 | Sinh viên được xem breakdown câu hỏi/đáp án tới mức nào và sau thời điểm nào? |
| P1 | `StudentEvaluations.Type` và thang điểm có data dictionary nào? |
| P1 | `UserAnnouncements.UserIds` được serialize theo chuẩn nào; status đọc được lưu ra sao? |
| P2 | Có cần hỗ trợ export và audit truy cập dữ liệu nhạy cảm ngay phase đầu? |

## 20. Definition of Ready cho triển khai

PRD sẵn sàng chuyển thành backlog triển khai khi hoàn tất tối thiểu:

- Phê duyệt sitemap và danh sách P0/P1.
- Chốt bốn quyết định P0 về mã sinh viên, xác thực, attendance và kết quả thi.
- Có RBAC matrix và chính sách masking.
- Có data dictionary enum và định nghĩa field tổng hợp.
- API contract/read model cho các màn hình P0 được review với database owner.
- Wireframe desktop/mobile cho hai app shell, overview, list và detail mẫu được duyệt.
- Kế hoạch test ownership, cross-service missing labels, empty tables và dataset lớn được thống nhất.

---

### Phụ lục — Quyết định thiết kế cốt lõi

PRD này cố ý không biến mọi bảng thành một menu. Các bảng kỹ thuật hoặc bảng nối được dùng phía sau read model; UI tổ chức theo hành trình nghiệp vụ. Mọi tính năng “giống website đại học” nhưng chưa có dữ liệu hoặc quy tắc đã được tách khỏi scope hiện tại để tránh giao diện đẹp nhưng đưa ra kết luận sai về sinh viên.
