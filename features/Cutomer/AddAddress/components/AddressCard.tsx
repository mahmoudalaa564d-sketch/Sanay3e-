import {
  MapPin,
  Home,
  BriefcaseBusiness,
  Pencil,
  Trash2,
  Star,
} from "lucide-react";

import type { Address } from "../types/Address";

interface Props {
  address: Address;
  onEdit: (address: Address) => void;
  onDelete: (address: Address) => void;
  onSetDefault: (id: number) => void;
}

const AddressCard = ({
  address,
  onEdit,
  onDelete,
  onSetDefault,
}: Props) => {
  return (
    <div className="rounded-2xl border border-gray-200 bg-white p-5 transition hover:border-blue-200 hover:shadow-md">

      {/* Header */}
      <div className="mb-4 flex items-center justify-between">

        <div className="flex items-center gap-3">

          <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-blue-50 text-[#0966C1]">
            {address.title === "Home" ? (
              <Home size={22} />
            ) : (
              <BriefcaseBusiness size={22} />
            )}
          </div>

          <div>
            <h3 className="font-bold text-gray-900">
              {address.title}
            </h3>

            {address.isDefault && (
              <span className="mt-1 inline-flex items-center gap-1 text-xs font-medium text-green-600">
                <Star size={13} fill="currentColor" />
                العنوان الافتراضي
              </span>
            )}
          </div>

        </div>

        <MapPin className="text-gray-400" size={20} />

      </div>

      {/* Address Details */}
      <div className="space-y-2 text-sm text-gray-600">

        <p>
          <span className="font-semibold text-gray-800">المدينة:</span>{" "}
          {address.city}
        </p>

        <p>
          <span className="font-semibold text-gray-800">الشارع:</span>{" "}
          {address.street}
        </p>

        <p>
          <span className="font-semibold text-gray-800">رقم المبنى:</span>{" "}
          {address.buildingNumber}
        </p>

        <p>
          <span className="font-semibold text-gray-800">رقم الشقة:</span>{" "}
          {address.apartmentNumber || "غير محدد"}
        </p>

      </div>

      <div className="my-5 border-t border-gray-100" />

      {/* Actions */}
      <div className="flex flex-wrap items-center justify-between gap-3">

        {!address.isDefault ? (
          <button
            onClick={() => onSetDefault(address.id)}
            className="text-sm font-medium text-[#0966C1] hover:underline"
          >
            تعيين كعنوان افتراضي
          </button>
        ) : (
          <span className="text-sm text-green-600">
            العنوان الأساسي
          </span>
        )}

        <div className="flex gap-2">

          <button
            onClick={() => onEdit(address)}
            className="flex items-center gap-1 rounded-lg bg-blue-50 px-3 py-2 text-sm text-[#0966C1] hover:bg-blue-100"
          >
            <Pencil size={15} />
            تعديل
          </button>

          <button
            onClick={() => onDelete(address)}
            className="flex items-center gap-1 rounded-lg bg-red-50 px-3 py-2 text-sm text-red-600 hover:bg-red-100"
          >
            <Trash2 size={15} />
            حذف
          </button>

        </div>

      </div>

    </div>
  );
};

export default AddressCard;