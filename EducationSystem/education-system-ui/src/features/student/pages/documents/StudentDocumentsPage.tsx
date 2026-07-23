import { DownloadOutlined, EyeOutlined, FileExcelOutlined, FilePdfOutlined, FileTextOutlined, FileWordOutlined, ReadOutlined, SearchOutlined } from '@ant-design/icons'
import { Alert, Card, Input, Select, Spin, Tag, Typography } from 'antd'
import { useQuery } from '@tanstack/react-query'
import dayjs from 'dayjs'
import { useMemo, useState } from 'react'
import { studentApi } from '../../studentApi'
import type { StudentDocument, StudentSubject } from '../../types'

export function StudentDocumentsPage() {
  const query = useQuery({ queryKey: ['student', 'documents'], queryFn: async () => {
    const [documents, subjects] = await Promise.all([studentApi.documents(), studentApi.subjects()])
    return { documents, subjects }
  } })
  if (query.isLoading) return <div className="center-state"><Spin /></div>
  if (query.isError || !query.data) return <Alert type="error" showIcon message="Không thể tải tài liệu môn học" />
  return <DocumentsView {...query.data} />
}

function DocumentsView({ documents, subjects }: { documents: StudentDocument[]; subjects: StudentSubject[] }) {
  const uniqueSubjects = useMemo(() => [...new Map(subjects.map(item => [item.subjectId, item])).values()], [subjects])
  const [selectedSubjectId, setSelectedSubjectId] = useState<string>(uniqueSubjects[0]?.subjectId ?? '')
  const [subjectSearch, setSubjectSearch] = useState('')
  const [documentSearch, setDocumentSearch] = useState('')
  const [type, setType] = useState<number>()
  const [sort, setSort] = useState<'newest' | 'oldest'>('newest')
  const selectedSubject = uniqueSubjects.find(item => item.subjectId === selectedSubjectId)
  const subjectNeedle = subjectSearch.trim().toLocaleLowerCase('vi')
  const visibleSubjects = uniqueSubjects.filter(item => !subjectNeedle || `${item.subjectCode} ${item.subjectName} ${item.className}`.toLocaleLowerCase('vi').includes(subjectNeedle))
  const documentNeedle = documentSearch.trim().toLocaleLowerCase('vi')
  const types = [...new Set(documents.map(item => item.rawType))].sort((a, b) => a - b)
  const visibleDocuments = documents.filter(item => (!selectedSubjectId || item.subjectId === selectedSubjectId) && (!documentNeedle || `${item.name} ${item.detail}`.toLocaleLowerCase('vi').includes(documentNeedle)) && (type == null || item.rawType === type)).sort((a, b) => (sort === 'newest' ? -1 : 1) * (dayjs(a.updateDate).valueOf() - dayjs(b.updateDate).valueOf()))

  return <div className="student-documents-page">
    <div className="student-page-heading"><Typography.Title level={2}>Tài liệu môn học</Typography.Title><Typography.Text type="secondary">Các tài liệu đã được công bố cho các môn học của bạn.</Typography.Text></div>
    <div className="documents-layout">
      <Card className="document-subjects-card" title="Chọn môn học"><Input allowClear prefix={<SearchOutlined />} value={subjectSearch} onChange={event => setSubjectSearch(event.target.value)} placeholder="Tìm môn học..." /><div className="document-subject-list">{visibleSubjects.map(item => { const count = documents.filter(document => document.subjectId === item.subjectId).length; return <button className={selectedSubjectId === item.subjectId ? 'active' : ''} key={item.subjectId} onClick={() => setSelectedSubjectId(item.subjectId)}><b>{item.subjectCode} - {item.subjectName}</b><span>{item.className}</span><Tag color={count ? 'blue' : 'default'}>{count} tài liệu</Tag></button> })}</div><Alert type="info" showIcon message="Không tìm thấy môn học?" description="Chỉ hiển thị tài liệu của các môn bạn đã đăng ký." /></Card>
      <main>
        <Card className="document-toolbar"><Input allowClear prefix={<SearchOutlined />} value={documentSearch} onChange={event => setDocumentSearch(event.target.value)} placeholder="Tìm kiếm tài liệu theo tên..." /><label><span>Loại tài liệu</span><Select allowClear value={type} onChange={setType} options={types.map(value => ({ value, label: `Loại ${value}` }))} placeholder="Tất cả loại" /></label><label><span>Sắp xếp</span><Select value={sort} onChange={setSort} options={[{ value: 'newest', label: 'Mới nhất' }, { value: 'oldest', label: 'Cũ nhất' }]} /></label></Card>
        <Card className="documents-main-card">
          <div className="documents-course-heading"><span><ReadOutlined /></span><div><Typography.Title level={3}>{selectedSubject ? `${selectedSubject.subjectCode} - ${selectedSubject.subjectName}` : 'Chọn môn học'}</Typography.Title><Typography.Text>{selectedSubject?.className ?? 'Chưa chọn lớp học phần'}</Typography.Text></div><b>Tổng cộng: {visibleDocuments.length} tài liệu</b></div>
          {visibleDocuments.length ? <div className="documents-table"><div className="documents-table-head"><b>Tên tài liệu</b><b>Loại tài liệu</b><b>Mô tả / Chi tiết</b><b>Ngày cập nhật</b><b>Tác vụ</b></div>{visibleDocuments.map(item => <div className="document-row" key={item.documentId}><DocumentIcon item={item} /><span><b>{item.name}</b><small>{fileName(item)}</small></span><Tag color="blue">Loại {item.rawType}</Tag><span>{item.detail || 'Chưa có mô tả'}</span><span>{dayjs(item.updateDate).format('DD/MM/YYYY')}<small>{dayjs(item.updateDate).format('HH:mm')}</small></span>{item.safeUrl ? <a href={item.safeUrl} target="_blank" rel="noopener noreferrer"><EyeOutlined /> Xem</a> : <span className="document-unavailable">URL chưa an toàn</span>}</div>)}</div> : <div className="documents-empty"><div><FileTextOutlined /></div><section><h3>Môn học chưa có tài liệu được công bố</h3><p>Hiện tại chưa có tài liệu nào cho môn học này.<br />Khi có tài liệu mới, chúng tôi sẽ cập nhật tại đây.</p></section></div>}
        </Card>
      </main>
    </div>
  </div>
}

function fileName(item: StudentDocument) {
  if (!item.safeUrl) return 'Chưa có tệp an toàn'
  try { return decodeURIComponent(new URL(item.safeUrl).pathname.split('/').pop() || item.name) } catch { return item.name }
}

function DocumentIcon({ item }: { item: StudentDocument }) {
  const value = `${item.name} ${item.safeUrl ?? ''}`.toLowerCase()
  const Icon = value.includes('.pdf') ? FilePdfOutlined : value.match(/\.(xlsx?|csv)/) ? FileExcelOutlined : value.match(/\.(docx?)/) ? FileWordOutlined : value.match(/\.(pptx?)/) ? DownloadOutlined : FileTextOutlined
  return <span className="document-file-icon"><Icon /></span>
}
