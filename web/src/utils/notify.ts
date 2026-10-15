import { notifications } from '@mantine/notifications'
import type { TFunction } from 'i18next'
import { getErrorMessage } from '../api/errors'

/** Small green toast in the corner, e.g. after saving. */
export function notifySuccess(message: string) {
  notifications.show({ color: 'green', message })
}

/** Red toast with a translated error message. Use it in the onError of mutations. */
export function notifyError(error: unknown, t: TFunction) {
  notifications.show({ color: 'red', message: getErrorMessage(error, t), autoClose: 8000 })
}
