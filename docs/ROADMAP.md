# Roadmap

## ✅ Milestone 1 – Foundation & gear inventory
- Layered ASP.NET Core API, PostgreSQL, EF Core migrations, Docker Compose
- Login with cookie (owner account), user roles prepared for clients
- Gear inventory: items with category, status, quantity, serial number, location, purchase data, notes, photo
- Editable categories (seeded with sensible defaults)
- Search, filters and sorting in the gear list
- German / English UI, light / dark mode
- Typed API client generated from Swagger, unit tests

## Milestone 2 – Projects, sessions & gear booking
- Clients / bands (contacts)
- Projects (album, EP, ...) with songs and song status (tracking → editing → mixing → mastered)
- Sessions (date/time blocks) inside projects, calendar view
- Assign gear to sessions → blocked for that time, conflict warning on double booking
- "In use" shown in the gear list based on bookings
- Gear packages (e.g. "drum mic package") to book several items at once

## Milestone 3 – Mix revision tool
- Upload mix versions per song (WAV/MP3), server-side conversion for streaming (FFmpeg)
- Waveform player (wavesurfer.js), seamless A/B switching between versions at the same position
- Timestamped comments with replies, open / resolved state
- Share link for clients without account, approval per song
- Optional loudness matching between versions

## Milestone 4 – I/O & input lists
- Model the studio I/O: interface channels, preamps, patchbay, outboard inserts
- Input list per session: source → mic → preamp → channel, printable as PDF
- Reusable input list templates

## Milestone 5 – Documents & templates
- File storage per project (lyrics, charts, references, contracts)
- Printable templates filled with session data: take sheets, track sheets, recall sheets

## Ideas for later
- Client accounts with access to their own projects only
- Maintenance log per gear item (tube swaps, repairs, recalibration)
- QR code labels for gear
- Time tracking & invoicing per project
- Gear wishlist / budget
