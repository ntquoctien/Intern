import type { ColumnsType } from 'antd/es/table'
import { DataTablePage } from '../../shared/components/DataTablePage'
import { dateTime, shortId } from '../../shared/components/tableRenderers'
import type { RecordItem } from '../../shared/types/api'

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
  { title: 'Id', dataIndex: 'id', sorter: true, render: shortId },
  { title: 'Student', dataIndex: 'studentId', sorter: true, render: shortId },
  { title: 'Template', dataIndex: 'formTemplateId', sorter: true, render: shortId },
  { title: 'Approval', dataIndex: 'approvalName', sorter: true },
  { title: 'Status', dataIndex: 'status', sorter: true },
  { title: 'Created', dataIndex: 'creationDate', sorter: true, render: dateTime },
  { title: 'Updated', dataIndex: 'updateDate', sorter: true, render: dateTime },
  { title: 'Note', dataIndex: 'note', sorter: true },
]

export function FormRequestsPage() {
  return (
    <DataTablePage<FormRequest>
      title="Form Requests"
      description="Read-only form requests from CommunicationService"
      service="communication"
      resourcePath="form-requests"
      columns={formRequestColumns}
      filterFields={['studentId', 'status', 'dateRange']}
    />
  )
}
