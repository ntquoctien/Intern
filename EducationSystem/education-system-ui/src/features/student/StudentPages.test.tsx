import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { StudentDashboardPage, StudentFormRequestsPage, StudentSchedulePage } from './StudentPages'
import { studentApi } from './studentApi'

function renderPage(page: React.ReactNode) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(<QueryClientProvider client={client}><MemoryRouter>{page}</MemoryRouter></QueryClientProvider>)
}

describe('student page accessibility and responsive views', () => {
  afterEach(() => vi.restoreAllMocks())

  it('renders both weekly desktop and day-oriented mobile schedule controls', async () => {
    vi.spyOn(studentApi, 'schedule').mockResolvedValue([{
      scheduleId: 'schedule-1', subjectTeachingId: 'class-1', subjectCode: 'SE101',
      subjectName: 'Kiến trúc phần mềm', className: 'SE101.01',
      startDateTime: '2026-07-20T08:00:00', endDateTime: '2026-07-20T10:00:00',
      roomName: 'A101', teacherName: null, note: '',
    }])
    const { container } = renderPage(<StudentSchedulePage />)
    expect(await screen.findAllByText('SE101')).not.toHaveLength(0)
    expect(screen.getAllByText(/Chưa có giảng viên/)).not.toHaveLength(0)
    expect(container.querySelector('.schedule-desktop')).toBeInTheDocument()
    expect(container.querySelector('.schedule-mobile')).toBeInTheDocument()
    expect(screen.getByRole('radio', { name: 'T2' })).toBeInTheDocument()
  })

  it('opens form-request detail from keyboard and exposes no mutation action', async () => {
    vi.spyOn(studentApi, 'formTemplates').mockResolvedValue([])
    vi.spyOn(studentApi, 'formRequests').mockResolvedValue([{
      formRequestId: 'request-1', formTemplateName: 'Xác nhận sinh viên',
      creationDate: '2026-07-20T08:00:00', updateDate: '2026-07-20T09:00:00',
      rawStatus: 1, approvalName: '', note: 'Bản ghi hiện có',
    }])
    renderPage(<StudentFormRequestsPage />)
    const cells = await screen.findAllByText('Xác nhận sinh viên')
    const row = cells.map(cell => cell.closest('button')).find(Boolean)!
    row.focus()
    fireEvent.keyDown(row, { key: 'Enter' })
    await waitFor(() => expect(screen.getByText('Chi tiết yêu cầu')).toBeInTheDocument())
    expect(screen.queryByRole('button', { name: /gửi|hủy|tải lên|sửa|xóa/i })).not.toBeInTheDocument()
  })

  it('shows evidence-based dashboard cards and the unsupported-metric notice', async () => {
    vi.spyOn(studentApi, 'profile').mockResolvedValue({
      studentId: '1', userId: '2', studentCode: 'SV000001', fullName: 'Sinh viên A',
      userName: 'student', expiresAt: '', majorCode: 'KTPM', majorName: 'Kỹ thuật phần mềm',
      facultyName: 'CNTT', academicYearName: '2026', academicYear: 2026,
      studyStatus: null, isGraduated: false,
    })
    vi.spyOn(studentApi, 'subjects').mockResolvedValue([])
    vi.spyOn(studentApi, 'schedule').mockResolvedValue([])
    vi.spyOn(studentApi, 'examResults').mockResolvedValue([])
    renderPage(<StudentDashboardPage />)
    expect(await screen.findByText('Lớp đã ghi danh')).toBeInTheDocument()
    expect(screen.getByText(/Không hiển thị GPA/)).toBeInTheDocument()
    expect(screen.queryByText(/đậu|rớt/i)).toBeInTheDocument()
  })
})
