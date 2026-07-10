import type { ColumnsType } from 'antd/es/table'
import { DataTablePage } from '../../shared/components/DataTablePage'
import { dateTime, requestStatusTag, cleanStudentReference } from '../../shared/components/tableRenderers'
import type { LookupItem, RecordItem } from '../../shared/types/api'

type FormRequest = RecordItem & {
  creationDate: string
  updateDate: string
  studentId: string
  formTemplateId?: string
  approvalId?: string
  approvalName: string
  note: string
  status: number
}

const formRequestColumns: ColumnsType<FormRequest> = [
  { title: 'Học viên yêu cầu', dataIndex: 'studentId', render: cleanStudentReference },
  { title: 'Mẫu đơn', dataIndex: 'formTemplateId' },
  { title: 'Cán bộ duyệt', dataIndex: 'approvalName', sorter: true },
  { title: 'Trạng thái', dataIndex: 'status', sorter: true, render: requestStatusTag },
  { title: 'Ngày tạo', dataIndex: 'creationDate', sorter: true, render: dateTime },
  { title: 'Ngày cập nhật', dataIndex: 'updateDate', sorter: true, render: dateTime },
  { title: 'Ghi chú', dataIndex: 'note', sorter: true },
]

const studentLabel = (item: LookupItem) =>
  item.nickname ? `Student: ${String(item.nickname)}` : 'Student record'

export function FormRequestsPage() {
  return (
    <DataTablePage<FormRequest>
      title="Yêu cầu biểu mẫu"
      description="Danh sách các yêu cầu biểu mẫu"
      service="communication"
      resourcePath="form-requests"
      columns={formRequestColumns}
      relationLookups={[
        {
          field: 'studentId',
          title: 'Student',
          service: 'academic',
          resourcePath: 'students',
          filterable: true,
          getLabel: studentLabel,
        },
        {
          field: 'formTemplateId',
          title: 'Template',
          service: 'communication',
          resourcePath: 'form-templates',
        },
      ]}
      filterFields={['studentId', 'status', 'dateRange']}
      searchPlaceholder="Search approval or note"
      searchHelp="approval name, note"
    />
  )
}
