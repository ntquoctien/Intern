# Tài liệu hệ thống và từ điển dữ liệu

> Cập nhật theo mã nguồn hiện tại. Hệ thống dùng .NET 8, ASP.NET Core, EF Core và SQL Server. Các màn hình/API nghiệp vụ hiện triển khai theo hướng **chỉ đọc**; không nên hiểu danh sách chức năng dưới đây là quyền tạo/sửa/xóa dữ liệu.

## 1. Tổng quan kiến trúc

Hệ thống quản lý đào tạo được chia thành bốn service, mỗi service sở hữu một schema. Service chỉ nên truy cập trực tiếp các bảng trong schema của mình. Các cột `...Id` trỏ sang service khác được xem là **tham chiếu logic** (không phải khóa ngoại SQL trong `DbContext` hiện tại).

| Service / schema | Vai trò | Các bảng chính |
|---|---|---|
| IdentityService / `identity` | Tài khoản, thiết bị, nhật ký, cấu hình | `Users`, `UserDevices`, `PasswordResets`, `AuditLogs`, `Settings` |
| AcademicService / `academic` | Đào tạo, hồ sơ SV, lớp học phần, lịch, học phí, đánh giá | 20 bảng từ `Faculties` đến `StudentEvaluationDetails` |
| ExamService / `exam` | Ngân hàng câu hỏi, kỳ thi, lượt thi, kết quả | `QuestionSuites`, `Questions`, `QuestionAnswers`, `SubjectTeachingExams`, `ExamAttempts`, `ExamQuestionSelections`, `ExamQuestionAnswers`, `ExamResults` |
| CommunicationService / `communication` | Thông báo, biểu mẫu và yêu cầu | `UserAnnouncements`, `FormTemplates`, `FormRequests` |

Quy ước: `PK` là khóa chính; `FK` là khóa ngoại vật lý trong cùng schema; `Ref` là ID tham chiếu logic sang bảng/service khác; `?` là cho phép rỗng. Các trường `IsDeleted` là cờ xóa mềm. Các mã số nguyên như `Role`, `Status`, `Type`, `Method` chưa có bảng mã/enum được xác nhận trong mã nguồn, vì vậy tài liệu không gán ý nghĩa chi tiết cho từng giá trị.

## 2. Người dùng và chức năng

### Sinh viên

Sinh viên đăng nhập bằng mã sinh viên (nguồn mặc định là `Students.Nickname`), nhận JWT ngắn hạn và chỉ được đọc dữ liệu có `StudentId` đúng với claim trong token.

| Chức năng | Dữ liệu/bảng sử dụng | Nội dung hiển thị |
|---|---|---|
| Đăng nhập, phiên hiện tại | `academic.Students`, `identity.Users` | Xác thực mã SV, lấy thông tin cơ bản và phát token; không tạo phiên lưu DB. |
| Hồ sơ | `Students`, `Users`, `Majors`, `Faculties`, `AcademicYears` | Thông tin cá nhân, ngành/khoa, niên khóa. |
| Chương trình/kế hoạch | `SemesterPlans`, `SemesterSubjects`, `Subjects`, `Majors`, `AcademicYears` | Kế hoạch học kỳ và các môn thuộc kế hoạch. |
| Môn học, lớp học phần | `SubjectStudents`, `SubjectTeachings`, `Subjects`, `SubjectTeachingTeachers`, `Users` | Các lớp học phần SV đã ghi danh, môn và giảng viên. |
| Thời khóa biểu | `SubjectSchedules`, `SubjectTeachings`, `Subjects`, `Rooms`, `Users` | Buổi học, thời gian, phòng, giảng viên. |
| Điểm danh | `Attendances`, `SubjectSchedules`, `SubjectTeachings`, `Subjects` | Bản ghi điểm danh của chính SV. `Attendance.Status` được hiển thị nguyên trạng. |
| Kỳ thi và kết quả | `ExamResults`, `SubjectTeachingExams`, `SubjectTeachings`, `Subjects`, `ExamAttempts` | Điểm kết quả gốc/tổng hợp, lịch thi, lần thi; không tự suy ra đạt/rớt hay GPA. |
| Đánh giá học tập | `StudentEvaluations`, `StudentEvaluationDetails`, `EvaluationCriterias`, `SubjectTeachings`, `Users` | Phiếu đánh giá, tiêu chí, điểm và nhận xét. |
| Thông báo | `UserAnnouncements`, `Users` (tham chiếu người nhận) | Thông báo dành cho tài khoản SV. |
| Tài liệu môn học | `SubjectDocuments`, `Subjects`, `SubjectStudents`, `SubjectTeachings` | Tài liệu/tệp đính kèm cho các môn đang học. |
| Học phí | `SemesterTuitions`, `SemesterPlans`, `AcademicYears`, `Majors` | Số tiền và ngày thanh toán theo kế hoạch học kỳ. |
| Biểu mẫu/yêu cầu | `FormTemplates`, `FormRequests` | Mẫu biểu có sẵn và yêu cầu của chính SV. |

