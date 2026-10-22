import { Button, Center, Loader } from '@mantine/core'
import { DatePickerInput } from '@mantine/dates'
import { IconArrowLeft, IconCalendar, IconPrinter } from '@tabler/icons-react'
import { useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useParams, useSearchParams } from 'react-router'
import logo from '../../assets/scumlight-logo-96.webp'
import classes from '../../components/PrintSheet.module.css'
import { formatDate, formatHours, parseDate } from '../../utils/format'
import { useProject } from '../projects/projectApi'
import { useSessionsOfProject } from './sessionApi'
import { buildTimetable, defaultTimetableRange, timetableColors, type TimetableDay } from './timetable'

/** Height of one hour in the table. */
const hourHeight = 26

/**
 * Printable session timetable of a project (route /projects/:id/timetable?from=...&to=...): one column per day,
 * every session as a block at its time, hours per day and in total - an overview for the band.
 * The range is chosen in the toolbar, so the drum weekend can be printed without the guitars a month later.
 */
export default function TimetablePage() {
  const projectId = Number(useParams().id)
  const { data: project } = useProject(projectId)
  const { data: sessions } = useSessionsOfProject(projectId)
  const [searchParams, setSearchParams] = useSearchParams()

  // The timetable is wide: landscape - only while this page is open
  useEffect(() => {
    const style = document.createElement('style')
    style.textContent = '@page { size: A4 landscape; margin: 10mm; }'
    document.head.appendChild(style)
    return () => style.remove()
  }, [])

  if (!project || !sessions) return <Center h="100vh"><Loader /></Center>

  const suggested = defaultTimetableRange(sessions)
  const from = searchParams.get('from') ?? suggested.from
  const to = searchParams.get('to') ?? suggested.to

  return (
    <TimetableSheet
      projectId={projectId}
      title={project.title}
      subtitle={[project.clientName, ...project.bands.map((b) => b.name)].filter((v, i, all) => all.indexOf(v) === i).join(' · ')}
      timetable={buildTimetable(sessions, from, to)}
      from={from}
      to={to}
      onRangeChange={(newFrom, newTo) => setSearchParams({ from: newFrom, to: newTo }, { replace: true })}
    />
  )
}

interface TimetableSheetProps {
  projectId: number
  title: string
  subtitle: string
  timetable: ReturnType<typeof buildTimetable>
  from: string
  to: string
  onRangeChange: (from: string, to: string) => void
}

