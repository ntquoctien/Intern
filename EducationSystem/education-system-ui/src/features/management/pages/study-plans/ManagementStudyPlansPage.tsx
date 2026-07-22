import { useQuery } from '@tanstack/react-query'
import { BookOutlined, CalendarOutlined, CheckCircleOutlined, CloseOutlined, DownloadOutlined, FilterOutlined, RightOutlined, SearchOutlined } from '@ant-design/icons'
import { Alert, Button, Card, Empty, Input, Select, Skeleton, Space, Table, Tag, Typography, type TableColumnsType } from 'antd'
import dayjs from 'dayjs'
import { Link, useSearchParams } from 'react-router-dom'
import { managementApi, type PlanDetail, type PlanSummary, type PlanSubject } from '../../managementApi'

const number = new Intl.NumberFormat('vi-VN')

export function ManagementStudyPlansPage() {
  const [params, setParams] = useSearchParams()
  const plans = useQuery({ queryKey: ['management', 'plans'], queryFn: managementApi.plans })
  const faculties = useQuery({ queryKey: ['management', 'faculties'], queryFn: managementApi.faculties })
  const majors = useQuery({ queryKey: ['management', 'majors'], queryFn: managementApi.majors })
  const years = useQuery({ queryKey: ['management', 'academic-years'], queryFn: managementApi.academicYears })

  const update = (values: Record<string, string | undefined>) => {
    const next = new URLSearchParams(params)
    Object.entries(values).forEach(([key, value]) => value ? next.set(key, value) : next.delete(key))
    if (!Object.hasOwn(values, 'page')) next.set('page', '1')
    setParams(next)
  }

  const planItems = plans.data ?? []
  const majorItems = majors.data?.items ?? []
  const facultyItems = faculties.data?.items ?? []
  const search = params.get('search') ?? ''
  const facultyId = params.get('facultyId') ?? ''
  const majorCode = params.get('major') ?? ''
  const academicYear = params.get('year') ?? ''
  const semester = params.get('semester') ?? ''
  const status = params.get('status') ?? ''
  const subjectId = params.get('subjectId') ?? ''
  const page = Number(params.get('page') ?? 1)
  const selectedId = params.get('id')

  const getMajor = (plan: PlanSummary) => majorItems.find((item) => item.code === plan.majorCode)
  const getFaculty = (plan: PlanSummary) => facultyItems.find((item) => item.id === getMajor(plan)?.facultyId)
  const filtered = planItems.filter((plan) => {
    const text = `${plan.majorCode} ${plan.majorName}`.toLocaleLowerCase('vi')
    return (!search || text.includes(search.toLocaleLowerCase('vi')))
      && (!facultyId || getFaculty(plan)?.id === facultyId)
      && (!majorCode || plan.majorCode === majorCode)
      && (!academicYear || plan.academicYearName === academicYear)
      && (!semester || String(plan.semester) === semester)
      && (!status || String(plan.isActive) === status)
      && (!subjectId || plan.subjectIds?.includes(subjectId))
  })
  const selected = filtered.find((item) => item.planId === selectedId) ?? planItems.find((item) => item.planId === selectedId) ?? filtered[0]
  const detail = useQuery({ queryKey: ['management', 'plan', selected?.planId], queryFn: () => managementApi.plan(selected!.planId), enabled: !!selected })

  if (plans.isLoading || faculties.isLoading || majors.isLoading || years.isLoading) return <Skeleton active paragraph={{ rows: 14 }} />
  if (plans.isError || faculties.isError || majors.isError || years.isError || !plans.data) return <Alert showIcon type="error" title="Không thể tải kế hoạch đào tạo" />

  const columns: TableColumnsType<PlanSummary> = [
    { title: 'Ngành', render: (_, row) => <Space orientation="vertical" size={0}><b>{row.majorCode}</b><Typography.Text type="secondary">{row.majorName}</Typography.Text></Space> },
    { title: 'Khoa', render: (_, row) => getFaculty(row)?.code ?? 'Chưa xác định' },
    { title: 'Khóa / Năm học', dataIndex: 'academicYearName' },
    { title: 'Học kỳ', dataIndex: 'semester', align: 'center', render: (value) => `HK ${value}` },
    { title: 'Thời gian', render: (_, row) => <>{dayjs(row.startDate).format('DD/MM/YYYY')}<br />– {dayjs(row.endDate).format('DD/MM/YYYY')}</> },
    { title: 'Số môn', dataIndex: 'subjectCount', align: 'center' },
    { title: 'Trạng thái', render: (_, row) => <Tag color={row.isActive ? 'green' : 'default'}>{row.isActive ? 'Đang áp dụng' : 'Không hoạt động'}</Tag> },
    { title: '', width: 44, render: () => <RightOutlined /> },
  ]

  const exportCsv = () => {
    const rows = [['Ngành', 'Tên ngành', 'Năm học', 'Học kỳ', 'Ngày bắt đầu', 'Ngày kết thúc', 'Số môn', 'Đang áp dụng'], ...filtered.map((plan) => [plan.majorCode, plan.majorName, plan.academicYearName, plan.semester, plan.startDate, plan.endDate, plan.subjectCount, plan.isActive ? 'Có' : 'Không'])]
    const blob = new Blob([`\uFEFF${rows.map((row) => row.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(',')).join('\n')}`], { type: 'text/csv;charset=utf-8' })
    const href = URL.createObjectURL(blob)
    const anchor = document.createElement('a'); anchor.href = href; anchor.download = 'ke-hoach-dao-tao.csv'; anchor.click(); URL.revokeObjectURL(href)
  }

  return <div className="management-page study-plans-page">
    <div className="study-page-actions"><Typography.Text type="secondary">Xem kế hoạch học kỳ theo ngành và khóa/năm học.</Typography.Text><Button type="primary" icon={<DownloadOutlined />} onClick={exportCsv}>Xuất CSV</Button></div>
    <Card className="dashboard-panel study-filters">
      <label>Khoa<Select value={facultyId || undefined} allowClear placeholder="Tất cả" options={facultyItems.map((item) => ({ value: item.id, label: `${item.code} · ${item.name}` }))} onChange={(value) => update({ facultyId: value })} /></label>
      <label>Ngành<Select value={majorCode || undefined} allowClear showSearch placeholder="Tất cả" options={majorItems.filter((item) => !facultyId || item.facultyId === facultyId).map((item) => ({ value: item.code, label: `${item.code} · ${item.name}` }))} onChange={(value) => update({ major: value })} /></label>
      <label>Khóa / Năm học<Select value={academicYear || undefined} allowClear placeholder="Tất cả" options={[...new Set(planItems.map((item) => item.academicYearName))].map((value) => ({ value, label: value }))} onChange={(value) => update({ year: value })} /></label>
      <label>Học kỳ<Select value={semester || undefined} allowClear placeholder="Tất cả" options={[...new Set(planItems.map((item) => item.semester))].sort().map((value) => ({ value: String(value), label: `Học kỳ ${value}` }))} onChange={(value) => update({ semester: value })} /></label>
      <label>Trạng thái<Select value={status || undefined} allowClear placeholder="Tất cả" options={[{ value: 'true', label: 'Đang áp dụng' }, { value: 'false', label: 'Không hoạt động' }]} onChange={(value) => update({ status: value })} /></label>
      <Input value={search} allowClear prefix={<SearchOutlined />} placeholder="Tìm mã hoặc tên ngành..." onChange={(event) => update({ search: event.target.value || undefined })} />
      <Button icon={<FilterOutlined />}>Bộ lọc khác</Button>
      <Button type="link" icon={<CloseOutlined />} onClick={() => setParams(new URLSearchParams())}>Xóa bộ lọc</Button>
    </Card>

    <div className="study-grid">
      <Card className="dashboard-panel study-list" title={<span>Danh sách kế hoạch học kỳ <Tag>{number.format(filtered.length)}</Tag></span>}>
        <Table rowKey="planId" columns={columns} dataSource={filtered} scroll={{ x: 900 }} pagination={{ current: page, pageSize: 10, total: filtered.length, showSizeChanger: true }} onChange={(pagination) => update({ page: String(pagination.current ?? 1) })} onRow={(row) => ({ onClick: () => update({ id: row.planId, page: String(page) }) })} rowClassName={(row) => row.planId === selected?.planId ? 'selected-plan-row' : ''} />
      </Card>
      <Card className="dashboard-panel study-detail" title="Chi tiết kế hoạch học kỳ">
        {selected ? detail.isLoading ? <Skeleton active /> : detail.isError || !detail.data ? <Alert showIcon type="error" title="Không thể tải chi tiết kế hoạch" /> : <PlanDetailPanel plan={detail.data} /> : <Empty description="Không có kế hoạch phù hợp" />}
      </Card>
    </div>
  </div>
}

