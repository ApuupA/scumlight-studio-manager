import { Button, Group, SimpleGrid, Stack, Textarea, TextInput } from '@mantine/core'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import type { Client, ClientInput } from '../../api/types'
import { textOrNull } from '../../utils/form'

interface ClientFormProps {
  /** The client to edit, undefined when creating a new one. */
  client?: Client
  isSaving: boolean
  onSubmit: (input: ClientInput) => void
  onCancel?: () => void
}

/** Form for a client, used in the "new client" dialog and on the client page. */
export default function ClientForm({ client, isSaving, onSubmit, onCancel }: ClientFormProps) {
  const { t } = useTranslation()

  const form = useForm({
    initialValues: {
      name: client?.name ?? '',
      contactPerson: client?.contactPerson ?? '',
      email: client?.email ?? '',
      phone: client?.phone ?? '',
      address: client?.address ?? '',
      notes: client?.notes ?? '',
    },
    validate: {
      name: (value) => (value.trim() ? null : t('validation.required')),
      email: (value) => (!value.trim() || /^\S+@\S+\.\S+$/.test(value.trim()) ? null : t('validation.email')),
    },
  })

  const handleSubmit = (values: typeof form.values) => {
    onSubmit({
      name: values.name.trim(),
      contactPerson: textOrNull(values.contactPerson),
      email: textOrNull(values.email),
      phone: textOrNull(values.phone),
      address: textOrNull(values.address),
      notes: textOrNull(values.notes),
    })
  }

  return (
    <form onSubmit={form.onSubmit(handleSubmit)}>
      <Stack>
        <TextInput
          label={t('clients.name')}
          placeholder={t('clients.namePlaceholder')}
          withAsterisk
          maxLength={200}
          data-autofocus
          {...form.getInputProps('name')}
        />
        <SimpleGrid cols={{ base: 1, sm: 2 }}>
          <TextInput label={t('clients.contactPerson')} maxLength={200} {...form.getInputProps('contactPerson')} />
          <TextInput label={t('clients.email')} type="email" maxLength={200} {...form.getInputProps('email')} />
          <TextInput label={t('clients.phone')} maxLength={50} {...form.getInputProps('phone')} />
          <TextInput label={t('clients.address')} maxLength={500} {...form.getInputProps('address')} />
        </SimpleGrid>
        <Textarea label={t('common.notes')} autosize minRows={2} maxLength={4000} {...form.getInputProps('notes')} />
        <Group justify="flex-end">
          {onCancel && <Button variant="default" onClick={onCancel}>{t('common.cancel')}</Button>}
          <Button type="submit" loading={isSaving}>{t('common.save')}</Button>
        </Group>
      </Stack>
    </form>
  )
}
