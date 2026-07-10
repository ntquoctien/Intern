import { LoadingOutlined } from '@ant-design/icons'
import { Select, Spin } from 'antd'
import { useEffect, useRef, useState } from 'react'
import { getLookup } from '../api/httpClient'
import { autocompleteSearchThresholds } from '../utils/filterLabels'
import type { LookupItem, ServiceKey } from '../types/api'

type SearchableSelectProps = {
  field: string
  placeholder: string
  value?: string
  onChange: (value: string | undefined) => void
  service: ServiceKey
  resourcePath: string
  minChars?: number
  debounceMs?: number
  maxResults?: number
  getLabel?: (item: LookupItem) => string
  style?: React.CSSProperties
}

export function SearchableSelect({
  placeholder,
  value,
  onChange,
  service,
  resourcePath,
  minChars = autocompleteSearchThresholds.minChars,
  debounceMs = autocompleteSearchThresholds.debounceMs,
  maxResults = autocompleteSearchThresholds.maxResults,
  getLabel,
  style,
}: SearchableSelectProps) {
  const [searchText, setSearchText] = useState('')
  const [options, setOptions] = useState<{ value: string; label: string }[]>([])
  const [loading, setLoading] = useState(false)
  const debounceTimer = useRef<ReturnType<typeof setTimeout> | null>(null)

  // Default label formatter
  const defaultGetLabel = (item: LookupItem): string => {
    const name = item.name ?? item.fullName ?? item.userName ?? item.nickname ?? item.code ?? item.subjectCode
    const secondary = item.userInternalId ?? item.subjectCode ?? item.code

    if (name && secondary && name !== secondary) {
      return `${String(name)} (${String(secondary)})`
    }

    return name ? String(name) : 'Record'
  }

  const labelFormatter = getLabel ?? defaultGetLabel

  // Fetch data when search text changes
  useEffect(() => {
    // Clear previous timer
    if (debounceTimer.current) {
      clearTimeout(debounceTimer.current)
    }

    // Don't search if text is too short
    if (searchText.trim().length < minChars) {
      setOptions([])
      return
    }

    setLoading(true)

    // Debounce the search
    debounceTimer.current = setTimeout(async () => {
      try {
        const response = await getLookup(service, resourcePath)
        const allItems = (response.data ?? []) as LookupItem[]

        // Simple client-side filter for now
        // In production, this could call a dedicated search endpoint
        const filtered = allItems
          .filter((item: LookupItem) => {
            const label = labelFormatter(item)
            return label.toLowerCase().includes(searchText.toLowerCase())
          })
          .slice(0, maxResults)

        setOptions(
          filtered.map((item: LookupItem) => ({
            value: String(item.id),
            label: labelFormatter(item),
          })),
        )
      } catch (error) {
        console.error(`Error fetching ${resourcePath}:`, error)
        setOptions([])
      } finally {
        setLoading(false)
      }
    }, debounceMs)

    return () => {
      if (debounceTimer.current) {
        clearTimeout(debounceTimer.current)
      }
    }
  }, [searchText, service, resourcePath, minChars, debounceMs, maxResults, labelFormatter])

  return (
    <Select
      allowClear
      showSearch
      placeholder={placeholder}
      style={style || { width: 260 }}
      value={value ?? undefined}
      options={options}
      onSearch={setSearchText}
      onChange={(newValue) => onChange(newValue ?? undefined)}
      notFoundContent={
        loading ? <Spin indicator={<LoadingOutlined style={{ fontSize: 14 }} spin />} /> : 'Không tìm thấy'
      }
      optionFilterProp="label"
    />
  )
}
