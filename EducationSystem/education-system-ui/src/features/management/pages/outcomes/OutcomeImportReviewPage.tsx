import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ArrowLeftOutlined, CheckCircleOutlined, FileSearchOutlined, FileTextOutlined, ReloadOutlined, SaveOutlined, StopOutlined } from '@ant-design/icons'
import { Alert, Button, Card, Collapse, Descriptions, Empty, Input, InputNumber, Modal, Popover, Progress, Select, Space, Table, Tabs, Tag, Typography, message, type TableColumnsType } from 'antd'
import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { OutcomeStatusTag } from './OutcomeStatusTag'
import { outcomeApi, problemMessage, type CloDraft, type DraftBase, type ExtractionQualityReport, type ImportReview, type MappingDraft, type PloDraft, type ReviewOperation, type SubjectDraft, type Warning } from './outcomeApi'

export function OutcomeImportReviewPage() {
  const id = Number(useParams().id)
  const navigate = useNavigate()
  const client = useQueryClient()
  const review = useQuery({
    queryKey: ['outcome-import-review', id],
    queryFn: () => outcomeApi.review(id),
    enabled: Number.isFinite(id),
    refetchInterval: query => ['Uploaded', 'Processing'].includes(query.state.data?.import.status ?? '') ? 2500 : false,
  })
  const approvedDocument = useQuery({
    queryKey: ['approved-outcome-document', id],
    queryFn: () => outcomeApi.approvedDocument(id),
    enabled: review.data?.import.status === 'Approved',
  })
  const refresh = async () => { await client.invalidateQueries({ queryKey: ['outcome-import-review', id] }) }
  const patch = useMutation({
    mutationFn: (operations: ReviewOperation[]) => outcomeApi.patch(id, review.data!.import.rowVersion, operations),
    onSuccess: data => { client.setQueryData(['outcome-import-review', id], data); message.success('Đã lưu thay đổi.') },
    onError: error => { message.error(problemMessage(error)); void refresh() },
  })
  const validate = useMutation({
    mutationFn: () => outcomeApi.validate(id, review.data!.import.rowVersion),
    onSuccess: data => { client.setQueryData(['outcome-import-review', id], data); message.success('Đã kiểm tra lại dữ liệu.') },
    onError: error => { message.error(problemMessage(error)); void refresh() },
  })
  const approve = useMutation({
    mutationFn: () => outcomeApi.approve(id, review.data!.import.rowVersion),
    onSuccess: () => { message.success('Đã phê duyệt và ghi CLO/PLO chính thức.'); void refresh() },
    onError: error => { message.error(problemMessage(error)); void refresh() },
  })
  const reject = useMutation({
    mutationFn: (reason: string) => outcomeApi.reject(id, review.data!.import.rowVersion, reason),
    onSuccess: () => { message.success('Đã từ chối bản import.'); void refresh() },
    onError: error => { message.error(problemMessage(error)); void refresh() },
  })

  if (review.isLoading) return <Card><Progress percent={0} status="active" /><Typography.Text>Đang tải workspace kiểm duyệt…</Typography.Text></Card>
  if (review.isError || !review.data) return <Alert type="error" showIcon message="Không thể tải dữ liệu kiểm duyệt" description={problemMessage(review.error)} action={<Button onClick={() => review.refetch()}>Thử lại</Button>} />
  const data = review.data
  const isProcessing = ['Uploaded', 'Processing'].includes(data.import.status)
  const canEdit = data.allowedActions.includes('edit')
  const blocking = data.validation.filter(item => item.severity === 'BlockingError')
  const warnings = data.validation.filter(item => item.severity !== 'BlockingError')

  const confirmApprove = () => Modal.confirm({
    title: 'Phê duyệt dữ liệu CLO/PLO?',
    content: 'Hệ thống sẽ ghi dữ liệu vào các bảng chính thức theo cơ chế insert-only. Thao tác này không thể chỉnh sửa lại trong workspace.',
    okText: 'Phê duyệt', cancelText: 'Huỷ', okButtonProps: { disabled: blocking.length > 0 },
    onOk: () => approve.mutateAsync(),
  })
  const confirmReject = () => {
    let reason = ''
    Modal.confirm({
      title: 'Từ chối bản import', content: <Input.TextArea placeholder="Nhập lý do bắt buộc" onChange={event => { reason = event.target.value }} />,
      okText: 'Từ chối', okButtonProps: { danger: true }, cancelText: 'Huỷ',
      onOk: async () => {
        if (!reason.trim()) { message.error('Vui lòng nhập lý do từ chối.'); throw new Error('reason-required') }
        await reject.mutateAsync(reason)
      },
    })
  }

  return <div className="outcome-page outcome-review-page">
    <Button icon={<ArrowLeftOutlined />} onClick={() => navigate('/management/system/outcomes')}>Danh sách import</Button>
    <div className="outcome-page-heading">
      <div><Space><Typography.Title level={3}>Kiểm duyệt CLO/PLO #{id}</Typography.Title><OutcomeStatusTag status={data.import.status} /></Space><Typography.Text type="secondary">{data.import.originalFileName}</Typography.Text></div>
      <Space wrap>
        <Button icon={<ReloadOutlined />} onClick={() => review.refetch()}>Tải lại</Button>
        {approvedDocument.data && <Button icon={<FileTextOutlined />} onClick={() => Modal.info({
          title: `Official JSON v${approvedDocument.data.documentVersion}`,
          width: 760,
          content: <pre className="outcome-source-json">{JSON.stringify(approvedDocument.data.content, null, 2)}</pre>,
        })}>JSON chính thức</Button>}
        {data.allowedActions.includes('validate') && <Button loading={validate.isPending} onClick={() => validate.mutate()}>Kiểm tra lại</Button>}
        {data.allowedActions.includes('reject') && <Button danger icon={<StopOutlined />} onClick={confirmReject}>Từ chối</Button>}
        {data.allowedActions.includes('approve') && <Button type="primary" icon={<CheckCircleOutlined />} disabled={blocking.length > 0} loading={approve.isPending} onClick={confirmApprove}>Phê duyệt</Button>}
      </Space>
    </div>
    {isProcessing && <Alert type="info" showIcon message="Tài liệu đang được phân tích" description={<Progress percent={data.import.progressPercent} status="active" />} />}
    {data.import.status === 'Failed' && <Alert type="error" showIcon message={data.import.errorCode ?? 'Xử lý lỗi'} description={data.import.errorMessage} action={data.import.isRetryable ? <Button onClick={() => outcomeApi.process(id).then(refresh)}>Thử lại</Button> : undefined} />}
    <ValidationSummary blocking={blocking} warnings={warnings} />
    {data.qualityReport && <QualitySummary report={data.qualityReport} />}
    {approvedDocument.data && <Card size="small" title="ApprovedOutcomeJsonDocument">
      <Descriptions size="small" column={{ xs: 1, md: 3 }} items={[
        { key: 'version', label: 'Phiên bản', children: `v${approvedDocument.data.documentVersion} / schema ${approvedDocument.data.schemaVersion}` },
        { key: 'status', label: 'Trạng thái', children: <Tag color="green">{approvedDocument.data.status}</Tag> },
        { key: 'hash', label: 'SHA-256', children: <Typography.Text code copyable>{approvedDocument.data.contentHash}</Typography.Text> },
      ]} />
    </Card>}
    {!data.document ? <Empty description="Chưa có dữ liệu trích xuất để kiểm duyệt." /> :
      <Tabs className="outcome-review-tabs" items={[
        { key: 'plos', label: `PLO (${data.document.plos.filter(item => !item.removed).length})`, children: <PloTable items={data.document.plos} editable={canEdit} saving={patch.isPending} save={operation => patch.mutate([operation])} /> },
        { key: 'subjects', label: `Môn học & CLO (${data.document.subjects.filter(item => !item.removed).length})`, children: <SubjectWorkspace items={data.document.subjects} editable={canEdit} saving={patch.isPending} save={operation => patch.mutate([operation])} /> },
        { key: 'matrix', label: `Ma trận (${data.document.mappings.filter(item => !item.removed).length})`, children: <MappingTable review={data} editable={canEdit} saving={patch.isPending} save={operation => patch.mutate([operation])} /> },
        { key: 'source', label: `Nguồn (${data.blocks.length})`, children: <SourceBlocks review={data} /> },
      ]} />}
  </div>
}

