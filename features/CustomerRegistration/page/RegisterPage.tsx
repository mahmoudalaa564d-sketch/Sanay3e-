import React, { useState } from 'react'
import { useForm } from 'react-hook-form';
import {  registerSchema, type RegisterFormData } from '../component/RegisterScema';
import { zodResolver } from '@hookform/resolvers/zod';

import axios from 'axios';
import RegisterForm from '../component/RegisterForm';
import { UseRegister } from '../hooks/Register';
import LoginSidebar from '../../Login/component/LoginSidebar';
import type { RegisterPayload } from '../type/RegisterPayload';
import VerifyOtp from '../../OTP/component/otp';
import { UseOTPRequest } from '../../OTP/hooks/useOTPRequesr';

export default function RegisterPage() {
const { mutateAsync,isSuccess,data:data2,error:er } = UseRegister();
  const [showOTP,SetShowOTP]=useState(false);
  const [userId, setUserId]=useState(0);
  const [identifier, setIdentifier]=useState('');
  const {data,error:otpError,mutateAsync:otpMutation}=UseOTPRequest();
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      fullName: '',
      identifier: '',
      password: '',
      confirmPassword: '',
    },
  });
  
// Request OTP to activate the account
  const SendOTPRequest = async () => {
    if (!identifier) {
      console.error("Please enter your email or phone number first.");
      return;
    }
    const channel = /^\S+@\S+\.\S+$/.test(identifier)
    ? "Email"
    : "Phone";
    console.log("id=",userId,"identifier channel=",channel);

    try {
     const result= await otpMutation({
        userId: userId,
        channel,
      });
    console.log("result=",result.data.developmentCode)
   await console.log("data",data?.data.developmentCode)
    } catch (error: unknown) {
      console.error("OTP request failed:", error);
    }
  };

  // 💡 إرسال إما email أو phoneNumber حسب المدخل (وليس الاثنين معاً)
  const FormatData = (data: RegisterFormData): RegisterPayload => {
    const input = data.identifier.trim();
    const isPhone = /^(\+)?\d+$/.test(input);
    setIdentifier(data.identifier.trim()); 
    return {
      fullName: data.fullName,
      password: data.password,
      email: !isPhone ? input : undefined,       // إذا كان إيميل، نرسله ونترك الهاتف undefined
      phoneNumber: isPhone ? input : undefined, // إذا كان هاتف، نرسله ونترك الإيميل undefined
    };
  };

  const onSubmit = async (data: RegisterFormData) => {
try{

  const apiPayload = FormatData(data);
  const response = await mutateAsync(apiPayload);
  console.log(isSuccess);
  console.log(response)
  setUserId(response.userId); // حفظ userId من
  await  console.log("data=======",data2);
}catch(error:unknown){
  if (axios.isAxiosError(error)) {
    console.error('Registration failed:', error.response?.data ?? error.message);
}
      //   console.log('تم التسجيل بنجاح:', response);

  };
  }
  return (
    <div
      className="flex h-screen w-full flex-row-reverse font-sans overflow-hidden bg-white"
      dir="rtl"
    >
      <div className="h-auto w-full flex flex-col justify-between p-8 lg:p-16 overflow-y-auto">

    <RegisterForm
    data2={data2}
    isSubmitting={isSubmitting}
    errors={errors}
    handleSubmit={handleSubmit}
    isSuccess={isSuccess}
    onSubmit={onSubmit}
    register={register}
    SetShowOTP={SetShowOTP}
                  SendOTPRequest={SendOTPRequest}
    error={er}
    />   
    {data && <div>كود التفعيل: {data?.data?.developmentCode}</div>}
                {showOTP&&
            <VerifyOtp
            userId={userId}
            identifier={identifier}
            />
          }
          {otpError && (<div className="text-red-500">خطأ في التحقق: {otpError?.message}</div>)}
    </div>
    <LoginSidebar/>
    </div>
  )
}
