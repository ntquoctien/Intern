import { useQuery } from '@tanstack/react-query'
import { Alert, Card, Col, Descriptions, Drawer, Empty, List, Row, Segmented, Space, Spin, Statistic, Table, Tag, Typography } from 'antd'
import dayjs from 'dayjs'
import { useMemo, useState, type ReactNode } from 'react'
import { studentApi } from './studentApi'
import type { Attendance, ExamResult, FormRequest, ScheduleItem, StudentSubject } from './types'

function Page({ title, description, children }: { title: string; description?: string; children: ReactNode }) {
  return <Space direction="vertical" size="large" style={{ width: '100%' }}><div><Typography.Title level={2}>{title}</Typography.Title>{description && <Typography.Text type="secondary">{description}</Typography.Text>}</div>{children}</Space>
}

function QueryState<T>({ query, empty, children }: { query: { isLoading: boolean; isError: boolean; data?: T }; empty?: boolean; children: (data: T) => ReactNode }) {
  if (query.isLoading) return <div className="center-state"><Spin tip="Đang tải dữ liệu..." /></div>
  if (query.isError) return <Alert type="error" showIcon message="Không thể tải dữ liệu" description="Vui lòng thử lại hoặc liên hệ quản trị viên nếu lỗi tiếp diễn." />
  if (!query.data || empty) return <Empty description="Chưa có dữ liệu" />
  return <>{children(query.data)}</>
}

const raw = (value?: number | null) => value == null ? 'Chưa xác định' : String(value)
const dateTime = (value: string) => dayjs(value).format('DD/MM/YYYY HH:mm')

export function StudentDashboardPage() {
  const query = useQuery({ queryKey: ['student', 'dashboard'], queryFn: async () => {
    const [profile, subjects, schedule, results] = await Promise.all([studentApi.profile(), studentApi.subjects(), studentApi.schedule(), studentApi.examResults()])
    return { profile, subjects, schedule, results }
  } })
  return <Page title="Tổng quan" description="Thông tin chỉ đọc từ dữ liệu hiện có."><QueryState query={query}>{({ profile, subjects, schedule, results }) => <>
    <Row gutter={[16, 16]}><Col xs={24} md={12} xl={6}><Card><Statistic title="Lớp đã ghi danh" value={subjects.length} /></Card></Col><Col xs={24} md={12} xl={6}><Card><Statistic title="Lịch học hiện có" value={schedule.length} /></Card></Col><Col xs={24} md={12} xl={6}><Card><Statistic title="Bản ghi kết quả" value={results.length} /></Card></Col><Col xs={24} md={12} xl={6}><Card><Statistic title="Trạng thái học tập (thô)" value={raw(profile.studyStatus)} /></Card></Col></Row>
    <Alert type="info" showIcon message="Không hiển thị GPA, tín chỉ tích lũy, đậu/rớt hoặc tỷ lệ điểm danh" description="Chưa có quy tắc nghiệp vụ được xác nhận để tính các chỉ số này." />
    <Row gutter={[16, 16]}><Col xs={24} lg={12}><Card title="Hồ sơ học tập"><Descriptions column={1}><Descriptions.Item label="Sinh viên">{profile.fullName} ({profile.studentCode})</Descriptions.Item><Descriptions.Item label="Ngành">{profile.majorName}</Descriptions.Item><Descriptions.Item label="Khoa">{profile.facultyName ?? 'Chưa có dữ liệu'}</Descriptions.Item><Descriptions.Item label="Niên khóa">{profile.academicYearName}</Descriptions.Item></Descriptions></Card></Col><Col xs={24} lg={12}><Card title="Lịch gần nhất"><List dataSource={schedule.slice(0, 5)} locale={{ emptyText: 'Chưa có lịch' }} renderItem={(item) => <List.Item><List.Item.Meta title={`${item.subjectCode} · ${item.className}`} description={`${dateTime(item.startDateTime)} · ${item.roomName ?? 'Chưa có phòng'}`} /></List.Item>} /></Card></Col></Row>
  </>}</QueryState></Page>
}

