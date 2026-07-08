import { Layout } from 'antd'
import { Outlet } from 'react-router-dom'
import { HeaderBar } from './HeaderBar'
import { Sidebar } from './Sidebar'

export function MainLayout() {
  return (
    <Layout className="app-shell">
      <Layout.Sider breakpoint="lg" collapsedWidth={0} width={260}>
        <Sidebar />
      </Layout.Sider>
      <Layout>
        <HeaderBar />
        <Layout.Content className="app-content">
          <Outlet />
        </Layout.Content>
      </Layout>
    </Layout>
  )
}
