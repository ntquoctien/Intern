/**
 * Data Formatters - Transforms raw API data into user-friendly display formats
 * Handles dates, strings, numbers, and enum conversions
 */

import dayjs from 'dayjs'
import {
  ATTENDANCE_STATUS_MAP,
  EXAM_RESULT_MAP,
  GENDER_MAP,
  QUESTION_LEVEL_MAP,
  ROLE_MAP,
  SCHEDULE_TYPE_MAP,
  STUDY_STATUS_MAP,
  getEnumColor,
  getEnumLabel,
} from './enumMappers'

// ============================================================================
// DATE FORMATTERS
// ============================================================================

/**
 * Format date-only string (removes time portion)
 * @param dateStr ISO date string (e.g., "2000-01-01 00:00:00" or "2000-01-01")
 * @returns Formatted date (DD/MM/YYYY) or '-' if invalid
 */
export function formatDateOnly(dateStr?: string | null): string {
  if (!dateStr) return '-'
  const parsed = dayjs(dateStr)
  return parsed.isValid() ? parsed.format('DD/MM/YYYY') : '-'
}

/**
 * Format date with time
 * @param dateStr ISO date string
 * @returns Formatted datetime (DD/MM/YYYY HH:mm) or '-' if invalid
 */
export function formatDateTime(dateStr?: string | null): string {
  if (!dateStr) return '-'
  const parsed = dayjs(dateStr)
  return parsed.isValid() ? parsed.format('DD/MM/YYYY HH:mm') : '-'
}

/**
 * Format time-only string
 * @param dateStr ISO date string
 * @returns Formatted time (HH:mm) or '-' if invalid
 */
export function formatTime(dateStr?: string | null): string {
  if (!dateStr) return '-'
  const parsed = dayjs(dateStr)
  return parsed.isValid() ? parsed.format('HH:mm') : '-'
}

// ============================================================================
// STRING FORMATTERS
// ============================================================================

/**
 * Normalize subject code to uppercase with hyphens
 * @param code Raw subject code (e.g., "ktchungoto")
 * @returns Normalized code (e.g., "KT-CHUNG-OTO") or '-' if empty
 */
export function formatSubjectCode(code?: string | null): string {
  if (!code || typeof code !== 'string') return '-'

  // Remove spaces and convert to uppercase
  const normalized = code.trim().toUpperCase()

  // Add hyphens if not already formatted (optional: customize based on rules)
  // For now, just return uppercased version
  return normalized || '-'
}

/**
 * Remove redundant prefix from value
 * @param value Original string value
 * @param prefix Prefix to remove (e.g., "Student: ")
 * @returns Value without prefix or original value if no match
 */
export function cleanPrefix(value?: string | null, prefix?: string): string {
  if (!value) return '-'

  if (prefix && value.startsWith(prefix)) {
    return value.substring(prefix.length).trim()
  }

  return value
}

/**
 * Truncate long strings with ellipsis
 * @param value String to truncate
 * @param maxLength Maximum characters before truncation
 * @returns Truncated string with "..." or original if shorter
 */
export function truncateString(value?: string | null, maxLength = 30): string {
  if (!value) return '-'

  if (value.length > maxLength) {
    return `${value.substring(0, maxLength)}...`
  }

  return value
}

// ============================================================================
// ENUM/CODE FORMATTERS
// ============================================================================

/**
 * Format user role with label
 * @param role Role code (99=Admin, 50=Teacher, 1=Student)
 * @returns User-friendly role label or '-'
 */
export function formatRole(role?: number | null): string {
  return getEnumLabel(role, ROLE_MAP, '-')
}

/**
 * Format study status with label
 * @param status Study status code
 * @returns User-friendly status label or '-'
 */
export function formatStudyStatus(status?: number | null): string {
  return getEnumLabel(status, STUDY_STATUS_MAP, '-')
}

/**
 * Format schedule type with label
 * @param type Schedule type code
 * @returns User-friendly type label or '-'
 */
export function formatScheduleType(type?: number | null): string {
  return getEnumLabel(type, SCHEDULE_TYPE_MAP, '-')
}

/**
 * Format attendance status with label
 * @param status Attendance status code
 * @returns User-friendly status label or '-'
 */
export function formatAttendanceStatus(status?: number | null): string {
  return getEnumLabel(status, ATTENDANCE_STATUS_MAP, '-')
}

/**
 * Format gender with label
 * @param gender Gender code (0=Nam, 1=Nữ, 2=Khác)
 * @returns User-friendly gender label or '-'
 */
export function formatGender(gender?: number | null): string {
  return getEnumLabel(gender, GENDER_MAP, '-')
}

/**
 * Format exam result with label
 * @param result Exam result code
 * @returns User-friendly result label or '-'
 */
export function formatExamResult(result?: number | null): string {
  return getEnumLabel(result, EXAM_RESULT_MAP, '-')
}

/**
 * Format question level/difficulty with label
 * @param level Question level code (0=Cơ bản, 1=Trung bình, 2=Nâng cao)
 * @returns User-friendly level label or '-'
 */
export function formatQuestionLevel(level?: number | null): string {
  return getEnumLabel(level, QUESTION_LEVEL_MAP, '-')
}

// ============================================================================
// COLOR GETTERS FOR BADGES/TAGS
// ============================================================================

/**
 * Get color for role badge
 */
export function getRoleColor(role?: number | null): string {
  return getEnumColor(role, ROLE_MAP, 'default')
}

/**
 * Get color for status badge
 */
export function getStatusColor(status?: number | null): string {
  return getEnumColor(status, STUDY_STATUS_MAP, 'default')
}

/**
 * Get color for schedule type badge
 */
