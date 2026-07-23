import { BookOutlined, CalendarOutlined, InfoCircleOutlined, RightOutlined, SearchOutlined, StarOutlined, UserOutlined } from '@ant-design/icons'
import { Alert, Button, Card, DatePicker, Empty, Select, Spin, Tag, Typography } from 'antd'
import { useQuery } from '@tanstack/react-query'
import dayjs from 'dayjs'
import { useState } from 'react'
import { studentApi } from '../../studentApi'
import type { StudentEvaluation } from '../../types'

export function StudentEvaluationsPage() {
  const query = useQuery({ queryKey: ['student', 'evaluations'], queryFn: studentApi.evaluations })
  const [course, setCourse] = useState<string>()
  const [teacher, setTeacher] = useState<string>()
  const [type, setType] = useState<number>()
  const [from, setFrom] = useState<string>()
  const [to, setTo] = useState<string>()
  const [selectedId, setSelectedId] = useState<string>()
  if (query.isLoading) return <div className="center-state"><Spin /></div>
  if (query.isError || !query.data) return <Alert type="error" showIcon message="Không thể tải đánh giá học tập" />
  const items = query.data
  const selected = items.find(item => item.evaluationId === selectedId) ?? items[0]
  const courses = [...new Map(items.filter(item => item.subjectTeachingId).map(item => [item.subjectTeachingId!, { value: item.subjectTeachingId!, label: `${item.subjectCode ?? 'Chưa có mã'} · ${item.subjectName ?? item.className ?? 'Chưa có tên'}` }])).values()]
  const teachers = [...new Set(items.map(item => item.teacherName).filter((value): value is string => Boolean(value)))]
  const types = [...new Set(items.map(item => item.rawType))].sort((a, b) => a - b)
  const filtered = items.filter(item => (!course || item.subjectTeachingId === course) && (!teacher || item.teacherName === teacher) && (type == null || item.rawType === type) && (!from || !dayjs(item.creationDate).isBefore(dayjs(from), 'day')) && (!to || !dayjs(item.creationDate).isAfter(dayjs(to), 'day')))
  const clear = () => { setCourse(undefined); setTeacher(undefined); setType(undefined); setFrom(undefined); setTo(undefined) }

  return <div className="student-evaluations-page">
    <div className="student-page-heading"><Typography.Title level={2}>Đánh giá học tập</Typography.Title><Typography.Text type="secondary">Xem các đánh giá và nhận xét được ghi nhận cho bạn.</Typography.Text></div>
    <Card className="evaluation-filter-card"><div className="evaluation-filters">
      <label><span>Khoảng ngày</span><DatePicker.RangePicker value={from && to ? [dayjs(from), dayjs(to)] : null} onChange={dates => { setFrom(dates?.[0]?.format('YYYY-MM-DD')); setTo(dates?.[1]?.format('YYYY-MM-DD')) }} /></label>
      <label><span>Lớp học phần</span><Select allowClear value={course} onChange={setCourse} options={courses} placeholder="Tất cả lớp học phần" /></label>
      <label><span>Giảng viên</span><Select allowClear value={teacher} onChange={setTeacher} options={teachers.map(value => ({ value, label: value }))} placeholder="Tất cả giảng viên" /></label>
      <label><span>Loại đánh giá</span><Select allowClear value={type} onChange={setType} options={types.map(value => ({ value, label: `Loại ${value}` }))} placeholder="Tất cả loại" /></label>
      <Button onClick={clear}>Xóa bộ lọc</Button><Button type="primary" icon={<SearchOutlined />}>Tìm kiếm</Button>
    </div></Card>
    <div className="evaluations-layout">
      <main><Card className="evaluation-list-card" title={`Tổng số: ${filtered.length} đánh giá`} extra="Sắp xếp: Mới nhất">
        {filtered.length ? <div className="evaluation-timeline">{filtered.map((item, index) => <button className={`evaluation-row ${selected?.evaluationId === item.evaluationId ? 'selected' : ''}`} key={item.evaluationId} onClick={() => setSelectedId(item.evaluationId)}>
          <time><b>{dayjs(item.creationDate).format('DD/MM/YYYY')}</b><span>{dayjs(item.creationDate).format('HH:mm')}</span></time><i className={`evaluation-icon tone-${['blue', 'green', 'purple', 'orange', 'red'][index % 5]}`}><StarOutlined /></i>
          <span className="evaluation-course"><b>{item.subjectCode ?? 'Chưa có mã'} · {item.subjectName ?? 'Chưa resolve tên môn'}</b><small>{item.className ?? 'Chưa resolve lớp'}{item.semester != null ? ` · HK ${item.semester}` : ''}</small></span>
          <span><b>{item.teacherName ?? 'Chưa cập nhật giảng viên'}</b><Tag color="blue">Loại {item.rawType}</Tag></span><strong>{item.rawTotalScore == null ? '—' : String(item.rawTotalScore)}</strong><RightOutlined />
        </button>)}</div> : <Empty description="Chưa có đánh giá được công bố" />}
      </Card><Alert className="evaluation-note" type="info" showIcon message="Lưu ý: Các điểm số hiển thị là giá trị ghi nhận tại thời điểm hiện tại và chưa phải điểm chính thức." /></main>
      <aside>{selected ? <EvaluationDetail item={selected} /> : <Card><Empty description="Chưa có đánh giá được công bố" /></Card>}</aside>
    </div>
  </div>
}

