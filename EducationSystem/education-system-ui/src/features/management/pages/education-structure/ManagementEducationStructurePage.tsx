import { useQuery } from '@tanstack/react-query'
import { BankOutlined, BookOutlined, CalendarOutlined, EyeOutlined, SearchOutlined, TeamOutlined, UserOutlined } from '@ant-design/icons'
import { Alert, Button, Card, Empty, Input, Select, Skeleton, Space, Table, Tabs, Tag, Typography, type TableColumnsType } from 'antd'
import dayjs from 'dayjs'
import { Link, useSearchParams } from 'react-router-dom'
import { managementApi, type AcademicYear, type Faculty, type Major } from '../../managementApi'

type StructureTab = 'faculties' | 'majors' | 'years'
const number = new Intl.NumberFormat('vi-VN')

export function ManagementEducationStructurePage() {
  const [params, setParams] = useSearchParams()
  const tab = (params.get('tab') as StructureTab) || 'faculties'
  const search = params.get('search') ?? ''
  const selectedId = params.get('id')
  const faculties = useQuery({ queryKey: ['management', 'faculties'], queryFn: managementApi.faculties })
  const majors = useQuery({ queryKey: ['management', 'majors'], queryFn: managementApi.majors })
  const years = useQuery({ queryKey: ['management', 'academic-years'], queryFn: managementApi.academicYears })
  const teachers = useQuery({ queryKey: ['management', 'teachers'], queryFn: managementApi.teachers })
  const dashboard = useQuery({ queryKey: ['management', 'dashboard'], queryFn: managementApi.dashboard })

  const update = (values: Record<string, string | undefined>) => {
    const next = new URLSearchParams(params)
    Object.entries(values).forEach(([key, value]) => value ? next.set(key, value) : next.delete(key))
    setParams(next)
  }

  const facultyItems = faculties.data?.items ?? []
  const majorItems = majors.data?.items ?? []
  const yearItems = years.data?.items ?? []
  const selectedFaculty = facultyItems.find((item) => item.id === selectedId)
  const normalize = (value: string) => value.toLocaleLowerCase('vi')
  const matches = <T extends { name: string; code?: string }>(item: T) => !search || normalize(`${item.code ?? ''} ${item.name}`).includes(normalize(search))
  const filteredFaculties = facultyItems.filter(matches)
  const filteredMajors = majorItems.filter(matches)
  const filteredYears = yearItems.filter((item) => !search || normalize(`${item.name} ${item.year}`).includes(normalize(search)))

  const facultyColumns: TableColumnsType<Faculty> = [
    { title: 'Mã khoa', dataIndex: 'code', width: 120, sorter: (a, b) => a.code.localeCompare(b.code) },
    { title: 'Tên khoa', dataIndex: 'name', render: (value) => <Space orientation="vertical" size={0}><b>{value}</b><Typography.Text type="success">● Đang hoạt động</Typography.Text></Space> },
    { title: 'Số ngành', align: 'center', render: (_, row) => majorItems.filter((major) => major.facultyId === row.id).length },
    { title: 'Sinh viên', align: 'center', render: (_, row) => number.format(dashboard.data?.studentsByFaculty.find((item) => item.name === row.name)?.count ?? 0) },
    { title: 'Giảng viên liên kết', align: 'center', render: (_, row) => teachers.data?.filter((teacher) => teacher.facultyCode === row.code).length ?? '—' },
    { title: 'Thao tác', width: 90, render: (_, row) => <Button aria-label={`Xem ${row.name}`} icon={<EyeOutlined />} onClick={() => update({ id: row.id })} /> },
  ]
  const majorColumns: TableColumnsType<Major> = [
    { title: 'Mã ngành', dataIndex: 'code', width: 140 },
    { title: 'Tên ngành', dataIndex: 'name', render: (value) => <b>{value}</b> },
    { title: 'Khoa', render: (_, row) => facultyItems.find((faculty) => faculty.id === row.facultyId)?.name ?? 'Chưa xác định' },
    { title: 'Mã loại đào tạo', dataIndex: 'trainingType', align: 'center', render: (value) => <Tag title="Database chưa có data dictionary ánh xạ mã loại đào tạo">Mã {value}</Tag> },
  ]
  const yearColumns: TableColumnsType<AcademicYear> = [
    { title: 'Năm học/Khóa', dataIndex: 'name', render: (value) => <b>{value}</b> },
    { title: 'Năm', dataIndex: 'year' },
    { title: 'Bắt đầu', dataIndex: 'startDate', render: (value) => dayjs(value).format('DD/MM/YYYY') },
    { title: 'Kết thúc', dataIndex: 'endDate', render: (value) => dayjs(value).format('DD/MM/YYYY') },
  ]

  if (faculties.isLoading || majors.isLoading || years.isLoading) return <Skeleton active paragraph={{ rows: 12 }} />
  if (faculties.isError || majors.isError || years.isError) return <Alert showIcon type="error" title="Không thể tải dữ liệu cơ cấu đào tạo" />

  const tabItems = [{ key: 'faculties', label: 'Khoa' }, { key: 'majors', label: 'Ngành' }, { key: 'years', label: 'Năm học/Khóa' }]

  return (
    <div className="management-page structure-page">
      <section className="structure-metrics">
        <Metric icon={<BankOutlined />} label="Tổng khoa" value={faculties.data?.totalItems ?? 0} tone="blue" />
        <Metric icon={<BookOutlined />} label="Tổng ngành" value={majors.data?.totalItems ?? 0} tone="green" />
        <Metric icon={<CalendarOutlined />} label="Năm học/Khóa" value={years.data?.totalItems ?? 0} tone="purple" />
        <Metric icon={<TeamOutlined />} label="Sinh viên" value={dashboard.data?.students ?? 0} tone="blue" />
        <Metric icon={<UserOutlined />} label="Giảng viên liên kết" value={dashboard.data?.teachers ?? 0} tone="orange" />
      </section>

      <div className="structure-grid">
        <Card className="dashboard-panel structure-list">
          <Tabs activeKey={tab} items={tabItems} onChange={(key) => update({ tab: key, id: undefined })} />
          <div className="structure-toolbar">
            <Input value={search} allowClear prefix={<SearchOutlined />} placeholder="Tìm kiếm mã hoặc tên..." onChange={(event) => update({ search: event.target.value || undefined })} />
            <Select defaultValue="active" options={[{ value: 'active', label: 'Đang hoạt động' }]} />
          </div>
          {tab === 'faculties' && <Table rowKey="id" columns={facultyColumns} dataSource={filteredFaculties} pagination={{ pageSize: 8, showSizeChanger: true }} onRow={(row) => ({ onClick: () => update({ id: row.id }) })} />}
          {tab === 'majors' && <Table rowKey="id" columns={majorColumns} dataSource={filteredMajors} pagination={{ pageSize: 8, showSizeChanger: true }} />}
          {tab === 'years' && <Table rowKey="id" columns={yearColumns} dataSource={filteredYears} pagination={{ pageSize: 8, showSizeChanger: true }} />}
        </Card>

        <Card className="dashboard-panel structure-detail" title="Chi tiết khoa">
          {!selectedFaculty ? <Empty description="Chọn một khoa để xem chi tiết" /> : <FacultyDetail faculty={selectedFaculty} majors={majorItems.filter((item) => item.facultyId === selectedFaculty.id)} teachers={teachers.data?.filter((item) => item.facultyCode === selectedFaculty.code) ?? []} />}
        </Card>
      </div>
    </div>
  )
}

