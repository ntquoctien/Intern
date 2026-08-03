import {
  CompressOutlined,
  FullscreenExitOutlined,
  FullscreenOutlined,
  MinusOutlined,
  PlusOutlined,
  PrinterOutlined,
  WarningOutlined,
} from '@ant-design/icons'
import { Button, Empty, Spin, Tag, Tooltip } from 'antd'
import { useEffect, useRef, useState } from 'react'
import type { FocusEvent } from 'react'
import { useResumeStore } from '../hooks/useResumeStore'
import type {
  AwardActivityItem,
  ApprovedInternship,
  CertificationItem,
  EligibleCourse,
  OptimizedBulletSection,
  OptimizedResumeResponseDto,
  OptimizedSkillGroup,
  ResumeContactInfo,
  UiProjectOverride,
} from '../types'
import { PRINTABLE_RESUME_ID, printA4Resume } from '../utils/printA4Resume'

const A4_WIDTH_PX = 794 // A4 width at 96dpi
const A4_HEIGHT_PX = 1123 // A4 height at 96dpi
const VIEWPORT_PADDING_PX = 48 // padding 2 phía của .a4-viewport

type EditableTextProps = {
  value: string
  onCommit: (value: string) => void
  as?: 'span' | 'p' | 'li' | 'strong' | 'div'
  className?: string
}

/** Text chỉnh sửa trực tiếp trên CV — commit khi blur, khôi phục giá trị cũ nếu để trống. */
function EditableText({ value, onCommit, as = 'span', className }: EditableTextProps) {
  const TagName = as
  const handleBlur = (event: FocusEvent<HTMLElement>) => {
    const next = event.currentTarget.textContent?.trim() ?? ''
    if (next && next !== value) {
      onCommit(next)
    } else if (!next) {
      event.currentTarget.textContent = value
    }
  }
  return (
    <TagName
      className={className}
      contentEditable
      suppressContentEditableWarning
      spellCheck={false}
      onBlur={handleBlur}
    >
      {value}
    </TagName>
  )
}
type A4PaperPreviewProps = {
  showToolbar?: boolean
}

