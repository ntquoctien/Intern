import type { ColumnsType } from 'antd/es/table'
import { DataTablePage } from '../../shared/components/DataTablePage'
import { linkedRecord } from '../../shared/components/tableRenderers'
import type { LookupItem, RecordItem } from '../../shared/types/api'

type ExamResult = RecordItem & {
  subjectTeachingExamId: string
  studentId: string
  examAttemptId?: string
  result?: number
  combinedResult?: number
  notes?: string
  examResultDesc?: string
}

type Question = RecordItem & {
  questionSuiteId: string
  questionText: string
  level: number
  imageUrl?: string
}

const examResultColumns: ColumnsType<ExamResult> = [
  { title: 'Student', dataIndex: 'studentId' },
  { title: 'Subject Teaching Exam', dataIndex: 'subjectTeachingExamId' },
  { title: 'Attempt', dataIndex: 'examAttemptId', render: linkedRecord },
  { title: 'Result', dataIndex: 'result', sorter: true },
  { title: 'Combined', dataIndex: 'combinedResult', sorter: true },
  { title: 'Description', dataIndex: 'examResultDesc', sorter: true },
  { title: 'Notes', dataIndex: 'notes', sorter: true },
]

const questionColumns: ColumnsType<Question> = [
  { title: 'Question Suite', dataIndex: 'questionSuiteId' },
  { title: 'Question', dataIndex: 'questionText', sorter: true },
  { title: 'Level', dataIndex: 'level', sorter: true },
  { title: 'Image', dataIndex: 'imageUrl', sorter: true },
]

const studentLabel = (item: LookupItem) =>
  item.nickname ? `Student: ${String(item.nickname)}` : 'Student record'

export function ExamResultsPage() {
  return (
    <DataTablePage<ExamResult>
      title="Exam Results"
      description="Server-side paged exam results"
      service="exam"
      resourcePath="exam-results"
      columns={examResultColumns}
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
          field: 'subjectTeachingExamId',
          title: 'Subject Teaching Exam',
          service: 'exam',
          resourcePath: 'subject-teaching-exams',
          filterable: true,
        },
      ]}
      filterFields={['studentId', 'subjectTeachingExamId']}
      searchPlaceholder="Search exam notes/result text"
      searchHelp="notes, result description, result detail"
    />
  )
}

export function QuestionsPage() {
  return (
    <DataTablePage<Question>
      title="Questions"
      description="Question bank records"
      service="exam"
      resourcePath="questions"
      columns={questionColumns}
      relationLookups={[
        {
          field: 'questionSuiteId',
          title: 'Question Suite',
          service: 'exam',
          resourcePath: 'question-suites',
        },
      ]}
      searchPlaceholder="Search question text"
      searchHelp="question text, image URL"
    />
  )
}
