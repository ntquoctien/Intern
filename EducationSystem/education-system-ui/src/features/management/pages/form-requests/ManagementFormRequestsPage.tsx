import { useQuery } from '@tanstack/react-query'
import { ClockCircleOutlined, CloseOutlined, DownloadOutlined, FileSearchOutlined, FileTextOutlined, FilterOutlined, LinkOutlined, PaperClipOutlined, SearchOutlined } from '@ant-design/icons'
import { Alert, Button, Card, Input, Pagination, Select, Skeleton, Space, Table, Tabs, Tag, Typography, type TableColumnsType } from 'antd'
import dayjs from 'dayjs'
import { useSearchParams } from 'react-router-dom'
import { managementApi, type ManagementFormRequest, type ManagementFormTemplate } from '../../managementApi'

const safeDocumentUrl = (value?: string | null) => value && /^https?:\/\//i.test(value) ? value : null

export function FormRequestsPage() {
  const [params, setParams] = useSearchParams()
  const tab = params.get('tab') ?? 'templates'
  const templateSearch = params.get('templateSearch') ?? ''
  const requestSearch = params.get('search') ?? ''
  const status = params.get('status') ?? ''
  const page = Number(params.get('page') ?? 1)
  const pageSize = Number(params.get('pageSize') ?? 10)
  const templates = useQuery({ queryKey: ['management', 'form-templates', templateSearch], queryFn: () => managementApi.formTemplates({ pageNumber: 1, pageSize: 100, search: templateSearch || undefined }) })
  const requests = useQuery({ queryKey: ['management', 'form-requests', page, pageSize, requestSearch, status], queryFn: () => managementApi.formRequests({ pageNumber: page, pageSize, search: requestSearch || undefined, status: status || undefined, sortBy: 'creationDate', sortDirection: 'desc' }) })
  const selectedId = params.get('id')
  const detail = useQuery({ queryKey: ['management', 'form-request', selectedId], queryFn: () => managementApi.formRequest(selectedId!), enabled: !!selectedId })
  const update = (values: Record<string, string | undefined>) => { const next = new URLSearchParams(params); Object.entries(values).forEach(([key, value]) => value ? next.set(key, value) : next.delete(key)); setParams(next) }
  const templateMap = new Map((templates.data?.items ?? []).map(item => [item.id, item]))
  const templateColumns: TableColumnsType<ManagementFormTemplate> = [
    { title: '#', width: 50, render: (_, __, index) => index + 1 },
    { title: 'Tên mẫu biểu', dataIndex: 'name' },
    { title: 'Tài liệu', render: (_, item) => safeDocumentUrl(item.documentUrl) ? <a href={item.documentUrl!} target="_blank" rel="noreferrer"><LinkOutlined /> Xem tài liệu</a> : 'Chưa có tài liệu' },
    { title: 'Trạng thái', render: () => <Tag color="green">Đang sử dụng</Tag> },
  ]
  const requestColumns: TableColumnsType<ManagementFormRequest> = [
    { title: 'Mã sinh viên', render: (_, item) => `…${item.studentId.slice(-8)}` },
    { title: 'Mẫu biểu', render: (_, item) => item.formTemplateId ? templateMap.get(item.formTemplateId)?.name ?? 'Mẫu biểu không còn trong danh mục' : 'Không liên kết mẫu' },
    { title: 'Ngày tạo', render: (_, item) => dayjs(item.creationDate).format('DD/MM/YYYY HH:mm') },
    { title: 'Ngày cập nhật', render: (_, item) => dayjs(item.updateDate).format('DD/MM/YYYY HH:mm') },
    { title: 'Trạng thái', render: (_, item) => <Tag color="blue">Trạng thái {item.status}</Tag> },
    { title: 'Người duyệt', dataIndex: 'approvalName', render: value => value || 'Chưa có' },
  ]
  return <div className="form-management-page">
    <div className="form-management-layout">
      <section className="form-management-main">
        <Card className="dashboard-panel form-tabs-card">
          <Tabs activeKey={tab} onChange={value => update({ tab: value })} items={[{ key: 'templates', label: <><FileTextOutlined /> Mẫu biểu</> }, { key: 'requests', label: <><FileSearchOutlined /> Yêu cầu</> }]} />
          <div className="form-section-heading"><Typography.Title level={4}>Danh sách mẫu biểu <Tag>{templates.data?.totalItems ?? 0}</Tag></Typography.Title><Space><Input allowClear prefix={<SearchOutlined />} value={templateSearch} placeholder="Tìm theo tên mẫu biểu…" onChange={event => update({ templateSearch: event.target.value || undefined })} /><Button icon={<FilterOutlined />}>Bộ lọc</Button></Space></div>
          {templates.isLoading ? <Skeleton active /> : templates.isError || !templates.data ? <Alert type="error" message="Không thể tải mẫu biểu" /> : <Table className="form-template-table" rowKey="id" columns={templateColumns} dataSource={templates.data.items} pagination={false} locale={{ emptyText: <FormEmpty icon={<FileTextOutlined />} title="Chưa có mẫu biểu nào" description="Hiện chưa có dữ liệu mẫu biểu trong hệ thống." /> }} />}
        </Card>
        <Card className="dashboard-panel form-requests-card">
          <div className="form-section-heading"><Typography.Title level={4}>Danh sách yêu cầu <Tag>{requests.data?.totalItems ?? 0}</Tag></Typography.Title><Space><Input allowClear prefix={<SearchOutlined />} value={requestSearch} placeholder="Tìm người duyệt, ghi chú…" onChange={event => update({ search: event.target.value || undefined, page: '1' })} /><Select allowClear value={status || undefined} placeholder="Trạng thái" options={[0, 1, 2, 3].map(value => ({ value: String(value), label: `Trạng thái ${value}` }))} onChange={value => update({ status: value, page: '1' })} /><Button icon={<DownloadOutlined />} onClick={() => exportRequests(requests.data?.items ?? [], templateMap)}>Export</Button></Space></div>
          {requests.isLoading ? <Skeleton active /> : requests.isError || !requests.data ? <Alert type="error" message="Không thể tải yêu cầu" /> : <><Table rowKey="id" columns={requestColumns} dataSource={requests.data.items} pagination={false} onRow={item => ({ onClick: () => update({ id: item.id }) })} rowClassName={item => item.id === selectedId ? 'selected-form-request-row' : ''} locale={{ emptyText: <FormEmpty icon={<FileSearchOutlined />} title="Chưa có yêu cầu nào" description="Hiện chưa có yêu cầu dịch vụ sinh viên nào." /> }} /><div className="form-pagination"><span>Hiển thị {requests.data.items.length} dòng / trang</span><Pagination current={page} pageSize={pageSize} total={requests.data.totalItems} showSizeChanger onChange={(next, size) => update({ page: String(next), pageSize: String(size) })} /></div></>}
        </Card>
      </section>
      <Card className="dashboard-panel form-request-detail" title="Chi tiết yêu cầu" extra={<Button type="text" icon={<CloseOutlined />} onClick={() => update({ id: undefined })} />}>{!selectedId ? <RequestEmpty /> : detail.isLoading ? <Skeleton active /> : detail.isError || !detail.data ? <Alert type="error" message="Không thể tải chi tiết yêu cầu" /> : <RequestDetail item={detail.data} template={detail.data.formTemplateId ? templateMap.get(detail.data.formTemplateId) : undefined} />}</Card>
    </div>
  </div>
}

