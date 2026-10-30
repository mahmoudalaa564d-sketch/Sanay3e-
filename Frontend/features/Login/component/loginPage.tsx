import React, { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { UseLogin } from "../hooks/UseLogin";
import axios from "axios";
import VerifyOtp from "../../OTP/component/otp";
import { UseOTPRequest } from "../../OTP/hooks/useOTPRequesr";
// 1. تعريف schema للتحقق من صحة البيانات باستخدام Zod
const loginSchema = z.object({
  identifier: z
    .string()
    .min(1, { message: "برجاء إدخال البريد الإلكتروني أو رقم الهاتف" }),
  password: z
    .string()
    .min(6, { message: "كلمة المرور يجب أن تكون 6 أحرف على الأقل" }),
});

export default function Sanay3yLogin() {
  const [showotp, setShowotp] = useState(false);
  const [identifier, setidentifier] = useState("");
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      identifier: "",
      password: "",
    },
  });
  const { mutateAsync: otp, error: otpError } = UseOTPRequest();
  const SendOTPRequest = async (id: number) => {
      const input =identifier.trim();
    const isPhone = /^\+?\d+$/.test(input);

    const channel =isPhone?"Phone":"Email";
    const response = await otp({ userId: id, channel: channel });
    console.log("re", response);
    if (otpError) {
      console.log(otpError.message);
    }
  };
  const { mutateAsync, error, isPending } = UseLogin();
  const [id, setid] = useState(0);
  const onSubmit = async (data: z.infer<typeof loginSchema>) => {
    setidentifier(data.identifier);
    try {
      const response = await mutateAsync(data);
      console.log("Login successful:", response);
    } catch (error: unknown) {
      if (axios.isAxiosError(error)) {
        const userId = error.response?.data?.userId;

        if (userId) {
          setid(Number(userId));
        }

        console.log("User ID:", userId);
      }
    }
  };

  return (
    <div
      className="flex h-screen w-full flex-row-reverse font-sans overflow-hidden bg-white"
      dir="rtl"
    >
      {/* النصف الأيمن: نموذج تسجيل الدخول والحقول */}
      <div className="w-full lg:w-1/2 flex flex-col justify-between p-8 lg:p-16 overflow-y-auto">
        {/* رأس الصفحة */}
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <span className="text-2xl font-black text-blue-600">Sanay3y</span>
            <span className="text-gray-400">·</span>
            <span className="text-xl font-bold text-gray-800">صنايعي</span>
          </div>
          <button className="text-sm font-semibold text-gray-600 hover:text-blue-600 border border-gray-200 px-3 py-1.5 rounded-lg transition">
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

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            {/* حقل البريد الإلكتروني أو رقم الهاتف */}
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

            {/* حقل كلمة المرور */}
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

            {/* زر تسجيل الدخول */}
            {error && (
              <p className="text-red-600">
                {axios.isAxiosError(error)
                  ? (error.response?.data?.message ??
                    "حدث خطأ أثناء تسجيل الدخول")
                  : error.message}
              </p>
            )}

            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full py-3.5 px-4 rounded-xl text-white font-semibold bg-blue-600 hover:bg-blue-700 shadow-lg shadow-blue-500/25 transition duration-200 disabled:opacity-50"
            >
              {isPending ? "جاري تسجيل الدخول..." : "تسجيل الدخول"}
            </button>
            {id > 0 && (
              <button
                onClick={() => {
                  setShowotp(true);
                  SendOTPRequest(id);
                }}
                className="block text-center text-blue-600 font-semibold hover:underline"
              >
                تفعيل الحساب
              </button>
            )}
          </form>
          {showotp && <VerifyOtp userId={id} identifier={identifier} />}

          {/* روابط سفلية */}
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

      {/* النصف الأيسر: القسم الجمالي */}
      <div className="hidden lg:flex lg:w-1/2 bg-gradient-to-br from-blue-600 to-blue-800 flex-col justify-between p-16 text-white relative overflow-hidden">
        <div className="absolute -top-24 -left-24 w-96 h-96 rounded-full bg-blue-500/20 blur-3xl pointer-events-none"></div>
        <div className="absolute -bottom-24 -right-24 w-96 h-96 rounded-full bg-blue-400/20 blur-3xl pointer-events-none"></div>

        <div className="z-10">
          <span className="inline-block bg-blue-500/30 backdrop-blur-md text-blue-100 text-xs font-bold px-3 py-1.5 rounded-full border border-blue-400/30">
            Verified Platform
          </span>
        </div>

        <div className="z-10 space-y-6 max-w-lg">
          <h2 className="text-4xl lg:text-5xl font-black tracking-tight leading-tight">
            كل خدمات الصيانة المنزلية في مكان واحد
          </h2>
          <p className="text-blue-100 text-lg leading-relaxed">
            Verified technicians. Transparent offers. Effortless booking.
          </p>
        </div>

        <div className="z-10 flex items-center gap-4 bg-white/10 backdrop-blur-md p-4 rounded-2xl border border-white/10 w-fit">
          <div className="flex items-center gap-1 text-amber-400 text-lg">
            ★ ★ ★ ★ ★
          </div>
          <div>
            <p className="text-sm font-bold text-white">4.9 avg.</p>
            <p className="text-xs text-blue-200">Verified technicians</p>
          </div>
        </div>
      </div>
    </div>
  );
}
