import React from "react";
import type {
  FieldErrors,
  SubmitHandler,
  UseFormHandleSubmit,
  UseFormRegister,
} from "react-hook-form";
import { ArrowRight, Globe, Lock, Mail, User } from "lucide-react";
import type { RegisterFormData } from "./RegisterScema";
import axios from "axios";

// type LoginFormProps = {
//   register: UseFormRegister<LoginFormData>;
//   handleSubmit: UseFormHandleSubmit<LoginFormData>;
//   onSubmit: SubmitHandler<LoginFormData>;
//   errors: FieldErrors<LoginFormData>;
//   isSubmitting: boolean;
//   isPending: boolean;
//   error: unknown;
//   id: number;
//   setShowotp: (value: boolean) => void;
//   SendOTPRequest: (id: number) => void | Promise<void>;
// };
type RegisterFormProps = {
  handleSubmit: UseFormHandleSubmit<RegisterFormData>;

  register: UseFormRegister<RegisterFormData>;

  onSubmit: SubmitHandler<RegisterFormData>;
  SetShowOTP: React.Dispatch<React.SetStateAction<boolean>>;

  errors: FieldErrors<RegisterFormData>;
  error: unknown;
    SendOTPRequest: () => void | Promise<void>;
  isSubmitting: boolean;
  data2: {
    message: string;
  } | null;

  isSuccess: boolean;
};
export default function RegisterForm({
  handleSubmit,
  onSubmit,
  errors,
  error,
  SendOTPRequest,
  // isPending,
  isSubmitting,
  register,
  isSuccess,
  data2,
  SetShowOTP,
  // setShowotp
}: RegisterFormProps) {
  return (
    <div className="w-full flex flex-col justify-between p-8 lg:p-16 ">
      {/* رأس الصفحة */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <span className="text-2xl font-black text-blue-600">Sanay3y</span>
          <span className="text-gray-400">·</span>
          <span className="text-xl font-bold text-gray-800">صنايعي</span>
        </div>
        <button
          type="button"
          className="text-sm font-semibold text-gray-600 hover:text-blue-600 border border-gray-200 px-3 py-1.5 rounded-lg transition flex items-center gap-1.5"
        >
          <Globe className="w-3.5 h-3.5" />
          <span>EN</span>
        </button>
      </div>

      {/* محتوى النموذج */}
      <div className="max-w-md w-full mx-auto space-y-5 my-auto py-8">
        <div>
          <h1 className="text-3xl font-extrabold text-gray-900 tracking-tight">
            إنشاء حساب جديد
          </h1>
          <p className="mt-2 text-sm text-gray-600">
            أدخل بياناتك الأساسية للبدء في استخدام منصة صنايعي.
          </p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-3.5">
          {/* حقل الاسم الكامل */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              الاسم الكامل
            </label>
            <div className="relative">
              <input
                type="text"
                {...register("fullName")}
                placeholder="أدخل اسمك الكامل"
                className={`w-full bg-gray-50 border text-gray-900 text-sm rounded-xl p-3 pr-10 outline-none transition-all placeholder-gray-400 ${
                  errors.fullName
                    ? "border-red-500 focus:ring-red-500"
                    : "border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                }`}
              />
              <User className="w-4 h-4 text-gray-400 absolute right-3.5 top-3.5" />
            </div>
            {errors.fullName && (
              <p className="mt-1 text-xs text-red-600">
                {errors.fullName.message}
              </p>
            )}
          </div>

          {/* حقل موحد للبريد أو الهاتف (يتم إرسال أحدهما فقط للـ Backend) */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              البريد الإلكتروني أو رقم الهاتف
            </label>
            <div className="relative">
              <input
                type="text"
                {...register("identifier")}
                placeholder="name@example.com أو 010xxxxxxxx"
                className={`w-full bg-gray-50 border text-gray-900 text-sm rounded-xl p-3 pr-10 outline-none transition-all placeholder-gray-400 text-left dir-ltr ${
                  errors.identifier
                    ? "border-red-500 focus:ring-red-500"
                    : "border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                }`}
              />
              <Mail className="w-4 h-4 text-gray-400 absolute right-3.5 top-3.5" />
            </div>
            {errors.identifier && (
              <p className="mt-1 text-xs text-red-600 text-right">
                {errors.identifier.message}
              </p>
            )}
          </div>

          {/* حقل كلمة المرور */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              كلمة المرور (8 أحرف على الأقل)
            </label>
            <div className="relative">
              <input
                type="password"
                {...register("password")}
                placeholder="••••••••"
                className={`w-full bg-gray-50 border text-gray-900 text-sm rounded-xl p-3 pr-10 outline-none transition-all placeholder-gray-400 ${
                  errors.password
                    ? "border-red-500 focus:ring-red-500"
                    : "border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                }`}
              />
              <Lock className="w-4 h-4 text-gray-400 absolute right-3.5 top-3.5" />
            </div>
            {errors.password && (
              <p className="mt-1 text-xs text-red-600">
                {errors.password.message}
              </p>
            )}
          </div>

          {/* حقل تأكيد كلمة المرور */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              تأكيد كلمة المرور
            </label>
            <div className="relative">
              <input
                type="password"
                {...register("confirmPassword")}
                placeholder="••••••••"
                className={`w-full bg-gray-50 border text-gray-900 text-sm rounded-xl p-3 pr-10 outline-none transition-all placeholder-gray-400 ${
                  errors.confirmPassword
                    ? "border-red-500 focus:ring-red-500"
                    : "border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                }`}
              />
              <Lock className="w-4 h-4 text-gray-400 absolute right-3.5 top-3.5" />
            </div>
            {errors.confirmPassword && (
              <p className="mt-1 text-xs text-red-600">
                {errors.confirmPassword.message}
              </p>
            )}
          </div>
          {/* data message*/}
          {data2 && (
            <p className="text-green-700 font-bold">
              {data2.message}
              <br />
              wait for acctive profile
            </p>
          )}
          {/*error*/}
          {axios.isAxiosError(error) && (
            <p className="text-red-600 text-sm font-medium">
              {error.response?.data?.message ?? error.message}
            </p>
          )}
          {/* زر الإرسال */}
          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full mt-3 py-3 px-4 rounded-xl text-white font-semibold bg-blue-600 hover:bg-blue-700 shadow-lg shadow-blue-500/25 transition duration-200 flex items-center justify-center gap-2 disabled:opacity-50"
          >
            <span>{isSubmitting ? "جاري الإرسال..." : "إنشاء الحساب"}</span>
            <ArrowRight className="w-4 h-4 rotate-180" />
          </button>
        </form>
        {/*زر تفعيل الحساب */}
        {isSuccess && (
          <button onClick={() =>{
           void SendOTPRequest()
            SetShowOTP(true)
          } 
        }>تفعيل الحساب </button>
        )}
        {/* روابط سفلية */}
        <div className="flex items-center justify-between text-sm pt-4 border-t border-gray-100">
          <p className="text-gray-600">
            لديك حساب بالفعل؟{" "}
            <a
              href="#login"
              className="font-semibold text-blue-600 hover:underline"
            >
              سجّل الدخول
            </a>
          </p>
          <a
            href="#tour"
            className="text-gray-500 hover:text-gray-800 font-medium transition"
          >
            جولة تعريفية
          </a>
        </div>
      </div>

      <div></div>
    </div>
  );
}
