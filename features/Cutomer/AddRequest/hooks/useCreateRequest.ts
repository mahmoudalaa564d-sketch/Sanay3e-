import { useMutation } from "@tanstack/react-query";
import type { CreateServiceRequestDto } from "../type/CreateServiceRequestDto";
import CustomerOrdersService from "../api/CustomerOrdersService";

export const useCreateOrder = () => {
  return useMutation({
    mutationFn: (data: CreateServiceRequestDto) =>
      CustomerOrdersService.createOrder(data),
  });
};