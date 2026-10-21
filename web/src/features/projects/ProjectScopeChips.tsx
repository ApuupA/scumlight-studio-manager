import { Chip, Group, Tooltip } from '@mantine/core'
import { useTranslation } from 'react-i18next'
import type { Project } from '../../api/types'
import { notifyError } from '../../utils/notify'
import { useSetProjectScope } from './projectApi'

type ScopePart = 'songwriting' | 'recording' | 'mixing' | 'mastering'

/**
 * The scope of a project in its header: Songwriting / Recording / Mixing / Mastering. A click saves right away.
 * Switching a part off only hides its tabs (e.g. input lists for a mix-only project) - the data stays.
 */
export default function ProjectScopeChips({ project }: { project: Project }) {
  const { t } = useTranslation()
  const setScope = useSetProjectScope()

  const value: ScopePart[] = [
    ...(project.includesSongwriting ? ['songwriting' as const] : []),
    ...(project.includesRecording ? ['recording' as const] : []),
    ...(project.includesMixing ? ['mixing' as const] : []),
    ...(project.includesMastering ? ['mastering' as const] : []),
  ]

  const handleChange = (parts: string[]) => {
    // At least one part must stay on (the backend checks this as well)
    if (parts.length === 0) return

    setScope.mutate(
      {
        id: project.id,
        input: {
          includesSongwriting: parts.includes('songwriting'),
          includesRecording: parts.includes('recording'),
          includesMixing: parts.includes('mixing'),
          includesMastering: parts.includes('mastering'),
        },
      },
      { onError: (error) => notifyError(error, t) },
    )
  }

  return (
    <Tooltip label={t('projects.scopeHint')} multiline w={300}>
      <div>
        <Chip.Group multiple value={value} onChange={handleChange}>
          <Group gap={6}>
            <Chip value="songwriting" size="xs">{t('projects.scope.songwriting')}</Chip>
            <Chip value="recording" size="xs">{t('projects.scope.recording')}</Chip>
            <Chip value="mixing" size="xs">{t('projects.scope.mixing')}</Chip>
            <Chip value="mastering" size="xs">{t('projects.scope.mastering')}</Chip>
          </Group>
        </Chip.Group>
      </div>
    </Tooltip>
  )
}
