import { Badge, Button, Group as MantineGroup, Modal, MultiSelect, Stack, Switch, Text, Tooltip } from '@mantine/core'
import { IconUsers, IconWorld } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { Project } from '../../api/types'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useIsOwner } from '../auth/authApi'
import { useGroups, useSetProjectAccess } from './groupApi'

/**
 * "Who sees the project" in the project header: the groups and "visible to everyone".
 * The owner changes it with a click, assistants only see it.
 */
export default function ProjectAccess({ project }: { project: Project }) {
  const { t } = useTranslation()
  const isOwner = useIsOwner()
  const [editing, setEditing] = useState(false)

  const badges = (
    <MantineGroup gap={4}>
      {project.isVisibleToAll && <Badge size="sm" variant="light" color="teal" leftSection={<IconWorld size={10} />}>{t('groups.visibleToAll')}</Badge>}
      {project.groups.map((g) => <Badge key={g.id} size="sm" variant="light" color="gray" leftSection={<IconUsers size={10} />}>{g.name}</Badge>)}
      {!project.isVisibleToAll && project.groups.length === 0 && <Text size="xs" c="dimmed">{t('groups.onlyOwner')}</Text>}
    </MantineGroup>
  )

  if (!isOwner) return badges

  return (
    <>
      <Tooltip label={t('groups.accessHint')} multiline w={300}>
        <Button variant="subtle" size="compact-xs" color="gray" onClick={() => setEditing(true)} aria-label={t('groups.access')}>
          {badges}
        </Button>
      </Tooltip>
      {editing && <AccessModal project={project} onClose={() => setEditing(false)} />}
    </>
  )
}

function AccessModal({ project, onClose }: { project: Project; onClose: () => void }) {
  const { t } = useTranslation()
  const { data: groups } = useGroups()
  const setAccess = useSetProjectAccess()
  const [groupIds, setGroupIds] = useState(project.groups.map((g) => String(g.id)))
  const [visibleToAll, setVisibleToAll] = useState(project.isVisibleToAll)

  const handleSave = () =>
    setAccess.mutate(
      { id: project.id, input: { groupIds: groupIds.map(Number), isVisibleToAll: visibleToAll } },
      {
        onSuccess: () => {
          notifySuccess(t('common.saved'))
          onClose()
        },
        onError: (error) => notifyError(error, t),
      },
    )

  return (
    <Modal opened onClose={onClose} title={t('groups.access')}>
      <Stack>
        <Text size="sm" c="dimmed">{t('groups.accessHint')}</Text>
        <MultiSelect
          label={t('groups.title')}
          placeholder={groups?.length === 0 ? t('groups.none') : undefined}
          data={(groups ?? []).map((g) => ({ value: String(g.id), label: g.name }))}
          value={groupIds}
          onChange={setGroupIds}
          searchable
        />
        <Switch label={t('groups.visibleToAll')} description={t('groups.visibleToAllHint')} checked={visibleToAll}
          onChange={(e) => setVisibleToAll(e.currentTarget.checked)} />
        <MantineGroup justify="flex-end">
          <Button variant="default" onClick={onClose}>{t('common.cancel')}</Button>
          <Button onClick={handleSave} loading={setAccess.isPending}>{t('common.save')}</Button>
        </MantineGroup>
      </Stack>
    </Modal>
  )
}
