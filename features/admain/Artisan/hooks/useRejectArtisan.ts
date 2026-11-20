import { useMutation, useQueryClient } from "@tanstack/react-query";
import AdminService from "../api/AdminService";


interface RejectArtisanVariables {
  userId: number;
  reason: string;
}

export const useRejectArtisan = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ userId, reason }: RejectArtisanVariables) =>
      AdminService.rejectArtisan(userId, reason),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["admin", "pending-artisans"],
      });
    },
  });
};