export function A4PaperPreview({ showToolbar = true }: A4PaperPreviewProps) {
  const {
    state,
    setZoomLevel,
    updateOptimizedSummary,
    updateOptimizedSkill,
    updateOptimizedBullet,
  } = useResumeStore()
  const {
    zoomLevel,
    optimizedCvResult,
    isOptimizingAi,
    contextLoaded,
    studentInfo,
    contactInfo,
    targetRole,
    selectedSubjectIds,
    eligibleCourses,
    uiProjects,
    selectedInternshipIds,
    approvedInternships,
    certifications,
    awardsAndActivities,
  } = state

  const viewportRef = useRef<HTMLDivElement>(null)
  const paperRef = useRef<HTMLDivElement>(null)
  const [isFullscreen, setIsFullscreen] = useState(false)
  const [fitScale, setFitScale] = useState(1)
  const [isOverflowing, setIsOverflowing] = useState(false)

  // Auto-fit toàn bộ trang A4 theo cả chiều rộng và chiều cao của viewport.
  useEffect(() => {
    const viewport = viewportRef.current
    if (!viewport) return
    const updateFitScale = () => {
      const availableWidth = Math.max(viewport.clientWidth - VIEWPORT_PADDING_PX, 1)
      const availableHeight = Math.max(viewport.clientHeight - VIEWPORT_PADDING_PX, 1)
      setFitScale(Math.min(
        availableWidth / A4_WIDTH_PX,
        availableHeight / A4_HEIGHT_PX,
        1,
      ))
    }
    updateFitScale()
    const observer = new ResizeObserver(updateFitScale)
    observer.observe(viewport)
    return () => observer.disconnect()
  }, [])

  // ESC để thoát chế độ toàn màn hình.
  useEffect(() => {
    if (!isFullscreen) return
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') setIsFullscreen(false)
    }
    window.addEventListener('keydown', handleKeyDown)
    return () => window.removeEventListener('keydown', handleKeyDown)
  }, [isFullscreen])

  // Phát hiện nội dung vượt quá 1 trang A4. transform scale không ảnh hưởng
  // layout nên scrollHeight/clientHeight phản ánh đúng kích thước thật.
  // Chạy sau mỗi render vì nội dung CV đổi từ nhiều nguồn (chọn môn, sửa dự án, AI...).
  useEffect(() => {
    const paper = paperRef.current
    if (paper) setIsOverflowing(paper.scrollHeight > paper.clientHeight + 1)
  }, [state])

  const effectiveScale = zoomLevel ?? fitScale
  const zoomPercent = Math.round(effectiveScale * 100)
  const selectedCertifications = certifications.filter(item => item.isSelectedForCv)
  const selectedAwards = awardsAndActivities.filter(item => item.isSelectedForCv)
  const roundToStep = (value: number) => Math.round(value * 10) / 10
  const handleZoomIn = () => setZoomLevel(roundToStep(effectiveScale + 0.1))
  const handleZoomOut = () => setZoomLevel(roundToStep(effectiveScale - 0.1))
  const handleFitToScreen = () => setZoomLevel(null)

  const renderPaperContent = () => {
    if (optimizedCvResult) {
      return (
        <OptimizedCvContent
          cv={optimizedCvResult}
          contactInfo={contactInfo}
          selectedCourses={eligibleCourses.filter(c => selectedSubjectIds.includes(c.subjectId))}
          onSummaryCommit={updateOptimizedSummary}
          onSkillCommit={updateOptimizedSkill}
          onBulletCommit={updateOptimizedBullet}
        />
      )
    }
    if (!contextLoaded) {
      return (
        <div className="cv-empty-state">
          <Empty
            image={Empty.PRESENTED_IMAGE_SIMPLE}
            description="Nhập mục tiêu & JD ở Bước 1 rồi nhấn Phân tích JD — bản nháp CV sẽ hiển thị tại đây."
          />
        </div>
      )
    }
    return (
      <DraftCvContent
        studentInfo={studentInfo}
        contactInfo={contactInfo}
        targetRole={targetRole}
        selectedCourses={eligibleCourses.filter(c => selectedSubjectIds.includes(c.subjectId))}
        uiProjects={uiProjects}
        selectedInternships={approvedInternships.filter(i =>
          selectedInternshipIds.includes(i.internshipId),
        )}
        certifications={selectedCertifications}
        awardsAndActivities={selectedAwards}
      />
    )
  }

  return (
    <div className={`a4-preview-container${isFullscreen ? ' fullscreen' : ''}`}>
      {showToolbar && (
        <div className="a4-toolbar">
          <Tooltip title="Thu nhỏ">
            <Button size="small" icon={<MinusOutlined />} onClick={handleZoomOut} disabled={effectiveScale <= 0.31} />
          </Tooltip>
          <span className="a4-zoom-percent">{zoomPercent}%</span>
          <Tooltip title="Phóng to">
            <Button size="small" icon={<PlusOutlined />} onClick={handleZoomIn} disabled={effectiveScale >= 2} />
          </Tooltip>
          <Tooltip title="Hiển thị toàn bộ trang A4">
            <Button size="small" icon={<CompressOutlined />} onClick={handleFitToScreen} disabled={zoomLevel === null} />
          </Tooltip>
          <div className="a4-toolbar-spacer" />
          <Tooltip title={isFullscreen ? 'Thoát toàn màn hình (Esc)' : 'Xem toàn màn hình'}>
            <Button
              size="small"
              icon={isFullscreen ? <FullscreenExitOutlined /> : <FullscreenOutlined />}
              onClick={() => setIsFullscreen(value => !value)}
            />
          </Tooltip>
          <Tooltip title="In CV / Xuất PDF">
            <Button size="small" type="primary" icon={<PrinterOutlined />} onClick={printA4Resume} />
          </Tooltip>
        </div>
      )}

      {isOverflowing && (
        <div className="a4-overflow-note">
          <Tag icon={<WarningOutlined />} color="warning">
            Nội dung vượt quá 1 trang A4 — sẽ tự thu nhỏ khi in
          </Tag>
        </div>
      )}

      <Spin spinning={isOptimizingAi} tip="AI đang tối ưu CV…" wrapperClassName="a4-spin">
        <div ref={viewportRef} className="a4-viewport">
          <div
            className="a4-paper-stage"
            style={{
              width: A4_WIDTH_PX * effectiveScale,
              height: A4_HEIGHT_PX * effectiveScale,
            }}
          >
            <div
              id={PRINTABLE_RESUME_ID}
              ref={paperRef}
              className="a4-paper"
              style={{ transform: `scale(${effectiveScale})` }}
            >
              {renderPaperContent()}
            </div>
          </div>
        </div>
      </Spin>

      {optimizedCvResult && (
        <p className="a4-edit-hint">
          Nhấp trực tiếp vào nội dung CV để chỉnh sửa · thay đổi được lưu khi rời ô
        </p>
      )}
    </div>
  )
}
function formatDate(dateStr?: string | null): string {
  if (!dateStr) return 'Hiện tại'
  return new Date(dateStr).toLocaleDateString('vi-VN', { month: '2-digit', year: 'numeric' })
}

