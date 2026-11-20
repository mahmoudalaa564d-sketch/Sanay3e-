import { useQuery } from "@tanstack/react-query";
import SpecialtyService from "../api/AdminSpecialtiesService";
import axios from "axios";

export const useGetSpecialties = () => {
  return useQuery({
    queryKey: ["specialties"],

    queryFn: async () => {
      try {
        const response = await SpecialtyService.getSpecialties();

        console.log("Specialties Response:", response);

        return response;
      } catch (error: unknown) {
      if (axios.isAxiosError(error)) {

          console.error("Get Specialties Error:", error);
          console.error("Status:", error.response?.status);
          console.error("Response Data:", error.response?.data);
          
          throw error;
        }
      }
    },
  });
};