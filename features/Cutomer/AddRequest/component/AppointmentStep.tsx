import { useFormContext } from "react-hook-form";

import { CalendarDays, Clock, MapPin } from "lucide-react";

import type { ServiceRequestFormData } from "./ServiceRequestSchema";
import { ErrorMessage } from "./ErrorMessage";

const AppointmentStep = () => {
  const {
    register,
    watch,
    setValue,
    formState: { errors },
  } = useFormContext<ServiceRequestFormData>();

  const isAsSoonAsPossible = watch("isAsSoonAsPossible");

  return (
    <section>
      <h2 className="mb-6 text-xl font-bold text-gray-800">
        موعد تنفيذ الخدمة
      </h2>

      <div className="grid gap-4 sm:grid-cols-2">

        {/* ASAP */}
        <button
          type="button"
          onClick={() => {
            setValue("isAsSoonAsPossible", true, {
              shouldValidate: true,
            });

            setValue("scheduledAtUtc", null, {
              shouldValidate: true,
            });
          }}
          className={`rounded-xl border p-5 text-right transition ${
            isAsSoonAsPossible
              ? "border-[#0966C1] bg-blue-50"
              : "border-gray-200"
          }`}
        >
          <Clock className="mb-3 text-[#0966C1]" />

          <h3 className="font-bold">في أسرع وقت ممكن</h3>

          <p className="mt-1 text-sm text-gray-500">
            أريد فنيًا في أقرب وقت متاح.
          </p>
        </button>

        {/* Scheduled */}
        <button
          type="button"
          onClick={() =>
            setValue("isAsSoonAsPossible", false, {
              shouldValidate: true,
            })
          }
          className={`rounded-xl border p-5 text-right transition ${
            !isAsSoonAsPossible
              ? "border-[#0966C1] bg-blue-50"
              : "border-gray-200"
          }`}
        >
          <CalendarDays className="mb-3 text-[#0966C1]" />

          <h3 className="font-bold">تحديد موعد لاحق</h3>

          <p className="mt-1 text-sm text-gray-500">
            أريد اختيار تاريخ ووقت محددين.
          </p>
        </button>

      </div>

      {!isAsSoonAsPossible && (
        <div className="mt-5">
          <label className="form-label">التاريخ والوقت *</label>

          <input
            type="datetime-local"
            {...register("scheduledAtUtc")}
            className="form-input"
          />

          <ErrorMessage message={errors.scheduledAtUtc?.message} />
        </div>
      )}

      {/* Coordinates */}
      <div className="mt-8 border-t border-gray-100 pt-6">

        <div className="mb-5 flex items-center gap-2">
          <MapPin className="text-[#0966C1]" />

          <h3 className="font-bold">الموقع الجغرافي</h3>
        </div>

        <div className="grid gap-5 md:grid-cols-2">

          <div>
            <label className="form-label">خط العرض Latitude</label>

            <input
              type="number"
              step="any"
              className="form-input"
              placeholder="30.7865"
              {...register("latitude", {
                setValueAs: (value) =>
                  value === "" ? null : Number(value),
              })}
            />

            <ErrorMessage message={errors.latitude?.message} />
          </div>

          <div>
            <label className="form-label">خط الطول Longitude</label>

            <input
              type="number"
              step="any"
              className="form-input"
              placeholder="31.0004"
              {...register("longitude", {
                setValueAs: (value) =>
                  value === "" ? null : Number(value),
              })}
            />

            <ErrorMessage message={errors.longitude?.message} />
          </div>

        </div>
      </div>
    </section>
  );
};

export default AppointmentStep;