import { useMutation } from "@tanstack/react-query";
import { ArtisanRegisterService } from "../api/ArtisanRefisterService";
import type { ArtisanRegisterPayload } from "../type/ArtisanRegisterPelyod";

const service = new ArtisanRegisterService();

export const UseArtisanRegister = () => {
  return useMutation({
    mutationFn: (data: ArtisanRegisterPayload) =>
      service.RegisterArtisan(data),
  });
};