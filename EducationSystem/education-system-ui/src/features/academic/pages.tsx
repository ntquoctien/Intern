import type { ColumnsType } from 'antd/es/table'
import { DataTablePage } from '../../shared/components/DataTablePage'
import { boolTag, dateTime, shortId } from '../../shared/components/tableRenderers'
import type { RecordItem } from '../../shared/types/api'

type Student = RecordItem & {
  userId: string
  academicYearId: string
  majorId: string
  studyStatus?: number
  gender?: number
  nickname?: string
  isGraduated: boolean
  hasIssue?: boolean
}

type Subject = RecordItem & {
  facultyId?: string
  subjectCode: string
  name: string
  creditPoint: number
  totalHours?: number
  isActived: boolean
}

type SubjectTeaching = RecordItem & {
  subjectId: string
  name: string
  startDate: string
  endDate: string
  totalSessions: number
  roomIdDefault?: string
}

type SubjectStudent = RecordItem & {
  subjectTeachingId: string
  studentId: string
}

type SubjectSchedule = RecordItem & {
  subjectTeachingId: string
  roomId?: string
  teacherId?: string
  startDateTime: string
  endDateTime: string
  scheduleType?: number
}

type Attendance = RecordItem & {
  subjectScheduleId: string
  studentId: string
  createdById: string
  status: number
  notes: string
  creationDate: string
  isFirstTypeWarning?: boolean
  isSecondTypeWarning?: boolean
}

const studentColumns: ColumnsType<Student> = [
  { title: 'Id', dataIndex: 'id', sorter: true, render: shortId },
  { title: 'User', dataIndex: 'userId', sorter: true, render: shortId },
  { title: 'Academic Year', dataIndex: 'academicYearId', sorter: true, render: shortId },
  { title: 'Major', dataIndex: 'majorId', sorter: true, render: shortId },
  { title: 'Nickname', dataIndex: 'nickname', sorter: true },
  { title: 'Study Status', dataIndex: 'studyStatus', sorter: true },
  { title: 'Gender', dataIndex: 'gender', sorter: true },
  { title: 'Graduated', dataIndex: 'isGraduated', sorter: true, render: boolTag },
  { title: 'Issue', dataIndex: 'hasIssue', sorter: true, render: boolTag },
]

const subjectColumns: ColumnsType<Subject> = [
  { title: 'Code', dataIndex: 'subjectCode', sorter: true },
  { title: 'Name', dataIndex: 'name', sorter: true },
  { title: 'Faculty', dataIndex: 'facultyId', sorter: true, render: shortId },
  { title: 'Credits', dataIndex: 'creditPoint', sorter: true },
  { title: 'Hours', dataIndex: 'totalHours', sorter: true },
  { title: 'Active', dataIndex: 'isActived', sorter: true, render: boolTag },
]

const subjectTeachingColumns: ColumnsType<SubjectTeaching> = [
  { title: 'Name', dataIndex: 'name', sorter: true },
  { title: 'Subject', dataIndex: 'subjectId', sorter: true, render: shortId },
  { title: 'Start', dataIndex: 'startDate', sorter: true, render: dateTime },
  { title: 'End', dataIndex: 'endDate', sorter: true, render: dateTime },
  { title: 'Sessions', dataIndex: 'totalSessions', sorter: true },
  { title: 'Default Room', dataIndex: 'roomIdDefault', sorter: true, render: shortId },
]

const subjectStudentColumns: ColumnsType<SubjectStudent> = [
  { title: 'Id', dataIndex: 'id', sorter: true, render: shortId },
  { title: 'Subject Teaching', dataIndex: 'subjectTeachingId', sorter: true, render: shortId },
  { title: 'Student', dataIndex: 'studentId', sorter: true, render: shortId },
]

const subjectScheduleColumns: ColumnsType<SubjectSchedule> = [
  { title: 'Id', dataIndex: 'id', sorter: true, render: shortId },
  { title: 'Subject Teaching', dataIndex: 'subjectTeachingId', sorter: true, render: shortId },
  { title: 'Room', dataIndex: 'roomId', sorter: true, render: shortId },
  { title: 'Teacher', dataIndex: 'teacherId', sorter: true, render: shortId },
  { title: 'Start', dataIndex: 'startDateTime', sorter: true, render: dateTime },
  { title: 'End', dataIndex: 'endDateTime', sorter: true, render: dateTime },
  { title: 'Type', dataIndex: 'scheduleType', sorter: true },
]

const attendanceColumns: ColumnsType<Attendance> = [
  { title: 'Id', dataIndex: 'id', sorter: true, render: shortId },
  { title: 'Student', dataIndex: 'studentId', sorter: true, render: shortId },
  { title: 'Schedule', dataIndex: 'subjectScheduleId', sorter: true, render: shortId },
  { title: 'Status', dataIndex: 'status', sorter: true },
  { title: 'Notes', dataIndex: 'notes', sorter: true },
  { title: 'Created', dataIndex: 'creationDate', sorter: true, render: dateTime },
  { title: 'First Warning', dataIndex: 'isFirstTypeWarning', sorter: true, render: boolTag },
  { title: 'Second Warning', dataIndex: 'isSecondTypeWarning', sorter: true, render: boolTag },
]

export function StudentsPage() {
  return (
    <DataTablePage<Student>
      title="Students"
      description="Read-only student records from AcademicService"
      service="academic"
      resourcePath="students"
      columns={studentColumns}
      filterFields={['userId']}
    />
  )
}

export function SubjectsPage() {
  return (
    <DataTablePage<Subject>
      title="Subjects"
      description="Subjects from AcademicService"
      service="academic"
      resourcePath="subjects"
      columns={subjectColumns}
    />
  )
}

export function SubjectTeachingsPage() {
  return (
    <DataTablePage<SubjectTeaching>
      title="Subject Teachings"
      description="Teaching periods and subject delivery"
      service="academic"
      resourcePath="subject-teachings"
      columns={subjectTeachingColumns}
      filterFields={['subjectTeachingId']}
    />
  )
}

export function SubjectStudentsPage() {
  return (
    <DataTablePage<SubjectStudent>
      title="Subject Students"
      description="Student enrollment by subject teaching"
      service="academic"
      resourcePath="subject-students"
      columns={subjectStudentColumns}
      filterFields={['studentId', 'subjectTeachingId']}
    />
  )
}

export function SubjectSchedulesPage() {
  return (
    <DataTablePage<SubjectSchedule>
      title="Subject Schedules"
      description="Schedule records from AcademicService"
      service="academic"
      resourcePath="subject-schedules"
      columns={subjectScheduleColumns}
      filterFields={['subjectTeachingId', 'dateRange']}
    />
  )
}

export function AttendancesPage() {
  return (
    <DataTablePage<Attendance>
      title="Attendances"
      description="Large attendance table with server-side pagination"
      service="academic"
      resourcePath="attendances"
      columns={attendanceColumns}
      filterFields={['studentId', 'subjectScheduleId', 'status', 'dateRange']}
    />
  )
}
