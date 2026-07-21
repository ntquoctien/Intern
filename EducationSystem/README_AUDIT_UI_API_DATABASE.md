# Báo cáo hiện trạng UI – API – Database của EducationSystem

> Phạm vi audit: source tại thời điểm 20/07/2026. Báo cáo chỉ phân tích, không sửa code và không thiết kế lại UI. Mọi kết luận dưới đây được truy vết từ route → component → HTTP client → controller/query service/DTO → entity/DbContext.

## Phần A — Tổng quan

### A.1. Kết quả định lượng

| Chỉ số | Kết quả | Căn cứ |
|---|---:|---|
| Route nghiệp vụ có trang dữ liệu | 10 | `education-system-ui/src/app/router.tsx:20-29` |
| Khai báo route con tổng cộng | 12 | 10 route nghiệp vụ + index redirect + wildcard redirect |
| Trang có dữ liệu | 10 | Cả 10 trang dùng `DataTablePage` |
| Bảng được EF map | 36 | 20 academic + 8 exam + 5 identity + 3 communication trong bốn `*DbContext.cs` |
| Bảng có API đọc | 29 | 13 academic + 8 exam + 5 identity + 3 communication |
| Bảng có dữ liệu xuất hiện trực tiếp/qua lookup trên UI | 17 | 10 bảng chính + 7 bảng chỉ dùng làm lookup |
| Bảng chưa xuất hiện trên UI | 19 | Xem ma trận Phần D |
| Kiểu hiển thị phổ biến nhất | Data table + detail drawer | Dùng chung `DataTablePage.tsx` và `DetailDrawer.tsx` |
| Trang có search text | 9/10 | Chỉ Subject Students đặt `searchable={false}` |
| Trang có ít nhất một filter hiển thị được | 6/10 | Students, Subject Students, Subject Schedules, Attendances, Exam Results, Form Requests |
| Trang có server-side pagination | 10/10 | `getPaged`, `Skip/Take`, `PagedResult`; giới hạn backend 100 dòng/trang |
| Trang tải toàn bộ bảng chính | 0/10 | Danh sách chính đều phân trang server |
| Trang tải toàn bộ lookup phụ | 9/10 | `getLookup()` gọi `/lookup` không phân trang; trừ Users không có relation lookup |
| Export Excel/CSV/PDF | 0/10 | Không có control hoặc HTTP call export |
| Create/Edit/Delete | 0/10 | Chỉ có xem Detail, Reset, Refresh |

### A.2. Kiến trúc hiển thị chung

Tất cả trang là biến thể của một component dùng chung:

1. `DataTablePage` giữ `pageNumber`, `pageSize`, `search`, `sortBy`, `sortDirection` và filter trong React state (`DataTablePage.tsx:118-137`).
2. `getPaged` gửi các tham số bằng GET tới service tương ứng (`httpClient.ts:13-23`).
3. Controller nhận `QueryParameters` và `ResourceQueryFilters`, query service lọc rồi `Count`, `Skip`, `Take`, project sang DTO.
4. Các FK được khai báo trong `relationLookups` sẽ gọi `/lookup`, tải toàn bộ lookup và đổi GUID thành nhãn. FK không có lookup được renderer thành “Linked record”, hoặc còn nguyên GUID nếu không có renderer.
5. Click `Detail` gọi `GET /{id}` và mở drawer 640px. Drawer tự liệt kê DTO detail, ẩn một số trường nhạy cảm/kỹ thuật.

Trạng thái loading, empty, error đều có; table dùng `scroll={{x: 'max-content'}}`, toolbar/filter có flex-wrap. Responsive vì vậy ở mức **một phần**: màn hình hẹp có cuộn ngang, nhưng không có card/mobile layout và drawer cố định 640px.

### A.3. Quy ước trạng thái chức năng

- **Có – server**: backend thực hiện search/filter/sort/pagination.
- **Có – UI**: có control và dữ liệu lookup được xử lý ở frontend, điều kiện cuối vẫn gửi server.
- **Một phần**: có nhưng phạm vi field/quan hệ chưa đủ hoặc nhãn UI không khớp query.
- **Không**: không có control/endpoint tương ứng.

| Chức năng chung | Trạng thái |
|---|---|
| Keyword search | Có – server trên 9 trang; Subject Students không có |
| Sort theo cột | Có – server với các cột đặt `sorter: true`; cột relation bị chủ động tắt sort sau khi gắn lookup |
| Pagination/chọn page size | Có – server, mặc định 20, backend cap 100 |
| Kết hợp nhiều filter | Có trên 5 trang có từ hai filter; filters được gửi cùng request |
| Reset filter/search/sort | Có |
| Lưu filter trên URL | Không; state mất khi reload/share URL |
| Export | Không |
| CRUD | Không; chỉ read list/detail |

## Phần B — Ma trận Page–API–Database

