import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { currentManagementNavigation, managementNavigation } from '../../../../app/managementNavigation'
import { managementApi } from '../../managementApi'
import { OutcomeImportListPage } from './OutcomeImportListPage'
import { OutcomeImportReviewPage } from './OutcomeImportReviewPage'
import { OutcomeImportUploadPage } from './OutcomeImportUploadPage'
import { duplicateImport, outcomeApi, type ImportReview } from './outcomeApi'

function renderPage(element: React.ReactNode, path: string, route = path) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } })
  return render(<QueryClientProvider client={client}>
    <MemoryRouter initialEntries={[path]}><Routes><Route path={route} element={element} /></Routes></MemoryRouter>
  </QueryClientProvider>)
}

describe('CLO/PLO management pages', () => {
  afterEach(() => vi.restoreAllMocks())

  it('reads the existing batch from a duplicate-import problem', () => {
    expect(duplicateImport({
      isAxiosError: true,
      response: {
        data: {
          errorCode: 'DUPLICATE_IMPORT',
          existingImportId: 42,
          existingImportStatus: 'PendingReview',
        },
      },
    })).toEqual({ id: 42, status: 'PendingReview' })
  })

  it('exposes a direct Administrator sidebar entry for nested outcome routes', () => {
    const entry = managementNavigation.find(item => item.id === 'outcomes')
    expect(entry).toMatchObject({
      label: 'CLO/PLO',
      path: '/management/system/outcomes',
      roles: ['Administrator'],
    })
    expect(currentManagementNavigation('/management/system/outcomes/12/review').id).toBe('outcomes')
  })

  it('renders the paged import list with a clear empty state table', async () => {
    vi.spyOn(outcomeApi, 'curricula').mockResolvedValue([])
    vi.spyOn(outcomeApi, 'list').mockResolvedValue({
      items: [], page: 1, pageSize: 20, totalItems: 0, totalPages: 0,
    })
    renderPage(<OutcomeImportListPage />, '/management/system/outcomes')
    expect(await screen.findByRole('heading', { name: 'Import CLO/PLO' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Import tài liệu/ })).toBeInTheDocument()
    expect((await screen.findAllByText('No data')).length).toBeGreaterThan(0)
  })

  it('guides upload through curriculum, document, processing and review steps', async () => {
    vi.spyOn(outcomeApi, 'curricula').mockResolvedValue([])
    vi.spyOn(outcomeApi, 'majors').mockResolvedValue([])
    vi.spyOn(managementApi, 'plans').mockResolvedValue([])
    vi.spyOn(outcomeApi, 'subjects').mockResolvedValue([])
    renderPage(<OutcomeImportUploadPage />, '/management/system/outcomes/import')
    expect(await screen.findByText('Chọn ngữ cảnh')).toBeInTheDocument()
    expect(screen.getByText('Chọn tài liệu')).toBeInTheDocument()
    expect(screen.getByText(/DOCX hoặc PDF/)).toBeInTheDocument()
    expect(document.querySelector<HTMLInputElement>('input[type="file"]')).toHaveAttribute('accept', '.docx,.pdf')
    expect(screen.getByText('LLM phân tích')).toBeInTheDocument()
    expect(screen.getByText('Kiểm duyệt')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Tải lên và phân tích/ })).toBeDisabled()
  })

  it('blocks approval when deterministic validation contains a blocking issue', async () => {
    const data: ImportReview = {
      import: {
        id: 12, curriculumVersionId: 7, originalFileName: 'outcomes.docx',
        fileSize: 1000, status: 'PendingReview', progressPercent: 100,
        processingStage: 'Completed', isRetryable: false,
        createdAt: '2026-07-29T00:00:00Z', updatedAt: '2026-07-29T00:00:00Z',
        rowVersion: 'AQ==',
      },
      document: {
        schemaVersion: '1.0', importBatchId: 12,
        curriculum: { draftId: 'curriculum', sourceReferences: [], confidence: 1, warnings: [], removed: false },
        plos: [], subjects: [], mappings: [], warnings: [],
      },
      validation: [{
        code: 'SUBJECT_NOT_FOUND', severity: 'BlockingError',
        entityDraftId: 'subject-1', fieldName: 'subject.code',
        message: 'Không tìm thấy môn học.',
      }],
      blocks: [],
      allowedActions: ['edit', 'validate', 'approve', 'reject'],
    }
    vi.spyOn(outcomeApi, 'review').mockResolvedValue(data)
    renderPage(<OutcomeImportReviewPage />, '/management/system/outcomes/12/review', '/management/system/outcomes/:id/review')
    expect(await screen.findByText('Chưa thể phê duyệt')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Phê duyệt/ })).toBeDisabled()
    expect(screen.getByText(/SUBJECT_NOT_FOUND/)).toBeInTheDocument()
  })

  it('shows extraction quality and the approved official JSON document', async () => {
    const data: ImportReview = {
      import: {
        id: 13, curriculumVersionId: 7, originalFileName: 'approved.docx',
        fileSize: 1000, status: 'Approved', progressPercent: 100,
        processingStage: 'Completed', isRetryable: false,
        createdAt: '2026-07-29T00:00:00Z', updatedAt: '2026-07-29T00:00:00Z',
        rowVersion: 'AQ==',
      },
      document: {
        schemaVersion: '1.0', importBatchId: 13,
        curriculum: { draftId: 'curriculum', sourceReferences: [], confidence: 1, warnings: [], removed: false },
        plos: [], subjects: [], mappings: [], warnings: [],
      },
      validation: [],
      qualityReport: {
        scopeCompliance: 1,
        sourceReferenceCoverage: 1,
        cloCountSource: 14,
        cloCountReviewed: 14,
        exactCloCodeMatchRate: 0,
        exactPloCodeMatchRate: 1,
        textOverlapAverage: .93,
        hallucinatedSubjectCount: 1,
        removedOutOfScopeSubjectCount: 1,
        danglingMappingCount: 0,
        duplicateMappingCount: 0,
        blockingErrors: [],
        warnings: [],
      },
      blocks: [],
      allowedActions: ['archive'],
    }
    vi.spyOn(outcomeApi, 'review').mockResolvedValue(data)
    vi.spyOn(outcomeApi, 'approvedDocument').mockResolvedValue({
      id: 1,
      curriculumVersionId: 7,
      importBatchId: 13,
      schemaVersion: '1.0',
      documentVersion: 1,
      content: { schemaVersion: '1.0', plos: [], subjects: [], mappings: [] },
      contentHash: 'a'.repeat(64),
      status: 'Active',
      generatedAt: '2026-07-29T00:00:00Z',
      generatedByExternalId: 'admin',
      generatedByName: 'Administrator',
      rowVersion: 'AQ==',
    })

    renderPage(<OutcomeImportReviewPage />, '/management/system/outcomes/13/review', '/management/system/outcomes/:id/review')

    expect(await screen.findByText('Chất lượng trích xuất')).toBeInTheDocument()
    expect(screen.getByText('14 / 14')).toBeInTheDocument()
    expect(await screen.findByText('ApprovedOutcomeJsonDocument')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /JSON chính thức/ })).toBeInTheDocument()
    expect(screen.getByText('Active')).toBeInTheDocument()
  })
})
