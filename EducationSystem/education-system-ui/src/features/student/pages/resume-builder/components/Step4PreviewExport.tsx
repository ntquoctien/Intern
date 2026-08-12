import { CheckCircleOutlined, EditOutlined, PrinterOutlined, ReloadOutlined, RobotOutlined, WarningOutlined } from '@ant-design/icons'
import { Alert, Button, Card, Progress, Tooltip, Typography, message } from 'antd'
import axios from 'axios'
import { useResumeStore } from '../hooks/useResumeStore'
import { useOptimizeResume } from '../hooks/useOptimizeResume'
import { useStudentAuth } from '../../../studentAuth'
import { printA4Resume } from '../utils/printA4Resume'

const { Title, Text } = Typography

type ProblemDetailsLike = {
  title?: string
  detail?: string
  errorCode?: string
  errors?: Record<string, string[]>
}

function formatBackendError(error: unknown) {
  if (!axios.isAxiosError(error)) return null
  if (error.code === 'ECONNABORTED') {
    return 'Tối ưu CV mất quá lâu và đã bị hủy sau 5 phút. Vui lòng thử lại hoặc rút gọn nội dung đã chọn.'
  }
  const data = error.response?.data as ProblemDetailsLike | undefined
  if (!data) return null

  const validationErrors = data.errors
    ? Object.entries(data.errors)
        .flatMap(([field, messages]) => messages.map(message => `${field}: ${message}`))
    : []

  const header = data.errorCode ?? data.title
  const detail = data.detail ?? validationErrors.join(' | ')
  if (!header && !detail) return null

  return [header, detail].filter(Boolean).join(' - ')
}

