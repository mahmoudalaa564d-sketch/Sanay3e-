import { z } from "zod";

export const loginSchema = z.object({
  identifier: z
    .string()
    .min(1, {
      message: "برجاء إدخال البريد الإلكتروني أو رقم الهاتف",
    }),

  password: z
    .string()
    .min(6, {
      message: "كلمة المرور يجب أن تكون 6 أحرف على الأقل",
    }),
});

export type LoginFormData = z.infer<typeof loginSchema>;