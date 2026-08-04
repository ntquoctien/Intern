import { useMutation } from '@tanstack/react-query'
import { httpClient } from '../../../../../shared/api/httpClient'
import type { ApiResponse } from '../../../../../shared/types/api'
import type { OptimizedResumeResponseDto, ResumeBuilderState } from '../types'
import { formatAwardPayload, formatCertificationPayload } from '../utils/resumeSelectionFormatters'

const careerApiOrigin =
  import.meta.env.VITE_AI_API_ORIGIN ?? import.meta.env.VITE_CAREER_API_ORIGIN ?? 'http://localhost:5005'

const uuidPattern = /^[0-9a-f]{8}(?:-[0-9a-f]{4}){3}-[0-9a-f]{12}$/i
const MAX_TARGET_ROLE_LENGTH = 200
const MAX_CAREER_FOCUS_TAG_LENGTH = 200
const MAX_UI_PROJECTS = 20
const MAX_CERTIFICATIONS = 20
const MAX_AWARDS = 20
const MAX_SUBJECT_IDS = 100
const MAX_INTERNSHIP_IDS = 100
const MAX_INT32 = 2_147_483_647
const OPTIMIZE_REQUEST_TIMEOUT_MS = 310_000

function trimToMax(value: string, maximumLength: number) {
  return value.trim().slice(0, maximumLength)
}

function normalizeProjectId(projectId: number) {
  return Number.isInteger(projectId) && projectId > 0 && projectId <= MAX_INT32 ? projectId : undefined
}

/**
 * Gọi POST /api/career/resume/optimize (CareerService BFF -> LLM + fact guard).
 * Payload khớp contract trong docs/career-resume-generator.md.
 */
async function postOptimizeResume(state: ResumeBuilderState, studentId: string) {
  const selectedUiProjects = state.uiProjects
    .filter(project => project.isSelectedForCv === true && project.projectName.trim())
    .slice(0, MAX_UI_PROJECTS)

  const response = await httpClient.post<ApiResponse<OptimizedResumeResponseDto>>(
    `${careerApiOrigin}/api/career/resume/optimize`,
    {
      studentId,
      targetRole: trimToMax(state.targetRole, MAX_TARGET_ROLE_LENGTH),
      jobDescription: state.jobDescription.trim(),
      careerFocusTag: trimToMax(state.careerFocusTag, MAX_CAREER_FOCUS_TAG_LENGTH) || undefined,
      selectedSubjectIds: state.selectedSubjectIds.filter(id => uuidPattern.test(id)).slice(0, MAX_SUBJECT_IDS),
      selectedInternshipIds: state.selectedInternshipIds.filter(
        id => Number.isInteger(id) && id > 0,
      ).slice(0, MAX_INTERNSHIP_IDS),
      uiProjects: selectedUiProjects
        .map(project => ({
          projectId: normalizeProjectId(project.projectId),
          projectName: trimToMax(project.projectName, 255),
          techStack: trimToMax(project.techStack, 500),
          myRole: trimToMax(project.myRole, 200),
          myContributions: trimToMax(project.myContributions, 4_000),
          teamSize: project.teamSize,
        })),
      certifications: state.certifications
        .filter(item => item.isSelectedForCv)
        .slice(0, MAX_CERTIFICATIONS)
        .map(formatCertificationPayload),
      awardsAndActivities: state.awardsAndActivities
        .filter(item => item.isSelectedForCv)
        .slice(0, MAX_AWARDS)
        .map(formatAwardPayload),
      currentSummaryDraft: state.optimizedCvResult?.professionalSummary ?? null,
      topK: 8,
      similarityThreshold: 0.65,
    },
    {
      timeout: OPTIMIZE_REQUEST_TIMEOUT_MS,
    },
  )
  return response.data.data
}

export function useOptimizeResume() {
  return useMutation({
    mutationFn: ({ state, studentId }: { state: ResumeBuilderState; studentId: string }) =>
      postOptimizeResume(state, studentId),
  })
}
