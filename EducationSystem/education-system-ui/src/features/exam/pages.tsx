import type { ColumnsType } from 'antd/es/table'
import { DataTablePage } from '../../shared/components/DataTablePage'
import { shortId } from '../../shared/components/tableRenderers'
import type { RecordItem } from '../../shared/types/api'

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
  { title: 'Id', dataIndex: 'id', sorter: true, render: shortId },
  { title: 'Student', dataIndex: 'studentId', sorter: true, render: shortId },
  {
    title: 'Subject Teaching Exam',
    dataIndex: 'subjectTeachingExamId',
    sorter: true,
    render: shortId,
  },
  { title: 'Attempt', dataIndex: 'examAttemptId', sorter: true, render: shortId },
  { title: 'Result', dataIndex: 'result', sorter: true },
  { title: 'Combined', dataIndex: 'combinedResult', sorter: true },
  { title: 'Description', dataIndex: 'examResultDesc', sorter: true },
  { title: 'Notes', dataIndex: 'notes', sorter: true },
]

const questionColumns: ColumnsType<Question> = [
  { title: 'Id', dataIndex: 'id', sorter: true, render: shortId },
  { title: 'Question Suite', dataIndex: 'questionSuiteId', sorter: true, render: shortId },
  { title: 'Question', dataIndex: 'questionText', sorter: true },
  { title: 'Level', dataIndex: 'level', sorter: true },
  { title: 'Image', dataIndex: 'imageUrl', sorter: true },
]

export function ExamResultsPage() {
  return (
    <DataTablePage<ExamResult>
      title="Exam Results"
      description="Server-side paged exam results"
      service="exam"
      resourcePath="exam-results"
      columns={examResultColumns}
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
      searchPlaceholder="Search question text"
      searchHelp="question text, image URL"
    />
  )
}
