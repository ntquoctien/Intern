import { useQuery } from '@tanstack/react-query'
import { CheckCircleOutlined, CloseOutlined, DownloadOutlined, FileTextOutlined, SearchOutlined, SettingOutlined, TeamOutlined, UserOutlined } from '@ant-design/icons'
import { Alert, Avatar, Button, Card, DatePicker, Empty, Input, InputNumber, Select, Skeleton, Space, Statistic, Table, Tabs, Tag, Typography, type TableColumnsType } from 'antd'
import dayjs from 'dayjs'
import { Link, useSearchParams } from 'react-router-dom'
import { managementApi, type StudentEvaluationBreakdown, type StudentEvaluationItem } from '../../managementApi'

const number = new Intl.NumberFormat('vi-VN')
const typeLabel = (value: number) => `Loại ${value}`

export function ManagementEvaluationsPage() {
  const [params, setParams] = useSearchParams()
  const page = Number(params.get('page') ?? 1); const pageSize = Number(params.get('pageSize') ?? 10)
  const search = params.get('search') ?? ''; const classId = params.get('classId') ?? ''; const type = params.get('type') ?? ''
  const from = params.get('from') ?? ''; const to = params.get('to') ?? ''; const minimumScore = params.get('min') ?? ''; const maximumScore = params.get('max') ?? ''
  const queryParams = { pageNumber: page, pageSize, search: search || undefined, classId: classId || undefined, type: type || undefined, fromDate: from || undefined, toDate: to || undefined, minimumScore: minimumScore || undefined, maximumScore: maximumScore || undefined }
  const evaluations = useQuery({ queryKey: ['management', 'student-evaluations', queryParams], queryFn: () => managementApi.studentEvaluationPage(queryParams) })
  const classesQuery = useQuery({ queryKey: ['management', 'classes'], queryFn: managementApi.classes })
  const selectedId = params.get('id') ?? evaluations.data?.items[0]?.evaluationId
  const detail = useQuery({ queryKey: ['management', 'student-evaluation', selectedId], queryFn: () => managementApi.studentEvaluationDetail(selectedId!), enabled: !!selectedId })
  const update = (values: Record<string, string | undefined>) => { const next = new URLSearchParams(params); Object.entries(values).forEach(([key, value]) => value ? next.set(key, value) : next.delete(key)); if (!Object.hasOwn(values, 'page')) next.set('page', '1'); setParams(next) }
  const classes = classesQuery.data ?? []
  if (evaluations.isLoading) return <Skeleton active paragraph={{ rows: 15 }} />
  if (evaluations.isError || !evaluations.data) return <Alert type="error" showIcon title="Không thể tải danh sách đánh giá sinh viên" />

  const columns: TableColumnsType<StudentEvaluationItem> = [
    { title: 'Sinh viên', width: 175, render: (_, item) => <Space><Avatar src={item.studentProfilePicUrl} icon={<UserOutlined />} /><span><b>{item.studentName ?? item.studentCode}</b><small>{item.studentCode}</small></span></Space> },
    { title: 'Học kỳ', width: 95, render: (_, item) => item.semester ? `Học kỳ ${item.semester}` : 'Chưa liên kết' },
    { title: 'Lớp học phần', width: 150, render: (_, item) => <>{item.subjectCode && <b>{item.subjectCode}</b>}<br />{item.className ?? 'Chưa liên kết'}</> },
    { title: 'Kỳ thi / Bài đánh giá', width: 130, render: (_, item) => item.examId ? item.examId.slice(0, 10) : item.questionId ? item.questionId.slice(0, 10) : 'Đánh giá chung' },
    { title: 'Type', width: 80, render: (_, item) => <Tag color="blue">{typeLabel(item.rawType)}</Tag> },
    { title: 'Tổng điểm', width: 85, render: (_, item) => item.totalScore == null ? <Typography.Text type="secondary">Chưa có</Typography.Text> : <b className="evaluation-score">{item.totalScore}</b> },
    { title: 'Ngày tạo', width: 110, render: (_, item) => <>{dayjs(item.creationDate).format('DD/MM/YYYY')}<br />{dayjs(item.creationDate).format('HH:mm')}</> },
    { title: 'Giảng viên', dataIndex: 'teacherName', width: 120, render: value => value ?? 'Chưa xác định' },
    { title: 'Nhận xét', dataIndex: 'comment', width: 160, render: value => <Typography.Paragraph ellipsis={{ rows: 2 }}>{value || '—'}</Typography.Paragraph> },
  ]
  return <div className="evaluation-page">
    <div className="evaluation-toolbar"><Typography.Text type="secondary">Tra cứu đánh giá, nhận xét và breakdown đang lưu cho sinh viên.</Typography.Text><Button type="primary" icon={<DownloadOutlined />} onClick={() => exportCsv(evaluations.data.items)}>Xuất CSV</Button></div>
    <div className="evaluation-layout">
      <section className="evaluation-main">
        <Card className="dashboard-panel evaluation-filters">
          <label>Sinh viên<Input allowClear value={search} prefix={<SearchOutlined />} placeholder="Tìm theo mã, nhận xét, giảng viên…" onChange={event => update({ search: event.target.value || undefined })} /></label>
          <label>Lớp học phần<Select allowClear showSearch value={classId || undefined} placeholder="Chọn lớp học phần" options={classes.map(item => ({ value: item.classId, label: `${item.subjectCode} · ${item.className}` }))} onChange={value => update({ classId: value })} /></label>
          <label>Loại đánh giá<Select allowClear value={type || undefined} placeholder="Tất cả" options={[...new Set(evaluations.data.items.map(item => item.rawType))].map(value => ({ value: String(value), label: typeLabel(value) }))} onChange={value => update({ type: value })} /></label>
          <label>Khoảng ngày<DatePicker.RangePicker value={from && to ? [dayjs(from), dayjs(to)] : null} onChange={dates => update({ from: dates?.[0]?.format('YYYY-MM-DD'), to: dates?.[1]?.format('YYYY-MM-DD') })} /></label>
          <label>Khoảng điểm<Space.Compact><InputNumber value={minimumScore ? Number(minimumScore) : null} placeholder="Từ" onChange={value => update({ min: value == null ? undefined : String(value) })} /><InputNumber value={maximumScore ? Number(maximumScore) : null} placeholder="Đến" onChange={value => update({ max: value == null ? undefined : String(value) })} /></Space.Compact></label>
          <Button icon={<CloseOutlined />} onClick={() => setParams(new URLSearchParams())}>Xóa bộ lọc</Button>
        </Card>
        <div className="evaluation-stats">
          <Stat icon={<FileTextOutlined />} title="Tổng đánh giá" value={evaluations.data.totalItems} />
          <Stat icon={<CheckCircleOutlined />} title="Điểm trung bình nguồn" value={evaluations.data.averageScore ?? 0} precision={2} tone="green" />
          <Stat icon={<FileTextOutlined />} title="Đánh giá có nhận xét" value={evaluations.data.withCommentCount} tone="purple" />
          <Stat icon={<TeamOutlined />} title="Sinh viên được đánh giá" value={evaluations.data.studentCount} tone="orange" />
          <Stat icon={<FileTextOutlined />} title="Đánh giá chưa có điểm" value={evaluations.data.withoutScoreCount} tone="blue" />
        </div>
        <Card className="dashboard-panel evaluation-list" title={<>Danh sách đánh giá <Tag>{number.format(evaluations.data.totalItems)}</Tag></>} extra={<Button icon={<SettingOutlined />}>Cài đặt cột</Button>}>
          <Table size="small" rowKey="evaluationId" scroll={{ x: 1160 }} columns={columns} dataSource={evaluations.data.items} rowClassName={item => item.evaluationId === selectedId ? 'selected-evaluation-row' : ''} onRow={item => ({ onClick: () => update({ id: item.evaluationId, page: String(page) }) })} pagination={{ current: page, pageSize, total: evaluations.data.totalItems, showSizeChanger: true, onChange: (next, size) => update({ page: String(next), pageSize: String(size) }) }} />
        </Card>
      </section>
      <Card className="dashboard-panel evaluation-detail" title="Chi tiết đánh giá" extra={<Button type="text" icon={<CloseOutlined />} onClick={() => update({ id: undefined })} />}>{detail.isLoading ? <Skeleton active /> : detail.isError || !detail.data ? <Empty description="Chọn đánh giá để xem chi tiết" /> : <EvaluationDetail item={detail.data} />}</Card>
    </div>
  </div>
}