### Quản trị / nhân sự quản lý

Hiện tại cổng quản trị là các API/màn hình đọc nội bộ; xác thực và RBAC quản trị chưa nằm trong phạm vi mã nguồn. Các nhóm chức năng là:

| Nhóm chức năng | Bảng sử dụng chính |
|---|---|
| Tổng quan điều hành | Tổng hợp `Students`, `SubjectTeachings`, `SubjectSchedules`, `ExamResults`, `UserAnnouncements`, `Users` |
| Cơ cấu đào tạo | `Faculties`, `Majors`, `AcademicYears`, `SemesterPlans`, `SemesterSubjects` |
| Sinh viên, giảng viên, tài khoản | `Students`, `TeacherFaculties`, `Users`, `UserDevices`, `PasswordResets`, `AuditLogs` |
| Môn, lớp, phân công, phòng và lịch | `Subjects`, `SubjectDocuments`, `SubjectTeachings`, `SubjectStudents`, `SubjectTeachingTeachers`, `Rooms`, `SubjectSchedules` |
| Điểm danh và đánh giá SV | `Attendances`, `StudentEvaluations`, `StudentEvaluationDetails`, `EvaluationCriterias` |
| Khảo thí và ngân hàng câu hỏi | Toàn bộ schema `exam` cùng tham chiếu sang lớp học phần/môn/phòng/giảng viên |
| Truyền thông và biểu mẫu | `UserAnnouncements`, `FormTemplates`, `FormRequests` |
| Cấu hình hệ thống | `Settings`, `AuditLogs`, `UserDevices`, `PasswordResets` |

## 3. Quan hệ dữ liệu cốt lõi

```text
Faculties 1─n Majors 1─n Students
AcademicYears 1─n Students, SemesterPlans 1─n SemesterSubjects
Subjects 1─n SubjectTeachings 1─n SubjectSchedules / SubjectStudents
Students 1─n Attendances / SemesterTuitions / StudentEvaluations
QuestionSuites 1─n Questions 1─n QuestionAnswers
SubjectTeachingExams 1─n ExamAttempts 1─n ExamQuestionSelections 1─n ExamQuestionAnswers
FormTemplates 1─n FormRequests
Users 1─n UserDevices / AuditLogs (và được tham chiếu logic ở các service khác)
```

## 4. Từ điển dữ liệu

### 4.1 Schema `identity`

#### `Users` — tài khoản người dùng

| Cột (kiểu) | Mô tả |
|---|---|
| `Id` (`Guid`, PK) | Định danh tài khoản. |
| `UserName` (`string`) | Tên đăng nhập. |
| `PasswordHash` (`string`) | Mật khẩu đã băm; không trả về UI/API công khai. |
| `FullName` (`string`) | Họ tên. |
| `BirthDate`, `IdentificationDate` (`DateTime?`) | Ngày sinh, ngày cấp giấy tờ định danh. |
| `IdentificationNumber` (`string?`) | Số giấy tờ định danh. |
| `UserInternalId` (`string`) | Mã nội bộ người dùng. |
| `Mobile`, `ProfilePicUrl` (`string?`) | Số điện thoại, URL ảnh đại diện. |
| `Role` (`int`) | Mã vai trò; chưa có bảng mã được xác nhận. |
| `IsActived` (`bool`) | Trạng thái kích hoạt tài khoản. |
| `LastEnforceAnnouncementRead` (`DateTime?`) | Lần gần nhất đọc thông báo bắt buộc. |
| `IsDeleted` (`bool`) | Cờ xóa mềm. |

