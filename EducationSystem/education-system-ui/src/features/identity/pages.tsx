import type { ColumnsType } from 'antd/es/table'
import { DataTablePage } from '../../shared/components/DataTablePage'
import { boolTag, dateTime, maskPhone } from '../../shared/components/tableRenderers'
import type { RecordItem } from '../../shared/types/api'

type User = RecordItem & {
  userName: string
  fullName: string
  userInternalId: string
  mobile?: string
  role: number
  isActived: boolean
  birthDate?: string
}

const userColumns: ColumnsType<User> = [
  { title: 'Username', dataIndex: 'userName', sorter: true },
  { title: 'Full Name', dataIndex: 'fullName', sorter: true },
  { title: 'Mobile', dataIndex: 'mobile', sorter: false, render: maskPhone },
  { title: 'Role', dataIndex: 'role', sorter: true },
  { title: 'Active', dataIndex: 'isActived', sorter: true, render: boolTag },
  { title: 'Birth Date', dataIndex: 'birthDate', sorter: true, render: dateTime },
]

export function UsersPage() {
  return (
    <DataTablePage<User>
      title="Users"
      description="Read-only user directory from IdentityService"
      service="identity"
      resourcePath="users"
      columns={userColumns}
      hiddenDetailFields={['userInternalId', 'mobile']}
      searchPlaceholder="Search users"
      searchHelp="username, full name"
    />
  )
}
