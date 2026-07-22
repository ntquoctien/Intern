import { useQuery } from '@tanstack/react-query'
import { CloseOutlined, DownloadOutlined, EyeOutlined, RightOutlined, SearchOutlined, UserOutlined } from '@ant-design/icons'
import { Alert, Avatar, Button, Card, Empty, Input, Select, Skeleton, Space, Table, Tabs, Tag, Typography, type TableColumnsType } from 'antd'
import dayjs from 'dayjs'
import { Link, useSearchParams } from 'react-router-dom'
import { managementApi, type TeacherClass, type TeacherDetail, type TeacherSummary } from '../../managementApi'

const number = new Intl.NumberFormat('vi-VN')

export function ManagementTeachersPage() {
  const [params, setParams] = useSearchParams()
  const teachers = useQuery({ queryKey: ['management', 'teachers'], queryFn: managementApi.teachers })
  const selectedId = params.get('id')
  const detail = useQuery({ queryKey: ['management', 'teacher', selectedId], queryFn: () => managementApi.teacher(selectedId!), enabled: !!selectedId })
  const update = (values: Record<string, string | undefined>) => { const next = new URLSearchParams(params); Object.entries(values).forEach(([key, value]) => value ? next.set(key, value) : next.delete(key)); if (!Object.hasOwn(values, 'page')) next.set('page', '1'); setParams(next) }

  if (teachers.isLoading) return <Skeleton active paragraph={{ rows: 15 }} />
  if (teachers.isError || !teachers.data) return <Alert showIcon type="error" title="Không thể tải danh sách giảng viên" />
  const search = params.get('search') ?? ''
  const faculty = params.get('faculty') ?? ''
  const head = params.get('head') ?? ''
  const page = Number(params.get('page') ?? 1)
  const filtered = teachers.data.filter((teacher) => (!search || `${teacher.fullName ?? ''} ${teacher.facultyCode} ${teacher.facultyName}`.toLocaleLowerCase('vi').includes(search.toLocaleLowerCase('vi'))) && (!faculty || teacher.facultyCode === faculty) && (!head || String(teacher.isHeadOfFaculty) === head))

  const columns: TableColumnsType<TeacherSummary> = [
    { title: 'Họ và tên', render: (_, row) => <Space><Avatar icon={<UserOutlined />} /><b>{row.fullName ?? 'Chưa xác định'}</b></Space> },
    { title: 'Khoa', render: (_, row) => <Space orientation="vertical" size={0}><span>{row.facultyName}</span><Typography.Text type="secondary">{row.facultyCode}</Typography.Text></Space> },
    { title: 'Trưởng khoa', align: 'center', render: (_, row) => row.isHeadOfFaculty ? <Tag color="green">Trưởng khoa</Tag> : '—' },
    { title: 'Số lớp phân công', dataIndex: 'classCount', align: 'center' },
    { title: 'Môn giảng dạy', dataIndex: 'subjectNames', render: (items: string[]) => items.slice(0, 2).join(', ') || 'Chưa có' },
    { title: 'Thao tác', width: 80, render: (_, row) => <Button icon={<EyeOutlined />} aria-label={`Xem ${row.fullName ?? 'giảng viên'}`} onClick={() => update({ id: row.teacherFacultyId, page: String(page) })} /> },
  ]
  const exportCsv = () => { const rows = [['Họ tên', 'Mã khoa', 'Khoa', 'Trưởng khoa', 'Số lớp', 'Môn giảng dạy'], ...filtered.map((item) => [item.fullName ?? '', item.facultyCode, item.facultyName, item.isHeadOfFaculty ? 'Có' : 'Không', item.classCount, item.subjectNames.join('; ')])]; const blob = new Blob([`\uFEFF${rows.map((row) => row.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(',')).join('\n')}`], { type: 'text/csv;charset=utf-8' }); const href = URL.createObjectURL(blob); const anchor = document.createElement('a'); anchor.href = href; anchor.download = 'danh-sach-giang-vien.csv'; anchor.click(); URL.revokeObjectURL(href) }

  return <div className="management-page teachers-page">
    <div className="teacher-page-actions"><Typography.Text type="secondary">Tra cứu giảng viên theo khoa và các lớp/lịch được phân công.</Typography.Text><Button type="primary" icon={<DownloadOutlined />} onClick={exportCsv}>Xuất CSV</Button></div>
    <Card className="dashboard-panel teacher-filters">
      <Input value={search} allowClear prefix={<SearchOutlined />} placeholder="Tìm theo tên hoặc khoa..." onChange={(event) => update({ search: event.target.value || undefined })} />
      <Select value={faculty || undefined} allowClear placeholder="Khoa: Tất cả" options={[...new Map(teachers.data.map((item) => [item.facultyCode, { value: item.facultyCode, label: `${item.facultyCode} · ${item.facultyName}` }])).values()]} onChange={(value) => update({ faculty: value })} />
      <Select value={head || undefined} allowClear placeholder="Trưởng khoa: Tất cả" options={[{ value: 'true', label: 'Trưởng khoa' }, { value: 'false', label: 'Giảng viên' }]} onChange={(value) => update({ head: value })} />
      <Button icon={<CloseOutlined />} onClick={() => setParams(new URLSearchParams())}>Xóa bộ lọc</Button>
    </Card>
    <div className="teachers-grid">
      <Card className="dashboard-panel teacher-list" title={<span>Danh sách giảng viên <Tag>{number.format(filtered.length)}</Tag></span>}><Table rowKey="teacherFacultyId" columns={columns} dataSource={filtered} scroll={{ x: 900 }} pagination={{ current: page, pageSize: 10, total: filtered.length, showSizeChanger: true }} onChange={(pagination) => update({ page: String(pagination.current ?? 1) })} onRow={(row) => ({ onClick: () => update({ id: row.teacherFacultyId, page: String(page) }) })} rowClassName={(row) => row.teacherFacultyId === selectedId ? 'selected-teacher-row' : ''} /></Card>
      <Card className="dashboard-panel teacher-detail" title="Hồ sơ giảng viên">{!selectedId ? <Empty description="Chọn một giảng viên để xem hồ sơ" /> : detail.isLoading ? <Skeleton active /> : detail.isError || !detail.data ? <Alert showIcon type="error" title="Không thể tải hồ sơ giảng viên" /> : <TeacherProfile teacher={detail.data} />}</Card>
    </div>
  </div>
}