| Trang | Route | Component | API danh sách / DTO | Bảng DB chính và lookup | Hiển thị | Search | Filter | Pagination |
|---|---|---|---|---|---|---|---|---|
| Học viên | `/academic/students` | `StudentsPage` | `GET /api/academic/students` / `StudentListItemDto` | `academic.Students`; lookup `identity.Users`, `academic.AcademicYears`, `academic.Majors` | 8 cột + detail | Một phần | User | Server |
| Môn học | `/academic/subjects` | `SubjectsPage` | `GET /api/academic/subjects` / `SubjectListItemDto` | `academic.Subjects`; lookup `academic.Faculties` | 6 cột + detail | Có | Không | Server |
| Lớp học phần | `/academic/subject-teachings` | `SubjectTeachingsPage` | `GET /api/academic/subject-teachings` / `SubjectTeachingListItemDto` | `academic.SubjectTeachings`; lookup `Subjects`, `Rooms` | 7 cột + detail | Có | Không | Server |
| Ghi danh học phần | `/academic/subject-students` | `SubjectStudentsPage` | `GET /api/academic/subject-students` / `SubjectStudentListItemDto` | `academic.SubjectStudents`; lookup `Students`, `SubjectTeachings` | 2 cột + detail | Không | Student, lớp học phần | Server |
| Lịch học | `/academic/subject-schedules` | `SubjectSchedulesPage` | `GET /api/academic/subject-schedules` / `SubjectScheduleListItemDto` | `academic.SubjectSchedules`; lookup `SubjectTeachings`, `Rooms` | 6 cột + detail | Có | Lớp học phần, ngày | Server |
| Điểm danh | `/academic/attendances` | `AttendancesPage` | `GET /api/academic/attendances` / `AttendanceListItemDto` | `academic.Attendances`; lookup `Students` | 7 cột + detail | Có | Student, status, ngày | Server |
| Kết quả thi | `/exam/exam-results` | `ExamResultsPage` | `GET /api/exam/exam-results` / `ExamResultListItemDto` | `exam.ExamResults`; lookup `academic.Students`, `exam.SubjectTeachingExams` | 7 cột + detail | Có | Student, kỳ thi | Server |
| Ngân hàng câu hỏi | `/exam/questions` | `QuestionsPage` | `GET /api/exam/questions` / `QuestionListItemDto` | `exam.Questions`; lookup `exam.QuestionSuites` | 4 cột + detail | Có | Không | Server |
| Tài khoản | `/identity/users` | `UsersPage` | `GET /api/identity/users` / `UserListItemDto` | `identity.Users` | 6 cột + detail | Có | Không | Server |
| Yêu cầu biểu mẫu | `/communication/form-requests` | `FormRequestsPage` | `GET /api/communication/form-requests` / `FormRequestListItemDto` | `communication.FormRequests`; lookup `academic.Students`, `communication.FormTemplates` | 7 cột + detail | Có | Student, status, ngày | Server |

## Phần C — Báo cáo chi tiết từng trang

### C.1. Học viên

- **Mục đích/người dùng dự kiến:** tra hồ sơ học viên cho quản trị viên và nhân viên đào tạo.
- **Cấu trúc UI:** mỗi row là một `Student`; 8 cột + Action/Detail. User, năm học, ngành được đổi từ GUID sang label bằng ba lookup. Không có create/edit/delete.
- **Khi nhiều dữ liệu:** list phân trang server; đồng thời tải toàn bộ Users, AcademicYears, Majors qua lookup. Loading/empty/error có đủ.
- **Search:** placeholder nói “tên hoặc mã học viên”, nhưng query chỉ tìm `Nickname`, nơi sinh, quê quán, địa chỉ, dân tộc, tôn giáo, trình độ. Không join `identity.Users`, nên không tìm được `User.FullName`/`UserInternalId`: **một phần và nhãn gây hiểu nhầm**.
- **Filter:** chỉ `userId`; dù đã có lookup AcademicYear và Major nhưng không filter được theo hai quan hệ này. Sort server cho nickname/status/gender/graduation/issue; pagination và page-size có.

| STT | Nhãn UI | Field FE/API DTO | Cột DB | Bảng nguồn | Kiểu | Ghi chú |
|---:|---|---|---|---|---|---|
| 1 | User | `userId` | `UserId` | `academic.Students` → lookup `identity.Users.Id` | Guid | Hiển thị FullName/UserName; quan hệ cross-service không có FK EF |
| 2 | Academic Year | `academicYearId` | `AcademicYearId` | `Students` → `AcademicYears` | Guid | Hiển thị `AcademicYear.Name` |
| 3 | Major | `majorId` | `MajorId` | `Students` → `Majors` | Guid | Hiển thị Code–Name |
| 4 | Mã học viên | `nickname` | `Nickname` | `Students` | string? | Tên field không chắc là mã chính thức |
| 5 | Trạng thái | `studyStatus` | `StudyStatus` | `Students` | int? | FE format tag |
| 6 | Giới tính | `gender` | `Gender` | `Students` | int? | FE map enum/tag |
| 7 | Đã tốt nghiệp | `isGraduated` | `IsGraduated` | `Students` | bool | FE tag Yes/No |
| 8 | Có vấn đề | `hasIssue` | `HasIssue` | `Students` | bool? | FE tag Yes/No |

| Nhóm cột `academic.Students` | Phân loại hiện trạng |
|---|---|
| `Id` | PK kỹ thuật; dùng row/detail request, không hiển thị |
| 8 field ở bảng trên | Có trong API và list UI |
| `RelativeUserId`, `PlaceOfBirth`, `Hometown`, `PermanentAddress`, `ContactAddress`, `Ethnicity`, `Religion`, `EducationLevel`, `FatherName`, `FatherOccupation`, `MotherName`, `MotherOccupation`, `SpouseName`, `SpouseOccupation`, `PolicySubject`, `PreviousOccupation`, `PostGraduationWorkplace`, `CommunistPartyJoinDate`, `OfficialPartyJoinDate`, `YouthUnionJoinDate`, `IssueDescription`, `LibraryId` | Có trong list DTO dù list không dùng; phần lớn chỉ phù hợp detail; các field địa chỉ/thân nhân bị drawer ẩn một phần |
| `IsDeleted` | Có trong DTO nhưng query đã loại deleted; field kỹ thuật, không nên hiển thị |
| `UserId`, `AcademicYearId`, `MajorId` | FK nên dùng label và filter; hiện chỉ User filterable |

**Bằng chứng:** `features/academic/pages.tsx` (`studentColumns`, `StudentsPage`); `StudentQueryService.cs`; `StudentListItemDto.cs`; `Student.cs`; `AcademicDbContext.cs` cấu hình FK AcademicYear/Major. Không có join Identity vì mỗi service dùng DbContext riêng.

### C.2. Môn học

