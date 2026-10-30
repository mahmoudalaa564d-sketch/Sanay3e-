import {useMutation}from"@tanstack/react-query";
import OtpService from "../../OTP/api/OTPService";
import type { CheckOtpRequest } from "../type/OTP";
export const UsecheckOTP=()=>{
const otp=new OtpService();
    return useMutation({
        mutationFn:(data:CheckOtpRequest)=> otp.requestOtp(data),
        onSuccess:data => data,
        onError:e=>e
    })
}