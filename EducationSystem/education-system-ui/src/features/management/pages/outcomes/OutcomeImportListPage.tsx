import { useQuery } from '@tanstack/react-query'
import { FileAddOutlined, ReloadOutlined, SearchOutlined } from '@ant-design/icons'
import { Alert, Button, Card, DatePicker, Form, Input, Progress, Select, Space, Table, Typography, type TableColumnsType } from 'antd'
import dayjs from 'dayjs'
import { useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { OutcomeStatusTag } from './OutcomeStatusTag'
import { outcomeApi, problemMessage, type OutcomeImportListItem } from './outcomeApi'

const statusOptions = [
  ['Uploaded', 'Đã tải lên'], ['Processing', 'Đang xử lý'],
  ['PendingReview', 'Chờ kiểm duyệt'], ['ValidationFailed', 'Cần chỉnh sửa'],
  ['Approved', 'Đã duyệt'], ['Rejected', 'Từ chối'],
  ['Failed', 'Xử lý lỗi'], ['Archived', 'Đã lưu trữ'],
].map(([value, label]) => ({ value, label }))

export function OutcomeImportListPage() {
  const navigate = useNavigate()
  const [filters, setFilters] = useState<Record<string, string | number | undefined>>({ page: 1, pageSize: 20 })
  const curricula = useQuery({ queryKey: ['outcome-curricula'], queryFn: outcomeApi.curricula })
  const imports = useQuery({
    queryKey: ['outcome-imports', filters],
    queryFn: () => outcomeApi.list(filters),
    refetchInterval: query => query.state.data?.items.some(item => item.status === 'Uploaded' || item.status === 'Processing') ? 3000 : false,
  })
  const columns = useMemo<TableColumnsType<OutcomeImportListItem>>(() => [
    {
      title: 'Tệp / Chương trình', key: 'file', width: 280,
      render: (_, item) => <div className="outcome-primary-cell">
        <Button type="link" onClick={() => navigate(`/management/system/outcomes/${item.id}/review`)}>{item.originalFileName}</Button>
        {item.selectedSubjectCode && <small>{item.selectedSubjectCode} — {item.selectedSubjectName}</small>}
        <small>{item.majorCode} · {item.curriculumCode} · {item.version}</small>
      </div>,
    },
    { title: 'Trạng thái', dataIndex: 'status', width: 150, render: status => <OutcomeStatusTag status={status} /> },
    {
      title: 'Tiến độ', key: 'progress', width: 180,
      render: (_, item) => <div><Progress percent={item.progressPercent} size="small" status={item.status === 'Failed' ? 'exception' : undefined} /><small>{item.processingStage ?? '—'}</small></div>,
    },
    { title: 'Dữ liệu', key: 'counts', width: 170, render: (_, item) => <span>{item.ploCount} PLO · {item.cloCount} CLO · {item.mappingCount} liên kết</span> },
    { title: 'Người tải', dataIndex: 'uploadedByName', width: 180 },
    { title: 'Ngày tải', dataIndex: 'createdAt', width: 150, render: value => dayjs(value).format('DD/MM/YYYY HH:mm') },
    {
      title: '', key: 'actions', fixed: 'right', width: 110,
      render: (_, item) => <Button onClick={() => navigate(`/management/system/outcomes/${item.id}/review`)}>Mở</Button>,
    },
  ], [navigate])

  return <div className="outcome-page">
    <div className="outcome-page-heading">
      <div><Typography.Title level={3}>Import CLO/PLO</Typography.Title><Typography.Text type="secondary">Chuẩn hoá dữ liệu chuẩn đầu ra từ DOCX trước khi sử dụng cho bộ lọc CV.</Typography.Text></div>
      <Space><Button icon={<ReloadOutlined />} onClick={() => imports.refetch()}>Làm mới</Button><Button type="primary" icon={<FileAddOutlined />} onClick={() => navigate('/management/system/outcomes/import')}>Import tài liệu</Button></Space>
    </div>
    <Card>
      <Form layout="inline" className="outcome-filter-bar" onValuesChange={(_, values) => setFilters(current => ({
        ...current, page: 1, status: values.status, curriculumVersionId: values.curriculumVersionId,
        keyword: values.keyword || undefined,
        fromDate: values.range?.[0]?.format('YYYY-MM-DD'),
        toDate: values.range?.[1]?.format('YYYY-MM-DD'),
      }))}>
        <Form.Item name="keyword"><Input allowClear prefix={<SearchOutlined />} placeholder="Tên tệp hoặc mã CTĐT" /></Form.Item>
        <Form.Item name="status"><Select allowClear placeholder="Trạng thái" options={statusOptions} style={{ width: 170 }} /></Form.Item>
        <Form.Item name="curriculumVersionId"><Select allowClear loading={curricula.isLoading} placeholder="Chương trình" showSearch optionFilterProp="label" style={{ width: 260 }} options={curricula.data?.map(item => ({ value: item.id, label: `${item.majorCode} · ${item.curriculumCode} · ${item.version}` }))} /></Form.Item>
        <Form.Item name="range"><DatePicker.RangePicker format="DD/MM/YYYY" /></Form.Item>
      </Form>
      {imports.isError && <Alert className="outcome-alert" type="error" showIcon message="Không thể tải danh sách import" description={problemMessage(imports.error)} />}
      <Table rowKey="id" loading={imports.isLoading} columns={columns} dataSource={imports.data?.items ?? []} scroll={{ x: 1150 }} pagination={{
        current: imports.data?.page ?? 1, pageSize: imports.data?.pageSize ?? 20, total: imports.data?.totalItems ?? 0,
        showSizeChanger: true, onChange: (page, pageSize) => setFilters(current => ({ ...current, page, pageSize })),
      }} />
    </Card>
  </div>
}