function PlanDetailPanel({ plan }: { plan: PlanDetail }) {
  const subjectColumns: TableColumnsType<PlanSubject> = [
    { title: 'Mã môn trong kế hoạch', dataIndex: 'snapshotCode', render: (value) => <span className="mono">{value}</span> },
    { title: 'Tên môn trong kế hoạch', dataIndex: 'snapshotName' },
    { title: 'Tín chỉ', dataIndex: 'snapshotCreditPoint', align: 'center' },
    { title: 'Môn danh mục', render: (_, row) => row.subjectId ? <Link to={`/management/education/subjects?search=${encodeURIComponent(row.catalogCode ?? row.snapshotCode)}`}>{row.catalogCode ?? 'Không còn hoạt động'}</Link> : <Typography.Text type="danger">Không có SubjectId</Typography.Text> },
    { title: 'Đối chiếu', render: (_, row) => <Tag color={row.catalogMatch ? 'green' : 'red'}>{row.catalogMatch ? 'Khớp' : 'Không khớp'}</Tag> },
  ]
  return <Space orientation="vertical" size="large" style={{ width: '100%' }}>
    <div className="plan-detail-heading"><span className="metric-icon metric-icon--purple"><CalendarOutlined /></span><div><Typography.Title level={4}>Kế hoạch HK {plan.semester} · {plan.academicYearName}</Typography.Title><Typography.Text>{plan.majorCode} · {plan.majorName}</Typography.Text></div><Tag color={plan.isActive ? 'green' : 'default'}>{plan.isActive ? 'Đang áp dụng' : 'Không hoạt động'}</Tag></div>
    <section className="plan-info-grid"><span>Mã kế hoạch<b className="mono">{plan.planId}</b></span><span>Ngành<b>{plan.majorName}</b></span><span>Khoa<b>{plan.facultyName ?? 'Chưa xác định'}</b></span><span>Học kỳ<b>HK {plan.semester}</b></span><span>Số môn trong kế hoạch<b>{number.format(plan.subjectCount)}</b></span><span>Tổng tín chỉ<b>{number.format(plan.totalCreditPoints)}</b></span></section>
    <section className="semester-timeline"><div><CheckCircleOutlined /> Bắt đầu<b>{dayjs(plan.startDate).format('DD/MM/YYYY')}</b></div><i /><div>Kết thúc <CheckCircleOutlined /><b>{dayjs(plan.endDate).format('DD/MM/YYYY')}</b></div></section>
    <section className="plan-subjects"><h4>Danh sách môn trong kế hoạch ({plan.subjectCount})</h4><Table size="small" rowKey="semesterSubjectId" columns={subjectColumns} dataSource={plan.subjects} pagination={{ pageSize: 8 }} scroll={{ x: 720 }} /></section>
    <div className="plan-detail-actions"><Link to={`/management/teaching/classes?search=${encodeURIComponent(plan.majorCode)}`}><Button icon={<CalendarOutlined />}>Tra cứu lớp học phần</Button></Link><Link to="/management/education/subjects"><Button icon={<BookOutlined />}>Xem danh mục môn</Button></Link></div>
  </Space>
}
