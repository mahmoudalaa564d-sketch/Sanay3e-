import { MapPin } from "lucide-react";

import AddressCard from "./AddressCard";

import type { Address } from "../types/Address";

interface Props {
  addresses: Address[];
  onEdit: (address: Address) => void;
  onDelete: (address: Address) => void;
  onSetDefault: (id: number) => void;
}

const AddressList = ({
  addresses,
  onEdit,
  onDelete,
  onSetDefault,
}: Props) => {

  if (addresses.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center rounded-2xl border border-dashed border-gray-300 bg-white py-16">

        <MapPin size={48} className="mb-4 text-gray-300" />

        <h3 className="text-lg font-bold text-gray-800">
          لا توجد عناوين
        </h3>

        <p className="mt-2 text-sm text-gray-500">
          أضف عنوانك الأول لاستقبال الفنيين
        </p>

      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 gap-5 lg:grid-cols-2">

      {Array.isArray(addresses) && addresses.map((address) => (
        <AddressCard
          key={address.id}
          address={address}
          onEdit={onEdit}
          onDelete={onDelete}
          onSetDefault={onSetDefault}
        />
      ))}

    </div>
  );
};

export default AddressList;