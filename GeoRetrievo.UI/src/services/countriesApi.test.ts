import { describe, expect, it } from 'vitest'
import { parseCountriesResponse, parseCountry } from './countriesApi'

describe('parseCountry', () => {
  it('normalizes required fields and supplies defaults for optional fields', () => {
    expect(parseCountry({ name: '  France ', code: ' fr ', population: '68000000' })).toEqual({
      name: 'France',
      code: 'FR',
      capital: '',
      region: 'Unknown',
      population: 68000000,
      currency: '',
      currencyCode: '',
      currencySymbol: '',
      languages: [],
      callingCode: '',
      flag: '',
    })

  })

  it('parses currency and language details returned by the API', () => {
    expect(parseCountry({
      name: 'France',
      code: 'FR',
      currencyCode: 'EUR',
      currencySymbol: '€',
      languages: ['French', 42],
      callingCode: '+33',
    })).toEqual(expect.objectContaining({
      currency: 'EUR',
      currencyCode: 'EUR',
      currencySymbol: '€',
      languages: ['French'],
      callingCode: '33',
    }))
  })

  it.each([
    null,
    undefined,
    {},
    { name: 'France' },
    { code: 'FR' },
    { name: '  ', code: 'FR' },
  ])('rejects invalid country data: %s', (data) => {
    expect(parseCountry(data)).toBeNull()
  })
})

describe('parseCountriesResponse', () => {
  it('keeps valid countries and skips invalid entries', () => {
    expect(parseCountriesResponse([
      { name: 'France', code: 'FR' },
      { name: '', code: 'XX' },
      { name: 'Japan', code: 'JP' },
    ])).toEqual([
      expect.objectContaining({ name: 'France', code: 'FR' }),
      expect.objectContaining({ name: 'Japan', code: 'JP' }),
    ])
  })

  it('throws when the API response is not an array', () => {
    expect(() => parseCountriesResponse({ countries: [] })).toThrow(
      'Invalid API response format: expected an array of countries',
    )
  })
})
