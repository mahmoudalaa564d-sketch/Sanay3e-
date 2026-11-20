import { z } from "zod";

const imageSchema = z
  .custom<FileList>(
    (value) =>
      typeof FileList !== "undefined" && value instanceof FileList,
    "يجب اختيار صورة",
  )
  .refine((files) => files.length > 0, "الصورة مطلوبة")
  .refine(
    (files) => files[0]?.type.startsWith("image/"),
    "يجب اختيار ملف صورة صحيح",
  );

export const artisanRegisterSchema = z.object({
  fullName: z.string().min(3, "الاسم يجب أن يكون 3 أحرف على الأقل").max(100),

  identifier: z.string().min(1, "الإيميل أو رقم الهاتف مطلوب"),

  password: z.string().min(8, "كلمة المرور يجب أن تكون 8 أحرف على الأقل").max(100),

  profession: z.string().min(2, "المهنة مطلوبة").max(100),

  experienceYears: z.number().int().min(0).max(80),

  bio: z.string().min(20, "النبذة يجب أن تكون 20 حرفًا على الأقل").max(1500),

  governorate: z.string().min(1, "المحافظة مطلوبة").max(100),

  center: z.string().min(1, "المركز مطلوب").max(100),

  village: z.string().min(1, "القرية مطلوبة"),

  nationalIdFrontImage: imageSchema,

  nationalIdBackImage: imageSchema,

  profileImage: imageSchema,

  specialties: z.string().min(1, "التخصص مطلوب"),

  serviceTypes: z.string().min(1, "نوع الخدمة مطلوب"),

  termsAccepted: z.boolean().refine((value) => value === true, {
    message: "يجب الموافقة على الشروط والأحكام",
  }),
});

export type ArtisanRegisterFormData = z.infer<
  typeof artisanRegisterSchema
>;