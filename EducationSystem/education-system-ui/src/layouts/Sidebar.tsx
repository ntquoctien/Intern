import { CustomerServiceOutlined, SafetyCertificateOutlined } from '@ant-design/icons'
import { Menu } from 'antd'
import { useLocation, useNavigate } from 'react-router-dom'
import { managementNavigation, type ManagementRole } from '../app/managementNavigation'

export function Sidebar({ role = 'Administrator', onNavigate }: { role?: ManagementRole; onNavigate?: () => void }) {
  const location = useLocation()
  const navigate = useNavigate()
  const items = managementNavigation.filter((item) => item.roles.includes(role)).map((item) => ({ key: item.path, icon: item.icon, label: item.label }))

  return <div className="management-sidebar">
    <div className="university-brand"><SafetyCertificateOutlined /><span>ĐẠI HỌC<br />VIỆT NAM</span></div>
    <Menu theme="dark" mode="inline" items={items} selectedKeys={[location.pathname]} onClick={({ key }) => { navigate(key); onNavigate?.() }} />
    <div className="support-card"><CustomerServiceOutlined /><div><b>Bạn cần hỗ trợ?</b><span>Trung tâm hỗ trợ</span></div></div>
  </div>
}
