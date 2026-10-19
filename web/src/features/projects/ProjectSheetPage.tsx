import { Button, Center, Loader } from '@mantine/core'
import { IconArrowLeft, IconPrinter } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useParams } from 'react-router'
import type { Project } from '../../api/types'
import logo from '../../assets/scumlight-logo-96.webp'
import classes from '../../components/PrintSheet.module.css'
import { formatDate, noteFromHz } from '../../utils/format'
import { useProjectInstruments } from '../instruments/instrumentApi'
import SetupPrintTable from '../instruments/SetupPrintTable'
import { formatSessionDates } from '../sessions/sessionDates'
import { useSessionsOfProject } from '../sessions/sessionApi'
import { useDrumTunings, useExternalGear, useProject, useProjectLinks, useSongs } from './projectApi'
import { formatTempos } from './songTempos'

/**
 * Printable project sheet (route /projects/:id/sheet): everything about the project on paper -
 * songs, instruments with their setups, band gear, drum tuning, references, sessions and notes.
 * Print it with the button or Ctrl+P - the browser can also save it as PDF.
 */
export default function ProjectSheetPage() {
  const id = Number(useParams().id)
  const { data: project } = useProject(id)

  if (!project) return <Center h="100vh"><Loader /></Center>

  return <ProjectSheet project={project} />
}

function ProjectSheet({ project }: { project: Project }) {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const { data: songs } = useSongs(project.id)
  const { data: instruments } = useProjectInstruments(project.id)
  const bandGear = useExternalGear(project.id).data?.filter((gear) => !gear.isSuggestion) // open suggestions are not printed
  const { data: drumTunings } = useDrumTunings(project.id)
  const { data: links } = useProjectLinks(project.id)
  const { data: sessions } = useSessionsOfProject(project.id)

  const technical = [
    project.sampleRate && `${project.sampleRate / 1000} kHz`,
    project.bitDepth && `${project.bitDepth} bit`,
    project.daw,
    project.targetLufs != null && `${t('projects.targetLufs')}: ${project.targetLufs} LUFS`,
  ].filter(Boolean)

  const notes = [
    [t('projects.recordingNotes'), project.recordingNotes],
    [t('projects.mixingNotes'), project.mixingNotes],
    [t('projects.masteringNotes'), project.masteringNotes],
  ].filter(([, text]) => text)

  return (
    <div className={classes.page}>
      <div className={classes.toolbar}>
        <Button variant="default" leftSection={<IconArrowLeft size={16} />} onClick={() => navigate(`/projects/${project.id}`)}>
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
          <span className={classes.label}>{t('projectSheet.title')}</span>
        </div>

        <h1>{project.title}</h1>
        <div className={classes.meta}>
          <div><strong>{project.clientName}</strong>{project.bands.length > 0 && ` · ${project.bands.map((b) => b.name).join(', ')}`}</div>
          <div>{t(`projectType.${project.type}`)} · {t(`projectStatus.${project.status}`)}</div>
          {(project.startDate || project.dueDate) && (
            <div>
              {project.startDate && `${t('projects.startDate')}: ${formatDate(project.startDate, i18n.language)}`}
              {project.startDate && project.dueDate && ' · '}
              {project.dueDate && `${t('projects.dueDate')}: ${formatDate(project.dueDate, i18n.language)}`}
            </div>
          )}
          {technical.length > 0 && <div className={classes.muted}>{technical.join(' · ')}</div>}
        </div>
        {project.description && <p style={{ whiteSpace: 'pre-wrap' }}>{project.description}</p>}

        {(songs ?? []).length > 0 && (
          <>
            <h2>{t('projects.tabs.songs')}</h2>
            <table>
              <thead>
                <tr>
                  <th>{t('songs.trackNumber')}</th>
                  <th>{t('songs.songTitle')}</th>
                  {project.bands.length > 1 && <th>{t('songs.band')}</th>}
                  <th>{t('songs.bpm')}</th>
                  <th>{t('songs.tuning')}</th>
                  <th>{t('songs.status')}</th>
                </tr>
              </thead>
              <tbody>
                {songs?.map((song) => (
                  <tr key={song.id}>
                    <td>{song.trackNumber}</td>
                    <td><strong>{song.title}</strong></td>
                    {project.bands.length > 1 && <td>{song.bandName}</td>}
                    <td>{formatTempos(song.tempos, t)}</td>
                    <td>{song.tuning}</td>
                    <td>{t(`songStatus.${song.status}`)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </>
        )}

        {(instruments ?? []).length > 0 && (
          <>
            <h2>{t('projects.tabs.instruments')}</h2>
            <SetupPrintTable instruments={instruments ?? []} />
          </>
        )}

        <div className={classes.columns}>
          <div>
            {(bandGear ?? []).length > 0 && (
              <>
                <h2>{t('sheet.bandGear')}</h2>
                <table>
                  <tbody>
                    {bandGear?.map((gear) => (
                      <tr key={gear.id}>
                        <td>{gear.quantity}×</td>
                        <td>{gear.name}{gear.owner && <span className={classes.muted}> · {gear.owner}</span>}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </>
            )}
          </div>
          <div>
            {(drumTunings ?? []).length > 0 && (
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
                    {drumTunings?.map((tuning) => (
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

        {(sessions ?? []).length > 0 && (
          <>
            <h2>{t('projects.tabs.sessions')}</h2>
            <table>
              <tbody>
                {sessions?.map((session) => (
                  <tr key={session.id}>
                    <td>{formatSessionDates(session, i18n.language, t('common.allDay'), t('sessions.daily'))}</td>
                    <td><strong>{session.title}</strong></td>
                    <td>{t(`sessionType.${session.type}`)}</td>
                    <td className={classes.muted}>{t(`sessionStatus.${session.status}`)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </>
        )}

        {(links ?? []).length > 0 && (
          <>
            <h2>{t('projects.tabs.links')}</h2>
            <table>
              <tbody>
                {links?.map((link) => (
                  <tr key={link.id}>
                    <td>{t(`projectLinkType.${link.type}`)}</td>
                    <td><strong>{link.title}</strong>{link.songTitle && <span className={classes.muted}> ({link.songTitle})</span>}</td>
                    <td className={classes.muted} style={{ wordBreak: 'break-all' }}>{link.url}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </>
        )}

        {notes.length > 0 && (
          <>
            <h2>{t('projects.productionNotes')}</h2>
            {notes.map(([title, text]) => (
              <div key={title} style={{ marginBottom: 8 }}>
                <strong>{title}</strong>
                <div style={{ whiteSpace: 'pre-wrap' }}>{text}</div>
              </div>
            ))}
          </>
        )}
      </div>
    </div>
  )
}
