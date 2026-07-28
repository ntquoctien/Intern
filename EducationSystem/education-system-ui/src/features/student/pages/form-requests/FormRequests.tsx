import {
  BankOutlined,
  CalendarOutlined,
  CheckCircleFilled,
  FileProtectOutlined,
  MailOutlined,
  PlusOutlined,
  SafetyCertificateOutlined,
} from '@ant-design/icons'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  Alert,
  Button,
  Card,
  DatePicker,
  Form,
  Input,
  Select,
  Table,
  Tag,
  Typography,
  message,
  type TableColumnsType,
} from 'antd'
import type { Dayjs } from 'dayjs'
import { useEffect, useMemo, useState } from 'react'
import { internshipApi } from '../../../internship/internshipApi'
import { studentApi } from '../../studentApi'
import type { FormRequest, StudentFormTemplate } from '../../types'

const internshipTemplateName = 'Đơn xác nhận thực tập doanh nghiệp'
const internshipTemplate: StudentFormTemplate = {
  formTemplateId: 'internship-verification',
  name: internshipTemplateName,
}

type InternshipFormValues = {
  companyName: string
  position: string
  mentorEmail: string
  duration: [Dayjs, Dayjs]
  taskDescription: string
}

const positionOptions = ['Backend', 'Frontend', 'Fullstack', 'Mobile', 'QA', 'UI/UX', 'DevOps', 'Data']
  .map(value => ({ value, label: value }))

const statusMeta: Record<number, { color: string; label: string; percent: number }> = {
  0: { color: 'orange', label: 'Chờ Doanh Nghiệp Xác Thực', percent: 25 },
  1: { color: 'blue', label: 'Chờ Nhà Trường Duyệt', percent: 62 },
  2: { color: 'green', label: 'Đã Phê Duyệt ✔️', percent: 100 },
  3: { color: 'red', label: 'Bị Từ Chối ❌', percent: 100 },
}

