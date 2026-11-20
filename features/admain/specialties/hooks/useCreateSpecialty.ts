import { useMutation, useQueryClient } from "@tanstack/react-query";
import SpecialtyService from "../api/AdminSpecialtiesService";
import type { SpecialtyRequest } from "../type/specialty";

export const useCreateSpecialty = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: SpecialtyRequest) =>
      SpecialtyService.createSpecialty(data),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["specialties"],
      });
    },
  });
};