function QualitySummary({ report }: { report: ExtractionQualityReport }) {
  const percent = (value: number) => `${Math.round(value * 100)}%`
  return <Card className="outcome-validation-card" title="Chất lượng trích xuất">
    <Descriptions size="small" column={{ xs: 1, sm: 2, lg: 4 }} items={[
      { key: 'scope', label: 'Đúng phạm vi', children: percent(report.scopeCompliance) },
      { key: 'source', label: 'Phủ nguồn dẫn', children: percent(report.sourceReferenceCoverage) },
      { key: 'clo', label: 'CLO nguồn / giữ lại', children: `${report.cloCountSource} / ${report.cloCountReviewed}` },
      { key: 'overlap', label: 'Text overlap', children: percent(report.textOverlapAverage) },
      { key: 'clo-code', label: 'Mã CLO khớp nguồn', children: percent(report.exactCloCodeMatchRate) },
      { key: 'plo-code', label: 'Mã PLO khớp nguồn', children: percent(report.exactPloCodeMatchRate) },
      { key: 'removed', label: 'Môn ngoài phạm vi đã loại', children: report.removedOutOfScopeSubjectCount },
      { key: 'mapping', label: 'Mapping treo / trùng', children: `${report.danglingMappingCount} / ${report.duplicateMappingCount}` },
    ]} />
  </Card>
}

