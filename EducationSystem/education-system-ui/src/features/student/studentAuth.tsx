import { useQueryClient } from '@tanstack/react-query'
import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { loginStudent, studentApi } from './studentApi'
import type { StudentSession } from './types'
import { clearStudentToken, getStudentToken, setStudentToken, setStudentUnauthorizedHandler, studentTokenExpired } from './studentAuthStore'

type StudentAuthValue = {
  session: StudentSession | null
  restoring: boolean
  login: (studentCode: string) => Promise<void>
  logout: () => void
}

const StudentAuthContext = createContext<StudentAuthValue | null>(null)

export function StudentAuthProvider({ children }: { children: ReactNode }) {
  const initialToken = getStudentToken()
  const [session, setSession] = useState<StudentSession | null>(null)
  const [restoring, setRestoring] = useState(Boolean(initialToken))
  const queryClient = useQueryClient()

  const logout = useCallback(() => {
    clearStudentToken()
    setSession(null)
    queryClient.removeQueries({ queryKey: ['student'] })
  }, [queryClient])

  useEffect(() => {
    setStudentUnauthorizedHandler(logout)
    const token = getStudentToken()
    if (!token || studentTokenExpired(token)) {
      logout()
      setRestoring(false)
      return () => setStudentUnauthorizedHandler()
    }
    studentApi.session()
      .then(setSession)
      .catch(logout)
      .finally(() => setRestoring(false))
    return () => setStudentUnauthorizedHandler()
  }, [logout])

  const value = useMemo<StudentAuthValue>(() => ({
    session,
    restoring,
    login: async (studentCode) => {
      const result = await loginStudent(studentCode.trim())
      setStudentToken(result.accessToken)
      setSession(result.session)
    },
    logout,
  }), [session, restoring, logout])

  return <StudentAuthContext.Provider value={value}>{children}</StudentAuthContext.Provider>
}

export function useStudentAuth() {
  const context = useContext(StudentAuthContext)
  if (!context) throw new Error('useStudentAuth must be used inside StudentAuthProvider')
  return context
}
