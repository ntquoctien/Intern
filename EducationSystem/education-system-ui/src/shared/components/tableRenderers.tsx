import { Tag, Typography } from 'antd'
import dayjs from 'dayjs'

export function shortId(value?: unknown) {
  if (typeof value !== 'string' || value.length < 12) {
    return value ? String(value) : '-'
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
  if (!value || typeof value !== 'string') {
    return '-'
  }

  const parsed = dayjs(value)
  return parsed.isValid() ? parsed.format('YYYY-MM-DD HH:mm') : value
}

export function boolTag(value?: unknown) {
  return <Tag color={value ? 'green' : 'default'}>{value ? 'Yes' : 'No'}</Tag>
}
