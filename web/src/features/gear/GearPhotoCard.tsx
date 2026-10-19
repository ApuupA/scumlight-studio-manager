import type { GearItem } from '../../api/types'
import PhotoCard from '../../components/PhotoCard'

/** The photo of a gear item with buttons to upload / replace / remove it. */
export default function GearPhotoCard({ item }: { item: GearItem }) {
  return <PhotoCard owner="GearItem" entity={item} invalidateKeys={['gearItems', 'categories']} alt={item.name} height={260} />
}
