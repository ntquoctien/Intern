import type { ColumnsType } from 'antd/es/table'
import { DataTablePage } from '../../shared/components/DataTablePage'
import { dateTime } from '../../shared/components/tableRenderers'
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
  { title: 'Student', dataIndex: 'studentId' },
  { title: 'Template', dataIndex: 'formTemplateId' },
  { title: 'Approval', dataIndex: 'approvalName', sorter: true },
  { title: 'Status', dataIndex: 'status', sorter: true },
  { title: 'Created', dataIndex: 'creationDate', sorter: true, render: dateTime },
  { title: 'Updated', dataIndex: 'updateDate', sorter: true, render: dateTime },
  { title: 'Note', dataIndex: 'note', sorter: true },
]

const studentLabel = (item: LookupItem) =>
  item.nickname ? `Student: ${String(item.nickname)}` : 'Student record'

export function FormRequestsPage() {
  return (
    <DataTablePage<FormRequest>
      title="Form Requests"
      description="Read-only form requests from CommunicationService"
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