export function FormRequests() {
  const [form] = Form.useForm<InternshipFormValues>()
  const [selectedTemplate, setSelectedTemplate] = useState(internshipTemplate.formTemplateId)
  const [requests, setRequests] = useState<FormRequest[]>([])
  const [messageApi, messageContext] = message.useMessage()
  const queryClient = useQueryClient()

  const sourceQuery = useQuery({
    queryKey: ['student', 'form-requests-with-templates'],
    queryFn: async () => {
      const [items, templates] = await Promise.all([
        studentApi.formRequests(),
        studentApi.formTemplates(),
      ])
      return { requests: items, templates }
    },
  })

  useEffect(() => {
    if (sourceQuery.data) setRequests(sourceQuery.data.requests)
  }, [sourceQuery.data])

  const templates = useMemo(() => {
    const apiTemplates = sourceQuery.data?.templates ?? []
    return apiTemplates.some(item =>
      item.name.toLocaleLowerCase('vi').includes('xác nhận thực tập'))
      ? apiTemplates
      : [internshipTemplate, ...apiTemplates]
  }, [sourceQuery.data])
  const selectedTemplateData = templates.find(item => item.formTemplateId === selectedTemplate)
  const isInternshipTemplate = selectedTemplateData?.name
    .toLocaleLowerCase('vi')
    .includes('xác nhận thực tập') ?? selectedTemplate === internshipTemplate.formTemplateId

  useEffect(() => {
    const apiInternshipTemplate = sourceQuery.data?.templates.find(item =>
      item.name.toLocaleLowerCase('vi').includes('xác nhận thực tập'))
    if (apiInternshipTemplate) setSelectedTemplate(apiInternshipTemplate.formTemplateId)
  }, [sourceQuery.data])

  const submitMutation = useMutation({
    mutationFn: internshipApi.submit,
    onSuccess: (result, payload) => {
      setRequests(current => [{
        formRequestId: result.requestId,
        formTemplateId: selectedTemplate,
        formTemplateName: selectedTemplateData?.name ?? internshipTemplateName,
        creationDate: result.createdAt,
        updateDate: result.createdAt,
        rawStatus: 0,
        approvalName: '',
        note: `${payload.companyName} · ${payload.position}`,
      }, ...current])
      form.resetFields()
      void queryClient.invalidateQueries({ queryKey: ['student', 'form-requests-with-templates'] })
      messageApi.success('Đã gửi yêu cầu và chuyển email xác thực đến Mentor')
    },
    onError: () => messageApi.error('Không thể gửi yêu cầu. Vui lòng thử lại.'),
  })

  const handleSubmit = (values: InternshipFormValues) => {
    submitMutation.mutate({
      companyName: values.companyName.trim(),
      position: values.position,
      mentorEmail: values.mentorEmail.trim(),
      startDate: values.duration[0].format('YYYY-MM-DD'),
      endDate: values.duration[1].format('YYYY-MM-DD'),
      taskDescription: values.taskDescription.trim(),
    })
  }

  const columns: TableColumnsType<FormRequest> = [
    {
      title: 'Biểu mẫu / Đơn vị',
      render: (_, item) => <div className="internship-request-name"><b>{item.formTemplateName ?? internshipTemplateName}</b><span>{item.note || 'Chưa có mô tả'}</span></div>,
    },
    { title: 'Ngày gửi', width: 125, render: (_, item) => new Date(item.creationDate).toLocaleDateString('vi-VN') },
    {
      title: 'Tiến trình xác thực',
      width: 260,
      render: (_, item) => <RequestStatus status={item.rawStatus} />,
    },
    { title: 'Người duyệt', width: 140, render: (_, item) => item.approvalName || '—' },
  ]

  return <div className="student-forms-page internship-requests-page">
    {messageContext}
    <div className="student-page-heading">
      <Typography.Title level={2}>Biểu mẫu và yêu cầu</Typography.Title>
      <Typography.Text type="secondary">Tạo đơn xác nhận thực tập và theo dõi tiến trình xử lý tập trung.</Typography.Text>
    </div>

    {sourceQuery.isError && (
      <Alert
        className="student-page-alert"
        type="error"
        showIcon
        title="Không thể tải dữ liệu biểu mẫu từ CommunicationService."
      />
    )}

    <div className="internship-form-layout">
      <Card className="internship-template-card rounded-2xl bg-white shadow-sm" title="Chọn biểu mẫu">
        <div className="internship-template-list">
          {templates.map(template => <button key={template.formTemplateId} className={selectedTemplate === template.formTemplateId ? 'selected' : ''} onClick={() => setSelectedTemplate(template.formTemplateId)}>
            <span><FileProtectOutlined /></span>
            <div><b>{template.name}</b><small>{template.name.toLocaleLowerCase('vi').includes('xác nhận thực tập') ? 'Gửi xác thực trực tuyến đến doanh nghiệp' : 'Biểu mẫu dịch vụ sinh viên'}</small></div>
            {selectedTemplate === template.formTemplateId && <CheckCircleFilled />}
          </button>)}
        </div>
      </Card>

      <Card className="internship-create-card rounded-2xl bg-white shadow-sm" title={<span><SafetyCertificateOutlined /> {isInternshipTemplate ? selectedTemplateData?.name ?? internshipTemplateName : 'Thông tin biểu mẫu'}</span>}>
        {isInternshipTemplate ? <Form form={form} layout="vertical" requiredMark="optional" onFinish={handleSubmit}>
          <div className="internship-form-grid">
            <Form.Item name="companyName" label="Tên doanh nghiệp" rules={[{ required: true, message: 'Vui lòng nhập tên doanh nghiệp' }]}><Input prefix={<BankOutlined />} placeholder="Ví dụ: FPT Software Cần Thơ" /></Form.Item>
            <Form.Item name="position" label="Vị trí thực tập" rules={[{ required: true, message: 'Vui lòng chọn vị trí' }]}><Select placeholder="Chọn vị trí" options={positionOptions} /></Form.Item>
          </div>
          <Form.Item name="mentorEmail" label="Email Mentor tại doanh nghiệp" rules={[{ required: true, type: 'email', message: 'Email Mentor chưa hợp lệ' }]}><Input prefix={<MailOutlined />} placeholder="mentor@company.com" /></Form.Item>
          <Form.Item name="duration" label="Thời gian thực tập" rules={[{ required: true, message: 'Vui lòng chọn thời gian thực tập' }]}><DatePicker.RangePicker prefix={<CalendarOutlined />} format="DD/MM/YYYY" style={{ width: '100%' }} /></Form.Item>
          <Form.Item name="taskDescription" label="Nhiệm vụ thực tế tự khai báo" rules={[{ required: true, min: 20, message: 'Hãy mô tả ít nhất 20 ký tự' }]}><Input.TextArea rows={5} maxLength={3000} showCount placeholder="Mô tả công việc, công nghệ và kết quả bạn trực tiếp thực hiện..." /></Form.Item>
          <Alert showIcon type="info" title="Sau khi gửi, hệ thống sẽ tạo đường dẫn bảo mật dùng một lần và gửi đến Email Mentor." />
          <Button className="internship-submit-button" type="primary" htmlType="submit" icon={<PlusOutlined />} loading={submitMutation.isPending}>Gửi yêu cầu</Button>
        </Form> : <Alert showIcon type="info" title="Biểu mẫu này chưa có form nhập liệu trực tuyến." />}
      </Card>
    </div>

    <Card className="internship-history-card rounded-2xl bg-white shadow-sm" title="Danh sách đơn đã gửi" extra={<Tag>{requests.length} yêu cầu</Tag>}>
      <Table rowKey="formRequestId" columns={columns} dataSource={requests} pagination={{ pageSize: 5, hideOnSinglePage: true }} loading={sourceQuery.isFetching} scroll={{ x: 780 }} />
    </Card>
  </div>
}

function RequestStatus({ status }: { status: number }) {
  const meta = statusMeta[status] ?? { color: 'default', label: `Trạng thái ${status}`, percent: 0 }
  return <div className={`smart-request-status status-${status}`}>
    <Tag color={meta.color}>{meta.label}</Tag>
    <div><i style={{ width: `${meta.percent}%` }} /></div>
  </div>
}

export const StudentFormRequestsPage = FormRequests
