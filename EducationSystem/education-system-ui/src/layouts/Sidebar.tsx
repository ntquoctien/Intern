import {
  BookOutlined,
  CalendarOutlined,
  DashboardOutlined,
  FileTextOutlined,
  ReadOutlined,
  TeamOutlined,
  UserOutlined,
} from '@ant-design/icons'
import { Menu } from 'antd'
import { useLocation, useNavigate } from 'react-router-dom'

const menuItems = [
  {
    key: 'overview',
    icon: <DashboardOutlined />,
    label: 'Tổng quan',
    children: [{ key: '/management/overview', label: 'Dashboard' }],
  },
  {
    key: 'education',
    icon: <ReadOutlined />,
    label: 'Đào tạo',
    children: [
      { key: '/management/education/structure', label: 'Khoa, ngành và năm học' },
      { key: '/management/education/plans', label: 'Kế hoạch học kỳ' },
      { key: '/management/education/subjects', label: 'Môn học' },
    ],
  },
  {
    key: 'people', icon: <UserOutlined />, label: 'Con người',
    children: [
      { key: '/management/people/students', label: 'Sinh viên' },
      { key: '/management/people/teachers', label: 'Giảng viên' },
      { key: '/management/people/users', label: 'Tài khoản nội bộ' },
    ],
  },
  {
    key: 'teaching', icon: <CalendarOutlined />, label: 'Giảng dạy',
    children: [
      { key: '/management/teaching/classes', label: 'Lớp học phần' },
      { key: '/management/teaching/assignments', label: 'Phân công/ghi danh' },
      { key: '/management/teaching/schedule', label: 'Lịch theo tuần' },
      { key: '/management/teaching/attendance', label: 'Điểm danh' },
    ],
  },
  {
    key: 'assessment', icon: <BookOutlined />, label: 'Đánh giá',
    children: [
      { key: '/management/assessment/results', label: 'Kết quả thi thô' },
      { key: '/management/assessment/question-suites', label: 'Ngân hàng câu hỏi' },
      { key: '/management/assessment/evaluations', label: 'Đánh giá sinh viên' },
    ],
  },
  {
    key: 'communication', icon: <TeamOutlined />, label: 'Truyền thông',
    children: [{ key: '/management/communication/announcements', label: 'Thông báo' }],
  },
  {
    key: 'forms', icon: <TeamOutlined />, label: 'Biểu mẫu / Liên lạc',
    children: [{ key: '/management/forms/requests', label: 'Yêu cầu biểu mẫu' }],
  },
  {
    key: 'system', icon: <FileTextOutlined />, label: 'Hệ thống',
    children: [{ key: '/management/system', label: 'Cấu hình và audit' }],
  },
]

function openKey(pathname: string) {
  if (pathname.includes('/education/')) return 'education'
  if (pathname.includes('/people/')) return 'people'
  if (pathname.includes('/teaching/')) return 'teaching'
  if (pathname.includes('/assessment/')) return 'assessment'
  if (pathname.includes('/communication/')) return 'communication'
  if (pathname.includes('/forms/')) return 'forms'
  if (pathname.includes('/system')) return 'system'
  return 'overview'
}

export function Sidebar() {
  const location = useLocation()
  const navigate = useNavigate()

  return (
    <>
      <div className="app-logo">
        <FileTextOutlined style={{ marginRight: 8 }} />
        EducationSystem
      </div>
      <Menu
        theme="dark"
        mode="inline"
        items={menuItems}
        defaultOpenKeys={[openKey(location.pathname)]}
        selectedKeys={[location.pathname]}
        onClick={({ key }) => navigate(key)}
      />
    </>
  )
}
