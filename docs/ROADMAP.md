# Roadmap

## ✅ Milestone 1 – Foundation & gear inventory
- Layered ASP.NET Core API, PostgreSQL, EF Core migrations, Docker Compose
- Login with cookie (owner account), user roles prepared for clients
- Gear inventory: items with category, status, quantity, serial number, location, purchase data, notes, photo
- Editable categories (seeded with sensible defaults)
- Search, filters and sorting in the gear list
- German / English UI, light / dark mode
- Typed API client generated from Swagger, unit tests

## ✅ Milestone 2 – Projects, sessions & gear booking
- Clients with bands; a project belongs to a client and can include several of its bands (split releases)
- Projects with type, status, deadline, technical settings (sample rate, bit depth, DAW, target LUFS)
  and notes per phase (recording, mixing, mastering)
- Songs with band, BPM, key, tuning and status (planned → recording → editing → mixing → mastering → done)
- References & rough mixes as links, drum tuning (TuneBot Hz shown as note), gear brought by the band
- Sessions as all-day or time range, with to-do list, worked-on songs, goals / where we are / next steps
- Calendar view (month) with session list
- Gear booking per session with quantities; double bookings and unavailable gear only **warn**, never block
- "In session today" in the gear list, upcoming bookings on the gear page
- Printable session sheet (to-dos, songs, gear to set up, band gear, drum tuning)

## Milestone 2 – revision after the first real test
**✅ Phase A – quick wins**
- Calendar lists all upcoming sessions (grouped by month), overdue sessions on top, done / cancelled collapsed
- Change the session status directly in the list
- Duplicate a session to one or many dates (series of recording days), "copy from" when creating a session
- Multi-day sessions (e.g. Fri – Sun, 9:00 – 17:00 daily)
- Set the status of several songs at once; song key removed
- "Good for" tags on gear (shown in the list, searchable)

**✅ Phase B – instruments & setup** (replaces the per-session gear booking)
- Recorded instruments per project, each with its setup (studio gear, band gear or free text + "used for", quantity)
- Signal chains for guitars / bass (e.g. Les Paul → Tube Screamer → JCM800 → 1960A → SM57), lists for drums
- Gear suggestions via "good for" tags; mics / DIs marked as "needs an input" (basis for the input lists)
- Sessions pick instruments and inherit their gear (also when creating / duplicating a session)
- Setup templates (save an instrument setup, create new instruments from it)
- "In session today" and "used in projects" on the gear pages; gear used in a setup can not be deleted
- Printable project sheet

**✅ Phase C – input lists**
- Settings page: studio inputs configured once (snake channel, preamp + group color, DAW channel), several at once with numbering
- Several input lists per project (tabs), pre-filled from the instrument setups; +48V / HPF / Tone / EQ, insert, comment
- Sessions pick their input list; printable input sheet

**✅ Feedback round after the first real use**
- Input lists: a pair (quantity 2, e.g. overheads) becomes "L" / "R"; source, microphone, stand and +48V can
  be dragged to another input (e.g. the snare to another preamp) – the channel settings (HPF, EQ, ...) stay
- Setups: lists show "mics / DIs" and "other gear" separately; chains highlight the mics and show the notes
- Songs: several tempos per song (tempo changes), optionally from which bar


## Milestone 3 – Mix revision tool
- Upload mix versions per song (WAV/MP3), server-side conversion for streaming (FFmpeg)
- Waveform player (wavesurfer.js), seamless A/B switching between versions at the same position
- Timestamped comments with replies, open / resolved state
- Share link for clients without account, approval per song
- Optional loudness matching between versions

## Milestone 4 – Patchbay & outboard
- Input lists moved to Milestone 2, phase C
- Patchbay and outboard inserts in the studio I/O, reusable input list templates

## Milestone 5 – Documents & templates
- File storage per project (lyrics, charts, references, contracts)
- Printable templates filled with session data: take sheets, track sheets, recall sheets

## Ideas for later
- Status history of songs (who changed what when)
- Client accounts with access to their own projects only
- Maintenance log per gear item (tube swaps, repairs, recalibration)
- QR code labels for gear
- Time tracking & invoicing per project
- Gear wishlist / budget
