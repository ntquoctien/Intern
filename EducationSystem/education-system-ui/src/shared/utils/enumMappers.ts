/**
 * Enum Mappers - Maps raw numeric/string codes to user-friendly business labels
 * Used across the Education System UI for consistent data presentation
 */

/** Role enumeration mapping */
export const ROLE_MAP: Record<number, { label: string; color: string }> = {
  99: { label: 'Quản trị viên', color: 'red' },
  50: { label: 'Giáo viên', color: 'orange' },
  1: { label: 'Học viên', color: 'green' },
}

/** Study Status enumeration mapping */
export const STUDY_STATUS_MAP: Record<number, { label: string; color: string }> = {
  0: { label: 'Chưa bắt đầu', color: 'default' },
  1: { label: 'Đã hoàn thành', color: 'blue' },
  2: { label: 'Đang học', color: 'green' },
  3: { label: 'Tạm dừng', color: 'orange' },
  4: { label: 'Bỏ cuộc', color: 'red' },
}

/** Schedule Type enumeration mapping */
export const SCHEDULE_TYPE_MAP: Record<number, { label: string; color: string }> = {
  0: { label: 'Lý thuyết', color: 'blue' },
  1: { label: 'Thực hành', color: 'green' },
  2: { label: 'Seminar', color: 'orange' },
  3: { label: 'Thi', color: 'red' },
  4: { label: 'Khác', color: 'default' },
}

/** Attendance Status enumeration mapping */
export const ATTENDANCE_STATUS_MAP: Record<number, { label: string; color: string }> = {
  0: { label: 'Có mặt', color: 'green' },
  1: { label: 'Vắng', color: 'red' },
  2: { label: 'Trễ', color: 'orange' },
  3: { label: 'Xin phép', color: 'blue' },
}

/** Gender enumeration mapping */
export const GENDER_MAP: Record<number, string> = {
  0: 'Nam',
  1: 'Nữ',
  2: 'Khác',
}

/** Exam Result mapping */
export const EXAM_RESULT_MAP: Record<number, { label: string; color: string }> = {
  0: { label: 'Không đạt', color: 'red' },
  1: { label: 'Đạt', color: 'green' },
  2: { label: 'Chưa chấm', color: 'default' },
}

/** Question Level/Difficulty mapping */
export const QUESTION_LEVEL_MAP: Record<number, { label: string; color: string }> = {
  0: { label: 'Cơ bản', color: 'blue' },
  1: { label: 'Trung bình', color: 'orange' },
  2: { label: 'Nâng cao', color: 'red' },
}

/** User Role mapping (alternative naming for clarity) */
export const USER_ROLE_MAP = ROLE_MAP

/**
 * Get label from any enum map with fallback
 */
export function getEnumLabel(
  value: unknown,
  enumMap: Record<number | string, string | { label: string }>,
  fallback = 'N/A'
): string {
  if (value === null || value === undefined) return fallback

  const mapped = enumMap[value as keyof typeof enumMap]
  if (!mapped) return fallback

  return typeof mapped === 'string' ? mapped : mapped.label
}

/**
 * Get color from enum map
 */
export function getEnumColor(
  value: unknown,
  enumMap: Record<number | string, { label: string; color: string }>,
  fallback = 'default'
): string {
  if (value === null || value === undefined) return fallback

  const mapped = enumMap[value as keyof typeof enumMap]
  return mapped?.color ?? fallback
}
