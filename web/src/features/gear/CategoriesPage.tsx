import { ActionIcon, Alert, Badge, Button, Center, Group, Loader, Stack, Table, Text, Title, Tooltip } from '@mantine/core'
import { modals } from '@mantine/modals'
import { IconPencil, IconPlus, IconTrash } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../api/errors'
import type { GearCategory, GearCategoryInput } from '../../api/types'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useCategories, useCreateCategory, useDeleteCategory, useUpdateCategory } from './categoryApi'
import CategoryFormModal from './CategoryFormModal'

/** State of the modal: closed, creating a new category, or editing an existing one. */
type ModalState = { open: false } | { open: true; category?: GearCategory }

export default function CategoriesPage() {
  const { t } = useTranslation()
  const { data: categories, isPending, error } = useCategories()
  const createCategory = useCreateCategory()
  const updateCategory = useUpdateCategory()
  const deleteCategory = useDeleteCategory()
  const [modal, setModal] = useState<ModalState>({ open: false })

  const closeModal = () => setModal({ open: false })

  const handleSubmit = (input: GearCategoryInput) => {
    const callbacks = {
      onSuccess: () => {
        notifySuccess(t('common.saved'))
        closeModal()
      },
      onError: (error: Error) => notifyError(error, t),
    }

    if (modal.open && modal.category) {
      updateCategory.mutate({ id: modal.category.id, input }, callbacks)
    } else {
      createCategory.mutate(input, callbacks)
    }
  }

  const confirmDelete = (category: GearCategory) => {
    modals.openConfirmModal({
      title: t('common.confirmDeleteTitle'),
      children: <Text size="sm">{t('categories.deleteConfirm', { name: category.name })}</Text>,
      labels: { confirm: t('common.delete'), cancel: t('common.cancel') },
      confirmProps: { color: 'red' },
      onConfirm: () =>
        deleteCategory.mutate(category.id, {
          onSuccess: () => notifySuccess(t('common.deleted')),
          onError: (error) => notifyError(error, t),
        }),
    })
  }

  if (isPending) {
    return <Center py="xl"><Loader /></Center>
  }

  if (error) {
    return <Alert color="red">{getErrorMessage(error, t)}</Alert>
  }

  const nextSortOrder = Math.max(0, ...categories.map((c) => c.sortOrder)) + 10

  return (
    <Stack>
      <Group justify="space-between">
        <Title order={2}>{t('categories.title')}</Title>
        <Button leftSection={<IconPlus size={18} />} onClick={() => setModal({ open: true })}>
          {t('categories.new')}
        </Button>
      </Group>

      {categories.length === 0 ? (
        <Text c="dimmed" ta="center" py="xl">{t('categories.empty')}</Text>
      ) : (
        <Table.ScrollContainer minWidth={560}>
          <Table highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>{t('categories.name')}</Table.Th>
                <Table.Th>{t('categories.description')}</Table.Th>
                <Table.Th w={110}>{t('categories.sortOrder')}</Table.Th>
                <Table.Th w={90}>{t('categories.itemCount')}</Table.Th>
                <Table.Th w={100} />
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {categories.map((category) => (
                <Table.Tr key={category.id}>
                  <Table.Td fw={500}>
                    {category.name}
                    {category.isSoftware && <Badge size="xs" variant="light" ml="xs">{t('categories.software')}</Badge>}
                  </Table.Td>
                  <Table.Td c="dimmed">{category.description}</Table.Td>
                  <Table.Td>{category.sortOrder}</Table.Td>
                  <Table.Td>{category.gearItemCount}</Table.Td>
                  <Table.Td>
                    <Group gap={4} justify="flex-end" wrap="nowrap">
                      <Tooltip label={t('common.edit')}>
                        <ActionIcon variant="subtle" onClick={() => setModal({ open: true, category })} aria-label={t('common.edit')}>
                          <IconPencil size={16} />
                        </ActionIcon>
                      </Tooltip>
                      <Tooltip label={t('common.delete')}>
                        <ActionIcon variant="subtle" color="red" onClick={() => confirmDelete(category)} aria-label={t('common.delete')}>
                          <IconTrash size={16} />
                        </ActionIcon>
                      </Tooltip>
                    </Group>
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        </Table.ScrollContainer>
      )}

      {/* key: re-create the modal form with fresh values for every opened category */}
      {modal.open && (
        <CategoryFormModal
          key={modal.category?.id ?? 'new'}
          opened
          category={modal.category}
          nextSortOrder={nextSortOrder}
          isSaving={createCategory.isPending || updateCategory.isPending}
          onSubmit={handleSubmit}
          onClose={closeModal}
        />
      )}
    </Stack>
  )
}