Quan hệ: được `UserDevices.UserId` và `AuditLogs.UserId` tham chiếu bằng FK; `Students.UserId`, `TeacherFaculties.UserId` và nhiều cột người tạo/giảng viên là Ref liên service.

#### `UserDevices` — thiết bị của tài khoản

| Cột (kiểu) | Mô tả |
|---|---|
| `Id` (`Guid`, PK) | Định danh thiết bị. |
| `UserId` (`Guid`, FK → `Users`) | Chủ sở hữu thiết bị. |
| `UserRole`, `DeviceType` (`int`) | Mã vai trò tại thiết bị và loại thiết bị. |
| `Identifier` (`string`) | Định danh thiết bị. |
| `PushId` (`string`) | Mã nhận thông báo đẩy; không hiển thị công khai. |

#### `PasswordResets` — yêu cầu đặt lại mật khẩu

| Cột (kiểu) | Mô tả |
|---|---|
| `Id` (`Guid`, PK) | Định danh yêu cầu. |
| `UserId` (`Guid`, Ref → `Users`) | Tài khoản yêu cầu đặt lại. Chưa có FK vật lý. |
| `CreationDate` (`DateTime`) | Thời điểm tạo yêu cầu. |
| `IsDeactive` (`bool`) | Yêu cầu đã vô hiệu hóa hay chưa. |

#### `AuditLogs` — nhật ký thao tác

| Cột (kiểu) | Mô tả |
|---|---|
| `Id` (`Guid`, PK) | Định danh nhật ký. |
| `Action` (`int`) | Mã hành động. |
| `Details` (`string`) | Nội dung chi tiết. |
| `RecordId` (`Guid`) | ID bản ghi bị tác động. |
| `RecordEntity` (`int?`), `RecordDesc` (`string`) | Mã loại và mô tả bản ghi. |
| `CreationDate` (`DateTime`) | Thời điểm phát sinh. |
| `UserId` (`Guid`, FK → `Users`) | Người thực hiện. |
| `IsDeleted` (`bool`) | Cờ xóa mềm. |

#### `Settings` — cấu hình hệ thống

| Cột (kiểu) | Mô tả |
|---|---|
| `Id` (`Guid`, PK) | Định danh cấu hình. |
| `Key`, `Value` (`string`) | Khóa và giá trị cấu hình. |
| `IsDeleted` (`bool`) | Cờ xóa mềm. |

### 4.2 Schema `academic`

#### `Faculties`, `Majors`, `AcademicYears`, `Rooms` — danh mục nền tảng

| Bảng | Cột (kiểu) | Mô tả |
|---|---|---|
| `Faculties` | `Id` (`Guid`, PK), `Name` (`string`), `Code` (`string`), `IsDeleted` (`bool`) | Khoa: định danh, tên, mã và cờ xóa mềm. |
| `Majors` | `Id` (`Guid`, PK), `FacultyId` (`Guid?`, FK → `Faculties`), `Name` (`string`), `Code` (`string`), `TrainingType` (`int`), `IsDeleted` (`bool`) | Ngành đào tạo; `TrainingType` là mã loại hình đào tạo. |
| `AcademicYears` | `Id` (`Guid`, PK), `Name` (`string`), `Year` (`int`), `StartDate`, `EndDate` (`DateTime`), `IsDeleted` (`bool`) | Niên/năm học và khoảng thời gian áp dụng. |
| `Rooms` | `Id` (`Guid`, PK), `Name` (`string`), `NumberOfSeats` (`int`), `IsDeleted` (`bool`) | Phòng học/thi, sức chứa và cờ xóa mềm. |

#### `Students` — hồ sơ học viên/sinh viên

