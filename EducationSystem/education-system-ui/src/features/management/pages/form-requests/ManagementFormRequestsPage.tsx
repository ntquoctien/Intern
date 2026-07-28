import {
  BankOutlined,
  CheckCircleOutlined,
  CloseOutlined,
  DownloadOutlined,
  FileSearchOutlined,
  FileTextOutlined,
  LinkOutlined,
  SafetyCertificateOutlined,
  SearchOutlined,
  UserOutlined,
} from '@ant-design/icons'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  Alert,
  Button,
  Card,
  Descriptions,
  Input,
  message,
  Modal,
  Pagination,
  Select,
  Skeleton,
  Space,
  Table,
  Tabs,
  Tag,
  Typography,
  type TableColumnsType,
} from 'antd'
import dayjs from 'dayjs'
import { useSearchParams } from 'react-router-dom'
import { managementApi, type ManagementFormRequest, type ManagementFormTemplate } from '../../managementApi'

const safeDocumentUrl = (value?: string | null) =>
  value && /^https?:\/\//i.test(value) ? value : null

const workflowStatus = (item: ManagementFormRequest) => {
  if (item.status === 2) return { color: 'green', label: 'Đã phê duyệt' }
  if (item.employerVerifiedStatus === 2) return { color: 'red', label: 'Doanh nghiệp từ chối' }
  if (item.employerVerifiedStatus === 1) return { color: 'blue', label: 'Chờ nhà trường duyệt' }
  return { color: 'orange', label: 'Chờ doanh nghiệp xác thực' }
}

