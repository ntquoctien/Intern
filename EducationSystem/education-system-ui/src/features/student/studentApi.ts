import type { ApiResponse } from '../../shared/types/api'
import { httpClient } from '../../shared/api/httpClient'
import type { Attendance, ExamResult, FormRequest, ScheduleItem, StudentProfile, StudentProgram, StudentSession, StudentSubject } from './types'

const origins = {
  identity: import.meta.env.VITE_IDENTITY_API_ORIGIN ?? 'http://localhost:5001',
  academic: import.meta.env.VITE_ACADEMIC_API_ORIGIN ?? 'http://localhost:5002',
  exam: import.meta.env.VITE_EXAM_API_ORIGIN ?? 'http://localhost:5003',
  communication: import.meta.env.VITE_COMMUNICATION_API_ORIGIN ?? 'http://localhost:5004',
}

async function read<T>(service: keyof typeof origins, path: string) {
  const response = await httpClient.get<ApiResponse<T>>(`${origins[service]}/api/student/me/${path}`)
  return response.data.data as T
}

export async function loginStudent(studentCode: string) {
  const response = await httpClient.post<ApiResponse<{ accessToken: string; session: StudentSession }>>(
    `${origins.identity}/api/auth/student/login`,
    { studentCode },
  )
  return response.data.data
}

export const studentApi = {
  session: async () => {
    const response = await httpClient.get<ApiResponse<StudentSession>>(`${origins.identity}/api/student/me/session`)
    return response.data.data as StudentSession
  },
  profile: () => read<StudentProfile>('academic', 'profile'),
  program: () => read<StudentProgram>('academic', 'program'),
  subjects: () => read<StudentSubject[]>('academic', 'subjects'),
  schedule: () => read<ScheduleItem[]>('academic', 'schedule'),
  examResults: () => read<ExamResult[]>('exam', 'exam-results'),
  attendance: () => read<Attendance[]>('academic', 'attendance'),
  formRequests: () => read<FormRequest[]>('communication', 'form-requests'),
  formRequest: (id: string) => read<FormRequest>('communication', `form-requests/${id}`),
}
