import {
  AimOutlined,
  EnvironmentOutlined,
  FileTextOutlined,
  GithubOutlined,
  LinkedinOutlined,
  MailOutlined,
  PhoneOutlined,
  SearchOutlined,
} from '@ant-design/icons'
import { Button, Card, Input, Space, Spin, Typography, message } from 'antd'
import TextArea from 'antd/es/input/TextArea'
import { useResumeStore } from '../hooks/useResumeStore'
import { useStudentAuth } from '../../../studentAuth'
import { httpClient } from '../../../../../shared/api/httpClient'
import type { ApiResponse } from '../../../../../shared/types/api'
import type { CloLevel } from '../types'

const { Title, Text } = Typography

const MAX_JD_LENGTH = 1500
const MAX_TARGET_ROLE_LENGTH = 200
const MAX_CAREER_FOCUS_TAG_LENGTH = 200
const RECOMMENDED_COURSE_LIMIT = 10
const academicApiOrigin = import.meta.env.VITE_ACADEMIC_API_ORIGIN ?? 'http://localhost:5002'
const careerApiOrigin =
  import.meta.env.VITE_AI_API_ORIGIN ?? import.meta.env.VITE_CAREER_API_ORIGIN ?? 'http://localhost:5005'

type MatchedSubjectOutcome = {
  outcomeCode?: string | null
  name?: string | null
  description?: string | null
  similarityScore?: number | null
  progressionLevel?: string | null
}

// Response của POST /api/career/resume/prepare-context (BFF -> VectorMatchService)
type PrepareContextResponse = {
  matchedSubjects: Array<{
    subjectId: string
    subjectCode: string
    subjectName: string
    creditPoint?: number
    score?: number
    courseOutcomes: MatchedSubjectOutcome[]
  }>
  isFallbackMode?: boolean
}

const normalizeProgression = (value?: string | null): CloLevel | undefined =>
  value === 'E' || value === 'R' || value === 'D' ? value : undefined

type AcademicContextResponse = {
  student: {
    studentId: string
    fullName: string
    userName: string
    majorName: string
    academicYear?: string | null
  }
  gpa?: number | null
  eligibleCourses: Array<{
    subjectId: string
    subjectCode: string
    subjectName: string
    score: number
    courseOutcomes: Array<{ name: string; description?: string | null }>
  }>
  projects: Array<{
    projectId: number
    projectName: string
    techStack: string
    sourceCodeUrl?: string | null
    teamSize: number
    myRole?: string | null
    myContributions?: string | null
  }>
  approvedInternships: Array<{
    internshipId: number
    companyName: string
    position: string
    startDate: string
    endDate?: string | null
    taskDescription?: string | null
  }>
}

