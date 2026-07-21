import { Empty, Typography } from 'antd'

type PlannedPageProps = {
  title: string
  description: string
}

export function PlannedPage({ title, description }: PlannedPageProps) {
  return (
    <section className="planned-page">
      <Typography.Title level={2}>{title}</Typography.Title>
      <Typography.Paragraph type="secondary">{description}</Typography.Paragraph>
      <Empty description="Trang đã được khởi tạo và đang chờ triển khai theo tài liệu UI." />
    </section>
  )
}
