import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createRef, useState } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { ControlPanel } from './ControlPanel'
import type { ComponentProps } from 'react'

function StatefulControlPanel(props: ComponentProps<typeof ControlPanel>) {
  const [search, setSearch] = useState(props.search)

  return (
    <ControlPanel
      {...props}
      search={search}
      onSearchChange={(value) => {
        props.onSearchChange(value)
        setSearch(value)
      }}
    />
  )
}

function renderControlPanel({
  search = '',
  selectedRegion = 'All',
}: {
  search?: string
  selectedRegion?: string
} = {}) {
  const onSearchChange = vi.fn()
  const onClearSearch = vi.fn()
  const onSortChange = vi.fn()
  const onRegionChange = vi.fn()

  render(
    <StatefulControlPanel
      search={search}
      onSearchChange={onSearchChange}
      onClearSearch={onClearSearch}
      searchInputRef={createRef<HTMLInputElement>()}
      sortBy="name-asc"
      onSortChange={onSortChange}
      regions={['All', 'Europe', 'Asia', 'North America', 'South America']}
      selectedRegion={selectedRegion}
      onRegionChange={onRegionChange}
    />,
  )

  return { onSearchChange, onClearSearch, onSortChange, onRegionChange }
}

describe('ControlPanel', () => {
  it('reports the text entered into the search field', async () => {
    const user = userEvent.setup()
    const { onSearchChange } = renderControlPanel()

    await user.type(screen.getByRole('searchbox', { name: 'Search countries' }), 'France')

    expect(onSearchChange).toHaveBeenLastCalledWith('France')
  })

  it('clears an existing search when the clear button is clicked', async () => {
    const user = userEvent.setup()
    const { onClearSearch } = renderControlPanel({ search: 'France' })

    await user.click(screen.getByRole('button', { name: 'Clear search query' }))

    expect(onClearSearch).toHaveBeenCalledTimes(1)
  })

  it('reports selected sort and region changes', async () => {
    const user = userEvent.setup()
    const { onSortChange, onRegionChange } = renderControlPanel()

    await user.selectOptions(screen.getByRole('combobox', { name: 'Sort countries by' }), 'pop-desc')
    await user.click(screen.getByRole('button', { name: 'Europe' }))

    expect(onSortChange).toHaveBeenCalledWith('pop-desc')
    expect(onRegionChange).toHaveBeenCalledWith('Europe')
  })

  it('marks the currently selected region as pressed', () => {
    renderControlPanel({ selectedRegion: 'Europe' })

    expect(screen.getByRole('button', { name: 'Europe' })).toHaveAttribute('aria-pressed', 'true')
    expect(screen.getByRole('button', { name: 'Asia' })).toHaveAttribute('aria-pressed', 'false')
  })

  it('renders North America and South America as selectable regions', async () => {
    const user = userEvent.setup()
    const { onRegionChange } = renderControlPanel()

    await user.click(screen.getByRole('button', { name: 'North America' }))
    await user.click(screen.getByRole('button', { name: 'South America' }))

    expect(onRegionChange).toHaveBeenNthCalledWith(1, 'North America')
    expect(onRegionChange).toHaveBeenNthCalledWith(2, 'South America')
  })
})
