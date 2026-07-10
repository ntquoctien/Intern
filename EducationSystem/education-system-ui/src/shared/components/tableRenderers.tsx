import { Tag, Typography } from 'antd'
import {
  cleanPrefix,
  formatAttendanceStatus,
  formatCredits,
  formatDateOnly,
  formatDateTime,
  formatEmptyValue,
  formatExamResult,
  formatGender,
  formatHours,
  formatQuestionLevel,
  formatRequestStatus,
  formatRole,
  formatScheduleType,
  formatStudyStatus,
  getAttendanceColor,
  getExamResultColor,
  getQuestionLevelColor,
  getRequestStatusColor,
  getScheduleTypeColor,
  getStatusColor,
  getRoleColor,
} from '../utils/dataFormatters'

export function shortId(value?: unknown) {
  if (typeof value !== 'string' || value.length < 12) {
    return formatEmptyValue(value)
  }

  return (
    <Typography.Text className="mono" copyable={{ text: value }}>
      {value.slice(0, 8)}...
    </Typography.Text>
  )
}

export function linkedRecord(value?: unknown) {
  return value ? <Typography.Text className="muted">Linked record</Typography.Text> : '-'
}

export function maskText(value?: unknown, visibleStart = 2, visibleEnd = 2) {
  if (!value) {
    return '-'
  }

  const text = String(value)
  if (text.length <= visibleStart + visibleEnd) {
    return '*'.repeat(text.length)
  }

  return `${text.slice(0, visibleStart)}${'*'.repeat(Math.min(6, text.length - visibleStart - visibleEnd))}${text.slice(-visibleEnd)}`
}

export function maskPhone(value?: unknown) {
  return maskText(value, 3, 2)
}

export function dateTime(value?: unknown) {
  return formatDateTime(value as string)
}

export function dateOnly(value?: unknown) {
  return formatDateOnly(value as string)
}

export function boolTag(value?: unknown) {
  return <Tag color={value ? 'green' : 'default'}>{value ? 'Có' : 'Không'}</Tag>
}

// ============================================================================
// ENUM/STATUS RENDERERS WITH BADGES
// ============================================================================

/**
 * Render role badge with color coding
 */
export function roleTag(role?: number) {
  const label = formatRole(role)
  const color = getRoleColor(role)
  return label === '-' ? '-' : <Tag color={color}>{label}</Tag>
}

/**
 * Render study status badge with color coding
 */
export function statusTag(status?: number) {
  const label = formatStudyStatus(status)
  const color = getStatusColor(status)
  return label === '-' ? '-' : <Tag color={color}>{label}</Tag>
}

/**
 * Render gender label
 */
export function genderTag(gender?: number) {
  const label = formatGender(gender)
  return label === '-' ? '-' : <span>{label}</span>
}

/**
 * Render schedule type badge with color coding
 */
export function scheduleTypeTag(type?: number) {
  const label = formatScheduleType(type)
  const color = getScheduleTypeColor(type)
  return label === '-' ? '-' : <Tag color={color}>{label}</Tag>
}

/**
 * Render attendance status badge with color coding
 */
export function attendanceTag(status?: number) {
  const label = formatAttendanceStatus(status)
  const color = getAttendanceColor(status)
  return label === '-' ? '-' : <Tag color={color}>{label}</Tag>
}

/**
 * Render exam result badge with color coding
 */
export function examResultTag(result?: number) {
  const label = formatExamResult(result)
  const color = getExamResultColor(result)
  return label === '-' ? '-' : <Tag color={color}>{label}</Tag>
}

/**
 * Render question level badge with color coding
 */
export function questionLevelTag(level?: number) {
  const label = formatQuestionLevel(level)
  const color = getQuestionLevelColor(level)
  return label === '-' ? '-' : <Tag color={color}>{label}</Tag>
}

/**
 * Render request status badge with color coding
 */
export function requestStatusTag(status?: number) {
  const label = formatRequestStatus(status)
  const color = getRequestStatusColor(status)
  return label === '-' ? '-' : <Tag color={color}>{label}</Tag>
}

/**
 * Render hours with unit suffix
 */
export function hoursRenderer(hours?: number) {
  return formatHours(hours)
}

/**
 * Render credits with unit suffix
 */
export function creditsRenderer(credits?: number) {
  return formatCredits(credits)
}

/**
 * Remove prefix from student/subject references
 */
export function cleanStudentReference(value?: string) {
  return cleanPrefix(value, 'Student: ')
}

export function cleanSubjectReference(value?: string) {
  return cleanPrefix(value, 'Subject: ')
}