export function FormRequestsPage() {
  const [params, setParams] = useSearchParams()
  const queryClient = useQueryClient()
  const [messageApi, messageContext] = message.useMessage()
  const [modal, modalContext] = Modal.useModal()
  const tab = params.get('tab') ?? 'requests'
  const templateSearch = params.get('templateSearch') ?? ''
  const requestSearch = params.get('search') ?? ''
  const status = params.get('status') ?? ''
  const page = Number(params.get('page') ?? 1)
  const pageSize = Number(params.get('pageSize') ?? 10)
  const selectedId = params.get('id')

  const templates = useQuery({
    queryKey: ['management', 'form-templates', templateSearch],
    queryFn: () => managementApi.formTemplates({
      pageNumber: 1,
      pageSize: 100,
      search: templateSearch || undefined,
    }),
    enabled: tab === 'templates',
  })
  const requests = useQuery({
    queryKey: ['management', 'form-requests', page, pageSize, requestSearch, status],
    queryFn: () => managementApi.formRequests({
      pageNumber: page,
      pageSize,
      search: requestSearch || undefined,
      status: status || undefined,
      sortBy: 'creationDate',
      sortDirection: 'desc',
    }),
  })
  const detail = useQuery({
    queryKey: ['management', 'form-request', selectedId],
    queryFn: () => managementApi.formRequest(selectedId!),
    enabled: !!selectedId,
  })
  const approve = useMutation({
    mutationFn: managementApi.approveInternship,
    onSuccess: async () => {
      messageApi.success('Đã phê duyệt và đồng bộ kỳ thực tập vào hồ sơ học vụ.')
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['management', 'form-requests'] }),
        queryClient.invalidateQueries({ queryKey: ['management', 'form-request', selectedId] }),
      ])
    },
    onError: () => {
      messageApi.error('Không thể phê duyệt. Hãy kiểm tra AcademicService và khóa InternalApi.')
    },
  })

  const update = (values: Record<string, string | undefined>) => {
    const next = new URLSearchParams(params)
    Object.entries(values).forEach(([key, value]) =>
      value ? next.set(key, value) : next.delete(key))
    setParams(next)
  }

  const confirmApproval = (item: ManagementFormRequest) => {
    modal.confirm({
      title: 'Phê duyệt thực tập',
      content: `Xác nhận duyệt kỳ thực tập của ${item.studentName || item.studentCode} tại ${item.companyName}? Dữ liệu sẽ được đồng bộ sang hồ sơ học vụ.`,
      okText: 'Phê duyệt & đồng bộ',
      cancelText: 'Hủy',
      okButtonProps: { style: { background: '#002140' } },
      onOk: async () => {
        try {
          await approve.mutateAsync(item.id)
        } catch {
          // useMutation.onError already presents the user-facing error.
        }
      },
    })
  }

  const templateColumns: TableColumnsType<ManagementFormTemplate> = [
    { title: '#', width: 50, render: (_, __, index) => index + 1 },
    { title: 'Tên mẫu biểu', dataIndex: 'name' },
    {
      title: 'Tài liệu',
      render: (_, item) => safeDocumentUrl(item.documentUrl)
        ? <a href={item.documentUrl!} target="_blank" rel="noreferrer"><LinkOutlined /> Xem tài liệu</a>
        : 'Chưa có tài liệu',
    },
    { title: 'Trạng thái', render: () => <Tag color="green">Đang sử dụng</Tag> },
  ]
  const requestColumns: TableColumnsType<ManagementFormRequest> = [
    {
      title: 'Sinh viên',
      width: 180,
      render: (_, item) => (
        <div className="management-student-cell">
          <b>{item.studentName || 'Chưa có họ tên'}</b>
          <span>{item.studentCode || `…${item.studentId.slice(-8)}`}</span>
        </div>
      ),
    },
    {
      title: 'Loại yêu cầu',
      width: 220,
      render: (_, item) => (
        <div className="management-student-cell">
          <b>{item.requestType || 'Yêu cầu dịch vụ sinh viên'}</b>
          {item.companyName && <span>{item.companyName} · {item.position}</span>}
        </div>
      ),
    },
    { title: 'Ngày gửi', width: 145, render: (_, item) => dayjs(item.creationDate).format('DD/MM/YYYY HH:mm') },
    {
      title: 'Tiến trình',
      width: 190,
      render: (_, item) => {
        const state = workflowStatus(item)
        return <Tag color={state.color}>{state.label}</Tag>
      },
    },
    { title: 'Người duyệt', width: 130, dataIndex: 'approvalName', render: value => value || 'Chưa có' },
  ]

  return (
    <div className="form-management-page">
      {messageContext}
      {modalContext}
      <div className="form-management-layout">
        <section className="form-management-main">
          <Card className="dashboard-panel form-tabs-card">
            <Tabs
              activeKey={tab}
              onChange={value => update({ tab: value, id: undefined })}
              items={[
                { key: 'requests', label: <><FileSearchOutlined /> Yêu cầu</> },
                { key: 'templates', label: <><FileTextOutlined /> Mẫu biểu</> },
              ]}
            />
            {tab === 'templates' ? (
              <>
                <div className="form-section-heading">
                  <Typography.Title level={4}>Danh sách mẫu biểu <Tag>{templates.data?.totalItems ?? 0}</Tag></Typography.Title>
                  <Input
                    allowClear
                    prefix={<SearchOutlined />}
                    value={templateSearch}
                    placeholder="Tìm theo tên mẫu biểu…"
                    onChange={event => update({ templateSearch: event.target.value || undefined })}
                  />
                </div>
                {templates.isLoading
                  ? <Skeleton active />
                  : templates.isError || !templates.data
                    ? <Alert type="error" title="Không thể tải mẫu biểu" />
                    : <Table
                        className="form-template-table"
                        rowKey="id"
                        columns={templateColumns}
                        dataSource={templates.data.items}
                        pagination={false}
                        locale={{ emptyText: <FormEmpty icon={<FileTextOutlined />} title="Chưa có mẫu biểu nào" description="Danh mục mẫu biểu hiện chưa có dữ liệu." /> }}
                      />}
              </>
            ) : (
              <>
                <div className="form-section-heading">
                  <Typography.Title level={4}>Danh sách yêu cầu <Tag>{requests.data?.totalItems ?? 0}</Tag></Typography.Title>
                  <Space wrap>
                    <Input
                      allowClear
                      prefix={<SearchOutlined />}
                      value={requestSearch}
                      placeholder="Tìm người duyệt, ghi chú…"
                      onChange={event => update({ search: event.target.value || undefined, page: '1' })}
                    />
                    <Select
                      allowClear
                      value={status || undefined}
                      placeholder="Tiến trình"
                      options={[
                        { value: '0', label: 'Chờ doanh nghiệp xác thực' },
                        { value: '1', label: 'Chờ nhà trường duyệt' },
                        { value: '2', label: 'Đã phê duyệt' },
                        { value: '3', label: 'Doanh nghiệp từ chối' },
                      ]}
                      onChange={value => update({ status: value, page: '1' })}
                    />
                    <Button icon={<DownloadOutlined />} onClick={() => exportRequests(requests.data?.items ?? [])}>Export</Button>
                  </Space>
                </div>
                {requests.isLoading
                  ? <Skeleton active />
                  : requests.isError || !requests.data
                    ? <Alert type="error" title="Không thể tải yêu cầu" />
                    : (
                      <>
                        <Table
                          rowKey="id"
                          columns={requestColumns}
                          dataSource={requests.data.items}
                          pagination={false}
                          onRow={item => ({ onClick: () => update({ id: item.id }) })}
                          rowClassName={item => item.id === selectedId ? 'selected-form-request-row' : ''}
                          locale={{ emptyText: <FormEmpty icon={<FileSearchOutlined />} title="Chưa có yêu cầu nào" description="Hiện chưa có yêu cầu dịch vụ sinh viên phù hợp bộ lọc." /> }}
                        />
                        <div className="form-pagination">
                          <span>Hiển thị {requests.data.items.length} dòng / trang</span>
                          <Pagination
                            current={page}
                            pageSize={pageSize}
                            total={requests.data.totalItems}
                            showSizeChanger
                            onChange={(next, size) => update({ page: String(next), pageSize: String(size) })}
                          />
                        </div>
                      </>
                    )}
              </>
            )}
          </Card>
        </section>
        <Card
          className="dashboard-panel form-request-detail"
          title="Chi tiết yêu cầu"
          extra={<Button type="text" icon={<CloseOutlined />} onClick={() => update({ id: undefined })} />}
        >
          {!selectedId
            ? <RequestEmpty />
            : detail.isLoading
              ? <Skeleton active />
              : detail.isError || !detail.data
                ? <Alert type="error" title="Không thể tải chi tiết yêu cầu" />
                : <RequestDetail item={detail.data} approving={approve.isPending} onApprove={() => confirmApproval(detail.data!)} />}
        </Card>
      </div>
    </div>
  )
}

