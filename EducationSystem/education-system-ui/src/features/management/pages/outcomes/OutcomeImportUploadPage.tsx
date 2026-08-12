import { useMemo, useState } from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { ArrowLeftOutlined, FileTextOutlined, InboxOutlined } from '@ant-design/icons'
import { Alert, Button, Card, Input, Progress, Result, Select, Space, Steps, Typography, Upload, message } from 'antd'
import { useNavigate } from 'react-router-dom'
import { managementApi } from '../../managementApi'
import { OutcomeStatusTag } from './OutcomeStatusTag'
import { duplicateImport, outcomeApi, problemMessage, type DuplicateImport, type ImportDetail } from './outcomeApi'

const docxMime = 'application/vnd.openxmlformats-officedocument.wordprocessingml.document'
const pdfMime = 'application/pdf'
const genericMime = 'application/octet-stream'

function hasExpectedMime(actual: string, expected: string) {
  return !actual || actual === expected || actual === genericMime
}

function isSupportedDocument(file: File) {
  const name = file.name.toLowerCase()
  return (name.endsWith('.docx') && hasExpectedMime(file.type, docxMime)) ||
    (name.endsWith('.pdf') && hasExpectedMime(file.type, pdfMime))
}

function normalizeSearch(value: string) {
  return value
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .trim()
    .toLocaleLowerCase('vi')
}