function EvaluationDetail({ item }: { item: StudentEvaluation }) {
  return <Card className="evaluation-detail-card" title="Chi tiết đánh giá"><div className="evaluation-detail-meta"><Tag color="blue">Loại {item.rawType}</Tag><span>Ngày tạo: {dayjs(item.creationDate).format('DD/MM/YYYY HH:mm')}</span></div>
    <div className="evaluation-detail-course"><span><BookOutlined /></span><div><b>{item.subjectCode ?? 'Chưa có mã'} · {item.subjectName ?? 'Chưa resolve tên môn'}</b><small>{item.className ?? 'Chưa resolve lớp'}{item.semester != null ? ` · HK ${item.semester}` : ''}</small></div></div>
    <section className="evaluation-facts"><div><UserOutlined /><span>Giảng viên<b>{item.teacherName ?? 'Chưa cập nhật'}</b></span></div><div><CalendarOutlined /><span>Loại đánh giá<b>Loại {item.rawType}</b></span></div><div><StarOutlined /><span>Tổng điểm (raw)<b>{item.rawTotalScore == null ? 'Chưa cập nhật' : String(item.rawTotalScore)}</b></span></div><div><InfoCircleOutlined /><span>Nhận xét chung<b>{item.comment ?? 'Chưa có nhận xét'}</b></span></div></section>
    <section className="evaluation-breakdown"><h4>Chi tiết theo tiêu chí</h4>{item.criteria.length ? <><div className="criterion-head"><b>Tiêu chí (snapshot)</b><b>Điểm của bạn</b><b>Điểm tối đa</b></div>{item.criteria.map((criterion, index) => <div className="criterion-row" key={criterion.detailId}><span>{index + 1}. {criterion.snapshotName ?? 'Chưa có tên tiêu chí'}</span><b>{criterion.studentScore ?? '—'}</b><span>{criterion.rawMaximumScore ?? '—'}</span></div>)}<div className="criterion-total"><b>Tổng điểm</b><strong>{item.rawTotalScore ?? '—'}</strong></div></> : <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Chưa có breakdown tiêu chí" />}</section>
    <Alert type="warning" showIcon message="Đây là điểm raw/đánh giá thành phần. Điểm chính thức sẽ được công bố theo thông báo của Khoa/Phòng Đào tạo." />
  </Card>
}
