import { useQuery } from '@tanstack/react-query'
import { CloseOutlined, DatabaseOutlined, FileImageOutlined, FilterOutlined, MoreOutlined, SearchOutlined } from '@ant-design/icons'
import { Alert, Button, Card, Empty, Image, Input, Pagination, Select, Skeleton, Space, Tag, Typography } from 'antd'
import dayjs from 'dayjs'
import { Link, useSearchParams } from 'react-router-dom'
import { managementApi, type QuestionBankItem, type SuiteSummary } from '../../managementApi'

const number = new Intl.NumberFormat('vi-VN')
const levelColor = (level: number) => ['green', 'blue', 'orange', 'red'][level] ?? 'default'
const levelLabel = (level: number) => `Mức ${level}`

export function ManagementQuestionBankPage() {
  const [params, setParams] = useSearchParams()
  const suitePage = Number(params.get('suitePage') ?? 1)
  const questionPage = Number(params.get('page') ?? 1)
  const pageSize = Number(params.get('pageSize') ?? 10)
  const suiteSearch = params.get('suiteSearch') ?? ''
  const questionSearch = params.get('search') ?? ''
  const level = params.get('level') ?? ''
  const suitesQuery = useQuery({ queryKey: ['management', 'question-suites'], queryFn: managementApi.suites })
  const subjectsQuery = useQuery({ queryKey: ['management', 'subjects'], queryFn: managementApi.subjects })
  const suites = (suitesQuery.data ?? []).filter(item => !suiteSearch || item.name.toLocaleLowerCase('vi').includes(suiteSearch.toLocaleLowerCase('vi')))
  const visibleSuites = suites.slice((suitePage - 1) * 10, suitePage * 10)
  const suiteId = params.get('suiteId') ?? visibleSuites[0]?.questionSuiteId
  const questions = useQuery({ queryKey: ['management', 'questions-page', suiteId, questionPage, pageSize, questionSearch, level], queryFn: () => managementApi.questionPage({ suiteId, pageNumber: questionPage, pageSize, search: questionSearch || undefined, level: level || undefined }), enabled: !!suiteId })
  const questionId = params.get('id') ?? questions.data?.items[0]?.questionId
  const detail = useQuery({ queryKey: ['management', 'question-detail', questionId], queryFn: () => managementApi.questionDetail(questionId!), enabled: !!questionId })
  const update = (values: Record<string, string | undefined>) => {
    const next = new URLSearchParams(params)
    Object.entries(values).forEach(([key, value]) => value ? next.set(key, value) : next.delete(key))
    setParams(next)
  }
  const subjects = new Map((subjectsQuery.data ?? []).map(item => [item.subjectId, item]))
  const selectedSuite = (suitesQuery.data ?? []).find(item => item.questionSuiteId === suiteId)

  if (suitesQuery.isLoading) return <Skeleton active paragraph={{ rows: 16 }} />
  if (suitesQuery.isError || !suitesQuery.data) return <Alert type="error" showIcon title="Không thể tải ngân hàng câu hỏi" />

  return <div className="question-bank-grid">
    <Card className="dashboard-panel question-suite-panel" title={<>Bộ câu hỏi <Tag>{number.format(suites.length)}</Tag></>}>
      <div className="question-panel-search"><Input allowClear prefix={<SearchOutlined />} value={suiteSearch} placeholder="Tìm theo tên bộ câu hỏi…" onChange={event => update({ suiteSearch: event.target.value || undefined, suitePage: '1' })} /><Button icon={<FilterOutlined />}>Bộ lọc</Button></div>
      <div className="question-suite-list">{visibleSuites.map(item => <SuiteRow key={item.questionSuiteId} item={item} selected={item.questionSuiteId === suiteId} subjectName={subjects.get(item.subjectId)?.name} onClick={() => update({ suiteId: item.questionSuiteId, id: undefined, page: '1' })} />)}</div>
      <Pagination size="small" current={suitePage} pageSize={10} total={suites.length} showSizeChanger={false} onChange={value => update({ suitePage: String(value) })} />
    </Card>
    <Card className="dashboard-panel question-list-panel" title={<>Câu hỏi <Tag>{number.format(questions.data?.totalItems ?? 0)}</Tag><small>{selectedSuite?.name}</small></>}>
      <div className="question-panel-search"><Input allowClear prefix={<SearchOutlined />} value={questionSearch} placeholder="Tìm theo nội dung câu hỏi…" onChange={event => update({ search: event.target.value || undefined, page: '1' })} /><Select allowClear value={level || undefined} placeholder="Mức" options={[0, 1, 2, 3].map(value => ({ value: String(value), label: levelLabel(value) }))} onChange={value => update({ level: value, page: '1' })} /></div>
      {questions.isLoading ? <Skeleton active /> : questions.isError || !questions.data ? <Alert type="error" message="Không thể tải câu hỏi" /> : <>
        <div className="question-list">{questions.data.items.map((item, index) => <QuestionRow key={item.questionId} item={item} index={(questionPage - 1) * pageSize + index + 1} selected={item.questionId === questionId} onClick={() => update({ id: item.questionId })} />)}</div>
        <Pagination size="small" current={questionPage} pageSize={pageSize} total={questions.data.totalItems} showSizeChanger onChange={(page, size) => update({ page: String(page), pageSize: String(size) })} />
      </>}
    </Card>
    <Card className="dashboard-panel question-detail-panel" title="Chi tiết câu hỏi" extra={<Button type="text" icon={<CloseOutlined />} onClick={() => update({ id: undefined })} />}>
      {detail.isLoading ? <Skeleton active /> : detail.isError || !detail.data ? <Empty description="Chọn câu hỏi để xem chi tiết" /> : <div className="question-detail-content">
        <Space><Tag>ID: {detail.data.questionId.slice(0, 12)}</Tag><Tag color={levelColor(detail.data.rawLevel)}>{levelLabel(detail.data.rawLevel)}</Tag></Space>
        <div className="question-meta"><span>Bộ câu hỏi<b>{detail.data.suiteName}</b></span><span>Môn học<b>{subjects.get(detail.data.subjectId)?.name ?? 'Đang đối chiếu…'}</b></span><span>Level nguồn<b>{detail.data.rawLevel}</b></span><span>Loại câu hỏi<b>{detail.data.answers.length ? 'Có phương án' : 'Không có phương án'}</b></span></div>
        <Typography.Title level={5}>Nội dung câu hỏi</Typography.Title>
        <div className="question-content-box"><Typography.Paragraph>{detail.data.questionText}</Typography.Paragraph>{safeImage(detail.data.imageUrl)}</div>
        <div className="question-answer-title"><Typography.Title level={5}>Đáp án</Typography.Title><Tag color="blue">Nội dung được giới hạn theo quyền</Tag></div>
        <div className="question-answer-list">{detail.data.answers.map((answer, index) => <div key={answer.answerId}><span>{String.fromCharCode(65 + index)}</span><p>{answer.answerText}</p>{safeImage(answer.imageUrl, true)}</div>)}</div>
        <Alert type="info" showIcon message="Cờ đáp án đúng không được gửi tới trình duyệt khi chưa có RBAC khảo thí được xác thực." />
        <Typography.Title level={5}>Metadata</Typography.Title>
        <div className="question-meta"><span>Suite ID<b>{detail.data.suiteId}</b></span><span>Số phương án<b>{detail.data.answers.length}</b></span><span>Ảnh câu hỏi<b>{detail.data.imageUrl ? 'Có' : 'Không'}</b></span><span>Cập nhật bộ câu hỏi<b>{selectedSuite ? dayjs(selectedSuite.creationTime).format('DD/MM/YYYY HH:mm') : '—'}</b></span></div>
        <Typography.Title level={5}>Liên kết nhanh</Typography.Title>
        <Space wrap><Button icon={<DatabaseOutlined />}>Xem bộ câu hỏi</Button><Link to={`/management/education/subjects?id=${detail.data.subjectId}`}><Button>Xem môn học</Button></Link><Link to={`/management/assessment/results?suiteId=${detail.data.suiteId}`}><Button>Sử dụng trong kỳ thi</Button></Link></Space>
      </div>}
    </Card>
  </div>
}

