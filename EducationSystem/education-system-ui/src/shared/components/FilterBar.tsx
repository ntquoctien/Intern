import { ReloadOutlined, SearchOutlined } from '@ant-design/icons'
import { Button, DatePicker, Input, InputNumber } from 'antd'
import dayjs from 'dayjs'
import { filterLabels } from '../utils/filterLabels'
import { SearchableSelect } from './SearchableSelect'
import type { FilterField, RelationLookup } from './DataTablePage'
import type { QueryParams } from '../types/api'

type FilterBarProps = {
  // Search
  searchText: string
  searchPlaceholder: string
  onSearchChange: (text: string) => void
  onSearchSubmit: () => void

  // Filters
  filters: QueryParams
  onFilterChange: (key: string, value: unknown) => void
  filterFields?: FilterField[]
  relationLookups?: RelationLookup[]

  // Actions
  onReset: () => void
  onRefresh: () => void
  isLoading?: boolean
  isSearchable?: boolean
}

export function FilterBar({
  searchText,
  searchPlaceholder,
  onSearchChange,
  onSearchSubmit,
  filters,
  onFilterChange,
  filterFields = [],
  relationLookups = [],
  onReset,
  onRefresh,
  isLoading = false,
  isSearchable = true,
}: FilterBarProps) {
  const relationLookupMap = new Map(relationLookups.map((l) => [l.field, l]))

  function renderSearchableSelect(field: Exclude<FilterField, 'dateRange'>) {
    const lookup = relationLookupMap.get(field)

    if (!lookup?.filterable) {
      return null
    }

    const placeholder = filterLabels[field as keyof typeof filterLabels] || `Filter by ${field}`

    return (
      <SearchableSelect
        key={field}
        field={field}
        placeholder={placeholder}
        value={(filters[field] as string | undefined) ?? undefined}
        onChange={(value) => onFilterChange(field, value)}
        service={lookup.service}
        resourcePath={lookup.resourcePath}
        getLabel={lookup.getLabel}
        style={{ width: 260 }}
      />
    )
  }

  function renderStatusFilter() {
    if (!filterFields.includes('status')) {
      return null
    }

    return (
      <InputNumber
        key="status"
        placeholder={filterLabels.status}
        style={{ width: 140 }}
        value={(filters.status as number | undefined) ?? undefined}
        onChange={(value) => onFilterChange('status', value ?? undefined)}
      />
    )
  }

  function renderDateRangeFilter() {
    if (!filterFields.includes('dateRange')) {
      return null
    }

    const fromDate = filters.fromDate ? dayjs(filters.fromDate as string) : null
    const toDate = filters.toDate ? dayjs(filters.toDate as string) : null

    return (
      <DatePicker.RangePicker
        key="dateRange"
        value={fromDate && toDate ? [fromDate, toDate] : null}
        onChange={(value) => {
          onFilterChange('fromDate', value?.[0] ? dayjs(value[0]).startOf('day').toISOString() : undefined)
          onFilterChange('toDate', value?.[1] ? dayjs(value[1]).endOf('day').toISOString() : undefined)
        }}
      />
    )
  }

  return (
    <div className="filter-bar-container">
      {/* First row: Search and Filters */}
      <div className="filter-bar-row">
        {isSearchable && (
          <Input
            allowClear
            prefix={<SearchOutlined />}
            placeholder={searchPlaceholder}
            className="filter-search-input"
            value={searchText}
            onChange={(event) => onSearchChange(event.target.value)}
            onPressEnter={onSearchSubmit}
          />
        )}

        {/* Relation filters */}
        {filterFields.includes('studentId') && renderSearchableSelect('studentId')}
        {filterFields.includes('subjectTeachingId') && renderSearchableSelect('subjectTeachingId')}
        {filterFields.includes('subjectScheduleId') && renderSearchableSelect('subjectScheduleId')}
        {filterFields.includes('subjectTeachingExamId') && renderSearchableSelect('subjectTeachingExamId')}
        {filterFields.includes('userId') && renderSearchableSelect('userId')}

        {/* Status filter */}
        {renderStatusFilter()}

        {/* Date range filter */}
        {renderDateRangeFilter()}
      </div>

      {/* Second row: Action buttons */}
      <div className="filter-bar-actions">
        {isSearchable && (
          <Button type="primary" onClick={onSearchSubmit}>
            Tìm kiếm
          </Button>
        )}
        <Button onClick={onReset}>Đặt lại</Button>
        <Button icon={<ReloadOutlined />} loading={isLoading} onClick={onRefresh}>
          Tải lại
        </Button>
      </div>
    </div>
  )
}
