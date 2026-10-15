import { createTheme, type CSSVariablesResolver, type MantineColorsTuple } from '@mantine/core'

/**
 * Scumlight Studio design - all colors and fonts of the app are defined here.
 * The colors are taken from the logo (src/assets/scumlight-logo.png):
 *   black #000000 · cream #FFF7E1 · red #CC3300 (lettering) · orange #EA700D (lettering shadow)
 *
 * Mantine needs every color as 10 shades from light (0) to dark (9).
 * Index 6 is the "main" shade used for buttons, links, active navigation etc.
 */

/** The red of the lettering - primary color of the app. */
const scumlight: MantineColorsTuple = [
  '#fff0e8', '#ffdccb', '#ffb796', '#ff8f5c', '#f96c2e',
  '#e75012', '#cc3300', '#ad2b00', '#8f2300', '#6e1b00',
]

/** The orange of the lettering shadow - used for highlights. */
const ember: MantineColorsTuple = [
  '#fff3e6', '#ffe2c4', '#ffc58a', '#ffa64d', '#f78b24',
  '#f07d16', '#ea700d', '#c75e08', '#a34c05', '#7d3a03',
]

/**
 * Mantine's dark mode palette, warmed up to match the logo:
 * dark[0] is the text color (cream), dark[7] the page background (almost black).
 */
const dark: MantineColorsTuple = [
  '#f5ecd6', '#ddd3bd', '#a8a08f', '#7a7468', '#4a4640',
  '#2e2b27', '#211f1c', '#141312', '#0d0c0b', '#070706',
]

export const brandColors = {
  cream: '#fff7e1',
  red: '#cc3300',
  orange: '#ea700d',
  black: '#000000',
}

export const theme = createTheme({
  colors: { scumlight, ember, dark },
  primaryColor: 'scumlight',
  primaryShade: 6,
  black: '#0d0c0b',
  defaultRadius: 'sm',

  fontFamily: "'Inter Variable', system-ui, sans-serif",
  headings: {
    // Blackletter like the logo lettering - only for headings, body text stays readable
    fontFamily: "'UnifrakturMaguntia', serif",
    fontWeight: '400',
  },
})

/**
 * Light mode: cream "paper" background instead of plain white, like the inside of the logo.
 */
export const cssVariablesResolver: CSSVariablesResolver = () => ({
  variables: {},
  light: {
    '--mantine-color-body': '#fbf4e4',
    '--mantine-color-default': '#fffaf0',
    '--mantine-color-default-hover': '#f3e8d0',
    '--mantine-color-default-border': '#ddcfb2',
  },
  dark: {},
})