export function StudentProfilePage() {
  const query = useQuery({ queryKey: ['student', 'profile'], queryFn: studentApi.profile })
  return <Page title="Hồ sơ học tập"><QueryState query={query}>{(p) => <Card><Descriptions bordered column={{ xs: 1, md: 2 }}><Descriptions.Item label="Họ tên">{p.fullName}</Descriptions.Item><Descriptions.Item label="MSSV">{p.studentCode}</Descriptions.Item><Descriptions.Item label="Tên đăng nhập">{p.userName}</Descriptions.Item><Descriptions.Item label="Ngành">{p.majorCode} · {p.majorName}</Descriptions.Item><Descriptions.Item label="Khoa">{p.facultyName ?? 'Chưa có dữ liệu'}</Descriptions.Item><Descriptions.Item label="Niên khóa">{p.academicYearName}</Descriptions.Item><Descriptions.Item label="Trạng thái học tập (thô)">{raw(p.studyStatus)}</Descriptions.Item><Descriptions.Item label="Đã tốt nghiệp">{p.isGraduated ? 'Có' : 'Không'}</Descriptions.Item><Descriptions.Item label="Cờ cảnh báo">{p.hasIssue == null ? 'Chưa xác định' : p.hasIssue ? 'Có' : 'Không'}</Descriptions.Item></Descriptions></Card>}</QueryState></Page>
}

export function StudentProgramPage() {
  const query = useQuery({ queryKey: ['student', 'program'], queryFn: studentApi.program })
  return <Page title="Kế hoạch học kỳ" description="Kế hoạch theo ngành và niên khóa; không khẳng định bắt buộc/tự chọn hay phiên bản chương trình."><QueryState query={query}>{(program) => <Space direction="vertical" style={{ width: '100%' }}>{program.semesterPlans.length === 0 ? <Empty description="Chưa có kế hoạch học kỳ" /> : program.semesterPlans.map((plan) => <Card key={plan.id} title={`Học kỳ ${plan.semester}`} extra={`${dayjs(plan.startDate).format('DD/MM/YYYY')} – ${dayjs(plan.endDate).format('DD/MM/YYYY')}`}><List dataSource={plan.subjects} renderItem={(subject) => <List.Item><List.Item.Meta title={`${subject.subjectCode} · ${subject.subjectName}`} description={`${subject.creditPoint} tín chỉ (giá trị kế hoạch)`} /></List.Item>} /></Card>)}</Space>}</QueryState></Page>
}

const subjectColumns = [
  { title: 'Mã môn', dataIndex: 'subjectCode' }, { title: 'Môn học', dataIndex: 'subjectName' },
  { title: 'Lớp', dataIndex: 'className' }, { title: 'Tín chỉ', dataIndex: 'creditPoint' },
  { title: 'Phòng mặc định', dataIndex: 'defaultRoom', render: (v: string | null) => v ?? 'Chưa có' },
  { title: 'Giai đoạn theo thời gian', dataIndex: 'schedulePhase' },
]
export function StudentSubjectsPage() {
  const query = useQuery({ queryKey: ['student', 'subjects'], queryFn: studentApi.subjects })
  return <Page title="Môn học & lớp" description="Giai đoạn chỉ dựa trên ngày bắt đầu/kết thúc, không phải kết quả hoàn thành."><QueryState query={query} empty={query.data?.length === 0}>{(items) => <Table<StudentSubject> rowKey="enrollmentId" columns={subjectColumns} dataSource={items} scroll={{ x: 800 }} />}</QueryState></Page>
}

function ScheduleCard({ item }: { item: ScheduleItem }) { return <Card size="small" title={`${item.subjectCode} · ${item.className}`}><Space direction="vertical"><span>{dayjs(item.startDateTime).format('HH:mm')}–{dayjs(item.endDateTime).format('HH:mm')}</span><span>{item.roomName ?? 'Chưa có phòng'}</span><span>{item.teacherName ?? 'Chưa có thông tin giảng viên'}</span></Space></Card> }
export function StudentSchedulePage() {
  const query = useQuery({ queryKey: ['student', 'schedule'], queryFn: studentApi.schedule })
  const [day, setDay] = useState<number>(dayjs().day())
  const days = [1, 2, 3, 4, 5, 6, 0]
  return <Page title="Thời khóa biểu"><QueryState query={query} empty={query.data?.length === 0}>{(items) => <><div className="schedule-desktop">{days.map((value) => <section key={value} className="schedule-day"><Typography.Text strong>{value === 0 ? 'Chủ nhật' : `Thứ ${value + 1}`}</Typography.Text>{items.filter((item) => dayjs(item.startDateTime).day() === value).map((item) => <ScheduleCard key={item.scheduleId} item={item} />)}</section>)}</div><div className="schedule-mobile"><Segmented block value={day} onChange={(v) => setDay(Number(v))} options={days.map((v) => ({ value: v, label: v === 0 ? 'CN' : `T${v + 1}` }))} /><Space direction="vertical" style={{ width: '100%', marginTop: 12 }}>{items.filter((item) => dayjs(item.startDateTime).day() === day).map((item) => <ScheduleCard key={item.scheduleId} item={item} />)}</Space></div></>}</QueryState></Page>
}

