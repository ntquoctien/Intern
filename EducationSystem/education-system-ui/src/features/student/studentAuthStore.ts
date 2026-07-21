const persistInSession = import.meta.env.VITE_STUDENT_SESSION_STORAGE === 'true'
const storageKey = 'education.student.access-token'
let accessToken: string | null = persistInSession ? sessionStorage.getItem(storageKey) : null
let unauthorizedHandler: (() => void) | undefined

export function getStudentToken() { return accessToken }

export function setStudentToken(token: string) {
  accessToken = token
  if (persistInSession) sessionStorage.setItem(storageKey, token)
}

export function clearStudentToken() {
  accessToken = null
  sessionStorage.removeItem(storageKey)
}

export function setStudentUnauthorizedHandler(handler?: () => void) { unauthorizedHandler = handler }

export function handleStudentUnauthorized() { unauthorizedHandler?.() }

export function studentTokenExpired(token: string) {
  try {
    const payload = JSON.parse(atob(token.split('.')[1])) as { exp?: number }
    return !payload.exp || payload.exp * 1000 <= Date.now()
  } catch {
    return true
  }
}