function TimetableSheet({ projectId, title, subtitle, timetable, from, to, onRangeChange }: TimetableSheetProps) {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const { days, startHour, endHour, totalHours } = timetable
  const hours = Array.from({ length: endHour - startHour }, (_, i) => startHour + i)
  const hasAllDay = days.some((d) => d.allDay.length > 0)

  const dayLabel = (day: string) =>
    new Intl.DateTimeFormat(i18n.language, { weekday: 'short', day: '2-digit', month: '2-digit' }).format(parseDate(day))
  const hourLabel = (hour: number) => `${String(hour).padStart(2, '0')}:00 – ${String((hour + 1) % 24).padStart(2, '0')}:00`
  const timeLabel = (minutes: number) => `${String(Math.floor(minutes / 60) % 24).padStart(2, '0')}:${String(minutes % 60).padStart(2, '0')}`

  return (
    <div className={classes.page}>
      <div className={classes.toolbar} style={{ maxWidth: '297mm', alignItems: 'center', gap: 12 }}>
        <Button variant="default" leftSection={<IconArrowLeft size={16} />} onClick={() => navigate(`/projects/${projectId}?tab=sessions`)}>
          {t('common.back')}
        </Button>
        <DatePickerInput
          type="range"
          aria-label={t('timetable.range')}
          leftSection={<IconCalendar size={16} />}
          valueFormat="L"
          w={260}
          allowSingleDateInRange
          value={[from, to]}
          // Only take over a complete range (the first click only sets the start)
          onChange={([newFrom, newTo]) => newFrom && newTo && onRangeChange(newFrom, newTo)}
        />
        <Button leftSection={<IconPrinter size={16} />} onClick={() => window.print()}>{t('common.print')}</Button>
      </div>

      <div className={classes.sheet} style={{ maxWidth: '297mm' }}>
        <div className={classes.header}>
          <div className={classes.brand}>
            <img src={logo} alt="" />
            {t('app.studioName')}
          </div>
          <span className={classes.label}>{t('timetable.title')}</span>
        </div>

        <h1>{title}</h1>
        <div className={classes.meta}>
          <div>{subtitle}</div>
          <div>{formatDate(from, i18n.language)} – {formatDate(to, i18n.language)}</div>
        </div>

        {days.length === 0 ? (
          <p className={classes.muted}>{t('timetable.empty')}</p>
        ) : (
          <div
            className={classes.timetable}
            style={{ gridTemplateColumns: `24mm repeat(${days.length}, 1fr)` }}
          >
            {/* Header: the days */}
            <div className={classes.timetableHead}>{t('timetable.time')}</div>
            {days.map((d) => <div key={d.day} className={classes.timetableHead}>{dayLabel(d.day)}</div>)}

            {/* Sessions without times */}
            {hasAllDay && (
              <>
                <div className={classes.timetableHour}>{t('common.allDay')}</div>
                {days.map((d) => (
                  <div key={d.day} className={classes.timetableAllDay}>
                    {d.allDay.map((s) => (
                      <div key={s.id} className={classes.timetableBlock} style={{ position: 'static', background: timetableColors[s.type] }}>
                        <strong>{s.title}</strong>
                      </div>
                    ))}
                  </div>
                ))}
              </>
            )}

            {/* The hours: labels on the left, one column per day with the blocks */}
            <div>
              {hours.map((hour) => (
                <div key={hour} className={classes.timetableHour} style={{ height: hourHeight }}>{hourLabel(hour)}</div>
              ))}
            </div>
            {days.map((d) => (
              <DayColumn key={d.day} day={d} startHour={startHour} hourCount={hours.length} timeLabel={timeLabel} />
            ))}

            {/* Footer: hours per day and in total */}
            <div className={classes.timetableHead}>{t('timetable.hours')}</div>
            {days.map((d) => (
              <div key={d.day} className={classes.timetableHead}>{d.hours > 0 ? formatHours(d.hours, i18n.language) : '–'}</div>
            ))}
            <div className={classes.timetableHead} style={{ gridColumn: `1 / span ${days.length + 1}` }}>
              {t('timetable.total')}: {formatHours(totalHours, i18n.language)}
            </div>
          </div>
        )}
      </div>
    </div>
  )
}

/** One day: hour lines in the background, the sessions as blocks at their time. */
function DayColumn({ day, startHour, hourCount, timeLabel }: {
  day: TimetableDay
  startHour: number
  hourCount: number
  timeLabel: (minutes: number) => string
}) {
  const top = (minutes: number) => ((minutes - startHour * 60) / 60) * hourHeight

  return (
    <div className={classes.timetableDay} style={{ height: hourCount * hourHeight }}>
      {Array.from({ length: hourCount }, (_, i) => (
        <div key={i} className={classes.timetableLine} style={{ top: i * hourHeight, height: hourHeight }} />
      ))}
      {day.blocks.map((block) => (
        <div
          key={block.session.id}
          className={classes.timetableBlock}
          style={{
            top: top(block.start),
            height: top(block.end) - top(block.start),
            left: `${(block.lane / block.laneCount) * 100}%`,
            width: `${100 / block.laneCount}%`,
            background: timetableColors[block.session.type],
          }}
        >
          <div className={classes.timetableTime}>{timeLabel(block.start)} – {timeLabel(block.end)}</div>
          <strong>{block.session.title}</strong>
          {block.session.goals && <div className={classes.timetableGoals}>{block.session.goals}</div>}
        </div>
      ))}
    </div>
  )
}
