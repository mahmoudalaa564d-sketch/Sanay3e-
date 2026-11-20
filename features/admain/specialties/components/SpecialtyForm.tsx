import { useEffect } from "react";
import { useForm } from "react-hook-form";

import type { SpecialtyRequest } from "../type/specialty";

interface SpecialtyFormProps {
  initialData?: SpecialtyRequest;
  onSubmit: (data: SpecialtyRequest) => void;
  isPending?: boolean;
}

const SpecialtyForm = ({
  initialData,
  onSubmit,
  isPending = false,
}: SpecialtyFormProps) => {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<SpecialtyRequest>({
    defaultValues: {
      name: "",
      nameEn: "",
      description: "",
      sortOrder: 1,
    },
  });

  useEffect(() => {
    reset(
      initialData ?? {
        name: "",
        nameEn: "",
        description: "",
        sortOrder: 1,
      }
    );
  }, [initialData, reset]);

  return (
    <form
      onSubmit={handleSubmit(onSubmit)}
      className="space-y-4"
      dir="rtl"
    >
      <div>
        <label className="block mb-2">اسم التخصص بالعربي</label>

        <input
          {...register("name", {
            required: "الاسم مطلوب",
            minLength: {
              value: 2,
              message: "الاسم يجب أن يكون حرفين على الأقل",
            },
          })}
          className="w-full border rounded-lg p-3 outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="مثلاً: كهرباء"
        />

        {errors.name && (
          <p className="text-red-500 text-sm mt-1">
            {errors.name.message}
          </p>
        )}
      </div>

      <div>
        <label className="block mb-2">اسم التخصص بالإنجليزية</label>

        <input
          {...register("nameEn", {
            required: "الاسم الإنجليزي مطلوب",
            minLength: {
              value: 2,
              message: "الاسم يجب أن يكون حرفين على الأقل",
            },
          })}
          className="w-full border rounded-lg p-3 outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="Electricity"
        />

        {errors.nameEn && (
          <p className="text-red-500 text-sm mt-1">
            {errors.nameEn.message}
          </p>
        )}
      </div>

      <div>
        <label className="block mb-2">الوصف</label>

        <textarea
          {...register("description")}
          rows={3}
          className="w-full border rounded-lg p-3 outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="وصف التخصص..."
        />
      </div>

      <div>
        <label className="block mb-2">ترتيب الظهور</label>

        <input
          type="number"
          {...register("sortOrder", {
            valueAsNumber: true,
            min: 0,
          })}
          className="w-full border rounded-lg p-3 outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <button
        type="submit"
        disabled={isPending}
        className="w-full bg-blue-600 text-white rounded-lg py-3 hover:bg-blue-700 disabled:opacity-50"
      >
        {isPending ? "جاري الحفظ..." : "حفظ"}
      </button>
    </form>
  );
};

export default SpecialtyForm;