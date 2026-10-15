import { Button, Group, Modal, SimpleGrid, Stack, Textarea, TextInput } from '@mantine/core'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import type { Band } from '../../api/types'
import { notifyError, notifySuccess } from '../../utils/notify'
import { textOrNull } from '../../utils/form'
import { useCreateBand, useUpdateBand } from './clientApi'

interface BandFormModalProps {
  clientId: number
  /** The band to edit, undefined when creating a new one. */
  band?: Band
  onClose: () => void
}

/** Dialog to create or edit a band of a client. Saves itself and closes on success. */
export default function BandFormModal({ clientId, band, onClose }: BandFormModalProps) {
  const { t } = useTranslation()
  const createBand = useCreateBand()
  const updateBand = useUpdateBand()

  const form = useForm({
    initialValues: {
      name: band?.name ?? '',
      genre: band?.genre ?? '',
      members: band?.members ?? '',
      email: band?.email ?? '',
      website: band?.website ?? '',
      notes: band?.notes ?? '',
    },
    validate: {
      name: (value) => (value.trim() ? null : t('validation.required')),
      email: (value) => (!value.trim() || /^\S+@\S+\.\S+$/.test(value.trim()) ? null : t('validation.email')),
    },
  })

  const handleSubmit = (values: typeof form.values) => {
    const input = {
      clientId,
      name: values.name.trim(),
      genre: textOrNull(values.genre),
      members: textOrNull(values.members),
      email: textOrNull(values.email),
      website: textOrNull(values.website),
      notes: textOrNull(values.notes),
    }
    const callbacks = {
      onSuccess: () => {
        notifySuccess(t('common.saved'))
        onClose()
      },
      onError: (error: Error) => notifyError(error, t),
    }

    if (band) {
      updateBand.mutate({ id: band.id, input }, callbacks)
    } else {
      createBand.mutate(input, callbacks)
    }
  }

  return (
    <Modal opened onClose={onClose} title={band ? t('bands.editTitle') : t('bands.newTitle')} size="lg">
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <SimpleGrid cols={{ base: 1, sm: 2 }}>
            <TextInput label={t('bands.name')} withAsterisk maxLength={200} data-autofocus {...form.getInputProps('name')} />
            <TextInput label={t('bands.genre')} maxLength={100} {...form.getInputProps('genre')} />
            <TextInput label={t('bands.email')} type="email" maxLength={200} {...form.getInputProps('email')} />
            <TextInput label={t('bands.website')} maxLength={500} {...form.getInputProps('website')} />
          </SimpleGrid>
          <Textarea
            label={t('bands.members')}
            placeholder={t('bands.membersPlaceholder')}
            autosize
            minRows={2}
            maxLength={1000}
            {...form.getInputProps('members')}
          />
          <Textarea label={t('common.notes')} autosize minRows={2} maxLength={4000} {...form.getInputProps('notes')} />
          <Group justify="flex-end">
            <Button variant="default" onClick={onClose}>{t('common.cancel')}</Button>
            <Button type="submit" loading={createBand.isPending || updateBand.isPending}>{t('common.save')}</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}
