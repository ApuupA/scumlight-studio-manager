import { Navigate, useParams } from 'react-router'

/**
 * Old band gear links (/band-gear/:token) open the tab "Equipment" of the client page - it has the same secret.
 */
export default function BandGearRedirect() {
  const token = useParams().token ?? ''
  return <Navigate to={`/client/${token}?tab=gear`} replace />
}
