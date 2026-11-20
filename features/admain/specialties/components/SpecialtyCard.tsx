import type { Specialty } from "../type/specialty";

interface SpecialtyCardProps {
  specialty: Specialty;
  onEdit: (specialty: Specialty) => void;
  onDelete: (id: number) => void;
}

const SpecialtyCard = ({
  specialty,
  onEdit,
  onDelete,
}: SpecialtyCardProps) => {
  return (
    <div className="bg-white border rounded-xl p-5 shadow-sm">
      <div className="flex items-start justify-between">
        <div>
          <h3 className="font-bold text-lg">{specialty.name}</h3>

          <p className="text-gray-500 text-sm mt-1">
            {specialty.nameEn}
          </p>
        </div>

        <span
          className={`text-xs px-3 py-1 rounded-full ${
            specialty.isActive
              ? "bg-green-100 text-green-700"
              : "bg-red-100 text-red-700"
          }`}
        >
          {specialty.isActive ? "نشط" : "غير نشط"}
        </span>
      </div>

      <p className="text-gray-600 text-sm my-4">
        {specialty.description || "لا يوجد وصف"}
      </p>

      <div className="flex gap-3 border-t pt-4">
        <button
          onClick={() => onEdit(specialty)}
          className="text-blue-600 hover:text-blue-800"
        >
          تعديل
        </button>

        <button
          onClick={() => onDelete(specialty.id)}
          className="text-red-600 hover:text-red-800"
        >
          حذف
        </button>
      </div>
    </div>
  );
};

export default SpecialtyCard;