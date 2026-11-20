import { useMutation, useQueryClient } from "@tanstack/react-query";
import AdminService from "../api/AdminService";

export const useApproveArtisan = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (userId: number) =>
      AdminService.approveArtisan(userId),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["admin", "pending-artisans"],
      });
    },
  });
};