import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import SuuntoInlineLogin from './SuuntoInlineLogin'

// Mock the useIntegrationsAuth hook
const mockSetSuuntoKey = vi.fn()
const mockGetSuuntoKey = vi.fn()

vi.mock('../../hooks/useIntegrationsAuth', () => ({
  useIntegrationsAuth: () => ({
    getSuuntoKey: mockGetSuuntoKey,
    setSuuntoKey: mockSetSuuntoKey,
  }),
}))

describe('SuuntoInlineLogin', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockGetSuuntoKey.mockReturnValue('')
  })

  it('renders SSTAuthorization input field', () => {
    render(<SuuntoInlineLogin />)

    expect(screen.getByLabelText(/sstauthorization/i)).toBeInTheDocument()
  })

  it('renders connect button', () => {
    render(<SuuntoInlineLogin />)

    expect(screen.getByRole('button', { name: /connect/i })).toBeInTheDocument()
  })

  it('displays error when submitting empty value', () => {
    render(<SuuntoInlineLogin />)

    fireEvent.click(screen.getByRole('button', { name: /connect/i }))

    expect(screen.getByText(/sstauthorization is required/i)).toBeInTheDocument()
  })

  it('calls setSuuntoKey with trimmed value when form is valid', () => {
    render(<SuuntoInlineLogin />)

    const input = screen.getByLabelText(/sstauthorization/i)
    fireEvent.change(input, { target: { value: '  my-auth-key  ' } })
    fireEvent.click(screen.getByRole('button', { name: /connect/i }))

    expect(mockSetSuuntoKey).toHaveBeenCalledWith('my-auth-key')
  })

  it('clears error when form is resubmitted', () => {
    render(<SuuntoInlineLogin />)

    // First submit with error
    fireEvent.click(screen.getByRole('button', { name: /connect/i }))
    expect(screen.getByText(/sstauthorization is required/i)).toBeInTheDocument()

    // Then fix and resubmit
    const input = screen.getByLabelText(/sstauthorization/i)
    fireEvent.change(input, { target: { value: 'valid-key' } })
    fireEvent.click(screen.getByRole('button', { name: /connect/i }))

    expect(screen.queryByText(/sstauthorization is required/i)).not.toBeInTheDocument()
  })

  it('has card title "Connect Suunto"', () => {
    render(<SuuntoInlineLogin />)

    expect(screen.getByText('Connect Suunto')).toBeInTheDocument()
  })
})
