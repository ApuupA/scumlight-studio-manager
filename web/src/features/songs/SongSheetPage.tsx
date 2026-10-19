import { Button, Center, Loader } from '@mantine/core'
import { IconArrowLeft, IconPrinter } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useParams } from 'react-router'
import type { Song } from '../../api/types'
import logo from '../../assets/scumlight-logo-96.webp'
import classes from '../../components/PrintSheet.module.css'
import { useProject } from '../projects/projectApi'
import { formatTempos, tempoAtBar } from '../projects/songTempos'
import { useSong, useSongSections, useSongTracks } from './songApi'

/**
 * Printable song sheet (route /songs/:id/sheet): arrangement, recording checklist and lyrics - for the session
 * or the vocal booth. Print it with the button or Ctrl+P - the browser can also save it as PDF.
 */
export default function SongSheetPage() {
  const id = Number(useParams().id)
  const { data: song } = useSong(id)

  if (!song) return <Center h="100vh"><Loader /></Center>

  return <SongSheet song={song} />
}

function SongSheet({ song }: { song: Song }) {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { data: project } = useProject(song.projectId)
  const { data: sections } = useSongSections(song.id)
  const { data: tracks } = useSongTracks(song.id)

  const details = [
    project && `${project.clientName} · ${project.title}`,
    song.bandName,
    formatTempos(song.tempos, t) && `${formatTempos(song.tempos, t)} BPM`,
    song.tuning,
  ].filter(Boolean)

  return (
    <div className={classes.page}>
      <div className={classes.toolbar}>
        <Button variant="default" leftSection={<IconArrowLeft size={16} />} onClick={() => navigate(`/songs/${song.id}`)}>
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
          <span className={classes.label}>{t('songPage.sheet')}</span>
        </div>

        <h1>{song.trackNumber > 0 && `${song.trackNumber}. `}{song.title}</h1>
        <div className={classes.meta}>
          <div>{details.join(' · ')}</div>
          <div>{t(`songStatus.${song.status}`)}</div>
        </div>
        {song.notes && <p style={{ whiteSpace: 'pre-wrap' }}>{song.notes}</p>}

        {sections && sections.length > 0 && (
          <>
            <h2>{t('songPage.arrangement')}</h2>
            <table>
              <thead>
                <tr>
                  <th>{t('songPage.part')}</th>
                  <th>{t('songPage.bars')}</th>
                  <th>BPM</th>
                  <th>{t('common.notes')}</th>
                </tr>
              </thead>
              <tbody>
                {sections.map((section) => (
                  <tr key={section.id}>
                    <td><strong>{section.name}</strong></td>
                    <td>{(section.fromBar != null || section.toBar != null) && `${section.fromBar ?? '?'} – ${section.toBar ?? '?'}`}</td>
                    <td>{tempoAtBar(song.tempos, section.fromBar)}</td>
                    <td>{section.notes}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </>
        )}

        {tracks && tracks.length > 0 && (
          <>
            <h2>{t('songPage.recording')}</h2>
            <table>
              <tbody>
                {tracks.map((track) => (
                  <tr key={track.id}>
                    <td className={classes.check}>{track.status === 'Open' ? '☐' : '☑'}</td>
                    <td><strong>{track.name}</strong></td>
                    <td className={classes.muted}>{t(`songTrackStatus.${track.status}`)}</td>
                    <td>{track.notes}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </>
        )}

        {song.lyrics && (
          <>
            <h2>{t('songPage.lyrics')}</h2>
            <p style={{ whiteSpace: 'pre-wrap', lineHeight: 1.6 }}>{song.lyrics}</p>
          </>
        )}
      </div>
    </div>
  )
}
