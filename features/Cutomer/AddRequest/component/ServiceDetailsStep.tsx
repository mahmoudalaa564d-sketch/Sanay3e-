import {  useFormContext } from "react-hook-form";

import type { ServiceRequestFormData } from "./ServiceRequestSchema";
import { ErrorMessage } from "./ErrorMessage";

type Props = {
  specialties: {
    id: number;
    name: string;
    nameEn: string;
  }[];

  isLoading: boolean;
};

const ServiceDetailsStep = ({
  specialties,
  isLoading,
}: Props) => {
  const {
    register,
    formState: { errors },
  } = useFormContext<ServiceRequestFormData>();

  return (
    <section>
      <h2 className="mb-6 text-xl font-bold text-gray-800">
        تفاصيل الخدمة
      </h2>

      <div className="grid gap-5 md:grid-cols-2">

        {/* Specialty */}
        <div>
          <label className="form-label">التخصص *</label>

          <select
            {...register("specialtyId", {
              valueAsNumber: true,
            })}
            className="form-input"
          >
            <option value={0}>
              {isLoading ? "جاري التحميل..." : "اختر التخصص"}
            </option>

            {specialties.map((specialty) => (
              <option key={specialty.id} value={specialty.id}>
                {specialty.name}
              </option>
            ))}
          </select>

          <ErrorMessage message={errors.specialtyId?.message} />
        </div>

        {/* Service Type */}
        <div>
          <label className="form-label">نوع الخدمة *</label>

          <input
            {...register("serviceTypeName")}
            className="form-input"
            placeholder="مثال: إصلاح تسريب مياه"
          />

          <ErrorMessage message={errors.serviceTypeName?.message} />
        </div>

        {/* Problem Description */}
        <div className="md:col-span-2">
          <label className="form-label">وصف المشكلة *</label>

          <textarea
            {...register("problemDescription")}
            rows={5}
            className="form-input resize-none"
            placeholder="اشرح المشكلة بالتفصيل..."
          />

          <ErrorMessage message={errors.problemDescription?.message} />
        </div>

        {/* Additional Notes */}
        <div className="md:col-span-2">
          <label className="form-label">ملاحظات إضافية</label>

          <textarea
            {...register("additionalNotes")}
            rows={3}
            className="form-input resize-none"
            placeholder="أي معلومات إضافية..."
          />

          <ErrorMessage message={errors.additionalNotes?.message} />
        </div>

      </div>
    </section>
  );
};

export default ServiceDetailsStep;