- **Mục đích/người dùng:** danh mục môn học cho quản trị/đào tạo/giảng viên.
- **UI:** mỗi row là `Subject`, 6 cột + detail; Faculty được lookup thành label; không CRUD.
- **Search:** server theo `SubjectCode`, `Name`, `Note`; placeholder chỉ nói code/name nên API tìm rộng hơn nhãn. Không filter Faculty, trạng thái active, số tín chỉ.

| STT | Nhãn UI | Field DTO | Cột DB | Nguồn | Kiểu | Ghi chú |
|---:|---|---|---|---|---|---|
| 1 | Mã môn | `subjectCode` | `SubjectCode` | `academic.Subjects` | string | Gốc |
| 2 | Tên môn | `name` | `Name` | `Subjects` | string | Gốc |
| 3 | Khoa/Bộ môn | `facultyId` | `FacultyId` | `Subjects` → `Faculties` | Guid? | Hiển thị label lookup |
| 4 | Tín chỉ | `creditPoint` | `CreditPoint` | `Subjects` | int | FE thêm đơn vị |
| 5 | Giờ học | `totalHours` | `TotalHours` | `Subjects` | int? | FE thêm đơn vị |
| 6 | Đang hoạt động | `isActived` | `IsActived` | `Subjects` | bool | FE tag |

`Note` có trong API/DB nhưng chỉ detail; `IsDeleted` là kỹ thuật và query đã loại; `Id` là PK. `FacultyId` nên filter, `IsActived` nên filter trạng thái. API list đang trả cả `Note`, `IsDeleted` dù table không dùng.

**Bằng chứng:** `academic/pages.tsx` (`subjectColumns`, `SubjectsPage`); `SubjectQueryService.cs`; `SubjectListItemDto.cs`; `Subject.cs`; mapping `Subject.FacultyId` trong `AcademicDbContext.cs`.

### C.3. Lớp học phần (Subject Teachings)

- **Mục đích/người dùng:** xem các đợt/lớp triển khai môn học; nhân viên đào tạo và giảng viên.
- **UI:** mỗi row là `SubjectTeaching`, 7 cột + detail. Subject và room default có lookup. Không filter theo môn/phòng/ngày; chỉ search tên.
- **Lỗi nguồn dữ liệu:** cột “Khoa / Bộ môn” đọc `facultyId`, nhưng `SubjectTeaching` entity, list/detail DTO và query projection đều **không có field này**. Cột sẽ trống; `formatFacilityCode` không thể tạo dữ liệu.

| STT | Nhãn UI | Field FE/API | Cột DB | Nguồn | Kiểu | Ghi chú |
|---:|---|---|---|---|---|---|
| 1 | Tên lớp học phần | `name` | `Name` | `academic.SubjectTeachings` | string | Gốc |
| 2 | Môn học | `subjectId` | `SubjectId` | `SubjectTeachings` → `Subjects` | Guid | Lookup code/name |
| 3 | Khoa/Bộ môn | FE `facultyId`; API không có | Không có trên `SubjectTeachings` | Có thể suy qua `Subjects.FacultyId`, nhưng query không join | — | **Không tìm thấy nguồn trong response** |
| 4 | Ngày bắt đầu | `startDate` | `StartDate` | `SubjectTeachings` | DateTime | FE format date |
| 5 | Ngày kết thúc | `endDate` | `EndDate` | `SubjectTeachings` | DateTime | FE format date |
| 6 | Số buổi | `totalSessions` | `TotalSessions` | `SubjectTeachings` | int | Gốc |
| 7 | Phòng mặc định | `roomIdDefault` | `RoomIdDefault` | `SubjectTeachings` → `Rooms` | Guid? | Lookup room name |

`Id` là PK; `IsDeleted` có trong DTO nhưng bị query loại và chỉ là kỹ thuật. DB còn liên kết `SubjectTeachingTeachers`, `SubjectStudents`, `SubjectSchedules`, `StudentEvaluations`, nhưng detail API không trả các collection. Nên bổ sung filter `SubjectId`, khoảng ngày, teacher, semester/academic year (nếu mô hình hóa được); không đưa collection thô lên list.

**Bằng chứng:** `academic/pages.tsx` (`subjectTeachingColumns`); `SubjectTeachingQueryService.cs`; `SubjectTeachingListItemDto.cs`; `SubjectTeaching.cs`; `AcademicDbContext.cs`.

### C.4. Ghi danh học phần (Subject Students)

- **UI:** mỗi row là quan hệ học viên–lớp học phần, chỉ 2 cột + detail; cả hai FK được lookup thành label và filterable. Không search text, không sort hữu ích ở UI, không CRUD.
- **Dữ liệu:** `Id` PK kỹ thuật; `SubjectTeachingId` và `StudentId` là toàn bộ cột nghiệp vụ của bảng. API/DTO không join tên, nên UI cần tải toàn bộ hai lookup.
- **Tra cứu:** filter kết hợp Student + SubjectTeaching hoạt động server. Chưa lọc ngược theo ngành, môn, khóa/năm học; label Student chỉ dựa `Nickname`, không có họ tên từ Identity.

| Nhãn | Field DTO/DB | Quan hệ | Hiện trạng |
|---|---|---|---|
| Lớp học phần | `SubjectTeachingId` | `SubjectStudents` → `SubjectTeachings` | Lookup tên, filter server |
| Học viên | `StudentId` | `SubjectStudents` → `Students` | Lookup nickname, filter server |

**Bằng chứng:** `SubjectStudentsPage`; `SubjectStudentQueryService.cs` filter hai GUID; `SubjectStudentListItemDto.cs`; `SubjectStudent.cs` và hai FK trong `AcademicDbContext.cs`.

### C.5. Lịch học (Subject Schedules)

- **UI:** mỗi row là một buổi/lịch học, 6 cột + detail. SubjectTeaching và Room đổi thành label; Teacher **không có lookup**, renderer chỉ hiện “Linked record”.
- **Search/filter:** search server chỉ trên `Note`; filter SubjectTeaching và date range; sort server trên thời gian/type. Không lọc room/teacher/schedule type.

