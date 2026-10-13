import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { ErrorState } from './ErrorState'

describe('ErrorState', () => {
    it('shows the error message and retries when requested', async () => {
        const user = userEvent.setup()
        const onRetry = vi.fn()
        render(
            <ErrorState
                error="An error occurred"
                onRetry={onRetry}
            />
        )
        expect(screen.getByRole('alert')).toHaveTextContent('An error occurred')
        await user.click(screen.getByRole('button', { name: 'Retry Connection' }))
        expect(onRetry).toHaveBeenCalledTimes(1)
    })
})