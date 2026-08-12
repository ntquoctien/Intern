import axios from 'axios'
import { httpClient } from '../../../../shared/api/httpClient'
import type { ApiResponse } from '../../../../shared/types/api'

const careerOrigin = import.meta.env.VITE_CAREER_API_ORIGIN ?? 'http://localhost:5005'
const academicOrigin = import.meta.env.VITE_ACADEMIC_API_ORIGIN ?? 'http://localhost:5002'
const baseUrl = `${careerOrigin}/api/management`

export type ImportStatus =
  | 'Uploaded' | 'Processing' | 'PendingReview' | 'ValidationFailed'
  | 'Approved' | 'Rejected' | 'Failed' | 'Archived'

export type CurriculumOption = {
  id: number
  majorExternalId?: string | null
  majorCode: string
  majorName: string
  curriculumCode: string
  curriculumName?: string | null
  version: string
  effectiveFrom?: string | null
  effectiveTo?: string | null
  status: string
}
export type CreateCurriculum = {
  majorExternalId?: string | null
  majorCode: string
  majorName: string
  curriculumCode: string
  curriculumName?: string
  version: string
}

export type MajorLookup = {
  id: string
  code: string
  name: string
}

export type SelectedSubject = {
  subjectId: string
  subjectCode: string
  name: string
}

export type OutcomeImportListItem = {
  id: number
  curriculumVersionId: number
  selectedSubjectExternalId?: string | null
  selectedSubjectCode?: string | null
  selectedSubjectName?: string | null
  majorCode: string
  majorName: string
  curriculumCode: string
  version: string
  originalFileName: string
  fileSize: number
  status: ImportStatus
  processingStage?: string | null
  progressPercent: number
  ploCount: number
  cloCount: number
  mappingCount: number
  uploadedByName: string
  createdAt: string
  updatedAt: string
  rowVersion: string
}