export function OutcomeImportUploadPage() {
  const navigate = useNavigate()
  const [majorId, setMajorId] = useState<string>()
  const [subjectId, setSubjectId] = useState<string>()
  const [subjectSearch, setSubjectSearch] = useState('')
  const [version, setVersion] = useState('')
  const [file, setFile] = useState<File>()
  const [batchId, setBatchId] = useState<number>()
  const [existingImport, setExistingImport] = useState<DuplicateImport>()

  const majors = useQuery({ queryKey: ['outcome-major-options'], queryFn: outcomeApi.majors })
  const plans = useQuery({ queryKey: ['management-plans', 'outcome-import'], queryFn: managementApi.plans })
  const subjects = useQuery({
    queryKey: ['academic-subject-lookup'],
    queryFn: outcomeApi.subjects,
    staleTime: 5 * 60 * 1000,
  })
  const curricula = useQuery({ queryKey: ['outcome-curricula'], queryFn: outcomeApi.curricula })

  const selectedMajor = majors.data?.find(item => item.id === majorId)
  const selectedSubject = subjects.data?.find(item => item.subjectId === subjectId)
  const majorOptions = useMemo(() => {
    const seen = new Set<string>()
    return (majors.data ?? [])
      .filter(item => {
        const key = `${item.code.trim().toLocaleLowerCase('vi')}|${item.name.trim().toLocaleLowerCase('vi')}`
        if (seen.has(key)) return false
        seen.add(key)
        return true
      })
      .map(item => ({
        value: item.id,
        label: `${item.code} — ${item.name}`,
      }))
  }, [majors.data])

  const subjectOptions = useMemo(() => {
    if (!selectedMajor || !subjects.data) return []

    const subjectIdsInMajor = new Set(
      (plans.data ?? [])
        .filter(plan => plan.majorCode.localeCompare(selectedMajor.code, undefined, { sensitivity: 'accent' }) === 0)
        .flatMap(plan => plan.subjectIds ?? []),
    )
    const query = normalizeSearch(subjectSearch)
    const candidates = query
      ? subjects.data.filter(subject => {
          const searchable = normalizeSearch(`${subject.subjectCode} ${subject.name}`)
          return searchable.includes(query)
        })
      : subjects.data.filter(subject => subjectIdsInMajor.has(subject.subjectId))

    const visible = selectedSubject && !candidates.some(item => item.subjectId === selectedSubject.subjectId)
      ? [selectedSubject, ...candidates]
      : candidates
    return visible
      .sort((left, right) => left.subjectCode.localeCompare(right.subjectCode))
      .map(subject => ({
        value: subject.subjectId,
        label: `${subject.subjectCode} — ${subject.name}`,
      }))
  }, [plans.data, selectedMajor, selectedSubject, subjectSearch, subjects.data])

  const detail = useQuery<ImportDetail>({
    queryKey: ['outcome-import-detail', batchId],
    queryFn: () => outcomeApi.detail(batchId!),
    enabled: !!batchId,
    refetchInterval: query =>
      ['Uploaded', 'Processing'].includes(query.state.data?.status ?? '') ? 2000 : false,
  })

  const upload = useMutation({
    mutationFn: async () => {
      if (!selectedMajor || !selectedSubject || !file || !version.trim())
        throw new Error('Vui lòng chọn đủ ngành, học phần, phiên bản và tệp DOCX/PDF.')

      const normalizedVersion = version.trim()
      let curriculum = curricula.data?.find(item =>
        item.majorCode.localeCompare(selectedMajor.code, undefined, { sensitivity: 'accent' }) === 0 &&
        item.version.localeCompare(normalizedVersion, undefined, { sensitivity: 'accent' }) === 0)

      if (!curriculum) {
        curriculum = await outcomeApi.createCurriculum({
          majorExternalId: selectedMajor.id,
          majorCode: selectedMajor.code,
          majorName: selectedMajor.name,
          curriculumCode: selectedMajor.code,
          curriculumName: `Chương trình đào tạo ${selectedMajor.name}`,
          version: normalizedVersion,
        })
        await curricula.refetch()
      }

      return outcomeApi.upload(curriculum.id, selectedSubject, file)
    },
    onSuccess: result => {
      setExistingImport(undefined)
      setBatchId(result.id)
    },
    onError: error => {
      const duplicate = duplicateImport(error)
      if (duplicate) {
        setExistingImport(duplicate)
        return
      }
      message.error(problemMessage(error))
    },
  })

  const current = !batchId
    ? (file ? 1 : 0)
    : ['Uploaded', 'Processing'].includes(detail.data?.status ?? '') ? 2 : 3
  const readyForReview =
    detail.data && ['PendingReview', 'ValidationFailed'].includes(detail.data.status)
  const academicLookupFailed = majors.isError || plans.isError || subjects.isError

  return <div className="outcome-page outcome-upload-page">
    <Button icon={<ArrowLeftOutlined />} onClick={() => navigate('/management/system/outcomes')}>
      Danh sách import
    </Button>
    <div className="outcome-page-heading">
      <div>
        <Typography.Title level={3}>Import tài liệu CLO/PLO</Typography.Title>
        <Typography.Text type="secondary">
          Chọn ngành, học phần và nhập phiên bản trước khi tải tài liệu lên phân tích.
        </Typography.Text>
      </div>
    </div>
    <Card>
      <Steps current={current} items={[
        { title: 'Chọn ngữ cảnh' },
        { title: 'Chọn tài liệu' },
        { title: 'LLM phân tích' },
        { title: 'Kiểm duyệt' },
      ]} />

      {!batchId ? <div className="outcome-upload-form">
        {existingImport && <Alert
          type="warning"
          showIcon
          title="Tài liệu này đã được import"
          description={`Đã tìm thấy bản import #${existingImport.id} (${existingImport.status}). Bạn có thể mở bản hiện có thay vì tạo dữ liệu trùng.`}
          action={<Button onClick={() => {
            if (['Uploaded', 'Processing'].includes(existingImport.status)) {
              setBatchId(existingImport.id)
              return
            }
            navigate(`/management/system/outcomes/${existingImport.id}/review`)
          }}>Mở bản import hiện có</Button>}
        />}
        {academicLookupFailed && <Alert
          type="error"
          showIcon
          message="Không thể tải dữ liệu đào tạo"
          description="Hãy kiểm tra AcademicService rồi tải lại trang."
        />}

        <label>
          <b>Ngành</b>
          <Select
            value={majorId}
            loading={majors.isLoading}
            showSearch
            optionFilterProp="label"
            placeholder="Chọn mã ngành hoặc tên ngành"
            options={majorOptions}
            onChange={value => {
              setMajorId(value)
              setSubjectId(undefined)
              setSubjectSearch('')
            }}
          />
        </label>

        <label>
          <b>Học phần</b>
          <Select
            value={subjectId}
            disabled={!majorId}
            loading={subjects.isLoading || plans.isLoading}
            showSearch
            filterOption={false}
            onSearch={setSubjectSearch}
            onChange={(value: string) => {
              setSubjectId(value)
              setSubjectSearch('')
            }}
            onClear={() => {
              setSubjectId(undefined)
              setSubjectSearch('')
            }}
            allowClear
            placeholder="Chọn trong ngành hoặc nhập mã/tên học phần để tìm"
            notFoundContent={subjectSearch
              ? 'Không tìm thấy học phần phù hợp'
              : 'Ngành chưa có học phần trong chương trình đào tạo'}
            options={subjectOptions}
          />
          <Typography.Text type="secondary">
            Mặc định hiển thị học phần thuộc ngành. Khi nhập từ khóa, hệ thống tìm theo mã hoặc tên trong danh mục học phần.
          </Typography.Text>
        </label>

        <label>
          <b>Phiên bản</b>
          <Input
            value={version}
            maxLength={50}
            onChange={event => setVersion(event.target.value)}
            placeholder="Ví dụ: 2026, K48 hoặc 1.0"
          />
        </label>

        <Upload.Dragger
          accept=".docx,.pdf"
          maxCount={1}
          beforeUpload={selected => {
            if (!isSupportedDocument(selected)) {
              message.error('Chỉ chấp nhận tệp DOCX hoặc PDF hợp lệ.')
              return Upload.LIST_IGNORE
            }
            if (selected.size > 10 * 1024 * 1024) {
              message.error('Tệp không được lớn hơn 10 MB.')
              return Upload.LIST_IGNORE
            }
            setFile(selected)
            setExistingImport(undefined)
            return false
          }}
          onRemove={() => {
            setFile(undefined)
            setExistingImport(undefined)
            return true
          }}
        >
          <p className="ant-upload-drag-icon"><InboxOutlined /></p>
          <p className="ant-upload-text">Kéo thả hoặc chọn tệp CLO/PLO dạng DOCX hoặc PDF</p>
          <p className="ant-upload-hint">Tối đa 10 MB. PDF sẽ được OCR khi cần; hệ thống lưu bản gốc để đối chiếu nguồn.</p>
        </Upload.Dragger>

        <Button
          type="primary"
          size="large"
          icon={<FileTextOutlined />}
          disabled={!selectedMajor || !selectedSubject || !version.trim() || !file}
          loading={upload.isPending}
          onClick={() => upload.mutate()}
        >
          Tải lên và phân tích
        </Button>
      </div> : <div className="outcome-processing">
        {detail.isError ? <Alert
          type="error"
          showIcon
          message="Không thể đọc trạng thái xử lý"
          description={problemMessage(detail.error)}
        /> : detail.data?.status === 'Failed' ? <Result
          status="error"
          title="Phân tích không thành công"
          subTitle={detail.data.errorMessage ?? detail.data.errorCode}
          extra={<Space>
            <Button onClick={() => navigate('/management/system/outcomes')}>Về danh sách</Button>
            {detail.data.isRetryable && <Button
              type="primary"
              onClick={() => outcomeApi.process(batchId).then(() => detail.refetch())}
            >
              Thử lại
            </Button>}
          </Space>}
        /> : <>
          <OutcomeStatusTag status={detail.data?.status ?? 'Uploaded'} />
          <Progress type="circle" percent={detail.data?.progressPercent ?? 0} />
          <Typography.Title level={4}>
            {detail.data?.processingStage ?? 'Đang chờ worker'}
          </Typography.Title>
          <Typography.Text type="secondary">
            Bạn có thể rời trang; quá trình vẫn tiếp tục ở nền.
          </Typography.Text>
          {readyForReview && <Button
            type="primary"
            size="large"
            onClick={() => navigate(`/management/system/outcomes/${batchId}/review`)}
          >
            Mở màn hình kiểm duyệt
          </Button>}
          {detail.data && ['Approved', 'Rejected', 'Archived'].includes(detail.data.status) &&
            <Button onClick={() => navigate(`/management/system/outcomes/${batchId}/review`)}>
              Xem chi tiết
            </Button>}
        </>}
      </div>}
    </Card>
  </div>
}