| Cột (kiểu) | Mô tả |
|---|---|
| `Id` (`Guid`, PK) | Định danh sinh viên. |
| `UserId` (`Guid`, Ref → `identity.Users`) | Tài khoản của SV. |
| `AcademicYearId` (`Guid`, FK → `AcademicYears`), `MajorId` (`Guid`, FK → `Majors`) | Niên khóa và ngành. |
| `RelativeUserId` (`Guid?`, Ref → `identity.Users`) | Tài khoản người thân (nếu có). |
| `StudyStatus`, `Gender` (`int?`) | Mã trạng thái học tập và giới tính. |
| `Nickname` (`string?`) | Bí danh; mặc định được dùng làm mã SV để đăng nhập portal. |
| `PlaceOfBirth`, `Hometown` (`string?`) | Nơi sinh, quê quán. |
| `PermanentAddress`, `ContactAddress` (`string?`) | Địa chỉ thường trú, liên hệ. |
| `Ethnicity`, `Religion`, `EducationLevel` (`string?`) | Dân tộc, tôn giáo, trình độ học vấn. |
| `FatherName`, `FatherOccupation`, `MotherName`, `MotherOccupation` (`string?`) | Thông tin cha/mẹ. |
| `SpouseName`, `SpouseOccupation` (`string?`) | Thông tin vợ/chồng. |
| `PolicySubject`, `PreviousOccupation`, `PostGraduationWorkplace` (`string?`) | Đối tượng chính sách, nghề trước học, nơi làm việc sau tốt nghiệp. |
| `CommunistPartyJoinDate`, `OfficialPartyJoinDate`, `YouthUnionJoinDate` (`DateTime?`) | Mốc tham gia Đảng/Đoàn. |
| `IsGraduated` (`bool`) | Đã tốt nghiệp. |
| `HasIssue` (`bool?`), `IssueDescription` (`string`) | Có vấn đề cần lưu ý và mô tả. |
| `LibraryId` (`int?`) | Mã độc giả/thư viện, nếu có. |
| `IsDeleted` (`bool`) | Cờ xóa mềm. |

#### `SemesterPlans`, `SemesterSubjects`, `SemesterTuitions` — kế hoạch học kỳ và học phí

| Bảng | Cột (kiểu) | Mô tả |
|---|---|---|
| `SemesterPlans` | `Id` (`Guid`, PK), `AcademicYearId` (`Guid`, FK), `MajorId` (`Guid`, FK), `Semester` (`int`), `StartDate`, `EndDate` (`DateTime`), `IsActive`, `IsDeleted` (`bool`) | Kế hoạch cho một học kỳ/ngành/năm học; cờ đang áp dụng và xóa mềm. |
| `SemesterSubjects` | `Id` (`Guid`, PK), `SemesterPlanId` (`Guid`, FK), `SubjectId` (`Guid?`, FK → `Subjects`), `SubjectName`, `SubjectCode` (`string`), `CreditPoint` (`int`), `IsDeleted` (`bool`) | Môn trong kế hoạch; có lưu tên/mã snapshot, nên vẫn hiển thị khi `SubjectId` rỗng. |
| `SemesterTuitions` | `Id` (`Guid`, PK), `SemesterPlanId` (`Guid`, FK), `StudentId` (`Guid`, FK → `Students`), `Amount` (`decimal(18,2)?`), `PaidDate` (`DateTime?`), `IsDeleted` (`bool`) | Học phí SV theo kế hoạch và ngày đã thanh toán (nếu có). |

#### `Subjects`, `SubjectDocuments` — môn học và tài liệu

| Bảng | Cột (kiểu) | Mô tả |
|---|---|---|
| `Subjects` | `Id` (`Guid`, PK), `FacultyId` (`Guid?`, FK → `Faculties`), `SubjectCode`, `Name` (`string`), `CreditPoint` (`int`), `TotalHours` (`int?`), `Note` (`string`), `IsActived`, `IsDeleted` (`bool`) | Danh mục môn: khoa phụ trách, mã/tên, tín chỉ, tổng giờ, ghi chú và trạng thái. |
| `SubjectDocuments` | `Id` (`Guid`, PK), `SubjectId` (`Guid`, FK → `Subjects`), `Type` (`int`), `Name`, `Detail`, `Url` (`string`), `CreateById`, `UserId` (`Guid`, Ref → `Users`), `CreationDate`, `UpdateDate` (`DateTime`) | Tài liệu của môn: loại, nội dung/đường dẫn, người tạo/cập nhật. |

#### `SubjectTeachings`, `SubjectTeachingTeachers`, `SubjectStudents` — lớp học phần và ghi danh

