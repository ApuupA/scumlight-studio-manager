import { Button, Group, Modal, Stack, Text, TextInput } from '@mantine/core'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { ProjectInstrument } from '../../api/types'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useSaveAsTemplate } from './instrumentApi'

/** Saves the setup of an instrument as reusable template - just asks for the template name. */
export default function SaveTemplateModal({ instrument, onClose }: { instrument: ProjectInstrument; onClose: () => void }) {
  const { t } = useTranslation()
  const saveAsTemplate = useSaveAsTemplate()
  const [name, setName] = useState(instrument.name)

  const handleSave = () => {
    saveAsTemplate.mutate({ instrumentId: instrument.id, name: name.trim() }, {
      onSuccess: () => {
        notifySuccess(t('templates.saved', { name: name.trim() }))
        onClose()
      },
      onError: (error) => notifyError(error, t),
    })
  }

  return (
    <Modal opened onClose={onClose} title={t('templates.saveAs')}>
      <Stack>
        <Text size="sm" c="dimmed">{t('templates.saveAsHint')}</Text>
        <TextInput label={t('templates.name')} value={name} onChange={(e) => setName(e.currentTarget.value)} maxLength={100} data-autofocus />
        <Group justify="flex-end">
          <Button variant="default" onClick={onClose}>{t('common.cancel')}</Button>
          <Button onClick={handleSave} disabled={!name.trim()} loading={saveAsTemplate.isPending}>{t('common.save')}</Button>
        </Group>
      </Stack>
    </Modal>
  )
}
