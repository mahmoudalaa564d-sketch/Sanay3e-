import z from "zod";

export const registerSchema = z.object({
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

export type RegisterFormData = z.infer<typeof registerSchema>;