type DraftCvContentProps = {
  studentInfo: { fullName: string; studentCode: string; majorName: string; gpa: number; academicYear: string }
  contactInfo: ResumeContactInfo
  targetRole: string
  selectedCourses: EligibleCourse[]
  uiProjects: UiProjectOverride[]
  selectedInternships: ApprovedInternship[]
  certifications: CertificationItem[]
  awardsAndActivities: AwardActivityItem[]
}

function normalizeWebUrl(value: string): string | null {
  const trimmed = value.trim()
  if (!trimmed) return null
  const candidate = /^https?:\/\//i.test(trimmed) ? trimmed : `https://${trimmed}`
  try {
    const url = new URL(candidate)
    return url.protocol === 'http:' || url.protocol === 'https:' ? url.toString() : null
  } catch {
    return null
  }
}

function ContactDetails({ contactInfo }: { contactInfo: ResumeContactInfo }) {
  const githubUrl = normalizeWebUrl(contactInfo.github)
  const linkedinUrl = normalizeWebUrl(contactInfo.linkedin)
  const items = [
    contactInfo.email && (
      <a key="email" href={`mailto:${contactInfo.email.trim()}`}>{contactInfo.email.trim()}</a>
    ),
    contactInfo.phone && (
      <a key="phone" href={`tel:${contactInfo.phone.replace(/[^\d+]/g, '')}`}>{contactInfo.phone.trim()}</a>
    ),
    contactInfo.address && <span key="address">{contactInfo.address.trim()}</span>,
    githubUrl && <a key="github" href={githubUrl}>GitHub</a>,
    linkedinUrl && <a key="linkedin" href={linkedinUrl}>LinkedIn</a>,
  ].filter(Boolean)

  if (items.length === 0) return null
  return (
    <div className="cv-contact-line">
      {items.map((item, index) => (
        <span className="cv-contact-item" key={index}>{item}</span>
      ))}
    </div>
  )
}

