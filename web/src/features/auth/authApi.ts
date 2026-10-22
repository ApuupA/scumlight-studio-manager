import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { ChangePasswordInput, LoginInput, ProfileInput } from '../../api/types'

const currentUserKey = ['currentUser']

/** The logged in user. Fails with a 401 ApiError if nobody is logged in. */
export function useCurrentUser() {
  return useQuery({
    queryKey: currentUserKey,
    queryFn: () => unwrap(api.GET('/api/Auth/me')),
    staleTime: 5 * 60 * 1000,
  })
}

/**
 * True for the studio owner. Assistants see no finance (times, prices, invoices, ...) and no settings -
 * the backend refuses those requests as well, hiding them here only keeps the pages clean.
 */
export function useIsOwner(): boolean {
  const { data: user } = useCurrentUser()
  return user?.role === 'Owner'
}

export function useLogin() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (input: LoginInput) => unwrap(api.POST('/api/Auth/login', { body: input })),
    onSuccess: (user) => queryClient.setQueryData(currentUserKey, user),
  })
}

export function useUpdateProfile() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (input: ProfileInput) => unwrap(api.PUT('/api/Auth/me', { body: input })),
    // The response is the updated user - put it into the cache, so the header shows the new name immediately
    onSuccess: (user) => queryClient.setQueryData(currentUserKey, user),
  })
}

export function useChangePassword() {
  return useMutation({
    mutationFn: (input: ChangePasswordInput) => unwrap(api.POST('/api/Auth/change-password', { body: input })),
  })
}

export function useLogout() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: () => unwrap(api.POST('/api/Auth/logout')),
    // Forget all cached data of the logged out user
    onSuccess: () => queryClient.clear(),
  })
}