function SuiteRow({ item, selected, subjectName, onClick }: { item: SuiteSummary; selected: boolean; subjectName?: string; onClick: () => void }) {
  return <button className={`question-suite-row ${selected ? 'selected' : ''}`} onClick={onClick}><span><b>{item.name}</b><MoreOutlined /></span><small>{subjectName ?? item.subjectId.slice(0, 12)}</small><small>Cập nhật: {dayjs(item.creationTime).format('DD/MM/YYYY')}</small><strong>{item.questionCount}</strong><div><i className="level-zero" /> {item.level0Count}<i className="level-one" /> {item.level1Count}<i className="level-two" /> {item.level2Count}<i className="level-three" /> {item.level3Count}</div></button>
}
function QuestionRow({ item, index, selected, onClick }: { item: QuestionBankItem; index: number; selected: boolean; onClick: () => void }) {
  return <button className={`question-row ${selected ? 'selected' : ''}`} onClick={onClick}><span>{index}</span><p>{item.questionText}{item.imageUrl && <FileImageOutlined />}</p><Tag color={levelColor(item.rawLevel)}>{levelLabel(item.rawLevel)}</Tag><small>{item.answerCount ? 'Có phương án' : 'Không có phương án'}</small></button>
}
function safeImage(url?: string | null, small = false) {
  if (!url || !/^https?:\/\//i.test(url)) return null
  return <Image className={small ? 'question-answer-image' : 'question-main-image'} src={url} fallback="" preview />
}
