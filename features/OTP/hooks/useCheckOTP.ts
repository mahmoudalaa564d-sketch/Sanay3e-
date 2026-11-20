import {useMutation}from"@tanstack/react-query";
import OtpService from "../../OTP/api/OTPService";
import type { CheckOtpRequest } from "../type/OTP";
export const UsecheckOTP=()=>{
const otp=new OtpService();
    return useMutation({
        mutationFn:(data:CheckOtpRequest)=> otp.checkOtp(data),
        onSuccess:data => data,
        onError:e=>e
    })
}