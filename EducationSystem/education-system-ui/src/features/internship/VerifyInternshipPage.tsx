import {
  BankOutlined,
  CheckCircleFilled,
  CloseCircleOutlined,
  SafetyCertificateOutlined,
  SendOutlined,
  UserOutlined,
} from '@ant-design/icons'
import { useMutation, useQuery } from '@tanstack/react-query'
import { Alert, Button, Card, Checkbox, Form, Input, InputNumber, Result, Space, Typography, message } from 'antd'
import { useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { internshipApi, type EmployerVerificationPayload } from './internshipApi'

type FeedbackValues = {
  confirmed: boolean
  score: number
  evaluationNotes?: string
}

export function VerifyInternshipPage() {
  const [params] = useSearchParams()
  const token = params.get('token') ?? ''
  const [form] = Form.useForm<FeedbackValues>()
  const [completedStatus, setCompletedStatus] = useState<1 | 2>()
  const [messageApi, messageContext] = message.useMessage()

  const contextQuery = useQuery({
    queryKey: ['public', 'internship-verification', token],
    queryFn: () => internshipApi.getEmployerContext(token),
    enabled: Boolean(token),
    retry: false,
  })

  const verificationMutation = useMutation({
    mutationFn: (payload: EmployerVerificationPayload) => internshipApi.verifyByEmployer(payload),
    onSuccess: result => setCompletedStatus(result.employerVerifiedStatus === 1 ? 1 : 2),
    onError: () => messageApi.error('Liên kết không hợp lệ, đã hết hạn hoặc đã được sử dụng.'),
  })

  const sendResponse = (isCorrect: boolean) => {
    const values = form.getFieldsValue()
    if (isCorrect && !values.confirmed) {
      messageApi.warning('Vui lòng tích xác nhận thông tin trước khi gửi phản hồi.')
      return
    }
    if (values.score == null) {
      messageApi.warning('Vui lòng nhập điểm đánh giá tổng quan.')
      return
    }
    verificationMutation.mutate({
      token,
      isInformationCorrect: isCorrect,
      score: values.score,
      evaluationNotes: values.evaluationNotes?.trim() || null,
    })
  }

  if (!token) {
    return <main className="verify-internship-shell min-h-screen"><Result status="warning" title="Thiếu mã xác thực" subTitle="Đường dẫn xác thực không đầy đủ. Vui lòng mở đúng liên kết được gửi qua Email." /></main>
  }

  if (completedStatus) {
    return <main className="verify-internship-shell min-h-screen"><Card className="verify-internship-card mx-auto max-w-2xl rounded-2xl border bg-white shadow-xl"><Result status={completedStatus === 1 ? 'success' : 'error'} title={completedStatus === 1 ? 'Đã gửi xác nhận thành công' : 'Đã ghi nhận từ chối xác nhận'} subTitle="Cảm ơn Quý doanh nghiệp đã phản hồi. Liên kết bảo mật này không thể sử dụng lại." /></Card></main>
  }

  if (contextQuery.isError) {
    return <main className="verify-internship-shell min-h-screen">
      <Card className="verify-internship-card mx-auto max-w-2xl rounded-2xl border bg-white shadow-xl">
        <Result
          status="error"
          title="Liên kết xác thực không hợp lệ"
          subTitle="Liên kết có thể đã được sử dụng, bị thu hồi hoặc không tồn tại."
        />
      </Card>
    </main>
  }

  const context = contextQuery.data

  return <main className="verify-internship-shell min-h-screen">
    {messageContext}
    <Card className="verify-internship-card mx-auto my-12 max-w-2xl rounded-2xl border bg-white shadow-xl" loading={contextQuery.isFetching}>
      <header className="verification-brand">
        <span className="verification-emblem">TDU</span>
        <div><b>CỔNG XÁC THỰC THỰC TẬP</b><strong>Cao Đẳng Tây Đô</strong></div>
      </header>

      {context && <><div className="verification-intro">
        <SafetyCertificateOutlined />
        <div><Typography.Title level={3}>Xác nhận thông tin thực tập</Typography.Title><Typography.Text>Phản hồi của Quý doanh nghiệp giúp nhà trường bảo đảm tính chính xác của hồ sơ sinh viên.</Typography.Text></div>
      </div>

      <section className="verification-summary">
        <div><BankOutlined /><span>Doanh nghiệp đối tác<b>{context.companyName}</b></span></div>
        <div><UserOutlined /><span>Sinh viên<b>{context.studentName} · MSSV: {context.studentCode}</b></span></div>
        <div><CheckCircleFilled /><span>Vị trí thực tập<b>{context.position}</b></span></div>
        <article><small>Nhiệm vụ sinh viên khai báo</small><p>{context.taskDescription}</p></article>
      </section>

      <Form form={form} layout="vertical" initialValues={{ confirmed: false, score: 8 }}>
        <Form.Item name="confirmed" valuePropName="checked">
          <Checkbox>Tôi xác nhận thông tin thực tập của sinh viên trên là hoàn toàn chính xác.</Checkbox>
        </Form.Item>
        <Form.Item name="score" label="Điểm đánh giá tổng quan (0–10)" rules={[{ required: true, message: 'Vui lòng nhập điểm đánh giá' }]}>
          <InputNumber min={0} max={10} step={0.5} precision={1} style={{ width: '100%' }} />
        </Form.Item>
        <Form.Item name="evaluationNotes" label="Đánh giá nhanh thái độ và năng lực">
          <Input.TextArea rows={5} maxLength={2000} showCount placeholder="Ví dụ: Tinh thần chủ động, khả năng chuyên môn, giao tiếp, mức độ hoàn thành nhiệm vụ..." />
        </Form.Item>
        <Alert className="verification-security-note" type="info" showIcon title="Phản hồi được bảo vệ bằng mã dùng một lần và chỉ phục vụ công tác xác minh của nhà trường." />
        <Space className="verification-actions" wrap>
          <Button className="verification-confirm-button" type="primary" icon={<SendOutlined />} loading={verificationMutation.isPending} onClick={() => sendResponse(true)}>Xác nhận & Gửi phản hồi</Button>
          <Button danger icon={<CloseCircleOutlined />} disabled={verificationMutation.isPending} onClick={() => sendResponse(false)}>Từ chối xác nhận</Button>
        </Space>
      </Form></>}
    </Card>
  </main>
}
