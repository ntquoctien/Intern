import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, useLocation } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import { Sidebar } from './Sidebar'

function CurrentPath() {
  return <output data-testid="current-path">{useLocation().pathname}</output>
}

describe('Sidebar', () => {
  it('opens the management form requests page from the admin menu', async () => {
    const user = userEvent.setup()
    render(
      <MemoryRouter initialEntries={['/management/overview']}>
        <Sidebar />
        <CurrentPath />
      </MemoryRouter>,
    )

    await user.click(screen.getByText('Liên lạc & Yêu cầu'))
    await user.click(screen.getByText('Yêu cầu biểu mẫu'))

    expect(screen.getByTestId('current-path')).toHaveTextContent('/management/forms/requests')
  })

  it('opens CLO/PLO management from the system menu', async () => {
    const user = userEvent.setup()
    render(
      <MemoryRouter initialEntries={['/management/overview']}>
        <Sidebar />
        <CurrentPath />
      </MemoryRouter>,
    )

    await user.click(screen.getByText('Hệ thống'))
    await user.click(screen.getByText('Chuẩn đầu ra CLO/PLO'))

    expect(screen.getByTestId('current-path')).toHaveTextContent('/management/system/outcomes')
  })
})
