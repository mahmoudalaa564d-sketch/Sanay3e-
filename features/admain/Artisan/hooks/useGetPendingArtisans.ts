import { useQuery } from "@tanstack/react-query";
import AdminService from "../api/AdminService";


export const useGetPendingArtisans = () => {
  return useQuery({
    queryKey: ["admin", "pending-artisans"],
    queryFn: () => AdminService.getPendingArtisans(),
  });
};