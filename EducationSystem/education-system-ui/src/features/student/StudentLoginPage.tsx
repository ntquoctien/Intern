import { Alert, Button, Card, Form, Input, Typography } from 'antd'
import axios from 'axios'
import { useState } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import type { ApiResponse } from '../../shared/types/api'
import { useStudentAuth } from './studentAuth'

export function StudentLoginPage() {
  const { session, login } = useStudentAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [error, setError] = useState<string>()
  const [submitting, setSubmitting] = useState(false)
  if (session) return <Navigate to="/student/dashboard" replace />

  const submit = async ({ studentCode, password }: { studentCode: string; password: string }) => {
    setSubmitting(true)
    setError(undefined)
    try {
      await login(studentCode, password)
      navigate('/student/dashboard', { replace: true })
    } catch (reason) {
      const response = axios.isAxiosError<ApiResponse<unknown>>(reason) ? reason.response?.data : undefined
      setError(response?.error?.code === 'STUDENT_IDENTITY_SERVICE_UNAVAILABLE'
        ? 'Hệ thống đang tạm thời gián đoạn. Vui lòng thử lại sau.'
        : 'Thông tin đăng nhập không hợp lệ.')
    } finally {
      setSubmitting(false)
    }
  }

  return <main className="student-login-shell">
    <section className="student-login-panel" aria-labelledby="student-login-title">
      <header className="student-login-brand">
        <span className="student-login-brand-mark" aria-hidden="true">TDU</span>
        <div>
          <strong>TRƯỜNG ĐẠI HỌC TÂY ĐÔ</strong>
          <span>CỔNG THÔNG TIN SINH VIÊN</span>
        </div>
      </header>
    <Card className="student-login-card">
      <Typography.Title id="student-login-title" level={2}>Đăng nhập</Typography.Title>
      <Typography.Paragraph type="secondary">Sử dụng mã số sinh viên và mật khẩu để tiếp tục.</Typography.Paragraph>
        {new URLSearchParams(location.search).get('reason') === 'expired' && <Alert type="warning" showIcon title="Phiên đã hết hạn. Vui lòng đăng nhập lại." />}
        {new URLSearchParams(location.search).get('reason') === 'unauthorized' && <Alert type="warning" showIcon title="Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại." />}
        {error && <Alert type="error" showIcon title={error} />}
        <Form layout="vertical" onFinish={submit}>
          <Form.Item name="studentCode" label="Mã số sinh viên" rules={[{ required: true, message: 'Vui lòng nhập MSSV' }, { max: 100 }]}>
            <Input autoFocus autoComplete="username" placeholder="Nhập mã số sinh viên" />
          </Form.Item>
          <Form.Item name="password" label="Mật khẩu" rules={[{ required: true, message: 'Vui lòng nhập mật khẩu' }]}>
            <Input.Password autoComplete="current-password" placeholder="Nhập mật khẩu" />
          </Form.Item>
          <Button type="primary" htmlType="submit" loading={submitting} block>Đăng nhập</Button>
        </Form>
        <Typography.Text className="student-login-hint" type="secondary">Mật khẩu mặc định: <strong>1</strong></Typography.Text>
    </Card>
    </section>
  </main>
}
