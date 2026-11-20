import { z } from "zod";

export const addressSchema = z.object({
  title: z.string().min(2, "اسم العنوان مطلوب"),

  governorate: z.string().min(1, "المحافظة مطلوبة"),

  city: z.string().min(1, "المدينة مطلوبة"),

  district: z.string(),

  street: z.string().min(1, "الشارع مطلوب"),

  buildingNumber: z.string(),
  floorNumber: z.string(),
  apartmentNumber: z.string(),

  landmark: z.string(),

  latitude: z.number().nullable(),

  longitude: z.number().nullable(),

  isDefault: z.boolean(),
});

export type AddressFormValues = z.infer<typeof addressSchema>;