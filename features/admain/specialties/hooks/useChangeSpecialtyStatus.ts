import { useMutation, useQueryClient } from "@tanstack/react-query";
import SpecialtyService from "../api/AdminSpecialtiesService";

export const useChangeSpecialtyStatus = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      id,
      isActive,
    }: {
      id: number;
      isActive: boolean;
    }) => SpecialtyService.changeStatus(id, { isActive }),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["specialties"],
      });
    },
  });
};