function TeacherProfile({ teacher }: { teacher: TeacherDetail }) {
  const classColumns: TableColumnsType<TeacherClass> = [{ title: 'Môn học', render: (_, row) => `${row.subjectCode} · ${row.subjectName}` }, { title: 'Lớp học phần', dataIndex: 'className' }, { title: 'Vai trò', render: (_, row) => row.isMainTeacher ? 'Giảng viên chính' : row.isMainTeacher === false ? 'Giảng viên phụ' : 'Chưa xác định' }, { title: 'Thời gian', render: (_, row) => `${dayjs(row.startDate).format('DD/MM/YYYY')} – ${dayjs(row.endDate).format('DD/MM/YYYY')}` }]
  const tabs = [
    { key: 'profile', label: 'Hồ sơ', children: <section className="teacher-info-grid"><span>Họ và tên<b>{teacher.fullName ?? 'Chưa xác định'}</b></span><span>Username<b>{teacher.userName ?? 'Chưa cung cấp'}</b></span><span>Khoa liên kết<b>{teacher.facultyName}</b></span><span>Vai trò<b>{teacher.isHeadOfFaculty ? 'Trưởng khoa' : 'Giảng viên'}</b></span><span>Trạng thái tài khoản<b>{teacher.isAccountActive == null ? 'Chưa xác định' : teacher.isAccountActive ? 'Hoạt động' : 'Không hoạt động'}</b></span><span>Mã liên kết<b className="mono">{teacher.teacherFacultyId}</b></span></section> },
    { key: 'classes', label: `Lớp phụ trách (${teacher.classCount})`, children: <Table size="small" rowKey="classId" columns={classColumns} dataSource={teacher.classes} pagination={{ pageSize: 5 }} scroll={{ x: 700 }} /> },
    { key: 'schedule', label: 'Lịch giảng', children: teacher.upcomingSchedule.length ? <div className="teacher-schedule-list">{teacher.upcomingSchedule.map((item) => <Link key={item.scheduleId} to={`/management/teaching/schedule?classId=${item.classId}`}><b>{dayjs(item.startDateTime).format('DD/MM HH:mm')} – {dayjs(item.endDateTime).format('HH:mm')}</b><span>{item.roomName ?? 'Chưa xếp phòng'}</span><span>{item.className} · {item.subjectName}</span><RightOutlined /></Link>)}</div> : <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Không có lịch sắp tới" /> },
  ]
  return <Space orientation="vertical" size="large" style={{ width: '100%' }}><div className="teacher-detail-heading"><Avatar size={72} src={teacher.profilePicUrl} icon={<UserOutlined />} /><div><Typography.Title level={3}>{teacher.fullName ?? 'Chưa xác định'}</Typography.Title><Typography.Text>{teacher.userName ?? 'Chưa có username'} · {teacher.facultyCode}</Typography.Text></div><Tag color={teacher.isAccountActive ? 'green' : 'default'}>{teacher.isAccountActive ? 'Hoạt động' : 'Chưa xác định'}</Tag></div><Tabs items={tabs} /><div className="teacher-detail-actions"><Link to={`/management/teaching/classes?teacherId=${teacher.teacherFacultyId}`}><Button type="primary">Xem tất cả lớp</Button></Link><Link to={`/management/teaching/schedule?teacherId=${teacher.teacherFacultyId}`}><Button>Xem lịch giảng</Button></Link></div></Space>
}
