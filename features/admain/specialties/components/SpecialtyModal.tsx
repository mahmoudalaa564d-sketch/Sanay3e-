import type { ReactNode } from "react";

interface SpecialtyModalProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  children: ReactNode;
}

const SpecialtyModal = ({
  isOpen,
  onClose,
  title,
  children,
}: SpecialtyModalProps) => {
  if (!isOpen) return null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
      onClick={onClose}
    >
      <div
        className="bg-white w-full max-w-lg rounded-2xl p-6 shadow-xl"
        dir="rtl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-xl font-bold">{title}</h2>

          <button
            onClick={onClose}
            className="text-gray-500 text-2xl"
          >
            ×
          </button>
        </div>

        {children}
      </div>
    </div>
  );
};

export default SpecialtyModal;