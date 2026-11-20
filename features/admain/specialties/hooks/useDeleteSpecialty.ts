import { useMutation, useQueryClient } from "@tanstack/react-query";
import SpecialtyService from "../api/AdminSpecialtiesService";

export const useDeleteSpecialty = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) =>
      SpecialtyService.deleteSpecialty(id),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["specialties"],
      });
    },
  });
};