import { useState } from "react";
import { Plus, MapPin } from "lucide-react";

import AddressList from "../components/AddressList";
import AddressForm from "../components/AddressForm";
import DeleteAddressDialog from "../components/DeleteAddressDialog";

import {
  useAddresses,
  useCreateAddress,
  useUpdateAddress,
  useDeleteAddress,
  useSetDefaultAddress,
} from "../hooks/UseAddress";

import type {
  Address,
  CreateAddressDto,
} from "../types/Address";

const AddressesPage = () => {

  const [isFormOpen, setIsFormOpen] = useState(false);

  const [selectedAddress, setSelectedAddress] =
    useState<Address | null>(null);

  const [deleteAddress, setDeleteAddress] =
    useState<Address | null>(null);

  // Queries
  const {
    data: addresses = [],
    isLoading,
    isError,
    error,
  } = useAddresses();

  // Mutations
  const createMutation = useCreateAddress();
  const updateMutation = useUpdateAddress();
  const deleteMutation = useDeleteAddress();
  const defaultMutation = useSetDefaultAddress();

  const isFormPending =
    createMutation.isPending || updateMutation.isPending;

  // Open Create Form
  const handleCreate = () => {
    setSelectedAddress(null);
    setIsFormOpen(true);
  };

  // Open Edit Form
  const handleEdit = (address: Address) => {
    setSelectedAddress(address);
    setIsFormOpen(true);
  };

  // Create / Update
const handleSubmit = async (data: CreateAddressDto) => {
  try {
    console.log(
      "FORM DATA:",
      JSON.stringify(data, null, 2)
    );

    if (selectedAddress) {
      await updateMutation.mutateAsync({
        id: selectedAddress.id,
        address: data,
      });
    } else {
      await createMutation.mutateAsync(data);
    }

    setIsFormOpen(false);
    setSelectedAddress(null);

  } catch (error) {
    console.error("Address operation failed:", error);
  }
};

  // Delete
  const handleDelete = async () => {
    if (!deleteAddress) return;

    try {
      await deleteMutation.mutateAsync(deleteAddress.id);
      setDeleteAddress(null);

    } catch (error) {
      console.error("Delete failed:", error);
    }
  };

  // Set Default
  const handleSetDefault = async (id: number) => {
    try {
      await defaultMutation.mutateAsync(id);
    } catch (error) {
      console.error("Set default failed:", error);
    }
  };

  return (
    <div dir="rtl" className="min-h-screen bg-gray-50 p-5 md:p-8">

      <div className="mx-auto max-w-6xl">

        {/* Header */}
        <div className="mb-8 flex flex-col justify-between gap-5 sm:flex-row sm:items-center">

          <div>

            <div className="flex items-center gap-3">

              <div className="rounded-xl bg-blue-50 p-3 text-[#0966C1]">
                <MapPin size={24} />
              </div>

              <h1 className="text-2xl font-bold text-gray-900">
                عناويني
              </h1>

            </div>

            <p className="mt-3 text-sm text-gray-500">
              إدارة عناوينك المسجلة لاستقبال الفنيين
            </p>

          </div>

          <button
            onClick={handleCreate}
            className="flex items-center justify-center gap-2 rounded-xl bg-[#0966C1] px-5 py-3 font-semibold text-white shadow-sm transition hover:bg-blue-700"
          >
            <Plus size={20} />
            إضافة عنوان جديد
          </button>

        </div>

        {/* Content */}
        <div className="rounded-2xl border border-gray-100 bg-white p-5 md:p-7">

          <div className="mb-6 flex items-center justify-between">

            <h2 className="font-bold text-gray-900">
              العناوين المسجلة
            </h2>

            <span className="rounded-lg bg-blue-50 px-3 py-1 text-sm font-medium text-[#0966C1]">
              {addresses.length} عناوين
            </span>

          </div>

          {isLoading ? (

            <div className="py-16 text-center text-gray-500">
              جاري تحميل العناوين...
            </div>

          ) : isError ? (

            <div className="py-16 text-center text-red-600">
              حدث خطأ أثناء تحميل العناوين.
              <p className="mt-2 text-sm">
                {error instanceof Error ? error.message : ""}
              </p>
            </div>

          ) : (

            <AddressList
              addresses={addresses.addresses}
              onEdit={handleEdit}
              onDelete={setDeleteAddress}
              onSetDefault={handleSetDefault}
            />

          )}

        </div>

      </div>

      {/* Create / Update Modal */}
      {isFormOpen && (
        <AddressForm
          key={selectedAddress?.id ?? "new"}
          initialData={selectedAddress}
          onSubmit={handleSubmit}
          onClose={() => {
            setIsFormOpen(false);
            setSelectedAddress(null);
          }}
          isPending={isFormPending}
        />
      )}

      {/* Delete Modal */}
      <DeleteAddressDialog
        isOpen={!!deleteAddress}
        onClose={() => setDeleteAddress(null)}
        onConfirm={handleDelete}
        isPending={deleteMutation.isPending}
      />

    </div>
  );
};

export default AddressesPage;