/** Bản nháp CV dựng trực tiếp từ dữ liệu form (chưa qua AI tối ưu). */
function DraftCvContent({
  studentInfo,
  contactInfo,
  targetRole,
  selectedCourses,
  uiProjects,
  selectedInternships,
  certifications,
  awardsAndActivities,
}: DraftCvContentProps) {
  return (
    <div className="cv-draft">
      <div className="cv-header">
        <h1>{studentInfo.fullName || 'Họ tên sinh viên'}</h1>
        {targetRole && <p className="cv-target-role">{targetRole}</p>}
        <ContactDetails contactInfo={contactInfo} />
      </div>

      <div className="cv-section">
        <h2>Tóm tắt chuyên môn</h2>
        <p className="cv-summary">
          Sinh viên {studentInfo.majorName || 'đang hoàn thiện hồ sơ'}
          {targetRole ? `, định hướng ${targetRole}` : ''}, mong muốn vận dụng kiến thức và kinh nghiệm từ học phần, dự án và thực tập trong môi trường chuyên nghiệp.
        </p>
      </div>

      {selectedCourses.length > 0 && (
        <div className="cv-section">
          <h2>Năng lực chuyên môn</h2>
          <p className="cv-skill-line">
            <strong>Kiến thức:</strong>
            <span>{selectedCourses.map(course => course.subjectName).join(' · ')}</span>
          </p>
        </div>
      )}

      {uiProjects.length > 0 && (
        <div className="cv-section">
          <h2>Dự án nổi bật</h2>
          {uiProjects.map(project => (
            <div key={project.projectId} className="cv-item">
              <div className="cv-item-header">
                <strong>{project.projectName}</strong>
                {project.myRole && <span className="cv-item-role">{project.myRole}</span>}
              </div>
              {project.techStack && <p className="cv-item-tech">{project.techStack}</p>}
              {project.myContributions && <p>{project.myContributions}</p>}
            </div>
          ))}
        </div>
      )}

      {selectedInternships.length > 0 && (
        <div className="cv-section">
          <h2>Kinh nghiệm làm việc</h2>
          {selectedInternships.map(internship => (
            <div key={internship.internshipId} className="cv-item">
              <div className="cv-item-header">
                <strong>{internship.position}</strong>
                <span className="cv-item-duration">
                  {formatDate(internship.startDate)} – {formatDate(internship.endDate)}
                </span>
              </div>
              <p className="cv-item-company">{internship.companyName}</p>
              {internship.taskDescription && <p>{internship.taskDescription}</p>}
            </div>
          ))}
        </div>
      )}

      <div className="cv-section">
        <h2>Học vấn & Học phần tiêu biểu</h2>
        <div className="cv-item cv-education-item">
          <div className="cv-item-header">
            <strong>Đại học Tây Đô</strong>
            {studentInfo.academicYear && <span className="cv-item-duration">{studentInfo.academicYear}</span>}
          </div>
          <p className="cv-education-meta">
            {studentInfo.majorName || 'Ngành học'}
            {studentInfo.gpa > 0 && ` · GPA ${studentInfo.gpa.toFixed(2)}`}
            {studentInfo.studentCode && ` · MSSV ${studentInfo.studentCode}`}
          </p>
          {selectedCourses.length > 0 && (
            <p className="cv-highlighted-courses">
              <strong>Học phần:</strong> {selectedCourses.map(course => `${course.subjectName} (${course.score.toFixed(1)})`).join(' · ')}
            </p>
          )}
        </div>
      </div>

      {certifications.length > 0 && (
        <div className="cv-section">
          <h2>Chứng chỉ</h2>
          <ul className="cv-bullets">
            {certifications.map(certification => (
              <li key={certification.id}>{certification.name} - {certification.issuer}</li>
            ))}
          </ul>
        </div>
      )}

      {awardsAndActivities.length > 0 && (
        <div className="cv-section">
          <h2>Giải thưởng & Hoạt động</h2>
          <ul className="cv-bullets">
            {awardsAndActivities.map(award => (
              <li key={award.id}>{award.title}{award.organization ? ` - ${award.organization}` : ''}</li>
            ))}
          </ul>
        </div>
      )}
    </div>
  )
}
type OptimizedCvContentProps = {
  cv: OptimizedResumeResponseDto
  contactInfo: ResumeContactInfo
  selectedCourses: EligibleCourse[]
  onSummaryCommit: (value: string) => void
  onSkillCommit: (group: OptimizedSkillGroup, index: number, value: string) => void
  onBulletCommit: (
    section: OptimizedBulletSection,
    index: number,
    bulletIndex: number,
    value: string,
  ) => void
}

