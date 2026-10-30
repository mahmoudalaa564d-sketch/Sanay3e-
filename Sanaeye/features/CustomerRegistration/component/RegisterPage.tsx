import React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { 
  User, 
  Mail, 
  Lock, 
  ArrowRight,
  Globe
} from 'lucide-react';
import { UseRegister } from '../hooks/Register';
import type { RegisterPayload } from '../type/RegisterPayload';
import axios from 'axios';

// مخطط التحقق الموحد للحقل (إيميل أو رقم هاتف)
const registerSchema = z.object({
  fullName: z
    .string()
    .min(2, { message: 'الاسم الكامل يجب أن يكون حرفين على الأقل' }),
  identifier: z
    .string()
    .min(1, { message: 'برجاء إدخال البريد الإلكتروني أو رقم الهاتف' })
    .refine((val) => {
      const trimmed = val.trim();
      const isEmail = z.string().email().safeParse(trimmed).success;
      const isPhone = /^(\+)?\d+$/.test(trimmed);
      return isEmail || isPhone;
    }, { message: 'برجاء إدخال بريد إلكتروني صحيح أو رقم هاتف صالح' }),
  password: z
    .string()
    .min(8, { message: 'كلمة المرور يجب ألا تقل عن 8 أحرف' }),
  confirmPassword: z
    .string()
    .min(1, { message: 'برجاء تأكيد كلمة المرور' }),
}).refine((data) => data.password === data.confirmPassword, {
  message: 'كلمتا المرور غير متطابقتين',
  path: ['confirmPassword'],
});

type RegisterFormData = z.infer<typeof registerSchema>;

export default function RegisterPage() {
  const { mutateAsync } = UseRegister();
  
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      fullName: '',
      identifier: '',
      password: '',
      confirmPassword: '',
    },
  });

  // 💡 إرسال إما email أو phoneNumber حسب المدخل (وليس الاثنين معاً)
  const FormatData = (data: RegisterFormData): RegisterPayload => {
    const input = data.identifier.trim();
    const isPhone = /^(\+)?\d+$/.test(input);

    return {
      fullName: data.fullName,
      password: data.password,
      email: !isPhone ? input : undefined,       // إذا كان إيميل، نرسله ونترك الهاتف undefined
      phoneNumber: isPhone ? input : undefined, // إذا كان هاتف، نرسله ونترك الإيميل undefined
    };
  };

  const onSubmit = async (data: RegisterFormData) => {
    try {
      const apiPayload = FormatData(data);
      const response = await mutateAsync(apiPayload);
      console.log('تم التسجيل بنجاح:', response);
    } catch (error: unknown) {
    if(axios.isAxiosError(error)){

      console.error('خطأ أثناء التسجيل:', error.message);
    }
    }
  };

  return (
    <div className="flex h-screen w-full flex-row-reverse font-sans overflow-hidden bg-white" dir="rtl">
      
      {/* النصف الأيمن: نموذج التسجيل */}
      <div className="w-full lg:w-1/2 flex flex-col justify-between p-8 lg:p-16 overflow-y-auto bg-white">
        
        {/* رأس النموذج */}
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <span className="text-2xl font-black text-blue-600">Sanay3y</span>
            <span className="text-gray-400">·</span>
            <span className="text-xl font-bold text-gray-800">صنايعي</span>
          </div>
          <button type="button" className="text-sm font-semibold text-gray-600 hover:text-blue-600 border border-gray-200 px-3 py-1.5 rounded-lg transition flex items-center gap-1.5">
            <Globe className="w-3.5 h-3.5" />
            <span>EN</span>
          </button>
        </div>

        {/* محتوى النموذج */}
        <div className="max-w-md w-full mx-auto space-y-5 my-auto py-8">
          <div>
            <h1 className="text-3xl font-extrabold text-gray-900 tracking-tight">إنشاء حساب جديد</h1>
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
                  {...register('fullName')}
                  placeholder="أدخل اسمك الكامل"
                  className={`w-full bg-gray-50 border text-gray-900 text-sm rounded-xl p-3 pr-10 outline-none transition-all placeholder-gray-400 ${
                    errors.fullName ? 'border-red-500 focus:ring-red-500' : 'border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500'
                  }`}
                />
                <User className="w-4 h-4 text-gray-400 absolute right-3.5 top-3.5" />
              </div>
              {errors.fullName && (
                <p className="mt-1 text-xs text-red-600">{errors.fullName.message}</p>
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
                  {...register('identifier')}
                  placeholder="name@example.com أو 010xxxxxxxx"
                  className={`w-full bg-gray-50 border text-gray-900 text-sm rounded-xl p-3 pr-10 outline-none transition-all placeholder-gray-400 text-left dir-ltr ${
                    errors.identifier ? 'border-red-500 focus:ring-red-500' : 'border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500'
                  }`}
                />
                <Mail className="w-4 h-4 text-gray-400 absolute right-3.5 top-3.5" />
              </div>
              {errors.identifier && (
                <p className="mt-1 text-xs text-red-600 text-right">{errors.identifier.message}</p>
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
                  {...register('password')}
                  placeholder="••••••••"
                  className={`w-full bg-gray-50 border text-gray-900 text-sm rounded-xl p-3 pr-10 outline-none transition-all placeholder-gray-400 ${
                    errors.password ? 'border-red-500 focus:ring-red-500' : 'border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500'
                  }`}
                />
                <Lock className="w-4 h-4 text-gray-400 absolute right-3.5 top-3.5" />
              </div>
              {errors.password && (
                <p className="mt-1 text-xs text-red-600">{errors.password.message}</p>
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
                  {...register('confirmPassword')}
                  placeholder="••••••••"
                  className={`w-full bg-gray-50 border text-gray-900 text-sm rounded-xl p-3 pr-10 outline-none transition-all placeholder-gray-400 ${
                    errors.confirmPassword ? 'border-red-500 focus:ring-red-500' : 'border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500'
                  }`}
                />
                <Lock className="w-4 h-4 text-gray-400 absolute right-3.5 top-3.5" />
              </div>
              {errors.confirmPassword && (
                <p className="mt-1 text-xs text-red-600">{errors.confirmPassword.message}</p>
              )}
            </div>

            {/* زر الإرسال */}
            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full mt-3 py-3 px-4 rounded-xl text-white font-semibold bg-blue-600 hover:bg-blue-700 shadow-lg shadow-blue-500/25 transition duration-200 flex items-center justify-center gap-2 disabled:opacity-50"
            >
              <span>{isSubmitting ? 'جاري الإرسال...' : 'إنشاء الحساب'}</span>
              <ArrowRight className="w-4 h-4 rotate-180" />
            </button>
          </form>

          {/* روابط سفلية */}
          <div className="flex items-center justify-between text-sm pt-4 border-t border-gray-100">
            <p className="text-gray-600">
              لديك حساب بالفعل؟{' '}
              <a href="#login" className="font-semibold text-blue-600 hover:underline">
                سجّل الدخول
              </a>
            </p>
            <a href="#tour" className="text-gray-500 hover:text-gray-800 font-medium transition">
              جولة تعريفية
            </a>
          </div>
        </div>

        <div></div>
      </div>

      {/* النصف الأيسر: القسم الجمالي الأزرق */}
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