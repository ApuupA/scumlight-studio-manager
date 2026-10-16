import { Button, Center, Loader } from '@mantine/core'
import { IconArrowLeft, IconPrinter } from '@tabler/icons-react'
import { useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useParams, useSearchParams } from 'react-router'
import type { InputList } from '../../api/types'
import logo from '../../assets/scumlight-logo-96.webp'
import { printColor } from '../../components/inputGroupColors'
import classes from '../../components/PrintSheet.module.css'
import { useProject } from '../projects/projectApi'
import { useSession } from '../sessions/sessionApi'
import { formatSessionDates } from '../sessions/sessionDates'
import { useInputList } from './inputListApi'

/**
 * Printable input sheet (route /input-lists/:id/sheet, optionally ?sessionId=...), laid out like the Excel input list:
 * header with session / client / date / engineer, one colored line per studio input. Printed in landscape.
 */
export default function InputSheetPage() {
  const id = Number(useParams().id)
  const { data: list } = useInputList(id)

  // The input sheet needs a wide page: landscape - only while this page is open, the other sheets stay portrait
  useEffect(() => {
    const style = document.createElement('style')
    style.textContent = '@page { size: A4 landscape; margin: 10mm; }'
    document.head.appendChild(style)
    return () => style.remove()
  }, [])

  if (!list) return <Center h="100vh"><Loader /></Center>

  return <InputSheet list={list} />
}

/** ☑ / ☐ for the switch columns. */
const check = (value: boolean) => (value ? '☑' : '☐')

function InputSheet({ list }: { list: InputList }) {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const sessionId = Number(searchParams.get('sessionId')) || undefined
  const { data: session } = useSession(sessionId)
  const { data: project } = useProject(list.projectId)

  const backTo = sessionId ? `/sessions/${sessionId}` : `/projects/${list.projectId}?tab=inputLists`

  return (
    <div className={classes.page}>
      <div className={classes.toolbar} style={{ maxWidth: '297mm' }}>
        <Button variant="default" leftSection={<IconArrowLeft size={16} />} onClick={() => navigate(backTo)}>
          {t('common.back')}
        </Button>
        <Button leftSection={<IconPrinter size={16} />} onClick={() => window.print()}>{t('common.print')}</Button>
      </div>

      <div className={classes.sheet} style={{ maxWidth: '297mm', fontSize: '9pt' }}>
        <div className={classes.inputSheetHeader}>
          <img src={logo} alt="" />
          <div><span className={classes.label}>{t('inputList.sheetSession')}</span>{session?.title ?? list.name}</div>
          <div><span className={classes.label}>{t('projects.client')}</span>{project?.clientName} · {project?.title}</div>
          <div>
            <span className={classes.label}>{t('sessions.date')}</span>
            {session ? formatSessionDates(session, i18n.language, t('common.allDay'), t('sessions.daily')) : ''}
          </div>
          <div><span className={classes.label}>{t('sessions.engineer')}</span>{session?.engineer}</div>
        </div>

        <table className={classes.inputTable}>
          <thead>
            <tr>
              <th>#</th>
              <th>{t('inputList.source')}</th>
              <th>{t('inputList.microphone')}</th>
              <th>{t('inputList.stand')}</th>
              <th>{t('inputList.snake')}</th>
              <th>{t('inputList.preamp')}</th>
              <th>{t('inputList.phantom')}</th>
              <th>{t('inputList.hpf')}</th>
              <th>{t('inputList.tone')}</th>
              <th>{t('inputList.eq')}</th>
              <th>{t('inputList.insert')}</th>
              <th>{t('inputList.dawChannel')}</th>
              <th>{t('inputList.comment')}</th>
            </tr>
          </thead>
          <tbody>
            {list.rows.map((row) => (
              <tr key={row.studioInputId} style={{ background: printColor(row.inputGroupColor) }}>
                <td>{row.number}</td>
                <td><strong>{row.source}</strong></td>
                <td>{row.microphone}</td>
                <td>{row.stand}</td>
                <td>{row.snakeChannel}</td>
                <td>{row.preamp}</td>
                <td className={classes.check}>{check(row.phantom)}</td>
                <td className={classes.check}>{check(row.hpf)}</td>
                <td className={classes.check}>{check(row.tone)}</td>
                <td className={classes.check}>{check(row.eq)}</td>
                <td>{row.insert}</td>
                <td>{row.dawChannel}</td>
                <td>{row.comment}</td>
              </tr>
            ))}
          </tbody>
        </table>
        {list.notes && <p className={classes.muted}>{list.notes}</p>}
      </div>
    </div>
  )
}
