/**
 * Constant ID for the printable resume element
 */
export const PRINTABLE_RESUME_ID = 'printable-resume'

/**
 * Handles printing the A4 resume to PDF or physical printer.
 * Scales the content to fit on a single A4 page.
 */
export function printA4Resume() {
  const paper = document.getElementById(PRINTABLE_RESUME_ID)
  if (!paper) {
    console.warn(`Element with id "${PRINTABLE_RESUME_ID}" not found`)
    return
  }

  // Fit user-edited content to one physical A4 page. scrollHeight includes any
  // content currently clipped by the screen preview's fixed A4 aspect ratio.
  const fitRatio = Math.min(1, paper.clientHeight / Math.max(paper.scrollHeight, 1))
  paper.style.setProperty('--print-content-scale', fitRatio.toFixed(4))

  // Clean up the print scale after printing
  window.addEventListener(
    'afterprint',
    () => paper.style.removeProperty('--print-content-scale'),
    { once: true },
  )

  window.print()
}
