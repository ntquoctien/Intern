export type ApiResponse<T> = {
  success: boolean
  message: string
  data: T
}

export type PagedResult<T> = {
  items: T[]
  pageNumber: number
  pageSize: number
  totalItems: number
  totalPages: number
}

export type QueryParams = {
  pageNumber?: number
  pageSize?: number
  search?: string
  sortBy?: string
  sortDirection?: 'asc' | 'desc'
  studentId?: string
  subjectScheduleId?: string
  subjectTeachingId?: string
  subjectTeachingExamId?: string
  userId?: string
  status?: number
  fromDate?: string
  toDate?: string
}

export type ServiceKey = 'academic' | 'exam' | 'identity' | 'communication'

export type RecordItem = Record<string, unknown> & {
  id: string
}

export type LookupItem = Record<string, unknown> & {
  id: string
}
