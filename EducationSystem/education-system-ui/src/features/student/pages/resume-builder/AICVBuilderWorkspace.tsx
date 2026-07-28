import {
  CheckCircleFilled,
  DeleteOutlined,
  DownloadOutlined,
  ExperimentOutlined,
  GithubOutlined,
  InfoCircleOutlined,
  PlusOutlined,
  RobotOutlined,
  SafetyCertificateOutlined,
} from '@ant-design/icons'
import { useMutation, useQuery } from '@tanstack/react-query'
import { Button, Checkbox, Collapse, Empty, Input, Progress, Radio, Select, Spin, Tooltip, message } from 'antd'
import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { httpClient } from '../../../../shared/api/httpClient'
import type { ApiResponse } from '../../../../shared/types/api'
import { useStudentAuth } from '../../studentAuth'

type Course = {
  id: string
  code: string
  name: string
  score: number
  semester: string
  competency: string
}

type Project = {
  name: string
  techStack: string
  githubUrl: string
  scale: 'solo' | 'team'
  role: string
  contribution: string
}

type Internship = {
  id: string
  company: string
  position: string
  startDate: string
  endDate?: string | null
  duration: string
  responsibilities: string
  source: 'self-declared' | 'school-approved'
}

type ApprovedInternshipApiDto = {
  internshipId: number
  studentId: string
  companyName: string
  position: string
  startDate: string
  endDate?: string | null
  taskDescription?: string | null
  formRequestId?: string | null
}

type ResumeData = {
  studentInfo: {
    fullName: string
    studentCode: string
    email: string
    phone: string
    location: string
    major: string
    university: string
    gpa: number
  }
  selectedCourses: string[]
  projects: Project[]
  internships: Internship[]
  aiContext: {
    targetRole: string
    jobDescription: string
  }
  cvOutput: {
    professionalSummary: string
    skills: {
      knowledge: string[]
      functional: string[]
      interpersonal: string[]
    }
  }
}

type ResumeContextResponse = {
  student: {
    studentId: string
    fullName: string
    userName: string
    majorName: string
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
    projectName: string
    techStack: string
    sourceCodeUrl?: string | null
    teamSize: number
    myRole?: string | null
    myContributions?: string | null
  }>
  approvedInternships: Array<{
    companyName: string
    position: string
    startDate: string
    endDate?: string | null
    taskDescription?: string | null
  }>
}

type AIOptimizeResponse = {
  cvOutput: ResumeData['cvOutput']
  metrics?: {
    contentPreservation?: number
    jobAlignment?: number
  }
}

const academicApiOrigin = import.meta.env.VITE_ACADEMIC_API_ORIGIN ?? 'http://localhost:5002'
const aiApiOrigin = import.meta.env.VITE_AI_API_ORIGIN ?? 'http://localhost:5005'
// Keep mock mode as the safe default. Set VITE_RESUME_API_MODE=live after the APIs are configured.
const useLiveResumeApi = import.meta.env.VITE_RESUME_API_MODE === 'live'
// Academic data can run in live mode independently; LLM calls remain disabled until explicitly enabled.
const useLiveAIOptimization = import.meta.env.VITE_AI_OPTIMIZE_MODE === 'live'

async function getResumeContext(studentId: string) {
  const response = await httpClient.get<ApiResponse<ResumeContextResponse>>(
    `${academicApiOrigin}/api/academic/resume/get-context-data/${studentId}`,
  )
  return response.data.data
}

async function getApprovedInternships(studentId: string) {
  const response = await httpClient.get<ApiResponse<ApprovedInternshipApiDto[]>>(
    `${academicApiOrigin}/api/academic/students/${studentId}/internships`,
  )
  return response.data.data
}

async function optimizeResume(payload: ResumeData) {
  // Phase 2 does not yet expose an LLM endpoint. Point VITE_AI_API_ORIGIN to the
  // future AI service and adjust only this adapter if its request/response contract differs.
  const response = await httpClient.post<ApiResponse<AIOptimizeResponse>>(
    `${aiApiOrigin}/api/ai/resume/optimize`,
    payload,
  )
  return response.data.data
}

