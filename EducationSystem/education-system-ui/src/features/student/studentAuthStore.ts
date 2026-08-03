const persistInSession = import.meta.env.VITE_STUDENT_SESSION_STORAGE === 'true'
const storageKey = 'education.student.access-token'

function readStoredToken() {
  try {
    return persistInSession
      ? sessionStorage.getItem(storageKey)
      : localStorage.getItem(storageKey) ?? sessionStorage.getItem(storageKey)
  } catch {
    return null
  }
}

let accessToken: string | null = readStoredToken()
let unauthorizedHandler: (() => void) | undefined

export function getStudentToken() { return accessToken }

export function setStudentToken(token: string) {
  accessToken = token
  try {
    if (persistInSession) {
      sessionStorage.setItem(storageKey, token)
      localStorage.removeItem(storageKey)
    } else {
      localStorage.setItem(storageKey, token)
      sessionStorage.setItem(storageKey, token)
    }
  } catch {
    // Ignore storage failures in private mode.
  }
}

export function clearStudentToken() {
  accessToken = null
  try {
    sessionStorage.removeItem(storageKey)
    localStorage.removeItem(storageKey)
  } catch {
    // Ignore storage failures.
  }
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
