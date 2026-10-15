# Notes for Claude

The studio owner maintains this code: an experienced C# backend developer who is new to React.
Readability beats cleverness. Follow the existing patterns instead of introducing new libraries or abstractions.

- Architecture, folder structure and the "add a new feature" checklist: see README.md.
- Backend style follows the owner's other API: BaseEntity, BaseController / BaseService / Repository<T>,
  DTO/Create + DTO/Read, AutoMapper MappingProfile, explicit DI registrations in Program.cs, XML doc comments.
- Business rule violations: throw `BusinessRuleException(errorCode, message)` and add the errorCode to
  `web/src/i18n/locales/en.json` and `de.json` under `errors`.
- Frontend: one folder per feature in `web/src/features/`, API hooks in `<feature>Api.ts`, Mantine components,
  every visible text through i18next (both languages).
- After changing DTOs or endpoints: `npm run api:generate --prefix web` and commit `openapi.json` + `schema.d.ts`.
- Git: develop on a feature branch (e.g. `feature/milestone-3-mix-revisions`), never commit directly to `main`.
- Before committing: `dotnet test src/StudioManager.sln`, `npm run lint --prefix web`, `npm run build --prefix web`.
