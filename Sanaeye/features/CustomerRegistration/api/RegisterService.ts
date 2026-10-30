import axios from "axios";
import type { RegisterPayload } from "../type/RegisterPayload";

export class RegisterService {
  async Register(data: RegisterPayload) {
    try {
      // 1. إضافة await للانتظار حتى انتهاء الطلب
      const response = await axios.post(
        "https://localhost:7136/api/Auth/register",
        data,
        {
          headers: {
            "Content-Type": "application/json",
          },
        }
      );
      // 2. إرجاع البيانات القادمة من الـ API فقط
      return response.data;
    } catch (e: unknown) {
      if (axios.isAxiosError(e)) {
        // 3. طباعة التفاصيل الدقيقة القادمة من الـ Backend لمعرفة سبب خطأ 400
        console.error("❌ تفاصيل خطأ 400 من الخادم (Backend Validation Errors):", e.response?.data);
        
        // رمي الخطأ الأصلي مع رسالة الـ Backend إن وجدت
        const errorMessage = e.response?.data?.message || e.response?.data || e.message;
        throw new Error(typeof errorMessage === 'string' ? errorMessage : JSON.stringify(errorMessage),{cause:e});
      }
      throw e;
    }
  }
}