function Metric({ icon, label, value, tone }: { icon: React.ReactNode; label: string; value: number; tone: string }) {
  return <Card bordered={false} className="metric-card"><div className={`metric-icon metric-icon--${tone}`}>{icon}</div><div className="metric-label">{label}</div><strong>{number.format(value)}</strong><small>Dữ liệu hiện tại</small></Card>
}

function FacultyDetail({ faculty, majors, teachers }: { faculty: Faculty; majors: Major[]; teachers: Awaited<ReturnType<typeof managementApi.teachers>> }) {
  return <Space orientation="vertical" size="large" style={{ width: '100%' }}>
    <div className="detail-heading"><span className="metric-icon metric-icon--blue"><BankOutlined /></span><div><Typography.Title level={4}>{faculty.name} <Tag color="blue">{faculty.code}</Tag></Typography.Title><Typography.Text type="success">● Đang hoạt động</Typography.Text></div></div>
    <section className="detail-section"><div className="detail-section-title">Ngành trực thuộc ({majors.length}) <Link to={`/management/education/structure?tab=majors&search=${encodeURIComponent(faculty.code)}`}>Xem tất cả</Link></div>{majors.length ? <div className="detail-chip-grid">{majors.slice(0, 6).map((major) => <div key={major.id}>{major.name}<small>{major.code}</small></div>)}</div> : <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Chưa có ngành liên kết" />}</section>
    <section className="detail-section"><div className="detail-section-title">Giảng viên liên kết ({teachers.length}) <Link to={`/management/people/teachers?search=${encodeURIComponent(faculty.code)}`}>Xem tất cả</Link></div>{teachers.slice(0, 5).map((teacher) => <div className="teacher-row" key={teacher.teacherFacultyId}><UserOutlined /><span>{teacher.fullName ?? 'Chưa có tên'}</span><small>{teacher.isHeadOfFaculty ? 'Trưởng khoa' : 'Giảng viên'}</small></div>)}</section>
    <Link className="detail-footer-link" to={`/management/education/subjects?facultyId=${faculty.id}`}>Xem môn học thuộc khoa</Link>
  </Space>
}