function mapContextToResumeData(context: ResumeContextResponse, current: ResumeData): ResumeData {
  return {
    ...current,
    studentInfo: {
      ...current.studentInfo,
      fullName: context.student.fullName,
      major: context.student.majorName,
      gpa: context.gpa ?? current.studentInfo.gpa,
    },
    selectedCourses: context.eligibleCourses.map(course => course.subjectId),
    projects: context.projects.map(project => ({
      name: project.projectName,
      techStack: project.techStack,
      githubUrl: project.sourceCodeUrl ?? '',
      scale: project.teamSize > 1 ? 'team' : 'solo',
      role: project.myRole ?? '',
      contribution: project.myContributions ?? '',
    })),
  }
}

function formatInternshipDate(value?: string | null) {
  if (!value) return 'Hiện tại'
  const [year, month, day] = value.slice(0, 10).split('-')
  return day && month && year ? `${day}/${month}/${year}` : value
}

function mapApprovedInternship(item: ApprovedInternshipApiDto): Internship {
  return {
    id: String(item.internshipId),
    company: item.companyName,
    position: item.position,
    startDate: item.startDate,
    endDate: item.endDate,
    duration: `${formatInternshipDate(item.startDate)} – ${formatInternshipDate(item.endDate)}`,
    responsibilities: item.taskDescription ?? '',
    source: 'school-approved',
  }
}

const eligibleCourses: Course[] = [
  { id: 'web', code: 'CT312', name: 'Lập trình Web', score: 8.2, semester: 'HK1 · 2025–2026', competency: 'Phát triển ứng dụng web' },
  { id: 'database', code: 'CT311', name: 'Cơ sở dữ liệu', score: 7.8, semester: 'HK2 · 2024–2025', competency: 'Thiết kế và tối ưu dữ liệu' },
  { id: 'network', code: 'CT205', name: 'Mạng máy tính', score: 7.8, semester: 'HK2 · 2024–2025', competency: 'Hạ tầng và giao thức mạng' },
  { id: 'software', code: 'CT300', name: 'Công nghệ phần mềm', score: 8.2, semester: 'HK1 · 2025–2026', competency: 'Quy trình phát triển phần mềm' },
  { id: 'analysis', code: 'CT296', name: 'Phân tích thiết kế hệ thống', score: 7.9, semester: 'HK1 · 2024–2025', competency: 'Phân tích yêu cầu nghiệp vụ' },
]

const initialResumeData: ResumeData = {
  studentInfo: {
    fullName: 'Nguyễn Trọng Nghĩa',
    studentCode: '23DTH118',
    email: 'trongnghia.23dth@student.tdu.edu.vn',
    phone: '093 812 2406',
    location: 'Cần Thơ, Việt Nam',
    major: 'Công nghệ thông tin',
    university: 'Đại học Tây Đô',
    gpa: 7.54,
  },
  selectedCourses: ['web', 'database', 'software'],
  projects: [{
    name: 'EducationSystem – Cổng thông tin đại học',
    techStack: 'React 19, TypeScript, ASP.NET Core, PostgreSQL',
    githubUrl: 'github.com/minhanh/education-system',
    scale: 'team',
    role: 'Frontend Developer',
    contribution: 'Xây dựng portal sinh viên, thiết kế component tái sử dụng và tối ưu trải nghiệm tra cứu học tập.',
  }],
  internships: [],
  aiContext: {
    targetRole: 'Frontend Developer Intern',
    jobDescription: 'Tìm kiếm ứng viên có nền tảng React, TypeScript, REST API, tư duy sản phẩm và khả năng làm việc nhóm.',
  },
  cvOutput: {
    professionalSummary: 'Sinh viên Công nghệ thông tin định hướng Frontend Development, có nền tảng vững về React, TypeScript và thiết kế hệ thống. Chủ động biến yêu cầu nghiệp vụ thành trải nghiệm web rõ ràng, dễ sử dụng.',
    skills: {
      knowledge: ['React & TypeScript', 'Cơ sở dữ liệu quan hệ', 'RESTful API & kiến trúc web'],
      functional: ['Xây dựng giao diện responsive', 'Git & quy trình làm việc nhóm', 'Phân tích yêu cầu'],
      interpersonal: ['Giao tiếp chủ động', 'Tư duy giải quyết vấn đề', 'Học hỏi nhanh'],
    },
  },
}

