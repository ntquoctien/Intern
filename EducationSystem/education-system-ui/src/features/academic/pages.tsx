import type { ColumnsType } from 'antd/es/table'
import { DataTablePage } from '../../shared/components/DataTablePage'
import {
  attendanceTag,
  boolTag,
  cleanStudentReference,
  creditsRenderer,
  dateOnly,
  dateTime,
  genderTag,
  hoursRenderer,
  linkedRecord,
  scheduleTypeTag,
  statusTag,
} from '../../shared/components/tableRenderers'
import { formatFacilityCode } from '../../shared/utils/dataFormatters'
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
  facultyId?: string
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
  { title: 'Mã học viên', dataIndex: 'nickname', sorter: true },
  { title: 'Trạng thái', dataIndex: 'studyStatus', sorter: true, render: statusTag },
  { title: 'Giới tính', dataIndex: 'gender', sorter: true, render: genderTag },
  { title: 'Đã tốt nghiệp', dataIndex: 'isGraduated', sorter: true, render: boolTag },
  { title: 'Có vấn đề', dataIndex: 'hasIssue', sorter: true, render: boolTag },
]

const subjectColumns: ColumnsType<Subject> = [
  { title: 'Mã môn học', dataIndex: 'subjectCode', sorter: true },
  { title: 'Tên môn học', dataIndex: 'name', sorter: true },
  { title: 'Khoa/Bộ môn', dataIndex: 'facultyId' },
  { title: 'Tín chỉ', dataIndex: 'creditPoint', sorter: true, render: creditsRenderer },
  { title: 'Giờ học', dataIndex: 'totalHours', sorter: true, render: hoursRenderer },
  { title: 'Đang hoạt động', dataIndex: 'isActived', sorter: true, render: boolTag },
]

const subjectTeachingColumns: ColumnsType<SubjectTeaching> = [
  { title: 'Tên lớp học phần', dataIndex: 'name', sorter: true },
  { title: 'Môn học', dataIndex: 'subjectId' },
  { title: 'Khoa / Bộ môn', dataIndex: 'facultyId', render: formatFacilityCode },
  { title: 'Ngày bắt đầu', dataIndex: 'startDate', sorter: true, render: dateOnly },
  { title: 'Ngày kết thúc', dataIndex: 'endDate', sorter: true, render: dateOnly },
  { title: 'Số buổi', dataIndex: 'totalSessions', sorter: true },
  { title: 'Phòng mặc định', dataIndex: 'roomIdDefault' },
]

const subjectStudentColumns: ColumnsType<SubjectStudent> = [
  { title: 'Lớp học phần', dataIndex: 'subjectTeachingId' },
  { title: 'Học viên', dataIndex: 'studentId', render: cleanStudentReference },
]

const subjectScheduleColumns: ColumnsType<SubjectSchedule> = [
  { title: 'Lớp học phần', dataIndex: 'subjectTeachingId' },
  { title: 'Phòng', dataIndex: 'roomId' },
  { title: 'Giáo viên', dataIndex: 'teacherId', render: linkedRecord },
  { title: 'Thời gian bắt đầu', dataIndex: 'startDateTime', sorter: true, render: dateTime },
  { title: 'Thời gian kết thúc', dataIndex: 'endDateTime', sorter: true, render: dateTime },
  { title: 'Loại', dataIndex: 'scheduleType', sorter: true, render: scheduleTypeTag },
]

const attendanceColumns: ColumnsType<Attendance> = [
  { title: 'Học viên', dataIndex: 'studentId', render: cleanStudentReference },
  { title: 'Buổi học', dataIndex: 'subjectScheduleId', render: linkedRecord },
  { title: 'Trạng thái', dataIndex: 'status', sorter: true, render: attendanceTag },
  { title: 'Ghi chú', dataIndex: 'notes', sorter: true },
  { title: 'Ngày ghi nhận', dataIndex: 'creationDate', sorter: true, render: dateTime },
  { title: 'Cảnh cáo lần 1', dataIndex: 'isFirstTypeWarning', sorter: true, render: boolTag },
  { title: 'Cảnh cáo lần 2', dataIndex: 'isSecondTypeWarning', sorter: true, render: boolTag },
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
      title="Danh sách học viên"
      description="Quản lý hồ sơ và thông tin học viên"
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
      searchPlaceholder="Tìm kiếm theo tên hoặc mã học viên"
      searchHelp="Nhập tên hoặc mã học viên"
    />
  )
}

export function SubjectsPage() {
  return (
    <DataTablePage<Subject>
      title="Quản lý môn học"
      description="Danh mục toàn bộ môn học trong hệ thống"
      service="academic"
      resourcePath="subjects"
      columns={subjectColumns}
      relationLookups={[
        { field: 'facultyId', title: 'Faculty', service: 'academic', resourcePath: 'faculties' },
      ]}
      searchPlaceholder="Tìm kiếm mã hoặc tên môn học"
      searchHelp="Nhập mã hoặc tên môn học"
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
