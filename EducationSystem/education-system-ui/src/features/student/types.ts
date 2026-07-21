export type StudentSession = {
  studentId: string
  userId: string
  studentCode: string
  fullName: string
  userName: string
  profilePicUrl?: string | null
  expiresAt: string
}

export type StudentProfile = StudentSession & {
  majorCode: string
  majorName: string
  facultyCode?: string | null
  facultyName?: string | null
  academicYearName: string
  academicYear: number
  studyStatus?: number | null
  isGraduated: boolean
  hasIssue?: boolean | null
}

export type ProgramSubject = { id: string; subjectId?: string | null; subjectCode: string; subjectName: string; creditPoint: number }
export type SemesterPlan = { id: string; semester: number; startDate: string; endDate: string; isActive: boolean; subjects: ProgramSubject[] }
export type StudentProgram = { majorCode: string; majorName: string; academicYearName: string; semesterPlans: SemesterPlan[] }
export type StudentSubject = { enrollmentId: string; subjectTeachingId: string; subjectId: string; subjectCode: string; subjectName: string; creditPoint: number; className: string; startDate: string; endDate: string; totalSessions: number; defaultRoom?: string | null; schedulePhase: string }
export type ScheduleItem = { scheduleId: string; subjectTeachingId: string; subjectCode: string; subjectName: string; className: string; startDateTime: string; endDateTime: string; scheduleType?: number | null; roomName?: string | null; teacherName?: string | null; note: string }
export type ExamResult = { examResultId: string; subjectTeachingExamId: string; subjectTeachingId: string; examAttemptId?: string | null; examName: string; examStartDate: string; examEndDate: string; examType: number; attemptSubmitDate?: string | null; rawResult?: number | null; rawCombinedResult?: number | null; description?: string | null; notes?: string | null }
export type Attendance = { attendanceId: string; scheduleId: string; subjectCode: string; subjectName: string; className: string; startDateTime: string; endDateTime: string; rawStatus: number; notes: string; creationDate: string; isFirstTypeWarning?: boolean | null; isSecondTypeWarning?: boolean | null }
export type FormRequest = { formRequestId: string; formTemplateId?: string | null; formTemplateName?: string | null; documentUrl?: string | null; creationDate: string; updateDate: string; rawStatus: number; approvalId?: string | null; approvalName: string; note: string }
