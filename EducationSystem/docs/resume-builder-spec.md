# Đặc tả chức năng CV thông minh cho sinh viên

## 1. Mục tiêu và phạm vi hiện tại

Chức năng **CV thông minh** tại route `/student/resume-builder` giúp sinh viên tạo một CV A4, một trang, thân thiện ATS từ dữ liệu học vụ, dự án e-portfolio và lịch sử thực tập đã được xác nhận.

Sinh viên chọn dữ liệu muốn đưa vào CV, nhập vị trí/JD, xem trước, chỉnh sửa nội dung và in/xuất PDF. AI (khi dịch vụ AI được cấu hình) chỉ tối ưu cách diễn đạt và nhóm kỹ năng; không được tự tạo kinh nghiệm, dự án hoặc kỹ năng không có dữ liệu nguồn.

Phạm vi này không bao gồm: xếp hạng sinh viên, tự động đánh giá khả năng được tuyển, tự công bố CV cho nhà tuyển dụng, hoặc lưu một CV đã chỉnh sửa thành hồ sơ học vụ chính thức.

## 2. Vai trò và quyền truy cập

| Vai trò | Quyền |
|---|---|
| Sinh viên đã đăng nhập | Chỉ đọc dữ liệu CV của chính mình, chọn mục, nhập/chỉnh nội dung trong phiên làm việc, tối ưu và xuất CV. |
| Doanh nghiệp/hướng dẫn thực tập | Chỉ mở liên kết token để xác nhận thông tin thực tập, chấm điểm 0--10 và để lại nhận xét. Không xem CV. |
| Quản trị viên/nhà trường | Phê duyệt yêu cầu thực tập sau khi doanh nghiệp xác nhận. Không chỉnh CV của sinh viên trong chức năng hiện tại. |

API dữ liệu CV yêu cầu chính sách `StudentPolicy` và kiểm tra `studentId` trên URL phải đúng với sinh viên trong token. API đồng bộ thực tập chỉ cho phép gọi nội bộ bằng API key.

## 3. Nguồn dữ liệu được sử dụng

| Nhóm | Nguồn thực tế | Trường sử dụng | Điều kiện / cách xử lý |
|---|---|---|---|
| Định danh học vụ | `academic.Students`, `academic.Majors`, `academic.Faculties`, `academic.AcademicYears`, identity user nội bộ | Họ tên, mã/tên ngành, khoa, niên khóa | Chỉ đồng bộ sang CV: họ tên và tên ngành. API có trả thêm mã ngành, khoa, niên khóa nhưng preview hiện chưa dùng. |
| Kết quả học tập | `academic.StudentEvaluations`, `StudentEvaluationDetails`, `SubjectTeachings`, `Subjects` | Mã/tên môn, số tín chỉ, điểm tổng kết, tiêu chí/chuẩn đầu ra | Chỉ lấy phiếu không bị xóa, điểm hợp lệ trong [0,10], lớp/môn còn hiệu lực. Mỗi môn được gộp bằng điểm trung bình các lần đánh giá. |
| GPA | Tính từ các môn hợp lệ ở trên | GPA hai chữ số thập phân | Trung bình có trọng số tín chỉ; nếu tổng tín chỉ bằng 0 thì trung bình cộng. Đây là GPA phục vụ CV, không thay thế GPA chính thức nếu quy chế trường có cách tính khác. |
| Học phần nổi bật | Tập môn đã gộp | Môn, điểm, chuẩn đầu ra | Chỉ các môn có điểm >= 7.0 mới là `eligibleCourses`; sinh viên tự tick/bỏ tick từng môn. |
| Dự án | `academic.StudentProjects` | Tên, tech stack, mô tả, URL mã nguồn, quy mô nhóm, vai trò, đóng góp, môn học liên kết | Dự án thuộc sinh viên. `TeamSize >= 1`; có thể gắn một môn học. UI hiện cho thêm/sửa/xóa trong bộ nhớ trình duyệt, chưa có API lưu dự án từ màn CV. |
| Thực tập | `academic.StudentInternships` | Công ty, vị trí, ngày bắt đầu/kết thúc, mô tả nhiệm vụ | Chỉ bản ghi được tạo sau quy trình doanh nghiệp xác nhận và nhà trường phê duyệt. Sinh viên tự chọn từng đợt để đưa vào CV. |
| Mục tiêu ứng tuyển | Người dùng nhập trên màn CV | Vị trí mong muốn, JD tối đa 1.500 ký tự | Là ngữ cảnh cho AI và tiêu đề CV, không phải dữ liệu học vụ. |
| Thông tin liên hệ | Trạng thái UI hiện tại | Email, số điện thoại, địa điểm, tên trường, mã SV | Đang lấy từ `initialResumeData` mẫu và cho phép sửa trực tiếp trên preview. API `resume/get-context-data` hiện **không trả** các trường này. Không được coi dữ liệu mẫu là dữ liệu chính thức. |

### Dữ liệu tuyệt đối không đưa vào CV tự động

