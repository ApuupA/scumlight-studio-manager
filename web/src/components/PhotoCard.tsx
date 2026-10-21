import { Button, Card, Center, FileButton, Group, Image, Stack, Text } from '@mantine/core'
import { IconPhoto, IconTrash, IconUpload } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { photoUrl, usePhoto, type PhotoOwner, type WithPhoto } from '../api/photos'
import { notifyError } from '../utils/notify'

interface PhotoCardProps {
  owner: PhotoOwner
  entity: WithPhoto
  /** Lists to reload after a change, e.g. ['clients']. */
  invalidateKeys: string[]
  alt: string
  height?: number
  /** Shown (with the hint below it) when the entity has no own picture, e.g. the client's picture for a band. */
  fallbackUrl?: string
  fallbackHint?: string
}

/**
 * The picture of a gear item, client, band or project with buttons to upload / replace / remove it.
 */
export default function PhotoCard({ owner, entity, invalidateKeys, alt, height = 220, fallbackUrl, fallbackHint }: PhotoCardProps) {
  const { t } = useTranslation()
  const { upload, remove } = usePhoto(owner, invalidateKeys)

  const handleFile = (file: File | null) => {
    if (file) {
      upload.mutate({ id: entity.id, file }, { onError: (error) => notifyError(error, t) })
    }
  }

  return (
    <Card withBorder>
      <Card.Section>
        {entity.hasPhoto || fallbackUrl ? (
          <Image src={photoUrl(owner, entity) ?? fallbackUrl} alt={alt} h={height} fit="contain" bg="var(--mantine-color-default-hover)" />
        ) : (
          <Center h={height} bg="var(--mantine-color-default-hover)">
            <Stack align="center" gap={4}>
              <IconPhoto size={40} stroke={1.2} color="var(--mantine-color-dimmed)" />
              <Text size="sm" c="dimmed">{t('photo.none')}</Text>
            </Stack>
          </Center>
        )}
      </Card.Section>

      <Group mt="md" gap="xs">
        <FileButton onChange={handleFile} accept="image/jpeg,image/png,image/webp">
          {(props) => (
            <Button {...props} size="xs" variant="light" leftSection={<IconUpload size={14} />} loading={upload.isPending}>
              {entity.hasPhoto ? t('photo.replace') : t('photo.upload')}
            </Button>
          )}
        </FileButton>
        {entity.hasPhoto && (
          <Button
            size="xs"
            variant="subtle"
            color="red"
            leftSection={<IconTrash size={14} />}
            loading={remove.isPending}
            onClick={() => remove.mutate(entity.id, { onError: (error) => notifyError(error, t) })}
          >
            {t('photo.remove')}
          </Button>
        )}
      </Group>
      {!entity.hasPhoto && fallbackUrl && fallbackHint && <Text size="xs" c="dimmed" mt="xs">{fallbackHint}</Text>}
      <Text size="xs" c="dimmed" mt="xs">{t('photo.hint')}</Text>
    </Card>
  )
}
