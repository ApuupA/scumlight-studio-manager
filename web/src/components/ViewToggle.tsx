import { Center, SegmentedControl, Tooltip } from '@mantine/core'
import { IconLayoutGrid, IconList } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import type { ViewMode } from './useViewMode'

/** The switch between list and tiles on an overview page (see useViewMode). */
export default function ViewToggle({ value, onChange }: { value: ViewMode; onChange: (mode: ViewMode) => void }) {
  const { t } = useTranslation()

  return (
    <SegmentedControl
      value={value}
      onChange={(mode) => onChange(mode as ViewMode)}
      data={[
        {
          value: 'list',
          label: <Tooltip label={t('view.list')}><Center><IconList size={16} /></Center></Tooltip>,
        },
        {
          value: 'tiles',
          label: <Tooltip label={t('view.tiles')}><Center><IconLayoutGrid size={16} /></Center></Tooltip>,
        },
      ]}
      aria-label={t('view.switch')}
    />
  )
}
