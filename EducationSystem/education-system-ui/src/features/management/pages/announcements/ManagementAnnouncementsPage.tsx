import { useQuery } from '@tanstack/react-query'
import { BellOutlined, CheckCircleOutlined, CloseOutlined, DownloadOutlined, FileTextOutlined, LinkOutlined, SearchOutlined, SettingOutlined, TeamOutlined } from '@ant-design/icons'
import { Alert, Button, Card, DatePicker, Empty, Input, Select, Skeleton, Statistic, Table, Tag, Typography, type TableColumnsType } from 'antd'
import dayjs from 'dayjs'
import { Link, useSearchParams } from 'react-router-dom'
import { managementApi, type ManagementAnnouncement } from '../../managementApi'

const number = new Intl.NumberFormat('vi-VN')
const rawLabel = (name: string, value?: number | null) => value == null ? 'Chưa xác định' : `${name} ${value}`
const titleOf = (message: string) => message.split(/\r?\n|[.!?]\s/)[0].trim().slice(0, 100) || 'Thông báo không có tiêu đề'

export function ManagementAnnouncementsPage() {
  const [params, setParams] = useSearchParams()
  const page = Number(params.get('page') ?? 1); const pageSize = Number(params.get('pageSize') ?? 10)
  const search = params.get('search') ?? ''; const type = params.get('type') ?? ''; const notificationType = params.get('notificationType') ?? ''; const status = params.get('status') ?? ''; const enforceRead = params.get('enforceRead') ?? ''; const recipient = params.get('recipient') ?? ''; const from = params.get('from') ?? ''; const to = params.get('to') ?? ''
  const queryParams = { pageNumber: page, pageSize, search: search || undefined, type: type || undefined, notificationType: notificationType || undefined, status: status || undefined, enforceRead: enforceRead || undefined, recipient: recipient || undefined, fromDate: from || undefined, toDate: to || undefined }
  const announcements = useQuery({ queryKey: ['management', 'announcements', queryParams], queryFn: () => managementApi.announcementPage(queryParams) })
  const selectedId = params.get('id') ?? announcements.data?.items[0]?.announcementId
  const detail = useQuery({ queryKey: ['management', 'announcement', selectedId], queryFn: () => managementApi.announcementDetail(selectedId!), enabled: !!selectedId })
  const update = (values: Record<string, string | undefined>) => { const next = new URLSearchParams(params); Object.entries(values).forEach(([key, value]) => value ? next.set(key, value) : next.delete(key)); if (!Object.hasOwn(values, 'page')) next.set('page', '1'); setParams(next) }
  if (announcements.isLoading) return <Skeleton active paragraph={{ rows: 15 }} />
  if (announcements.isError || !announcements.data) return <Alert type="error" showIcon title="Không thể tải danh sách thông báo" />
  const types = [...new Set(announcements.data.items.map(item => item.rawType))]
  const notificationTypes = [...new Set(announcements.data.items.map(item => item.rawNotificationType).filter(value => value != null))]
  const statuses = Object.entries(announcements.data.statusCounts).sort(([a], [b]) => Number(a) - Number(b))
  const columns: TableColumnsType<ManagementAnnouncement> = [
    { title: 'Tiêu đề / Nội dung xem trước', width: 310, render: (_, item) => <div className="announcement-preview"><b>{titleOf(item.message)}</b><Typography.Paragraph ellipsis={{ rows: 2 }}>{item.message}</Typography.Paragraph></div> },
    { title: 'Loại', width: 95, render: (_, item) => <Tag color="purple">{rawLabel('Loại', item.rawType)}</Tag> },
    { title: 'Loại thông báo', width: 115, render: (_, item) => <Tag color="blue">{rawLabel('Mã', item.rawNotificationType)}</Tag> },
    { title: 'Đối tượng nhận', dataIndex: 'recipientSummary', width: 130 },
    { title: 'Trạng thái', width: 105, render: (_, item) => <Tag color="green">{rawLabel('Trạng thái', item.rawStatus)}</Tag> },
    { title: 'Bắt buộc đọc', width: 90, render: (_, item) => <Tag color={item.enforceRead ? 'green' : 'red'}>{item.enforceRead ? 'Có' : 'Không'}</Tag> },
    { title: 'Ngày tạo', width: 110, render: (_, item) => <>{dayjs(item.creationDate).format('DD/MM/YYYY')}<br />{dayjs(item.creationDate).format('HH:mm')}</> },
  ]
  return <div className="announcement-page">
    <div className="announcement-toolbar"><Typography.Text type="secondary">Tra cứu và theo dõi các thông báo đã tồn tại trong hệ thống.</Typography.Text><Button type="primary" icon={<DownloadOutlined />} onClick={() => exportCsv(announcements.data.items)}>Xuất CSV</Button></div>
    <div className="announcement-layout">
      <section className="announcement-main">
        <Card className="dashboard-panel announcement-filters">
          <label>Nội dung<Input allowClear value={search} prefix={<SearchOutlined />} placeholder="Tìm nội dung thông báo…" onChange={event => update({ search: event.target.value || undefined })} /></label>
          <label>Loại (Type)<Select allowClear value={type || undefined} placeholder="Tất cả" options={types.map(value => ({ value: String(value), label: rawLabel('Loại', value) }))} onChange={value => update({ type: value })} /></label>
          <label>Loại thông báo<Select allowClear value={notificationType || undefined} placeholder="Tất cả" options={notificationTypes.map(value => ({ value: String(value), label: rawLabel('Mã', value) }))} onChange={value => update({ notificationType: value })} /></label>
          <label>Trạng thái<Select allowClear value={status || undefined} placeholder="Tất cả" options={statuses.map(([value]) => ({ value, label: rawLabel('Trạng thái', Number(value)) }))} onChange={value => update({ status: value })} /></label>
          <label>Bắt buộc đọc<Select allowClear value={enforceRead || undefined} placeholder="Tất cả" options={[{ value: 'true', label: 'Có' }, { value: 'false', label: 'Không' }]} onChange={value => update({ enforceRead: value })} /></label>
          <label>Người nhận<Input allowClear value={recipient} placeholder="Mã người nhận…" onChange={event => update({ recipient: event.target.value || undefined })} /></label>
          <label>Ngày tạo<DatePicker.RangePicker value={from && to ? [dayjs(from), dayjs(to)] : null} onChange={dates => update({ from: dates?.[0]?.format('YYYY-MM-DD'), to: dates?.[1]?.format('YYYY-MM-DD') })} /></label>
          <Button icon={<CloseOutlined />} onClick={() => setParams(new URLSearchParams())}>Xóa bộ lọc</Button>
        </Card>
        <div className="announcement-stats"><AStat icon={<FileTextOutlined />} title="Tổng thông báo" value={announcements.data.totalItems} />{statuses.slice(0, 3).map(([code, count], index) => <AStat key={code} icon={index === 0 ? <CheckCircleOutlined /> : <BellOutlined />} title={rawLabel('Trạng thái', Number(code))} value={count} tone={['green', 'orange', 'purple'][index]} />)}<AStat icon={<BellOutlined />} title="Bắt buộc đọc" value={announcements.data.enforceReadCount} tone="blue" /></div>
        <Card className="dashboard-panel announcement-list" title={<>Danh sách thông báo <Tag>{number.format(announcements.data.totalItems)}</Tag></>} extra={<Button icon={<SettingOutlined />}>Cài đặt cột</Button>}><Table size="small" rowKey="announcementId" scroll={{ x: 1050 }} columns={columns} dataSource={announcements.data.items} rowClassName={item => item.announcementId === selectedId ? 'selected-announcement-row' : ''} onRow={item => ({ onClick: () => update({ id: item.announcementId, page: String(page) }) })} pagination={{ current: page, pageSize, total: announcements.data.totalItems, showSizeChanger: true, onChange: (next, size) => update({ page: String(next), pageSize: String(size) }) }} /></Card>
      </section>
      <Card className="dashboard-panel announcement-detail" title="Chi tiết thông báo" extra={<Button type="text" icon={<CloseOutlined />} onClick={() => update({ id: undefined })} />}>{detail.isLoading ? <Skeleton active /> : detail.isError || !detail.data ? <Empty description="Chọn thông báo để xem chi tiết" /> : <AnnouncementDetail item={detail.data} />}</Card>
    </div>
  </div>
}

