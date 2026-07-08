import { Typography } from 'antd'

export function HeaderBar() {
  return (
    <header className="app-header">
      <Typography.Text strong>Read-only Education System</Typography.Text>
      <Typography.Text className="muted">Direct service mode</Typography.Text>
    </header>
  )
}
