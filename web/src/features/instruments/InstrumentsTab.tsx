import { Button, Group, Stack, Text } from '@mantine/core'
import { IconPlus, IconTemplate } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { Project } from '../../api/types'
import InstrumentCard from './InstrumentCard'
import InstrumentFormModal from './InstrumentFormModal'
import { useProjectInstruments } from './instrumentApi'
import TemplatesModal from './TemplatesModal'

/**
 * Project tab "Instruments & setup": the recorded instruments and the gear used for each of them.
 * Sessions choose instruments and thereby get their gear.
 */
export default function InstrumentsTab({ project }: { project: Project }) {
  const { t } = useTranslation()
  const { data: instruments } = useProjectInstruments(project.id)
  const [dialog, setDialog] = useState<'none' | 'instrument' | 'templates'>('none')

  return (
    <Stack>
      <Group justify="space-between">
        <Text size="sm" c="dimmed" maw={600}>{t('instruments.hint')}</Text>
        <Group gap="xs">
          <Button variant="default" leftSection={<IconTemplate size={16} />} onClick={() => setDialog('templates')}>
            {t('templates.fromTemplate')}
          </Button>
          <Button variant="light" leftSection={<IconPlus size={16} />} onClick={() => setDialog('instrument')}>
            {t('instruments.new')}
          </Button>
        </Group>
      </Group>

      {instruments?.length === 0 && <Text c="dimmed" ta="center" py="md">{t('instruments.empty')}</Text>}
      {instruments?.map((instrument) => (
        <InstrumentCard key={instrument.id} project={project} instrument={instrument} />
      ))}

      {dialog === 'instrument' && <InstrumentFormModal project={project} onClose={() => setDialog('none')} />}
      {dialog === 'templates' && <TemplatesModal project={project} onClose={() => setDialog('none')} />}
    </Stack>
  )
}
