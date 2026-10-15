import { Button, Card, Center, FileButton, Group, Image, Stack, Text } from '@mantine/core'
import { IconPhoto, IconTrash, IconUpload } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import type { GearItem } from '../../api/types'
import { notifyError } from '../../utils/notify'
import { gearPhotoUrl, useDeleteGearPhoto, useUploadGearPhoto } from './gearApi'

/** Shows the photo of a gear item with buttons to upload / replace / remove it. */
export default function GearPhotoCard({ item }: { item: GearItem }) {
  const { t } = useTranslation()
  const upload = useUploadGearPhoto()
  const remove = useDeleteGearPhoto()

  const handleFile = (file: File | null) => {
    if (file) {
      upload.mutate({ id: item.id, file }, { onError: (error) => notifyError(error, t) })
    }
  }

  return (
    <Card withBorder>
      <Card.Section>
        {item.hasPhoto ? (
          <Image src={gearPhotoUrl(item)} alt={item.name} h={260} fit="contain" bg="var(--mantine-color-default-hover)" />
        ) : (
          <Center h={260} bg="var(--mantine-color-default-hover)">
            <Stack align="center" gap={4}>
              <IconPhoto size={40} stroke={1.2} color="var(--mantine-color-dimmed)" />
              <Text size="sm" c="dimmed">{t('gear.noPhoto')}</Text>
            </Stack>
          </Center>
        )}
      </Card.Section>

      <Group mt="md" gap="xs">
        <FileButton onChange={handleFile} accept="image/jpeg,image/png,image/webp">
          {(props) => (
            <Button {...props} variant="light" leftSection={<IconUpload size={16} />} loading={upload.isPending}>
              {item.hasPhoto ? t('gear.replacePhoto') : t('gear.uploadPhoto')}
            </Button>
          )}
        </FileButton>
        {item.hasPhoto && (
          <Button
            variant="subtle"
            color="red"
            leftSection={<IconTrash size={16} />}
            loading={remove.isPending}
            onClick={() => remove.mutate(item.id, { onError: (error) => notifyError(error, t) })}
          >
            {t('gear.removePhoto')}
          </Button>
        )}
      </Group>
      <Text size="xs" c="dimmed" mt="xs">{t('gear.photoHint')}</Text>
    </Card>
  )
}