/** CV sau khi AI tối ưu — cho phép chỉnh sửa trực tiếp summary, mô tả kỹ năng và bullet points. */
function OptimizedCvContent({
  cv,
  contactInfo,
  selectedCourses,
  onSummaryCommit,
  onSkillCommit,
  onBulletCommit,
}: OptimizedCvContentProps) {
  const renderSkillGroup = (
    title: string,
    group: OptimizedSkillGroup,
    skills: OptimizedResumeResponseDto['skills'][OptimizedSkillGroup],
  ) => {
    if (skills.length === 0) return null
    return (
      <p className="cv-skill-line">
        <strong>{title}:</strong>
        <span>
          {skills.map((skill, index) => (
            <span className="cv-skill-entry" key={index}>
              {skill.skillName}
              {skill.description && ' — '}
              <EditableText
                value={skill.description}
                onCommit={value => onSkillCommit(group, index, value)}
              />
            </span>
          ))}
        </span>
      </p>
    )
  }

  return (
    <div className="cv-optimized">
      <div className="cv-header-optimized">
        <h1>{cv.header.fullName}</h1>
        {cv.header.targetRole && <p className="cv-target-role">{cv.header.targetRole}</p>}
        <ContactDetails contactInfo={contactInfo} />
      </div>

      <div className="cv-section">
        <h2>Tóm tắt chuyên môn</h2>
        <EditableText as="p" className="cv-summary" value={cv.professionalSummary} onCommit={onSummaryCommit} />
      </div>

      <div className="cv-section cv-skills-section">
        <h2>Năng lực chuyên môn</h2>
        <div className="cv-skills-list">
          {renderSkillGroup('Kiến thức', 'knowledgeDomain', cv.skills.knowledgeDomain)}
          {renderSkillGroup('Kỹ năng nghề nghiệp', 'functionalSkills', cv.skills.functionalSkills)}
          {renderSkillGroup('Kỹ năng cá nhân', 'interpersonalSkills', cv.skills.interpersonalSkills)}
        </div>
      </div>

      {cv.projects.length > 0 && (
        <div className="cv-section">
          <h2>Dự án nổi bật</h2>
          {cv.projects.map((project, index) => (
            <div key={index} className="cv-item">
              <div className="cv-item-header">
                <strong>{project.projectName}</strong>
                <span className="cv-item-role">{project.myRole}</span>
              </div>
              <p className="cv-item-tech">{project.techStack}</p>
              <ul className="cv-bullets">
                {project.actionBulletPoints.map((bullet, bulletIndex) => (
                  <EditableText
                    as="li"
                    key={bulletIndex}
                    value={bullet}
                    onCommit={value => onBulletCommit('projects', index, bulletIndex, value)}
                  />
                ))}
              </ul>
            </div>
          ))}
        </div>
      )}

      {cv.internships.length > 0 && (
        <div className="cv-section">
          <h2>Kinh nghiệm làm việc</h2>
          {cv.internships.map((internship, index) => (
            <div key={index} className="cv-item">
              <div className="cv-item-header">
                <strong>{internship.position}</strong>
                <span className="cv-item-duration">{internship.durationText}</span>
              </div>
              <p className="cv-item-company">{internship.companyName}</p>
              <ul className="cv-bullets">
                {internship.actionBulletPoints.map((bullet, bulletIndex) => (
                  <EditableText
                    as="li"
                    key={bulletIndex}
                    value={bullet}
                    onCommit={value => onBulletCommit('internships', index, bulletIndex, value)}
                  />
                ))}
              </ul>
            </div>
          ))}
        </div>
      )}

      <div className="cv-section">
        <h2>Học vấn & Học phần tiêu biểu</h2>
        <div className="cv-item cv-education-item">
          <div className="cv-item-header">
            <strong>{cv.education.institutionName || 'Đại học Tây Đô'}</strong>
            <span className="cv-item-duration">{cv.education.durationText}</span>
          </div>
          <p className="cv-education-meta">
            {cv.education.majorName || cv.header.majorName}
            {cv.education.degreeName && ` · ${cv.education.degreeName}`}
            {cv.education.gpa != null && ` · GPA ${cv.education.gpa.toFixed(2)}`}
            {cv.header.studentCode && ` · MSSV ${cv.header.studentCode}`}
          </p>
          {selectedCourses.length > 0 && (
            <p className="cv-highlighted-courses">
              <strong>Học phần:</strong> {selectedCourses.map(course => `${course.subjectName} (${course.score.toFixed(1)})`).join(' · ')}
            </p>
          )}
        </div>
      </div>

      {cv.certifications.length > 0 && (
        <div className="cv-section">
          <h2>Chứng chỉ</h2>
          <ul className="cv-bullets">
            {cv.certifications.map((certification, index) => (
              <li key={index}>{certification}</li>
            ))}
          </ul>
        </div>
      )}

      {cv.awardsAndActivities.length > 0 && (
        <div className="cv-section">
          <h2>Giải thưởng & Hoạt động</h2>
          <ul className="cv-bullets">
            {cv.awardsAndActivities.map((award, index) => (
              <li key={index}>{award}</li>
            ))}
          </ul>
        </div>
      )}
    </div>
  )
}