| STT | Nhãn | Field DTO/DB | Nguồn/format | Nhận xét |
|---:|---|---|---|---|
| 1 | Lớp học phần | `SubjectTeachingId` | `SubjectSchedules` → lookup `SubjectTeachings` | Có label/filter |
| 2 | Phòng | `RoomId` | → lookup `Rooms` | Có label, chưa filter |
| 3 | Giáo viên | `TeacherId` | → `TeacherFaculties.Id` | Chỉ “Linked record”; không biết tên giáo viên |
| 4 | Bắt đầu | `StartDateTime` | DateTime, FE format | List/sort |
| 5 | Kết thúc | `EndDateTime` | DateTime, FE format | List/sort |
| 6 | Loại | `ScheduleType` | int?, FE tag | Chưa filter |

`Note` có trong DTO/DB, dùng search và detail nhưng không list; `IsDeleted` kỹ thuật; `Id` PK. `TeacherFaculty` chỉ có `UserId`/`FacultyId`, muốn hiện tên phải nối tiếp sang Identity Users qua cross-service lookup/API composition.

**Bằng chứng:** `SubjectSchedulesPage`; `SubjectScheduleQueryService.cs`; `SubjectScheduleListItemDto.cs`; `SubjectSchedule.cs`; `TeacherFaculty.cs`.

### C.6. Điểm danh

- **UI:** mỗi row là một bản ghi điểm danh của học viên tại buổi học, 7 cột + detail. Student có lookup; `SubjectScheduleId` chỉ hiện “Linked record”.
- **Search/filter:** search `Notes`; filter Student, exact numeric Status, CreationDate range. Query service còn hỗ trợ `SubjectScheduleId` nhưng UI không khai báo filter/lookup này — **có trong API nhưng UI chưa khai thác**.

| Nhãn | Field DTO/DB | Kiểu/format | Nhận xét |
|---|---|---|---|
| Học viên | `StudentId` | Guid → nickname | Filter server |
| Buổi học | `SubjectScheduleId` | Guid → “Linked record” | Nên hiện thời gian/lớp/phòng và bật filter |
| Trạng thái | `Status` | int → tag | Input filter số, chưa phải danh sách nhãn |
| Ghi chú | `Notes` | string | Search/sort |
| Ngày ghi nhận | `CreationDate` | DateTime → format | Date filter/sort |
| Cảnh cáo lần 1/2 | `IsFirstTypeWarning`, `IsSecondTypeWarning` | bool? → tag | Gốc |

`CreatedById` có trong API/DB nhưng không có trên list; drawer không có lookup nên chỉ “Linked record”. `Id` PK. Bảng không có `IsDeleted`; đây là toàn bộ 9 cột vật lý của entity.

**Bằng chứng:** `AttendancesPage`; `AttendanceQueryService.cs`; `AttendanceListItemDto.cs`; `Attendance.cs`.

### C.7. Kết quả thi

- **UI:** mỗi row là kết quả một học viên trong một kỳ thi học phần, 7 cột + detail. Student và SubjectTeachingExam có lookup/filter; ExamAttempt chỉ “Linked record”.
- **Search:** server trên `Notes`, `ExamResultDesc`, `ExamResultDetail`; field cuối không có ở table nhưng có detail. Không filter theo mức điểm/pass-fail, môn, lớp, năm/kỳ.

| Nhãn | Field DTO/DB | Kiểu | Nhận xét |
|---|---|---|---|
| Học viên | `StudentId` | Guid | Cross-service lookup, filter |
| Kỳ thi | `SubjectTeachingExamId` | Guid | Lookup tên, filter |
| Lần thi | `ExamAttemptId` | Guid? | Chỉ “Linked record” |
| Kết quả | `Result` | float? | FE tag |
| Kết quả ghép | `CombinedResult` | float? | Gốc |
| Mô tả | `ExamResultDesc` | string? | Search/list |
| Ghi chú | `Notes` | string? | Search/list |

`ExamResultDetail` có API/DB, dùng search và drawer nhưng không list; `IsDeleted` kỹ thuật; `Id` PK. Không có field CLO/PLO/criteria. Liên kết đến lớp/môn phải đi `SubjectTeachingExam.SubjectTeachingId` qua service khác nhưng UI không có drill-down.

**Bằng chứng:** `ExamResultsPage`; `ExamResultQueryService.cs`; `ExamResultListItemDto.cs`; `ExamResult.cs`; `SubjectTeachingExam.cs`.

### C.8. Ngân hàng câu hỏi

- **Đối tượng mỗi row:** một `Question`, không phải bộ đề hay môn học.
- **UI:** 4 cột + detail. Question Suite được lookup thành tên; nội dung không truncate/clamp, CSS cho wrap và overflow visible nên câu dài làm row rất cao. `ImageUrl` chỉ là text, không preview ảnh.
- **Search/pagination:** search server theo `QuestionText` và `ImageUrl`; phân trang server đúng. Lookup toàn bộ QuestionSuites không phân trang.
- **Filter:** không có bất kỳ filter nào, kể cả suite, subject, level/type/status/creator.
- **Preview:** detail chỉ lặp lại 5 field của Question; không tải `QuestionAnswers`, không preview đáp án/đáp án đúng.

| STT | Nhãn UI | Field DTO | Cột DB | Bảng/quan hệ | Kiểu | Nhận xét |
|---:|---|---|---|---|---|---|
| 1 | Bộ câu hỏi | `questionSuiteId` | `QuestionSuiteId` | `Questions` → `QuestionSuites` | Guid | Có label nhưng không filter |
| 2 | Nội dung câu hỏi | `questionText` | `QuestionText` | `Questions` | string | Search, không giới hạn chiều dài UI |
| 3 | Mức độ | `level` | `Level` | `Questions` | int | FE tag; chưa filter |
| 4 | Hình ảnh | `imageUrl` | `ImageUrl` | `Questions` | string? | Hiện URL, không render ảnh |

