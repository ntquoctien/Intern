import { CalendarOutlined, DashboardOutlined, FileTextOutlined, IdcardOutlined, LogoutOutlined, ReadOutlined, ScheduleOutlined, SolutionOutlined, UnorderedListOutlined } from '@ant-design/icons'
import { Avatar, Button, Layout, Menu, Space, Spin, Typography } from 'antd'
import { Navigate, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { useStudentAuth } from './studentAuth'

const menuItems = [
  { key: '/student/dashboard', icon: <DashboardOutlined />, label: 'Tổng quan' },
  { key: '/student/profile', icon: <IdcardOutlined />, label: 'Hồ sơ học tập' },
  { key: '/student/program', icon: <ReadOutlined />, label: 'Kế hoạch học kỳ' },
  { key: '/student/subjects', icon: <UnorderedListOutlined />, label: 'Môn học & lớp' },
  { key: '/student/schedule', icon: <CalendarOutlined />, label: 'Thời khóa biểu' },
  { key: '/student/exam-results', icon: <SolutionOutlined />, label: 'Kết quả thi thô' },
  { key: '/student/attendance', icon: <ScheduleOutlined />, label: 'Điểm danh thô' },
  { key: '/student/form-requests', icon: <FileTextOutlined />, label: 'Yêu cầu biểu mẫu' },
]

export function StudentProtectedLayout() {
  const { session, restoring, logout } = useStudentAuth()
  const location = useLocation()
  const navigate = useNavigate()
  if (restoring) return <div className="center-state"><Spin size="large" tip="Đang khôi phục phiên..." /></div>
  if (!session) return <Navigate to="/student/login" replace state={{ from: location.pathname }} />

  return <Layout className="student-shell">
    <Layout.Sider breakpoint="lg" collapsedWidth={0} width={250} theme="light">
      <div className="student-brand">Student Portal</div>
      <Menu mode="inline" selectedKeys={[location.pathname]} items={menuItems} onClick={({ key }) => navigate(key)} />
    </Layout.Sider>
    <Layout>
      <Layout.Header className="student-header">
        <Space><Avatar src={session.profilePicUrl}>{session.fullName.slice(0, 1)}</Avatar><div><Typography.Text strong>{session.fullName}</Typography.Text><br /><Typography.Text type="secondary">{session.studentCode}</Typography.Text></div></Space>
        <Button icon={<LogoutOutlined />} onClick={() => { logout(); navigate('/student/login', { replace: true }) }}>Đăng xuất</Button>
      </Layout.Header>
      <Layout.Content className="student-content"><Outlet /></Layout.Content>
    </Layout>
  </Layout>
}
