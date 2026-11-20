import { useEffect } from "react";

import {
  X,
  MapPin,
  Home,
  MapPinned,
  Building2,
  CheckCircle2,
  Navigation,
} from "lucide-react";

import {
  useForm,
  FormProvider,
} from "react-hook-form";

import { zodResolver } from "@hookform/resolvers/zod";

import {
  addressSchema,
  type AddressFormValues,
} from "./AddressSchema";

import AddressLocationPicker from "./AddressLocationPicker";

import type { Address } from "../types/Address";

interface Props {
  initialData: Address | null;
  onSubmit: (data: AddressFormValues) => void;
  onClose: () => void;
  isPending: boolean;
}

const AddressForm = ({
  initialData,
  onSubmit,
  onClose,
  isPending,
}: Props) => {
  const methods = useForm<AddressFormValues>({
    resolver: zodResolver(addressSchema),

    defaultValues: {
      title: initialData?.title ?? "",
      governorate: initialData?.governorate ?? "",
      city: initialData?.city ?? "",
      district: initialData?.district ?? "",
      street: initialData?.street ?? "",
      buildingNumber: initialData?.buildingNumber ?? "",
      floorNumber: initialData?.floorNumber ?? "",
      apartmentNumber: initialData?.apartmentNumber ?? "",
      landmark: initialData?.landmark ?? "",
      latitude: initialData?.latitude ?? null,
      longitude: initialData?.longitude ?? null,
      isDefault: initialData?.isDefault ?? false,
    },
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = methods;

  useEffect(() => {
    reset({
      title: initialData?.title ?? "",
      governorate: initialData?.governorate ?? "",
      city: initialData?.city ?? "",
      district: initialData?.district ?? "",
      street: initialData?.street ?? "",
      buildingNumber: initialData?.buildingNumber ?? "",
      floorNumber: initialData?.floorNumber ?? "",
      apartmentNumber: initialData?.apartmentNumber ?? "",
      landmark: initialData?.landmark ?? "",
      latitude: initialData?.latitude ?? null,
      longitude: initialData?.longitude ?? null,
      isDefault: initialData?.isDefault ?? false,
    });
  }, [initialData, reset]);

  const fields = [
    {
      name: "title",
      label: "اسم العنوان",
      placeholder: "مثال: المنزل، العمل",
      required: true,
      icon: Home,
    },
    {
      name: "governorate",
      label: "المحافظة",
      placeholder: "مثال: الغربية",
      required: true,
      icon: MapPin,
    },
    {
      name: "city",
      label: "المدينة",
      placeholder: "مثال: طنطا",
      required: true,
      icon: Building2,
    },
    {
      name: "district",
      label: "المنطقة / الحي",
      placeholder: "مثال: وسط البلد",
      required: false,
      icon: MapPinned,
    },
    {
      name: "street",
      label: "اسم الشارع",
      placeholder: "مثال: شارع البحر",
      required: true,
      icon: Navigation,
    },
    {
      name: "buildingNumber",
      label: "رقم المبنى",
      placeholder: "رقم المبنى",
      required: false,
      icon: Building2,
    },
    {
      name: "floorNumber",
      label: "رقم الدور",
      placeholder: "رقم الدور",
      required: false,
      icon: Building2,
    },
    {
      name: "apartmentNumber",
      label: "رقم الشقة",
      placeholder: "رقم الشقة",
      required: false,
      icon: Home,
    },
    {
      name: "landmark",
      label: "علامة مميزة",
      placeholder: "مثال: بجوار المسجد",
      required: false,
      icon: MapPin,
    },
  ] as const;

  return (
    <div
      className="fixed inset-0 z-[100] flex items-center justify-center bg-slate-950/60 p-3 backdrop-blur-sm sm:p-5"
      dir="rtl"
    >
      <div className="flex max-h-[95vh] w-full max-w-3xl flex-col overflow-hidden rounded-3xl bg-white shadow-2xl">

        {/* Header */}
        <div className="relative overflow-hidden bg-[#0966C1] px-6 py-6 text-white sm:px-8">

          <div className="absolute -left-10 -top-16 h-48 w-48 rounded-full bg-white/10" />
          <div className="absolute -bottom-20 right-1/3 h-40 w-40 rounded-full bg-white/10" />

          <div className="relative flex items-center justify-between">

            <div className="flex items-center gap-4">

              <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-white/15 backdrop-blur">
                <MapPin size={28} />
              </div>

              <div>
                <h2 className="text-xl font-bold sm:text-2xl">
                  {initialData ? "تعديل العنوان" : "إضافة عنوان جديد"}
                </h2>

                <p className="mt-1 text-sm text-blue-100">
                  أضف تفاصيل موقعك لتسهيل الوصول إليك
                </p>
              </div>

            </div>

            <button
              type="button"
              onClick={onClose}
              className="rounded-xl p-2 text-white/80 transition hover:bg-white/15 hover:text-white"
            >
              <X size={23} />
            </button>

          </div>

        </div>

        {/* Form Content */}
        <div className="overflow-y-auto px-5 py-6 sm:px-8">

          <FormProvider {...methods}>
            <form
              onSubmit={handleSubmit(onSubmit)}
              className="space-y-8"
            >

              {/* Section 1: Basic Information */}
              <section>

                <div className="mb-5 flex items-center gap-3">

                  <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-blue-50 text-[#0966C1]">
                    <Home size={20} />
                  </div>

                  <div>
                    <h3 className="font-bold text-gray-900">
                      معلومات العنوان
                    </h3>

                    <p className="text-xs text-gray-500">
                      البيانات الأساسية لموقعك
                    </p>
                  </div>

                </div>

                <div className="grid grid-cols-1 gap-5 sm:grid-cols-2">

                  {fields.slice(0, 4).map((field) => {
                    const Icon = field.icon;

                    return (
                      <div
                        key={field.name}
                        className="space-y-2"
                      >

                        <label className="flex items-center gap-1.5 text-sm font-semibold text-gray-700">

                          {field.label}

                          {field.required && (
                            <span className="text-red-500">*</span>
                          )}

                        </label>

                        <div className="relative">

                          <Icon
                            size={18}
                            className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400"
                          />

                          <input
                            type="text"
                            placeholder={field.placeholder}
                            {...register(field.name)}
                            className={`w-full rounded-xl border bg-gray-50 py-3 pr-11 pl-4 text-sm text-gray-800 outline-none transition placeholder:text-gray-400 focus:bg-white focus:ring-4 ${
                              errors[field.name]
                                ? "border-red-400 focus:border-red-400 focus:ring-red-100"
                                : "border-gray-200 focus:border-[#0966C1] focus:ring-blue-100"
                            }`}
                          />

                        </div>

                        {errors[field.name] && (
                          <p className="text-xs text-red-500">
                            {errors[field.name]?.message}
                          </p>
                        )}

                      </div>
                    );
                  })}

                </div>

              </section>

              {/* Divider */}
              <div className="border-t border-gray-100" />

              {/* Section 2: Detailed Address */}
              <section>

                <div className="mb-5 flex items-center gap-3">

                  <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-blue-50 text-[#0966C1]">
                    <Building2 size={20} />
                  </div>

                  <div>
                    <h3 className="font-bold text-gray-900">
                      تفاصيل إضافية
                    </h3>

                    <p className="text-xs text-gray-500">
                      ساعد الفني في الوصول إليك بسهولة
                    </p>
                  </div>

                </div>

                <div className="grid grid-cols-1 gap-5 sm:grid-cols-2">

                  {fields.slice(4).map((field) => {
                    const Icon = field.icon;

                    return (
                      <div
                        key={field.name}
                        className="space-y-2"
                      >

                        <label className="flex items-center gap-1.5 text-sm font-semibold text-gray-700">

                          {field.label}

                          {field.required && (
                            <span className="text-red-500">*</span>
                          )}

                        </label>

                        <div className="relative">

                          <Icon
                            size={18}
                            className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400"
                          />

                          <input
                            type="text"
                            placeholder={field.placeholder}
                            {...register(field.name)}
                            className={`w-full rounded-xl border bg-gray-50 py-3 pr-11 pl-4 text-sm text-gray-800 outline-none transition placeholder:text-gray-400 focus:bg-white focus:ring-4 ${
                              errors[field.name]
                                ? "border-red-400 focus:border-red-400 focus:ring-red-100"
                                : "border-gray-200 focus:border-[#0966C1] focus:ring-blue-100"
                            }`}
                          />

                        </div>

                        {errors[field.name] && (
                          <p className="text-xs text-red-500">
                            {errors[field.name]?.message}
                          </p>
                        )}

                      </div>
                    );
                  })}

                </div>

              </section>

              {/* Divider */}
              <div className="border-t border-gray-100" />

              {/* Section 3: Map */}
              <section>

                <div className="mb-5 flex items-center gap-3">

                  <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-blue-50 text-[#0966C1]">
                    <MapPin size={20} />
                  </div>

                  <div>
                    <h3 className="font-bold text-gray-900">
                      تحديد الموقع على الخريطة
                    </h3>

                    <p className="text-xs text-gray-500">
                      حدد موقع منزلك بدقة حتى يتمكن الفني من الوصول إليك
                    </p>
                  </div>

                </div>

                <div className="overflow-hidden rounded-2xl border border-gray-200 bg-gray-50 p-3 sm:p-4">

                  <AddressLocationPicker />

                </div>

              </section>

              {/* Default Address */}
              <section className="rounded-2xl border border-blue-100 bg-blue-50/60 p-4">

                <label className="flex cursor-pointer items-center justify-between gap-3">

                  <div className="flex items-center gap-3">

                    <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-white text-[#0966C1]">
                      <CheckCircle2 size={21} />
                    </div>

                    <div>
                      <p className="text-sm font-bold text-gray-800">
                        تعيين كعنوان افتراضي
                      </p>

                      <p className="mt-1 text-xs text-gray-500">
                        استخدام هذا العنوان تلقائيًا عند طلب خدمة جديدة
                      </p>
                    </div>

                  </div>

                  <input
                    type="checkbox"
                    {...register("isDefault")}
                    className="h-5 w-5 cursor-pointer accent-[#0966C1]"
                  />

                </label>

              </section>

              {/* Actions */}
              <div className="flex flex-col-reverse gap-3 border-t border-gray-100 pt-5 sm:flex-row">

                <button
                  type="button"
                  onClick={onClose}
                  disabled={isPending}
                  className="rounded-xl border border-gray-200 px-6 py-3 text-sm font-semibold text-gray-600 transition hover:bg-gray-50 disabled:opacity-50 sm:w-32"
                >
                  إلغاء
                </button>

                <button
                  type="submit"
                  disabled={isPending}
                  className="flex flex-1 items-center justify-center gap-2 rounded-xl bg-[#0966C1] px-6 py-3 text-sm font-bold text-white shadow-lg shadow-blue-900/10 transition hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-60"
                >
                  {isPending ? (
                    <>
                      <span className="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
                      جاري الحفظ...
                    </>
                  ) : (
                    <>
                      <MapPin size={18} />
                      {initialData ? "حفظ التعديلات" : "إضافة العنوان"}
                    </>
                  )}
                </button>

              </div>

            </form>
          </FormProvider>

        </div>

      </div>
    </div>
  );
};

export default AddressForm;