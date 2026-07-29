import { useQuery } from '@tanstack/react-query'
import { AuditOutlined, DatabaseOutlined, FileProtectOutlined, LaptopOutlined, LockOutlined, ReloadOutlined, SafetyCertificateOutlined, SearchOutlined } from '@ant-design/icons'
import { Alert, Button, Card, Input, Skeleton, Statistic, Table, Tabs, Tag, type TableColumnsType } from 'antd'
import dayjs from 'dayjs'
import { managementApi, type SafeSystemSetting } from '../../managementApi'

const number = new Intl.NumberFormat('vi-VN')
const deviceColor = ['#58a5f5', '#246bea', '#20ad67', '#f5ae45', '#8d5ce5']

export function ManagementSystemPage() {
  const overview = useQuery({ queryKey: ['management', 'system-overview'], queryFn: managementApi.systemOverview })
  if (overview.isLoading) return <Skeleton active paragraph={{ rows: 15 }} />
  if (overview.isError || !overview.data) return <Alert type="error" showIcon title="Không thể tải tổng quan hệ thống" />
  const data = overview.data
  const settingColumns: TableColumnsType<SafeSystemSetting> = [{ title: 'Key', dataIndex: 'key' }, { title: 'Giá trị', render: (_, item) => <span className={item.isMasked ? 'masked-setting' : ''}>{item.displayValue}</span> }]
  return <div className="system-page">
    <OutcomeImportEntry />
    <Alert className="system-sensitive-alert" type="warning" showIcon icon={<SafetyCertificateOutlined />} message={<b>DỮ LIỆU NHẠY CẢM</b>} description="Khu vực hệ thống có thể chứa dữ liệu nhạy cảm. Chỉ sử dụng đúng mục đích và tuân thủ chính sách bảo mật." closable />
    <Tabs className="system-tabs" items={[{ key: 'overview', label: 'Tổng quan' }, { key: 'settings', label: 'Cấu hình' }, { key: 'audit', label: 'Audit' }, { key: 'devices', label: 'Thiết bị' }, { key: 'resets', label: 'Hỗ trợ reset mật khẩu' }]} />
    <Card className="dashboard-panel system-overview-card" title="Tổng quan hệ thống">
      <div className="system-summary-grid">
        <SystemSummary icon={<DatabaseOutlined />} title="Settings" value={data.settingCount} suffix="bản ghi" tone="green" />
        <SystemSummary icon={<AuditOutlined />} title="Audit Logs" value={data.auditCount} suffix="bản ghi" tone="purple" />
        <SystemSummary icon={<LaptopOutlined />} title="Thiết bị (User Devices)" value={data.deviceCount} suffix="thiết bị" tone="blue" />
        <SystemSummary icon={<LockOutlined />} title="Password Resets" value={data.resetRequestCount} suffix="bản ghi" tone="orange" />
      </div>
      <div className="system-dashboard-grid">
        <Card size="small" title="Cấu hình hệ thống" extra="Xem tất cả"><Input prefix={<SearchOutlined />} placeholder="Tìm theo key…" /><Table size="small" rowKey="settingId" columns={settingColumns} dataSource={data.settings.slice(0, 8)} pagination={false} /></Card>
        <Card size="small" title="Audit logs gần nhất" extra="Xem tất cả">{data.recentAudits.length ? <div className="system-audit-list">{data.recentAudits.map(item => <div key={item.auditId}><Tag>Action {item.rawAction}</Tag><span>{item.recordDescription || 'Không có mô tả'}</span><small>{dayjs(item.creationDate).format('DD/MM/YYYY HH:mm')}</small></div>)}</div> : <SystemEmpty icon={<AuditOutlined />} title="Chưa có nhật ký hoạt động" description="Dữ liệu audit sẽ xuất hiện khi hệ thống bắt đầu ghi nhận sự kiện." action={<Button icon={<ReloadOutlined />}>Làm mới</Button>} />}</Card>
        <Card size="small" title="Thiết bị theo loại" extra="Xem tất cả"><DeviceChart total={data.deviceCount} values={data.devicesByType} /></Card>
        <Card size="small" title="Yêu cầu reset mật khẩu" extra="Xem tất cả">{data.resetRequestCount === 0 ? <><Alert type="warning" showIcon message="PasswordResets hiện chưa có dữ liệu. Chỉ tạo reset mật khẩu khi có yêu cầu xác thực." /><SystemEmpty icon={<LockOutlined />} title="Chưa có yêu cầu" description="Không có yêu cầu reset mật khẩu nào trong hệ thống." /></> : <SystemEmpty icon={<LockOutlined />} title={`${data.resetRequestCount} yêu cầu nguồn`} description="Dữ liệu secret và thông tin định danh không được hiển thị." />}</Card>
      </div>
      <Alert className="system-security-note" type="info" showIcon icon={<SafetyCertificateOutlined />} message={<><b>Lưu ý bảo mật</b><ul><li>Không hiển thị password hash, password salt, reset secret, push identifier hoặc dữ liệu định danh đầy đủ.</li><li>Chỉ System Admin mới có quyền truy cập khu vực này.</li><li>Phase hiện tại chỉ đọc, không sửa cấu hình hoặc thực hiện reset.</li></ul></>} />
    </Card>
  </div>
}

function OutcomeImportEntry() {
  return <Card className="outcome-system-entry">
    <div><span><FileProtectOutlined /></span><div><b>Chuẩn đầu ra CLO/PLO</b><p>Import DOCX, theo dõi LLM phân tích và kiểm duyệt dữ liệu trước khi đưa vào bộ lọc CV.</p></div></div>
    <Button type="primary" href="/management/system/outcomes">Quản lý CLO/PLO</Button>
  </Card>
}

function SystemSummary({ icon, title, value, suffix, tone }: { icon: React.ReactNode; title: string; value: number; suffix: string; tone: string }) { return <Card className={`system-summary system-summary-${tone}`}><span>{icon}</span><div><Statistic title={title} value={value} formatter={value => number.format(Number(value))} /><small>{suffix}</small></div></Card> }
function SystemEmpty({ icon, title, description, action }: { icon: React.ReactNode; title: string; description: string; action?: React.ReactNode }) { return <div className="system-empty"><span>{icon}</span><b>{title}</b><p>{description}</p>{action}</div> }
function DeviceChart({ total, values }: { total: number; values: { rawDeviceType: number; count: number }[] }) {
  let offset = 0
  const segments = values.map((item, index) => { const start = offset; offset += total ? item.count / total * 100 : 0; return `${deviceColor[index % deviceColor.length]} ${start}% ${offset}%` })
  return <div className="device-chart-wrap"><div className="device-donut" style={{ background: `conic-gradient(${segments.join(',') || '#e7edf5 0 100%'})` }}><span><b>{number.format(total)}</b>thiết bị</span></div><div className="device-legend">{values.map((item, index) => <div key={item.rawDeviceType}><i style={{ background: deviceColor[index % deviceColor.length] }} /><span>Loại thiết bị {item.rawDeviceType}</span><b>{item.count} ({total ? (item.count / total * 100).toFixed(1) : 0}%)</b></div>)}</div></div>
}
