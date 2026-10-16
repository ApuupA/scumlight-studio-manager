/**
 * Colors an input group can have (rows of the input list are colored by group, like in the Excel list).
 * The name is a Mantine palette color for the app; `print` is a light version for the printed input sheet.
 */
export const inputGroupColors: { name: string; print: string }[] = [
  { name: 'gray', print: '#d9d9d9' },
  { name: 'orange', print: '#f6b26b' },
  { name: 'red', print: '#ea9999' },
  { name: 'blue', print: '#a4c2f4' },
  { name: 'green', print: '#b6d7a8' },
  { name: 'yellow', print: '#ffe599' },
  { name: 'teal', print: '#a2d9ce' },
  { name: 'violet', print: '#c9b6e4' },
  { name: 'pink', print: '#f4b6d2' },
]

/** Light print color for a group color name ("orange" -> "#f6b26b"), white for rows without group. */
export function printColor(name: string | null | undefined): string {
  return inputGroupColors.find((color) => color.name === name)?.print ?? '#ffffff'
}

/** Background for a row in the app: the light variant of the theme color. */
export function rowBackground(name: string | null | undefined): string | undefined {
  return name ? `var(--mantine-color-${name}-light)` : undefined
}
