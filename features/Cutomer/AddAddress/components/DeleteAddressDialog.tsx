import { AlertTriangle } from "lucide-react";

interface Props {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  isPending: boolean;
}

const DeleteAddressDialog = ({
  isOpen,
  onClose,
  onConfirm,
  isPending,
}: Props) => {

  if (!isOpen) return null;

  return (
    <div
      dir="rtl"
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
    >

      <div className="w-full max-w-sm rounded-2xl bg-white p-6 text-center">

        <div className="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-full bg-red-50 text-red-600">
          <AlertTriangle size={28} />
        </div>

        <h2 className="text-lg font-bold text-gray-900">
          حذف العنوان
        </h2>

        <p className="mt-2 text-sm text-gray-500">
          هل أنت متأكد أنك تريد حذف هذا العنوان؟
          لا يمكن التراجع عن هذه العملية.
        </p>

        <div className="mt-6 flex gap-3">

          <button
            onClick={onConfirm}
            disabled={isPending}
            className="flex-1 rounded-xl bg-red-600 py-3 font-semibold text-white hover:bg-red-700 disabled:opacity-50"
          >
            {isPending ? "جاري الحذف..." : "تأكيد الحذف"}
          </button>

          <button
            onClick={onClose}
            className="flex-1 rounded-xl border border-gray-200 py-3 text-gray-600 hover:bg-gray-50"
          >
            إلغاء
          </button>

        </div>

      </div>

    </div>
  );
};

export default DeleteAddressDialog;