export function Step1TargetJd() {
  const { state, setTargetJd, setContactInfo, setLoadingContext, setContextData, setStep } = useResumeStore()
  const { targetRole, jobDescription, careerFocusTag, contactInfo, isLoadingContext } = state
  const { session } = useStudentAuth()

  const handleAnalyze = async () => {
    if (!session?.studentId) {
      message.error('Không tìm thấy thông tin sinh viên. Vui lòng đăng nhập lại.')
      return
    }

    setLoadingContext(true)
    try {
      // Gọi song song: context thô từ AcademicService + phân tích JD qua
      // CareerService BFF (prepare-context -> VectorMatchService sentence-BERT).
      const [contextResult, prepareResult] = await Promise.allSettled([
        httpClient.get<ApiResponse<AcademicContextResponse>>(
          `${academicApiOrigin}/api/academic/resume/get-context-data/${session.studentId}`,
        ),
        httpClient.post<ApiResponse<PrepareContextResponse>>(
          `${careerApiOrigin}/api/career/resume/prepare-context`,
          {
            studentId: session.studentId,
            targetRole: targetRole.trim(),
            jobDescription: jobDescription.trim(),
            // Chưa chọn môn/thực tập ở bước này: để trống để VectorMatch
            // index toàn bộ môn đủ điều kiện (điểm >= 7.0) của sinh viên.
            selectedSubjectIds: [],
            selectedInternshipIds: [],
            uiProjects: [],
            careerFocusTag: careerFocusTag.trim() || undefined,
            topK: RECOMMENDED_COURSE_LIMIT,
            similarityThreshold: 0.65,
          },
        ),
      ])

      if (contextResult.status === 'rejected') {
        throw contextResult.reason
      }

      const data = contextResult.value.data.data
      if (!data) {
        throw new Error('Empty academic context response')
      }

      // VectorMatch là tầng nâng cao: nếu dịch vụ phân tích JD không khả dụng
      // thì vẫn hiển thị môn học theo điểm số thay vì chặn luồng.
      const vectorMatchUnavailable = prepareResult.status === 'rejected'
        || prepareResult.value.data.data?.isFallbackMode === true
      if (vectorMatchUnavailable) {
        console.warn(
          'Vector match unavailable, falling back to raw scores:',
          prepareResult.status === 'rejected'
            ? prepareResult.reason
            : 'CareerService returned fallback mode',
        )
      }
      const matchedSubjects =
        prepareResult.status === 'fulfilled'
          ? (prepareResult.value.data.data?.matchedSubjects ?? [])
          : []

      const matchBySubjectId = new Map(
        matchedSubjects.map(m => [m.subjectId.toLowerCase(), m] as const),
      )

      const rankedCourses = data.eligibleCourses.map(c => {
        const match = vectorMatchUnavailable
          ? undefined
          : matchBySubjectId.get(c.subjectId.toLowerCase())
        const outcomes = match?.courseOutcomes ?? []
        const outcomeByName = new Map<string, MatchedSubjectOutcome>()
        for (const outcome of outcomes) {
          if (outcome.name) {
            outcomeByName.set(outcome.name.trim().toLowerCase(), outcome)
          }
        }
        const bestOutcome = outcomes.reduce<MatchedSubjectOutcome | undefined>(
          (best, outcome) =>
            (outcome.similarityScore ?? 0) > (best?.similarityScore ?? 0) ? outcome : best,
          undefined,
        )
        return {
          subjectId: c.subjectId,
          subjectCode: c.subjectCode,
          subjectName: c.subjectName,
          score: c.score,
          similarityScore: bestOutcome?.similarityScore ?? undefined,
          cloLevel: normalizeProgression(bestOutcome?.progressionLevel),
          recommendationSource: match ? 'vector' as const : 'score-fallback' as const,
          matchedOutcomes: outcomes
            .filter(outcome => outcome.description || outcome.name)
            .sort((a, b) => (b.similarityScore ?? 0) - (a.similarityScore ?? 0))
            .map(outcome => ({
              code: outcome.outcomeCode || outcome.name || 'CLO',
              description: outcome.description || outcome.name || '',
              similarityScore: outcome.similarityScore ?? 0,
              cloLevel: normalizeProgression(outcome.progressionLevel),
            })),
          courseOutcomes: c.courseOutcomes.map(o => ({
            ...o,
            cloLevel: normalizeProgression(
              outcomeByName.get(o.name.trim().toLowerCase())?.progressionLevel,
            ),
          })),
        }
      })

      // Fallback responses also contain matchedSubjects ranked by score. They
      // must not be treated as vector matches, otherwise every score-fallback
      // course is removed by the vector-only filter below.
      const hasVectorRecommendations =
        !vectorMatchUnavailable && matchedSubjects.length > 0
      const courses = rankedCourses
        .filter(course => !hasVectorRecommendations || course.recommendationSource === 'vector')
        .sort((a, b) => {
          if (hasVectorRecommendations) {
            return (b.similarityScore ?? 0) - (a.similarityScore ?? 0) || b.score - a.score
          }
          return b.score - a.score
        })
        .slice(0, RECOMMENDED_COURSE_LIMIT)

      setContextData({
        studentInfo: {
          fullName: data.student.fullName,
          studentCode: data.student.userName,
          majorName: data.student.majorName,
          gpa: data.gpa ?? 0,
          academicYear: data.student.academicYear ?? '',
        },
        courses,
        projects: data.projects.map(p => ({
          projectId: p.projectId,
          projectName: p.projectName,
          techStack: p.techStack,
          sourceCodeUrl: p.sourceCodeUrl,
          teamSize: p.teamSize,
          myRole: p.myRole,
          myContributions: p.myContributions,
        })),
        internships: data.approvedInternships.map(i => ({
          internshipId: i.internshipId,
          companyName: i.companyName,
          position: i.position,
          startDate: i.startDate,
          endDate: i.endDate,
          taskDescription: i.taskDescription,
        })),
      })

      if (courses.length === 0) {
        message.warning(
          'Dữ liệu học tập chưa có môn nào đạt điểm ≥ 7.0 nên chưa có môn học đề xuất.',
        )
      } else if (vectorMatchUnavailable || !hasVectorRecommendations) {
        message.warning('Chưa có kết quả khớp JD. Chỉ hiển thị tối đa 10 môn điểm cao để tham khảo.')
      } else if (matchedSubjects.length > 0) {
        message.success(`Đã phân tích JD — đề xuất ${courses.length} môn học phù hợp nhất!`)
      } else {
        message.success('Đã tải dữ liệu học tập thành công!')
      }
      setStep(1)
    } catch (error) {
      console.error('Failed to load academic context:', error)
      message.error('Không thể tải dữ liệu học tập. Vui lòng thử lại.')
    } finally {
      setLoadingContext(false)
    }
  }

  const isValid = targetRole.trim().length > 0 && jobDescription.trim().length > 0

  return (
    <div className="step-target-jd">
      <Card className="step-card">
        <Space direction="vertical" size="large" style={{ width: '100%' }}>
          <div className="step-header">
            <AimOutlined className="step-icon" />
            <div>
              <Title level={4} style={{ margin: 0 }}>Mục tiêu & Mô tả công việc</Title>
              <Text type="secondary">Nhập vị trí mục tiêu và mô tả công việc để AI phân tích</Text>
            </div>
          </div>

          <div className="form-section">
            <label className="form-label">
              <FileTextOutlined /> Vị trí mục tiêu (Target Role)
            </label>
            <Input
              placeholder="VD: Frontend Developer, Business Analyst..."
              value={targetRole}
              onChange={e => setTargetJd(e.target.value.slice(0, MAX_TARGET_ROLE_LENGTH), jobDescription, careerFocusTag)}
              size="large"
              maxLength={MAX_TARGET_ROLE_LENGTH}
            />
          </div>

          <div className="form-section">
            <label className="form-label">
              <FileTextOutlined /> Career Focus Tag (tùy chọn)
            </label>
            <Input
              placeholder="VD: Tập trung Backend & Microservices"
              value={careerFocusTag}
              onChange={e => setTargetJd(targetRole, jobDescription, e.target.value.slice(0, MAX_CAREER_FOCUS_TAG_LENGTH))}
              size="large"
              maxLength={MAX_CAREER_FOCUS_TAG_LENGTH}
            />
          </div>

          <div className="form-section contact-form-section">
            <label className="form-label">
              <MailOutlined /> Thông tin liên hệ hiển thị trên CV
            </label>
            <Text type="secondary">
              Thông tin này chỉ dùng để dựng CV và không được gửi đến AI.
            </Text>
            <div className="contact-form-grid">
              <Input
                type="email"
                prefix={<MailOutlined />}
                placeholder="Email"
                value={contactInfo.email}
                maxLength={254}
                onChange={e => setContactInfo({ ...contactInfo, email: e.target.value })}
              />
              <Input
                prefix={<PhoneOutlined />}
                placeholder="Số điện thoại"
                value={contactInfo.phone}
                maxLength={30}
                onChange={e => setContactInfo({ ...contactInfo, phone: e.target.value })}
              />
              <Input
                prefix={<EnvironmentOutlined />}
                placeholder="Địa chỉ (VD: Cần Thơ, Việt Nam)"
                value={contactInfo.address}
                maxLength={200}
                onChange={e => setContactInfo({ ...contactInfo, address: e.target.value })}
              />
              <Input
                prefix={<GithubOutlined />}
                placeholder="github.com/ten-tai-khoan"
                value={contactInfo.github}
                maxLength={300}
                onChange={e => setContactInfo({ ...contactInfo, github: e.target.value })}
              />
              <Input
                prefix={<LinkedinOutlined />}
                placeholder="linkedin.com/in/ten-tai-khoan"
                value={contactInfo.linkedin}
                maxLength={300}
                onChange={e => setContactInfo({ ...contactInfo, linkedin: e.target.value })}
              />
            </div>
          </div>

          <div className="form-section">
            <label className="form-label">
              <FileTextOutlined /> Mô tả công việc (Job Description)
              <span className="char-count">{jobDescription.length}/{MAX_JD_LENGTH}</span>
            </label>
            <TextArea
              placeholder="Dán nội dung mô tả công việc từ nhà tuyển dụng..."
              value={jobDescription}
              onChange={e => setTargetJd(targetRole, e.target.value.slice(0, MAX_JD_LENGTH), careerFocusTag)}
              rows={8}
              showCount
              maxLength={MAX_JD_LENGTH}
            />
          </div>

          <div className="step-actions">
            <Button
              type="primary"
              size="large"
              icon={isLoadingContext ? <Spin size="small" /> : <SearchOutlined />}
              onClick={handleAnalyze}
              disabled={!isValid || isLoadingContext}
              loading={isLoadingContext}
            >
              {isLoadingContext ? 'Đang phân tích...' : 'Phân tích JD & Tìm môn học phù hợp ➔'}
            </Button>
            {isLoadingContext && (
              <Text type="secondary" className="loading-text">
                Đang phân tích từ khóa và tìm kiếm môn học phù hợp...
              </Text>
            )}
          </div>
        </Space>
      </Card>
    </div>
  )
}
