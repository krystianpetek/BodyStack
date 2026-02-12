import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import IntegrationSelector from './IntegrationSelector'
import type { IntegrationKey } from './IntegrationSelector'

describe('IntegrationSelector', () => {
  it('renders all integration tabs', () => {
    const onChange = vi.fn()
    render(<IntegrationSelector activeIntegration="fitatu" onIntegrationChange={onChange} />)

    expect(screen.getByRole('button', { name: 'Fitatu' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Suunto' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Template' })).toBeInTheDocument()
  })

  it('marks active integration as selected', () => {
    const onChange = vi.fn()
    render(<IntegrationSelector activeIntegration="fitatu" onIntegrationChange={onChange} />)

    const fitatuButton = screen.getByRole('button', { name: 'Fitatu' })
    const suuntoButton = screen.getByRole('button', { name: 'Suunto' })

    // Fitatu should have active styling (bg-slate-900)
    expect(fitatuButton.className).toContain('bg-slate-900')
    // Suunto should not
    expect(suuntoButton.className).toContain('border')
    expect(suuntoButton.className).not.toContain('bg-slate-900')
  })

  it('calls onIntegrationChange when tab clicked', () => {
    const onChange = vi.fn()
    render(<IntegrationSelector activeIntegration="fitatu" onIntegrationChange={onChange} />)

    fireEvent.click(screen.getByRole('button', { name: 'Suunto' }))

    expect(onChange).toHaveBeenCalledWith('suunto')
  })

  it('renders correct styling for each integration', () => {
    const integrations: IntegrationKey[] = ['fitatu', 'suunto', 'template']

    integrations.forEach((integration) => {
      const onChange = vi.fn()
      const { unmount } = render(
        <IntegrationSelector activeIntegration={integration} onIntegrationChange={onChange} />
      )

      const activeButton = screen.getByRole('button', { name: integration.charAt(0).toUpperCase() + integration.slice(1) })
      expect(activeButton.className).toContain('bg-slate-900')

      unmount()
    })
  })
})