function AStat({ icon, title, value, tone = 'navy' }: { icon: React.ReactNode; title: string; value: number; tone?: string }) { return <Card className={`announcement-stat announcement-stat-${tone}`}><span>{icon}</span><Statistic title={title} value={value} formatter={value => number.format(Number(value))} /></Card> }
function AnnouncementDetail({ item }: { item: ManagementAnnouncement }) {
  const internalLink = item.safeDeepLink ? `${item.safeDeepLink}${item.deepLinkParameter ? `?${item.deepLinkParameter}` : ''}` : null
  return <div className="announcement-detail-content"><div className="announcement-heading"><span><BellOutlined /></span><div><Typography.Title level={3}>{titleOf(item.message)}</Typography.Title><Tag color="green">{rawLabel('Trạng thái', item.rawStatus)}</Tag><p>ID: {item.announcementId}</p></div></div><div className="announcement-info-grid"><span>Loại<b>{rawLabel('Loại', item.rawType)}</b></span><span>Loại thông báo<b>{rawLabel('Mã', item.rawNotificationType)}</b></span><span>Trạng thái<b>{rawLabel('Trạng thái', item.rawStatus)}</b></span><span>Ngày tạo<b>{dayjs(item.creationDate).format('DD/MM/YYYY HH:mm')}</b></span><span>Bắt buộc đọc<b>{item.enforceRead ? 'Có' : 'Không'}</b></span><span>Đối tượng nhận<b>{item.recipientSummary}</b></span></div><Typography.Title level={5}>Đối tượng nhận</Typography.Title><div className="announcement-recipient"><TeamOutlined /><span>Danh sách người nhận đã được parse an toàn<b>{item.recipientSummary}</b></span></div><Typography.Title level={5}>Nội dung thông báo</Typography.Title><div className="announcement-message">{item.message}</div><Typography.Title level={5}>Liên kết liên quan</Typography.Title>{internalLink ? <Link className="announcement-link" to={internalLink}><LinkOutlined /> {internalLink}</Link> : <Alert type="warning" showIcon message="Deep link nguồn không thuộc route nội bộ được cho phép hoặc không có liên kết." />}<Typography.Title level={5}>Thông tin kỹ thuật</Typography.Title><div className="announcement-tech"><span>Deep link đã kiểm tra<b>{item.safeDeepLink ?? 'Không hợp lệ / không có'}</b></span><span>Entity Object ID<b>{item.entityObjectId}</b></span><span>Tham số liên kết<b>{item.deepLinkParameter ?? '—'}</b></span></div><Alert type="info" showIcon message="Danh sách không trả raw UserIds; gửi, sửa và xóa thông báo thuộc phase write-enabled riêng." /></div>
}
function exportCsv(items: ManagementAnnouncement[]) { const rows = [['ID', 'Nội dung', 'Type raw', 'Notification type raw', 'Status raw', 'Người nhận', 'Bắt buộc đọc', 'Ngày tạo'], ...items.map(item => [item.announcementId, item.message, item.rawType, item.rawNotificationType ?? '', item.rawStatus, item.recipientSummary, item.enforceRead ? 'Có' : 'Không', item.creationDate])]; const blob = new Blob([`\uFEFF${rows.map(row => row.map(value => `"${String(value).replaceAll('"', '""')}"`).join(',')).join('\n')}`], { type: 'text/csv;charset=utf-8' }); const href = URL.createObjectURL(blob); const anchor = document.createElement('a'); anchor.href = href; anchor.download = 'thong-bao.csv'; anchor.click(); URL.revokeObjectURL(href) }
