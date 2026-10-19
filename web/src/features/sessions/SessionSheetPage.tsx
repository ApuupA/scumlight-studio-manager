import { Button, Center, Loader } from '@mantine/core'
import { IconArrowLeft, IconPrinter } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useParams } from 'react-router'
import type { Session } from '../../api/types'
import logo from '../../assets/scumlight-logo-96.webp'
import { noteFromHz } from '../../utils/format'
import { useDrumTunings, useExternalGear, useProject, useSongs } from '../projects/projectApi'
import { useProjectInstruments } from '../instruments/instrumentApi'
import SetupPrintTable from '../instruments/SetupPrintTable'
import { useSession, useSessionTasks } from './sessionApi'
import { formatSessionDates } from './sessionDates'
import classes from '../../components/PrintSheet.module.css'
import { formatTempos } from '../projects/songTempos'

/**
 * Printable session sheet (route /sessions/:id/sheet): everything needed on the session day on one page.
 * Print it with the button or Ctrl+P - the browser can also save it as PDF.
 */
export default function SessionSheetPage() {
  const id = Number(useParams().id)
  const { data: session } = useSession(id)

  if (!session) return <Center h="100vh"><Loader /></Center>

  return <SessionSheet session={session} />
}

function SessionSheet({ session }: { session: Session }) {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const { data: project } = useProject(session.projectId)
  const { data: tasks } = useSessionTasks(session.id)
  const { data: projectInstruments } = useProjectInstruments(session.projectId)
  const { data: allSongs } = useSongs(session.projectId)
  const externalGear = useExternalGear(session.projectId).data?.filter((gear) => !gear.isSuggestion) // open suggestions are not printed
  const { data: drumTunings } = useDrumTunings(session.projectId)

  // Only the songs worked on in this session - or all songs of the project if none are selected
  const songs = (allSongs ?? []).filter((song) => session.songIds.length === 0 || session.songIds.includes(song.id))

  // The instruments recorded in this session - their setups are the gear to set up
  const sessionInstrumentIds = session.instruments.map((instrument) => instrument.id)
  const instruments = (projectInstruments ?? []).filter((instrument) => sessionInstrumentIds.includes(instrument.id))

  // Drum tuning: project-wide values plus those of the songs in this session
  const tunings = (drumTunings ?? []).filter((tuning) => !tuning.songId || songs.some((song) => song.id === tuning.songId))

  return (
    <div className={classes.page}>
      <div className={classes.toolbar}>
        <Button variant="default" leftSection={<IconArrowLeft size={16} />} onClick={() => navigate(`/sessions/${session.id}`)}>
          {t('common.back')}
        </Button>
        <Button leftSection={<IconPrinter size={16} />} onClick={() => window.print()}>{t('common.print')}</Button>
      </div>

      <div className={classes.sheet}>
        <div className={classes.header}>
          <div className={classes.brand}>
            <img src={logo} alt="" />
            {t('app.studioName')}
          </div>
          <span className={classes.label}>{t('sheet.title')}</span>
        </div>

        <h1>{session.title}</h1>
        <div className={classes.meta}>
          <div><strong>{formatSessionDates(session, i18n.language, t('common.allDay'), t('sessions.daily'))}</strong></div>
          <div>{t(`sessionType.${session.type}`)} · {t(`sessionStatus.${session.status}`)}</div>
          <div>
            {t('sessions.project')}: <strong>{session.projectTitle}</strong> ({session.clientName})
          </div>
          <div>{project?.bands.map((band) => band.name).join(', ')}</div>
          {session.engineer && <div>{t('sessions.engineer')}: {session.engineer}</div>}
          {session.attendees && <div>{t('sessions.attendees')}: {session.attendees}</div>}
          {project && (project.sampleRate || project.bitDepth || project.daw) && (
            <div className={classes.muted}>
              {[project.sampleRate && `${project.sampleRate / 1000} kHz`, project.bitDepth && `${project.bitDepth} bit`, project.daw]
                .filter(Boolean).join(' · ')}
            </div>
          )}
        </div>

        {session.goals && (
          <>
            <h2>{t('sessions.goals')}</h2>
            <div style={{ whiteSpace: 'pre-wrap' }}>{session.goals}</div>
          </>
        )}

        {(tasks ?? []).length > 0 && (
          <>
            <h2>{t('tasks.title')}</h2>
            <table>
              <tbody>
                {tasks?.map((task) => (
                  <tr key={task.id}>
                    <td className={classes.check}>{task.isDone ? '☑' : '☐'}</td>
                    <td>{task.text}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </>
        )}

        {songs.length > 0 && (
          <>
            <h2>{t('projects.tabs.songs')}</h2>
            <table>
              <thead>
                <tr>
                  <th>{t('songs.trackNumber')}</th>
                  <th>{t('songs.songTitle')}</th>
                  {(project?.bands.length ?? 0) > 1 && <th>{t('songs.band')}</th>}
                  <th>{t('songs.bpm')}</th>
                  <th>{t('songs.tuning')}</th>
                  <th>{t('songs.status')}</th>
                </tr>
              </thead>
              <tbody>
                {songs.map((song) => (
                  <tr key={song.id}>
                    <td>{song.trackNumber}</td>
                    <td><strong>{song.title}</strong></td>
                    {(project?.bands.length ?? 0) > 1 && <td>{song.bandName}</td>}
                    <td>{formatTempos(song.tempos, t)}</td>
                    <td>{song.tuning}</td>
                    <td>{t(`songStatus.${song.status}`)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </>
        )}

        <div className={classes.columns}>
          <div>
            {instruments.length > 0 && (
              <>
                <h2>{t('sheet.gear')}</h2>
                <SetupPrintTable instruments={instruments} />
              </>
            )}
          </div>

          <div>
            {(externalGear ?? []).length > 0 && (
              <>
                <h2>{t('sheet.bandGear')}</h2>
                <table>
                  <tbody>
                    {externalGear?.map((gear) => (
                      <tr key={gear.id}>
                        <td className={classes.check}>☐</td>
                        <td>{gear.quantity}×</td>
                        <td>{gear.name}{gear.owner && <span className={classes.muted}> · {gear.owner}</span>}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </>
            )}

            {tunings.length > 0 && (
              <>
                <h2>{t('projects.tabs.drums')}</h2>
                <table>
                  <thead>
                    <tr>
                      <th>{t('drums.drum')}</th>
                      <th>{t('drums.batter')}</th>
                      <th>{t('drums.resonant')}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {tunings.map((tuning) => (
                      <tr key={tuning.id}>
                        <td>{tuning.drum}{tuning.songTitle && <span className={classes.muted}> ({tuning.songTitle})</span>}</td>
                        <td>{tuning.batterHz && `${tuning.batterHz} Hz · ${noteFromHz(tuning.batterHz)}`}</td>
                        <td>{tuning.resonantHz && `${tuning.resonantHz} Hz · ${noteFromHz(tuning.resonantHz)}`}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </>
            )}
          </div>
        </div>

        {/* Space to write on during the session - pre-filled if notes already exist */}
        <h2>{t('sessions.summary')} / {t('common.notes')}</h2>
        <div className={classes.writeArea}>
          {[session.summary, session.nextSteps, session.notes].filter(Boolean).join('\n\n')}
        </div>
      </div>
    </div>
  )
}
