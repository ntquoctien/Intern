import {
  BookOutlined,
  FileTextOutlined,
  ReadOutlined,
  TeamOutlined,
  UserOutlined,
} from '@ant-design/icons'
import { Menu } from 'antd'
import { useLocation, useNavigate } from 'react-router-dom'

const menuItems = [
  {
    key: 'academic',
    icon: <ReadOutlined />,
    label: 'Academic',
    children: [
      { key: '/academic/students', label: 'Students' },
      { key: '/academic/subjects', label: 'Subjects' },
      { key: '/academic/subject-teachings', label: 'Subject Teachings' },
      { key: '/academic/subject-students', label: 'Subject Students' },
      { key: '/academic/subject-schedules', label: 'Subject Schedules' },
      { key: '/academic/attendances', label: 'Attendances' },
    ],
  },
  {
    key: 'exam',
    icon: <BookOutlined />,
    label: 'Exam',
    children: [
      { key: '/exam/exam-results', label: 'Exam Results' },
      { key: '/exam/questions', label: 'Questions' },
    ],
  },
  {
    key: 'identity',
    icon: <UserOutlined />,
    label: 'Identity',
    children: [{ key: '/identity/users', label: 'Users' }],
  },
  {
    key: 'communication',
    icon: <TeamOutlined />,
    label: 'Communication',
    children: [{ key: '/communication/form-requests', label: 'Form Requests' }],
  },
]

function openKey(pathname: string) {
  if (pathname.startsWith('/exam')) return 'exam'
  if (pathname.startsWith('/identity')) return 'identity'
  if (pathname.startsWith('/communication')) return 'communication'
  return 'academic'
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
