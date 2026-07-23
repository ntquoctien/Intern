import { BellOutlined, BookOutlined, CalendarOutlined, FilterOutlined, InfoCircleOutlined, NotificationOutlined, RightOutlined, SearchOutlined, SafetyCertificateOutlined } from '@ant-design/icons'
import { Alert, Button, Card, Empty, Input, Select, Spin, Tag, Typography } from 'antd'
import { useQuery } from '@tanstack/react-query'
import dayjs from 'dayjs'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { studentApi } from '../../studentApi'
import type { StudentAnnouncement } from '../../types'

type AnnouncementFilter = 'all' | 'unread' | 'required'

export function StudentAnnouncementsPage() {
  const query = useQuery({ queryKey: ['student', 'announcements'], queryFn: studentApi.announcements })
  const [search, setSearch] = useState('')
  const [filter, setFilter] = useState<AnnouncementFilter>('all')
  const [sort, setSort] = useState<'newest' | 'oldest'>('newest')
  const [selectedId, setSelectedId] = useState<string>()
  if (query.isLoading) return <div className="center-state"><Spin /></div>
  if (query.isError || !query.data) return <Alert type="error" showIcon message="Không thể tải thông báo" />
  const items = [...query.data]
  const normalized = search.trim().toLocaleLowerCase('vi')
  const filtered = items.filter(item => (!normalized || item.message.toLocaleLowerCase('vi').includes(normalized)) && (filter === 'all' || filter === 'unread' && item.rawStatus === 0 || filter === 'required' && item.enforceRead)).sort((a, b) => (sort === 'newest' ? -1 : 1) * (dayjs(a.creationDate).valueOf() - dayjs(b.creationDate).valueOf()))
  const selected = items.find(item => item.announcementId === selectedId) ?? filtered[0]
  const unreadCount = items.filter(item => item.rawStatus === 0).length
  const requiredCount = items.filter(item => item.enforceRead).length

  return <div className="student-announcements-page">
    <div className="student-page-heading"><Typography.Title level={2}>Thông báo</Typography.Title><Typography.Text type="secondary">Cập nhật từ nhà trường và các đơn vị liên quan.</Typography.Text></div>
    <div className="announcements-layout">
      <Card className="announcement-inbox">
        <div className="announcement-search"><Input allowClear prefix={<SearchOutlined />} value={search} onChange={event => setSearch(event.target.value)} placeholder="Tìm kiếm tiêu đề hoặc nội dung..." /><Button icon={<FilterOutlined />}>Bộ lọc</Button></div>
        <div className="announcement-chips"><button className={filter === 'all' ? 'active' : ''} onClick={() => setFilter('all')}>Tất cả ({items.length})</button><button className={filter === 'unread' ? 'active' : ''} onClick={() => setFilter('unread')}><i /> Chưa đọc ({unreadCount})</button><button className={filter === 'required' ? 'active' : ''} onClick={() => setFilter('required')}><i className="red" /> Bắt buộc đọc ({requiredCount})</button><Select value={sort} onChange={setSort} options={[{ value: 'newest', label: 'Mới nhất' }, { value: 'oldest', label: 'Cũ nhất' }]} /></div>
        <div className="announcement-list">{filtered.length ? filtered.map((item, index) => <button className={`announcement-list-item ${selected?.announcementId === item.announcementId ? 'selected' : ''}`} key={item.announcementId} onClick={() => setSelectedId(item.announcementId)}>
          <i className={item.rawStatus === 0 ? 'unread-dot' : ''} /><span className={`announcement-type-icon tone-${['blue', 'green', 'orange', 'purple'][index % 4]}`}><NotificationOutlined /></span><span className="announcement-preview"><b>{announcementTitle(item)}</b><small>Loại {item.rawNotificationType ?? item.rawType}</small><p>{item.message}</p></span><time>{dayjs(item.creationDate).format('HH:mm')}<span>{dayjs(item.creationDate).format('DD/MM/YYYY')}</span></time>{item.enforceRead && <Tag color="red">Bắt buộc đọc</Tag>}
        </button>) : <Empty description="Không có thông báo phù hợp" />}</div>
        <div className="announcement-count">Hiển thị {filtered.length} trong tổng số {items.length} thông báo</div>
      </Card>
      <aside>{selected ? <AnnouncementDetail item={selected} /> : <Card><Empty description="Chưa có thông báo" /></Card>}</aside>
    </div>
  </div>
}

const announcementTitle = (item: StudentAnnouncement) => {
  const firstLine = item.message.split(/\r?\n/)[0].trim()
  return firstLine.length > 90 ? `${firstLine.slice(0, 87)}…` : firstLine || `Thông báo loại ${item.rawType}`
}

function AnnouncementDetail({ item }: { item: StudentAnnouncement }) {
  return <Card className="announcement-detail-card">
    <div className="announcement-detail-badges"><span><BellOutlined /></span><Tag color="blue">Loại {item.rawNotificationType ?? item.rawType}</Tag>{item.enforceRead && <Tag color="red">Bắt buộc đọc</Tag>}</div>
    <Typography.Title level={3}>{announcementTitle(item)}</Typography.Title>
    <div className="announcement-detail-meta"><span><InfoCircleOutlined /> Trạng thái raw: {item.rawStatus}</span><span><CalendarOutlined /> {dayjs(item.creationDate).format('DD/MM/YYYY HH:mm')}</span></div>
    {item.enforceRead && <Alert type="success" showIcon icon={<SafetyCertificateOutlined />} message="Đây là thông báo bắt buộc đọc. Vui lòng xem đầy đủ nội dung." />}
    <div className="announcement-message">{item.message}</div>
    {item.safeDeepLink && <section className="announcement-quick-nav"><h4>Điều hướng nhanh</h4><Link to={item.safeDeepLink}><span><BookOutlined /></span><div><b>Mở nội dung liên quan</b><small>{item.safeDeepLink}</small></div><RightOutlined /></Link></section>}
    <Alert type="info" showIcon message="Trang hiện ở chế độ chỉ đọc. Trạng thái đọc được hiển thị theo giá trị raw từ hệ thống." />
  </Card>
}
