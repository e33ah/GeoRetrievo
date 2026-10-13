import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { EmptyState } from './EmptyState'

describe('EmptyState', () => {
    it('shows the active filters and clears them when requested', async () => {
        const user = userEvent.setup()
        const onResetFilters = vi.fn()

        render(
            <EmptyState
                search="Atlantis"
                region="Europe"
                onResetFilters={onResetFilters}
            />,
        )

        expect(screen.getByText('"Atlantis"')).toBeInTheDocument()
        expect(screen.getByText(/in Europe/)).toBeInTheDocument()

        await user.click(screen.getByRole('button', { name: 'Clear all filters' }))

        expect(onResetFilters).toHaveBeenCalledTimes(1)
    })
})
