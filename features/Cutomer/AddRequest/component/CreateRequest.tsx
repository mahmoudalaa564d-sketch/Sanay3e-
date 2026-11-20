import { useState } from "react";
import {
  ClipboardList,
  MapPin,
  CalendarDays,
  Clock,
  Home,
  Navigation,
  FileText,
} from "lucide-react";

const CreateServiceRequestPage = () => {
  const [isAsSoonAsPossible, setIsAsSoonAsPossible] = useState(true);

  return (
    <div dir="rtl" className="min-h-screen bg-gray-50 px-4 py-8 md:px-8">

      <div className="mx-auto max-w-4xl">

        {/* Header */}
        <div className="mb-8">
          <h1 className="text-2xl font-bold text-gray-800 md:text-3xl">
            إنشاء طلب خدمة
          </h1>

          <p className="mt-2 text-sm text-gray-500">
            املأ البيانات التالية لإرسال طلبك والعثور على الفني المناسب.
          </p>
        </div>

        <div className="space-y-6">

          {/* Service Details */}
          <section className="rounded-2xl border border-gray-200 bg-white p-5 md:p-7">

            <SectionHeader
              icon={<ClipboardList size={22} />}
              title="تفاصيل الخدمة"
              description="حدد نوع الخدمة واشرح المشكلة التي تواجهك."
            />

            <div className="grid gap-5 md:grid-cols-2">

              <div>
                <label className="form-label">
                  التخصص <Required />
                </label>

                <select
                  name="specialtyId"
                  required
                  className="form-input"
                  defaultValue=""
                >
                  <option value="" disabled>
                    اختر التخصص
                  </option>

                  <option value="1">كهرباء</option>
                  <option value="2">سباكة</option>
                  <option value="3">تكييف</option>
                  <option value="4">أجهزة منزلية</option>
                </select>
              </div>

              <Input
                label="نوع الخدمة"
                name="serviceTypeName"
                placeholder="مثال: إصلاح تسريب مياه"
                maxLength={150}
                required
              />

              <div className="md:col-span-2">
                <label className="form-label">
                  وصف المشكلة <Required />
                </label>

                <textarea
                  name="problemDescription"
                  required
                  minLength={5}
                  maxLength={3000}
                  rows={5}
                  placeholder="اشرح المشكلة التي تواجهك بالتفصيل..."
                  className="form-input resize-none"
                />

                <p className="mt-1 text-xs text-gray-400">
                  الحد الأدنى 5 أحرف والحد الأقصى 3000 حرف.
                </p>
              </div>

              <div className="md:col-span-2">
                <label className="form-label">
                  ملاحظات إضافية
                </label>

                <textarea
                  name="additionalNotes"
                  maxLength={1000}
                  rows={3}
                  placeholder="أي معلومات إضافية تريد إخبار الفني بها..."
                  className="form-input resize-none"
                />
              </div>

            </div>
          </section>

          {/* Address Details */}
          <section className="rounded-2xl border border-gray-200 bg-white p-5 md:p-7">

            <SectionHeader
              icon={<Home size={22} />}
              title="عنوان تنفيذ الخدمة"
              description="حدد العنوان الذي تريد وصول الفني إليه."
            />

            <div className="grid gap-5 md:grid-cols-2">

              <Input
                label="اسم العنوان"
                name="addressTitle"
                placeholder="مثال: المنزل أو العمل"
                maxLength={100}
                required
              />

              <Input
                label="المحافظة"
                name="governorate"
                placeholder="مثال: الغربية"
                maxLength={100}
                required
              />

              <Input
                label="المدينة"
                name="city"
                placeholder="مثال: طنطا"
                maxLength={100}
                required
              />

              <Input
                label="المنطقة / الحي"
                name="district"
                placeholder="اسم المنطقة أو الحي"
                maxLength={100}
              />

              <Input
                label="الشارع"
                name="street"
                placeholder="اسم الشارع"
                maxLength={200}
                required
              />

              <Input
                label="رقم المبنى"
                name="buildingNumber"
                placeholder="رقم المبنى"
                maxLength={50}
              />

              <Input
                label="رقم الدور"
                name="floorNumber"
                placeholder="رقم الدور"
                maxLength={50}
              />

              <Input
                label="رقم الشقة"
                name="apartmentNumber"
                placeholder="رقم الشقة"
                maxLength={50}
              />

              <div className="md:col-span-2">
                <Input
                  label="علامة مميزة بالقرب من العنوان"
                  name="landmark"
                  placeholder="مثال: بجوار مسجد أو مدرسة..."
                  maxLength={250}
                />
              </div>

            </div>
          </section>

          {/* Appointment */}
          <section className="rounded-2xl border border-gray-200 bg-white p-5 md:p-7">

            <SectionHeader
              icon={<CalendarDays size={22} />}
              title="موعد تنفيذ الخدمة"
              description="اختر الوقت المناسب لتنفيذ طلبك."
            />

            <div className="grid gap-4 sm:grid-cols-2">

              {/* As Soon As Possible */}
              <button
                type="button"
                onClick={() => setIsAsSoonAsPossible(true)}
                className={`rounded-xl border p-5 text-right transition ${
                  isAsSoonAsPossible
                    ? "border-[#0966C1] bg-blue-50"
                    : "border-gray-200 hover:border-gray-300"
                }`}
              >
                <div className="mb-3 flex items-center justify-between">
                  <Clock
                    className={
                      isAsSoonAsPossible
                        ? "text-[#0966C1]"
                        : "text-gray-400"
                    }
                    size={24}
                  />

                  <span
                    className={`h-4 w-4 rounded-full border-2 ${
                      isAsSoonAsPossible
                        ? "border-[#0966C1] bg-[#0966C1]"
                        : "border-gray-300"
                    }`}
                  />
                </div>

                <h3 className="font-bold text-gray-800">
                  في أسرع وقت ممكن
                </h3>

                <p className="mt-1 text-sm text-gray-500">
                  أريد فنيًا في أقرب وقت متاح.
                </p>
              </button>

              {/* Scheduled */}
              <button
                type="button"
                onClick={() => setIsAsSoonAsPossible(false)}
                className={`rounded-xl border p-5 text-right transition ${
                  !isAsSoonAsPossible
                    ? "border-[#0966C1] bg-blue-50"
                    : "border-gray-200 hover:border-gray-300"
                }`}
              >
                <div className="mb-3 flex items-center justify-between">
                  <CalendarDays
                    className={
                      !isAsSoonAsPossible
                        ? "text-[#0966C1]"
                        : "text-gray-400"
                    }
                    size={24}
                  />

                  <span
                    className={`h-4 w-4 rounded-full border-2 ${
                      !isAsSoonAsPossible
                        ? "border-[#0966C1] bg-[#0966C1]"
                        : "border-gray-300"
                    }`}
                  />
                </div>

                <h3 className="font-bold text-gray-800">
                  تحديد موعد لاحق
                </h3>

                <p className="mt-1 text-sm text-gray-500">
                  أريد اختيار تاريخ ووقت محددين.
                </p>
              </button>

            </div>

            {/* Scheduled Date */}
            {!isAsSoonAsPossible && (
              <div className="mt-5">
                <label className="form-label">
                  التاريخ والوقت <Required />
                </label>

                <input
                  type="datetime-local"
                  name="scheduledAtUtc"
                  required
                  className="form-input"
                />
              </div>
            )}

            {/* Hidden API Field */}
            <input
              type="hidden"
              name="isAsSoonAsPossible"
              value={String(isAsSoonAsPossible)}
            />

          </section>

          {/* Location Coordinates */}
          <section className="rounded-2xl border border-gray-200 bg-white p-5 md:p-7">

            <SectionHeader
              icon={<Navigation size={22} />}
              title="الموقع الجغرافي"
              description="يمكنك إضافة إحداثيات موقعك لمساعدة الفني في الوصول إليك."
            />

            <div className="grid gap-5 md:grid-cols-2">

              <Input
                label="خط العرض (Latitude)"
                name="latitude"
                type="number"
                placeholder="مثال: 30.7865"
                min="-90"
                max="90"
                step="any"
              />

              <Input
                label="خط الطول (Longitude)"
                name="longitude"
                type="number"
                placeholder="مثال: 31.0004"
                min="-180"
                max="180"
                step="any"
              />

            </div>

            <button
              type="button"
              className="mt-5 flex items-center gap-2 rounded-lg border border-[#0966C1] px-4 py-2.5 text-sm font-semibold text-[#0966C1] transition hover:bg-blue-50"
            >
              <MapPin size={18} />
              تحديد موقعي على الخريطة
            </button>

          </section>

          {/* Submit */}
          <div className="rounded-2xl border border-gray-200 bg-white p-5 md:p-7">

            <div className="mb-5 flex items-start gap-3">
              <FileText className="mt-1 text-[#0966C1]" size={22} />

              <div>
                <h3 className="font-bold text-gray-800">
                  مراجعة طلبك
                </h3>

                <p className="mt-1 text-sm text-gray-500">
                  تأكد من صحة البيانات قبل إرسال الطلب.
                </p>
              </div>
            </div>

            <button
              type="button"
              className="w-full rounded-xl bg-[#0966C1] py-4 font-bold text-white transition hover:bg-blue-700"
            >
              إرسال طلب الخدمة
            </button>

          </div>

        </div>
      </div>

      {/* Styles */}
      <style>{`
        .form-label {
          display: block;
          margin-bottom: 8px;
          font-size: 14px;
          font-weight: 600;
          color: #374151;
        }

        .form-input {
          width: 100%;
          border: 1px solid #e5e7eb;
          border-radius: 10px;
          padding: 12px 14px;
          background-color: white;
          color: #374151;
          font-size: 14px;
          outline: none;
          transition: all 0.2s;
        }

        .form-input:focus {
          border-color: #0966C1;
          box-shadow: 0 0 0 3px rgba(9, 102, 193, 0.1);
        }

        .form-input::placeholder {
          color: #9ca3af;
        }
      `}</style>

    </div>
  );
};