export function Step4PreviewExport() {
  const { state, setStep, setOptimizing, setOptimizedResult } = useResumeStore()
  const {
    optimizedCvResult,
    isOptimizingAi,
    targetRole,
    jobDescription,
    selectedSubjectIds,
    selectedInternshipIds,
    uiProjects,
  } = state
  const { session } = useStudentAuth()
  const optimizeMutation = useOptimizeResume()

  const handleOptimize = () => {
    if (!session?.studentId) {
      message.error('Không tìm thấy thông tin sinh viên. Vui lòng đăng nhập lại.')
      return
    }
    if (targetRole.trim().length < 2) {
      message.warning('Vui lòng nhập vị trí mục tiêu ở Bước 1.')
      return
    }
    if (jobDescription.trim().length < 10) {
      message.warning('Vui lòng nhập mô tả công việc (tối thiểu 10 ký tự) ở Bước 1.')
      return
    }

    setOptimizing(true)
    optimizeMutation.mutate(
      { state, studentId: session.studentId },
      {
        onSuccess: result => {
          setOptimizedResult(result)
          message.success('AI đã tối ưu CV thành công! Kiểm tra bản xem trước bên phải.')
        },
        onError: error => {
          console.error('Optimize resume failed:', error)
          const backendMessage = formatBackendError(error)
          if (backendMessage) {
            message.error(backendMessage)
            return
          }
          message.error('Không thể tối ưu CV bằng AI. Vui lòng thử lại.')
        },
        onSettled: () => setOptimizing(false),
      },
    )
  }

  return (
    <Card className="step-card">
      <Title level={4}>
        <CheckCircleOutlined /> Xác nhận thông tin
      </Title>
      <Text type="secondary">Kiểm tra lại thông tin trước khi tối ưu bằng AI</Text>

      <div className="summary-section" style={{ marginTop: 24 }}>
        <Title level={5}>Tóm tắt lựa chọn</Title>
        <div className="summary-grid">
          <div className="summary-item">
            <Text strong>Vị trí mục tiêu:</Text>
            <Text>{targetRole || 'Chưa nhập'}</Text>
          </div>
          <div className="summary-item">
            <Text strong>Học phần đã chọn:</Text>
            <Text>{selectedSubjectIds.length} học phần</Text>
          </div>
          <div className="summary-item">
            <Text strong>Dự án:</Text>
            <Text>{uiProjects.length} dự án</Text>
          </div>
          <div className="summary-item">
            <Text strong>Kỳ thực tập:</Text>
            <Text>{selectedInternshipIds.length} kỳ thực tập</Text>
          </div>
        </div>
      </div>

      {!optimizedCvResult && (
        <div className="optimize-section" style={{ marginTop: 24 }}>
          <RobotOutlined style={{ fontSize: 48, color: '#1890ff', marginBottom: 16 }} />
          <Title level={5}>Sẵn sàng tối ưu CV bằng AI?</Title>
          <Text type="secondary" style={{ display: 'block', marginBottom: 16 }}>
            AI sẽ viết lại professional summary, tối ưu từ khóa theo JD và format bullet points chuyên nghiệp
          </Text>
          <Button
            type="primary"
            size="large"
            icon={<RobotOutlined />}
            loading={isOptimizingAi}
            onClick={handleOptimize}
          >
            Tối ưu CV bằng AI
          </Button>
          <div style={{ marginTop: 12 }}>
            <Text type="secondary" style={{ fontSize: 12 }}>
              AI chỉ gọt giũa cách diễn đạt, không tự bịa thêm kinh nghiệm.
            </Text>
          </div>
        </div>
      )}

      {optimizeMutation.isError && (
        <Alert
          style={{ marginTop: 16 }}
          type="error"
          showIcon
          title="Tối ưu CV thất bại"
            description="Kiểm tra log console hoặc response backend để xem mã lỗi thật; nếu là timeout, hãy thử lại hoặc rút gọn dữ liệu đã chọn."
        />
      )}

      {optimizedCvResult && !optimizeMutation.isError && (
        <>
          <Alert
            style={{ marginTop: 16 }}
            type="success"
            showIcon
            message="CV đã được tối ưu thành công!"
            description="Bạn có thể nhấp trực tiếp vào nội dung trên bản xem trước để chỉnh sửa trước khi in."
          />

          {optimizedCvResult.qualityMetrics.hasHallucinationWarning && (
            <Alert
              style={{ marginTop: 16 }}
              type="warning"
              showIcon
              icon={<WarningOutlined />}
              message="Cảnh báo nội dung"
              description="AI có thể đã thêm nội dung chưa có trong dữ liệu gốc. Vui lòng rà soát kỹ bản xem trước."
            />
          )}

          <div className="metrics-section" style={{ marginTop: 24 }}>
            <Title level={5}>Chỉ số chất lượng CV</Title>
            <div className="metrics-grid">
              <div className="metric-item">
                <div className="metric-header">
                  <Tooltip title="Latent Space Cosine Similarity giữa nội dung CV và JD">
                    <Text strong>Độ phù hợp JD</Text>
                  </Tooltip>
                  <Text>{Math.round(optimizedCvResult.qualityMetrics.jobAlignmentScore * 100)}%</Text>
                </div>
                <Progress
                  percent={Math.round(optimizedCvResult.qualityMetrics.jobAlignmentScore * 100)}
                  strokeColor="#52c41a"
                  showInfo={false}
                />
              </div>
              <div className="metric-item">
                <div className="metric-header">
                  <Tooltip title="Token Space Overlap giữa nội dung gốc và nội dung đã tối ưu">
                    <Text strong>Độ trung thực dữ liệu</Text>
                  </Tooltip>
                  <Text>{Math.round(optimizedCvResult.qualityMetrics.contentPreservationScore * 100)}%</Text>
                </div>
                <Progress
                  percent={Math.round(optimizedCvResult.qualityMetrics.contentPreservationScore * 100)}
                  strokeColor="#1890ff"
                  showInfo={false}
                />
              </div>
            </div>
          </div>
        </>
      )}

      <div className="action-buttons" style={{ marginTop: 32 }}>
        <Button icon={<EditOutlined />} onClick={() => setStep(2)}>
          Quay lại chỉnh sửa
        </Button>
        <Button
          type="primary"
          size="large"
          icon={<PrinterOutlined />}
          onClick={printA4Resume}
          disabled={!optimizedCvResult}
        >
          In CV / Xuất PDF
        </Button>
        <Button
          icon={<ReloadOutlined />}
          onClick={handleOptimize}
          loading={isOptimizingAi}
          disabled={!optimizedCvResult}
        >
          Tối ưu lại
        </Button>
      </div>
    </Card>
  )
}
