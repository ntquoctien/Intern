import { ReloadOutlined, SearchOutlined } from '@ant-design/icons'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import {
  Alert,
  Button,
  DatePicker,
  Empty,
  Input,
  InputNumber,
  Table,
  Typography,
  message,
} from 'antd'
import type { ColumnsType, TablePaginationConfig } from 'antd/es/table'
import type { SorterResult } from 'antd/es/table/interface'
import dayjs from 'dayjs'
import { useEffect, useMemo, useState } from 'react'
import { getById, getPaged } from '../api/httpClient'
import type { QueryParams, RecordItem, ServiceKey } from '../types/api'
import { DetailDrawer } from './DetailDrawer'
import { PageHeader } from './PageHeader'

export type FilterField =
  | 'studentId'
  | 'subjectScheduleId'
  | 'subjectTeachingId'
  | 'subjectTeachingExamId'
  | 'userId'
  | 'status'
  | 'dateRange'

type DataTablePageProps<T extends RecordItem> = {
  title: string
  description: string
  service: ServiceKey
  resourcePath: string
  columns: ColumnsType<T>
  filterFields?: FilterField[]
  searchPlaceholder?: string
  searchHelp?: string
  searchable?: boolean
}

export function DataTablePage<T extends RecordItem>({
  title,
  description,
  service,
  resourcePath,
  columns,
  filterFields = [],
  searchPlaceholder = 'Search by text',
  searchHelp,
  searchable = true,
}: DataTablePageProps<T>) {
  const [pageNumber, setPageNumber] = useState(1)
  const [pageSize, setPageSize] = useState(20)
  const [searchText, setSearchText] = useState('')
  const [search, setSearch] = useState('')
  const [sortBy, setSortBy] = useState<string>()
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc')
  const [filters, setFilters] = useState<QueryParams>({})
  const [selectedId, setSelectedId] = useState<string>()
  const queryClient = useQueryClient()

  const params = useMemo<QueryParams>(
    () => ({
      ...filters,
      pageNumber,
      pageSize,
      search: search || undefined,
      sortBy,
      sortDirection,
    }),
    [filters, pageNumber, pageSize, search, sortBy, sortDirection],
  )

  const listQuery = useQuery({
    queryKey: ['paged', service, resourcePath, params],
    queryFn: () => getPaged<T>(service, resourcePath, params),
  })

  const detailQuery = useQuery({
    queryKey: ['detail', service, resourcePath, selectedId],
    queryFn: () => getById<T>(service, resourcePath, selectedId!),
    enabled: Boolean(selectedId),
  })

  const data = listQuery.data?.data
  const tableColumns = useMemo<ColumnsType<T>>(
    () => [
      ...columns,
      {
        title: 'Action',
        key: 'action',
        width: 96,
        fixed: 'right',
        render: (_, record) => (
          <Button type="link" onClick={() => setSelectedId(record.id)}>
            Detail
          </Button>
        ),
      },
    ],
    [columns],
  )

  function handleTableChange(
    pagination: TablePaginationConfig,
    _filters: Record<string, unknown>,
    sorter: SorterResult<T> | SorterResult<T>[],
  ) {
    setPageNumber(pagination.current ?? 1)
    setPageSize(pagination.pageSize ?? 20)

    const activeSorter = Array.isArray(sorter) ? sorter[0] : sorter
    if (activeSorter?.field && activeSorter.order) {
      setSortBy(String(activeSorter.field))
      setSortDirection(activeSorter.order === 'descend' ? 'desc' : 'asc')
    } else {
      setSortBy(undefined)
      setSortDirection('asc')
    }
  }

  function updateFilter(key: keyof QueryParams, value: unknown) {
    setPageNumber(1)
    setFilters((current) => ({
      ...current,
      [key]: value || undefined,
    }))
  }

  useEffect(() => {
    const timeout = window.setTimeout(() => {
      setPageNumber(1)
      setSearch(searchText.trim())
    }, 350)

    return () => window.clearTimeout(timeout)
  }, [searchText])

  async function refreshData() {
    await queryClient.invalidateQueries({
      queryKey: ['paged', service, resourcePath],
    })
    await listQuery.refetch()
    message.success('Data refreshed')
  }

  function applySearch() {
    setPageNumber(1)
    setSearch(searchText.trim())
  }

  function resetSearchAndFilters() {
    setSearchText('')
    setSearch('')
    setFilters({})
    setSortBy(undefined)
    setSortDirection('asc')
    setPageNumber(1)
  }

  return (
    <div className="page-panel">
      <div className="page-toolbar">
        <PageHeader title={title} description={description} />
        <div className="page-actions">
          {searchable ? (
            <>
              <Input
                allowClear
                prefix={<SearchOutlined />}
                placeholder={searchPlaceholder}
                style={{ width: 320 }}
                value={searchText}
                onChange={(event) => setSearchText(event.target.value)}
                onPressEnter={applySearch}
              />
              <Button type="primary" onClick={applySearch}>
                Search
              </Button>
            </>
          ) : null}
          <Button onClick={resetSearchAndFilters}>Reset</Button>
          <Button icon={<ReloadOutlined />} loading={listQuery.isFetching} onClick={refreshData}>
            Refresh
          </Button>
        </div>
      </div>

      <Typography.Text className="muted" style={{ display: 'block', marginBottom: 12 }}>
        Total rows: {data?.totalItems ?? 0}
        {search ? ` | Search: "${search}"` : ''}
      </Typography.Text>
      {searchable && searchHelp ? (
        <Typography.Text className="muted" style={{ display: 'block', marginBottom: 12 }}>
          Text search fields: {searchHelp}
        </Typography.Text>
      ) : null}
      {!searchable ? (
        <Typography.Text className="muted" style={{ display: 'block', marginBottom: 12 }}>
          This page has no text search. Use exact filters below.
        </Typography.Text>
      ) : null}

      {filterFields.length > 0 ? (
        <div className="filter-row">
          {filterFields.includes('studentId') ? (
            <Input
              allowClear
              placeholder="Exact studentId"
              style={{ width: 260 }}
              onChange={(event) => updateFilter('studentId', event.target.value)}
            />
          ) : null}
          {filterFields.includes('subjectScheduleId') ? (
            <Input
              allowClear
              placeholder="Exact subjectScheduleId"
              style={{ width: 260 }}
              onChange={(event) => updateFilter('subjectScheduleId', event.target.value)}
            />
          ) : null}
          {filterFields.includes('subjectTeachingId') ? (
            <Input
              allowClear
              placeholder="Exact subjectTeachingId"
              style={{ width: 260 }}
              onChange={(event) => updateFilter('subjectTeachingId', event.target.value)}
            />
          ) : null}
          {filterFields.includes('subjectTeachingExamId') ? (
            <Input
              allowClear
              placeholder="Exact subjectTeachingExamId"
              style={{ width: 260 }}
              onChange={(event) =>
                updateFilter('subjectTeachingExamId', event.target.value)
              }
            />
          ) : null}
          {filterFields.includes('userId') ? (
            <Input
              allowClear
              placeholder="Exact userId"
              style={{ width: 260 }}
              onChange={(event) => updateFilter('userId', event.target.value)}
            />
          ) : null}
          {filterFields.includes('status') ? (
            <InputNumber
              placeholder="Exact status"
              style={{ width: 140 }}
              onChange={(value) => updateFilter('status', value ?? undefined)}
            />
          ) : null}
          {filterFields.includes('dateRange') ? (
            <DatePicker.RangePicker
              onChange={(value) => {
                setPageNumber(1)
                setFilters((current) => ({
                  ...current,
                  fromDate: value?.[0] ? dayjs(value[0]).startOf('day').toISOString() : undefined,
                  toDate: value?.[1] ? dayjs(value[1]).endOf('day').toISOString() : undefined,
                }))
              }}
            />
          ) : null}
        </div>
      ) : null}

      {listQuery.isError ? (
        <Alert
          showIcon
          type="error"
          message="Unable to load data"
          description={
            (listQuery.error as Error).message === 'Network Error'
              ? 'Network Error. Make sure all backend services are running on ports 5001, 5002, 5003, and 5004.'
              : (listQuery.error as Error).message
          }
          style={{ marginBottom: 16 }}
        />
      ) : null}

      <Table<T>
        columns={tableColumns}
        dataSource={data?.items ?? []}
        loading={listQuery.isLoading || listQuery.isFetching}
        locale={{
          emptyText: listQuery.isLoading ? <Typography.Text>Loading</Typography.Text> : <Empty />,
        }}
        pagination={{
          current: data?.pageNumber ?? pageNumber,
          pageSize: data?.pageSize ?? pageSize,
          total: data?.totalItems ?? 0,
          showSizeChanger: true,
        }}
        rowKey="id"
        scroll={{ x: 'max-content' }}
        size="middle"
        onChange={handleTableChange}
      />

      <DetailDrawer<T>
        title={`${title} detail`}
        open={Boolean(selectedId)}
        loading={detailQuery.isLoading || detailQuery.isFetching}
        record={detailQuery.data?.data}
        onClose={() => setSelectedId(undefined)}
      />
    </div>
  )
}
