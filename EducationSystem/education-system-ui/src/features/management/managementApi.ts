import type { ApiResponse } from '../../shared/types/api'
import { httpClient } from '../../shared/api/httpClient'
import { getPaged } from '../../shared/api/httpClient'

const academicOrigin = import.meta.env.VITE_ACADEMIC_API_ORIGIN ?? 'http://localhost:5002'
const examOrigin = import.meta.env.VITE_EXAM_API_ORIGIN ?? 'http://localhost:5003'
const identityOrigin = import.meta.env.VITE_IDENTITY_API_ORIGIN ?? 'http://localhost:5001'

async function read<T>(origin: string, path: string) {
  const response = await httpClient.get<ApiResponse<T>>(`${origin}/api/management/${path}`)
  return response.data.data as T
}

export type Dashboard = { students: number; teachers: number; subjects: number; classes: number; schedules: number; attendanceRecords: number; studentsByFaculty: { name: string; count: number }[] }
export type StudentSummary = { studentId: string; userId?: string; studentCode: string; fullName?: string | null; majorCode: string; majorName: string; facultyName?: string | null; academicYearName: string; rawStudyStatus?: number | null; isGraduated: boolean; hasIssue?: boolean | null }
export type TeacherSummary = { teacherFacultyId: string; userId: string; fullName?: string | null; facultyCode: string; facultyName: string; isHeadOfFaculty: boolean; classCount: number; subjectNames: string[] }
export type TeacherClass = { classId: string; className: string; subjectCode: string; subjectName: string; startDate: string; endDate: string; isMainTeacher?: boolean | null }
export type TeacherScheduleItem = { scheduleId: string; classId: string; className: string; subjectCode: string; subjectName: string; startDateTime: string; endDateTime: string; roomName?: string | null }
export type TeacherDetail = { teacherFacultyId: string; userId: string; fullName?: string | null; userName?: string | null; profilePicUrl?: string | null; isAccountActive?: boolean | null; facultyId: string; facultyCode: string; facultyName: string; isHeadOfFaculty: boolean; classCount: number; mainClassCount: number; classes: TeacherClass[]; upcomingSchedule: TeacherScheduleItem[] }
export type PlanSummary = { planId: string; majorCode: string; majorName: string; academicYearName: string; semester: number; startDate: string; endDate: string; isActive: boolean; subjectCount: number; subjectIds?: string[] }
export type PlanSubject = { semesterSubjectId: string; subjectId?: string | null; snapshotCode: string; snapshotName: string; snapshotCreditPoint: number; catalogCode?: string | null; catalogName?: string | null; catalogCreditPoint?: number | null; catalogMatch: boolean }
export type PlanDetail = PlanSummary & { majorId: string; facultyName?: string | null; academicYearId: string; totalCreditPoints: number; subjects: PlanSubject[] }
export type ClassSummary = { classId: string; className: string; subjectId: string; subjectCode: string; subjectName: string; facultyName?: string | null; startDate: string; endDate: string; totalSessions: number; defaultRoom?: string | null; roomCapacity?: number | null; studentCount: number; mainTeacherName?: string | null; teacherNames: string[] }
export type ClassStudent = { studentId: string; studentCode: string; fullName?: string | null; majorCode: string; majorName: string; academicYearName: string }
export type ClassTeacher = { teacherFacultyId: string; userId: string; fullName?: string | null; facultyCode: string; facultyName: string; isMainTeacher?: boolean | null }
export type ClassSchedule = { scheduleId: string; startDateTime: string; endDateTime: string; roomName?: string | null; scheduleType?: number | null; attendanceCount: number }
export type ClassDetail = ClassSummary & { facultyId?: string | null; facultyCode?: string | null; defaultRoomId?: string | null; roomCapacity?: number | null; teacherCount: number; scheduleCount: number; attendanceCount: number; students: ClassStudent[]; teachers: ClassTeacher[]; schedules: ClassSchedule[] }
export type TeachingAssignment = { assignmentId: string; teacherFacultyId: string; userId: string; fullName?: string | null; userName?: string | null; profilePicUrl?: string | null; facultyCode: string; facultyName: string; classId: string; className: string; subjectId: string; subjectCode: string; subjectName: string; startDate: string; endDate: string; isMainTeacher?: boolean | null; studentCount: number; roomCapacity?: number | null }
export type TeacherAssignmentsDetail = { teacherFacultyId: string; userId: string; fullName?: string | null; userName?: string | null; profilePicUrl?: string | null; facultyCode: string; facultyName: string; assignmentCount: number; mainAssignmentCount: number; assistantAssignmentCount: number; assignments: TeachingAssignment[] }
export type ManagementRoom = { roomId: string; name: string; numberOfSeats: number; scheduleCount: number; defaultClassCount: number }
export type TeachingSchedule = { scheduleId: string; classId: string; className: string; subjectId: string; subjectCode: string; subjectName: string; roomId?: string | null; roomName?: string | null; roomCapacity?: number | null; teacherFacultyId?: string | null; teacherName?: string | null; startDateTime: string; endDateTime: string; scheduleType?: number | null; note: string; studentCount: number }
export type ManagementSchedule = { scheduleId: string; subjectCode: string; subjectName: string; className: string; startDateTime: string; endDateTime: string; roomName?: string | null; teacherName?: string | null; rawScheduleType?: number | null }
export type ManagementAttendance = { attendanceId: string; subjectCode: string; subjectName: string; className: string; startDateTime: string; rawStatus: number; firstWarning?: boolean | null; secondWarning?: boolean | null }
export type RawResult = { examResultId: string; studentId: string; examName: string; examDate: string; rawResult?: number | null; rawCombinedResult?: number | null; description?: string | null }
export type ResultSummary = { recordCount: number; resultValueCount: number; minimumRawResult?: number | null; maximumRawResult?: number | null; averageRawResult?: number | null }
export type SuiteSummary = { questionSuiteId: string; subjectId: string; name: string; creationTime: string; questionCount: number }
export type SuiteDetail = SuiteSummary & { questions: { questionId: string; questionText: string; rawLevel: number; imageUrl?: string | null; answers: { answerId: string; answerText: string; imageUrl?: string | null; isSourceMarkedAnswer: boolean }[] }[] }
export type Faculty = { id: string; code: string; name: string; isDeleted: boolean }
export type Major = { id: string; facultyId?: string | null; code: string; name: string; trainingType: number; isDeleted: boolean }
export type AcademicYear = { id: string; name: string; year: number; startDate: string; endDate: string; isDeleted: boolean }
export type ManagementUser = { id: string; userName: string; fullName: string; birthDate?: string | null; identificationDate?: string | null; maskedIdentificationNumber?: string | null; userInternalId: string; maskedMobile?: string | null; profilePicUrl?: string | null; role: number; isActive: boolean; lastEnforceAnnouncementRead?: string | null }
export type ManagementSubject = { subjectId: string; facultyId?: string | null; facultyCode?: string | null; facultyName?: string | null; subjectCode: string; name: string; creditPoint: number; totalHours?: number | null; isActive: boolean; planCount: number; classCount: number; documentCount: number; specialNoteCount: number }
export type ManagementSubjectDetail = ManagementSubject & { note: string }