export type PagedResponse<T> = {
  items: T[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
}

export type ImportDetail = {
  id: number
  curriculumVersionId: number
  selectedSubjectExternalId?: string | null
  selectedSubjectCode?: string | null
  selectedSubjectName?: string | null
  originalFileName: string
  fileSize: number
  status: ImportStatus
  processingStage?: string | null
  progressPercent: number
  errorCode?: string | null
  errorMessage?: string | null
  isRetryable: boolean
  createdAt: string
  updatedAt: string
  rowVersion: string
}

export type SourceReference = {
  blockId: string
  elementType: string
  sequence: number
  headingPath: string[]
  paragraphIndex?: number | null
  tableIndex?: number | null
  rowIndex?: number | null
  columnIndex?: number | null
  excerpt: string
}

export type Warning = {
  code: string
  severity: 'BlockingError' | 'Warning' | string
  entityDraftId?: string | null
  fieldName?: string | null
  message: string
}

export type DraftBase = {
  draftId: string
  sourceReferences: SourceReference[]
  confidence: number
  warnings: Warning[]
  removed: boolean
}
export type PloDraft = DraftBase & {
  ploCode: string
  description: string
  originalDescription?: string | null
  sortOrder: number
}
export type CloDraft = DraftBase & {
  cloCode: string
  description: string
  originalDescription?: string | null
  sortOrder: number
}
export type SubjectDraft = DraftBase & {
  subject: {
    externalId?: string | null
    code: string
    name: string
    credits?: number | null
    matchStatus: string
  }
  clos: CloDraft[]
}
export type MappingDraft = DraftBase & {
  cloDraftId: string
  ploDraftId: string
  progressionLevel: 'E' | 'R' | 'D' | string
}
export type DraftDocument = {
  schemaVersion: string
  importBatchId: number
  curriculum: DraftBase & {
    majorCode?: string | null
    majorName?: string | null
    curriculumCode?: string | null
    curriculumName?: string | null
    version?: string | null
  }
  plos: PloDraft[]
  subjects: SubjectDraft[]
  mappings: MappingDraft[]
  warnings: Warning[]
}
export type DocumentBlock = {
  blockId: string
  blockType: string
  sequence: number
  headingLevel?: number | null
  contentJson: string
}
export type ImportReview = {
  import: ImportDetail
  document?: DraftDocument | null
  validation: Warning[]
  qualityReport?: ExtractionQualityReport | null
  blocks: DocumentBlock[]
  allowedActions: string[]
}
export type ExtractionQualityReport = {
  scopeCompliance: number
  sourceReferenceCoverage: number
  cloCountSource: number
  cloCountReviewed: number
  exactCloCodeMatchRate: number
  exactPloCodeMatchRate: number
  textOverlapAverage: number
  hallucinatedSubjectCount: number
  removedOutOfScopeSubjectCount: number
  danglingMappingCount: number
  duplicateMappingCount: number
  blockingErrors: string[]
  warnings: string[]
}
export type ApprovedOutcomeDocument = {
  id: number
  curriculumVersionId: number
  importBatchId: number
  schemaVersion: string
  documentVersion: number
  content: unknown
  contentHash: string
  status: 'Active' | 'Superseded'
  generatedAt: string
  generatedByExternalId: string
  generatedByName: string
  supersededAt?: string | null
  rowVersion: string
}
export type ReviewOperation = {
  entityType: string
  draftId: string
  fieldName?: string
  value?: unknown
  action?: 'Edit' | 'Remove' | 'Restore'
  note?: string
}

export type OutcomeProblem = {
  title?: string
  detail?: string
  errorCode?: string
  existingImportId?: number
  existingImportStatus?: ImportStatus
}

export type DuplicateImport = { id: number; status: ImportStatus }

export function duplicateImport(error: unknown): DuplicateImport | undefined {
  if (!axios.isAxiosError<OutcomeProblem>(error)) return undefined
  const problem = error.response?.data
  if (problem?.errorCode !== 'DUPLICATE_IMPORT' || !problem.existingImportId || !problem.existingImportStatus)
    return undefined
  return { id: problem.existingImportId, status: problem.existingImportStatus }
}

export function problemMessage(error: unknown) {
  if (axios.isAxiosError<OutcomeProblem>(error))
    return error.response?.data?.detail ?? error.message
  return error instanceof Error ? error.message : 'Đã xảy ra lỗi không xác định.'
}

export const outcomeApi = {
  majors: async () =>
    (await httpClient.get<ApiResponse<MajorLookup[]>>(`${academicOrigin}/api/academic/majors/lookup`)).data.data,
  subjects: async () => {
    const items = (await httpClient.get<ApiResponse<Array<{
      id: string
      subjectCode: string
      name: string
    }>>>(`${academicOrigin}/api/academic/subjects/lookup`)).data.data
    return items.map(item => ({
      subjectId: item.id,
      subjectCode: item.subjectCode,
      name: item.name,
    } satisfies SelectedSubject))
  },
  curricula: async () =>
    (await httpClient.get<CurriculumOption[]>(`${baseUrl}/curriculum-versions`)).data,
  createCurriculum: async (request: CreateCurriculum) =>
    (await httpClient.post<CurriculumOption>(`${baseUrl}/curriculum-versions`, request)).data,
  list: async (params: Record<string, string | number | undefined>) =>
    (await httpClient.get<PagedResponse<OutcomeImportListItem>>(`${baseUrl}/outcome-imports`, { params })).data,
  upload: async (curriculumVersionId: number, subject: SelectedSubject, file: File) => {
    const form = new FormData()
    form.append('curriculumVersionId', String(curriculumVersionId))
    form.append('subjectExternalId', subject.subjectId)
    form.append('subjectCode', subject.subjectCode)
    form.append('subjectName', subject.name)
    form.append('file', file)
    return (await httpClient.post<{ id: number; status: ImportStatus; fileName: string; rowVersion: string }>(
      `${baseUrl}/outcome-imports`, form,
    )).data
  },
  detail: async (id: number) =>
    (await httpClient.get<ImportDetail>(`${baseUrl}/outcome-imports/${id}`)).data,
  review: async (id: number) =>
    (await httpClient.get<ImportReview>(`${baseUrl}/outcome-imports/${id}/review`)).data,
  approvedDocument: async (id: number) =>
    (await httpClient.get<ApprovedOutcomeDocument>(`${baseUrl}/outcome-imports/${id}/approved-document`)).data,
  process: async (id: number) =>
    httpClient.post(`${baseUrl}/outcome-imports/${id}/process`),
  patch: async (id: number, rowVersion: string, operations: ReviewOperation[]) =>
    (await httpClient.patch<ImportReview>(`${baseUrl}/outcome-imports/${id}/review`, { rowVersion, operations })).data,
  validate: async (id: number, rowVersion: string) =>
    (await httpClient.post<ImportReview>(`${baseUrl}/outcome-imports/${id}/validate`, { rowVersion })).data,
  approve: async (id: number, rowVersion: string) =>
    (await httpClient.post<ImportDetail>(`${baseUrl}/outcome-imports/${id}/approve`, { rowVersion, confirmed: true })).data,
  reject: async (id: number, rowVersion: string, reason: string) =>
    (await httpClient.post<ImportDetail>(`${baseUrl}/outcome-imports/${id}/reject`, { rowVersion, reason })).data,
  archive: async (id: number, rowVersion: string) =>
    (await httpClient.post<ImportDetail>(`${baseUrl}/outcome-imports/${id}/archive`, { rowVersion })).data,
}