function FormEmpty({ icon, title, description }: { icon: React.ReactNode; title: string; description: string }) {
  return <div className="form-empty"><span>{icon}</span><b>{title}</b><p>{description}</p></div>
}

function RequestEmpty() {
  return (
    <div className="request-empty-detail">
      <FormEmpty icon={<FileTextOutlined />} title="Chọn một yêu cầu" description="Chọn yêu cầu từ danh sách để xem thông tin xác thực và phê duyệt." />
    </div>
  )
}

function RequestDetail({
  item,
  approving,
  onApprove,
}: {
  item: ManagementFormRequest
  approving: boolean
  onApprove: () => void
}) {
  const state = workflowStatus(item)
  const canApprove = item.employerVerifiedStatus === 1 && item.status !== 2
  // Backward-compatible fallback: older CommunicationService DTOs do not expose
  // companyName/requestType yet, but a non-zero employer verification state can
  // only belong to the internship workflow.
  const isInternship =
    !!item.companyName ||
    item.employerVerifiedStatus > 0 ||
    item.requestType?.toLocaleLowerCase('vi').includes('thực tập')
  return (
    <div className="request-detail-content">
      <div className="request-detail-heading">
        <span><FileTextOutlined /></span>
        <div>
          <Typography.Title level={3}>{item.requestType || 'Yêu cầu dịch vụ sinh viên'}</Typography.Title>
          <Tag color={state.color}>{state.label}</Tag>
          <p>Mã yêu cầu: {item.id}</p>
        </div>
      </div>

      <Typography.Title level={5}>Thông tin sinh viên</Typography.Title>
      <Descriptions column={1} size="small" bordered>
        <Descriptions.Item label={<><UserOutlined /> Họ tên</>}>{item.studentName || 'Chưa có dữ liệu'}</Descriptions.Item>
        <Descriptions.Item label="MSSV">{item.studentCode || `…${item.studentId.slice(-8)}`}</Descriptions.Item>
        <Descriptions.Item label="Ngày gửi">{dayjs(item.creationDate).format('DD/MM/YYYY HH:mm')}</Descriptions.Item>
      </Descriptions>

      {isInternship && (
        <>
          <Typography.Title level={5}>Thông tin thực tập</Typography.Title>
          <Descriptions column={1} size="small" bordered>
            <Descriptions.Item label={<><BankOutlined /> Doanh nghiệp</>}>{item.companyName || item.note?.split('·')[0]?.trim() || 'Chưa tải được dữ liệu'}</Descriptions.Item>
            <Descriptions.Item label="Vị trí">{item.position || '—'}</Descriptions.Item>
            <Descriptions.Item label="Thời gian">
              {item.startDate ? dayjs(item.startDate).format('DD/MM/YYYY') : '—'} – {item.endDate ? dayjs(item.endDate).format('DD/MM/YYYY') : 'Hiện tại'}
            </Descriptions.Item>
            <Descriptions.Item label="Email Mentor">{item.mentorEmail || '—'}</Descriptions.Item>
            <Descriptions.Item label="Nhiệm vụ"><span className="pre-line-text">{item.taskDescription || '—'}</span></Descriptions.Item>
          </Descriptions>

          <Typography.Title level={5}>Xác thực của doanh nghiệp</Typography.Title>
          {item.employerVerifiedStatus === 1 ? (
            <Alert
              type="success"
              showIcon
              icon={<SafetyCertificateOutlined />}
              title="Doanh nghiệp đã xác nhận thông tin"
              description={
                <div>
                  <p>Điểm đánh giá: <b>{item.employerScore ?? 'Không chấm'}</b></p>
                  <p>Nhận xét: {item.employerEvaluationNotes || 'Không có nhận xét'}</p>
                  {item.employerVerifiedAt && <p>Xác nhận lúc: {dayjs(item.employerVerifiedAt).format('DD/MM/YYYY HH:mm')}</p>}
                </div>
              }
            />
          ) : item.employerVerifiedStatus === 2 ? (
            <Alert type="error" showIcon title="Doanh nghiệp đã từ chối xác nhận" description={item.employerEvaluationNotes || undefined} />
          ) : (
            <Alert type="warning" showIcon title="Đang chờ doanh nghiệp xác thực qua email" />
          )}

          <div className="request-approval-actions">
            <Button
              type="primary"
              size="large"
              icon={<CheckCircleOutlined />}
              disabled={!canApprove}
              loading={approving}
              onClick={onApprove}
              style={{ background: canApprove ? '#002140' : undefined }}
            >
              {item.status === 2 ? 'Đã phê duyệt' : 'Phê duyệt & đồng bộ hồ sơ'}
            </Button>
            {!canApprove && item.status !== 2 && (
              <Typography.Text type="secondary">
                Chỉ có thể duyệt sau khi doanh nghiệp xác nhận.
              </Typography.Text>
            )}
          </div>
        </>
      )}

      <Typography.Title level={5}>Thông tin xử lý</Typography.Title>
      <Descriptions column={1} size="small" bordered>
        <Descriptions.Item label="Người duyệt">{item.approvalName || 'Chưa có'}</Descriptions.Item>
        <Descriptions.Item label="Ghi chú">{item.note || '—'}</Descriptions.Item>
        <Descriptions.Item label="Cập nhật">{dayjs(item.updateDate).format('DD/MM/YYYY HH:mm')}</Descriptions.Item>
      </Descriptions>
    </div>
  )
}

function exportRequests(items: ManagementFormRequest[]) {
  const rows = [
    ['Mã yêu cầu', 'Loại yêu cầu', 'MSSV', 'Họ tên', 'Doanh nghiệp', 'Vị trí', 'Ngày gửi', 'Tiến trình', 'Người duyệt'],
    ...items.map(item => [
      item.id,
      item.requestType,
      item.studentCode,
      item.studentName,
      item.companyName ?? '',
      item.position ?? '',
      item.creationDate,
      workflowStatus(item).label,
      item.approvalName,
    ]),
  ]
  const csv = rows.map(row => row.map(value => `"${String(value).replaceAll('"', '""')}"`).join(',')).join('\n')
  const blob = new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' })
  const href = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = href
  anchor.download = 'yeu-cau-bieu-mau.csv'
  anchor.click()
  URL.revokeObjectURL(href)
}
