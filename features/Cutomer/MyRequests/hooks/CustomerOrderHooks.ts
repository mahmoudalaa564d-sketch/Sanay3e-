import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import {
  getOrders,
  getOrderById,
  cancelOrder,
} from "../services/OrderService";

// Get All Orders
export const useGetOrders = () => {
  return useQuery({
    queryKey: ["orders"],
    queryFn: getOrders,
  });
};

// Get Order By ID
export const useGetOrderById = (id: number) => {
  return useQuery({
    queryKey: ["orders", id],
    queryFn: () => getOrderById(id),
    enabled: !!id,
  });
};

// Cancel Order
export const useCancelOrder = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: cancelOrder,

    onSuccess: (_, id) => {
      console.log("Order Cancelled Successfully");

      // Refresh Orders
      queryClient.invalidateQueries({
        queryKey: ["orders"],
      });

      // Refresh Specific Order
      queryClient.invalidateQueries({
        queryKey: ["orders", id],
      });
    },

    onError: (error) => {
      console.error("Cancel Order Error:", error);
    },
  });
};