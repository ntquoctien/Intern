import { beforeEach, describe, expect, it, vi } from 'vitest'
import { clearStudentToken, getStudentToken, setStudentToken, studentTokenExpired } from './studentAuthStore'

function token(exp: number) {
  return `header.${btoa(JSON.stringify({ exp }))}.signature`
}

describe('student token lifecycle', () => {
  beforeEach(() => {
    clearStudentToken()
    sessionStorage.clear()
  })

  it('detects expired and malformed sessions', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-07-21T00:00:00Z'))
    expect(studentTokenExpired(token(Math.floor(Date.now() / 1000) - 1))).toBe(true)
    expect(studentTokenExpired(token(Math.floor(Date.now() / 1000) + 60))).toBe(false)
    expect(studentTokenExpired('invalid')).toBe(true)
    vi.useRealTimers()
  })

  it('clears memory and sessionStorage on logout', () => {
    setStudentToken('access-token')
    sessionStorage.setItem('education.student.access-token', 'access-token')
    clearStudentToken()
    expect(getStudentToken()).toBeNull()
    expect(sessionStorage.getItem('education.student.access-token')).toBeNull()
  })
})
