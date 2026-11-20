import { useMutation, useQueryClient } from "@tanstack/react-query";
import SpecialtyService from "../api/AdminSpecialtiesService";
import type { SpecialtyRequest } from "../type/specialty";

interface UpdateSpecialtyVariables {
  id: number;
  data: SpecialtyRequest;
}

export const useUpdateSpecialty = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: UpdateSpecialtyVariables) =>
      SpecialtyService.updateSpecialty(id, data),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["specialties"],
      });
    },
  });
};