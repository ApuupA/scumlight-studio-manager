import { Select } from '@mantine/core'
import { useTranslation } from 'react-i18next'

/** The years to choose from: next year back to 2020 (the studio's bookings start there at the earliest). */
function yearOptions(): string[] {
  const years: string[] = []
  for (let year = new Date().getFullYear() + 1; year >= 2020; year--) {
    years.push(String(year))
  }
  return years
}

/**
 * Year picker for the expense list and the yearly overview.
 */
export default function YearSelect({ value, onChange }: { value: number; onChange: (year: number) => void }) {
  const { t } = useTranslation()

  return (
    <Select
      aria-label={t('expenses.year')}
      w={110}
      data={yearOptions()}
      value={String(value)}
      onChange={(year) => year && onChange(Number(year))}
      allowDeselect={false}
    />
  )
}
