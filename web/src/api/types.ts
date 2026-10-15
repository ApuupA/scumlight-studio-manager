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

/** All values of the GearStatus enum, in the order of the C# enum. */
export const gearStatuses: GearStatus[] = ['Available', 'Defective', 'InRepair', 'LentOut', 'Retired']
