import type { ApiResponse } from '../../shared/types/api'
import { httpClient } from '../../shared/api/httpClient'

const academicOrigin = import.meta.env.VITE_ACADEMIC_API_ORIGIN ?? 'http://localhost:5002'
const examOrigin = import.meta.env.VITE_EXAM_API_ORIGIN ?? 'http://localhost:5003'

async function read<T>(origin: string, path: string) {
  const response = await httpClient.get<ApiResponse<T>>(`${origin}/api/management/${path}`)
  return response.data.data as T
}

export type Dashboard = { students: number; teachers: number; subjects: number; classes: number; schedules: number; attendanceRecords: number; studentsByFaculty: { name: string; count: number }[] }
export type StudentSummary = { studentId: string; studentCode: string; fullName?: string | null; majorCode: string; majorName: string; facultyName?: string | null; academicYearName: string; rawStudyStatus?: number | null; isGraduated: boolean; hasIssue?: boolean | null }
export type TeacherSummary = { teacherFacultyId: string; userId: string; fullName?: string | null; facultyCode: string; facultyName: string; isHeadOfFaculty: boolean; classCount: number; subjectNames: string[] }
export type PlanSummary = { planId: string; majorCode: string; majorName: string; academicYearName: string; semester: number; startDate: string; endDate: string; isActive: boolean; subjectCount: number }
export type ClassSummary = { classId: string; className: string; subjectId: string; subjectCode: string; subjectName: string; facultyName?: string | null; startDate: string; endDate: string; totalSessions: number; defaultRoom?: string | null; studentCount: number; teacherNames: string[] }
export type ManagementSchedule = { scheduleId: string; subjectCode: string; subjectName: string; className: string; startDateTime: string; endDateTime: string; roomName?: string | null; teacherName?: string | null; rawScheduleType?: number | null }
export type ManagementAttendance = { attendanceId: string; subjectCode: string; subjectName: string; className: string; startDateTime: string; rawStatus: number; firstWarning?: boolean | null; secondWarning?: boolean | null }
export type RawResult = { examResultId: string; studentId: string; examName: string; examDate: string; rawResult?: number | null; rawCombinedResult?: number | null; description?: string | null }
export type ResultSummary = { recordCount: number; resultValueCount: number; minimumRawResult?: number | null; maximumRawResult?: number | null; averageRawResult?: number | null }
export type SuiteSummary = { questionSuiteId: string; subjectId: string; name: string; creationTime: string; questionCount: number }
export type SuiteDetail = SuiteSummary & { questions: { questionId: string; questionText: string; rawLevel: number; imageUrl?: string | null; answers: { answerId: string; answerText: string; imageUrl?: string | null; isSourceMarkedAnswer: boolean }[] }[] }

export const managementApi = {
  dashboard: () => read<Dashboard>(academicOrigin, 'dashboard'),
  students: () => read<StudentSummary[]>(academicOrigin, 'students'),
  teachers: () => read<TeacherSummary[]>(academicOrigin, 'teachers'),
  plans: () => read<PlanSummary[]>(academicOrigin, 'plans'),
  classes: () => read<ClassSummary[]>(academicOrigin, 'classes'),
  schedule: () => read<ManagementSchedule[]>(academicOrigin, 'schedule'),
  attendance: () => read<ManagementAttendance[]>(academicOrigin, 'attendance'),
  results: () => read<RawResult[]>(examOrigin, 'exam-results'),
  resultSummary: () => read<ResultSummary>(examOrigin, 'exam-results/summary'),
  suites: () => read<SuiteSummary[]>(examOrigin, 'question-suites'),
  suite: (id: string) => read<SuiteDetail>(examOrigin, `question-suites/${id}`),
}