| Bảng | Cột (kiểu) | Mô tả |
|---|---|---|
| `SubjectTeachings` | `Id` (`Guid`, PK), `SubjectId` (`Guid`, FK → `Subjects`), `Name` (`string`), `StartDate`, `EndDate` (`DateTime`), `TotalSessions` (`int`), `RoomIdDefault` (`Guid?`, FK → `Rooms`), `IsDeleted` (`bool`) | Lớp học phần: môn, tên, khoảng học, số buổi, phòng mặc định. |
| `SubjectTeachingTeachers` | `Id` (`Guid`, PK), `SubjectTeachingId` (`Guid`, FK), `TeacherId` (`Guid?`, Ref → `Users`), `IsMainTeacher` (`bool?`), `IsDeleted` (`bool`) | Phân công giảng viên cho lớp; cờ giảng viên chính. |
| `SubjectStudents` | `Id` (`Guid`, PK), `SubjectTeachingId` (`Guid`, FK), `StudentId` (`Guid`, FK → `Students`) | Ghi danh một SV vào một lớp học phần. |

#### `SubjectSchedules`, `Attendances`, `SubjectSpecialNotes` — lịch, chuyên cần và lưu ý

| Bảng | Cột (kiểu) | Mô tả |
|---|---|---|
| `SubjectSchedules` | `Id` (`Guid`, PK), `SubjectTeachingId` (`Guid`, FK), `RoomId` (`Guid?`, FK → `Rooms`), `TeacherId` (`Guid?`, Ref → `Users`), `StartDateTime`, `EndDateTime` (`DateTime`), `ScheduleType` (`int?`), `Note` (`string`), `IsDeleted` (`bool`) | Một buổi học; bộ `SubjectTeachingId` + thời điểm bắt đầu/kết thúc là duy nhất. |
| `Attendances` | `Id` (`Guid`, PK), `SubjectScheduleId` (`Guid`, FK), `StudentId` (`Guid`, FK), `CreatedById` (`Guid`, Ref → `Users`), `Status` (`int`), `Notes` (`string`), `CreationDate` (`DateTime`), `IsFirstTypeWarning`, `IsSecondTypeWarning` (`bool?`) | Điểm danh một SV tại một buổi; mỗi cặp SV–buổi chỉ có một bản ghi. |
| `SubjectSpecialNotes` | `Id` (`Guid`, PK), `SubjectId` (`Guid`, FK), `StudentId` (`Guid`, FK), `CreatedById` (`Guid`, Ref → `Users`), `Notes` (`string`), `Type` (`int`), `CreationDate` (`DateTime`), `IsDeleted` (`bool`) | Lưu ý riêng của SV theo môn học. |

#### `TeacherFaculties` — giảng viên thuộc khoa

| Cột (kiểu) | Mô tả |
|---|---|
| `Id` (`Guid`, PK) | Định danh phân công. |
| `UserId` (`Guid`, Ref → `identity.Users`) | Tài khoản giảng viên. |
| `FacultyId` (`Guid`, FK → `Faculties`) | Khoa công tác. |
| `IsHeadOfFaculty`, `IsDeleted` (`bool`) | Là trưởng khoa và cờ xóa mềm. |

#### `EvaluationCriterias`, `StudentEvaluations`, `StudentEvaluationDetails` — đánh giá SV

| Bảng | Cột (kiểu) | Mô tả |
|---|---|---|
| `EvaluationCriterias` | `Id` (`Guid`, PK), `Name`, `Description` (`string`), `Type` (`int`), `Score` (`decimal(18,2)`), `ParentId` (`Guid?`, FK tự tham chiếu), `QuestionId` (`Guid?`, Ref → `exam.Questions`) | Tiêu chí đánh giá; có thể tạo cấu trúc cha–con và gắn câu hỏi khảo sát. |
| `StudentEvaluations` | `Id` (`Guid`, PK), `StudentId` (`Guid`, FK), `SubjectTeachingId`, `SemesterPlanId` (`Guid?`, FK), `SubjectTeachingExamId`, `QuestionId`, `TeacherId` (`Guid?`, Ref), `TeacherName` (`string?`), `Type` (`int`), `Comment` (`string?`), `TotalScore` (`decimal(18,2)?`), `CreationDate` (`DateTime`), `UpdatedDate` (`DateTime?`), `IsDeleted` (`bool`) | Phiếu đánh giá của SV, ngữ cảnh lớp/kế hoạch/thi/giảng viên, tổng điểm và nhận xét. |
| `StudentEvaluationDetails` | `Id` (`Guid`, PK), `StudentEvaluationId` (`Guid`, FK), `EvaluationCriteriaId` (`Guid?`, FK), `EvaluationName` (`string?`), `StudentScore`, `Score` (`decimal(18,2)?`), `IsDeleted` (`bool`) | Chi tiết điểm từng tiêu chí; có lưu tên tiêu chí snapshot. |

