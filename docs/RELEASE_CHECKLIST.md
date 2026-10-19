# Release checklist

How a change gets from the PC to the server without risking the real data.
**The server is the one real database.** Real data is only entered there; the PC is for development and testing.

## 1. Develop (PC)

- [ ] Work on a feature branch, never on `main` (e.g. `feature/mix-revisions`)
- [ ] `dotnet test src/StudioManager.sln`, `npm run lint --prefix web`, `npm run build --prefix web` – all green
- [ ] DTOs / endpoints changed? `npm run api:generate --prefix web` and commit `openapi.json` + `schema.d.ts`

## 2. Database changes? (only if there is a new migration)

Open the new file in `src/StudioManager.DataLayer/Migrations/` and check:

| In the migration | Question |
|---|---|
| `AddColumn` with `nullable: false` | Does it have a default value (`defaultValue` / `defaultValueSql`)? Otherwise it fails on existing rows. |
| `DropColumn` / `DropTable` | Is that data really not needed anymore – or must it be copied first (`migrationBuilder.Sql(...)` **before** the drop)? |
| `DropColumn` + `AddColumn` of a similar name | That is a rename done wrong – it loses the data. Use `RenameColumn` instead. |
| `AlterColumn` (type / length) | Can every existing value be converted (e.g. text → number)? |

Then **test the migration on a copy of the server data**:

```bash
# Server: make a fresh backup
~/studio-manager/scripts/backup.sh for-test
```
```bash
# PC (Git Bash, in the repository folder): copy it and load it into the test database "studio_test"
scp apu@studio.scumlight-cloud.de:'~/studio-backups/studio-*for-test*' data/
bash scripts/load-server-data.sh data/studio-<date>-for-test.sql data/studio-files-<date>-for-test.tgz
```

- [ ] Start the API against the test data (the script prints the command) – it starts = migration worked
- [ ] Click through the changed pages in the browser – old data looks right
- [ ] Delete the copied backup files from `data/` afterwards (real data, not for the PC)

## 3. Merge and mark the version (PC)

- [ ] Pull request → merge into `main`
- [ ] The PR says whether existing data is affected ("no database change" / "migration X, data is kept")
- [ ] Give the version a number and push it:
  ```bash
  git checkout main && git pull
  git tag -a v1.2.0 -m "Mix revisions"
  git push origin v1.2.0
  ```
  Numbers: `v1.2.0` → `v1.3.0` for new features, `v1.2.1` for fixes.

## 4. Update the server

As `apu` on the server:

```bash
~/studio-manager/scripts/update.sh
```

It shows what is new, asks, then: backup → new version → rebuild → checks that the app answers.

- [ ] The script ends with "Updated to v… - the app is running."
- [ ] Open https://studio.scumlight-cloud.de, log in, look at the changed pages

## 5. If something goes wrong

`update.sh` prints the exact commands. In general:

```bash
cd ~/studio-manager
git reset --hard <previous version, e.g. v1.1.0>
docker compose up -d --build
# only if the database was changed by the update: restore the backup made by update.sh
./scripts/restore.sh ~/studio-backups/studio-<date>-before-update.sql ~/studio-backups/studio-files-<date>-before-update.tgz
```

`restore.sh` asks for `RESTORE` before it replaces anything.

## Regularly

- [ ] Copy the newest backups from the server to the PC (or another disk) – e.g. once a week:
  ```bash
  scp apu@studio.scumlight-cloud.de:'~/studio-backups/studio-*' /path/to/your/backup/disk/
  ```
- [ ] Once in a while: load a backup with `load-server-data.sh` and look at it – proves that the backups really work
- [ ] Free disk space on the server: `df -h /` (important once audio files are uploaded)
