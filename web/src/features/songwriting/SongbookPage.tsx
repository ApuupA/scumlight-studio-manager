import { Button, Center, Loader } from '@mantine/core'
import { IconArrowLeft, IconPrinter } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useParams } from 'react-router'
import type { Song, VoiceMemo } from '../../api/types'
import logo from '../../assets/scumlight-logo-96.webp'
import classes from '../../components/PrintSheet.module.css'
import { useProject, useSongs } from '../projects/projectApi'
import { formatTempos, tempoAtBar } from '../projects/songTempos'
import { useSongSections } from '../songs/songApi'
import { useVoiceMemos } from './songwritingApi'

/**
 * Printable songbook of a project (route /projects/:id/songbook) for the rehearsal room: an overview of all songs,
 * then every song on its own page with tempo, tuning, arrangement (with the voice memos of each part) and lyrics.
 * Print it with the button or Ctrl+P - the browser can also save it as PDF.
 */
export default function SongbookPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const projectId = Number(useParams().id)
  const { data: project } = useProject(projectId)
  const { data: songs } = useSongs(projectId)
  const { data: memos } = useVoiceMemos(projectId)

  if (!project || !songs) return <Center h="100vh"><Loader /></Center>

  return (
    <div className={classes.page}>
      <div className={classes.toolbar}>
        <Button variant="default" leftSection={<IconArrowLeft size={16} />} onClick={() => navigate(`/projects/${projectId}?tab=songwriting`)}>
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
          <span className={classes.label}>{t('songbook.title')}</span>
        </div>

        <h1>{project.title}</h1>
        <div className={classes.meta}>
          <div>{[project.clientName, ...project.bands.map((b) => b.name)].join(' · ')}</div>
          <div>{t('songbook.songCount', { count: songs.length })}</div>
        </div>

        {/* Overview: the set list with the most important facts */}
        <h2>{t('songbook.overview')}</h2>
        <table>
          <thead>
            <tr>
              <th>#</th>
              <th>{t('songwriting.song')}</th>
              <th>BPM</th>
              <th>{t('songbook.tuning')}</th>
              <th>{t('songs.status')}</th>
            </tr>
          </thead>
          <tbody>
            {songs.map((song) => (
              <tr key={song.id}>
                <td>{song.trackNumber > 0 && song.trackNumber}</td>
                <td><strong>{song.title}</strong></td>
                <td>{formatTempos(song.tempos, t)}</td>
                <td>{song.tuning}</td>
                <td className={classes.muted}>{t(`songStatus.${song.status}`)}</td>
              </tr>
            ))}
          </tbody>
        </table>

        {songs.map((song) => (
          <SongbookSong key={song.id} song={song} memos={(memos ?? []).filter((m) => m.songId === song.id)} />
        ))}
      </div>
    </div>
  )
}

/** One song of the songbook - starts on a new page when printed. */
function SongbookSong({ song, memos }: { song: Song; memos: VoiceMemo[] }) {
  const { t } = useTranslation()
  const { data: sections } = useSongSections(song.id)

  const details = [
    formatTempos(song.tempos, t) && `${formatTempos(song.tempos, t)} BPM`,
    song.tuning,
    song.bandName,
  ].filter(Boolean)

  // Memos without a part are listed below the arrangement
  const memosWithoutPart = memos.filter((m) => m.songSectionId == null)
  const memoLabel = (memo: VoiceMemo) => (memo.notes ? `${memo.title} – ${memo.notes}` : memo.title)

  return (
    <div className={classes.songbookSong}>
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
                {memos.length > 0 && <th>{t('songwriting.memos')}</th>}
              </tr>
            </thead>
            <tbody>
              {sections.map((section) => (
                <tr key={section.id}>
                  <td><strong>{section.name}</strong></td>
                  <td>{(section.fromBar != null || section.toBar != null) && `${section.fromBar ?? '?'} – ${section.toBar ?? '?'}`}</td>
                  <td>{tempoAtBar(song.tempos, section.fromBar)}</td>
                  <td>{section.notes}</td>
                  {memos.length > 0 && (
                    <td className={classes.muted}>
                      {memos.filter((m) => m.songSectionId === section.id).map(memoLabel).join('; ')}
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </>
      )}

      {memosWithoutPart.length > 0 && (
        <>
          <h2>{t('songwriting.memos')}</h2>
          <ul>
            {memosWithoutPart.map((memo) => <li key={memo.id}>{memoLabel(memo)}</li>)}
          </ul>
        </>
      )}

      {song.lyrics && (
        <>
          <h2>{t('songPage.lyrics')}</h2>
          <p style={{ whiteSpace: 'pre-wrap', lineHeight: 1.6 }}>{song.lyrics}</p>
        </>
      )}
    </div>
  )
}
