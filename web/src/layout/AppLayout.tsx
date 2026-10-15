import { ActionIcon, AppShell, Burger, Button, Group, NavLink, Text, Title, useMantineColorScheme } from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import { IconCategory, IconLogout, IconMicrophone2, IconMoon, IconSun, IconTool } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { NavLink as RouterNavLink, Outlet, useNavigate } from 'react-router'
import { useCurrentUser, useLogout } from '../features/auth/authApi'
import LanguageSwitcher from './LanguageSwitcher'

/** Entries of the side navigation. New features (projects, sessions, mixes, ...) are added here. */
const navItems = [
  { to: '/gear', labelKey: 'nav.gear', icon: IconTool },
  { to: '/categories', labelKey: 'nav.categories', icon: IconCategory },
]

/**
 * Frame around all logged in pages: header, side navigation and the page content (<Outlet />).
 */
export default function AppLayout() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const [navbarOpened, { toggle: toggleNavbar, close: closeNavbar }] = useDisclosure()
  const { colorScheme, toggleColorScheme } = useMantineColorScheme()
  const { data: user } = useCurrentUser()
  const logout = useLogout()

  const handleLogout = () => {
    logout.mutate(undefined, { onSuccess: () => navigate('/login', { replace: true }) })
  }

  return (
    <AppShell
      header={{ height: 56 }}
      navbar={{ width: 220, breakpoint: 'sm', collapsed: { mobile: !navbarOpened } }}
      padding="md"
    >
      <AppShell.Header>
        <Group h="100%" px="md" justify="space-between" wrap="nowrap">
          <Group gap="xs" wrap="nowrap">
            <Burger opened={navbarOpened} onClick={toggleNavbar} hiddenFrom="sm" size="sm" />
            <IconMicrophone2 color="var(--mantine-primary-color-filled)" />
            <Title order={4}>{t('app.title')}</Title>
          </Group>

          <Group gap="sm" wrap="nowrap">
            <Text size="sm" c="dimmed" visibleFrom="sm">
              {user?.displayName}
            </Text>
            <LanguageSwitcher />
            <ActionIcon variant="default" size="lg" onClick={toggleColorScheme} aria-label={t('nav.toggleColorScheme')}>
              {colorScheme === 'dark' ? <IconSun size={18} /> : <IconMoon size={18} />}
            </ActionIcon>
          </Group>
        </Group>
      </AppShell.Header>

      <AppShell.Navbar p="sm">
        <AppShell.Section grow>
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              component={RouterNavLink}
              to={item.to}
              label={t(item.labelKey)}
              leftSection={<item.icon size={18} />}
              onClick={closeNavbar}
            />
          ))}
        </AppShell.Section>

        <AppShell.Section>
          <Button
            variant="subtle"
            color="gray"
            fullWidth
            justify="flex-start"
            leftSection={<IconLogout size={18} />}
            onClick={handleLogout}
          >
            {t('nav.logout')}
          </Button>
        </AppShell.Section>
      </AppShell.Navbar>

      <AppShell.Main>
        <Outlet />
      </AppShell.Main>
    </AppShell>
  )
}
