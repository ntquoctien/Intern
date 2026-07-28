import { Drawer, Layout } from 'antd'
import { useState } from 'react'
import { Outlet } from 'react-router-dom'
import { HeaderBar } from './HeaderBar'
import { Sidebar } from './Sidebar'

export function MainLayout() {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false)
  return (
    <Layout className="app-shell">
      <Layout.Sider className="desktop-sidebar" width={240}>
        <Sidebar />
      </Layout.Sider>
      <Drawer className="mobile-sidebar" placement="left" size={240} open={mobileMenuOpen} onClose={() => setMobileMenuOpen(false)} styles={{ body: { padding: 0 } }}>
        <Sidebar onNavigate={() => setMobileMenuOpen(false)} />
      </Drawer>
      <Layout>
        <HeaderBar onOpenMenu={() => setMobileMenuOpen(true)} />
        <Layout.Content className="app-content">
          <Outlet />
        </Layout.Content>
      </Layout>
    </Layout>
  )
}
