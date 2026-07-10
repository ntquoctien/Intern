import type { ColumnsType } from 'antd/es/table'
import { DataTablePage } from '../../shared/components/DataTablePage'
import {
  cleanStudentReference,
  examResultTag,
  linkedRecord,
  questionLevelTag,
} from '../../shared/components/tableRenderers'
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
  { title: 'Học viên', dataIndex: 'studentId', render: cleanStudentReference },
  { title: 'Kỳ thi', dataIndex: 'subjectTeachingExamId' },
  { title: 'Lần thi', dataIndex: 'examAttemptId', render: linkedRecord },
  { title: 'Kết quả', dataIndex: 'result', sorter: true, render: examResultTag },
  { title: 'Kết quả ghép', dataIndex: 'combinedResult', sorter: true },
  { title: 'Mô tả', dataIndex: 'examResultDesc', sorter: true },
  { title: 'Ghi chú', dataIndex: 'notes', sorter: true },
]

const questionColumns: ColumnsType<Question> = [
  { title: 'Bộ câu hỏi', dataIndex: 'questionSuiteId' },
  { title: 'Nội dung câu hỏi', dataIndex: 'questionText', sorter: true },
  { title: 'Mức độ', dataIndex: 'level', sorter: true, render: questionLevelTag },
  { title: 'Hình ảnh', dataIndex: 'imageUrl', sorter: true },
]

const studentLabel = (item: LookupItem) =>
  item.nickname ? `Student: ${String(item.nickname)}` : 'Student record'

export function ExamResultsPage() {
  return (
    <DataTablePage<ExamResult>
      title="Kết quả thi"
      description="Bảng điểm và kết quả các kỳ thi học phần"
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
      searchPlaceholder="Tìm kiếm theo ghi chú hoặc mô tả kết quả"
      searchHelp="ghi chú, mô tả kết quả"
    />
  )
}

export function QuestionsPage() {
  return (
    <DataTablePage<Question>
      title="Ngân hàng câu hỏi"
      description="Danh mục câu hỏi thi trắc nghiệm và tự luận"
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
      searchPlaceholder="Tìm kiếm nội dung câu hỏi"
      searchHelp="nội dung câu hỏi, URL hình ảnh"
    />
  )
}