export function getScheduleTypeColor(type?: number | null): string {
  return getEnumColor(type, SCHEDULE_TYPE_MAP, 'default')
}

/**
 * Get color for attendance badge
 */
export function getAttendanceColor(status?: number | null): string {
  return getEnumColor(status, ATTENDANCE_STATUS_MAP, 'default')
}

/**
 * Get color for exam result badge
 */
export function getExamResultColor(result?: number | null): string {
  return getEnumColor(result, EXAM_RESULT_MAP, 'default')
}

/**
 * Get color for question level badge
 */
export function getQuestionLevelColor(level?: number | null): string {
  return getEnumColor(level, QUESTION_LEVEL_MAP, 'default')
}

// ============================================================================
// NUMBER FORMATTERS
// ============================================================================

/**
 * Format large numbers with thousand separators
 * @param value Number to format
 * @returns Formatted number (e.g., "1,234.56")
 */
export function formatNumber(value?: number | null): string {
  if (value === null || value === undefined) return '-'

  return new Intl.NumberFormat('vi-VN').format(value)
}

/**
 * Format as percentage
 * @param value Number between 0-100
 * @returns Formatted percentage string
 */
export function formatPercent(value?: number | null): string {
  if (value === null || value === undefined) return '-'

  return new Intl.NumberFormat('vi-VN', {
    style: 'percent',
    minimumFractionDigits: 1,
    maximumFractionDigits: 1,
  }).format(value / 100)
}

/**
 * Format decimal number with fixed precision
 * @param value Number to format
 * @param decimals Number of decimal places
 * @returns Formatted number
 */
export function formatDecimal(value?: number | null, decimals = 2): string {
  if (value === null || value === undefined) return '-'

  return new Intl.NumberFormat('vi-VN', {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  }).format(value)
}

// ============================================================================
// EMPTY VALUE HANDLING
// ============================================================================

/**
 * Format empty/null/undefined values consistently
 * @param value Any value
 * @param emptyPlaceholder Placeholder for empty values (default: '-')
 * @returns Value if present, placeholder if empty
 */
export function formatEmptyValue(value?: unknown, emptyPlaceholder = '-'): string {
  if (value === null || value === undefined || value === '' || Number.isNaN(value)) {
    return emptyPlaceholder
  }

  if (typeof value === 'string') {
    return value.trim() || emptyPlaceholder
  }

  if (typeof value === 'number') {
    return formatNumber(value)
  }

  if (typeof value === 'boolean') {
    return value ? 'Có' : 'Không'
  }

  return String(value)
}

/**
 * Format hours with "giờ" suffix
 * @param hours Number of hours
 * @returns Formatted hours (e.g., "45 giờ") or '-'
 */
export function formatHours(hours?: number | null): string {
  if (hours === null || hours === undefined) return '-'

  return `${hours} giờ`
}

/**
 * Format credit points
 * @param credits Number of credits
 * @returns Formatted credits (e.g., "3 tín chỉ") or '-'
 */
export function formatCredits(credits?: number | null): string {
  if (credits === null || credits === undefined) return '-'

  return `${credits} tín chỉ`
}

// ============================================================================
// COMPOSITE FORMATTERS
// ============================================================================

/**
 * Format student reference by removing "Student: " prefix
 * @param value Raw student reference (e.g., "Student: SV000234")
 * @returns Cleaned value (e.g., "SV000234")
 */
export function formatStudentReference(value?: string | null): string {
  return cleanPrefix(value, 'Student: ')
}

/**
 * Format subject reference (Code - Name)
 * @param code Subject code
 * @param name Subject name
 * @returns Formatted subject reference
 */
export function formatSubjectReference(code?: string | null, name?: string | null): string {
  const formattedCode = formatSubjectCode(code)
  if (formattedCode === '-' && !name) return '-'

  if (formattedCode !== '-' && name) {
    return `${formattedCode} - ${name}`
  }

  return name || formattedCode
}

/**
 * Format request status with friendly label (used in form requests, etc.)
 */
export function formatRequestStatus(status?: number | null): string {
  switch (status) {
    case 1:
      return 'Chờ phê duyệt'
    case 2:
      return 'Đã phê duyệt'
    case 3:
      return 'Từ chối'
    case 4:
      return 'Hủy bỏ'
    default:
      return '-'
  }
}

/**
 * Get color for request status badge
 */
export function getRequestStatusColor(status?: number | null): string {
  switch (status) {
    case 1:
      return 'processing'
    case 2:
      return 'success'
    case 3:
      return 'error'
    case 4:
      return 'default'
    default:
      return 'default'
  }
}

// ============================================================================
// FACILITY/DEPARTMENT CODE MAPPING
// ============================================================================

/**
 * Map facility/department codes to friendly Vietnamese names
 * @param code Facility code (Ph, Xư, T1, etc.)
 * @returns Friendly department name or original code if not found
 */
export function formatFacilityCode(code?: string | null): string {
  if (!code || typeof code !== 'string') return '-'

  const facilityMap: Record<string, string> = {
    'Ph': 'Phun sơn',
    'Xư': 'Xưởng thực hành',
    'T1': 'Tổ bộ môn 1',
    'T2': 'Tổ bộ môn 2',
    'T3': 'Tổ bộ môn 3',
    'KT': 'Khoa Kỹ thuật',
    'QT': 'Khoa Quản trị',
    'KD': 'Khoa Kinh doanh',
  }

  return facilityMap[code.trim()] || code.trim()
}

/**
 * Fix "Linked record" placeholder to show more meaningful message
 * @param value Any value indicating linked record
 * @returns Dash or custom message
 */
export function formatLinkedRecordPlaceholder(value?: unknown): string {
  if (!value || value === 'Linked record') {
    return '-'
  }
  return String(value)
}