`Questions` chỉ có thêm `Id` PK. Dữ liệu đáp án nằm ở `QuestionAnswers(Id, QuestionId, AnswerText, ImageUrl, IsAnswer)` và có API riêng nhưng Question detail không composition. `QuestionSuites` có `SubjectId`, `Name`, `UpdatedById`, `CreationTime`; chỉ `Name` đi qua lookup. Từ câu hỏi có thể suy chuỗi DB `Question → QuestionSuite → Subject`, nhưng UI không có link/filter và quan hệ Subject là GUID cross-service, không có EF FK. Không có bảng/cột Chapter, Topic, CLO, PLO, Skill/Competency, QuestionType hoặc UsageStatus trong 36 bảng; vì vậy **chưa đủ bằng chứng** để phân loại theo các chiều này. `EvaluationCriteria.QuestionId` và `StudentEvaluation.QuestionId` là GUID rời, không được EF map tới ExamService.

**Bằng chứng:** `features/exam/pages.tsx` (`questionColumns`, `QuestionsPage`); `QuestionQueryService.cs`; `QuestionDetailDto.cs`; `Question.cs`; `QuestionAnswer.cs`; `QuestionSuite.cs`; `DataTablePage.tsx`; CSS `.ant-table-cell`.

### C.9. Tài khoản

- **UI:** mỗi row là `identity.User`, 6 cột + detail; không relation lookup; intended user là quản trị viên.
- **Search:** server theo username, full name, identification number, internal id, mobile, profile URL; UI help chỉ nói username/full name nên mô tả chưa đầy đủ. Không filter role/active.

| Nhãn | Field DTO/DB | Kiểu/format | Nhận xét |
|---|---|---|---|
| Tên đăng nhập | `UserName` | string | Search/sort |
| Họ tên | `FullName` | string | Search/sort |
| Di động | `Mobile` | string? → masked | Field nhạy cảm, vẫn hiện dạng che |
| Vai trò | `Role` | int → tag | Chưa filter |
| Hoạt động | `IsActived` | bool → tag | Chưa filter |
| Ngày sinh | `BirthDate` | DateTime? | FE date |

API còn trả `IdentificationDate`, `IdentificationNumber`, `UserInternalId`, `ProfilePicUrl`, `LastEnforceAnnouncementRead`, `IsDeleted`; chỉ một phần phù hợp detail. Entity còn `PasswordHash`, `PasswordSalt` nhưng DTO **không trả** — đúng nguyên tắc bảo mật. Drawer chủ động ẩn identification, profile URL, internal id, mobile; `IsDeleted` mặc định ẩn. `LastEnforceAnnouncementRead` có thể hiện detail.

**Bằng chứng:** `UsersPage`; `UserQueryService.cs`; `UserListItemDto.cs`; `User.cs`; `DetailDrawer.tsx` hidden fields.

### C.10. Yêu cầu biểu mẫu

- **UI:** mỗi row là một `FormRequest`, 7 cột + detail. Student và template có lookup; approval hiển thị `ApprovalName` trực tiếp.
- **Search/filter:** search ApprovalName/Note; filter Student, exact numeric Status, CreationDate range. Chưa filter template/approver; status input số không thân thiện.

| Nhãn | Field DTO/DB | Kiểu/nguồn | Nhận xét |
|---|---|---|---|
| Học viên yêu cầu | `StudentId` | Guid, lookup `academic.Students` | Cross-service, filter |
| Mẫu đơn | `FormTemplateId` | FK → `FormTemplates` | Có label, chưa filter |
| Cán bộ duyệt | `ApprovalName` | string | Có `ApprovalId` song song nhưng không lookup |
| Trạng thái | `Status` | int → tag | Filter số |
| Ngày tạo/cập nhật | `CreationDate`, `UpdateDate` | DateTime → format | Date filter chỉ theo CreationDate |
| Ghi chú | `Note` | string | Search/list |

`ApprovalId` có trong API/DB nhưng list không dùng; detail chỉ thành “Linked record”; `IsDeleted` kỹ thuật; `Id` PK. `FormTemplate.DocumentUrl` không được composition vào request DTO; chỉ detail template riêng mới có.

**Bằng chứng:** `FormRequestsPage`; `FormRequestQueryService.cs`; `FormRequestListItemDto.cs`; `FormRequest.cs`; `CommunicationDbContext.cs`.

## Phần D — Ma trận bao phủ 36 bảng database

“Có trên UI” gồm cả bảng được gọi làm lookup; không đồng nghĩa có trang quản lý riêng.

