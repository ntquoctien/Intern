import { useQuery } from '@tanstack/react-query'
import { CloseOutlined, DownloadOutlined, EyeOutlined, IdcardOutlined, SearchOutlined, TeamOutlined, UserOutlined } from '@ant-design/icons'
import { Alert, Avatar, Button, Card, Empty, Input, Select, Skeleton, Space, Table, Tag, Typography, type TableColumnsType } from 'antd'
import dayjs from 'dayjs'
import { Link, useSearchParams } from 'react-router-dom'
import { managementApi, type ManagementUser, type StudentSummary, type TeacherSummary } from '../../managementApi'
import { ROLE_MAP } from '../../../../shared/utils/enumMappers'

const number = new Intl.NumberFormat('vi-VN')
type LinkInfo = { kind: 'student' | 'teacher' | 'other'; student?: StudentSummary; teacher?: TeacherSummary }

export function ManagementUsersPage() {
  const [params, setParams] = useSearchParams()
  const users = useQuery({ queryKey: ['management', 'users'], queryFn: managementApi.users })
  const students = useQuery({ queryKey: ['management', 'students'], queryFn: managementApi.students })
  const teachers = useQuery({ queryKey: ['management', 'teachers'], queryFn: managementApi.teachers })
  const selectedId = params.get('id')
  const detail = useQuery({ queryKey: ['management', 'user', selectedId], queryFn: () => managementApi.user(selectedId!), enabled: !!selectedId })
  const update = (values: Record<string, string | undefined>) => { const next = new URLSearchParams(params); Object.entries(values).forEach(([key, value]) => value ? next.set(key, value) : next.delete(key)); if (!Object.hasOwn(values, 'page')) next.set('page', '1'); setParams(next) }

  if (users.isLoading || students.isLoading || teachers.isLoading) return <Skeleton active paragraph={{ rows: 15 }} />
  if (users.isError || students.isError || teachers.isError || !users.data || !students.data || !teachers.data) return <Alert showIcon type="error" title="Không thể tải danh sách tài khoản" />

  const links = new Map<string, LinkInfo>()
  students.data.forEach((student) => { if (student.userId) links.set(student.userId, { kind: 'student', student }) })
  teachers.data.forEach((teacher) => links.set(teacher.userId, { ...(links.get(teacher.userId) ?? { kind: 'teacher' }), kind: 'teacher', teacher }))
  const linkOf = (id: string) => links.get(id) ?? { kind: 'other' as const }
  const search = params.get('search') ?? ''; const role = params.get('role') ?? ''; const status = params.get('status') ?? ''; const linked = params.get('linked') ?? ''; const page = Number(params.get('page') ?? 1)
  const filtered = users.data.filter((user) => (!search || `${user.userName} ${user.fullName} ${user.userInternalId}`.toLocaleLowerCase('vi').includes(search.toLocaleLowerCase('vi'))) && (!role || String(user.role) === role) && (!status || String(user.isActive) === status) && (!linked || linkOf(user.id).kind === linked))
  const label = (user: ManagementUser) => ROLE_MAP[user.role]?.label ?? (linkOf(user.id).teacher ? 'Giảng viên' : linkOf(user.id).student ? 'Sinh viên' : `Vai trò ${user.role}`)
  const columns: TableColumnsType<ManagementUser> = [
    { title: 'Username', render: (_, row) => <Space><Avatar src={row.profilePicUrl} icon={<UserOutlined />} /><b>{row.userName}</b></Space> },
    { title: 'Họ và tên', dataIndex: 'fullName' }, { title: 'Mã nội bộ', dataIndex: 'userInternalId' },
    { title: 'Vai trò', render: (_, row) => <Tag color={linkOf(row.id).teacher ? 'blue' : linkOf(row.id).student ? 'purple' : 'default'}>{label(row)}</Tag> },
    { title: 'Trạng thái', render: (_, row) => <Tag color={row.isActive ? 'green' : 'orange'}>{row.isActive ? 'Hoạt động' : 'Tạm khóa'}</Tag> },
    { title: 'Loại liên kết', render: (_, row) => linkOf(row.id).teacher ? 'Giảng viên' : linkOf(row.id).student ? 'Sinh viên' : 'Khác' },
    { title: 'Điện thoại', dataIndex: 'maskedMobile' },
    { title: 'Thao tác', width: 80, render: (_, row) => <Button icon={<EyeOutlined />} aria-label={`Xem ${row.userName}`} onClick={() => update({ id: row.id, page: String(page) })} /> },
  ]
  const exportCsv = () => { const rows = [['Username', 'Họ tên', 'Mã nội bộ', 'Vai trò', 'Trạng thái', 'Liên kết'], ...filtered.map((item) => [item.userName, item.fullName, item.userInternalId, label(item), item.isActive ? 'Hoạt động' : 'Tạm khóa', linkOf(item.id).kind])]; const blob = new Blob([`\uFEFF${rows.map((row) => row.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(',')).join('\n')}`], { type: 'text/csv;charset=utf-8' }); const href = URL.createObjectURL(blob); const anchor = document.createElement('a'); anchor.href = href; anchor.download = 'danh-sach-tai-khoan.csv'; anchor.click(); URL.revokeObjectURL(href) }

  return <div className="management-page users-page">
    <div className="user-page-actions"><Typography.Text type="secondary">Tra cứu danh tính, trạng thái tài khoản và phạm vi liên kết.</Typography.Text><Button type="primary" icon={<DownloadOutlined />} onClick={exportCsv}>Xuất CSV</Button></div>
    <Card className="dashboard-panel user-filters"><Input value={search} allowClear prefix={<SearchOutlined />} placeholder="Tìm username, họ tên, mã nội bộ..." onChange={(event) => update({ search: event.target.value || undefined })} /><Select value={role || undefined} allowClear placeholder="Vai trò: Tất cả" options={[...new Set(users.data.map((item) => item.role))].sort().map((value) => ({ value: String(value), label: ROLE_MAP[value]?.label ?? `Vai trò ${value}` }))} onChange={(value) => update({ role: value })} /><Select value={status || undefined} allowClear placeholder="Trạng thái: Tất cả" options={[{ value: 'true', label: 'Hoạt động' }, { value: 'false', label: 'Tạm khóa' }]} onChange={(value) => update({ status: value })} /><Select value={linked || undefined} allowClear placeholder="Liên kết: Tất cả" options={[{ value: 'student', label: 'Sinh viên' }, { value: 'teacher', label: 'Giảng viên' }, { value: 'other', label: 'Khác' }]} onChange={(value) => update({ linked: value })} /><Button icon={<CloseOutlined />} onClick={() => setParams(new URLSearchParams())}>Xóa bộ lọc</Button></Card>
    <div className="users-grid"><Card className="dashboard-panel user-list" title={<span>Danh sách tài khoản <Tag>{number.format(filtered.length)}</Tag></span>}><Table rowKey="id" columns={columns} dataSource={filtered} scroll={{ x: 1050 }} pagination={{ current: page, pageSize: 10, total: filtered.length, showSizeChanger: true }} onChange={(pagination) => update({ page: String(pagination.current ?? 1) })} onRow={(row) => ({ onClick: () => update({ id: row.id, page: String(page) }) })} rowClassName={(row) => row.id === selectedId ? 'selected-user-row' : ''} /></Card><Card className="dashboard-panel user-detail" title="Chi tiết tài khoản">{!selectedId ? <Empty description="Chọn một tài khoản để xem chi tiết" /> : detail.isLoading ? <Skeleton active /> : detail.isError || !detail.data ? <Alert showIcon type="error" title="Không thể tải chi tiết tài khoản" /> : <UserProfile user={detail.data} link={linkOf(detail.data.id)} />}</Card></div>
  </div>
}

