import { useFormContext } from "react-hook-form";

import type { ServiceRequestFormData } from "./ServiceRequestSchema";
import FormInput from "./FormInput";
import LocationPicker from "./LocationPicker";

const AddressStep = () => {
  const {
    register,
    formState: { errors },
  } = useFormContext<ServiceRequestFormData>();

  return (
    <section>
      <h2 className="mb-6 text-xl font-bold text-gray-800">
        عنوان تنفيذ الخدمة
      </h2>

      <div className="grid gap-5 md:grid-cols-2">
        <FormInput
          label="اسم العنوان"
          name="addressTitle"
          register={register}
          error={errors.addressTitle?.message}
          placeholder="مثال: المنزل"
        />

        <FormInput
          label="المحافظة"
          name="governorate"
          register={register}
          error={errors.governorate?.message}
          placeholder="مثال: الغربية"
        />

        <FormInput
          label="المدينة"
          name="city"
          register={register}
          error={errors.city?.message}
          placeholder="مثال: طنطا"
        />

        <FormInput
          label="المنطقة / الحي"
          name="district"
          register={register}
          error={errors.district?.message}
        />

        <FormInput
          label="الشارع"
          name="street"
          register={register}
          error={errors.street?.message}
        />

        <FormInput
          label="رقم المبنى"
          name="buildingNumber"
          register={register}
          error={errors.buildingNumber?.message}
        />

        <FormInput
          label="رقم الدور"
          name="floorNumber"
          register={register}
          error={errors.floorNumber?.message}
        />

        <FormInput
          label="رقم الشقة"
          name="apartmentNumber"
          register={register}
          error={errors.apartmentNumber?.message}
        />

        <div className="md:col-span-2">
          <FormInput
            label="علامة مميزة"
            name="landmark"
            register={register}
            error={errors.landmark?.message}
            placeholder="بجوار مسجد أو مدرسة..."
          />
        </div>
      </div>

      {/* اختيار الموقع من الخريطة */}
      <LocationPicker />
    </section>
  );
};

export default AddressStep;