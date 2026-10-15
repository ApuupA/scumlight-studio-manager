import { Center, Loader } from '@mantine/core'
import { Navigate, Outlet } from 'react-router'
import { useCurrentUser } from './authApi'

/**
 * Guards all inner routes: shows them only for a logged in user, otherwise redirects to /login.
 * <Outlet /> is the placeholder where the matching child route is rendered.
 */
export default function RequireAuth() {
  const { data: user, isPending } = useCurrentUser()

  if (isPending) {
    return (
      <Center h="100vh">
        <Loader />
      </Center>
    )
  }

  return user ? <Outlet /> : <Navigate to="/login" replace />
}
