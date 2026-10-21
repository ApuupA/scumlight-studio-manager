import type { InvoicePaymentStatus, MixVersionStatus, ProjectStatus, SessionStatus, SongStatus, SongTrackStatus } from '../api/types'

/**
 * Badge colors per status value. When a C# enum gets a new value, TypeScript reports the missing color here.
 * The colors follow the workflow: grey = not started, orange/red = in production, green = done.
 */

export const projectStatusColors: Record<ProjectStatus, string> = {
  Planning: 'gray',
  PreProduction: 'gray',
  Recording: 'scumlight',
  Editing: 'ember',
  Mixing: 'ember',
  Mastering: 'yellow',
  Completed: 'green',
  OnHold: 'blue',
  Cancelled: 'dark',
}

export const songStatusColors: Record<SongStatus, string> = {
  Planned: 'gray',
  Recording: 'scumlight',
  Editing: 'ember',
  Mixing: 'ember',
  Mastering: 'yellow',
  Done: 'green',
}

export const sessionStatusColors: Record<SessionStatus, string> = {
  Planned: 'gray',
  Confirmed: 'scumlight',
  Completed: 'green',
  Cancelled: 'dark',
}

export const songTrackStatusColors: Record<SongTrackStatus, string> = {
  Open: 'gray',
  Recorded: 'scumlight',
  Edited: 'green',
}

export const invoicePaymentStatusColors: Record<InvoicePaymentStatus, string> = {
  Draft: 'gray',
  Open: 'blue',
  PartiallyPaid: 'yellow',
  Paid: 'green',
  Overdue: 'red',
  Cancelled: 'dark',
  Cancellation: 'dark',
}

export const mixVersionStatusColors: Record<MixVersionStatus, string> = {
  InProgress: 'gray',
  Sent: 'blue',
  ChangesRequested: 'ember',
  Approved: 'green',
}
