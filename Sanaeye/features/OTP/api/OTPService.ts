import axios from "axios";
import type { CheckOtpRequest, RequestOtpRequest } from "../type/OTP";

const api = axios.create({
  baseURL: "https://localhost:7136",
});

class OtpService {
  async requestOtp(data: RequestOtpRequest) {
    const result = await api.post("/api/Otp/request", data);
    return result;
  }

  async checkOtp(data: CheckOtpRequest) {
    const result = await api.post("/api/Otp/verify", data);
    return result;
  }
}

export default OtpService;
