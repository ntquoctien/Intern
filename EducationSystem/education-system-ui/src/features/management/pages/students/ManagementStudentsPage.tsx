import { useQuery } from '@tanstack/react-query'
import { CloseOutlined, DownloadOutlined, RightOutlined, SearchOutlined, UserOutlined, WarningOutlined } from '@ant-design/icons'
import { Alert, Avatar, Button, Card, Empty, Input, Select, Skeleton, Space, Table, Tabs, Tag, Typography, type TableColumnsType } from 'antd'
import { Link, useSearchParams } from 'react-router-dom'
import { managementApi, type StudentSummary } from '../../managementApi'

const number = new Intl.NumberFormat('vi-VN')

export function ManagementStudentsPage() {
  const [params, setParams] = useSearchParams()
  const students = useQuery({ queryKey: ['management', 'students'], queryFn: managementApi.students })
  const faculties = useQuery({ queryKey: ['management', 'faculties'], queryFn: managementApi.faculties })
  const majors = useQuery({ queryKey: ['management', 'majors'], queryFn: managementApi.majors })
  const selectedId = params.get('id')
  const detail = useQuery({ queryKey: ['management', 'student', selectedId], queryFn: () => managementApi.student(selectedId!), enabled: !!selectedId })

  const update = (values: Record<string, string | undefined>) => {
    const next = new URLSearchParams(params)
    Object.entries(values).forEach(([key, value]) => value ? next.set(key, value) : next.delete(key))
    if (!Object.hasOwn(values, 'page')) next.set('page', '1')
    setParams(next)
  }

  if (students.isLoading || faculties.isLoading || majors.isLoading) return <Skeleton active paragraph={{ rows: 15 }} />
  if (students.isError || faculties.isError || majors.isError || !students.data) return <Alert showIcon type="error" title="Không thể tải danh sách sinh viên" />

  const search = params.get('search') ?? ''
  const faculty = params.get('faculty') ?? ''
  const major = params.get('major') ?? ''
  const year = params.get('year') ?? ''
  const studyStatus = params.get('studyStatus') ?? ''
  const graduation = params.get('graduation') ?? ''
  const issue = params.get('issue') ?? ''
  const page = Number(params.get('page') ?? 1)
  const filtered = students.data.filter((student) => {
    const text = `${student.studentCode} ${student.fullName ?? ''} ${student.majorCode} ${student.majorName}`.toLocaleLowerCase('vi')
    return (!search || text.includes(search.toLocaleLowerCase('vi')))
      && (!faculty || student.facultyName === faculty)
      && (!major || student.majorCode === major)
      && (!year || student.academicYearName === year)
      && (!studyStatus || String(student.rawStudyStatus ?? '') === studyStatus)
      && (!graduation || String(student.isGraduated) === graduation)
      && (!issue || String(Boolean(student.hasIssue)) === issue)
  })

  const columns: TableColumnsType<StudentSummary> = [
    { title: 'Mã sinh viên', dataIndex: 'studentCode', width: 140, render: (value) => <span className="mono">{value}</span> },
    { title: 'Họ và tên', dataIndex: 'fullName', render: (value) => <b>{value ?? 'Chưa có tên'}</b> },
    { title: 'Khoa', dataIndex: 'facultyName', render: (value) => value ?? 'Chưa xác định' },
    { title: 'Ngành', render: (_, row) => <Space orientation="vertical" size={0}><span>{row.majorName}</span><Typography.Text type="secondary">{row.majorCode}</Typography.Text></Space> },
    { title: 'Khóa / Năm học', dataIndex: 'academicYearName' },
    { title: 'Trạng thái học', render: (_, row) => <Tag color={row.isGraduated ? 'blue' : 'green'}>{row.isGraduated ? 'Đã tốt nghiệp' : `Mã ${row.rawStudyStatus ?? '—'}`}</Tag> },
    { title: 'Có vấn đề', render: (_, row) => row.hasIssue ? <Tag color="warning" icon={<WarningOutlined />}>Có</Tag> : '—' },
    { title: '', width: 40, render: () => <RightOutlined /> },
  ]

  const exportCsv = () => {
    const rows = [['Mã sinh viên', 'Họ tên', 'Khoa', 'Mã ngành', 'Ngành', 'Năm học', 'Đã tốt nghiệp', 'Có vấn đề'], ...filtered.map((student) => [student.studentCode, student.fullName ?? '', student.facultyName ?? '', student.majorCode, student.majorName, student.academicYearName, student.isGraduated ? 'Có' : 'Không', student.hasIssue ? 'Có' : 'Không'])]
    const blob = new Blob([`\uFEFF${rows.map((row) => row.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(',')).join('\n')}`], { type: 'text/csv;charset=utf-8' })
    const href = URL.createObjectURL(blob); const anchor = document.createElement('a'); anchor.href = href; anchor.download = 'danh-sach-sinh-vien.csv'; anchor.click(); URL.revokeObjectURL(href)
  }

  return <div className="management-page students-page">
    <div className="student-page-actions"><Typography.Text type="secondary">Tìm kiếm và quản lý hồ sơ học tập của sinh viên theo quyền.</Typography.Text><Button type="primary" icon={<DownloadOutlined />} onClick={exportCsv}>Xuất CSV</Button></div>
    <Card className="dashboard-panel student-filters">
      <div className="student-filter-heading"><b>Bộ lọc</b><Button type="link" onClick={() => setParams(new URLSearchParams())} icon={<CloseOutlined />}>Xóa bộ lọc</Button></div>
      <Input value={search} allowClear prefix={<SearchOutlined />} placeholder="Tìm theo mã sinh viên, họ tên..." onChange={(event) => update({ search: event.target.value || undefined })} />
      <Select value={faculty || undefined} allowClear placeholder="Khoa: Tất cả" options={[...new Set(students.data.map((item) => item.facultyName).filter(Boolean))].map((value) => ({ value: value!, label: value! }))} onChange={(value) => update({ faculty: value })} />
      <Select value={major || undefined} allowClear placeholder="Ngành: Tất cả" options={(majors.data?.items ?? []).map((item) => ({ value: item.code, label: `${item.code} · ${item.name}` }))} onChange={(value) => update({ major: value })} />
      <Select value={year || undefined} allowClear placeholder="Khóa/Năm học: Tất cả" options={[...new Set(students.data.map((item) => item.academicYearName))].map((value) => ({ value, label: value }))} onChange={(value) => update({ year: value })} />
      <Select value={studyStatus || undefined} allowClear placeholder="Trạng thái học: Tất cả" options={[...new Set(students.data.map((item) => item.rawStudyStatus).filter((value) => value != null))].map((value) => ({ value: String(value), label: `Mã trạng thái ${value}` }))} onChange={(value) => update({ studyStatus: value })} />
      <Select value={graduation || undefined} allowClear placeholder="Tốt nghiệp: Tất cả" options={[{ value: 'true', label: 'Đã tốt nghiệp' }, { value: 'false', label: 'Chưa tốt nghiệp' }]} onChange={(value) => update({ graduation: value })} />
      <Select value={issue || undefined} allowClear placeholder="Vấn đề: Tất cả" options={[{ value: 'true', label: 'Có vấn đề' }, { value: 'false', label: 'Không có vấn đề' }]} onChange={(value) => update({ issue: value })} />
    </Card>
    <div className="students-grid">
      <Card className="dashboard-panel student-list" title={<span>Danh sách sinh viên <Tag>{number.format(filtered.length)}</Tag></span>}>
        <Table rowKey="studentId" columns={columns} dataSource={filtered} scroll={{ x: 1050 }} pagination={{ current: page, pageSize: 10, total: filtered.length, showSizeChanger: true }} onChange={(pagination) => update({ page: String(pagination.current ?? 1) })} onRow={(row) => ({ onClick: () => update({ id: row.studentId, page: String(page) }) })} rowClassName={(row) => row.studentId === selectedId ? 'selected-student-row' : ''} />
      </Card>
      <Card className="dashboard-panel student-detail" title="Hồ sơ sinh viên">
        {!selectedId ? <Empty description="Chọn một sinh viên để xem hồ sơ" /> : detail.isLoading ? <Skeleton active /> : detail.isError || !detail.data ? <Alert showIcon type="error" title="Không thể tải hồ sơ sinh viên" /> : <StudentDetail student={detail.data} />}
      </Card>
    </div>
  </div>
}

