import { ProjectOutlined, ReloadOutlined, RobotOutlined } from '@ant-design/icons'
import { Button, Card, Checkbox, Empty, Space, Typography, Tabs, message } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { httpClient } from '../../../../../shared/api/httpClient'
import type { ApiResponse } from '../../../../../shared/types/api'
import { useStudentAuth } from '../../../studentAuth'
import { useResumeStore } from '../hooks/useResumeStore'
import type { ApprovedInternship } from '../types'
import { ProjectFormModal } from './ProjectFormModal'
import { ProjectPersonalForm } from './ProjectPersonalForm'
import { ProjectCard } from './ProjectCard'
import { CertAndAwardManager } from './CertAndAwardManager'
import { formatAwardPayload, formatCertificationPayload } from '../utils/resumeSelectionFormatters'

const { Title, Text } = Typography
const academicApiOrigin = import.meta.env.VITE_ACADEMIC_API_ORIGIN ?? 'http://localhost:5002'

export function Step3EvidenceOverride() {
  const { state, updateUiProject, toggleInternshipSelection, setStep, addPersonalProject, deletePersonalProject, preparePayload, setApprovedInternships } = useResumeStore()
  const { session } = useStudentAuth()
  const { uiProjects, approvedInternships, selectedInternshipIds, eligibleCourses, targetRole, jobDescription, careerFocusTag, selectedSubjectIds, studentInfo, certifications, awardsAndActivities } = state
  const [personalFormOpen, setPersonalFormOpen] = useState(false)
  const [projectFormOpen, setProjectFormOpen] = useState(false)
  const [editingProject, setEditingProject] = useState<number | null>(null)
  const [creatingSchoolProject, setCreatingSchoolProject] = useState(false)
  const [isRefreshingInternships, setIsRefreshingInternships] = useState(false)

  const refreshInternships = useCallback(async (showError = true) => {
    if (!session?.studentId) return
    setIsRefreshingInternships(true)
    try {
      const response = await httpClient.get<ApiResponse<ApprovedInternship[]>>(
        `${academicApiOrigin}/api/academic/students/${session.studentId}/internships`,
      )
      setApprovedInternships(response.data.data ?? [])
    } catch {
      if (showError) {
        message.error('Không thể tải lại danh sách thực tập đã được duyệt.')
      }
    } finally {
      setIsRefreshingInternships(false)
    }
  }, [session?.studentId, setApprovedInternships])

  useEffect(() => {
    void refreshInternships(false)
    const handleFocus = () => void refreshInternships(false)
    window.addEventListener('focus', handleFocus)
    return () => window.removeEventListener('focus', handleFocus)
  }, [refreshInternships])

  const personalProjects = uiProjects.filter(p => p.source === 'personal')
  const portfolioProjects = uiProjects.filter(p => p.source !== 'personal')
  const selectedCount = uiProjects.filter(p => p.isSelectedForCv).length
  const selectedCertifications = certifications.filter(item => item.isSelectedForCv)
  const selectedAwards = awardsAndActivities.filter(item => item.isSelectedForCv)

  const formatDate = (dateStr?: string | null) => {
    if (!dateStr) return 'Hiện tại'
    return new Date(dateStr).toLocaleDateString('vi-VN')
  }

  const handleEditProject = (projectId: number) => {
    setEditingProject(projectId)
    setProjectFormOpen(true)
  }

  const handleSaveProject = (project: any) => {
    const index = uiProjects.findIndex(p => p.projectId === project.projectId)
    if (index >= 0) {
      updateUiProject(index, project)
    }
  }

  const handleMapCourse = (projectId: number, courseId: string, courseCode: string, courseName: string) => {
    const index = uiProjects.findIndex(p => p.projectId === projectId)
    if (index >= 0) {
      const updated = {
        ...uiProjects[index],
        mappedCourseId: courseId,
        mappedCourseCode: courseCode,
        mappedCourseName: courseName,
      }
      updateUiProject(index, updated)
    }
  }

  const handleContinue = () => {
    const selectedProjects = uiProjects.filter(p => p.isSelectedForCv)
    const payload = {
      studentId: studentInfo.studentCode,
      targetRole,
      jobDescription,
      careerFocusTag: careerFocusTag.trim() || null,
      selectedSubjectIds,
      selectedInternshipIds,
      uiProjects: selectedProjects,
      certifications: selectedCertifications.map(formatCertificationPayload),
      awardsAndActivities: selectedAwards.map(formatAwardPayload),
    }
    preparePayload(payload)
    setStep(3)
  }

  const tabItems = [
    {
      key: 'personal',
      label: `Dự án Cá nhân (${personalProjects.length})`,
      children: renderPersonalTab(),
    },
    {
      key: 'group',
      label: `Dự án Trường (${portfolioProjects.length})`,
      children: renderGroupTab(),
    },
    {
      key: 'internship',
      label: `Thực tập (${approvedInternships.length})`,
      children: renderInternshipTab(),
    },
    {
      key: 'cert-award',
      label: `Chứng chỉ & Thành tích (${selectedCertifications.length + selectedAwards.length})`,
      children: <CertAndAwardManager />,
    },
  ]

  return (
    <div className="step-evidence-override">
      <Card className="step-card">
        <Space direction="vertical" size="large" style={{ width: '100%' }}>
          <div className="step-header">
            <ProjectOutlined className="step-icon" />
            <div>
              <Title level={4} style={{ margin: 0 }}>Dự án, Thực tập & Bằng chứng CV</Title>
              <Text type="secondary">Chọn và chỉnh sửa dự án, thực tập, chứng chỉ và thành tích để đưa vào CV</Text>
            </div>
          </div>

          <div style={{ backgroundColor: '#f0f5ff', padding: '12px 16px', borderRadius: '6px' }}>
            <Text>
              📌 <strong>Đã chọn: {selectedCount}/2 dự án</strong> (Khuyên dùng tối đa 2 dự án sát nhất với JD)
            </Text>
          </div>

          <Tabs items={tabItems} />

          <div className="step-actions">
            <Button onClick={() => setStep(1)}>← Quay lại</Button>
            <Button type="primary" size="large" icon={<RobotOutlined />} onClick={handleContinue}>
              Tiếp tục ➔ Xem trước & Tối ưu AI
            </Button>
          </div>
        </Space>
      </Card>

      <ProjectPersonalForm
        open={personalFormOpen}
        onClose={() => setPersonalFormOpen(false)}
        onConfirm={p => addPersonalProject(p)}
        eligibleCourses={eligibleCourses}
      />

      <ProjectFormModal
        open={projectFormOpen || creatingSchoolProject}
        onClose={() => {
          setProjectFormOpen(false)
          setCreatingSchoolProject(false)
          setEditingProject(null)
        }}
        onConfirm={handleSaveProject}
        initialProject={editingProject ? uiProjects.find(p => p.projectId === editingProject) : null}
        eligibleCourses={eligibleCourses}
      />
    </div>
  )

  function renderPersonalTab() {
    return (
      <Space direction="vertical" size="large" style={{ width: '100%' }}>
        <Button type="dashed" block onClick={() => setPersonalFormOpen(true)}>
          + Thêm Dự án Cá nhân
        </Button>
        {personalProjects.length === 0 ? (
          <Empty description="Chưa có dự án cá nhân" image={Empty.PRESENTED_IMAGE_SIMPLE} />
        ) : (
          personalProjects.map((p) => (
            <ProjectCard
              key={p.projectId}
              project={p}
              isSelected={p.isSelectedForCv || false}
              onSelect={(projectId, selected) => {
                const index = uiProjects.findIndex(proj => proj.projectId === projectId)
                if (index >= 0) {
                  updateUiProject(index, { ...uiProjects[index], isSelectedForCv: selected })
                }
              }}
              onEdit={() => {}} // Personal projects don't have edit
              onDelete={deletePersonalProject}
              eligibleCourses={eligibleCourses}
              showEditButton={false}
              showDeleteButton={true}
            />
          ))
        )}
      </Space>
    )
  }

  function renderGroupTab() {
    return (
      <Space direction="vertical" size="large" style={{ width: '100%' }}>
        <Button type="dashed" block onClick={() => setCreatingSchoolProject(true)}>
          + Thêm Dự án Trường
        </Button>
        {portfolioProjects.length === 0 ? (
          <Empty description="Chưa có dự án trường" image={Empty.PRESENTED_IMAGE_SIMPLE} />
        ) : (
          portfolioProjects.map((p) => (
            <ProjectCard
              key={p.projectId}
              project={p}
              isSelected={p.isSelectedForCv || false}
              onSelect={(projectId, selected) => {
                const index = uiProjects.findIndex(proj => proj.projectId === projectId)
                if (index >= 0) {
                  updateUiProject(index, { ...uiProjects[index], isSelectedForCv: selected })
                }
              }}
              onEdit={handleEditProject}
              eligibleCourses={eligibleCourses}
              onMapCourse={handleMapCourse}
              showEditButton={true}
              showDeleteButton={false}
            />
          ))
        )}
      </Space>
    )
  }

  function renderInternshipTab() {
    return (
      <Space direction="vertical" size="large" style={{ width: '100%' }}>
        <Button
          icon={<ReloadOutlined />}
          loading={isRefreshingInternships}
          onClick={() => void refreshInternships()}
        >
          Làm mới danh sách thực tập
        </Button>
        {approvedInternships.length === 0 ? (
          <Empty description="Chưa có thực tập được duyệt" image={Empty.PRESENTED_IMAGE_SIMPLE} />
        ) : (
          <>
            {approvedInternships.map(internship => {
              const isSelected = selectedInternshipIds.includes(internship.internshipId)
              return (
                <Card key={internship.internshipId} size="small">
                  <Space direction="vertical" style={{ width: '100%' }} size="small">
                    <div style={{ display: 'flex', gap: '12px', alignItems: 'flex-start' }}>
                      <Checkbox
                        checked={isSelected}
                        onChange={() => toggleInternshipSelection(internship.internshipId)}
                        style={{ marginTop: '4px' }}
                      />
                      <div style={{ flex: 1 }}>
                        <Text strong>{internship.companyName}</Text>
                        <br />
                        <Text>{internship.position}</Text>
                        <br />
                        <Text type="secondary">
                          {formatDate(internship.startDate)} - {formatDate(internship.endDate)}
                        </Text>
                        {internship.taskDescription && (
                          <>
                            <br />
                            <Text type="secondary">{internship.taskDescription}</Text>
                          </>
                        )}
                      </div>
                    </div>
                  </Space>
                </Card>
              )
            })}
            <Text type="secondary">
              Đã chọn {selectedInternshipIds.length} kỳ thực tập
            </Text>
          </>
        )}
      </Space>
    )
  }
}
