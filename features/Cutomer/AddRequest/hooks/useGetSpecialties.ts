import { useQuery } from "@tanstack/react-query";
import CustomerOrdersService from "../api/CustomerOrdersService";

export const useGetSpecialties = () => {
  return useQuery({
    queryKey: ["specialties"],
    queryFn: () => CustomerOrdersService.getSpecialties(),
  });
};