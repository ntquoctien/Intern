let nextTemporaryProjectId = -1

export function createTemporaryProjectId() {
  return nextTemporaryProjectId--
}