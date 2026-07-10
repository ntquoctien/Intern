import type { ColumnsType } from 'antd/es/table'
import { DataTablePage } from '../../shared/components/DataTablePage'
import { boolTag, dateOnly, maskPhone, roleTag } from '../../shared/components/tableRenderers'
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
  { title: 'Tên đăng nhập', dataIndex: 'userName', sorter: true },
  { title: 'Họ tên', dataIndex: 'fullName', sorter: true },
  { title: 'Di động', dataIndex: 'mobile', sorter: false, render: maskPhone },
  { title: 'Vai trò', dataIndex: 'role', sorter: true, render: roleTag },
  { title: 'Hoạt động', dataIndex: 'isActived', sorter: true, render: boolTag },
  { title: 'Ngày sinh', dataIndex: 'birthDate', sorter: true, render: dateOnly },
]

export function UsersPage() {
  return (
    <DataTablePage<User>
      title="Quản lý tài khoản"
      description="Danh sách tài khoản hệ thống"
      service="identity"
      resourcePath="users"
      columns={userColumns}
      hiddenDetailFields={['userInternalId', 'mobile']}
      searchPlaceholder="Search users"
      searchHelp="username, full name"
    />
  )
}