function ValidationSummary({ blocking, warnings }: { blocking: Warning[]; warnings: Warning[] }) {
  if (!blocking.length && !warnings.length) return <Alert type="success" showIcon message="Dữ liệu đã vượt qua các kiểm tra bắt buộc." />
  return <Card className="outcome-validation-card" title={`Kiểm tra dữ liệu: ${blocking.length} lỗi chặn · ${warnings.length} cảnh báo`}>
    {!!blocking.length && <Alert type="error" showIcon message="Chưa thể phê duyệt" description={<ul>{blocking.map((item, index) => <li key={`${item.code}-${index}`}><b>{item.code}</b>: {item.message}</li>)}</ul>} />}
    {!!warnings.length && <Collapse ghost items={[{ key: 'warnings', label: `Xem ${warnings.length} cảnh báo`, children: <ul>{warnings.map((item, index) => <li key={`${item.code}-${index}`}><b>{item.code}</b>: {item.message}</li>)}</ul> }]} />}
  </Card>
}

function PloTable({ items, editable, saving, save }: { items: PloDraft[]; editable: boolean; saving: boolean; save: (operation: ReviewOperation) => void }) {
  const columns: TableColumnsType<PloDraft> = [
    { title: 'Mã PLO', width: 150, render: (_, item) => <EditableText value={item.ploCode} disabled={!editable} saving={saving} onSave={value => save({ entityType: 'PLO', draftId: item.draftId, fieldName: 'ploCode', value })} /> },
    { title: 'Mô tả', render: (_, item) => <EditableText value={item.description} textArea disabled={!editable} saving={saving} onSave={value => save({ entityType: 'PLO', draftId: item.draftId, fieldName: 'description', value })} /> },
    { title: 'Thứ tự', width: 110, render: (_, item) => <EditableNumber value={item.sortOrder} disabled={!editable} saving={saving} onSave={value => save({ entityType: 'PLO', draftId: item.draftId, fieldName: 'sortOrder', value })} /> },
    { title: 'Tin cậy', width: 110, render: (_, item) => <Tag color={item.confidence < .6 ? 'orange' : 'green'}>{Math.round(item.confidence * 100)}%</Tag> },
    { title: 'Minh chứng', width: 110, render: (_, item) => <Evidence item={item} /> },
  ]
  return <Table rowKey="draftId" columns={columns} dataSource={items.filter(item => !item.removed)} pagination={false} scroll={{ x: 900 }} />
}

function SubjectWorkspace({ items, editable, saving, save }: { items: SubjectDraft[]; editable: boolean; saving: boolean; save: (operation: ReviewOperation) => void }) {
  return <Collapse items={items.filter(item => !item.removed).map(subject => ({
    key: subject.draftId,
    label: <Space><b>{subject.subject.code || 'Chưa có mã môn'}</b><span>{subject.subject.name}</span><Tag color={subject.subject.matchStatus === 'Matched' ? 'green' : 'red'}>{subject.subject.matchStatus}</Tag><Tag>{subject.clos.filter(item => !item.removed).length} CLO</Tag></Space>,
    children: <div>
      <Descriptions size="small" column={{ xs: 1, md: 3 }} items={[
        { key: 'code', label: 'Mã môn', children: <EditableText value={subject.subject.code} disabled={!editable} saving={saving} onSave={value => save({ entityType: 'Subject', draftId: subject.draftId, fieldName: 'subject.code', value })} /> },
        { key: 'name', label: 'Tên môn', children: <EditableText value={subject.subject.name} disabled={!editable} saving={saving} onSave={value => save({ entityType: 'Subject', draftId: subject.draftId, fieldName: 'subject.name', value })} /> },
        { key: 'credits', label: 'Tín chỉ', children: <EditableNumber value={subject.subject.credits ?? 0} disabled={!editable} saving={saving} onSave={value => save({ entityType: 'Subject', draftId: subject.draftId, fieldName: 'subject.credits', value })} /> },
      ]} />
      <Table rowKey="draftId" pagination={false} dataSource={subject.clos.filter(item => !item.removed)} columns={[
        { title: 'Mã CLO', width: 150, render: (_: unknown, clo: CloDraft) => <EditableText value={clo.cloCode} disabled={!editable} saving={saving} onSave={value => save({ entityType: 'CLO', draftId: clo.draftId, fieldName: 'cloCode', value })} /> },
        { title: 'Mô tả', render: (_: unknown, clo: CloDraft) => <EditableText value={clo.description} textArea disabled={!editable} saving={saving} onSave={value => save({ entityType: 'CLO', draftId: clo.draftId, fieldName: 'description', value })} /> },
        { title: 'Tin cậy', width: 100, render: (_: unknown, clo: CloDraft) => `${Math.round(clo.confidence * 100)}%` },
        { title: 'Nguồn', width: 100, render: (_: unknown, clo: CloDraft) => <Evidence item={clo} /> },
      ]} />
    </div>,
  }))} />
}