function StudentDetail({ student }: { student: StudentSummary }) {
  const tabs = [
    { key: 'overview', label: 'Tổng quan', children: <><section className="student-info-grid"><span>Mã sinh viên<b>{student.studentCode}</b></span><span>Trạng thái học<b>Mã {student.rawStudyStatus ?? '—'}</b></span><span>Khoa<b>{student.facultyName ?? 'Chưa xác định'}</b></span><span>Ngành<b>{student.majorName}</b></span><span>Khóa / Năm học<b>{student.academicYearName}</b></span><span>Tốt nghiệp<b>{student.isGraduated ? 'Có' : 'Chưa'}</b></span></section>{student.hasIssue && <Alert showIcon type="warning" title="Hồ sơ được đánh dấu có vấn đề" />}</> },
    { key: 'classes', label: 'Lớp học phần', children: <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Chọn xem chi tiết hồ sơ để tải lớp học phần" /> },
    { key: 'results', label: 'Kỳ thi / Kết quả', children: <CourseGpaRule /> },
  ]
  return <Space orientation="vertical" size="large" style={{ width: '100%' }}><div className="student-detail-heading"><Avatar size={72} icon={<UserOutlined />} /><div><Typography.Title level={3}>{student.fullName ?? 'Chưa có tên'}</Typography.Title><Typography.Text>{student.studentCode} · {student.majorCode}</Typography.Text></div><Tag color="green">Hồ sơ</Tag></div><Tabs items={tabs} /><div className="student-detail-actions"><Link to={`/management/teaching/schedule?studentId=${student.studentId}`}><Button>Xem lịch học</Button></Link><Link to={`/management/assessment/results?studentId=${student.studentId}`}><Button type="primary">Xem kết quả</Button></Link></div></Space>
}

function CourseGpaRule() {
  return <Alert type="info" showIcon title="Điều kiện tính điểm tổng kết môn" description={<div>Điểm môn = Chuyên cần × 10% + Kiểm tra × 40% + Thi × 50%.<br />Chỉ tính khi cả ba điểm thành phần đều tồn tại, thuộc cùng sinh viên và cùng môn, có giá trị hợp lệ từ 0 đến 10. Database hiện chưa có điểm chuyên cần và chưa xác định loại đánh giá kiểm tra.</div>} />
}
