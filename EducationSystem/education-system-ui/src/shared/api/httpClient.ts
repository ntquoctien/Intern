import axios from 'axios'
import { serviceBaseUrls } from './serviceBaseUrls'
import type { ApiResponse, PagedResult, QueryParams, ServiceKey } from '../types/api'

export const httpClient = axios.create({
  timeout: 15_000,
})

function serviceUrl(service: ServiceKey, path: string) {
  return `${serviceBaseUrls[service]}/${path.replace(/^\/+/, '')}`
}

export async function getPaged<T>(
  service: ServiceKey,
  path: string,
  params: QueryParams,
) {
  const response = await httpClient.get<ApiResponse<PagedResult<T>>>(
    serviceUrl(service, path),
    { params },
  )
  return response.data
}

export async function getById<T>(service: ServiceKey, path: string, id: string) {
  const response = await httpClient.get<ApiResponse<T>>(serviceUrl(service, `${path}/${id}`))
  return response.data
}

export async function getLookup<T>(service: ServiceKey, path: string) {
  const response = await httpClient.get<ApiResponse<T[]>>(serviceUrl(service, `${path}/lookup`))
  return response.data
}
