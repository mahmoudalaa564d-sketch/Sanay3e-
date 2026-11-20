import type { FormEventHandler } from "react";
import type {
  UseFormRegister,
  FieldErrors,
} from "react-hook-form";

import {
  User,
  Lock,
  BriefcaseBusiness,
  CalendarDays,
  MapPin,
  FileImage,
  Wrench,
  CheckCircle2,
  Loader2,
} from "lucide-react";

import type { ArtisanRegisterFormData } from "./ArtisanRegisterSchema";

// ==================== Props ====================

type ArtisanRegisterFormProps = {
  register: UseFormRegister<ArtisanRegisterFormData>;
  errors: FieldErrors<ArtisanRegisterFormData>;
  onSubmit: FormEventHandler<HTMLFormElement>;
  isPending: boolean;
  isSuccess: boolean;
  onActivateOtp: () => void;
};

// ==================== Component ====================

export default function ArtisanRegisterForm({
  register,
  errors,
  onSubmit,
  isPending,
  isSuccess,
  onActivateOtp,
}: ArtisanRegisterFormProps) {
  const inputStyle =
    "w-full rounded-xl border border-gray-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-blue-500 focus:ring-2 focus:ring-blue-100";

  const labelStyle = "mb-2 block text-sm font-semibold text-gray-700";

  const errorStyle = "mt-1 text-xs text-red-500";

  return (
    <div className="min-h-screen bg-slate-50 px-4 py-10 sm:px-6 lg:px-8">
      <div className="mx-auto max-w-3xl">

        {/* Header */}

        <div className="mb-8 text-center">
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">
            انضم إلى صنايعي
          </h1>

          <p className="mt-3 text-sm text-slate-500">
            أنشئ حسابك كفني وابدأ في تقديم خدماتك للعملاء.
          </p>
        </div>

        {/* Form */}

        <form
          onSubmit={onSubmit}
          className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm"
        >
          <div className="space-y-10 px-6 py-8 sm:px-8">

            {/* ==================== Personal Information ==================== */}

            <section>
              <div className="mb-6 flex items-center gap-3">
                <div className="rounded-xl bg-blue-50 p-3 text-blue-600">
                  <User size={22} />
                </div>

                <div>
                  <h2 className="text-lg font-bold text-slate-900">
                    المعلومات الشخصية
                  </h2>

                  <p className="text-xs text-slate-500">
                    أدخل بياناتك الشخصية لإنشاء حسابك.
                  </p>
                </div>
              </div>

              <div className="grid gap-5 sm:grid-cols-2">

                {/* Full Name */}

                <div className="sm:col-span-2">
                  <label className={labelStyle}>
                    الاسم بالكامل
                  </label>

                  <input
                    type="text"
                    placeholder="أدخل اسمك بالكامل"
                    className={inputStyle}
                    {...register("fullName")}
                  />

                  {errors.fullName && (
                    <p className={errorStyle}>
                      {errors.fullName.message}
                    </p>
                  )}
                </div>

                {/* Email or Phone */}

                <div className="sm:col-span-2">
                  <label className={labelStyle}>
                    البريد الإلكتروني أو رقم الهاتف
                  </label>

                  <input
                    type="text"
                    placeholder="example@email.com أو 01xxxxxxxxx"
                    className={inputStyle}
                    {...register("identifier")}
                  />

                  {errors.identifier && (
                    <p className={errorStyle}>
                      {errors.identifier.message}
                    </p>
                  )}

                  <p className="mt-2 text-xs text-gray-400">
                    يمكنك التسجيل باستخدام البريد الإلكتروني أو رقم الهاتف.
                  </p>
                </div>

                {/* Password */}

                <div className="sm:col-span-2">
                  <label className={labelStyle}>
                    كلمة المرور
                  </label>

                  <div className="relative">
                    <Lock
                      size={18}
                      className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400"
                    />

                    <input
                      type="password"
                      placeholder="أدخل كلمة المرور"
                      className={`${inputStyle} pr-11`}
                      {...register("password")}
                    />
                  </div>

                  {errors.password && (
                    <p className={errorStyle}>
                      {errors.password.message}
                    </p>
                  )}
                </div>

              </div>
            </section>

            <div className="border-t border-slate-100" />

            {/* ==================== Professional Information ==================== */}

            <section>
              <div className="mb-6 flex items-center gap-3">
                <div className="rounded-xl bg-blue-50 p-3 text-blue-600">
                  <BriefcaseBusiness size={22} />
                </div>

                <div>
                  <h2 className="text-lg font-bold text-slate-900">
                    المعلومات المهنية
                  </h2>

                  <p className="text-xs text-slate-500">
                    أخبرنا عن مهنتك وخبراتك.
                  </p>
                </div>
              </div>

              <div className="grid gap-5 sm:grid-cols-2">

                {/* Profession */}

                <div>
                  <label className={labelStyle}>
                    المهنة
                  </label>

                  <select
                    className={inputStyle}
                    defaultValue=""
                    {...register("profession")}
                  >
                    <option value="" disabled>
                      اختر مهنتك
                    </option>

                    <option value="Electrician">كهربائي</option>
                    <option value="Plumber">سباك</option>
                    <option value="Carpenter">نجار</option>
                    <option value="Painter">نقاش</option>
                    <option value="AC Technician">فني تكييف</option>
                    <option value="Appliance Technician">
                      فني أجهزة منزلية
                    </option>
                    <option value="Other">أخرى</option>
                  </select>

                  {errors.profession && (
                    <p className={errorStyle}>
                      {errors.profession.message}
                    </p>
                  )}
                </div>

                {/* Experience Years */}

                <div>
                  <label className={labelStyle}>
                    سنوات الخبرة
                  </label>

                  <div className="relative">
                    <CalendarDays
                      size={18}
                      className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400"
                    />

                    <input
                      type="number"
                      min="0"
                      placeholder="عدد سنوات الخبرة"
                      className={`${inputStyle} pr-11`}
                      {...register("experienceYears", {
                        valueAsNumber: true,
                      })}
                    />
                  </div>

                  {errors.experienceYears && (
                    <p className={errorStyle}>
                      {errors.experienceYears.message}
                    </p>
                  )}
                </div>

                {/* Specialties */}

                <div>
                  <label className={labelStyle}>
                    التخصصات
                  </label>

                  <div className="relative">
                    <Wrench
                      size={18}
                      className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400"
                    />

                    <input
                      type="text"
                      placeholder="مثال: تأسيس كهرباء، صيانة"
                      className={`${inputStyle} pr-11`}
                      {...register("specialties")}
                    />
                  </div>

                  {errors.specialties && (
                    <p className={errorStyle}>
                      {errors.specialties.message}
                    </p>
                  )}
                </div>

                {/* Service Types */}

                <div>
                  <label className={labelStyle}>
                    أنواع الخدمات
                  </label>

                  <input
                    type="text"
                    placeholder="مثال: تركيب، صيانة، إصلاح"
                    className={inputStyle}
                    {...register("serviceTypes")}
                  />

                  {errors.serviceTypes && (
                    <p className={errorStyle}>
                      {errors.serviceTypes.message}
                    </p>
                  )}
                </div>

                {/* Bio */}

                <div className="sm:col-span-2">
                  <label className={labelStyle}>
                    نبذة عنك
                  </label>

                  <textarea
                    rows={4}
                    placeholder="اكتب نبذة مختصرة عن خبراتك وخدماتك..."
                    className={`${inputStyle} resize-none`}
                    {...register("bio")}
                  />

                  {errors.bio && (
                    <p className={errorStyle}>
                      {errors.bio.message}
                    </p>
                  )}
                </div>

              </div>
            </section>

            <div className="border-t border-slate-100" />

            {/* ==================== Address ==================== */}

            <section>
              <div className="mb-6 flex items-center gap-3">
                <div className="rounded-xl bg-blue-50 p-3 text-blue-600">
                  <MapPin size={22} />
                </div>

                <div>
                  <h2 className="text-lg font-bold text-slate-900">
                    العنوان
                  </h2>

                  <p className="text-xs text-slate-500">
                    حدد المنطقة التي تقدم فيها خدماتك.
                  </p>
                </div>
              </div>

              <div className="grid gap-5 sm:grid-cols-2">

                {/* Governorate */}

                <div>
                  <label className={labelStyle}>
                    المحافظة
                  </label>

                  <input
                    type="text"
                    placeholder="المحافظة"
                    className={inputStyle}
                    {...register("governorate")}
                  />

                  {errors.governorate && (
                    <p className={errorStyle}>
                      {errors.governorate.message}
                    </p>
                  )}
                </div>

                {/* Center */}

                <div>
                  <label className={labelStyle}>
                    المركز
                  </label>

                  <input
                    type="text"
                    placeholder="المركز"
                    className={inputStyle}
                    {...register("center")}
                  />

                  {errors.center && (
                    <p className={errorStyle}>
                      {errors.center.message}
                    </p>
                  )}
                </div>

                {/* Village */}

                <div className="sm:col-span-2">
                  <label className={labelStyle}>
                    القرية أو المنطقة
                  </label>

                  <input
                    type="text"
                    placeholder="القرية أو المنطقة"
                    className={inputStyle}
                    {...register("village")}
                  />

                  {errors.village && (
                    <p className={errorStyle}>
                      {errors.village.message}
                    </p>
                  )}
                </div>

              </div>
            </section>

            <div className="border-t border-slate-100" />

            {/* ==================== Images ==================== */}

            <section>
              <div className="mb-6 flex items-center gap-3">
                <div className="rounded-xl bg-blue-50 p-3 text-blue-600">
                  <FileImage size={22} />
                </div>

                <div>
                  <h2 className="text-lg font-bold text-slate-900">
                    المستندات والصور
                  </h2>

                  <p className="text-xs text-slate-500">
                    ارفع الصور المطلوبة للتحقق من حسابك.
                  </p>
                </div>
              </div>

              <div className="grid gap-6 sm:grid-cols-2">

                {/* National ID Front */}

                <div>
                  <label className={labelStyle}>
                    صورة البطاقة - الوجه الأمامي
                  </label>

                  <input
                    type="file"
                    accept="image/*"
                    className={inputStyle}
                    {...register("nationalIdFrontImage")}
                  />

                  {errors.nationalIdFrontImage && (
                    <p className={errorStyle}>
                      {errors.nationalIdFrontImage.message}
                    </p>
                  )}
                </div>

                {/* National ID Back */}

                <div>
                  <label className={labelStyle}>
                    صورة البطاقة - الوجه الخلفي
                  </label>

                  <input
                    type="file"
                    accept="image/*"
                    className={inputStyle}
                    {...register("nationalIdBackImage")}
                  />

                  {errors.nationalIdBackImage && (
                    <p className={errorStyle}>
                      {errors.nationalIdBackImage.message}
                    </p>
                  )}
                </div>

                {/* Profile Image */}

                <div className="sm:col-span-2">
                  <label className={labelStyle}>
                    الصورة الشخصية
                  </label>

                  <input
                    type="file"
                    accept="image/*"
                    className={inputStyle}
                    {...register("profileImage")}
                  />

                  {errors.profileImage && (
                    <p className={errorStyle}>
                      {errors.profileImage.message}
                    </p>
                  )}
                </div>

              </div>
            </section>

            <div className="border-t border-slate-100" />

            {/* ==================== Terms ==================== */}

            <section>
              <label className="flex cursor-pointer items-start gap-3">
                <input
                  type="checkbox"
                  className="mt-1 h-4 w-4 accent-blue-600"
                  {...register("termsAccepted")}
                />

                <span className="text-sm leading-6 text-gray-600">
                  أوافق على الشروط والأحكام وسياسة الخصوصية الخاصة بمنصة صنايعي.
                </span>
              </label>

              {errors.termsAccepted && (
                <p className={errorStyle}>
                  {errors.termsAccepted.message}
                </p>
              )}
            </section>

            {/* ==================== Submit ==================== */}

            <div className="space-y-4">

              <button
                type="submit"
                disabled={isPending || isSuccess}
                className="flex w-full items-center justify-center gap-2 rounded-xl bg-blue-600 px-6 py-4 font-bold text-white transition hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {isPending ? (
                  <>
                    <Loader2 size={20} className="animate-spin" />
                    جاري إنشاء الحساب...
                  </>
                ) : isSuccess ? (
                  <>
                    <CheckCircle2 size={20} />
                    تم إنشاء الحساب
                  </>
                ) : (
                  "إنشاء حساب فني"
                )}
              </button>

              {/* OTP Activation */}

              {isSuccess && (
                <div className="rounded-xl border border-green-200 bg-green-50 p-5 text-center">

                  <div className="mb-2 flex justify-center text-green-600">
                    <CheckCircle2 size={30} />
                  </div>

                  <h3 className="font-bold text-green-800">
                    تم إنشاء حسابك بنجاح
                  </h3>

                  <p className="mt-2 text-sm text-green-700">
                    قم بتفعيل حسابك للمتابعة.
                  </p>

                  <button
                    type="button"
                    onClick={onActivateOtp}
                    className="mt-4 rounded-xl bg-green-600 px-8 py-3 font-semibold text-white transition hover:bg-green-700"
                  >
                    تفعيل الحساب
                  </button>

                </div>
              )}

            </div>

          </div>
        </form>
      </div>
    </div>
  );
}