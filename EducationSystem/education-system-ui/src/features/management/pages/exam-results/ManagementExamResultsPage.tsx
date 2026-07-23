import { useQuery } from '@tanstack/react-query'
import { CalendarOutlined, CheckSquareOutlined, DownloadOutlined, FileTextOutlined, LeftOutlined, SettingOutlined, TeamOutlined } from '@ant-design/icons'
import { Alert, Button, Card, DatePicker, Empty, Input, Select, Skeleton, Space, Statistic, Table, Tabs, Tag, Typography, type TableColumnsType } from 'antd'
import dayjs from 'dayjs'
import { Link, useSearchParams } from 'react-router-dom'
import { managementApi, type ExamAttempt, type ExamResultRecord, type ManagementExam } from '../../managementApi'

const formatNumber = new Intl.NumberFormat('vi-VN')
const examType = (value: number) => `Loại ${value}`
const methodName = (value?: number | null) => value == null ? 'Chưa xác định' : `Phương thức ${value}`

export function ManagementExamResultsPage() {
  const [params, setParams] = useSearchParams()
  const tab = params.get('tab') ?? 'exams'
  const page = Number(params.get('page') ?? 1)
  const pageSize = Number(params.get('pageSize') ?? 10)
  const search = params.get('search') ?? ''
  const type = params.get('type') ?? ''
  const fromDate = params.get('from') ?? ''
  const toDate = params.get('to') ?? ''
  const queryParams = { pageNumber: page, pageSize, search: search || undefined, type: type || undefined, fromDate: fromDate || undefined, toDate: toDate || undefined }
  const exams = useQuery({ queryKey: ['management', 'exams-page', queryParams], queryFn: () => managementApi.examPage(queryParams) })
  const classesQuery = useQuery({ queryKey: ['management', 'classes'], queryFn: managementApi.classes })
  const roomsQuery = useQuery({ queryKey: ['management', 'rooms'], queryFn: managementApi.rooms })
  const selectedId = params.get('id') ?? exams.data?.items[0]?.examId
  const detail = useQuery({ queryKey: ['management', 'exam-detail', selectedId], queryFn: () => managementApi.examDetail(selectedId!), enabled: !!selectedId })
  const attempts = useQuery({ queryKey: ['management', 'exam-attempts', page, pageSize, selectedId], queryFn: () => managementApi.examAttempts({ pageNumber: page, pageSize, subjectTeachingExamId: selectedId }), enabled: tab === 'attempts' })
  const results = useQuery({ queryKey: ['management', 'exam-results-page', page, pageSize, selectedId], queryFn: () => managementApi.examResultPage({ pageNumber: page, pageSize, subjectTeachingExamId: selectedId }), enabled: tab === 'results' })
  const update = (values: Record<string, string | undefined>) => {
    const next = new URLSearchParams(params)
    Object.entries(values).forEach(([key, value]) => value ? next.set(key, value) : next.delete(key))
    if (!Object.hasOwn(values, 'page')) next.set('page', '1')
    setParams(next)
  }
  const classMap = new Map((classesQuery.data ?? []).map(item => [item.classId, item]))
  const roomMap = new Map((roomsQuery.data ?? []).map(item => [item.roomId, item.name]))

  if (exams.isLoading) return <Skeleton active paragraph={{ rows: 14 }} />
  if (exams.isError || !exams.data) return <Alert type="error" showIcon title="Không thể tải dữ liệu kỳ thi" />

  const examColumns: TableColumnsType<ManagementExam> = [
    { title: 'Tên kỳ thi', dataIndex: 'name', width: 130, render: value => <b>{value}</b> },
    { title: 'Môn học', width: 180, render: (_, item) => { const course = classMap.get(item.subjectTeachingId); return course ? <><b>{course.subjectCode}</b><br />{course.subjectName}</> : 'Đang đối chiếu…' } },
    { title: 'Lớp học phần', width: 150, render: (_, item) => classMap.get(item.subjectTeachingId)?.className ?? item.subjectTeachingId.slice(0, 8) },
    { title: 'Loại kỳ thi', width: 90, render: (_, item) => examType(item.type) },
    { title: 'Thời gian thi', width: 145, render: (_, item) => <>{dayjs(item.startDate).format('DD/MM/YYYY')}<br />{dayjs(item.startDate).format('HH:mm')} – {dayjs(item.endDate).format('HH:mm')}</> },
    { title: 'Phòng thi', width: 100, render: (_, item) => item.roomId ? roomMap.get(item.roomId) ?? item.roomId.slice(0, 8) : 'Chưa xếp' },
    { title: 'Lượt thi', dataIndex: 'attemptCount', width: 75 },
    { title: 'Kết quả', dataIndex: 'resultCount', width: 75 },
    { title: 'Trạng thái', width: 95, render: (_, item) => <Tag color={dayjs(item.endDate).isBefore(dayjs()) ? 'green' : 'blue'}>{dayjs(item.endDate).isBefore(dayjs()) ? 'Đã kết thúc' : 'Sắp diễn ra'}</Tag> },
  ]
  const attemptColumns: TableColumnsType<ExamAttempt> = [
    { title: 'Sinh viên', dataIndex: 'studentId', render: value => <Link to={`/management/people/students?id=${value}`}>{value}</Link> },
    { title: 'Kỳ thi', dataIndex: 'subjectTeachingExamId' },
    { title: 'Lưu nháp', dataIndex: 'draftDate', render: value => value ? dayjs(value).format('DD/MM/YYYY HH:mm') : '—' },
    { title: 'Nộp bài', dataIndex: 'submitDate', render: value => value ? dayjs(value).format('DD/MM/YYYY HH:mm') : 'Chưa nộp' },
  ]
  const resultColumns: TableColumnsType<ExamResultRecord> = [
    { title: 'Sinh viên', dataIndex: 'studentId', render: value => <Link to={`/management/people/students?id=${value}`}>{value}</Link> },
    { title: 'Kết quả nguồn', dataIndex: 'result', render: rawValue },
    { title: 'Tổng hợp nguồn', dataIndex: 'combinedResult', render: rawValue },
    { title: 'Mô tả', dataIndex: 'examResultDesc', render: value => value || '—' },
    { title: 'Ghi chú', dataIndex: 'notes', render: value => value || '—' },
  ]

  return <div className="exam-management-page">
    <div className="exam-page-toolbar">
      <Typography.Text type="secondary">Tra cứu kỳ thi, lượt làm bài và kết quả trong ngữ cảnh môn, lớp và sinh viên.</Typography.Text>
      <Button type="primary" icon={<DownloadOutlined />} onClick={() => exportExams(exams.data.items)}>Xuất CSV</Button>
    </div>
    <div className="exam-page-layout">
      <section className="exam-page-main">
        <Card className="dashboard-panel exam-filter-card">
          <Tabs activeKey={tab} onChange={value => update({ tab: value, id: undefined })} items={[
            { key: 'exams', label: 'Kỳ thi' }, { key: 'attempts', label: 'Lượt thi' }, { key: 'results', label: 'Kết quả' },
          ]} />
          <div className="exam-filter-grid">
            <label>Tìm kiếm<Input value={search} placeholder="Tên kỳ thi, ghi chú…" onChange={event => update({ search: event.target.value || undefined })} /></label>
            <label>Loại kỳ thi<Select allowClear value={type || undefined} placeholder="Tất cả" options={[0, 1, 2, 3, 4].map(value => ({ value: String(value), label: examType(value) }))} onChange={value => update({ type: value })} /></label>
            <label>Khoảng ngày<DatePicker.RangePicker value={fromDate && toDate ? [dayjs(fromDate), dayjs(toDate)] : null} onChange={dates => update({ from: dates?.[0]?.format('YYYY-MM-DD'), to: dates?.[1]?.format('YYYY-MM-DD') })} /></label>
            <Button onClick={() => setParams(new URLSearchParams({ tab }))}>Xóa bộ lọc</Button>
          </div>
        </Card>
        <div className="exam-stat-grid">
          <Summary icon={<FileTextOutlined />} title="Tổng kỳ thi" value={exams.data.totalItems} tone="purple" />
          <Summary icon={<TeamOutlined />} title="Tổng lượt thi" value={exams.data.totalAttempts} tone="green" />
          <Summary icon={<CheckSquareOutlined />} title="Tổng kết quả" value={exams.data.totalResults} tone="blue" />
          <Summary icon={<CheckSquareOutlined />} title="Kết quả có điểm" value={exams.data.resultsWithScore} tone="teal" />
          <Summary icon={<FileTextOutlined />} title="Kết quả chưa có điểm" value={exams.data.totalResults - exams.data.resultsWithScore} tone="red" />
        </div>
        {tab === 'exams' && <Card className="dashboard-panel" title={<>Danh sách kỳ thi <Tag>{formatNumber.format(exams.data.totalItems)}</Tag></>} extra={<Button icon={<SettingOutlined />}>Cài đặt cột</Button>}>
          <Table rowKey="examId" size="small" scroll={{ x: 1100 }} columns={examColumns} dataSource={exams.data.items} rowClassName={item => item.examId === selectedId ? 'selected-exam-row' : ''} onRow={item => ({ onClick: () => update({ id: item.examId, page: String(page) }) })} pagination={{ current: page, pageSize, total: exams.data.totalItems, showSizeChanger: true, onChange: (next, size) => update({ page: String(next), pageSize: String(size) }) }} />
        </Card>}
        {tab === 'attempts' && <DataCard title="Danh sách lượt thi" query={attempts} columns={attemptColumns} rowKey="id" page={page} pageSize={pageSize} update={update} />}
        {tab === 'results' && <DataCard title="Danh sách kết quả nguồn" query={results} columns={resultColumns} rowKey="id" page={page} pageSize={pageSize} update={update} />}
      </section>
      <Card className="dashboard-panel exam-detail-panel" title={<Space><Button icon={<LeftOutlined />} />Chi tiết kỳ thi</Space>}>
        {detail.isLoading ? <Skeleton active /> : detail.data ? <ExamDetail item={detail.data} classItem={classMap.get(detail.data.subjectTeachingId)} roomName={detail.data.roomId ? roomMap.get(detail.data.roomId) : undefined} /> : <Empty description="Chọn kỳ thi để xem chi tiết" />}
      </Card>
    </div>
  </div>
}

