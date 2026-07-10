import type { ColumnsType } from 'antd/es/table'
import { DataTablePage } from '../../shared/components/DataTablePage'
import { boolTag, dateTime, linkedRecord } from '../../shared/components/tableRenderers'
import type { LookupItem, RecordItem } from '../../shared/types/api'

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
  { title: 'User', dataIndex: 'userId' },
  { title: 'Academic Year', dataIndex: 'academicYearId' },
  { title: 'Major', dataIndex: 'majorId' },
  { title: 'Nickname', dataIndex: 'nickname', sorter: true },
  { title: 'Study Status', dataIndex: 'studyStatus', sorter: true },
  { title: 'Gender', dataIndex: 'gender', sorter: true },
  { title: 'Graduated', dataIndex: 'isGraduated', sorter: true, render: boolTag },
  { title: 'Issue', dataIndex: 'hasIssue', sorter: true, render: boolTag },
]

const subjectColumns: ColumnsType<Subject> = [
  { title: 'Code', dataIndex: 'subjectCode', sorter: true },
  { title: 'Name', dataIndex: 'name', sorter: true },
  { title: 'Faculty', dataIndex: 'facultyId' },
  { title: 'Credits', dataIndex: 'creditPoint', sorter: true },
  { title: 'Hours', dataIndex: 'totalHours', sorter: true },
  { title: 'Active', dataIndex: 'isActived', sorter: true, render: boolTag },
]

const subjectTeachingColumns: ColumnsType<SubjectTeaching> = [
  { title: 'Name', dataIndex: 'name', sorter: true },
  { title: 'Subject', dataIndex: 'subjectId' },
  { title: 'Start', dataIndex: 'startDate', sorter: true, render: dateTime },
  { title: 'End', dataIndex: 'endDate', sorter: true, render: dateTime },
  { title: 'Sessions', dataIndex: 'totalSessions', sorter: true },
  { title: 'Default Room', dataIndex: 'roomIdDefault' },
]

const subjectStudentColumns: ColumnsType<SubjectStudent> = [
  { title: 'Subject Teaching', dataIndex: 'subjectTeachingId' },
  { title: 'Student', dataIndex: 'studentId' },
]

const subjectScheduleColumns: ColumnsType<SubjectSchedule> = [
  { title: 'Subject Teaching', dataIndex: 'subjectTeachingId' },
  { title: 'Room', dataIndex: 'roomId' },
  { title: 'Teacher', dataIndex: 'teacherId', render: linkedRecord },
  { title: 'Start', dataIndex: 'startDateTime', sorter: true, render: dateTime },
  { title: 'End', dataIndex: 'endDateTime', sorter: true, render: dateTime },
  { title: 'Type', dataIndex: 'scheduleType', sorter: true },
]

const attendanceColumns: ColumnsType<Attendance> = [
  { title: 'Student', dataIndex: 'studentId' },
  { title: 'Schedule', dataIndex: 'subjectScheduleId', render: linkedRecord },
  { title: 'Status', dataIndex: 'status', sorter: true },
  { title: 'Notes', dataIndex: 'notes', sorter: true },
  { title: 'Created', dataIndex: 'creationDate', sorter: true, render: dateTime },
  { title: 'First Warning', dataIndex: 'isFirstTypeWarning', sorter: true, render: boolTag },
  { title: 'Second Warning', dataIndex: 'isSecondTypeWarning', sorter: true, render: boolTag },
]

const userLabel = (item: LookupItem) =>
  item.fullName ? `${String(item.fullName)} (${String(item.userName ?? 'user')})` : String(item.userName ?? 'User')

const studentLabel = (item: LookupItem) =>
  item.nickname ? `Student: ${String(item.nickname)}` : 'Student record'

const subjectLabel = (item: LookupItem) =>
  item.subjectCode && item.name
    ? `${String(item.subjectCode)} - ${String(item.name)}`
    : String(item.name ?? 'Subject')

const majorLabel = (item: LookupItem) =>
  item.code && item.name ? `${String(item.code)} - ${String(item.name)}` : String(item.name ?? 'Major')

export function StudentsPage() {
  return (
    <DataTablePage<Student>
      title="Students"
      description="Read-only student records from AcademicService"
      service="academic"
      resourcePath="students"
      columns={studentColumns}
      relationLookups={[
        {
          field: 'userId',
          title: 'User',
          service: 'identity',
          resourcePath: 'users',
          filterable: true,
          getLabel: userLabel,
        },
        {
          field: 'academicYearId',
          title: 'Academic Year',
          service: 'academic',
          resourcePath: 'academic-years',
        },
        {
          field: 'majorId',
          title: 'Major',
          service: 'academic',
          resourcePath: 'majors',
          getLabel: majorLabel,
        },
      ]}
      filterFields={['userId']}
      hiddenDetailFields={[
        'relativeUserId',
        'libraryId',
        'placeOfBirth',
        'hometown',
        'permanentAddress',
        'contactAddress',
        'fatherName',
        'motherName',
        'spouseName',
      ]}
      searchPlaceholder="Search profile text"
      searchHelp="nickname and non-ID profile text returned by the API"
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
      relationLookups={[
        { field: 'facultyId', title: 'Faculty', service: 'academic', resourcePath: 'faculties' },
      ]}
      searchPlaceholder="Search subject code/name"
      searchHelp="subject code, name, note"
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
      relationLookups={[
        {
          field: 'subjectId',
          title: 'Subject',
          service: 'academic',
          resourcePath: 'subjects',
          getLabel: subjectLabel,
        },
        {
          field: 'roomIdDefault',
          title: 'Default Room',
          service: 'academic',
          resourcePath: 'rooms',
        },
      ]}
      searchPlaceholder="Search teaching name"
      searchHelp="name"
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
      relationLookups={[
        {
          field: 'studentId',
          title: 'Student',
          service: 'academic',
          resourcePath: 'students',
          filterable: true,
          getLabel: studentLabel,
        },
        {
          field: 'subjectTeachingId',
          title: 'Subject Teaching',
          service: 'academic',
          resourcePath: 'subject-teachings',
          filterable: true,
        },
      ]}
      filterFields={['studentId', 'subjectTeachingId']}
      searchable={false}
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
      relationLookups={[
        {
          field: 'subjectTeachingId',
          title: 'Subject Teaching',
          service: 'academic',
          resourcePath: 'subject-teachings',
          filterable: true,
        },
        { field: 'roomId', title: 'Room', service: 'academic', resourcePath: 'rooms' },
      ]}
      filterFields={['subjectTeachingId', 'dateRange']}
      searchPlaceholder="Search schedule note"
      searchHelp="note"
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
      relationLookups={[
        {
          field: 'studentId',
          title: 'Student',
          service: 'academic',
          resourcePath: 'students',
          filterable: true,
          getLabel: studentLabel,
        },
      ]}
      filterFields={['studentId', 'status', 'dateRange']}
      searchPlaceholder="Search attendance notes"
      searchHelp="notes"
    />
  )
}
