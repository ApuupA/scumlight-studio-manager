import { ActionIcon, Anchor, AppShell, Burger, Button, Group, NavLink, useMantineColorScheme } from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import { IconCalendarEvent, IconCategory, IconDisc, IconLogout, IconMoon, IconSettings, IconSun, IconTool, IconUser, IconUsersGroup } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { NavLink as RouterNavLink, Outlet, useNavigate } from 'react-router'
import { useCurrentUser, useLogout } from '../features/auth/authApi'
import classes from './AppLayout.module.css'
import Brand from './Brand'
import LanguageSwitcher from './LanguageSwitcher'

/** Entries of the side navigation. New features (projects, sessions, mixes, ...) are added here. */
const navItems = [
  { to: '/sessions', labelKey: 'nav.calendar', icon: IconCalendarEvent },
  { to: '/projects', labelKey: 'nav.projects', icon: IconDisc },
  { to: '/clients', labelKey: 'nav.clients', icon: IconUsersGroup },
  { to: '/gear', labelKey: 'nav.gear', icon: IconTool },
  { to: '/categories', labelKey: 'nav.categories', icon: IconCategory },
  { to: '/settings', labelKey: 'nav.settings', icon: IconSettings },
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

  // Language and light / dark switch - shown in the header on desktop and in the navigation on phones
  const settings = (
    <>
      <LanguageSwitcher />
      <ActionIcon variant="default" size="lg" onClick={toggleColorScheme} aria-label={t('nav.toggleColorScheme')}>
        {colorScheme === 'dark' ? <IconSun size={18} /> : <IconMoon size={18} />}
      </ActionIcon>
    </>
  )

  return (
    <AppShell
      header={{ height: 64 }}
      navbar={{ width: 220, breakpoint: 'sm', collapsed: { mobile: !navbarOpened } }}
      padding="md"
    >
      <AppShell.Header className={classes.header}>
        <Group h="100%" px="md" justify="space-between" wrap="nowrap">
          <Group gap="xs" wrap="nowrap">
            <Burger opened={navbarOpened} onClick={toggleNavbar} hiddenFrom="sm" size="sm" />
            <Brand />
          </Group>

          {/* On phones these settings move into the navigation (see below), so the header has room for the logo */}
          <Group gap="sm" wrap="nowrap" visibleFrom="sm">
            <Anchor component={RouterNavLink} to="/profile" size="sm" c="dimmed">
              {user?.displayName}
            </Anchor>
            {settings}
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
          <Group gap="sm" px="sm" pb="sm" hiddenFrom="sm">
            {settings}
          </Group>
          <NavLink
            component={RouterNavLink}
            to="/profile"
            label={t('nav.profile')}
            leftSection={<IconUser size={18} />}
            onClick={closeNavbar}
          />
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
