import { ActionIcon, Alert, Badge, Button, Center, Group, Loader, NumberInput, Paper, Stack, Text, TextInput, Title } from '@mantine/core'
import { useForm } from '@mantine/form'
import { IconPlus, IconTrash } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router'
import logo from '../../assets/scumlight-logo-480.webp'
import LanguageSwitcher from '../../layout/LanguageSwitcher'
import { notifyError } from '../../utils/notify'
import classes from '../auth/LoginPage.module.css'
import { useAddBandGearSuggestion, useBandGearForm, useDeleteBandGearSuggestion } from './bandGearLinkApi'

/**
 * Public page for the band (route /band-gear/:token, no login): enter the gear you bring to the studio.
 * Every entry is a suggestion until the studio accepts it.
 */
export default function BandGearPage() {
  const { t } = useTranslation()
  const token = useParams().token ?? ''
  const { data: page, isPending, error } = useBandGearForm(token)
  const addSuggestion = useAddBandGearSuggestion(token)
  const deleteSuggestion = useDeleteBandGearSuggestion(token)

  const form = useForm({
    initialValues: { name: '', quantity: 1 as number | string, owner: '', notes: '' },
    validate: { name: (value) => (value.trim() ? null : t('validation.required')) },
  })

  const handleSubmit = (values: typeof form.values) => {
    addSuggestion.mutate(
      {
        name: values.name.trim(),
        quantity: Number(values.quantity) || 1,
        owner: values.owner.trim() || null,
        notes: values.notes.trim() || null,
      },
      {
        // Keep "who brings it" - the next item is often from the same person
        onSuccess: () => form.setValues({ name: '', quantity: 1, notes: '' }),
        onError: (error) => notifyError(error, t),
      },
    )
  }

  return (
    <div className={classes.page}>
      <div className={classes.language}>
        <LanguageSwitcher />
      </div>

      <img src={logo} alt={t('app.studioName')} className={classes.logo} style={{ width: 160 }} />

      <Paper withBorder shadow="xl" p="lg" className={classes.card} style={{ width: 640 }}>
        {isPending ? (
          <Center py="xl"><Loader /></Center>
        ) : error || !page ? (
          <Alert color="red" title={t('bandGearPage.invalidTitle')}>{t('bandGearPage.invalid')}</Alert>
        ) : (
          <Stack>
            <div>
              <Title order={3}>{t('bandGearPage.title')}</Title>
              <Text c="dimmed">{[page.bandNames, page.projectTitle].filter(Boolean).join(' · ')}</Text>
            </div>
            <Text size="sm">{t('bandGearPage.hint')}</Text>

            {/* What is already on the list - so nothing is entered twice */}
            {page.entries.length > 0 && (
              <Stack gap={4}>
                {page.entries.map((entry) => (
                  <Group key={entry.id} justify="space-between" wrap="nowrap" gap="xs">
                    <div style={{ minWidth: 0 }}>
                      <Text size="sm" fw={500}>{entry.quantity > 1 && `${entry.quantity}× `}{entry.name}</Text>
                      <Text size="xs" c="dimmed">{[entry.owner, entry.notes].filter(Boolean).join(' · ')}</Text>
                    </div>
                    <Group gap={4} wrap="nowrap" style={{ flexShrink: 0 }}>
                      <Badge size="sm" variant="light" color={entry.isSuggestion ? 'yellow' : 'green'}>
                        {entry.isSuggestion ? t('bandGearPage.pending') : t('bandGearPage.accepted')}
                      </Badge>
                      {entry.isSuggestion && (
                        <ActionIcon variant="subtle" color="red" aria-label={t('common.delete')} loading={deleteSuggestion.isPending}
                          onClick={() => deleteSuggestion.mutate(entry.id, { onError: (error) => notifyError(error, t) })}>
                          <IconTrash size={16} />
                        </ActionIcon>
                      )}
                    </Group>
                  </Group>
                ))}
              </Stack>
            )}

            <form onSubmit={form.onSubmit(handleSubmit)}>
              <Stack gap="xs">
                <Group grow align="flex-start">
                  <TextInput label={t('bandGearPage.name')} placeholder={t('bandGearPage.namePlaceholder')} withAsterisk maxLength={200}
                    {...form.getInputProps('name')} />
                  <NumberInput label={t('bandGearPage.quantity')} min={1} max={100} allowDecimal={false} maw={100}
                    {...form.getInputProps('quantity')} />
                </Group>
                <Group grow align="flex-start">
                  <TextInput label={t('bandGearPage.owner')} placeholder={t('bandGearPage.ownerPlaceholder')} maxLength={200}
                    {...form.getInputProps('owner')} />
                  <TextInput label={t('common.notes')} placeholder={t('bandGearPage.notesPlaceholder')} maxLength={1000}
                    {...form.getInputProps('notes')} />
                </Group>
                <Group justify="flex-end">
                  <Button type="submit" leftSection={<IconPlus size={16} />} loading={addSuggestion.isPending}>
                    {t('bandGearPage.add')}
                  </Button>
                </Group>
              </Stack>
            </form>
          </Stack>
        )}
      </Paper>
    </div>
  )
}
