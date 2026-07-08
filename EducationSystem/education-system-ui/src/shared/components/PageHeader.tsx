import { Typography } from 'antd'

type PageHeaderProps = {
  title: string
  description?: string
}

export function PageHeader({ title, description }: PageHeaderProps) {
  return (
    <div>
      <Typography.Title level={3} style={{ margin: 0 }}>
        {title}
      </Typography.Title>
      {description ? (
        <Typography.Text className="muted">{description}</Typography.Text>
      ) : null}
    </div>
  )
}