async function readAll<T>(path: string) {
  const response = await getPaged<T>('academic', path, { pageNumber: 1, pageSize: 100, sortBy: 'code', sortDirection: 'asc' })
  return response.data
}

export const managementApi = {
  dashboard: () => read<Dashboard>(academicOrigin, 'dashboard'),
  students: () => read<StudentSummary[]>(academicOrigin, 'students'),
  student: (id: string) => read<StudentSummary>(academicOrigin, `students/${id}`),
  teachers: () => read<TeacherSummary[]>(academicOrigin, 'teachers'),
  teacher: (id: string) => read<TeacherDetail>(academicOrigin, `teachers/${id}`),
  plans: () => read<PlanSummary[]>(academicOrigin, 'plans'),
  plan: (id: string) => read<PlanDetail>(academicOrigin, `plans/${id}`),
  classes: () => read<ClassSummary[]>(academicOrigin, 'classes'),
  class: (id: string) => read<ClassDetail>(academicOrigin, `classes/${id}`),
  teachingAssignments: () => read<TeachingAssignment[]>(academicOrigin, 'teaching-assignments'),
  teacherAssignments: (id: string) => read<TeacherAssignmentsDetail>(academicOrigin, `teaching-assignments/${id}`),
  rooms: () => read<ManagementRoom[]>(academicOrigin, 'rooms'),
  teachingSchedule: () => read<TeachingSchedule[]>(academicOrigin, 'teaching-schedule'),
  schedule: () => read<ManagementSchedule[]>(academicOrigin, 'schedule'),
  attendance: () => read<ManagementAttendance[]>(academicOrigin, 'attendance'),
  results: () => read<RawResult[]>(examOrigin, 'exam-results'),
  resultSummary: () => read<ResultSummary>(examOrigin, 'exam-results/summary'),
  suites: () => read<SuiteSummary[]>(examOrigin, 'question-suites'),
  suite: (id: string) => read<SuiteDetail>(examOrigin, `question-suites/${id}`),
  faculties: () => readAll<Faculty>('faculties'),
  majors: () => readAll<Major>('majors'),
  academicYears: () => readAll<AcademicYear>('academic-years'),
  users: () => read<ManagementUser[]>(identityOrigin, 'users'),
  user: (id: string) => read<ManagementUser>(identityOrigin, `users/${id}`),
  subjects: () => read<ManagementSubject[]>(academicOrigin, 'subjects'),
  subject: (id: string) => read<ManagementSubjectDetail>(academicOrigin, `subjects/${id}`),
}