function Stat({ icon, title, value, precision, tone = 'navy' }: { icon: React.ReactNode; title: string; value: number; precision?: number; tone?: string }) { return <Card className={`evaluation-stat evaluation-stat-${tone}`}><span>{icon}</span><Statistic title={title} value={value} precision={precision} /></Card> }
function EvaluationDetail({ item }: { item: StudentEvaluationItem }) {
  const breakdownColumns: TableColumnsType<StudentEvaluationBreakdown> = [
    { title: 'Tiêu chí', render: (_, row, index) => `${index + 1}. ${row.criteriaName ?? row.snapshotName ?? 'Chưa đặt tên'}` },
    { title: 'Tên tại thời điểm đánh giá', render: (_, row) => row.snapshotName ?? '—' },
    { title: 'Điểm SV', dataIndex: 'studentScore', render: raw },
    { title: 'Điểm tối đa', dataIndex: 'maximumScore', render: raw },
    { title: 'Đối chiếu', render: (_, row) => <Tag color={row.criteriaResolved ? 'green' : 'orange'}>{row.criteriaResolved ? 'Đã khớp' : 'Chỉ có dữ liệu lưu'}</Tag> },
  ]
  const info = <><div className="evaluation-info-grid"><span>Kỳ thi / Bài đánh giá<b>{item.examId ?? item.questionId ?? 'Đánh giá chung'}</b></span><span>Giảng viên<b>{item.teacherName ?? 'Chưa xác định'}</b></span><span>Lớp học phần<b>{item.subjectCode ? `${item.subjectCode} · ${item.className}` : 'Chưa liên kết'}</b></span><span>Tổng điểm<b>{item.totalScore ?? 'Chưa có'}</b></span><span>Học kỳ<b>{item.semester ? `Học kỳ ${item.semester}` : 'Chưa liên kết'}</b></span><span>Loại đánh giá (Type)<b>{typeLabel(item.rawType)}</b></span><span>Ngày tạo<b>{dayjs(item.creationDate).format('DD/MM/YYYY HH:mm')}</b></span><span>Cập nhật cuối<b>{item.updatedDate ? dayjs(item.updatedDate).format('DD/MM/YYYY HH:mm') : 'Chưa cập nhật'}</b></span></div><Typography.Title level={5}>Ghi chú / Nhận xét</Typography.Title><Typography.Paragraph>{item.comment || 'Không có nhận xét.'}</Typography.Paragraph></>
  return <div className="evaluation-detail-content"><div className="evaluation-student"><Avatar size={66} src={item.studentProfilePicUrl} icon={<UserOutlined />} /><div><Typography.Title level={3}>{item.studentName ?? item.studentCode}</Typography.Title><Tag color="green">{item.studentCode}</Tag><p>{item.majorName} · {item.academicYearName}</p></div></div><Tabs items={[{ key: 'info', label: 'Thông tin đánh giá', children: info }, { key: 'breakdown', label: `Breakdown (${item.breakdown.length})`, children: <Table size="small" rowKey="detailId" columns={breakdownColumns} dataSource={item.breakdown} pagination={false} /> }, { key: 'history', label: 'Lịch sử thay đổi', children: <Alert type="info" message={`Tạo ${dayjs(item.creationDate).format('DD/MM/YYYY HH:mm')}; cập nhật ${item.updatedDate ? dayjs(item.updatedDate).format('DD/MM/YYYY HH:mm') : 'chưa có'}.`} /> }]} /><Typography.Title level={5}>Breakdown điểm ({item.breakdown.length} tiêu chí)</Typography.Title><Table size="small" rowKey="detailId" columns={breakdownColumns} dataSource={item.breakdown} pagination={false} scroll={{ x: 620 }} /><Alert type="warning" showIcon message="Tiêu chí không còn trong danh mục được hiển thị bằng tên đã lưu tại thời điểm đánh giá; hệ thống không tự tạo xếp loại." /><div className="evaluation-links"><Link to={`/management/people/students?id=${item.studentId}`}><Button>Xem hồ sơ sinh viên</Button></Link>{item.classId && <Link to={`/management/teaching/classes?id=${item.classId}`}><Button>Xem lớp học phần</Button></Link>}{item.examId && <Link to={`/management/assessment/results?id=${item.examId}`}><Button>Xem kỳ thi</Button></Link>}</div></div>
}
function raw(value?: number | null) { return value == null ? '—' : value }
function exportCsv(items: StudentEvaluationItem[]) { const rows = [['Mã SV', 'Sinh viên', 'Lớp', 'Type raw', 'Tổng điểm', 'Ngày tạo', 'Giảng viên', 'Nhận xét'], ...items.map(item => [item.studentCode, item.studentName ?? '', item.className ?? '', item.rawType, item.totalScore ?? '', item.creationDate, item.teacherName ?? '', item.comment ?? ''])]; const blob = new Blob([`\uFEFF${rows.map(row => row.map(value => `"${String(value).replaceAll('"', '""')}"`).join(',')).join('\n')}`], { type: 'text/csv;charset=utf-8' }); const href = URL.createObjectURL(blob); const anchor = document.createElement('a'); anchor.href = href; anchor.download = 'danh-gia-sinh-vien.csv'; anchor.click(); URL.revokeObjectURL(href) }
