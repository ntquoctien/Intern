import { BellOutlined, BookOutlined, CalendarOutlined, CreditCardOutlined, DashboardOutlined, FileDoneOutlined, FormOutlined, IdcardOutlined, LogoutOutlined, MenuUnfoldOutlined, NotificationOutlined, ReadOutlined, ScheduleOutlined, SolutionOutlined, TrophyOutlined } from '@ant-design/icons'
import { Avatar, Button, Drawer, Layout, Menu, Space, Spin, Typography } from 'antd'
import dayjs from 'dayjs'
import { useState } from 'react'
import { Navigate, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { useStudentAuth } from './studentAuth'

const menuItems = [
  { key: '/student/dashboard', icon: <DashboardOutlined />, label: 'Tổng quan' },
  { key: '/student/schedule', icon: <CalendarOutlined />, label: 'Lịch học' },
  { key: '/student/subjects', icon: <BookOutlined />, label: 'Lớp học phần' },
  { key: '/student/attendance', icon: <ScheduleOutlined />, label: 'Điểm danh' },
  { key: '/student/program', icon: <ReadOutlined />, label: 'Kế hoạch học tập' },
  { key: '/student/exam-results', icon: <TrophyOutlined />, label: 'Thi & Kết quả' },
  { key: '/student/evaluations', icon: <SolutionOutlined />, label: 'Đánh giá học tập' },
  { key: '/student/announcements', icon: <NotificationOutlined />, label: 'Thông báo' },
  { key: '/student/form-requests', icon: <FormOutlined />, label: 'Mẫu biểu & Yêu cầu' },
  { key: '/student/tuition', icon: <CreditCardOutlined />, label: 'Học phí' },
  { key: '/student/documents', icon: <FileDoneOutlined />, label: 'Tài liệu môn học' },
  { key: '/student/profile', icon: <IdcardOutlined />, label: 'Hồ sơ cá nhân' },
]

export function StudentProtectedLayout() {
  const { session, restoring, logout } = useStudentAuth()
  const location = useLocation()
  const navigate = useNavigate()
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false)
  if (restoring) return <div className="center-state"><Spin size="large" tip="Đang khôi phục phiên..." /></div>
  if (!session) return <Navigate to="/student/login" replace state={{ from: location.pathname }} />

  return <Layout className="student-shell">
    <Layout.Sider className="student-sider" breakpoint="lg" collapsedWidth={0} width={260}>
      <div className="student-brand"><span className="student-brand-emblem">TDU</span><span>CỔNG SINH VIÊN<small>ĐẠI HỌC TÂY ĐÔ</small></span></div>
      <Menu className="student-navigation" mode="inline" selectedKeys={[location.pathname]} items={menuItems} onClick={({ key }) => navigate(key)} />
      <div className="student-sider-help"><b>Bạn cần hỗ trợ?</b><span>Trung tâm trợ giúp</span></div>
    </Layout.Sider>
    <Layout>
      <Layout.Header className="student-header">
        <div className="student-header-leading"><Button className="student-mobile-menu-button" type="text" aria-label="Mở menu sinh viên" icon={<MenuUnfoldOutlined />} onClick={() => setMobileMenuOpen(true)} /><div className="student-header-welcome"><Typography.Title level={3}>Xin chào, {session.fullName} 👋</Typography.Title><Typography.Text type="secondary">Chúc bạn một ngày học tập hiệu quả!</Typography.Text></div></div>
        <Space size="large" className="student-header-actions">
          <Typography.Text className="student-current-date"><CalendarOutlined /> {dayjs().format('dddd, DD/MM/YYYY')}</Typography.Text>
          <Button type="text" aria-label="Thông báo" icon={<BellOutlined />} />
          <Space><Avatar size={42} src={session.profilePicUrl}>{session.fullName.slice(0, 1)}</Avatar><div><Typography.Text strong>{session.fullName}</Typography.Text><br /><Typography.Text type="secondary">{session.studentCode}</Typography.Text></div></Space>
          <Button type="text" title="Đăng xuất" aria-label="Đăng xuất" icon={<LogoutOutlined />} onClick={() => { logout(); navigate('/student/login', { replace: true }) }} />
        </Space>
      </Layout.Header>
      <Layout.Content className="student-content"><Outlet /></Layout.Content>
      <Drawer className="student-mobile-drawer" placement="left" width={280} open={mobileMenuOpen} onClose={() => setMobileMenuOpen(false)} destroyOnHidden title={<span className="student-drawer-title"><span className="student-brand-emblem">TDU</span>CỔNG SINH VIÊN</span>}>
        <Menu mode="inline" selectedKeys={[location.pathname]} items={menuItems} onClick={({ key }) => { navigate(key); setMobileMenuOpen(false) }} />
        <Button className="student-drawer-logout" icon={<LogoutOutlined />} onClick={() => { logout(); setMobileMenuOpen(false); navigate('/student/login', { replace: true }) }}>Đăng xuất</Button>
      </Drawer>
    </Layout>
  </Layout>
}
