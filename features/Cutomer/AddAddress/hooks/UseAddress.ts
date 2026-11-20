import {
  useQuery,
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";

import AddressService from "../services/AddressService";

import type {
  CreateAddressDto,
  UpdateAddressDto,
} from "../types/Address";

const QUERY_KEY = ["addresses"];

// GET ALL ADDRESSES
export const useAddresses = () => {
  return useQuery({
    queryKey: QUERY_KEY,
    queryFn: () => AddressService.getAll(),
  });
};

// CREATE ADDRESS
export const useCreateAddress = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (address: CreateAddressDto) =>
      AddressService.create(address),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
    },
  });
};

// UPDATE ADDRESS
export const useUpdateAddress = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      id,
      address,
    }: {
      id: number;
      address: UpdateAddressDto;
    }) => AddressService.update(id, address),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
    },
  });
};

// DELETE ADDRESS
export const useDeleteAddress = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => AddressService.delete(id),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
    },
  });
};

// SET DEFAULT ADDRESS
export const useSetDefaultAddress = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => AddressService.setDefault(id),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
    },
  });
};