Không dùng số định danh, ngày sinh, địa chỉ thường trú, gia đình/người thân, dân tộc/tôn giáo, tình trạng học vụ, cờ vấn đề, học phí, chuyên cần, chi tiết câu trả lời thi, mật khẩu, thiết bị hay audit log. Các trường này không liên quan trực tiếp đến năng lực tuyển dụng hoặc là dữ liệu nhạy cảm.

## 4. Nhóm nội dung được viết vào CV

| Phần CV | Nội dung | Nguồn | Quy tắc hiển thị |
|---|---|---|---|
| Header | Họ tên, vị trí mục tiêu, email, điện thoại, địa điểm | Họ tên từ API; phần còn lại hiện là UI/mẫu | Có thể sửa trực tiếp; không tự lấy thông tin nhạy cảm. |
| Tóm tắt chuyên môn | Một đoạn định hướng, nền tảng và giá trị phù hợp JD | AI + dữ liệu được chọn | Phải để sinh viên duyệt/sửa; AI chỉ dùng claims có bằng chứng. |
| Năng lực chuyên môn | Kiến thức; kỹ năng nghề nghiệp; kỹ năng cá nhân | AI + học phần, chuẩn đầu ra, dự án, thực tập | Đây là gợi ý, không phải chứng nhận. Không được khẳng định kỹ năng cá nhân nếu không có dữ liệu/đồng ý của sinh viên. |
| Dự án nổi bật | Tên, vai trò/cá nhân, tech stack, đóng góp | `StudentProjects` hoặc sinh viên thêm trong UI | Hiện mọi dự án có tên đều được preview; cần chọn lọc theo JD khi AI hoạt động. |
| Kinh nghiệm làm việc | Vị trí, thời gian, công ty, nhiệm vụ | Chỉ `StudentInternships` đã phê duyệt | Chỉ hiện khi sinh viên đã chọn ít nhất một đợt. Có nhãn ngầm “school-approved” trong dữ liệu UI nhưng không in nhãn này lên CV. |
| Học vấn & học phần tiêu biểu | Trường, ngành, GPA, MSSV, môn và điểm đã chọn | Ngành/GPA/môn từ API; trường/MSSV hiện là mẫu | Chỉ hiển thị các học phần sinh viên chọn. GPA không có cơ chế ẩn riêng trong UI hiện tại. |

CV hiện chưa có các phần: chứng chỉ, ngoại ngữ, giải thưởng, hoạt động CLB, liên kết LinkedIn/portfolio, ảnh đại diện hoặc thư giới thiệu.

## 5. Luồng nghiệp vụ

```text
Đăng nhập sinh viên
  -> tải context học vụ và danh sách thực tập đã phê duyệt
  -> sinh viên nhập vị trí/JD; chọn môn và đợt thực tập; kiểm tra/sửa dự án
  -> hệ thống tạo bản preview từ dữ liệu đã chọn
  -> (tuỳ cấu hình) gửi payload sang AI để tối ưu câu chữ/kỹ năng
  -> sinh viên rà soát, chỉnh sửa trực tiếp
  -> in hoặc xuất PDF A4
```

`VITE_RESUME_API_MODE=live` bật đồng bộ context học vụ. `VITE_AI_OPTIMIZE_MODE=live` mới gọi API AI. Khi chưa bật AI, UI đang dùng nội dung mô phỏng; kết quả này không được hiểu là AI đã đánh giá hồ sơ thật.

## 6. Đánh giá, xác nhận thực tập và cách đưa vào CV

### 6.1 Quy trình xác nhận hiện có

1. Sinh viên gửi yêu cầu: tên công ty, vị trí, email người hướng dẫn, ngày bắt đầu/kết thúc và mô tả nhiệm vụ. Ngày kết thúc không được trước ngày bắt đầu.
2. Hệ thống tạo token và gửi email cho người hướng dẫn ở doanh nghiệp.
3. Doanh nghiệp xác nhận thông tin đúng/sai, chấm điểm bắt buộc từ 0 đến 10 và có thể nhập nhận xét.
4. Chỉ khi doanh nghiệp xác nhận đúng (`EmployerVerifiedStatus = 1`), nhà trường mới có thể phê duyệt.
5. Khi phê duyệt, Communication Service gọi Academic Service tạo `StudentInternship`. Thao tác idempotent theo `FormRequestId`, tránh sinh trùng khi retry.
6. Màn CV chỉ đọc `StudentInternship`; vì vậy chỉ thực tập đã qua cả hai bước mới xuất hiện để chọn.

### 6.2 Dữ liệu nào được dùng trong CV

Sau khi phê duyệt, CV nhận: **công ty, vị trí, thời gian, mô tả nhiệm vụ**. Sinh viên có thể sửa bản diễn đạt hiển thị trong preview; sửa này hiện không ghi ngược về bản ghi thực tập chính thức.