| STT | Schema | Bảng | Entity | API | Trên UI | Trang sử dụng | Bao phủ | Ghi chú |
|---:|---|---|---|---|---|---|---|---|
| 1 | academic | AcademicYears | Có | Có | Có | Students lookup | Partial | Chỉ Id/Name lookup |
| 2 | academic | Attendances | Có | Có | Có | Attendances | Partial | List/detail đọc |
| 3 | academic | EvaluationCriterias | Có | Có | Không | — | API only | Chưa nối câu hỏi/kết quả trên UI |
| 4 | academic | Faculties | Có | Có | Có | Subjects lookup | Partial | Lookup, không trang riêng |
| 5 | academic | Majors | Có | Có | Có | Students lookup | Partial | Lookup, không filter |
| 6 | academic | Rooms | Có | Có | Có | Teachings/Schedules lookup | Partial | Lookup |
| 7 | academic | SemesterPlans | Có | Không | Không | — | Database only | Mắt xích ngành–năm–học kỳ chưa có API |
| 8 | academic | SemesterSubjects | Có | Không | Không | — | Database only | Mắt xích chương trình/kế hoạch–môn |
| 9 | academic | SemesterTuitions | Có | Có | Không | — | API only | Không thuộc 10 UI hiện tại |
| 10 | academic | Students | Có | Có | Có | Students và nhiều lookup | Partial | DTO overfetch, thiếu join User |
| 11 | academic | StudentEvaluations | Có | Có | Không | — | API only | Có điểm/criteria tiềm năng nhưng rời UI |
| 12 | academic | StudentEvaluationDetails | Có | Không | Không | — | Database only | Chi tiết tiêu chí chưa API |
| 13 | academic | Subjects | Có | Có | Có | Subjects/Teachings lookup | Partial | Trang danh mục |
| 14 | academic | SubjectDocuments | Có | Không | Không | — | Database only | Không API/UI |
| 15 | academic | SubjectSchedules | Có | Có | Có | Subject Schedules | Partial | Teacher chưa có label |
| 16 | academic | SubjectSpecialNotes | Có | Không | Không | — | Database only | Không API/UI |
| 17 | academic | SubjectStudents | Có | Có | Có | Subject Students | Full | Bảng nối chỉ có 2 FK nghiệp vụ |
| 18 | academic | SubjectTeachings | Có | Có | Có | Subject Teachings và lookup | Partial | Chưa teacher/semester context |
| 19 | academic | SubjectTeachingTeachers | Có | Không | Không | — | Database only | Thiếu API nối giảng viên–lớp |
| 20 | academic | TeacherFaculties | Có | Không | Không | — | Database only | Không có trang/API giảng viên |
| 21 | exam | ExamAttempts | Có | Có | Không | — | API only | ID chỉ xuất hiện trong ExamResult, không lookup data |
| 22 | exam | ExamQuestionAnswers | Có | Có | Không | — | API only | Snapshot đáp án lần thi |
| 23 | exam | ExamQuestionSelections | Có | Có | Không | — | API only | Snapshot câu hỏi lần thi |
| 24 | exam | ExamResults | Có | Có | Có | Exam Results | Partial | Không criteria/CLO/PLO |
| 25 | exam | Questions | Có | Có | Có | Questions | Partial | List/detail cơ bản |
| 26 | exam | QuestionAnswers | Có | Có | Không | — | API only | Không composition vào Question preview |
| 27 | exam | QuestionSuites | Có | Có | Có | Questions lookup | Partial | Chỉ Id/Name lookup |
| 28 | exam | SubjectTeachingExams | Có | Có | Có | Exam Results lookup | Partial | Chỉ Id/Name lookup |
| 29 | identity | AuditLogs | Có | Có | Không | — | API only | Technical/admin, chưa UI |
| 30 | identity | PasswordResets | Có | Có | Không | — | API only | Nhạy cảm/technical, không cần list nghiệp vụ chung |
| 31 | identity | Settings | Có | Có | Không | — | API only | Admin/technical |
| 32 | identity | Users | Có | Có | Có | Users, Students lookup | Partial | Password không rò DTO |
| 33 | identity | UserDevices | Có | Có | Không | — | API only | Technical/sensitive |
| 34 | communication | FormRequests | Có | Có | Có | Form Requests | Partial | Có list/detail |
| 35 | communication | FormTemplates | Có | Có | Có | Form Requests lookup | Partial | Chỉ lookup trên UI |
| 36 | communication | UserAnnouncements | Có | Có | Không | — | API only | Không UI |

Lưu ý: bốn class `AcademicFaculty`, `CommunicationFormTemplate`, `ExamQuestionSuite`, `IdentityUser` tồn tại trong thư mục Entities nhưng **không có DbSet/ToTable trong DbContext**, nên không được tính vào 36 bảng EF-mapped. Chúng có dấu hiệu là scaffold/template dư và cần xác minh trước khi xóa; báo cáo không coi chúng là bảng thật.

## Phần E — Cột/quan hệ còn thiếu theo mục đích sử dụng

### E.1. Nên bổ sung vào danh sách (qua DTO đã join/label, không phải GUID thô)

| Trang | Field/nhãn cần có | Hiện thiếu ở đâu |
|---|---|---|
| Students | Họ tên, mã tài khoản/học viên chuẩn | API Student chưa composition `identity.Users` |
| Subject Teachings | Khoa/Bộ môn đúng nguồn; giảng viên chính; kỳ/năm học nếu xác lập được | FE đang dùng `facultyId` không tồn tại; API thiếu teacher/semester |
| Subject Schedules | Tên giảng viên; mô tả buổi học gắn lớp/môn | API chỉ trả TeacherId; cross-service identity chưa composition |
| Attendances | Nhãn buổi học (lớp–thời gian–phòng); người ghi nhận | API chỉ ID; UI thiếu lookup |
| Exam Results | Tên môn/lớp, tên lần thi | API chỉ ID và kết quả |
| Questions | Tên môn/code môn; tóm tắt nội dung có clamp; preview ảnh | API chỉ QuestionSuiteId, UI không đi tiếp Subject |
| Form Requests | Mã/tên học viên rõ; mã người duyệt hoặc label thống nhất | Lookup student chỉ nickname; ApprovalId không resolve |

### E.2. Chỉ nên bổ sung vào detail

- Students: thông tin cá nhân, quê quán, liên hệ, gia đình, đoàn/đảng, issue description; cần phân quyền và masking. Không đưa lên list.
- Subjects: `Note` và tài liệu môn (`SubjectDocuments`) nếu có API.
- Subject Teachings: danh sách giáo viên, lịch, sinh viên, đánh giá liên quan dưới dạng liên kết/tóm tắt.
- Schedules/Attendances: `Note`, người tạo, audit ngắn.
- Exam Results: `ExamResultDetail`, attempt timeline, breakdown criteria nếu dữ liệu được chuẩn hóa.
- Questions: AnswerText/Image/IsAnswer từ `QuestionAnswers`, metadata suite/subject, lịch sử creator/update nếu có.
- Users: Identification fields/ProfilePic/LastEnforceAnnouncementRead theo phân quyền; tuyệt đối không trả hash/salt.
- Form Requests: template DocumentUrl, approver identity và lịch sử trạng thái nếu hệ thống có.

