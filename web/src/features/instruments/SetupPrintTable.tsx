import { useTranslation } from 'react-i18next'
import type { ProjectInstrument, SetupItem } from '../../api/types'
import classes from '../../components/PrintSheet.module.css'

function micsFirst(items: SetupItem[]) {
  return [...items.filter((item) => item.needsInput), ...items.filter((item) => !item.needsInput)]
}

/**
 * The setups of instruments for the printable sheets: one block per instrument, one row per entry
 * with a checkbox for the setup. Chains are numbered with arrows (1 → 2 → 3) to show the signal flow.
 */
export default function SetupPrintTable({ instruments }: { instruments: ProjectInstrument[] }) {
  const { t } = useTranslation()

  return (
    <table>
      <tbody>
        {instruments.map((instrument) => [
          <tr key={`instrument-${instrument.id}`}>
            <td colSpan={4} className={classes.category}>
              {instrument.name}
              {instrument.bandName && <span className={classes.muted}> · {instrument.bandName}</span>}
            </td>
          </tr>,
          // A chain keeps its order (signal flow), a list shows the mics / DIs first - like on the project page
          ...(instrument.isChain ? instrument.setupItems : micsFirst(instrument.setupItems)).map((item, index) => (
            <tr key={item.id}>
              <td className={classes.check}>☐</td>
              <td>{instrument.isChain ? (index === 0 ? '' : '→') : `${item.quantity}×`}</td>
              <td>
                {instrument.isChain && item.quantity > 1 && `${item.quantity}× `}
                {item.displayName}
                {item.source === 'Band' && <span className={classes.muted}> ({item.externalGearOwner ?? t('setupSource.Band')})</span>}
                {item.needsInput && ' 🎤'}
              </td>
              <td className={classes.muted}>{item.usedFor}</td>
            </tr>
          )),
        ])}
      </tbody>
    </table>
  )
}
