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
