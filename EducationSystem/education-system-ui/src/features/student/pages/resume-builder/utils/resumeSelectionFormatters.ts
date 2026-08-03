import type { AwardActivityItem, CertificationItem } from '../types'

function joinParts(parts: Array<string | null | undefined>, separator = ' - ') {
  return parts
    .map(part => part?.trim())
    .filter((part): part is string => Boolean(part))
    .join(separator)
}

export function formatCertificationLabel(certification: CertificationItem) {
  return joinParts([certification.name, certification.issuer])
}

export function formatCertificationPayload(certification: CertificationItem) {
  const details = [
    formatCertificationLabel(certification),
    certification.issueDate?.trim() ? `Issue: ${certification.issueDate.trim()}` : null,
    certification.expirationDate?.trim() ? `Expire: ${certification.expirationDate.trim()}` : null,
    certification.credentialUrl?.trim() ? `URL: ${certification.credentialUrl.trim()}` : null,
  ]
  return joinParts(details, ' | ')
}

export function formatAwardLabel(award: AwardActivityItem) {
  return joinParts([award.title, award.organization])
}

export function formatAwardPayload(award: AwardActivityItem) {
  const details = [
    formatAwardLabel(award),
    award.achievedDate?.trim() ? award.achievedDate.trim() : null,
    award.description?.trim() ? award.description.trim() : null,
  ]
  return joinParts(details, ' | ')
}