# Running the Studio Manager on the server

Guide for the Ubuntu server (Strato) that already runs Nextcloud. The Studio Manager runs **completely separate**
from Nextcloud: own containers, own PostgreSQL, own Apache site. Nextcloud is not touched.

```
Browser ──https──▶ Apache (port 443, already there)
                      │  studio.scumlight-cloud.de
                      ▼
                   127.0.0.1:8081 ──▶ web container (nginx) ──▶ api container ──▶ db container
                                                                         pgAdmin (127.0.0.1:5050, SSH tunnel only)
```

The app runs at `studio.scumlight-cloud.de`, as user **`apu`** in **`~/studio-manager`** (next to `~/nextcloud-docker`).
Commands for Apache and certbot need `sudo`, everything else runs as `apu`.

**Never touch Nextcloud:** only run `docker compose` inside `~/studio-manager` – there it only affects the Studio Manager.

---

## 1. Subdomain

In the Strato customer area: create the subdomain (e.g. `studio.scumlight-cloud.de`) and point it to the IP address of the
server (DNS **A record**, like the domain of Nextcloud). Check it on the server – it must print the server's IP:

```bash
getent hosts studio.scumlight-cloud.de
```

It can take a while until a new DNS entry is visible.

Both entries must point to **this** server: the **A record** (IPv4, `85.215.53.239`) and the **AAAA record**
(IPv6, `2a02:2479:2b:ab00::1` – shown by `ip -6 addr show scope global`). A new subdomain may come with a default
AAAA record of the provider – then certbot fails ("Invalid response … 204"), because Let's Encrypt checks over IPv6.
Without an A record, browsers on IPv4-only connections can not reach the page.

## 2. Get the code

The repository is private, so the server needs its own read-only key ("deploy key"):

```bash
ssh-keygen -t ed25519 -f ~/.ssh/studio_deploy -N "" -C "studio server"
cat ~/.ssh/studio_deploy.pub
```

On GitHub: repository → **Settings → Deploy keys → Add deploy key**, paste the printed line, do **not** tick
"Allow write access". Then tell SSH to use this key for GitHub and clone:

```bash
cat >> ~/.ssh/config <<'EOF'
Host github.com
    IdentityFile ~/.ssh/studio_deploy
EOF

git clone git@github.com:ApuupA/scumlight-studio-manager.git ~/studio-manager
cd ~/studio-manager
```

## 3. Settings (.env)

```bash
cp .env.example .env
nano .env
```

| Setting | Value on the server |
|---|---|
| `DB_PASSWORD` | long random password, e.g. from `openssl rand -base64 24` |
| `OWNER_EMAIL` / `OWNER_PASSWORD` / `OWNER_NAME` | your login (only used on the very first start, when there is no user yet) |
| `PGADMIN_EMAIL` / `PGADMIN_PASSWORD` | login for pgAdmin (another random password) |
| `WEB_PORT` | `127.0.0.1:8081` – only Apache on the same server can reach the app, 8080 belongs to Nextcloud |
| `PROXY_COUNT` | `2` – Apache and nginx are in front of the API (needed for the login limit and the secure cookie) |

`.env` contains the passwords: it is ignored by git and should stay readable for root only (`chmod 600 .env`).

## 4. Start

```bash
docker compose up -d --build
docker compose ps                  # api, db, web, pgadmin: "Up"
curl -I http://127.0.0.1:8081      # HTTP/1.1 200 OK
```

The first build takes a few minutes. On the first start the API creates the database tables and your owner account.

## 5. Apache site with HTTPS

Enable the Apache modules for forwarding (nothing happens if they are already enabled):

```bash
a2enmod proxy proxy_http headers ssl
```

Create the site `/etc/apache2/sites-available/studio-manager.conf`:

```apache
<VirtualHost *:80>
    ServerName studio.scumlight-cloud.de

    # Everything goes to the web container of the Studio Manager
    ProxyPreserveHost On
    ProxyPass        / http://127.0.0.1:8081/
    ProxyPassReverse / http://127.0.0.1:8081/
</VirtualHost>
```

```bash
a2ensite studio-manager
apache2ctl configtest              # must say "Syntax OK"
systemctl reload apache2
```

Get the certificate with certbot (most likely already installed for Nextcloud – otherwise
`apt install certbot python3-certbot-apache`). Choose **redirect** when it asks, so http always goes to https:

```bash
certbot --apache -d studio.scumlight-cloud.de
```

