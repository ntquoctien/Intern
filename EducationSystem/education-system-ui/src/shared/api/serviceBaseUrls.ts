import type { ServiceKey } from '../types/api'

export const serviceBaseUrls: Record<ServiceKey, string> = {
  academic:
    import.meta.env.VITE_ACADEMIC_API_BASE ?? 'http://localhost:5002/api/academic',
  communication:
    import.meta.env.VITE_COMMUNICATION_API_BASE ??
    'http://localhost:5004/api/communication',
  exam: import.meta.env.VITE_EXAM_API_BASE ?? 'http://localhost:5003/api/exam',
  identity:
    import.meta.env.VITE_IDENTITY_API_BASE ?? 'http://localhost:5001/api/identity',
}
