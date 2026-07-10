/**
 * Vietnamese localization labels and descriptions
 * Removed all technical backend references (AcademicService, IdentityService, etc.)
 */

// Navigation & Menu Labels
export const NAVIGATION_LABELS = {
  ACADEMIC: 'Quản lý học tập',
  STUDENTS: 'Danh sách học viên',
  SUBJECTS: 'Danh mục môn học',
  SUBJECT_TEACHINGS: 'Phân công giảng dạy',
  SUBJECT_STUDENTS: 'Danh sách lớp môn học',
  SUBJECT_SCHEDULES: 'Lịch học chi tiết',
  ATTENDANCES: 'Điểm danh học viên',
  EXAM: 'Kỳ thi & Đánh giá',
  EXAM_RESULTS: 'Kết quả thi',
  QUESTIONS: 'Ngân hàng câu hỏi',
  IDENTITY: 'Quản lý tài khoản',
  USERS: 'Quản lý người dùng',
  COMMUNICATION: 'Liên lạc & Yêu cầu',
  FORM_REQUESTS: 'Yêu cầu biểu mẫu',
}

// Page Titles & Descriptions
export const PAGE_DESCRIPTIONS = {
  STUDENTS: {
    title: 'Danh sách học viên',
    description: 'Quản lý hồ sơ và thông tin học viên trong hệ thống',
  },
  SUBJECTS: {
    title: 'Danh mục môn học',
    description: 'Danh sách toàn bộ các môn học và thông tin chi tiết',
  },
  SUBJECT_TEACHINGS: {
    title: 'Phân công giảng dạy',
    description: 'Quản lý các lớp học phần và phân công giáo viên giảng dạy',
  },
  SUBJECT_STUDENTS: {
    title: 'Danh sách lớp môn học',
    description: 'Danh sách học viên được đăng ký trong các lớp học phần',
  },
  SUBJECT_SCHEDULES: {
    title: 'Lịch học chi tiết',
    description: 'Thông tin và thời gian lịch học của các lớp học phần',
  },
  ATTENDANCES: {
    title: 'Điểm danh học viên',
    description: 'Bảng theo dõi chuyên cần và điểm danh chi tiết của học viên',
  },
  EXAM_RESULTS: {
    title: 'Kết quả thi',
    description: 'Bảng điểm và kết quả các kỳ thi của học viên',
  },
  QUESTIONS: {
    title: 'Ngân hàng câu hỏi',
    description: 'Quản lý kho câu hỏi cho các bài thi và kiểm tra',
  },
  USERS: {
    title: 'Quản lý người dùng',
    description: 'Danh bạ tài khoản người dùng trên hệ thống',
  },
  FORM_REQUESTS: {
    title: 'Yêu cầu biểu mẫu',
    description: 'Danh sách các yêu cầu gửi biểu mẫu trực tuyến',
  },
}

// Button & Action Labels
export const ACTION_LABELS = {
  SEARCH: 'Tìm kiếm',
  RESET: 'Đặt lại',
  REFRESH: 'Tải lại',
  ACTION: 'Thao tác',
  DETAIL: 'Chi tiết',
  EDIT: 'Chỉnh sửa',
  DELETE: 'Xóa',
  CREATE: 'Tạo mới',
  SAVE: 'Lưu',
  CANCEL: 'Hủy',
  BACK: 'Quay lại',
}