const examColumns = [
  { title: 'Kỳ thi', dataIndex: 'examName' }, { title: 'Thời gian', dataIndex: 'examStartDate', render: dateTime },
  { title: 'Điểm thô', dataIndex: 'rawResult', render: raw }, { title: 'Điểm tổng hợp thô', dataIndex: 'rawCombinedResult', render: raw },
  { title: 'Mô tả nguồn', dataIndex: 'description', render: (v: string | null) => v ?? 'Chưa xác định' }, { title: 'Ghi chú', dataIndex: 'notes', render: (v: string | null) => v ?? '—' },
]
export function StudentExamResultsPage() { const query = useQuery({ queryKey: ['student', 'exam-results'], queryFn: studentApi.examResults }); return <Page title="Kết quả thi thô" description="Không suy diễn đậu/rớt hoặc tự chọn điểm chính thức."><QueryState query={query} empty={query.data?.length === 0}>{(items) => <Table<ExamResult> rowKey="examResultId" columns={examColumns} dataSource={items} scroll={{ x: 850 }} />}</QueryState></Page> }

const attendanceColumns = [
  { title: 'Môn học', render: (_: unknown, row: Attendance) => `${row.subjectCode} · ${row.subjectName}` }, { title: 'Lớp', dataIndex: 'className' },
  { title: 'Buổi học', dataIndex: 'startDateTime', render: dateTime }, { title: 'Trạng thái số (thô)', dataIndex: 'rawStatus', render: (v: number) => <Tag>{v}</Tag> },
  { title: 'Cảnh báo', render: (_: unknown, row: Attendance) => row.isFirstTypeWarning || row.isSecondTypeWarning ? 'Có cờ cảnh báo' : 'Không có cờ' }, { title: 'Ghi chú', dataIndex: 'notes' },
]
export function StudentAttendancePage() { const query = useQuery({ queryKey: ['student', 'attendance'], queryFn: studentApi.attendance }); return <Page title="Điểm danh thô" description="Giữ nguyên trạng thái số; không gán nhãn có mặt/vắng và không tính tỷ lệ."><QueryState query={query} empty={query.data?.length === 0}>{(items) => <Table<Attendance> rowKey="attendanceId" columns={attendanceColumns} dataSource={items} scroll={{ x: 850 }} />}</QueryState></Page> }

export function StudentFormRequestsPage() {
  const query = useQuery({ queryKey: ['student', 'form-requests'], queryFn: studentApi.formRequests })
  const [selected, setSelected] = useState<FormRequest>()
  const columns = useMemo(() => [
    { title: 'Biểu mẫu', dataIndex: 'formTemplateName', render: (v: string | null) => v ?? 'Chưa có tên mẫu' },
    { title: 'Ngày tạo', dataIndex: 'creationDate', render: dateTime }, { title: 'Trạng thái thô', dataIndex: 'rawStatus' }, { title: 'Người duyệt', dataIndex: 'approvalName', render: (v: string) => v || 'Chưa có' },
  ], [])
  return <Page title="Yêu cầu biểu mẫu" description="Chỉ hiển thị yêu cầu đã tồn tại; không gửi, tải lên hoặc hủy yêu cầu."><QueryState query={query} empty={query.data?.length === 0}>{(items) => <Table<FormRequest> rowKey="formRequestId" columns={columns} dataSource={items} onRow={(record) => ({ onClick: () => setSelected(record), tabIndex: 0, onKeyDown: (event) => { if (event.key === 'Enter') setSelected(record) } })} />}</QueryState><Drawer title="Chi tiết yêu cầu" open={Boolean(selected)} onClose={() => setSelected(undefined)}>{selected && <Descriptions column={1}><Descriptions.Item label="Biểu mẫu">{selected.formTemplateName ?? 'Chưa có'}</Descriptions.Item><Descriptions.Item label="Trạng thái thô">{selected.rawStatus}</Descriptions.Item><Descriptions.Item label="Ngày tạo">{dateTime(selected.creationDate)}</Descriptions.Item><Descriptions.Item label="Cập nhật">{dateTime(selected.updateDate)}</Descriptions.Item><Descriptions.Item label="Người duyệt">{selected.approvalName || 'Chưa có'}</Descriptions.Item><Descriptions.Item label="Ghi chú">{selected.note || '—'}</Descriptions.Item></Descriptions>}</Drawer></Page>
}
