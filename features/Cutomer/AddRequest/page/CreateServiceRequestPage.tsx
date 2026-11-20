import { useState } from "react";

import {
  FormProvider,
  useForm,
} from "react-hook-form";

import { zodResolver } from "@hookform/resolvers/zod";

import {
  serviceRequestSchema,
  type ServiceRequestFormData,
} from "../component/ServiceRequestSchema";

import { useCreateOrder } from "../hooks/useCreateRequest";

import ServiceDetailsStep from "../component/ServiceDetailsStep";
import AddressStep from "../component/AddressStep";
import AppointmentStep from "../component/AppointmentStep";
import { useGetSpecialties } from "../hooks/useGetSpecialties";
const steps = [
  "تفاصيل الخدمة",
  "عنوان الخدمة",
  "الموعد والموقع",
];

const CreateServiceRequestPage = () => {
  const [currentStep, setCurrentStep] = useState(0);

  const methods = useForm<ServiceRequestFormData>({
    resolver: zodResolver(serviceRequestSchema),

    mode: "onTouched",

    defaultValues: {
      specialtyId: 0,
      serviceTypeName: "",
      problemDescription: "",
      additionalNotes: "",

      addressTitle: "",
      governorate: "",
      city: "",
      district: "",
      street: "",
      buildingNumber: "",
      floorNumber: "",
      apartmentNumber: "",
      landmark: "",

      isAsSoonAsPossible: true,
      scheduledAtUtc: null,

      latitude: null,
      longitude: null,
    },
  });

  const {
    handleSubmit,
    trigger,
  } = methods;

  const { data: specialties = [], isLoading: specialtiesLoading } =
    useGetSpecialties();

  const {
    mutate: createOrder,
    isPending,
    isSuccess,
    isError,
    error,
  } = useCreateOrder();

  // Fields validation for each step
  const stepFields = [
    [
      "specialtyId",
      "serviceTypeName",
      "problemDescription",
      "additionalNotes",
    ],
    [
      "addressTitle",
      "governorate",
      "city",
      "district",
      "street",
      "buildingNumber",
      "floorNumber",
      "apartmentNumber",
      "landmark",
    ],
    [
      "isAsSoonAsPossible",
      "scheduledAtUtc",
      "latitude",
      "longitude",
    ],
  ] as const;

  // Next Step
  const handleNext = async () => {
    const isValid = await trigger(stepFields[currentStep]);

    if (!isValid) return;

    setCurrentStep((prev) => prev + 1);

    window.scrollTo({
      top: 0,
      behavior: "smooth",
    });
  };

  // Previous Step
  const handlePrevious = () => {
    setCurrentStep((prev) => prev - 1);
  };

  // Submit
  const onSubmit = (data: ServiceRequestFormData) => {
    createOrder({
      ...data,

      scheduledAtUtc: data.isAsSoonAsPossible
        ? null
        : data.scheduledAtUtc
          ? new Date(data.scheduledAtUtc).toISOString()
          : null,
    });
  };

  return (
    <FormProvider {...methods}>
      <div dir="rtl" className="min-h-screen bg-gray-50 px-4 py-8 md:px-8">
        <div className="mx-auto max-w-4xl">

          {/* Header */}
          <div className="mb-8">
            <h1 className="text-2xl font-bold text-gray-800 md:text-3xl">
              إنشاء طلب خدمة
            </h1>

            <p className="mt-2 text-sm text-gray-500">
              أكمل الخطوات التالية للعثور على الفني المناسب.
            </p>
          </div>

          {/* Progress */}
          <div className="mb-6 flex items-center gap-3">
            {steps.map((step, index) => (
              <div
                key={step}
                className={`flex-1 rounded-lg p-3 text-center text-sm font-semibold ${
                  currentStep === index
                    ? "bg-[#0966C1] text-white"
                    : currentStep > index
                      ? "bg-blue-100 text-[#0966C1]"
                      : "bg-white text-gray-400"
                }`}
              >
                {index + 1}. {step}
              </div>
            ))}
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit(onSubmit)}>
            <div className="rounded-2xl border border-gray-200 bg-white p-5 md:p-7">

              {currentStep === 0 && (
                <ServiceDetailsStep
                  specialties={specialties}
                  isLoading={specialtiesLoading}
                />
              )}

              {currentStep === 1 && <AddressStep />}

              {currentStep === 2 && <AppointmentStep />}

              {/* Navigation */}
              <div className="mt-8 flex items-center justify-between border-t pt-5">

                <button
                  type="button"
                  onClick={handlePrevious}
                  disabled={currentStep === 0}
                  className="rounded-lg border px-6 py-3 font-semibold text-gray-600 disabled:opacity-30"
                >
                  السابق
                </button>

                {currentStep < 2 ? (
                  <button
                    type="button"
                    onClick={handleNext}
                    className="rounded-lg bg-[#0966C1] px-6 py-3 font-semibold text-white"
                  >
                    التالي ←
                  </button>
                ) : (
                  <button
                    type="submit"
                    disabled={isPending}
                    className="rounded-lg bg-[#0966C1] px-6 py-3 font-semibold text-white disabled:opacity-50"
                  >
                    {isPending ? "جاري الإرسال..." : "إرسال الطلب"}
                  </button>
                )}
              </div>

              {isError && (
                <p className="mt-4 text-sm text-red-600">
                  {error instanceof Error
                    ? error.message
                    : "حدث خطأ أثناء إرسال الطلب"}
                </p>
              )}

              {isSuccess && (
                <p className="mt-4 text-sm text-green-600">
                  تم إرسال طلبك بنجاح.
                </p>
              )}

            </div>
          </form>
        </div>
      </div>
    </FormProvider>
  );
};

export default CreateServiceRequestPage;