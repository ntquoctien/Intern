import { Alert, Button, Card, Form, Input, Space, Typography } from 'antd'
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

  const submit = async ({ studentCode }: { studentCode: string }) => {
    setSubmitting(true)
    setError(undefined)
    try {
      await login(studentCode)
      navigate('/student/dashboard', { replace: true })
    } catch (reason) {
      const response = axios.isAxiosError<ApiResponse<unknown>>(reason) ? reason.response?.data : undefined
      setError(response?.error?.code === 'STUDENT_CODE_AMBIGUOUS'
        ? 'Mã sinh viên đang trùng với nhiều hồ sơ. Vui lòng liên hệ quản trị viên.'
        : response?.error?.code === 'STUDENT_NOT_FOUND'
          ? 'Không tìm thấy mã sinh viên hợp lệ.'
          : response?.message ?? 'Không thể đăng nhập lúc này.')
    } finally {
      setSubmitting(false)
    }
  }

  return <main className="student-login-shell">
    <Card className="student-login-card">
      <Space direction="vertical" size="large" style={{ width: '100%' }}>
        <div><Typography.Title level={2}>Cổng thông tin sinh viên</Typography.Title><Typography.Text type="secondary">Đăng nhập bằng mã số sinh viên (MSSV)</Typography.Text></div>
        {new URLSearchParams(location.search).get('reason') === 'expired' && <Alert type="warning" showIcon message="Phiên đã hết hạn. Vui lòng đăng nhập lại." />}
        <Alert type="warning" showIcon message="Chỉ dành cho demo/nội bộ" description="MSSV không phải là cơ chế xác minh danh tính an toàn. Không dùng cổng này như hệ thống đăng nhập production." />
        {error && <Alert type="error" showIcon message={error} />}
        <Form layout="vertical" onFinish={submit}>
          <Form.Item name="studentCode" label="Mã số sinh viên" rules={[{ required: true, message: 'Vui lòng nhập MSSV' }, { max: 100 }]}>
            <Input autoFocus autoComplete="username" placeholder="Ví dụ: SV000001" />
          </Form.Item>
          <Button type="primary" htmlType="submit" loading={submitting} block>Đăng nhập</Button>
        </Form>
      </Space>
    </Card>
  </main>
}
