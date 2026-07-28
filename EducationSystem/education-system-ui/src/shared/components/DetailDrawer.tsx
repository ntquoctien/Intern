import { Descriptions, Drawer, Empty, Spin, Tag, Typography } from 'antd'
import dayjs from 'dayjs'
import type { RecordItem } from '../types/api'

type DetailDrawerProps<T extends RecordItem> = {
  title: string
  open: boolean
  loading: boolean
  record?: T
  hiddenFields?: string[]
  fieldLabels?: Record<string, string>
  relationLabels?: Record<string, Map<string, string>>
  onClose: () => void
}

const defaultHiddenFields = new Set([
  'id',
  'isDeleted',
  'identificationNumber',
  'identificationDate',
  'profilePicUrl',
  'passwordHash',
  'passwordSalt',
  'token',
  'resetToken',
  'permanentAddress',
  'contactAddress',
  'fatherName',
  'fatherOccupation',
  'motherName',
  'motherOccupation',
  'spouseName',
  'spouseOccupation',
])

function isGuidLike(value: string) {
  return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(value)
}

function humanizeKey(key: string) {
  return key
    .replace(/Id$/, '')
    .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
    .replace(/^./, (char) => char.toUpperCase())
}

function renderValue(value: unknown) {
  if (value === null || value === undefined || value === '') {
    return <Typography.Text className="muted">-</Typography.Text>
  }

  if (typeof value === 'boolean') {
    return <Tag color={value ? 'green' : 'default'}>{value ? 'Yes' : 'No'}</Tag>
  }

  if (typeof value === 'string') {
    if (isGuidLike(value)) {
      return <Typography.Text className="muted">Linked record</Typography.Text>
    }

    const date = dayjs(value)
    if (date.isValid() && /\d{4}-\d{2}-\d{2}T/.test(value)) {
      return date.format('YYYY-MM-DD HH:mm')
    }

    return value.length > 80 ? <Typography.Paragraph>{value}</Typography.Paragraph> : value
  }

  return String(value)
}

export function DetailDrawer<T extends RecordItem>({
  title,
  open,
  loading,
  record,
  hiddenFields = [],
  fieldLabels = {},
  relationLabels = {},
  onClose,
}: DetailDrawerProps<T>) {
  const hidden = new Set([...defaultHiddenFields, ...hiddenFields])

  return (
    <Drawer title={title} open={open} size={640} onClose={onClose}>
      {loading ? <Spin /> : null}
      {!loading && !record ? <Empty description="No detail data" /> : null}
      {!loading && record ? (
        <Descriptions bordered column={1} size="small">
          {Object.entries(record)
            .filter(([key]) => !hidden.has(key))
            .map(([key, value]) => {
              const relationValue =
                typeof value === 'string' ? relationLabels[key]?.get(value) : undefined
              return (
                <Descriptions.Item key={key} label={fieldLabels[key] ?? humanizeKey(key)}>
                  {relationValue ?? renderValue(value)}
                </Descriptions.Item>
              )
            })}
        </Descriptions>
      ) : null}
    </Drawer>
  )
}