function Summary({ icon, title, value, tone }: { icon: React.ReactNode; title: string; value: number; tone: string }) {
  return <Card className={`exam-summary exam-summary-${tone}`}><span>{icon}</span><Statistic title={title} value={value} formatter={value => formatNumber.format(Number(value))} /></Card>
}

function ExamDetail({ item, classItem, roomName }: { item: ManagementExam; classItem?: { classId: string; className: string; subjectCode: string; subjectName: string }; roomName?: string }) {
  const ended = dayjs(item.endDate).isBefore(dayjs())
  return <div className="exam-detail-content">
    <div className="exam-detail-heading"><span><FileTextOutlined /></span><div><Typography.Title level={3}>{item.name}</Typography.Title><Tag color={ended ? 'green' : 'blue'}>{ended ? 'Đã kết thúc' : 'Sắp diễn ra'}</Tag><p>{classItem ? `${classItem.subjectCode} · ${classItem.subjectName}` : 'Đang đối chiếu môn học…'}</p><p>{classItem?.className ?? item.subjectTeachingId}</p></div></div>
    <div className="exam-detail-highlights">
      <span>Loại kỳ thi<b>{examType(item.type)}</b></span><span>Thời gian<b>{dayjs(item.startDate).format('DD/MM/YYYY HH:mm')} – {dayjs(item.endDate).format('HH:mm')}</b></span><span>Phòng thi<b>{roomName ?? 'Chưa xếp'}</b></span><span>Số lượt thi<b>{item.attemptCount}</b></span>
    </div>
    <Tabs items={[
      { key: 'general', label: 'Thông tin chung', children: <div className="exam-info-grid"><span>Question suite<b>{item.questionSuiteName ?? item.questionSuiteId ?? 'Chưa cấu hình'}</b></span><span>Phương pháp chấm<b>{methodName(item.method)}</b></span><span>Tổng số câu hỏi<b>{item.questionCount}</b></span><span>Câu dễ / thường / khó / thực hành<b>{item.easyCount} / {item.normalCount} / {item.hardCount} / {item.practiceCount}</b></span><span>Cho phép thông báo SV<b>{item.allowNotifyStudent ? 'Có' : 'Không'}</b></span><span>Ghi chú<b>{item.notes || '—'}</b></span></div> },
      { key: 'attempts', label: `Lượt thi (${item.attemptCount})`, children: <Alert type="info" message="Chọn tab Lượt thi ở danh sách để tra cứu server-side." /> },
      { key: 'results', label: `Kết quả (${item.resultCount})`, children: <Alert type="info" message="Kết quả được hiển thị nguyên trạng từ nguồn, không suy diễn đạt/rớt." /> },
      { key: 'config', label: 'Cấu hình', children: <div className="exam-info-grid"><span>Method raw<b>{item.method ?? 'null'}</b></span><span>Type raw<b>{item.type}</b></span></div> },
    ]} />
    <Typography.Title level={5}>Thống kê dữ liệu nguồn</Typography.Title>
    <div className="exam-result-stats"><Statistic title="Lượt thi" value={item.attemptCount} /><Statistic title="Kết quả" value={item.resultCount} /><Statistic title="Số câu" value={item.questionCount} /></div>
    <Alert type="warning" showIcon message="Không tự kết luận điểm chính thức, đạt/rớt, điểm chữ hoặc GPA từ các giá trị raw." />
    <div className="exam-quick-links"><Link to={`/management/teaching/classes?id=${item.subjectTeachingId}`}><Button icon={<TeamOutlined />}>Xem lớp học phần</Button></Link><Link to="/management/teaching/schedule"><Button icon={<CalendarOutlined />}>Xem lịch thi</Button></Link></div>
  </div>
}

