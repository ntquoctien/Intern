import {
  BookOutlined,
  FileTextOutlined,
  FileProtectOutlined,
  ReadOutlined,
  SettingOutlined,
  TeamOutlined,
  UserOutlined,
} from '@ant-design/icons'
import { Menu } from 'antd'
import { useLocation, useNavigate } from 'react-router-dom'
import { NAVIGATION_LABELS } from '../shared/constants/labels'

const menuItems = [
  {
    key: 'academic',
    icon: <ReadOutlined />,
    label: NAVIGATION_LABELS.ACADEMIC,
    children: [
      { key: '/academic/students', label: NAVIGATION_LABELS.STUDENTS },
      { key: '/academic/subjects', label: NAVIGATION_LABELS.SUBJECTS },
      { key: '/academic/subject-teachings', label: NAVIGATION_LABELS.SUBJECT_TEACHINGS },
      { key: '/academic/subject-students', label: NAVIGATION_LABELS.SUBJECT_STUDENTS },
      { key: '/academic/subject-schedules', label: NAVIGATION_LABELS.SUBJECT_SCHEDULES },
      { key: '/academic/attendances', label: NAVIGATION_LABELS.ATTENDANCES },
    ],
  },
  {
    key: 'exam',
    icon: <BookOutlined />,
    label: NAVIGATION_LABELS.EXAM,
    children: [
      { key: '/exam/exam-results', label: NAVIGATION_LABELS.EXAM_RESULTS },
      { key: '/exam/questions', label: NAVIGATION_LABELS.QUESTIONS },
    ],
  },
  {
    key: 'identity',
    icon: <UserOutlined />,
    label: NAVIGATION_LABELS.IDENTITY,
    children: [{ key: '/identity/users', label: NAVIGATION_LABELS.USERS }],
  },
  {
    key: 'communication',
    icon: <TeamOutlined />,
    label: NAVIGATION_LABELS.COMMUNICATION,
    children: [{ key: '/management/forms/requests', label: NAVIGATION_LABELS.FORM_REQUESTS }],
  },
  {
    key: 'system',
    icon: <SettingOutlined />,
    label: NAVIGATION_LABELS.SYSTEM,
    children: [
      { key: '/management/system', label: NAVIGATION_LABELS.SYSTEM_OVERVIEW },
      {
        key: '/management/system/outcomes',
        icon: <FileProtectOutlined />,
        label: NAVIGATION_LABELS.OUTCOMES,
      },
    ],
  },
]

function openKey(pathname: string) {
  if (pathname.startsWith('/exam')) return 'exam'
  if (pathname.startsWith('/identity')) return 'identity'
  if (pathname.startsWith('/communication') || pathname.startsWith('/management/forms')) return 'communication'
  if (pathname.startsWith('/management/system')) return 'system'
  return 'academic'
}

type SidebarProps = {
  onNavigate?: () => void
}

export function Sidebar({ onNavigate }: SidebarProps) {
  const location = useLocation()
  const navigate = useNavigate()

  const handleNavigate = (key: string) => {
    navigate(key)
    onNavigate?.()
  }

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
        onClick={({ key }) => handleNavigate(key)}
      />
    </>
  )
}
