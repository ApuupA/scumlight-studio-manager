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


## ✅ v1.0.0 – in production (2026-10-19)
- Running at studio.scumlight-cloud.de; more users, login limit, secure cookie
- Update / backup / restore scripts and a release checklist ([RELEASE_CHECKLIST.md](RELEASE_CHECKLIST.md))

## Milestone 3 – Basics for the daily work (in this order)

**✅ 1. Song page** (v1.1.0)
- One page per song: arrangement (parts like "Chorus 1", bars, notes like "needs doubled guitars")
- Recording checklist: the tracks of the song (Drums, Guitar L/R, Lead vocal, ...) with status open / recorded / edited,
  pre-filled from the project's instruments or copied from another song; progress per song in the song list
- Lyrics, reference links, drum tunings and sessions of the song; printable song sheet

**✅ 2. Pictures & tiles, calendar feed** (v1.2.0, v1.3.0)
- Pictures for clients and bands, cover art for projects; list / tile view
- Calendar subscription link: sessions show up in the Nextcloud / phone calendar

**✅ 3. Band gear via link** (v1.4.0)
- A link per project (no login) where the band enters the gear it brings; you accept the entries

**4. Finance** (replaces Papierkram) – step by step
- ✅ a) (v1.5.0) Default hourly rate in the settings; time tracking (entries, start / stop timer, "session done → book the hours");
  price per project (hourly rate or flat price, extra items); hours, value, invoiced and open amount per project
- ✅ b1) (v1.6.0) Standard tasks (settings, e.g. "Recording / Tag" = 8 h × hourly rate) for time entries and the timer;
  business data, small business / VAT, payment terms and invoice number range on the profile page
- ✅ b2) (v1.7.0) Invoices from open hours / items (grouped by standard task, notes stay internal): gap-free numbers, all required content, locked once issued (corrections by
  cancellation invoice), PDF via print; payments and partial payments, open / overdue overview;
  small business (§ 19 UStG) or VAT switchable
- ✅ c) (v1.8.0) Quotes per project with standard tasks (project rate), own number range, validity, print; status sent / accepted /
  declined; "Create invoice" turns an accepted quote into an invoice draft. Navigation "Invoices" became "Finance" (tabs)
- d) Expenses with receipts; yearly overview of income and expenses (basis for the EÜR), CSV export

## Later
- **Start page**: today's session, next sessions, open tasks, unpaid invoices, album progress
- **Recall photos** per session / instrument setup (mic positions, amp settings)
- Contacts per client / band, global search
- Gear: maintenance log, lending (who has which mic), QR code labels, wishlist / budget
- **Nextcloud**: link a Nextcloud folder per project (lyrics, references, contracts – replaces own file storage)
- **Mix revision tool** (separate big task, maybe on top of Nextcloud): mix versions per song, waveform player with
  A/B switching, timestamped comments, share link for clients with approval per song, loudness matching
- Patchbay and outboard inserts in the studio I/O, reusable input list templates
- Printable take / track / recall sheets
- Status history of songs, client accounts with access to their own projects only