Điểm doanh nghiệp, nhận xét và thời điểm xác nhận được lưu trong `communication.FormRequests.VerificationData` dưới `EmployerAssessment`, và hiện hiển thị cho luồng quản trị biểu mẫu. Chúng **không được đồng bộ vào `StudentInternships`, không có trong `ResumeContextDto`, và không được in/đưa vào prompt AI**.

### 6.3 Quy tắc đánh giá đề xuất nếu mở rộng

Không nên biến điểm thực tập thành điểm xếp hạng CV. Nếu cần sử dụng trong tương lai, chỉ dùng làm mức xác thực nội bộ:

| Trạng thái | Xử lý CV |
|---|---|
| Chưa xác nhận / bị doanh nghiệp từ chối | Không hiển thị. |
| Doanh nghiệp xác nhận, chưa được trường duyệt | Không hiển thị. |
| Được trường duyệt | Cho phép sinh viên chọn; đưa dữ liệu mô tả đã xác thực. |
| Điểm/nhận xét doanh nghiệp | Không tự in. Chỉ cho phép trích một nhận xét sau khi doanh nghiệp và sinh viên đồng ý rõ ràng. |

Nếu cần dùng điểm để gợi ý phát triển, phải tách thành dashboard riêng, giải thích tiêu chí và không gửi cho nhà tuyển dụng mặc định.

## 7. Quy tắc AI và các chỉ số hiển thị

Payload AI gồm thông tin CV đã chọn, vị trí mục tiêu và JD. API dự kiến trả về `professionalSummary`, ba nhóm skills, và có thể trả hai chỉ số:

- **Content preservation**: tỷ lệ nội dung nguồn còn được bảo toàn sau tối ưu. UI mô tả theo token/ngữ nghĩa; hiện giá trị mặc định/mô phỏng là 95%.
- **Job alignment**: độ tương đồng embedding giữa CV và JD. Đây là chỉ số hỗ trợ chỉnh CV, không là điểm năng lực hay xác suất trúng tuyển; hiện mặc định/mô phỏng là 65% và 88% sau thao tác giả lập.

Yêu cầu bắt buộc cho AI:

1. Chỉ dùng nội dung được sinh viên chọn và JD người dùng nhập.
2. Không tự tạo công ty, dự án, con số kết quả, chứng chỉ, thời gian làm việc hay kỹ năng.
3. Giữ nguyên nghĩa, không thay đổi dữ kiện; kết quả luôn có thể sửa trước khi xuất.
4. Nếu không đủ minh chứng, dùng ngôn ngữ thận trọng như “đã học/đã tham gia”, không dùng “thành thạo/chuyên gia”.
5. Lưu log kỹ thuật tối thiểu, không gửi trường dữ liệu nhạy cảm sang dịch vụ AI.

## 8. API và hợp đồng dữ liệu chính

| API | Mục đích | Bảo vệ |
|---|---|---|
| `GET /api/academic/resume/get-context-data/{studentId}` | Trả hồ sơ học vụ, GPA, môn đủ điều kiện, dự án và thực tập đã duyệt | StudentPolicy + ownership check |
| `GET /api/academic/students/{studentId}/internships` | Trả lịch sử thực tập đã phê duyệt để sinh viên chọn | StudentPolicy + ownership check |
| `POST /api/communication/.../internship` | Sinh viên gửi khai báo thực tập | Xác thực sinh viên theo controller tương ứng |
| `POST /verify-internship` | Doanh nghiệp xác nhận/chấm 0--10 theo token | Token một lần |
| Phê duyệt thực tập | Quản trị viên phê duyệt sau xác nhận | Quyền quản trị |
| `POST /api/ai/resume/optimize` | Hợp đồng AI dự kiến, chưa có service triển khai trong repo | Chỉ bật qua cấu hình live |

## 9. Các khoảng trống cần hoàn thiện

1. Bổ sung API nguồn chính thức cho email, điện thoại, địa điểm được phép công khai, tên trường và mã sinh viên; bỏ `initialResumeData` khi chạy live.
2. Thêm API CRUD có ownership cho `StudentProjects`; hiện context chỉ đọc, còn thao tác UI không bền vững.
3. Lưu version CV, lựa chọn môn/thực tập, ngôn ngữ, mục tiêu/JD và thời điểm xuất để người dùng mở lại được bản cũ.
4. Bổ sung lựa chọn ẩn GPA và MSSV; không nên bắt buộc công khai.
5. Làm rõ cách tính GPA theo quy chế đào tạo và tránh dùng `StudentEvaluations` nếu chúng là khảo sát/đánh giá không tương đương điểm học phần chính thức.
6. Quy định rõ tên chuẩn đầu ra: hiện dữ liệu lấy từ `StudentEvaluationDetails/EvaluationCriteria`, không phải một bảng CLO/PLO chuyên biệt.
7. Triển khai AI thật cùng kiểm thử chống bịa dữ liệu, kiểm tra tiếng Việt/tiếng Anh và cảnh báo nội dung không có nguồn.
8. Quy định retention, xoá/xuất dữ liệu CV và việc chia sẻ với bên thứ ba trước khi mở tính năng gửi CV.

