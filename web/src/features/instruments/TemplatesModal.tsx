import { ActionIcon, Button, Group, Modal, Paper, Select, Stack, Text, TextInput } from '@mantine/core'
import { IconTrash } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { Project, SetupTemplate } from '../../api/types'
import { confirmDelete } from '../../utils/confirmDelete'
import { idOrNull } from '../../utils/form'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useApplyTemplate, useDeleteTemplate, useSetupTemplates } from './instrumentApi'

/**
 * List of all setup templates: use one for this project (creates a new instrument) or delete it.
 */
export default function TemplatesModal({ project, onClose }: { project: Project; onClose: () => void }) {
  const { t } = useTranslation()
  const { data: templates } = useSetupTemplates()
  const applyTemplate = useApplyTemplate()
  const deleteTemplate = useDeleteTemplate()

  // The template chosen with "use" and the values for the new instrument
  const [chosen, setChosen] = useState<SetupTemplate | null>(null)
  const [instrumentName, setInstrumentName] = useState('')
  const [bandId, setBandId] = useState<string | null>(project.bands.length === 1 ? String(project.bands[0].id) : null)

  const choose = (template: SetupTemplate) => {
    setChosen(template)
    setInstrumentName(template.name)
  }

  const handleApply = () => {
    if (!chosen) return
    applyTemplate.mutate(
      { projectId: project.id, setupTemplateId: chosen.id, instrumentName: instrumentName.trim() || null, bandId: idOrNull(bandId) },
      {
        onSuccess: (instrument) => {
          notifySuccess(t('templates.applied', { name: instrument.name }))
          onClose()
        },
        onError: (error) => notifyError(error, t),
      },
    )
  }

  const handleDelete = (template: SetupTemplate) =>
    confirmDelete(t, t('templates.deleteConfirm', { name: template.name }), () =>
      deleteTemplate.mutate(template.id, { onError: (error) => notifyError(error, t) }),
    )

  return (
    <Modal opened onClose={onClose} title={t('templates.title')} size="lg">
      <Stack>
        {templates?.length === 0 && <Text c="dimmed">{t('templates.empty')}</Text>}

        {templates?.map((template) => (
          <Paper
            key={template.id}
            withBorder
            p="sm"
            style={{ borderColor: chosen?.id === template.id ? 'var(--mantine-primary-color-filled)' : undefined }}
          >
            <Group justify="space-between" wrap="nowrap" align="flex-start">
              <div style={{ minWidth: 0 }}>
                <Text fw={600}>{template.name}</Text>
                <Text size="xs" c="dimmed" lineClamp={2}>
                  {template.items.map((item) => [item.displayName, item.usedFor].filter(Boolean).join(' · ')).join(template.isChain ? ' → ' : ', ')}
                </Text>
              </div>
              <Group gap={4} wrap="nowrap">
                <Button size="xs" variant={chosen?.id === template.id ? 'filled' : 'light'} onClick={() => choose(template)}>
                  {t('templates.use')}
                </Button>
                <ActionIcon variant="subtle" color="red" onClick={() => handleDelete(template)} aria-label={t('common.delete')}>
                  <IconTrash size={16} />
                </ActionIcon>
              </Group>
            </Group>
          </Paper>
        ))}

        {chosen && (
          <Paper withBorder p="sm">
            <Stack gap="sm">
              <Group grow align="flex-start">
                <TextInput label={t('instruments.name')} value={instrumentName} onChange={(e) => setInstrumentName(e.currentTarget.value)} maxLength={100} />
                {project.bands.length > 0 && (
                  <Select
                    label={t('instruments.band')}
                    clearable
                    value={bandId}
                    onChange={setBandId}
                    data={project.bands.map((band) => ({ value: String(band.id), label: band.name }))}
                  />
                )}
              </Group>
              <Group justify="flex-end">
                <Button onClick={handleApply} loading={applyTemplate.isPending}>{t('templates.create')}</Button>
              </Group>
            </Stack>
          </Paper>
        )}
      </Stack>
    </Modal>
  )
}
