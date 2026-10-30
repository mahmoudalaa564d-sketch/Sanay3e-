import type {
  UseFormRegister,
  UseFormHandleSubmit,
  FieldErrors,
  SubmitHandler,
} from "react-hook-form";
import  type { LoginFormData } from "./LoginScema";
import axios from "axios";


type LoginFormProps = {
  register: UseFormRegister<LoginFormData>;
  handleSubmit: UseFormHandleSubmit<LoginFormData>;
  onSubmit: SubmitHandler<LoginFormData>;
  errors: FieldErrors<LoginFormData>;
  isSubmitting: boolean;
  isPending: boolean;
  error: unknown;
  id: number;
  setShowotp: (value: boolean) => void;
  SendOTPRequest: (id: number) => void | Promise<void>;
};

export default function LoginForm({
  register,
  handleSubmit,
  onSubmit,
  errors,
  isSubmitting,
  isPending,
  error,
  id,
  setShowotp,
  SendOTPRequest,
}: LoginFormProps) {
  return (
    <div className="w-full flex flex-col justify-between p-8 lg:p-16 overflow-y-auto">
      {/* رأس الصفحة */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <span className="text-2xl font-black text-blue-600">
            Sanay3y
          </span>

          <span className="text-gray-400">·</span>

          <span className="text-xl font-bold text-gray-800">
            صنايعي
          </span>
        </div>

        <button
          type="button"
          className="text-sm font-semibold text-gray-600 hover:text-blue-600 border border-gray-200 px-3 py-1.5 rounded-lg transition"
        >
          EN
        </button>
      </div>

      {/* محتوى النموذج */}
      <div className="max-w-md w-full mx-auto space-y-6 my-auto py-8">
        <div>
          <h1 className="text-3xl font-extrabold text-gray-900 tracking-tight">
            مرحباً بعودتك
          </h1>

          <p className="mt-2 text-sm text-gray-600">
            أدخل بيانات حسابك للمتابعة وإدارة خدمات الصيانة الخاصة بك.
          </p>
        </div>

        <form
          onSubmit={handleSubmit(onSubmit)}
          className="space-y-4"
        >
          {/* البريد الإلكتروني أو رقم الهاتف */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">
              البريد الإلكتروني أو رقم الهاتف
            </label>

            <input
              type="text"
              {...register("identifier")}
              placeholder="name@example.com أو 010xxxxxxxx"
              className={`w-full bg-gray-50 border text-gray-900 text-sm rounded-xl p-3.5 outline-none transition ${
                errors.identifier
                  ? "border-red-500 focus:ring-red-500"
                  : "border-gray-300 focus:ring-blue-500 focus:border-blue-500"
              }`}
            />

            {errors.identifier && (
              <p className="mt-1 text-xs text-red-600">
                {errors.identifier.message}
              </p>
            )}
          </div>

          {/* كلمة المرور */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">
              كلمة المرور
            </label>

            <input
              type="password"
              {...register("password")}
              placeholder="••••••••"
              className={`w-full bg-gray-50 border text-gray-900 text-sm rounded-xl p-3.5 outline-none transition ${
                errors.password
                  ? "border-red-500 focus:ring-red-500"
                  : "border-gray-300 focus:ring-blue-500 focus:border-blue-500"
              }`}
            />

            {errors.password && (
              <p className="mt-1 text-xs text-red-600">
                {errors.password.message}
              </p>
            )}
          </div>

          {/* خيارات إضافية */}
          <div className="flex items-center justify-between text-sm py-1">
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
              />

              <span className="text-gray-600">تذكرني</span>
            </label>

            <a
              href="#forgot"
              className="text-blue-600 hover:underline font-medium"
            >
              نسيت كلمة المرور؟
            </a>
          </div>

          {/* رسالة الخطأ */}
          {error != null && (
            <p className="text-red-600">
              {axios.isAxiosError(error)
                ? error?.response?.data.message
                : "حدث خطأ أثناء تسجيل الدخول"}
            </p>
          )}

          {/* زر تسجيل الدخول */}
          <button
            type="submit"
            disabled={isSubmitting || isPending}
            className="w-full py-3.5 px-4 rounded-xl text-white font-semibold bg-blue-600 hover:bg-blue-700 shadow-lg shadow-blue-500/25 transition duration-200 disabled:opacity-50"
          >
            {isPending
              ? "جاري تسجيل الدخول..."
              : "تسجيل الدخول"}
          </button>

          {/* تفعيل الحساب */}
          {id > 0 && (
            <button
              type="button"
              onClick={() => {
                setShowotp(true);
                void SendOTPRequest(id);
              }}
              className="block text-center text-blue-600 font-semibold hover:underline"
            >
              تفعيل الحساب
            </button>
          )}
        </form>

        {/* الروابط السفلية */}
        <div className="flex items-center justify-between text-sm pt-4 border-t border-gray-100">
          <p className="text-gray-600">
            ليس لديك حساب؟{" "}
            <a
              href="#register"
              className="font-semibold text-blue-600 hover:underline"
            >
              أنشئ حساباً
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
