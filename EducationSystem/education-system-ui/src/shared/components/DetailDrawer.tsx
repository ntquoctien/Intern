import { Descriptions, Drawer, Empty, Spin, Tag, Typography } from 'antd'
import dayjs from 'dayjs'
import type { RecordItem } from '../types/api'

type DetailDrawerProps<T extends RecordItem> = {
  title: string
  open: boolean
  loading: boolean
  record?: T
  onClose: () => void
}

function renderValue(value: unknown) {
  if (value === null || value === undefined || value === '') {
    return <Typography.Text className="muted">-</Typography.Text>
  }

  if (typeof value === 'boolean') {
    return <Tag color={value ? 'green' : 'default'}>{value ? 'Yes' : 'No'}</Tag>
  }

  if (typeof value === 'string') {
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
  onClose,
}: DetailDrawerProps<T>) {
  return (
    <Drawer title={title} open={open} width={640} onClose={onClose}>
      {loading ? <Spin /> : null}
      {!loading && !record ? <Empty description="No detail data" /> : null}
      {!loading && record ? (
        <Descriptions bordered column={1} size="small">
          {Object.entries(record).map(([key, value]) => (
            <Descriptions.Item key={key} label={key}>
              {renderValue(value)}
            </Descriptions.Item>
          ))}
        </Descriptions>
      ) : null}
    </Drawer>
  )
}
