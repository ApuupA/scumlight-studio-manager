import { Navigate, Outlet } from 'react-router'
import { useIsOwner } from './authApi'

/**
 * Guards the pages only the owner may see (finance, settings). An assistant who opens such an address
 * (e.g. an old bookmark) lands on the start page. Used inside RequireAuth, so the user is already loaded.
 */
export default function RequireOwner() {
  const isOwner = useIsOwner()
  return isOwner ? <Outlet /> : <Navigate to="/" replace />
}
