import { Alert, Anchor, Button, Center, Grid, Group, Loader, Paper, Stack, Text, Title } from '@mantine/core'
import { modals } from '@mantine/modals'
import { IconArrowLeft, IconTrash } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate, useParams } from 'react-router'
import { getErrorMessage } from '../../api/errors'
import type { GearItemInput } from '../../api/types'
import { formatDate } from '../../utils/format'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useCreateGearItem, useDeleteGearItem, useGearItem, useUpdateGearItem } from './gearApi'
import GearItemForm from './GearItemForm'
import GearPhotoCard from './GearPhotoCard'
import GearStatusBadge from './GearStatusBadge'
import GearUsages from './GearUsages'

/**
 * Page for one gear item. Route /gear/new creates a new item, /gear/:id edits an existing one.
 */
export default function GearItemPage() {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const params = useParams()
  const id = params.id ? Number(params.id) : undefined
  const isNew = id === undefined

  const { data: item, isPending, error } = useGearItem(id)
  const createItem = useCreateGearItem()
  const updateItem = useUpdateGearItem()
  const deleteItem = useDeleteGearItem()

  const handleSubmit = (input: GearItemInput) => {
    if (isNew) {
      createItem.mutate(input, {
        onSuccess: (created) => {
          notifySuccess(t('common.saved'))
          navigate(`/gear/${created.id}`, { replace: true }) // now a photo can be uploaded
        },
        onError: (error) => notifyError(error, t),
      })
    } else {
      updateItem.mutate({ id, input }, {
        onSuccess: () => notifySuccess(t('common.saved')),
        onError: (error) => notifyError(error, t),
      })
    }
  }

  const confirmDelete = () => {
    modals.openConfirmModal({
      title: t('common.confirmDeleteTitle'),
      children: <Text size="sm">{t('gear.deleteConfirm', { name: item?.name })}</Text>,
      labels: { confirm: t('common.delete'), cancel: t('common.cancel') },
      confirmProps: { color: 'red' },
      onConfirm: () =>
        deleteItem.mutate(id!, {
          onSuccess: () => {
            notifySuccess(t('common.deleted'))
            navigate('/gear')
          },
          onError: (error) => notifyError(error, t),
        }),
    })
  }

  if (!isNew && isPending) {
    return <Center py="xl"><Loader /></Center>
  }

  if (!isNew && error) {
    return <Alert color="red">{getErrorMessage(error, t)}</Alert>
  }

  return (
    <Stack>
      <Anchor component={Link} to="/gear" size="sm">
        <Group gap={4}><IconArrowLeft size={14} />{t('common.back')}</Group>
      </Anchor>

      <Group justify="space-between">
        <Group gap="sm">
          <Title order={2}>{isNew ? t('gear.newTitle') : item?.name}</Title>
          {item && <GearStatusBadge status={item.status} />}
        </Group>
        {item && (
          <Button variant="subtle" color="red" leftSection={<IconTrash size={16} />} onClick={confirmDelete}>
            {t('common.delete')}
          </Button>
        )}
      </Group>

      <Grid gap="lg">
        <Grid.Col span={{ base: 12, md: 8 }}>
          <Paper withBorder p="md">
            {/* key: re-create the form with fresh values when switching to another item */}
            <GearItemForm
              key={item?.id ?? 'new'}
              item={item}
              isSaving={createItem.isPending || updateItem.isPending}
              onSubmit={handleSubmit}
              onCancel={() => navigate('/gear')}
            />
          </Paper>
        </Grid.Col>

        {item && (
          <Grid.Col span={{ base: 12, md: 4 }}>
            <Stack>
              <GearPhotoCard item={item} />
              <GearUsages gearItemId={item.id} />
              <Text size="xs" c="dimmed">
                {t('common.created')}: {formatDate(item.dateCreated, i18n.language)}
                {' · '}
                {t('common.updated')}: {formatDate(item.dateUpdated, i18n.language)}
              </Text>
            </Stack>
          </Grid.Col>
        )}
      </Grid>
    </Stack>
  )
}
