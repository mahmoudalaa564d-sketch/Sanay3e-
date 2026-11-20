import type { Specialty } from "../type/specialty";
import SpecialtyCard from "./SpecialtyCard";

interface SpecialtiesListProps {
  specialties: Specialty[];
  onEdit: (specialty: Specialty) => void;
  onDelete: (id: number) => void;
}

const SpecialtiesList = ({
  specialties,
  onEdit,
  onDelete,
}: SpecialtiesListProps) => {
  if (!specialties.length) {
    return (
      <div className="text-center py-12 text-gray-500">
        لا توجد تخصصات حتى الآن
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-5">
      {specialties.map((specialty) => (
        <SpecialtyCard
          key={specialty.id}
          specialty={specialty}
          onEdit={onEdit}
          onDelete={onDelete}
        />
      ))}
    </div>
  );
};

export default SpecialtiesList;