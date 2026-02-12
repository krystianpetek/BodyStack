import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import FitatuInlineLogin from './FitatuInlineLogin'

// Mock the fitatuLogin API
const mockFitatuLogin = vi.fn()
vi.mock('../../api/fitatuApi', () => ({
  fitatuLogin: (credentials: { username: string; password: string }) => mockFitatuLogin(credentials),
}))

describe('FitatuInlineLogin', () => {
  const defaultProps = {
    onLoggedIn: vi.fn(),
  }

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders username and password inputs', () => {
    render(<FitatuInlineLogin {...defaultProps} />)

    expect(screen.getByLabelText(/username/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument()
  })

  it('renders connect button', () => {
    render(<FitatuInlineLogin {...defaultProps} />)

    expect(screen.getByRole('button', { name: /connect/i })).toBeInTheDocument()
  })

  it('calls fitatuLogin and onLoggedIn on successful submit', async () => {
    mockFitatuLogin.mockResolvedValueOnce(undefined)
    const onLoggedIn = vi.fn()
    
    render(<FitatuInlineLogin onLoggedIn={onLoggedIn} />)

    fireEvent.change(screen.getByLabelText(/username/i), { target: { value: 'testuser' } })
    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'testpass' } })
    fireEvent.click(screen.getByRole('button', { name: /connect/i }))

    await waitFor(() => {
      expect(mockFitatuLogin).toHaveBeenCalledWith({ username: 'testuser', password: 'testpass' })
      expect(onLoggedIn).toHaveBeenCalled()
    })
  })

  it('displays error message on login failure', async () => {
    mockFitatuLogin.mockRejectedValueOnce(new Error('Invalid credentials'))
    
    render(<FitatuInlineLogin {...defaultProps} />)

    fireEvent.change(screen.getByLabelText(/username/i), { target: { value: 'testuser' } })
    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'wrongpass' } })
    fireEvent.click(screen.getByRole('button', { name: /connect/i }))

    await waitFor(() => {
      expect(screen.getByText(/invalid credentials/i)).toBeInTheDocument()
    })
  })

  it('has card title "Connect Fitatu"', () => {
    render(<FitatuInlineLogin {...defaultProps} />)

    expect(screen.getByText('Connect Fitatu')).toBeInTheDocument()
  })

  it('disables button while submitting', async () => {
    mockFitatuLogin.mockImplementation(() => new Promise(resolve => setTimeout(resolve, 100)))
    
    render(<FitatuInlineLogin {...defaultProps} />)

    fireEvent.change(screen.getByLabelText(/username/i), { target: { value: 'testuser' } })
    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'testpass' } })
    fireEvent.click(screen.getByRole('button', { name: /connect/i }))

    expect(screen.getByRole('button')).toBeDisabled()
    expect(screen.getByRole('button')).toHaveTextContent(/connecting/i)
  })
})
