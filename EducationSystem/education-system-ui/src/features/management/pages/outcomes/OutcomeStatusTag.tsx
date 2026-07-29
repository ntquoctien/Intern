import { Tag } from 'antd'
import type { ImportStatus } from './outcomeApi'

const presentation: Record<ImportStatus, { color: string; label: string }> = {
  Uploaded: { color: 'default', label: 'Đã tải lên' },
  Processing: { color: 'processing', label: 'Đang xử lý' },
  PendingReview: { color: 'blue', label: 'Chờ kiểm duyệt' },
  ValidationFailed: { color: 'error', label: 'Cần chỉnh sửa' },
  Approved: { color: 'success', label: 'Đã duyệt' },
  Rejected: { color: 'volcano', label: 'Từ chối' },
  Failed: { color: 'error', label: 'Xử lý lỗi' },
  Archived: { color: 'default', label: 'Đã lưu trữ' },
}

export function OutcomeStatusTag({ status }: { status: ImportStatus }) {
  const item = presentation[status] ?? { color: 'default', label: status }
  return <Tag color={item.color}>{item.label}</Tag>
}