function FormEmpty({ icon, title, description }: { icon: React.ReactNode; title: string; description: string }) { return <div className="form-empty"><span>{icon}</span><b>{title}</b><p>{description}</p></div> }
function RequestEmpty() { return <div className="request-empty-detail"><FormEmpty icon={<FileTextOutlined />} title="Chọn một yêu cầu" description="Vui lòng chọn một yêu cầu từ danh sách để xem chi tiết." /><InfoSkeleton /><Typography.Title level={5}>Tệp đính kèm</Typography.Title><FormEmpty icon={<PaperClipOutlined />} title="Chưa có tệp đính kèm" description="Schema hiện tại không có bảng tệp đính kèm cho yêu cầu." /><Typography.Title level={5}>Lịch sử xử lý</Typography.Title><FormEmpty icon={<ClockCircleOutlined />} title="Chưa có lịch sử" description="Schema hiện tại không lưu lịch sử xử lý riêng." /></div> }
function InfoSkeleton() { return <div className="form-info-grid"><span>Mã yêu cầu<b>—</b></span><span>Ngày cập nhật<b>—</b></span><span>Mẫu biểu<b>—</b></span><span>Người duyệt<b>—</b></span><span>Sinh viên<b>—</b></span><span>Approval name<b>—</b></span><span>Trạng thái<b>—</b></span><span>Ghi chú<b>—</b></span></div> }
function RequestDetail({ item, template }: { item: ManagementFormRequest; template?: ManagementFormTemplate }) { const url = safeDocumentUrl(template?.documentUrl); return <div className="request-detail-content"><div className="request-detail-heading"><span><FileTextOutlined /></span><div><Typography.Title level={3}>{template?.name ?? 'Yêu cầu dịch vụ sinh viên'}</Typography.Title><Tag color="blue">Trạng thái {item.status}</Tag><p>Mã yêu cầu: {item.id}</p></div></div><Typography.Title level={5}>Thông tin chung</Typography.Title><div className="form-info-grid"><span>Mã yêu cầu<b>{item.id}</b></span><span>Ngày cập nhật<b>{dayjs(item.updateDate).format('DD/MM/YYYY HH:mm')}</b></span><span>Mẫu biểu<b>{template?.name ?? 'Mẫu biểu không còn trong danh mục'}</b></span><span>Người duyệt<b>{item.approvalName || 'Chưa có'}</b></span><span>Sinh viên<b>…{item.studentId.slice(-8)}</b></span><span>Approval ID<b>{item.approvalId ? `…${item.approvalId.slice(-8)}` : 'Chưa có'}</b></span><span>Ngày tạo<b>{dayjs(item.creationDate).format('DD/MM/YYYY HH:mm')}</b></span><span>Ghi chú<b>{item.note || '—'}</b></span></div><Typography.Title level={5}>Tệp đính kèm</Typography.Title>{url ? <a className="form-document-link" href={url} target="_blank" rel="noreferrer"><PaperClipOutlined /> Xem tài liệu mẫu đã kiểm tra</a> : <FormEmpty icon={<PaperClipOutlined />} title="Chưa có tệp đính kèm" description="Không có tài liệu hợp lệ được liên kết với yêu cầu này." />}<Typography.Title level={5}>Lịch sử xử lý</Typography.Title><Alert type="info" showIcon message={`Tạo ${dayjs(item.creationDate).format('DD/MM/YYYY HH:mm')}; cập nhật ${dayjs(item.updateDate).format('DD/MM/YYYY HH:mm')}. Không dựng thêm timeline vì schema không có history.`} /></div> }
function exportRequests(items: ManagementFormRequest[], templates: Map<string, ManagementFormTemplate>) { const rows = [['Mã yêu cầu', 'Mẫu biểu', 'Sinh viên', 'Ngày tạo', 'Ngày cập nhật', 'Status raw', 'Người duyệt', 'Ghi chú'], ...items.map(item => [item.id, item.formTemplateId ? templates.get(item.formTemplateId)?.name ?? '' : '', item.studentId, item.creationDate, item.updateDate, item.status, item.approvalName, item.note])]; const blob = new Blob([`\uFEFF${rows.map(row => row.map(value => `"${String(value).replaceAll('"', '""')}"`).join(',')).join('\n')}`], { type: 'text/csv;charset=utf-8' }); const href = URL.createObjectURL(blob); const anchor = document.createElement('a'); anchor.href = href; anchor.download = 'yeu-cau-bieu-mau.csv'; anchor.click(); URL.revokeObjectURL(href) }