### E.3. Nên bổ sung vào filter/search

| Nhóm nghiệp vụ | Filter/search còn thiếu |
|---|---|
| Ngành/chương trình | Major, Faculty, AcademicYear, SemesterPlan, Semester; hiện `ResourceQueryFilters` không có các field này |
| Môn/lớp | SubjectId, SubjectTeachingId xuyên các trang; teacher, room, schedule type |
| Sinh viên | FullName/UserInternalId từ Identity; AcademicYear, Major, StudyStatus, IsGraduated, HasIssue |
| Giảng viên | User/TeacherFaculty/Faculty, lớp phụ trách; hiện chưa có trang/controller TeacherFaculties |
| Thi/kết quả | Subject, class, exam type, date, score range/pass-fail, attempt |
| Câu hỏi | QuestionSuiteId, SubjectId, Level; Chapter/Topic/CLO/PLO/Skill/Type/Status chưa có mô hình dữ liệu rõ |
| Biểu mẫu | FormTemplateId, ApprovalId, status dạng enum label, UpdateDate range |

## Phần F — Đánh giá luồng tra cứu nghiệp vụ

| Luồng | Kết luận | Bằng chứng/khoảng trống |
|---|---|---|
| Ngành → chương trình → môn → lớp → SV → kết quả | **Dữ liệu có một phần, UI chưa liên kết** | Major → SemesterPlan → SemesterSubject → Subject có ở DB; hai bảng giữa không API/UI. Students/Teachings/Results là các trang rời, không drill-down |
| Môn → lớp HP → GV → SV → đánh giá → điểm → CLO/kỹ năng | **Hỗ trợ một phần** | Subject → Teaching → Student có DB/UI rời; Teacher join table không API; Evaluation API-only; CLO/PLO/skill không có bảng/field rõ |
| Lớp → ngành → khóa → SV → môn → GV | **Hỗ trợ một phần** | SubjectTeaching nối Subject, Subject nối Faculty; không nối trực tiếp Major/semester plan; teacher join và student list không được composition |
| SV → lớp → ngành → môn đã học → điểm → CLO/PLO → kỹ năng | **Dữ liệu có nhưng UI chưa liên kết; đoạn CLO/PLO chưa có dữ liệu** | Students, SubjectStudents, Results tách trang; filter chỉ từng ID; không có navigation; CLO/PLO/skills vắng schema |
| GV → môn → lớp → SV → câu hỏi/đánh giá đã tạo | **API/database chưa hỗ trợ đủ** | TeacherFaculties và SubjectTeachingTeachers không controller; Question chỉ có suite/level/text, suite có UpdatedById nhưng không có CreatedBy/teacher relation chắc chắn |

Kết luận chung: UI đang tổ chức **theo bảng dữ liệu/resource**, không theo hành trình nghiệp vụ. Nó phù hợp để xem và đối chiếu từng danh sách, nhưng chưa đủ cho tra cứu tổng hợp theo ngành, môn, lớp, sinh viên hoặc giảng viên.

## Phần G — Vấn đề phát hiện và mức ưu tiên

| Mức | Trang/file/API/DB | Bằng chứng code | Ảnh hưởng thực tế | Ưu tiên |
|---|---|---|---|---|
| Critical | Toàn hệ thống; `ResourceQueryFilters.cs`; `SemesterPlans`, `SemesterSubjects`, `TeacherFaculties`, `SubjectTeachingTeachers` | Filter model chỉ có 8 field chung; 7 bảng academic không controller | Không hoàn thành được các chuỗi tra cứu ngành/chương trình/GV; người dùng phải dò từng GUID/trang | P0 |
| High | Subject Teachings; `academic/pages.tsx`; DTO/entity/query | UI đọc `facultyId` nhưng `SubjectTeachingListItemDto` và entity không có | Cột Khoa/Bộ môn trống, nguồn dữ liệu không xác định trong response | P0 |
| High | Students; placeholder/`StudentQueryService`/`Students` + `identity.Users` | UI nói tìm tên/mã nhưng query không join User.FullName/UserInternalId | Search trả kết quả sai kỳ vọng nghiệp vụ | P0 |
| High | Question Bank; `QuestionQueryService`, `QuestionDetailDto`, `QuestionAnswers` | Không filter; detail không trả answers; không truncate content | Không phân loại/preview được câu hỏi; khó dùng khi ngân hàng lớn | P0 |
| High | Toàn hệ thống lookup; `DataTablePage.tsx:151-156`, `httpClient.ts:30-33` | Mỗi relation gọi `/lookup` trả toàn bộ list | 9 trang có nguy cơ payload/memory lớn dù list chính đã phân trang | P1 |
| High | Cross-service Student/Teacher/Exam/FormRequest | Nhiều GUID ngoài DbContext: UserId, StudentId, SubjectId, SubjectTeachingId, ApprovalId | Không có FK DB/EF hoặc composition đảm bảo label/tính toàn vẹn; UI hay chỉ hiện “Linked record” | P1 |
| High | Luồng CLO/PLO/skill | Không có entity/column tương ứng trong 36 bảng | Không thể truy vết kết quả/câu hỏi tới chuẩn đầu ra/năng lực | P1, cần xác nhận phạm vi dữ liệu |
| Medium | Students DTO | `StudentListItemDto` chứa gần như toàn bộ entity, table chỉ dùng 8 field | Overfetch dữ liệu cá nhân; tăng rủi ro lộ dữ liệu và payload | P1 |
| Medium | Attendances/Schedules/Exam Results | DTO chỉ ID, UI thiếu lookup tương ứng | Mất ngữ cảnh lớp–môn–giảng viên–buổi học | P1 |
| Medium | 10 trang | Router + các `pages.tsx` không có link master-detail | Không đi từ tổng quan đến chi tiết liên quan; detail chỉ là dump field | P1 |
| Medium | Filter status | `InputNumber` dùng chung trong `DataTablePage` | Người dùng phải biết mã enum, dễ nhập sai | P2 |
| Medium | Filter state | Toàn bộ state trong component, router không đọc query string | Reload/back/share link làm mất điều kiện tra cứu | P2 |
| Medium | Question content/image | table CSS wrap/overflow visible; imageUrl là text | Row rất cao, khó scan; không preview media | P2 |
| Medium | DTO list/detail nhiều resource | Nhiều `ListItemDto` và `DetailDto` có field gần như giống nhau | List overfetch, detail thiếu dữ liệu quan hệ thay vì thực sự chuyên biệt | P2 |
| Low | Responsive | Table cuộn ngang; drawer width 640; không mobile view | Dùng được một phần trên màn hình nhỏ nhưng thao tác khó | P3 |
| Low | Bốn entity không map | `AcademicFaculty`, `CommunicationFormTemplate`, `ExamQuestionSuite`, `IdentityUser` | Gây nhiễu source/model và dễ bị hiểu nhầm là bảng thật | P3, xác minh trước khi dọn |

