import {useMutation}from"@tanstack/react-query";
import OtpService from "../../OTP/api/OTPService";
import type { RequestOtpRequest } from "../type/OTP";
export const UseOTPRequest=()=>{
const otp=new OtpService();
    return useMutation({
        mutationFn:(data:RequestOtpRequest)=> otp.requestOtp(data),
        onSuccess:data => data,
        onError:e=>e
    })
}