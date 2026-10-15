# Studio Manager

A web application to manage a recording studio: gear inventory, projects & sessions, documents and (later) mix revisions.

**Status:** Milestone 1 – foundation + gear inventory. See [docs/ROADMAP.md](docs/ROADMAP.md).

| Part | Technology | Folder |
|---|---|---|
| Backend | ASP.NET Core 10 Web API (controllers), EF Core 10, PostgreSQL, AutoMapper, Serilog | [src/](src) |
| Frontend | React 19 + TypeScript, Vite, Mantine UI, TanStack Query, React Router, i18next (DE/EN) | [web/](web) |
| Hosting | Docker Compose: PostgreSQL + API + nginx (serves the frontend, forwards `/api`) | [docker-compose.yml](docker-compose.yml) |

---

## Run it locally (development)

Requirements: .NET 10 SDK, Node.js 22+ (21 works too, but 22 LTS is recommended), Docker Desktop.

```bash
# once: restore the local .NET tools (dotnet-ef, swagger CLI) and the npm packages
dotnet tool restore
npm install --prefix web

# 1) database (PostgreSQL in Docker, port 5433)
docker compose up -d db

# 2) API on http://localhost:5080 (Swagger UI: http://localhost:5080/swagger)
dotnet run --project src/StudioManager.API

# 3) frontend on http://localhost:5173 (in a second terminal)
npm run dev --prefix web
```

Or open `src/StudioManager.sln` in Visual Studio / Rider and start `StudioManager.API` there.

Log in with the development account from [appsettings.Development.json](src/StudioManager.API/appsettings.Development.json):
**owner@studio.local / studio**. The account is created on the first start when the database has no users.

Database migrations are applied automatically when the API starts.

## Run everything in Docker (like on the server)

```bash
cp .env.example .env      # set real passwords!
docker compose up -d --build
```

The app is then available on http://localhost:8080. Uploaded files and the database live in the Docker volumes
`studio-files` and `db-data`.

---

## Project structure

```
src/
  StudioManager.API/              Controllers, Program.cs (DI setup), error handling
  StudioManager.DataLayer/        Entities (Models/), DTOs (DTO/Create, DTO/Read), AppDbContext, Migrations
  StudioManager.RepositoryLayer/  Generic repository IRepository<T> / Repository<T>
  StudioManager.ServiceLayer/     Business logic: IService/ + Service/, AutoMapper profile, exceptions
  StudioManager.Tests.Unit/       xUnit + Moq + EF InMemory
web/
  src/api/                        Typed API client (generated types in schema.d.ts)
  src/features/<feature>/         Pages, components and API hooks of one feature (auth, gear, ...)
  src/layout/                     App frame: header, navigation, language switch
  src/i18n/locales/               Translations (en.json, de.json)
```

The backend follows the layered structure **Controller → Service → Repository → DbContext**:

- `BaseController<TReadDto, TCreateDto, TEntity, TService>` provides GET (all / by id), POST, PUT, DELETE.
- `BaseService<T, TReadDto, TCreateDto>` implements the CRUD logic. Derived services override single methods
  to add business rules (see `GearCategoryService.Delete`).
- Services throw `EntityNotFoundException` (→ 404) or `BusinessRuleException` (→ 409 with an `errorCode`).
  `ApiExceptionHandler` turns them into HTTP responses, so controllers need no try/catch.
- Create DTOs are used for POST **and** PUT, and carry the validation attributes (`[Required]`, `[MaxLength]`, ...).

### The typed API client

The frontend does not write URLs and types by hand. They are generated from the API's Swagger description:

```bash
npm run api:generate --prefix web
```

This builds the API, writes `web/openapi.json` and generates `web/src/api/schema.d.ts`. After changing a C# DTO or
endpoint, run it and TypeScript reports every place in the frontend that needs to be adapted.

---

## How to add a new feature (checklist)

Example: a new entity `Session`.

**Backend**
1. Entity `DataLayer/Models/SessionModels/Session.cs` (inherits `BaseEntity`), add `DbSet<Session>` + configuration in `AppDbContext`.
2. DTOs `DTO/Create/SessionCreateDto.cs` (with validation attributes) and `DTO/Read/SessionReadDto.cs`.
3. Mappings in `ServiceLayer/Mapper/MappingProfile.cs` (`MappingProfileTests` tells you if something is missing).
4. `ISessionService : IBaseService<...>` + `SessionService : BaseService<...>`, register it in `Program.cs`.
5. `SessionController : BaseController<...>` in `API/Controllers/SessionControllers/`.
6. Migration: `dotnet ef migrations add AddSessions --project src/StudioManager.DataLayer --startup-project src/StudioManager.API`
7. Tests in `StudioManager.Tests.Unit`.

**Frontend**
1. `npm run api:generate --prefix web`, add type aliases in `web/src/api/types.ts`.
2. `web/src/features/sessions/sessionApi.ts` with the query / mutation hooks (copy `gear/categoryApi.ts`).
3. Pages in the same folder, routes in `web/src/App.tsx`, navigation entry in `web/src/layout/AppLayout.tsx`.
4. Texts in `web/src/i18n/locales/en.json` **and** `de.json`.

New to React? Read [docs/FRONTEND_GUIDE.md](docs/FRONTEND_GUIDE.md) – it explains the frontend with C# analogies.

## Design

The look is derived from the Scumlight Studio logo (`web/src/assets/scumlight-logo.png`, original file):

- **Colors and fonts** are all defined in [web/src/theme.ts](web/src/theme.ts): red `#CC3300` (primary color),
  orange `#EA700D`, cream `#FFF7E1`, black. Dark mode is the default; light mode uses a cream "paper" background.
- **Font**: *Inter* for all text, bundled with the app (no Google requests). The logo carries the brand, texts stay plain.
- **Logo files**: `scumlight-logo-480.webp` (login page), `scumlight-logo-96.webp` (header), `public/favicon.png`,
  `public/apple-touch-icon.png` – all generated from the original. Replace them if the logo changes.
- Component specific styles live in CSS modules next to the component (`Brand.module.css`, `LoginPage.module.css`).

## Tests & checks

```bash
dotnet test src/StudioManager.sln     # backend unit tests
npm run lint --prefix web             # frontend lint
npm run build --prefix web            # frontend type check + production build
```

## Configuration

| Setting | Meaning |
|---|---|
| `ConnectionStrings:StudioDatabase` | PostgreSQL connection string |
| `FileStorage:RootPath` | Folder for uploaded files (Docker: volume `/data/files`) |
| `OwnerAccount:Email / Password / DisplayName` | Owner account created on the first start |
| `AutoMapper:LicenseKey` | Optional – AutoMapper 15+ logs a license warning without it (free community license on automapper.io) |

In Docker these are set as environment variables (`ConnectionStrings__StudioDatabase`, ...) in `docker-compose.yml` / `.env`.
