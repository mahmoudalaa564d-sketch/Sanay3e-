import {
  useQuery,
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";

import {
  getWallet,
  createTopUp,
  getTransactions,
  createWalletPayment,
} from "../services/WalletService";

// ================= Wallet =================

export const useGetWallet = () =>
  useQuery({
    queryKey: ["wallet"],
    queryFn: getWallet,
  });

// ================= Top Ups =================

export const useCreateTopUp = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: createTopUp,

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["wallet"] });
      queryClient.invalidateQueries({ queryKey: ["transactions"] });
    },
  });
};

// ================= Transactions =================

export const useGetTransactions = () =>
  useQuery({
    queryKey: ["transactions"],
    queryFn: getTransactions,
  });

// ================= Payments =================

export const useCreateWalletPayment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: createWalletPayment,

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["wallet"] });
      queryClient.invalidateQueries({ queryKey: ["transactions"] });
    },
  });
};