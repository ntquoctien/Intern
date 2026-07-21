import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { StudentLoginPage } from './StudentLoginPage'
import { StudentProtectedLayout } from './StudentLayout'
import { useStudentAuth } from './studentAuth'

vi.mock('./studentAuth', () => ({ useStudentAuth: vi.fn() }))
const mockedAuth = vi.mocked(useStudentAuth)
const session = { studentId: '1', userId: '2', studentCode: 'SV000001', fullName: 'Nguyễn Văn A', userName: 'student', expiresAt: '2026-07-21T00:15:00Z' }

function renderRoutes(initialPath: string, includeLoginPage = false) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(<QueryClientProvider client={client}><MemoryRouter initialEntries={[initialPath]}><Routes>
    <Route path="/student/login" element={includeLoginPage ? <StudentLoginPage /> : <div>LOGIN_DESTINATION</div>} />
    <Route element={<StudentProtectedLayout />}><Route path="/student/dashboard" element={<div>DASHBOARD_DESTINATION</div>} /></Route>
  </Routes></MemoryRouter></QueryClientProvider>)
}

function renderLoginRoutes() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(<QueryClientProvider client={client}><MemoryRouter initialEntries={['/student/login']}><Routes>
    <Route path="/student/login" element={<StudentLoginPage />} />
    <Route path="/student/dashboard" element={<div>DASHBOARD_DESTINATION</div>} />
  </Routes></MemoryRouter></QueryClientProvider>)
}

describe('student routes', () => {
  beforeEach(() => vi.clearAllMocks())

  it('redirects unauthenticated users to login', () => {
    mockedAuth.mockReturnValue({ session: null, restoring: false, login: vi.fn(), logout: vi.fn() })
    renderRoutes('/student/dashboard')
    expect(screen.getByText('LOGIN_DESTINATION')).toBeInTheDocument()
  })

  it('redirects to dashboard after successful MSSV-only login', async () => {
    const login = vi.fn().mockResolvedValue(undefined)
    mockedAuth.mockReturnValue({ session: null, restoring: false, login, logout: vi.fn() })
    renderLoginRoutes()
    await userEvent.type(screen.getByLabelText('Mã số sinh viên'), 'SV000001')
    await userEvent.click(screen.getByRole('button', { name: 'Đăng nhập' }))
    expect(login).toHaveBeenCalledWith('SV000001')
    expect(screen.getByText('DASHBOARD_DESTINATION')).toBeInTheDocument()
    expect(screen.queryByLabelText(/mật khẩu/i)).not.toBeInTheDocument()
  })

  it('clears client session and navigates to login on logout', async () => {
    const logout = vi.fn()
    mockedAuth.mockReturnValue({ session, restoring: false, login: vi.fn(), logout })
    renderRoutes('/student/dashboard')
    await userEvent.click(screen.getByRole('button', { name: /Đăng xuất/ }))
    expect(logout).toHaveBeenCalledOnce()
    expect(screen.getByText('LOGIN_DESTINATION')).toBeInTheDocument()
  })

  it('renders keyboard-addressable read-only navigation without mutation actions', () => {
    mockedAuth.mockReturnValue({ session, restoring: false, login: vi.fn(), logout: vi.fn() })
    renderRoutes('/student/dashboard')
    expect(screen.getByRole('menuitem', { name: /Thời khóa biểu/ })).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /tạo|sửa|xóa|duyệt|nộp/i })).not.toBeInTheDocument()
  })
})