function DataCard<T extends object>({ title, query, columns, rowKey, page, pageSize, update }: { title: string; query: { isLoading: boolean; isError: boolean; data?: { items: T[]; totalItems: number } }; columns: TableColumnsType<T>; rowKey: string; page: number; pageSize: number; update: (values: Record<string, string | undefined>) => void }) {
  return <Card className="dashboard-panel" title={title}>{query.isLoading ? <Skeleton active /> : query.isError || !query.data ? <Alert type="error" message={`Không thể tải ${title.toLowerCase()}`} /> : <Table size="small" rowKey={rowKey} columns={columns} dataSource={query.data.items} pagination={{ current: page, pageSize, total: query.data.totalItems, onChange: (next, size) => update({ page: String(next), pageSize: String(size) }) }} />}</Card>
}

function rawValue(value?: number | null) { return value == null ? <Typography.Text type="secondary">Chưa có</Typography.Text> : <b>{value}</b> }
function exportExams(items: ManagementExam[]) {
  const rows = [['Tên kỳ thi', 'Lớp học phần', 'Bắt đầu', 'Kết thúc', 'Loại raw', 'Lượt thi', 'Kết quả'], ...items.map(item => [item.name, item.subjectTeachingId, item.startDate, item.endDate, item.type, item.attemptCount, item.resultCount])]
  const blob = new Blob([`\uFEFF${rows.map(row => row.map(value => `"${String(value).replaceAll('"', '""')}"`).join(',')).join('\n')}`], { type: 'text/csv;charset=utf-8' })
  const href = URL.createObjectURL(blob); const anchor = document.createElement('a'); anchor.href = href; anchor.download = 'ky-thi.csv'; anchor.click(); URL.revokeObjectURL(href)
}
