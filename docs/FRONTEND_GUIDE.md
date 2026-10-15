# Frontend guide for C# developers

The frontend is written in **TypeScript** (JavaScript with static types – feels close to C#) using **React**.
This guide maps the important concepts to things you know from ASP.NET Core.

## The big picture

```
Browser URL ──► React Router (App.tsx) ──► Page component ──► API hook (TanStack Query) ──► api client ──► ASP.NET Core
   /gear/5          "routing table"          GearItemPage        useGearItem(5)              GET /api/GearItem/5
```

| Frontend | C# / ASP.NET Core analogy |
|---|---|
| `App.tsx` with `<Route path="/gear/:id">` | Route table / `[Route]` attributes |
| A **component** (`function GearListPage() { ... return <...> }`) | A Razor view + its code-behind in one function |
| **Props** (`function GearStatusBadge({ status })`) | Constructor / method parameters |
| **State** (`const [search, setSearch] = useState('')`) | A private field. Calling `setSearch(...)` re-renders the component with the new value |
| **Hooks** (`useXyz()`) | Things injected/resolved for the component (like DI). Only call them at the top of a component, never inside `if`/loops |
| `useMemo(() => ..., [a, b])` | A cached computed property, recalculated only when `a` or `b` change |
| `src/api/schema.d.ts` (generated) | The DTO classes, generated from swagger.json |
| `api.GET('/api/GearItem/{id}', ...)` | A typed `HttpClient` call |
| `useQuery` / `useMutation` (TanStack Query) | A repository with built-in cache: reading / writing |
| `queryClient.invalidateQueries(...)` | "Cache is stale, reload" – every component showing that data updates automatically |
| `t('gear.title')` (i18next) | `IStringLocalizer["gear.title"]` / resource files |
| Mantine components (`<Button>`, `<Table>`, `<TextInput>`) | A UI control library (like Telerik / DevExpress) |

## How a page works – example `CategoriesPage.tsx`

1. **Load data**: `const { data: categories, isPending, error } = useCategories()`
   - `isPending` is true while loading → show a `<Loader />`
   - `error` is set if the API failed → show an `<Alert>`
   - `data` contains the `GearCategoryReadDto[]`
2. **Change data**: `const deleteCategory = useDeleteCategory()` then `deleteCategory.mutate(id, { onSuccess, onError })`
   - The hook invalidates the category cache on success → the table reloads by itself. No manual refresh code.
3. **Render**: the `return (...)` block is JSX – HTML-like syntax where `{...}` embeds TypeScript expressions:
   ```tsx
   {categories.map((category) => (
     <Table.Tr key={category.id}>        // key = unique id, React needs it for lists
       <Table.Td>{category.name}</Table.Td>
     </Table.Tr>
   ))}
   ```

Every time state or query data changes, React calls the component function again and updates only what changed
in the browser. You never manipulate the page directly.

## Forms

Forms use `@mantine/form` (see `GearItemForm.tsx`):

```tsx
const form = useForm({ initialValues: {...}, validate: { name: (v) => (v.trim() ? null : t('validation.required')) } })
<TextInput label={t('gear.name')} {...form.getInputProps('name')} />   // binds value, onChange and error
<form onSubmit={form.onSubmit((values) => save(values))}>
```

Client validation is only for quick feedback – the real validation is the DataAnnotations on the C# Create DTO.
A 400 response is shown as a red notification automatically via `notifyError`.

## Errors and translations

- The API returns ProblemDetails. `BusinessRuleException("gearCategory.hasItems", ...)` arrives as
  `{ status: 409, errorCode: "gearCategory.hasItems" }`.
- `getErrorMessage()` (src/api/errors.ts) looks up `errors.gearCategory.hasItems` in the translation files.
  **When you add a new BusinessRuleException in C#, add its errorCode to `en.json` and `de.json`.**

## Useful commands

```bash
npm run dev            # start dev server with hot reload (http://localhost:5173)
npm run api:generate   # regenerate API types after changing C# DTOs / endpoints
npm run build          # type check + production build - run before committing
npm run lint           # code style checks
```

## Where to learn more

- React: https://react.dev/learn (the "Quick Start" and "Thinking in React" pages are enough to start)
- Mantine components: https://mantine.dev (every component has copy-paste examples)
- TanStack Query: https://tanstack.com/query/latest/docs/framework/react/overview
