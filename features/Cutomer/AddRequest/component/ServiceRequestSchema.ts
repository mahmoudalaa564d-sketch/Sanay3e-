import { z } from "zod";

export const serviceRequestSchema = z
  .object({
    // Step 1
    specialtyId: z.number().min(1, "يرجى اختيار التخصص"),

    serviceTypeName: z
      .string()
      .min(1, "نوع الخدمة مطلوب")
      .max(150, "الحد الأقصى 150 حرف"),

    problemDescription: z
      .string()
      .min(5, "وصف المشكلة يجب ألا يقل عن 5 أحرف")
      .max(3000, "الحد الأقصى 3000 حرف"),

    additionalNotes: z
      .string()
      .max(1000, "الحد الأقصى 1000 حرف")
      .nullable(),

    // Step 2
    addressTitle: z.string().min(1, "اسم العنوان مطلوب").max(100),

    governorate: z.string().min(1, "المحافظة مطلوبة").max(100),

    city: z.string().min(1, "المدينة مطلوبة").max(100),

    district: z.string().max(100).nullable(),

    street: z.string().min(1, "الشارع مطلوب").max(200),

    buildingNumber: z.string().max(50).nullable(),

    floorNumber: z.string().max(50).nullable(),

    apartmentNumber: z.string().max(50).nullable(),

    landmark: z.string().max(250).nullable(),

    // Step 3
    isAsSoonAsPossible: z.boolean(),

    scheduledAtUtc: z.string().nullable(),

    latitude: z.number().min(-90).max(90).nullable(),

    longitude: z.number().min(-180).max(180).nullable(),
  })
  .superRefine((data, ctx) => {
    if (!data.isAsSoonAsPossible) {
      if (!data.scheduledAtUtc) {
        ctx.addIssue({
          code: "custom",
          path: ["scheduledAtUtc"],
          message: "يرجى تحديد موعد الخدمة",
        });
      } else if (
        new Date(data.scheduledAtUtc).getTime() <= Date.now()
      ) {
        ctx.addIssue({
          code: "custom",
          path: ["scheduledAtUtc"],
          message: "يجب اختيار موعد في المستقبل",
        });
      }
    }

    if (
      (data.latitude === null) !== (data.longitude === null)
    ) {
      ctx.addIssue({
        code: "custom",
        path: ["latitude"],
        message: "يجب إدخال خط العرض وخط الطول معًا",
      });
    }
  });

export type ServiceRequestFormData = z.infer<
  typeof serviceRequestSchema
>;