function MappingTable({ review, editable, saving, save }: { review: ImportReview; editable: boolean; saving: boolean; save: (operation: ReviewOperation) => void }) {
  const document = review.document!
  const cloNames = useMemo(() => new Map(document.subjects.flatMap(subject => subject.clos.map(clo => [clo.draftId, `${subject.subject.code} · ${clo.cloCode}`] as const))), [document])
  const ploNames = useMemo(() => new Map(document.plos.map(plo => [plo.draftId, plo.ploCode])), [document])
  const columns: TableColumnsType<MappingDraft> = [
    { title: 'CLO', render: (_, item) => cloNames.get(item.cloDraftId) ?? item.cloDraftId },
    { title: 'PLO', render: (_, item) => ploNames.get(item.ploDraftId) ?? item.ploDraftId },
    { title: 'Mức đóng góp', width: 180, render: (_, item) => <Select value={item.progressionLevel} disabled={!editable || saving} options={[{ value: 'E', label: 'E — Giới thiệu' }, { value: 'R', label: 'R — Củng cố' }, { value: 'D', label: 'D — Thành thạo' }]} onChange={value => save({ entityType: 'Mapping', draftId: item.draftId, fieldName: 'progressionLevel', value })} /> },
    { title: 'Nguồn', width: 100, render: (_, item) => <Evidence item={item} /> },
  ]
  return <Table rowKey="draftId" columns={columns} dataSource={document.mappings.filter(item => !item.removed)} pagination={{ pageSize: 20 }} />
}

function SourceBlocks({ review }: { review: ImportReview }) {
  return <Collapse items={review.blocks.map(block => {
    let content = block.contentJson
    try { content = JSON.stringify(JSON.parse(content), null, 2) } catch { /* retain source */ }
    return { key: block.blockId, label: <Space><Tag>{block.blockType}</Tag><b>{block.blockId}</b><span>#{block.sequence}</span></Space>, children: <pre className="outcome-source-json">{content}</pre> }
  })} />
}

function Evidence({ item }: { item: DraftBase }) {
  return <Popover title="Minh chứng trong tài liệu nguồn" content={<div className="outcome-evidence">{item.sourceReferences.length ? item.sourceReferences.map(source => <div key={`${source.blockId}-${source.sequence}`}><b>{source.blockId}</b><small>{source.headingPath.join(' › ')}</small><p>{source.excerpt}</p></div>) : 'Chưa có nguồn tham chiếu.'}</div>} trigger="click"><Button size="small" icon={<FileSearchOutlined />}>{item.sourceReferences.length}</Button></Popover>
}

function EditableText({ value, textArea, disabled, saving, onSave }: { value: string; textArea?: boolean; disabled: boolean; saving: boolean; onSave: (value: string) => void }) {
  const [draft, setDraft] = useState(value)
  useEffect(() => setDraft(value), [value])
  const input = textArea ? <Input.TextArea autoSize={{ minRows: 2, maxRows: 6 }} value={draft} disabled={disabled} onChange={event => setDraft(event.target.value)} /> : <Input value={draft} disabled={disabled} onChange={event => setDraft(event.target.value)} />
  return <div className="outcome-editable">{input}{!disabled && draft !== value && <Button type="text" aria-label="Lưu thay đổi" icon={<SaveOutlined />} loading={saving} onClick={() => onSave(draft)} />}</div>
}

function EditableNumber({ value, disabled, saving, onSave }: { value: number; disabled: boolean; saving: boolean; onSave: (value: number) => void }) {
  const [draft, setDraft] = useState(value)
  useEffect(() => setDraft(value), [value])
  return <div className="outcome-editable"><InputNumber min={0} value={draft} disabled={disabled} onChange={next => setDraft(next ?? 0)} />{!disabled && draft !== value && <Button type="text" aria-label="Lưu thay đổi" icon={<SaveOutlined />} loading={saving} onClick={() => onSave(draft)} />}</div>
}
