/**
 * Vietnamese labels and placeholders for filter components
 */

export const filterLabels = {
  studentId: 'Lọc theo học viên',
  userId: 'Lọc theo tài khoản',
  subjectTeachingId: 'Lọc theo lớp học phần',
  subjectTeachingExamId: 'Lọc theo kỳ thi',
  subjectScheduleId: 'Lọc theo lịch học',
  status: 'Chọn trạng thái',
  dateRange: 'Từ ngày → Đến ngày',
}

export const searchPlaceholders = {
  students: 'Tìm theo tên hoặc mã học viên...',
  subjects: 'Tìm theo tên hoặc mã môn học...',
  subjectTeachings: 'Tìm theo tên lớp học phần hoặc mã...',
  subjectStudents: 'Nhập mã học viên hoặc tên lớp học phần...',
  subjectSchedules: 'Tìm theo phòng học hoặc mã lớp...',
  attendances: 'Tìm theo mã học viên hoặc lớp học phần...',
  examResults: 'Tìm theo ghi chú, mô tả hoặc chi tiết kết quả...',
  questions: 'Tìm theo nội dung câu hỏi, hình ảnh URL...',
  users: 'Tìm theo tên đăng nhập hoặc họ tên...',
  formRequests: 'Tìm theo phê duyệt hoặc ghi chú...',
}

export const autocompleteSearchThresholds = {
  minChars: 2,
  debounceMs: 300,
  maxResults: 20,
}
