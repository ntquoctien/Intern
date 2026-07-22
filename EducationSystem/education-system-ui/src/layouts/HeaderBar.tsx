import { BellOutlined, MenuOutlined, ReloadOutlined, UserOutlined } from '@ant-design/icons'
import { Avatar, Breadcrumb, Button, Select, Space, Typography } from 'antd'
import { useQueryClient } from '@tanstack/react-query'
import dayjs from 'dayjs'
import { useLocation } from 'react-router-dom'
import { currentManagementNavigation } from '../app/managementNavigation'

export function HeaderBar({ onOpenMenu }: { onOpenMenu?: () => void }) {
  const location = useLocation()
  const queryClient = useQueryClient()
  const current = currentManagementNavigation(location.pathname)
  const title = current.id === 'overview' ? 'Tổng quan điều hành' : current.id === 'structure' ? 'Khoa, ngành và năm học' : current.breadcrumb.at(-1)

  return <header className="management-header">
    <div className="page-identity">
      <Button className="mobile-menu-button" type="text" icon={<MenuOutlined />} onClick={onOpenMenu} aria-label="Mở menu" />
      <div><Typography.Title level={2}>{title}</Typography.Title><Breadcrumb items={current.breadcrumb.map((item) => ({ title: item }))} /></div>
    </div>
    <div className="header-controls">
      <label>Phạm vi đơn vị<Select value="university" options={[{ value: 'university', label: 'Trường Đại học Việt Nam' }]} /></label>
      <label>Khoa/Đơn vị<Select value="all" options={[{ value: 'all', label: 'Tất cả' }]} /></label>
      <div className="refresh-info"><span>Cập nhật dữ liệu</span><b>{dayjs().format('HH:mm, DD/MM/YYYY')}</b></div>
      <Button icon={<ReloadOutlined />} onClick={() => queryClient.invalidateQueries()}>Làm mới</Button>
      <BellOutlined className="header-bell" />
      <Space><Avatar icon={<UserOutlined />} /><div className="account-copy"><b>Quản trị hệ thống</b><span>Quản trị viên</span></div></Space>
    </div>
  </header>
}