function UserProfile({ user, link }: { user: ManagementUser; link: LinkInfo }) {
  return <Space orientation="vertical" size="large" style={{ width: '100%' }}><div className="user-detail-heading"><Avatar size={76} src={user.profilePicUrl} icon={<UserOutlined />} /><div><Typography.Title level={3}>{user.fullName}</Typography.Title><Typography.Text>{user.userName} · {user.userInternalId}</Typography.Text><div><Tag color={ROLE_MAP[user.role]?.color ?? 'default'}>{ROLE_MAP[user.role]?.label ?? `Vai trò ${user.role}`}</Tag></div></div><Tag color={user.isActive ? 'green' : 'orange'}>{user.isActive ? 'Hoạt động' : 'Tạm khóa'}</Tag></div><section className="user-info-grid"><span><IdcardOutlined /> Số định danh<b>{user.maskedIdentificationNumber ?? 'Chưa cung cấp'}</b></span><span>Ngày cấp<b>{user.identificationDate ? dayjs(user.identificationDate).format('DD/MM/YYYY') : 'Chưa cung cấp'}</b></span><span>Ngày sinh<b>{user.birthDate ? dayjs(user.birthDate).format('DD/MM/YYYY') : 'Chưa cung cấp'}</b></span><span>Điện thoại<b>{user.maskedMobile ?? 'Chưa cung cấp'}</b></span><span>Đã đọc thông báo bắt buộc<b>{user.lastEnforceAnnouncementRead ? dayjs(user.lastEnforceAnnouncementRead).format('DD/MM/YYYY HH:mm') : 'Chưa xác nhận'}</b></span></section><Typography.Text type="secondary">Thông tin nhạy cảm được che tại API trước khi gửi tới trình duyệt.</Typography.Text>{link.student && <Card size="small" title={<><TeamOutlined /> Liên kết sinh viên</>}><div className="linked-profile"><span>Mã sinh viên<b>{link.student.studentCode}</b></span><span>Ngành<b>{link.student.majorName}</b></span><span>Khoa<b>{link.student.facultyName ?? 'Chưa xác định'}</b></span></div><Link to={`/management/people/students?id=${link.student.studentId}`}><Button>Xem hồ sơ sinh viên</Button></Link></Card>}{link.teacher && <Card size="small" title={<><UserOutlined /> Liên kết giảng viên</>}><div className="linked-profile"><span>Khoa / Bộ môn<b>{link.teacher.facultyName}</b></span><span>Chức danh<b>{link.teacher.isHeadOfFaculty ? 'Trưởng khoa' : 'Giảng viên'}</b></span></div><Link to={`/management/people/teachers?id=${link.teacher.teacherFacultyId}`}><Button>Xem hồ sơ giảng viên</Button></Link></Card>}</Space>
}