If certbot asks which virtual host to use and lists the Nextcloud sites: press **`c`** (cancel) – the
`studio-manager.conf` site is missing or not enabled. Never choose a Nextcloud site.

certbot creates `/etc/apache2/sites-available/studio-manager-le-ssl.conf` (the https copy of the site).
Add **one line** to that file, below `ProxyPreserveHost On` – it tells the app that the browser used https,
so the login cookie is only ever sent encrypted:

```apache
    RequestHeader set X-Forwarded-Proto "https"
```

```bash
apache2ctl configtest
systemctl reload apache2
```

Open https://studio.scumlight-cloud.de – the login page should appear. Check the cookie: after logging in, the browser's
developer tools (F12 → Application / Storage → Cookies) show `studio.auth` with **Secure** ticked.
certbot renews the certificate automatically.

## 6. pgAdmin from your PC

pgAdmin is not reachable from the internet (only `127.0.0.1` on the server). Open an SSH tunnel from your PC and
keep the window open while you work:

```bash
ssh -L 5050:localhost:5050 root@studio.scumlight-cloud.de   # or the user you normally log in with
```

Then open http://localhost:5050 on your PC and log in with `PGADMIN_EMAIL` / `PGADMIN_PASSWORD`. The database
password asked for by "Studio Manager" is `DB_PASSWORD`. (If pgAdmin also runs locally on your PC, stop it first –
both use port 5050.)

## 7. Optional: take your local data along

Without this step the server starts empty. With it, the server gets everything from your PC, including your
login (the owner account from the server's `.env` is then not used). Do it **before step 4**, or reset the server
database first with `docker compose down -v` (deletes all data on the server!).

On your PC:

```bash
docker compose exec -T db pg_dump -U studio -d studio > studio-transfer.sql
scp studio-transfer.sql apu@studio.scumlight-cloud.de:~/studio-manager/
```

On the server – start only the database, load the data, then everything else:

```bash
cd ~/studio-manager
docker compose up -d db
docker compose exec -T db psql -U studio -d studio < studio-transfer.sql
docker compose up -d --build
rm studio-transfer.sql
```

Uploaded files (photos) are not in the database. If you have some, copy the volume as well:

```bash
# PC
docker run --rm -v recording-studio-tool_studio-files:/data -v "$PWD":/backup alpine tar czf /backup/studio-files.tgz -C /data .
scp studio-files.tgz apu@studio.scumlight-cloud.de:~/studio-manager/
# Server (before "docker compose up -d --build")
docker run --rm -v studio-manager_studio-files:/data -v ~/studio-manager:/backup alpine tar xzf /backup/studio-files.tgz -C /data
```

## 8. Updates

```bash
~/studio-manager/scripts/update.sh
```

Shows what is new, asks, then: backup → new version → rebuild → checks that the app answers. Database changes
(migrations) are applied automatically when the API starts. The whole process, including testing database changes
on a copy of the server data first: [RELEASE_CHECKLIST.md](RELEASE_CHECKLIST.md).

## 9. Backups

```bash
~/studio-manager/scripts/backup.sh
```

saves the database (`studio-<date>.sql`) **and** the uploaded files (`studio-files-<date>.tgz`) in `~/studio-backups`
and deletes backups older than 14 days. Every night at 3:00 – add with `crontab -e` (as `apu`):

```cron
0 3 * * * ~/studio-manager/scripts/backup.sh >> ~/studio-backups/backup.log 2>&1
```

Restore (asks for `RESTORE` first, replaces all data):

```bash
~/studio-manager/scripts/restore.sh ~/studio-backups/studio-<date>.sql ~/studio-backups/studio-files-<date>.tgz
```

Copy the backups to your PC from time to time – a backup on the same server does not help if the server is lost.

**Where the data is:** in the Docker volumes `studio-manager_db-data` (database) and `studio-manager_studio-files`
(photos, login cookie keys) under `/var/lib/docker/volumes/`. They survive updates, rebuilds and reboots – only
`docker compose down -v` or `docker volume rm` deletes them. Never use those on the server.

## Why the ports are safe

- Docker opens published ports **past the firewall** (ufw). That is why the Studio Manager publishes everything on
  `127.0.0.1` only: the app (8081), PostgreSQL (5433) and pgAdmin (5050) can not be reached from outside, only
  through Apache with https – or pgAdmin through the SSH tunnel.
- The login allows 10 attempts per 15 minutes per IP address.
