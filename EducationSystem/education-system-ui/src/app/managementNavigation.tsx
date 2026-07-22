import { BankOutlined, BookOutlined, CalendarOutlined, CheckSquareOutlined, DashboardOutlined, FileTextOutlined, IdcardOutlined, ReadOutlined, SettingOutlined, TeamOutlined, UserOutlined } from '@ant-design/icons'

export type ManagementRole = 'Administrator' | 'AcademicManager' | 'Viewer'
export type ManagementNavigationItem = {
  id: string
  label: string
  path: string
  icon: React.ReactNode
  roles: ManagementRole[]
  breadcrumb: string[]
}

const allRoles: ManagementRole[] = ['Administrator', 'AcademicManager', 'Viewer']

export const managementNavigation: ManagementNavigationItem[] = [
  { id: 'overview', label: 'Dashboard', path: '/management/overview', icon: <DashboardOutlined />, roles: allRoles, breadcrumb: ['Trang chủ', 'Quản trị', 'Tổng quan điều hành'] },
  { id: 'students', label: 'Sinh viên', path: '/management/people/students', icon: <TeamOutlined />, roles: allRoles, breadcrumb: ['Trang chủ', 'Quản trị', 'Sinh viên'] },
  { id: 'structure', label: 'Khoa / Ngành', path: '/management/education/structure', icon: <BankOutlined />, roles: allRoles, breadcrumb: ['Trang chủ', 'Quản trị', 'Cấu trúc đào tạo'] },
  { id: 'subjects', label: 'Môn học', path: '/management/education/subjects', icon: <BookOutlined />, roles: allRoles, breadcrumb: ['Trang chủ', 'Quản trị', 'Môn học'] },
  { id: 'plans', label: 'Kế hoạch đào tạo', path: '/management/education/plans', icon: <CalendarOutlined />, roles: allRoles, breadcrumb: ['Trang chủ', 'Quản trị', 'Kế hoạch đào tạo'] },
  { id: 'classes', label: 'Lớp học phần', path: '/management/teaching/classes', icon: <ReadOutlined />, roles: allRoles, breadcrumb: ['Trang chủ', 'Quản trị', 'Lớp học phần'] },
  { id: 'assignments', label: 'Phân công giảng viên', path: '/management/teaching/assignments', icon: <TeamOutlined />, roles: ['Administrator', 'AcademicManager'], breadcrumb: ['Trang chủ', 'Quản trị', 'Phân công giảng viên'] },
  { id: 'teachers', label: 'Giảng viên', path: '/management/people/teachers', icon: <UserOutlined />, roles: allRoles, breadcrumb: ['Trang chủ', 'Quản trị', 'Giảng viên'] },
  { id: 'schedule', label: 'Phòng & Lịch giảng dạy', path: '/management/teaching/schedule', icon: <CalendarOutlined />, roles: allRoles, breadcrumb: ['Trang chủ', 'Quản trị', 'Phòng và lịch giảng dạy'] },
  { id: 'attendance', label: 'Điểm danh', path: '/management/teaching/attendance', icon: <CheckSquareOutlined />, roles: allRoles, breadcrumb: ['Trang chủ', 'Quản trị', 'Điểm danh'] },
  { id: 'results', label: 'Kỳ thi / Kết quả', path: '/management/assessment/results', icon: <FileTextOutlined />, roles: allRoles, breadcrumb: ['Trang chủ', 'Quản trị', 'Kỳ thi và kết quả'] },
  { id: 'forms', label: 'Biểu mẫu', path: '/management/forms/requests', icon: <FileTextOutlined />, roles: allRoles, breadcrumb: ['Trang chủ', 'Quản trị', 'Biểu mẫu'] },
  { id: 'system', label: 'Hệ thống', path: '/management/system', icon: <SettingOutlined />, roles: ['Administrator'], breadcrumb: ['Trang chủ', 'Quản trị', 'Hệ thống'] },
  { id: 'users', label: 'Tài khoản', path: '/management/people/users', icon: <IdcardOutlined />, roles: ['Administrator'], breadcrumb: ['Trang chủ', 'Quản trị', 'Tài khoản người dùng'] },
]

export function currentManagementNavigation(pathname: string) {
  return managementNavigation.find((item) => pathname === item.path) ?? managementNavigation[0]
}