const newProject = (): Project => ({
  name: '',
  techStack: '',
  githubUrl: '',
  scale: 'solo',
  role: '',
  contribution: '',
})

export function AICVBuilderWorkspace() {
  const { session } = useStudentAuth()
  const [resumeData, setResumeData] = useState<ResumeData>(initialResumeData)
  const [selectedInternshipIds, setSelectedInternshipIds] = useState<string[]>([])
  const [isOptimizing, setIsOptimizing] = useState(false)
  const [jobAlignment, setJobAlignment] = useState(65)
  const [contentPreservation, setContentPreservation] = useState(95)
  const [savedAt, setSavedAt] = useState('Vừa xong')
  const [messageApi, messageContext] = message.useMessage()

  const contextQuery = useQuery({
    queryKey: ['student', 'resume-context', session?.studentId],
    queryFn: () => getResumeContext(session!.studentId),
    enabled: useLiveResumeApi && Boolean(session?.studentId),
  })

  const approvedInternshipsQuery = useQuery({
    queryKey: ['student', 'approved-internships', session?.studentId],
    queryFn: () => getApprovedInternships(session!.studentId),
    enabled: Boolean(session?.studentId),
    staleTime: 60_000,
  })

  const optimizeMutation = useMutation({
    mutationFn: optimizeResume,
    onSuccess: result => {
      setResumeData(current => ({ ...current, cvOutput: result.cvOutput }))
      setSavedAt('Vừa xong')
      setContentPreservation(result.metrics?.contentPreservation ?? 95)
      setJobAlignment(result.metrics?.jobAlignment ?? 88)
      messageApi.success('CV đã được tối ưu theo mô tả công việc')
    },
    onError: () => messageApi.error('Không thể tối ưu CV. Vui lòng thử lại.'),
  })

  useEffect(() => {
    if (contextQuery.data) {
      setResumeData(current => mapContextToResumeData(contextQuery.data, current))
      setSavedAt('Đã đồng bộ API')
    }
  }, [contextQuery.data])

  const updateResume = (updater: (current: ResumeData) => ResumeData) => {
    setResumeData(current => updater(current))
    setSavedAt('Vừa xong')
  }

  const handleSubjectSelectionChange = (courseId: string, checked: boolean) => {
    updateResume(current => ({
      ...current,
      selectedCourses: checked
        ? [...new Set([...current.selectedCourses, courseId])]
        : current.selectedCourses.filter(id => id !== courseId),
    }))
  }

  const handleProjectChange = (index: number, field: keyof Project, value: string) => {
    updateResume(current => ({
      ...current,
      projects: current.projects.map((project, projectIndex) =>
        projectIndex === index ? { ...project, [field]: value } : project),
    }))
  }

  const handleInternshipSelectionChange = (
    source: ApprovedInternshipApiDto,
    checked: boolean,
  ) => {
    const id = String(source.internshipId)
    setSelectedInternshipIds(current =>
      checked ? [...new Set([...current, id])] : current.filter(item => item !== id))
    updateResume(current => ({
      ...current,
      internships: checked
        ? current.internships.some(item => item.id === id)
          ? current.internships
          : [...current.internships, mapApprovedInternship(source)]
        : current.internships.filter(item => item.id !== id),
    }))
  }

  const handleInternshipInlineEdit = (
    id: string,
    field: 'company' | 'position' | 'duration' | 'responsibilities',
    newValue: string,
  ) => {
    const value = newValue.trim()
    updateResume(current => ({
      ...current,
      internships: current.internships.map(internship =>
        internship.id === id ? { ...internship, [field]: value } : internship),
    }))
  }

  const handleInlineEdit = (statePath: string, newValue: string) => {
    const value = newValue.trim()
    if (!value) return
    const [section, ...pathParts] = statePath.split('.')
    const path = pathParts.join('.')
    updateResume(current => {
      if (section === 'studentInfo') {
        return { ...current, studentInfo: { ...current.studentInfo, [path]: value } }
      }
      if (section === 'aiContext') {
        return { ...current, aiContext: { ...current.aiContext, [path]: value } }
      }
      if (section === 'cvOutput' && path === 'professionalSummary') {
        return { ...current, cvOutput: { ...current.cvOutput, professionalSummary: value } }
      }
      if (section === 'skills') {
        const [group, itemIndex] = path.split('.')
        const key = group as keyof ResumeData['cvOutput']['skills']
        return {
          ...current,
          cvOutput: {
            ...current.cvOutput,
            skills: {
              ...current.cvOutput.skills,
              [key]: current.cvOutput.skills[key].map((item, index) => index === Number(itemIndex) ? value : item),
            },
          },
        }
      }
      if (section === 'projects') {
        const [itemIndex, field] = path.split('.')
        return {
          ...current,
          projects: current.projects.map((project, index) =>
            index === Number(itemIndex) ? { ...project, [field]: value } : project),
        }
      }
      return current
    })
  }

  const handleProjectSubmit = (projectData: Project) => {
    updateResume(current => ({
      ...current,
      projects: [...current.projects, projectData],
    }))
  }

  const handleAIOptimize = () => {
    if (isOptimizing || optimizeMutation.isPending) return
    if (useLiveAIOptimization) {
      optimizeMutation.mutate(resumeData)
      return
    }
    setIsOptimizing(true)
    window.setTimeout(() => {
      updateResume(current => ({
        ...current,
        cvOutput: {
          professionalSummary: `Sinh viên ${current.studentInfo.major} định hướng ${current.aiContext.targetRole}, có kinh nghiệm xây dựng sản phẩm thực tế với React và TypeScript. Vận dụng tốt nền tảng cơ sở dữ liệu, REST API và tư duy lấy người dùng làm trung tâm để tạo ra giao diện ổn định, dễ mở rộng.`,
          skills: {
            knowledge: ['React 19 & TypeScript', 'REST API & kiến trúc web', 'SQL & mô hình dữ liệu'],
            functional: ['Phát triển UI responsive, chuẩn accessibility', 'Git, code review & Agile teamwork', 'Phân tích và chuyển hóa yêu cầu nghiệp vụ'],
            interpersonal: ['Giao tiếp rõ ràng', 'Chủ động giải quyết vấn đề', 'Thích nghi và học hỏi nhanh'],
          },
        },
      }))
      setJobAlignment(88)
      setIsOptimizing(false)
      messageApi.success('CV đã được tối ưu theo mô tả công việc')
    }, 2000)
  }

  const handlePrint = () => {
    const paper = document.getElementById('printable-resume')
    if (!paper) return

    // Fit user-edited content to one physical A4 page. scrollHeight includes any
    // content currently clipped by the screen preview's fixed A4 aspect ratio.
    const fitRatio = Math.min(1, paper.clientHeight / Math.max(paper.scrollHeight, 1))
    paper.style.setProperty('--print-content-scale', fitRatio.toFixed(4))
    window.addEventListener(
      'afterprint',
      () => paper.style.removeProperty('--print-content-scale'),
      { once: true },
    )
    window.print()
  }

  const inlineProps = (section: string, path: string) => ({
    contentEditable: true,
    suppressContentEditableWarning: true,
    onBlur: (event: React.FocusEvent<HTMLElement>) => handleInlineEdit(`${section}.${path}`, event.currentTarget.textContent ?? ''),
  })

  const availableCourses: Course[] = contextQuery.data?.eligibleCourses.map(course => ({
    id: course.subjectId,
    code: course.subjectCode,
    name: course.subjectName,
    score: course.score,
    semester: 'Từ dữ liệu học vụ',
    competency: course.courseOutcomes.map(outcome => outcome.name).join(', '),
  })) ?? eligibleCourses
  const selectedCourseData = availableCourses.filter(course => resumeData.selectedCourses.includes(course.id))
  const isAILoading = isOptimizing || optimizeMutation.isPending

  const accordionItems = [
    {
      key: 'context',
      label: <AccordionLabel step="01" title="Vị trí & JD tuyển dụng" subtitle="Giúp AI hiểu mục tiêu nghề nghiệp" />,
      children: <div className="resume-form-stack">
        <Field label="Vị trí mong muốn" hint="Chức danh bạn đang ứng tuyển">
          <Input value={resumeData.aiContext.targetRole} placeholder="Ví dụ: Frontend Developer Intern" onChange={event => updateResume(current => ({ ...current, aiContext: { ...current.aiContext, targetRole: event.target.value } }))} />
        </Field>
        <Field label="Mô tả công việc – JD" hint={`${resumeData.aiContext.jobDescription.length}/1.500 ký tự`}>
          <Input.TextArea value={resumeData.aiContext.jobDescription} rows={5} maxLength={1500} placeholder="Dán mô tả công việc để AI căn chỉnh từ khóa..." onChange={event => updateResume(current => ({ ...current, aiContext: { ...current.aiContext, jobDescription: event.target.value } }))} />
        </Field>
        <div className="resume-tip"><ExperimentOutlined /><span><b>Mẹo nhỏ</b>JD càng đầy đủ, AI càng tối ưu nội dung sát yêu cầu nhà tuyển dụng.</span></div>
      </div>,
    },
    {
      key: 'courses',
      label: <AccordionLabel step="02" title="Học phần nền tảng" subtitle={`${resumeData.selectedCourses.length} học phần đã chọn · GPA ${resumeData.studentInfo.gpa.toFixed(2)}`} />,
      children: <div className="resume-course-list">
        <div className="resume-vqf-note"><SafetyCertificateOutlined /><span><b>Khung năng lực VQF Bậc 6</b>Chọn các học phần phản ánh tốt nhất năng lực chuyên môn của bạn.</span></div>
        {availableCourses.map(course => <label className="resume-course-option" key={course.id}>
          <Checkbox checked={resumeData.selectedCourses.includes(course.id)} onChange={event => handleSubjectSelectionChange(course.id, event.target.checked)} />
          <span className="resume-course-main"><b>{course.name}</b><small>{course.code} · {course.semester}</small></span>
          <span className="resume-course-score">{course.score.toFixed(1)}</span>
        </label>)}
      </div>,
    },
    {
      key: 'projects',
      label: <AccordionLabel step="03" title="Dự án thực tế" subtitle={`${resumeData.projects.length} dự án đã khai báo`} />,
      children: <div className="resume-form-stack">
        {resumeData.projects.map((project, index) => <div className="resume-dynamic-card" key={index}>
          <div className="resume-dynamic-head"><span>Dự án {index + 1}</span><Button type="text" danger aria-label={`Xóa dự án ${index + 1}`} icon={<DeleteOutlined />} onClick={() => updateResume(current => ({ ...current, projects: current.projects.filter((_, itemIndex) => itemIndex !== index) }))} /></div>
          <Field label="Tên dự án"><Input value={project.name} placeholder="Tên sản phẩm hoặc dự án" onChange={event => handleProjectChange(index, 'name', event.target.value)} /></Field>
          <Field label="Công nghệ (Tech stack)"><Input value={project.techStack} placeholder="React, TypeScript, ASP.NET Core..." onChange={event => handleProjectChange(index, 'techStack', event.target.value)} /></Field>
          <div className="resume-form-grid">
            <Field label="Link GitHub"><Input prefix={<GithubOutlined />} value={project.githubUrl} placeholder="github.com/..." onChange={event => handleProjectChange(index, 'githubUrl', event.target.value)} /></Field>
            <Field label="Quy mô"><Radio.Group optionType="button" buttonStyle="solid" value={project.scale} options={[{ label: 'Cá nhân', value: 'solo' }, { label: 'Nhóm', value: 'team' }]} onChange={event => handleProjectChange(index, 'scale', event.target.value)} /></Field>
          </div>
          {project.scale === 'team' && <>
            <Field label="Vai trò của bạn"><Input value={project.role} placeholder="Ví dụ: Frontend Developer" onChange={event => handleProjectChange(index, 'role', event.target.value)} /></Field>
            <Field label="Đóng góp cá nhân thực tế"><Input.TextArea rows={3} value={project.contribution} placeholder="Bạn đã trực tiếp làm gì và tạo ra kết quả nào?" onChange={event => handleProjectChange(index, 'contribution', event.target.value)} /></Field>
          </>}
        </div>)}
        <Button className="resume-add-button" icon={<PlusOutlined />} onClick={() => handleProjectSubmit(newProject())}>Thêm dự án</Button>
      </div>,
    },
    {
      key: 'internships',
      label: <AccordionLabel step="04" title="Thực tập & Kinh nghiệm" subtitle={`${selectedInternshipIds.length} đợt đã chọn`} />,
      children: <div className="resume-form-stack">
        {approvedInternshipsQuery.isLoading && (
          <div className="resume-internship-loading"><Spin size="small" /><span>Đang tải lịch sử thực tập đã duyệt...</span></div>
        )}
        {approvedInternshipsQuery.isError && (
          <Empty
            image={Empty.PRESENTED_IMAGE_SIMPLE}
            description="Không thể tải lịch sử thực tập. Vui lòng thử lại sau."
          />
        )}
        {approvedInternshipsQuery.isSuccess && approvedInternshipsQuery.data.length === 0 && (
          <div className="resume-internship-empty">
            <Empty
              image={Empty.PRESENTED_IMAGE_SIMPLE}
              description={(
                <span>
                  Bạn chưa có lịch sử thực tập được phê duyệt. Hãy vào phần{' '}
                  <Link to="/communication/form-requests">Yêu cầu biểu mẫu</Link>{' '}
                  để gửi đơn xác nhận thực tập trước.
                </span>
              )}
            />
          </div>
        )}
        {approvedInternshipsQuery.isSuccess && approvedInternshipsQuery.data.length > 0 && (
          <div className="resume-approved-internships">
            {approvedInternshipsQuery.data.map(internship => {
              const id = String(internship.internshipId)
              const selected = selectedInternshipIds.includes(id)
              return (
                <label className={`resume-approved-internship${selected ? ' selected' : ''}`} key={id}>
                  <Checkbox
                    checked={selected}
                    onChange={event => handleInternshipSelectionChange(internship, event.target.checked)}
                  />
                  <span className="resume-approved-internship-copy">
                    <b>Thực tập sinh {internship.position} tại {internship.companyName}</b>
                    <small>{formatInternshipDate(internship.startDate)} – {formatInternshipDate(internship.endDate)}</small>
                    {internship.taskDescription && <span>{internship.taskDescription}</span>}
                  </span>
                  <SafetyCertificateOutlined title="Đã được nhà trường phê duyệt" />
                </label>
              )
            })}
          </div>
        )}
      </div>,
    },
  ]

  return <div className="resume-builder-page grid overflow-hidden rounded-2xl bg-slate-100">
    {messageContext}
    <aside className="resume-builder-panel resume-editor-panel bg-slate-50">
      <div className="resume-builder-heading">
        <div><span className="resume-eyebrow"><RobotOutlined /> AI-POWERED RESUME</span><h1>Thiết kế CV thông minh</h1><p>Khai báo dữ liệu thật, AI sẽ giúp bạn diễn đạt tốt hơn.</p></div>
        <span className="resume-save-state"><CheckCircleFilled /> Đã lưu · {savedAt}</span>
      </div>
      <div className="resume-progress-overview"><span><b>Hoàn thiện hồ sơ</b><small>4/4 mục</small></span><Progress percent={100} showInfo={false} strokeColor="#0B3A60" trailColor="#e7edf3" /></div>
      <Collapse className="resume-accordions" bordered={false} defaultActiveKey={['context', 'courses', 'projects', 'internships']} expandIconPosition="end" items={accordionItems} />
      <div className="resume-ai-action">
        <Button size="large" type="primary" icon={<RobotOutlined />} loading={isAILoading} onClick={handleAIOptimize}>Tối ưu hóa CV bằng AI</Button>
        <span>AI chỉ gọt giũa cách diễn đạt, không tự tạo kinh nghiệm.</span>
      </div>
    </aside>

    <main className="resume-builder-panel resume-preview-panel bg-slate-100">
      <div className="resume-preview-toolbar">
        <div><b>Bản xem trước</b><span>A4 · ATS-friendly · 1 trang</span></div>
        <div className="resume-toolbar-actions"><Select size="small" value="vi" options={[{ value: 'vi', label: 'Tiếng Việt' }]} /><Button icon={<DownloadOutlined />} type="primary" onClick={handlePrint}>In CV / Xuất PDF</Button></div>
      </div>
      <div className="resume-metrics">
        <Metric label="Content Preservation" sublabel="Độ trung thực dữ liệu" percent={contentPreservation} tone="green" tooltip={<><b>Token Space Overlap</b><br />Tỷ lệ token/ngữ nghĩa từ dữ liệu nguồn còn được bảo toàn trong CV sau tối ưu: |T<sub>source</sub> ∩ T<sub>CV</sub>| / |T<sub>source</sub>|.</>} />
        <Metric label="Job Alignment" sublabel="Độ khớp JD" percent={jobAlignment} tone="navy" tooltip={<><b>Latent Space Cosine Similarity</b><br />Độ tương đồng giữa vector embedding của CV và JD: cos(θ) = (v<sub>CV</sub> · v<sub>JD</sub>) / (‖v<sub>CV</sub>‖‖v<sub>JD</sub>‖).</>} />
      </div>
      <div className="resume-paper-stage">
        <Spin spinning={isAILoading} indicator={<RobotOutlined spin />} tip={<span>AI đang phân tích JD và tối ưu nội dung…</span>}>
          <article className="resume-paper aspect-[1/1.414] w-[210mm] bg-white shadow-2xl" id="printable-resume">
            <header className="cv-header">
              <h1 {...inlineProps('studentInfo', 'fullName')}>{resumeData.studentInfo.fullName}</h1>
              <h2 {...inlineProps('aiContext', 'targetRole')}>{resumeData.aiContext.targetRole}</h2>
              <p><span {...inlineProps('studentInfo', 'email')}>{resumeData.studentInfo.email}</span><i>•</i><span {...inlineProps('studentInfo', 'phone')}>{resumeData.studentInfo.phone}</span><i>•</i><span {...inlineProps('studentInfo', 'location')}>{resumeData.studentInfo.location}</span></p>
            </header>
            <CvSection title="Tóm tắt chuyên môn">
              <p className="cv-summary" {...inlineProps('cvOutput', 'professionalSummary')}>{resumeData.cvOutput.professionalSummary}</p>
            </CvSection>
            <CvSection title="Năng lực chuyên môn">
              <div className="cv-skills">
                <SkillLine label="Kiến thức" items={resumeData.cvOutput.skills.knowledge} group="knowledge" inlineProps={inlineProps} />
                <SkillLine label="Kỹ năng nghề nghiệp" items={resumeData.cvOutput.skills.functional} group="functional" inlineProps={inlineProps} />
                <SkillLine label="Kỹ năng cá nhân" items={resumeData.cvOutput.skills.interpersonal} group="interpersonal" inlineProps={inlineProps} />
              </div>
            </CvSection>
            <CvSection title="Dự án nổi bật">
              {resumeData.projects.filter(project => project.name).map((project, index) => <div className="cv-entry" key={index}>
                <div className="cv-entry-title"><b {...inlineProps('projects', `${index}.name`)}>{project.name}</b><span {...inlineProps('projects', `${index}.role`)}>{project.scale === 'team' ? project.role : 'Dự án cá nhân'}</span></div>
                <em {...inlineProps('projects', `${index}.techStack`)}>{project.techStack}</em>
                <ul><li {...inlineProps('projects', `${index}.contribution`)}>{project.contribution || 'Phân tích, xây dựng và hoàn thiện các chức năng cốt lõi của dự án.'}</li></ul>
              </div>)}
            </CvSection>
            {resumeData.internships.length > 0 && (
              <CvSection title="Kinh nghiệm làm việc">
                {resumeData.internships.map(internship => (
                  <div className="cv-entry cv-internship-entry" key={internship.id}>
                    <div className="cv-entry-title">
                      <b
                        contentEditable
                        suppressContentEditableWarning
                        onBlur={event => handleInternshipInlineEdit(internship.id, 'position', event.currentTarget.textContent ?? '')}
                      >
                        {internship.position}
                      </b>
                      <span
                        contentEditable
                        suppressContentEditableWarning
                        onBlur={event => handleInternshipInlineEdit(internship.id, 'duration', event.currentTarget.textContent ?? '')}
                      >
                        {internship.duration}
                      </span>
                    </div>
                    <em
                      contentEditable
                      suppressContentEditableWarning
                      onBlur={event => handleInternshipInlineEdit(internship.id, 'company', event.currentTarget.textContent ?? '')}
                    >
                      {internship.company}
                    </em>
                    <ul>
                      <li
                        contentEditable
                        suppressContentEditableWarning
                        onBlur={event => handleInternshipInlineEdit(internship.id, 'responsibilities', event.currentTarget.textContent ?? '')}
                      >
                        {internship.responsibilities || 'Chưa có mô tả nhiệm vụ.'}
                      </li>
                    </ul>
                  </div>
                ))}
              </CvSection>
            )}
            <CvSection title="Học vấn & học phần tiêu biểu">
              <div className="cv-entry-title"><b {...inlineProps('studentInfo', 'university')}>{resumeData.studentInfo.university}</b><span>2023 – 2027</span></div>
              <p><span {...inlineProps('studentInfo', 'major')}>{resumeData.studentInfo.major}</span> · GPA {resumeData.studentInfo.gpa.toFixed(2)} · MSSV {resumeData.studentInfo.studentCode}</p>
              <p className="cv-course-line">{selectedCourseData.map(course => `${course.name} (${course.score.toFixed(1)})`).join('  •  ')}</p>
            </CvSection>
          </article>
        </Spin>
      </div>
      <p className="resume-edit-hint">Nhấp trực tiếp vào nội dung trên CV để chỉnh sửa · Thay đổi được tự động lưu</p>
    </main>
  </div>
}