### 4.3 Schema `exam`

#### `QuestionSuites`, `Questions`, `QuestionAnswers` — ngân hàng câu hỏi

| Bảng | Cột (kiểu) | Mô tả |
|---|---|---|
| `QuestionSuites` | `Id` (`Guid`, PK), `SubjectId` (`Guid`, Ref → `academic.Subjects`), `Name` (`string`), `UpdatedById` (`Guid?`, Ref → `identity.Users`), `CreationTime` (`DateTime`) | Bộ câu hỏi của một môn, người cập nhật và thời điểm tạo. |
| `Questions` | `Id` (`Guid`, PK), `QuestionSuiteId` (`Guid`, FK), `QuestionText` (`string`), `Level` (`int`), `ImageUrl` (`string?`) | Câu hỏi trong bộ; mã độ khó/cấp độ và ảnh minh họa. |
| `QuestionAnswers` | `Id` (`Guid`, PK), `QuestionId` (`Guid`, FK), `AnswerText` (`string`), `ImageUrl` (`string?`), `IsAnswer` (`bool`) | Phương án trả lời; `IsAnswer` đánh dấu đáp án đúng. |

#### `SubjectTeachingExams` — kỳ thi của lớp học phần

| Cột (kiểu) | Mô tả |
|---|---|
| `Id` (`Guid`, PK) | Định danh kỳ thi. |
| `SubjectTeachingId` (`Guid`, Ref → `academic.SubjectTeachings`) | Lớp học phần tổ chức thi. |
| `QuestionSuiteId` (`Guid?`, FK → `QuestionSuites`) | Bộ câu hỏi sử dụng. |
| `Name` (`string`) | Tên kỳ thi. |
| `StartDate`, `EndDate` (`DateTime`) | Khoảng thời gian thi. |
| `RoomId`, `TeacherId` (`Guid?`, Ref) | Phòng thi và giảng viên phụ trách. |
| `Notes` (`string`) | Ghi chú. |
| `Type`, `Method` (`int`, `int?`) | Mã loại kỳ thi và phương thức. |
| `Count`, `NumOfEasy`, `NumOfNormal`, `NumOfHard`, `NumOfPractice` (`int`) | Tổng số câu và phân bổ theo mức/câu thực hành. |
| `AllowNotifyStudent`, `IsDeleted` (`bool`) | Cho phép thông báo SV và cờ xóa mềm. |

#### `ExamAttempts`, `ExamQuestionSelections`, `ExamQuestionAnswers`, `ExamResults` — quá trình và kết quả thi

| Bảng | Cột (kiểu) | Mô tả |
|---|---|---|
| `ExamAttempts` | `Id` (`Guid`, PK), `StudentId` (`Guid`, Ref → `academic.Students`), `SubjectTeachingExamId` (`Guid`, FK), `DraftDate`, `SubmitDate` (`DateTime?`), `IsDeleted` (`bool`) | Một lượt làm bài; mốc lưu nháp và nộp. |
| `ExamQuestionSelections` | `Id` (`Guid`, PK), `ExamAttemptId` (`Guid`, FK), `AnswerId` (`Guid?`, Ref → `QuestionAnswers`), `QuestionText`, `ImageUrl` (`string`), `Order` (`int`), `IsFlag` (`bool`), `CreationDate` (`DateTime`), `SubmitDate` (`DateTime?`) | Câu hỏi được đưa vào lượt thi, đáp án chọn, thứ tự và cờ đánh dấu. |
| `ExamQuestionAnswers` | `Id` (`Guid`, PK), `ExamQuestionSelectionId` (`Guid`, FK), `AnswerText` (`string`), `IsAnswer` (`bool`) | Các phương án snapshot cho câu hỏi đã chọn; cờ đáp án đúng. |
| `ExamResults` | `Id` (`Guid`, PK), `SubjectTeachingExamId` (`Guid`, FK), `StudentId` (`Guid`, Ref → `academic.Students`), `ExamAttemptId` (`Guid?`, FK), `Result`, `CombinedResult` (`float?`), `Notes`, `ExamResultDesc`, `ExamResultDetail` (`string?`), `IsDeleted` (`bool`) | Kết quả thi. Chỉ hiển thị giá trị gốc/tổng hợp, không suy ra xếp loại. |