// Column Header Labels
export const COLUMN_LABELS = {
  // Common
  ID: 'Mã định danh',
  NAME: 'Tên',
  CODE: 'Mã',
  DESCRIPTION: 'Mô tả',
  NOTES: 'Ghi chú',
  STATUS: 'Trạng thái',
  CREATED_DATE: 'Ngày tạo',
  UPDATED_DATE: 'Ngày cập nhật',

  // Academic
  USER_ID: 'Tài khoản',
  STUDENT_ID: 'Học viên',
  SUBJECT_ID: 'Môn học',
  SUBJECT_CODE: 'Mã môn học',
  SUBJECT_NAME: 'Tên môn học',
  SUBJECT_TEACHING_ID: 'Lớp học phần',
  FACULTY_ID: 'Khoa/Bộ môn',
  CREDIT_POINT: 'Tín chỉ',
  TOTAL_HOURS: 'Giờ học',
  STUDY_STATUS: 'Trạng thái học',
  GENDER: 'Giới tính',
  GRADUATED: 'Đã tốt nghiệp',
  ISSUE: 'Có vấn đề',
  ACTIVE: 'Đang hoạt động',
  NICKNAME: 'Mã học viên',
  ACADEMIC_YEAR: 'Năm học',
  MAJOR: 'Chuyên ngành',

  // Subject Teaching
  TEACHING_NAME: 'Tên lớp học phần',
  START_DATE: 'Ngày bắt đầu',
  END_DATE: 'Ngày kết thúc',
  TOTAL_SESSIONS: 'Số buổi',
  DEFAULT_ROOM: 'Phòng mặc định',
  DEPARTMENT: 'Khoa/Bộ môn',

  // Schedule
  ROOM_ID: 'Phòng',
  TEACHER_ID: 'Giáo viên',
  START_TIME: 'Thời gian bắt đầu',
  END_TIME: 'Thời gian kết thúc',
  SCHEDULE_TYPE: 'Loại học',

  // Attendance
  ATTENDANCE_STATUS: 'Trạng thái điểm danh',
  SCHEDULE_ID: 'Buổi học',
  FIRST_WARNING: 'Cảnh cáo lần 1',
  SECOND_WARNING: 'Cảnh cáo lần 2',
  ATTENDANCE_DATE: 'Ngày điểm danh',

  // Exam
  EXAM_NAME: 'Tên kỳ thi',
  EXAM_DATE: 'Ngày thi',
  EXAM_SCORE: 'Điểm',
  EXAM_RESULT: 'Kết quả',
  QUESTION_LEVEL: 'Mức độ',
  QUESTION_CONTENT: 'Nội dung câu hỏi',

  // Identity
  USERNAME: 'Tên tài khoản',
  FULL_NAME: 'Họ và tên',
  EMAIL: 'Email',
  ROLE: 'Chức vụ',
  DATE_OF_BIRTH: 'Ngày sinh',
  IS_ACTIVE: 'Đang hoạt động',

  // Communication
  FORM_NAME: 'Tên biểu mẫu',
  FORM_TYPE: 'Loại biểu mẫu',
  SENDER: 'Người gửi',
  SENT_DATE: 'Ngày gửi',
  REQUEST_STATUS: 'Trạng thái yêu cầu',
}

// Search & Filter Help Text
export const SEARCH_HELP = {
  STUDENTS: 'Tìm kiếm theo mã hoặc tên học viên',
  SUBJECTS: 'Tìm kiếm theo mã hoặc tên môn học',
  SUBJECT_TEACHINGS: 'Tìm kiếm theo tên lớp học phần',
  SUBJECT_STUDENTS: 'Tìm kiếm theo tên hoặc mã học viên',
  SUBJECT_SCHEDULES: 'Tìm kiếm theo lớp hoặc phòng học',
  ATTENDANCES: 'Tìm kiếm theo tên hoặc mã học viên',
  EXAM_RESULTS: 'Tìm kiếm theo tên hoặc mã học viên',
  QUESTIONS: 'Tìm kiếm theo nội dung câu hỏi',
  USERS: 'Tìm kiếm theo tên tài khoản hoặc email',
  FORM_REQUESTS: 'Tìm kiếm theo loại biểu mẫu',
}

// Empty state messages
export const EMPTY_MESSAGES = {
  NO_DATA: 'Không có dữ liệu',
  NO_RESULTS: 'Không tìm thấy kết quả',
  LOADING: 'Đang tải dữ liệu...',
  ERROR: 'Có lỗi xảy ra',
}