export const StudentResumeBuilderPage = AICVBuilderWorkspace

function AccordionLabel({ step, title, subtitle }: { step: string; title: string; subtitle: string }) {
  return <div className="resume-accordion-label"><span>{step}</span><div><b>{title}</b><small>{subtitle}</small></div></div>
}

function Field({ label, hint, children }: { label: string; hint?: string; children: React.ReactNode }) {
  return <label className="resume-field"><span>{label}{hint && <small>{hint}</small>}</span>{children}</label>
}

function Metric({ label, sublabel, percent, tone, tooltip }: { label: string; sublabel: string; percent: number; tone: 'green' | 'navy'; tooltip: React.ReactNode }) {
  return <div className="resume-metric"><div><span><b>{label}<Tooltip title={tooltip} placement="bottom"><InfoCircleOutlined className="resume-metric-info" aria-label={`Giải thích ${label}`} /></Tooltip></b><small>{sublabel}</small></span><strong className={`resume-metric-${tone}`}>{percent}%</strong></div><Progress percent={percent} showInfo={false} strokeColor={tone === 'green' ? '#12a37f' : '#0B3A60'} trailColor="#e5eaf0" /></div>
}

function CvSection({ title, children }: { title: string; children: React.ReactNode }) {
  return <section className="cv-section"><h3>{title}</h3>{children}</section>
}

function SkillLine({ label, items, group, inlineProps }: { label: string; items: string[]; group: string; inlineProps: (section: string, path: string) => Record<string, unknown> }) {
  return <p><b>{label}:</b> {items.map((item, index) => <span key={`${group}-${index}`}><span {...inlineProps('skills', `${group}.${index}`)}>{item}</span>{index < items.length - 1 && ' · '}</span>)}</p>
}
