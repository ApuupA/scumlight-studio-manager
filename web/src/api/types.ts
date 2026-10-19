import type { components } from './schema'

/**
 * Short names for the generated DTO types. These are 1:1 the C# DTOs from StudioManager.DataLayer/DTO.
 * After changing a DTO in C#, run `npm run api:generate` and TypeScript shows every place that needs an update.
 */
type Schemas = components['schemas']

export type GearItem = Schemas['GearItemReadDto']
export type GearItemInput = Schemas['GearItemCreateDto']
export type GearStatus = Schemas['GearStatus']

export type GearCategory = Schemas['GearCategoryReadDto']
export type GearCategoryInput = Schemas['GearCategoryCreateDto']

export type User = Schemas['UserReadDto']
export type LoginInput = Schemas['LoginDto']
export type ProfileInput = Schemas['UpdateProfileDto']
export type ChangePasswordInput = Schemas['ChangePasswordDto']

export type Client = Schemas['ClientReadDto']
export type ClientInput = Schemas['ClientCreateDto']
export type Band = Schemas['BandReadDto']
export type BandInput = Schemas['BandCreateDto']

export type Project = Schemas['ProjectReadDto']
export type ProjectInput = Schemas['ProjectCreateDto']
export type ProjectType = Schemas['ProjectType']
export type ProjectStatus = Schemas['ProjectStatus']
export type Song = Schemas['SongReadDto']
export type SongInput = Schemas['SongCreateDto']
export type SongTempo = Schemas['SongTempoReadDto']
export type SongStatus = Schemas['SongStatus']
export type SongStatusUpdateInput = Schemas['SongStatusUpdateDto']
export type ProjectLink = Schemas['ProjectLinkReadDto']
export type ProjectLinkInput = Schemas['ProjectLinkCreateDto']
export type ProjectLinkType = Schemas['ProjectLinkType']
export type DrumTuning = Schemas['DrumTuningReadDto']
export type DrumTuningInput = Schemas['DrumTuningCreateDto']
export type ExternalGear = Schemas['ExternalGearReadDto']
export type ExternalGearInput = Schemas['ExternalGearCreateDto']

export type Session = Schemas['SessionReadDto']
export type SessionInput = Schemas['SessionCreateDto']
export type SessionDuplicateInput = Schemas['SessionDuplicateDto']
export type SessionType = Schemas['SessionType']
export type SessionStatus = Schemas['SessionStatus']
export type SessionTask = Schemas['SessionTaskReadDto']
export type SessionTaskInput = Schemas['SessionTaskCreateDto']

export type ProjectInstrument = Schemas['ProjectInstrumentReadDto']
export type ProjectInstrumentInput = Schemas['ProjectInstrumentCreateDto']
export type SetupItem = Schemas['InstrumentSetupItemReadDto']
export type SetupItemInput = Schemas['InstrumentSetupItemCreateDto']
export type SetupItemSource = Schemas['SetupItemSource']
export type SetupTemplate = Schemas['SetupTemplateReadDto']
export type ApplySetupTemplateInput = Schemas['ApplySetupTemplateDto']
export type GearUsage = Schemas['GearUsageDto']

export type InputGroup = Schemas['InputGroupReadDto']
export type InputGroupInput = Schemas['InputGroupCreateDto']
export type StudioInput = Schemas['StudioInputReadDto']
export type StudioInputInput = Schemas['StudioInputCreateDto']
export type StudioInputBulkInput = Schemas['StudioInputBulkCreateDto']
export type InputList = Schemas['InputListReadDto']
export type InputListInput = Schemas['InputListCreateDto']
export type InputListRow = Schemas['InputListRowReadDto']
export type InputListRowInput = Schemas['InputListRowSaveDto']

// All values of the C# enums, in the order of the C# enum (used for dropdowns)
export const gearStatuses: GearStatus[] = ['Available', 'Defective', 'InRepair', 'LentOut', 'Retired']
export const projectTypes: ProjectType[] = ['Album', 'Ep', 'Single', 'SplitRelease', 'Demo', 'Other']
export const projectStatuses: ProjectStatus[] = [
  'Planning', 'PreProduction', 'Recording', 'Editing', 'Mixing', 'Mastering', 'Completed', 'OnHold', 'Cancelled',
]
export const songStatuses: SongStatus[] = ['Planned', 'Recording', 'Editing', 'Mixing', 'Mastering', 'Done']
export const projectLinkTypes: ProjectLinkType[] = ['Reference', 'RoughMix', 'Demo', 'Other']
export const sessionTypes: SessionType[] = [
  'PreProduction', 'Recording', 'Overdubs', 'Editing', 'Mixing', 'Mastering', 'Production', 'Other',
]
export const sessionStatuses: SessionStatus[] = ['Planned', 'Confirmed', 'Completed', 'Cancelled']