## Phần H — Đối chiếu database theo loại cột

| Loại | Ví dụ đã kiểm tra | Quyết định hiển thị |
|---|---|---|
| PK | `Id` của mọi bảng | Không cần list; dùng routing/action nội bộ |
| FK nội service | Student.MajorId, Subject.FacultyId, Schedule.RoomId | Nên đổi thành label; filter nếu có giá trị nghiệp vụ |
| GUID cross-service | Student.UserId, ExamResult.StudentId, QuestionSuite.SubjectId, FormRequest.StudentId | Cần API composition/lookup có kiểm soát; không coi là FK EF đã bảo đảm |
| Audit/kỹ thuật | `IsDeleted`, Creation/Update dates | IsDeleted không hiển thị; ngày chỉ hiện khi có ý nghĩa nghiệp vụ/detail |
| Nhạy cảm | User.PasswordHash/Salt, identification/mobile; địa chỉ/thân nhân Student; device PushId | Không trả/list trực tiếp; detail phải phân quyền/mask |
| Dữ liệu mô tả | Note, Description, Detail | Detail/search; chỉ list nếu cần scan và có truncate |
| Enum số | Status, Role, Type, Level, Gender | Hiện label/tag; filter bằng select, không input số |
| Có thể dư/chưa xác minh | bốn entity không map; các field name snapshot như SemesterSubject.SubjectName/Code | Chưa đủ bằng chứng để xóa; cần kiểm tra migration/data thực tế |

## Kết luận và thứ tự ưu tiên

1. **Ưu tiên đầu:** sửa hợp đồng dữ liệu/hiện trạng của Subject Teachings và Students; đây là hai lỗi làm cột/search sai ngay trên UI hiện hữu.
2. **Tiếp theo:** Question Bank — thêm khả năng truy vấn theo suite/subject/level và endpoint detail composition với answers; đồng thời xác nhận có cần mô hình Chapter/Topic/CLO/PLO/Skill/Type/Status hay không.
3. **Sau đó:** tạo read model/API tra cứu theo nghiệp vụ cho Subject/Class/Student/Teacher thay vì ép UI ghép nhiều lookup toàn bộ.
4. **Bổ sung mắt xích backend:** SemesterPlans/SemesterSubjects và TeacherFaculties/SubjectTeachingTeachers; đây là dữ liệu đã có trong DB nhưng chưa có API.
5. **Chuẩn hóa DTO:** List DTO chỉ trả field cần cho table và label quan hệ; Detail DTO trả ngữ cảnh có kiểm soát; lookup cần search/pagination hoặc autocomplete server.
6. **Cuối cùng:** URL-backed filters, enum selects, export theo nhu cầu, và cải thiện responsive. Đây là cải thiện sử dụng, không giải quyết được khoảng trống quan hệ dữ liệu cốt lõi.

Các API/DTO cần ưu tiên bổ sung để hỗ trợ tra cứu:

- Student search/read model có `FullName`, `UserInternalId`, `AcademicYearName`, `MajorCode/Name` và filters tương ứng.
- SubjectTeaching read model có `SubjectCode/Name`, `FacultyCode/Name`, teacher chính, semester/academic year (sau khi xác nhận quan hệ).
- Schedule/Attendance read model có class, subject, room, teacher, student labels và filters quan hệ.
- ExamResult read model có subject/class/exam/attempt/student labels, score/date filters và breakdown criteria nếu có.
- Question list/detail có Suite + Subject, Level filter, answer preview; CLO/PLO/skill chỉ bổ sung sau khi có schema/quan hệ được phê duyệt.
- Teacher API/read model dựa trên `TeacherFaculties` + `SubjectTeachingTeachers` + Identity User, thay vì chỉ trả `TeacherId`.

## Danh mục bằng chứng chính

- Frontend route: `education-system-ui/src/app/router.tsx`
- Khai báo cột/trang: `education-system-ui/src/features/{academic,exam,identity,communication}/pages.tsx`
- Hành vi table/filter/lookup/detail: `education-system-ui/src/shared/components/DataTablePage.tsx`, `DetailDrawer.tsx`
- HTTP contract: `education-system-ui/src/shared/api/httpClient.ts`, `src/shared/types/api.ts`
- Backend query contract: `src/BuildingBlocks/SharedKernel/QueryParameters.cs`, `ResourceQueryFilters.cs`, `PagedResult.cs`
- Controllers/query/DTO: `src/Services/*Service/Controllers`, `Application/Services`, `Application/DTOs`
- Database/entity relationship: bốn `Infrastructure/Persistence/*DbContext.cs` và `Entities/*.cs`

