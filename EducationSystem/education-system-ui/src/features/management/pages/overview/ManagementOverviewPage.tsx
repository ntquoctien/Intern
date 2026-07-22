import { useQuery } from '@tanstack/react-query'
import {
  BankOutlined, BookOutlined, CalendarOutlined, CheckSquareOutlined,
  ClockCircleOutlined, RightOutlined, ScheduleOutlined, TeamOutlined, UserOutlined,
} from '@ant-design/icons'
import { Alert, Card, Col, Empty, Progress, Row, Skeleton, Space, Typography } from 'antd'
import { Link } from 'react-router-dom'
import { managementApi } from '../../managementApi'

const number = new Intl.NumberFormat('vi-VN')

const cards = [
  { key: 'students', label: 'Tổng sinh viên', icon: <TeamOutlined />, href: '/management/people/students', tone: 'blue' },
  { key: 'teachers', label: 'Giảng viên', icon: <UserOutlined />, href: '/management/people/teachers', tone: 'green' },
  { key: 'subjects', label: 'Môn học', icon: <BookOutlined />, href: '/management/education/subjects', tone: 'purple' },
  { key: 'classes', label: 'Lớp học phần', icon: <TeamOutlined />, href: '/management/teaching/classes', tone: 'orange' },
  { key: 'schedules', label: 'Lịch học', icon: <CalendarOutlined />, href: '/management/teaching/schedule', tone: 'blue' },
  { key: 'attendanceRecords', label: 'Bản ghi điểm danh', icon: <CheckSquareOutlined />, href: '/management/teaching/attendance', tone: 'green' },
] as const

export function ManagementOverviewPage() {
  const dashboard = useQuery({ queryKey: ['management', 'dashboard'], queryFn: managementApi.dashboard })
  const structure = useQuery({
    queryKey: ['management', 'overview-structure'],
    queryFn: async () => Promise.all([managementApi.faculties(), managementApi.majors()]),
  })

  if (dashboard.isLoading) return <Skeleton active paragraph={{ rows: 12 }} />
  if (dashboard.isError || !dashboard.data) return <Alert showIcon type="error" title="Không thể tải dữ liệu tổng quan" />

  const data = dashboard.data
  const facultyCount = structure.data?.[0].totalItems
  const majorCount = structure.data?.[1].totalItems
  const maxFaculty = Math.max(1, ...data.studentsByFaculty.map((item) => item.count))

  return (
    <div className="management-page overview-page">
      <section className="metric-grid">
        {cards.map((item) => (
          <Link className="metric-link" to={item.href} key={item.key}>
            <Card className="metric-card" bordered={false}>
              <div className={`metric-icon metric-icon--${item.tone}`}>{item.icon}</div>
              <div className="metric-label">{item.label}<RightOutlined /></div>
              <strong>{number.format(data[item.key])}</strong>
              <small>Dữ liệu hiện tại</small>
            </Card>
          </Link>
        ))}
      </section>

      <section className="overview-main-grid">
        <Card className="dashboard-panel" title="Cơ cấu sinh viên" extra={<Link to="/management/education/structure">Xem chi tiết</Link>}>
          {data.studentsByFaculty.length === 0 ? <Empty description="Chưa có dữ liệu phân bố" /> : (
            <div className="faculty-bars">
              {data.studentsByFaculty.map((item) => (
                <div className="faculty-bar" key={item.name}>
                  <span>{item.name}</span>
                  <Progress percent={Math.round(item.count / maxFaculty * 100)} showInfo={false} strokeColor="#2f6fed" />
                  <b>{number.format(item.count)}</b>
                </div>
              ))}
            </div>
          )}
        </Card>

        <Card className="dashboard-panel" title="Quy mô đào tạo" extra={<Typography.Text type="secondary">Dữ liệu hiện tại</Typography.Text>}>
          <Row gutter={[16, 16]}>
            <Col xs={24} sm={12}><div className="summary-tile"><BankOutlined /><span>Khoa</span><b>{facultyCount == null ? '—' : number.format(facultyCount)}</b></div></Col>
            <Col xs={24} sm={12}><div className="summary-tile"><BookOutlined /><span>Ngành</span><b>{majorCount == null ? '—' : number.format(majorCount)}</b></div></Col>
            <Col xs={24} sm={12}><div className="summary-tile"><CalendarOutlined /><span>Lịch học</span><b>{number.format(data.schedules)}</b></div></Col>
            <Col xs={24} sm={12}><div className="summary-tile"><ScheduleOutlined /><span>Điểm danh</span><b>{number.format(data.attendanceRecords)}</b></div></Col>
          </Row>
          <Alert className="history-note" showIcon icon={<ClockCircleOutlined />} type="info" title="Chưa có dữ liệu lịch sử để tính xu hướng theo tháng." />
        </Card>
      </section>

      <section className="overview-bottom-grid">
        <Card className="dashboard-panel" title="Cảnh báo chất lượng dữ liệu">
          <Space orientation="vertical" size="middle">
            <Alert showIcon type="warning" title="Các chỉ số tăng trưởng chưa được hiển thị" description="Backend hiện chỉ cung cấp dữ liệu tại thời điểm hiện tại, chưa có chuỗi dữ liệu lịch sử." />
            <Alert showIcon type="info" title="Chỉ số nghiệp vụ được giữ ở dạng thô" description="Không suy diễn GPA, tỷ lệ đạt hoặc tỷ lệ chuyên cần khi chưa có quy tắc xác nhận." />
          </Space>
        </Card>
        <Card className="dashboard-panel quick-access" title="Truy cập nhanh">
          {cards.slice(0, 6).map((item) => <Link to={item.href} key={item.key}><span className={`metric-icon metric-icon--${item.tone}`}>{item.icon}</span>{item.label}<RightOutlined /></Link>)}
        </Card>
      </section>
    </div>
  )
}
