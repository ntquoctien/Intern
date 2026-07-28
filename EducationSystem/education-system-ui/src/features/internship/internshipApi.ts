import type { ApiResponse } from '../../shared/types/api'
import { httpClient } from '../../shared/api/httpClient'

const communicationOrigin =
  import.meta.env.VITE_COMMUNICATION_API_ORIGIN ?? 'http://localhost:5004'

export type SubmitInternshipPayload = {
  companyName: string
  position: string
  mentorEmail: string
  startDate: string
  endDate?: string | null
  taskDescription?: string | null
}

export type InternshipRequestCreated = {
  requestId: string
  verificationStatus: string
  createdAt: string
}

export type EmployerVerificationContext = {
  companyName: string
  studentName: string
  studentCode: string
  position: string
  startDate: string
  endDate?: string | null
  taskDescription?: string | null
}

export type EmployerVerificationPayload = {
  token: string
  isInformationCorrect: boolean
  score: number
  evaluationNotes?: string | null
}

export const internshipApi = {
  submit: async (payload: SubmitInternshipPayload) => {
    // Student JWT is attached by the shared Axios interceptor.
    const response = await httpClient.post<ApiResponse<InternshipRequestCreated>>(
      `${communicationOrigin}/api/communication/form-requests/submit-internship-request`,
      payload,
    )
    return response.data.data
  },

  getEmployerContext: async (token: string) => {
    const response = await httpClient.get<ApiResponse<EmployerVerificationContext>>(
      `${communicationOrigin}/api/communication/form-requests/internship-verification-context`,
      { params: { token } },
    )
    return response.data.data
  },

  verifyByEmployer: async (payload: EmployerVerificationPayload) => {
    // This endpoint is [AllowAnonymous]; the one-time token is its credential.
    const response = await httpClient.post<ApiResponse<{
      requestId: string
      employerVerifiedStatus: number
      verifiedAt: string
    }>>(
      `${communicationOrigin}/api/communication/form-requests/verify-by-employer`,
      payload,
    )
    return response.data.data
  },
}
