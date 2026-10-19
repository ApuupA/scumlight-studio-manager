import { AspectRatio, Card, Center, Image, Stack, Text } from '@mantine/core'
import type { ReactNode } from 'react'

interface TileProps {
  /** Picture URL - without one the icon is shown on a grey area. */
  image?: string
  icon: ReactNode
  title: string
  subtitle?: ReactNode
  /** Badges / progress below the title. */
  children?: ReactNode
  onClick: () => void
}

/**
 * One tile of a tile view (clients, projects, gear): square picture, title, subtitle and badges.
 */
export default function Tile({ image, icon, title, subtitle, children, onClick }: TileProps) {
  return (
    <Card withBorder padding="sm" onClick={onClick} style={{ cursor: 'pointer' }}>
      <Card.Section>
        <AspectRatio ratio={1}>
          {image ? (
            <Image src={image} alt={title} fit="cover" />
          ) : (
            <Center bg="var(--mantine-color-default-hover)" c="dimmed">{icon}</Center>
          )}
        </AspectRatio>
      </Card.Section>
      <Stack gap={4} mt="sm">
        <Text fw={600} lineClamp={1}>{title}</Text>
        {subtitle && <Text size="xs" c="dimmed" lineClamp={2}>{subtitle}</Text>}
        {children}
      </Stack>
    </Card>
  )
}
