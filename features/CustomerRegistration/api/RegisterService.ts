import axios from "axios";
import type { RegisterPayload } from "../type/RegisterPayload";

export class RegisterService {
  async Register(data: RegisterPayload) {
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
  }
}