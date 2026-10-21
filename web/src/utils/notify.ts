import { notifications } from '@mantine/notifications'
import type { TFunction } from 'i18next'
import { getErrorMessage } from '../api/errors'

/** Small green toast in the corner, e.g. after saving. */
export function notifySuccess(message: string) {
  notifications.show({ color: 'green', message })
}

/** Red toast with a translated error message. Use it in the onError of mutations. title: e.g. the file that failed. */
export function notifyError(error: unknown, t: TFunction, title?: string) {
  notifications.show({ color: 'red', title, message: getErrorMessage(error, t), autoClose: 8000 })
}