/* Reusable Input */

type InputProps = {
  label: string;
  name: string;
  placeholder: string;
  maxLength?: number;
  required?: boolean;
  type?: string;
  min?: string;
  max?: string;
  step?: string;
};

const Input = ({
  label,
  name,
  placeholder,
  maxLength,
  required = false,
  type = "text",
  min,
  max,
  step,
}: InputProps) => {
  return (
    <div>
      <label htmlFor={name} className="form-label">
        {label}
        {required && <Required />}
      </label>

      <input
        id={name}
        name={name}
        type={type}
        placeholder={placeholder}
        maxLength={maxLength}
        required={required}
        min={min}
        max={max}
        step={step}
        className="form-input"
      />
    </div>
  );
};

/* Reusable Section Header */

const SectionHeader = ({
  icon,
  title,
  description,
}: {
  icon: React.ReactNode;
  title: string;
  description: string;
}) => {
  return (
    <div className="mb-6 flex items-start gap-3 border-b border-gray-100 pb-5">
      <div className="rounded-xl bg-blue-50 p-3 text-[#0966C1]">
        {icon}
      </div>

      <div>
        <h2 className="font-bold text-gray-800">
          {title}
        </h2>

        <p className="mt-1 text-sm text-gray-500">
          {description}
        </p>
      </div>
    </div>
  );
};

/* Required Mark */

const Required = () => (
  <span className="mr-1 text-red-500">*</span>
);

export default CreateServiceRequestPage;