### 4.4 Schema `communication`

#### `FormTemplates` — mẫu biểu

| Cột (kiểu) | Mô tả |
|---|---|
| `Id` (`Guid`, PK) | Định danh mẫu. |
| `Name` (`string`) | Tên biểu mẫu. |
| `DocumentUrl` (`string?`) | Đường dẫn tài liệu/mẫu tải về. |
| `IsDeleted` (`bool`) | Cờ xóa mềm. |

#### `FormRequests` — yêu cầu theo biểu mẫu

| Cột (kiểu) | Mô tả |
|---|---|
| `Id` (`Guid`, PK) | Định danh yêu cầu. |
| `CreationDate`, `UpdateDate` (`DateTime`) | Thời điểm tạo/cập nhật. |
| `StudentId` (`Guid`, Ref → `academic.Students`) | Sinh viên gửi yêu cầu. |
| `FormTemplateId` (`Guid?`, FK → `FormTemplates`) | Mẫu biểu được chọn. |
| `ApprovalId` (`Guid?`, Ref → `identity.Users`), `ApprovalName` (`string`) | Người/phê duyệt được ghi nhận. |
| `Note` (`string`) | Nội dung hoặc ghi chú yêu cầu. |
| `Status` (`int`) | Mã trạng thái xử lý. |
| `IsDeleted` (`bool`) | Cờ xóa mềm. |

#### `UserAnnouncements` — thông báo người dùng

| Cột (kiểu) | Mô tả |
|---|---|
| `Id` (`Guid`, PK) | Định danh thông báo. |
| `Type` (`int`) | Mã loại thông báo. |
| `UserIds` (`string`) | Danh sách ID người nhận được lưu dạng chuỗi. |
| `Message` (`string`) | Nội dung thông báo. |
| `CreationDate` (`DateTime`) | Thời điểm tạo. |
| `Status` (`int`) | Mã trạng thái. |
| `DeepLink`, `DeepLinkParam` (`string`) | Đường dẫn sâu và tham số khi người dùng mở thông báo. |
| `EntityObjectId` (`Guid`) | ID đối tượng nghiệp vụ liên quan. |
| `EnforceRead` (`bool`) | Có bắt buộc người dùng đọc hay không. |
| `NotificationType` (`int?`) | Mã loại kênh/thông báo bổ sung. |
| `IsDeleted` (`bool`) | Cờ xóa mềm. |

## 5. Ràng buộc và lưu ý triển khai

- Các liên kết có FK được EF Core cấu hình rõ trong cùng schema; các `UserId`, `StudentId`, `TeacherId`, `SubjectId`, `RoomId` xuyên service chủ yếu là liên kết logic, cần kiểm tra dữ liệu qua API nội bộ thay vì join SQL liên schema.
- Các chỉ mục/unique đáng chú ý: một bản ghi điểm danh cho mỗi cặp `StudentId` + `SubjectScheduleId`; một buổi lịch không trùng theo `SubjectTeachingId` + thời điểm bắt đầu/kết thúc; kết quả thi được đánh chỉ mục theo `StudentId` + `SubjectTeachingExamId`.
- Dữ liệu nhạy cảm gồm `PasswordHash`, giấy tờ định danh, thông tin gia đình, `PushId`; không đưa vào danh sách hoặc API công khai nếu không thật sự cần.
- Không tự diễn giải các trường mã số hoặc tính GPA, tỷ lệ chuyên cần, đạt/rớt, công nợ… khi chưa có quy tắc nghiệp vụ và bảng mã chính thức.
