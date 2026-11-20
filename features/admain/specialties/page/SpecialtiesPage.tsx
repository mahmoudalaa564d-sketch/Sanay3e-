import { useState } from "react";

import type {
  Specialty,
  SpecialtyRequest,
} from "../type/specialty";

import { useGetSpecialties } from "../hooks/useGetSpecialties";
import { useCreateSpecialty } from "../hooks/useCreateSpecialty";
import { useUpdateSpecialty } from "../hooks/useUpdateSpecialty";
import { useDeleteSpecialty } from "../hooks/useDeleteSpecialty";

import SpecialtiesList from "../components/SpecialtiesList";
import SpecialtyForm from "../components/SpecialtyForm";
import SpecialtyModal from "../components/SpecialtyModal";

const SpecialtiesPage = () => {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedSpecialty, setSelectedSpecialty] =
    useState<Specialty | null>(null);

  const {
    data: specialties = [],
    isLoading,
    isError,
  } = useGetSpecialties();

  const createMutation = useCreateSpecialty();
  const updateMutation = useUpdateSpecialty();
  const deleteMutation = useDeleteSpecialty();

  const handleAdd = () => {
    setSelectedSpecialty(null);
    setIsModalOpen(true);
  };

  const handleEdit = (specialty: Specialty) => {
    setSelectedSpecialty(specialty);
    setIsModalOpen(true);
  };

  const handleSubmit = (data: SpecialtyRequest) => {
    if (selectedSpecialty) {
      updateMutation.mutate(
        {
          id: selectedSpecialty.id,
          data,
        },
        {
          onSuccess: () => setIsModalOpen(false),
        }
      );
    } else {
      createMutation.mutate(data, {
        onSuccess: () => setIsModalOpen(false),
      });
    }
  };

  const handleDelete = (id: number) => {
    if (window.confirm("هل أنت متأكد من حذف هذا التخصص؟")) {
      deleteMutation.mutate(id);
    }
  };

  const isPending =
    createMutation.isPending || updateMutation.isPending;

  if (isLoading) return <p>جاري تحميل التخصصات...</p>;

  if (isError) return <p>حدث خطأ أثناء تحميل التخصصات</p>;

  return (
    <section className="p-6 space-y-6" dir="rtl">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">إدارة التخصصات</h1>
          <p className="text-gray-500 mt-1">
            إدارة تخصصات الفنيين والخدمات
          </p>
        </div>

        <button
          onClick={handleAdd}
          className="bg-blue-600 text-white px-5 py-3 rounded-lg hover:bg-blue-700"
        >
          + إضافة تخصص
        </button>
      </div>

      <SpecialtiesList
        specialties={specialties}
        onEdit={handleEdit}
        onDelete={handleDelete}
      />

      <SpecialtyModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={selectedSpecialty ? "تعديل التخصص" : "إضافة تخصص جديد"}
      >
        <SpecialtyForm
          initialData={
            selectedSpecialty
              ? {
                  name: selectedSpecialty.name,
                  nameEn: selectedSpecialty.nameEn,
                  description: selectedSpecialty.description,
                  sortOrder: selectedSpecialty.sortOrder,
                }
              : undefined
          }
          onSubmit={handleSubmit}
          isPending={isPending}
        />
      </SpecialtyModal>
    </section>
  );
};

export default SpecialtiesPage;