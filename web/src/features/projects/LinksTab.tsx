import { Anchor, Badge, Button, Group, Table, Text } from '@mantine/core'
import { IconExternalLink, IconPlus } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { ProjectLink } from '../../api/types'
import RowActions from '../../components/RowActions'
import { confirmDelete } from '../../utils/confirmDelete'
import { notifyError, notifySuccess } from '../../utils/notify'
import LinkFormModal from './LinkFormModal'
import { useDeleteProjectLink, useProjectLinks } from './projectApi'

type ModalState = { open: false } | { open: true; link?: ProjectLink }

/** References and rough mixes of a project, as links (file upload comes with the mix revision tool). */
export default function LinksTab({ projectId }: { projectId: number }) {
  const { t } = useTranslation()
  const { data: links } = useProjectLinks(projectId)
  const deleteLink = useDeleteProjectLink()
  const [modal, setModal] = useState<ModalState>({ open: false })

  const handleDelete = (link: ProjectLink) =>
    confirmDelete(t, t('links.deleteConfirm', { name: link.title }), () =>
      deleteLink.mutate(link.id, {
        onSuccess: () => notifySuccess(t('common.deleted')),
        onError: (error) => notifyError(error, t),
      }),
    )

  return (
    <>
      <Group justify="space-between" mb="sm">
        <Text size="sm" c="dimmed">{t('links.hint')}</Text>
        <Button variant="light" leftSection={<IconPlus size={16} />} onClick={() => setModal({ open: true })}>
          {t('links.new')}
        </Button>
      </Group>

      {links?.length === 0 ? (
        <Text c="dimmed" ta="center" py="md">{t('links.empty')}</Text>
      ) : (
        <Table.ScrollContainer minWidth={600}>
          <Table highlightOnHover>
            <Table.Tbody>
              {links?.map((link) => (
                <Table.Tr key={link.id}>
                  <Table.Td w={130}>
                    <Badge variant="light" color={link.type === 'Reference' ? 'blue' : 'ember'}>
                      {t(`projectLinkType.${link.type}`)}
                    </Badge>
                  </Table.Td>
                  <Table.Td>
                    {/* target=_blank opens the link in a new browser tab */}
                    <Anchor href={link.url} target="_blank" rel="noopener noreferrer" fw={500}>
                      <Group gap={4} wrap="nowrap">{link.title}<IconExternalLink size={14} /></Group>
                    </Anchor>
                    {link.notes && <Text size="xs" c="dimmed">{link.notes}</Text>}
                  </Table.Td>
                  <Table.Td c="dimmed">{link.songTitle}</Table.Td>
                  <Table.Td w={90}>
                    <RowActions onEdit={() => setModal({ open: true, link })} onDelete={() => handleDelete(link)} />
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        </Table.ScrollContainer>
      )}

      {modal.open && (
        <LinkFormModal key={modal.link?.id ?? 'new'} projectId={projectId} link={modal.link} onClose={() => setModal({ open: false })} />
      )}
    </>
  )
}
