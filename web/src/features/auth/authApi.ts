import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { LoginInput } from '../../api/types'

const currentUserKey = ['currentUser']

/** The logged in user. Fails with a 401 ApiError if nobody is logged in. */
export function useCurrentUser() {
  return useQuery({
    queryKey: currentUserKey,
    queryFn: () => unwrap(api.GET('/api/Auth/me')),
    staleTime: 5 * 60 * 1000,
  })
}

export function useLogin() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (input: LoginInput) => unwrap(api.POST('/api/Auth/login', { body: input })),
    onSuccess: (user) => queryClient.setQueryData(currentUserKey, user),
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
