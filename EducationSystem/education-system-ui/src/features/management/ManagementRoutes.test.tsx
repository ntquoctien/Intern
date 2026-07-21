import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { LegacyRedirect } from '../../app/router'
import { ManagementQuestionSuitesPage, ManagementSchedulePage, ManagementStudentsPage } from './ManagementPages'
import { managementApi } from './managementApi'

function Providers({ children, path }: { children: React.ReactNode; path: string }) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return <QueryClientProvider client={client}><MemoryRouter initialEntries={[path]}>{children}</MemoryRouter></QueryClientProvider>
}
function LocationProbe() { const location = useLocation(); return <div>{location.pathname}{location.search}</div> }

describe('management navigation and read-only interactions', () => {
  afterEach(() => vi.restoreAllMocks())

  it('redirects a legacy route and preserves query-state intent', () => {
    render(<Providers path="/academic/students?search=SV&page=2"><Routes>
      <Route path="/academic/students" element={<LegacyRedirect to="/management/people/students" />} />
      <Route path="/management/people/students" element={<LocationProbe />} />
    </Routes></Providers>)
    expect(screen.getByText('/management/people/students?search=SV&page=2')).toBeInTheDocument()
  })

  it('restores search from URL and opens student drill-down without mutation controls', async () => {
    vi.spyOn(managementApi, 'students').mockResolvedValue([
      { studentId: '1', studentCode: 'SV000001', fullName: 'Sinh viên A', majorCode: 'SE', majorName: 'KTPM', facultyName: 'CNTT', academicYearName: '2026', rawStudyStatus: 1, isGraduated: false },
      { studentId: '2', studentCode: 'OTHER', fullName: 'Sinh viên B', majorCode: 'CS', majorName: 'KHMT', facultyName: 'CNTT', academicYearName: '2026', rawStudyStatus: 2, isGraduated: false },
    ])
    render(<Providers path="/management/people/students?search=SV000001"><Routes><Route path="/management/people/students" element={<ManagementStudentsPage />} /></Routes></Providers>)
    const input = await screen.findByRole('searchbox', { name: 'Tìm kiếm danh sách' })
    expect(input).toHaveValue('SV000001')
    expect(screen.getByText('SV000001')).toBeInTheDocument()
    expect(screen.queryByText('OTHER')).not.toBeInTheDocument()
    await userEvent.click(screen.getByText('SV000001'))
    expect(await screen.findByText('Chi tiết sinh viên')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /tạo|sửa|xóa|lưu/i })).not.toBeInTheDocument()
  })

  it('renders responsive weekly schedule and QuestionSuite-first read-only preview', async () => {
    vi.spyOn(managementApi, 'schedule').mockResolvedValue([{ scheduleId: 's1', subjectCode: 'SE101', subjectName: 'Kiến trúc', className: 'SE101.01', startDateTime: '2026-07-20T08:00:00', endDateTime: '2026-07-20T10:00:00', teacherName: null }])
    const schedule = render(<Providers path="/management/teaching/schedule"><ManagementSchedulePage /></Providers>)
    expect((await screen.findAllByText(/SE101/)).length).toBeGreaterThan(0)
    expect(schedule.container.querySelector('.schedule-desktop')).toBeInTheDocument()
    expect(schedule.container.querySelector('.schedule-mobile')).toBeInTheDocument()
    schedule.unmount()

    vi.spyOn(managementApi, 'suites').mockResolvedValue([{ questionSuiteId: 'q1', subjectId: 'sub1', name: 'Bộ câu hỏi A', creationTime: '2026-07-20T08:00:00', questionCount: 1 }])
    vi.spyOn(managementApi, 'suite').mockResolvedValue({ questionSuiteId: 'q1', subjectId: 'sub1', name: 'Bộ câu hỏi A', creationTime: '2026-07-20T08:00:00', questionCount: 1, questions: [{ questionId: 'question1', questionText: 'Nội dung câu hỏi', rawLevel: 2, answers: [{ answerId: 'a1', answerText: 'Phương án A', isSourceMarkedAnswer: true }] }] })
    render(<Providers path="/management/assessment/question-suites"><ManagementQuestionSuitesPage /></Providers>)
    await userEvent.click(await screen.findByText('Bộ câu hỏi A'))
    expect(await screen.findByText('Nội dung câu hỏi')).toBeInTheDocument()
    expect(screen.getByText('Phương án A')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /sửa|xóa|thêm|lưu/i })).not.toBeInTheDocument()
  })
})
