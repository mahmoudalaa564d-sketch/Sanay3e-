import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";

import LoginForm from "../component/LoginForm";
import LoginSidebar from "../component/LoginSidebar";

import {
  loginSchema,
  type LoginFormData,
} from "../component/LoginScema";

import { UseLogin } from "../hooks/useLogin";
import { UseOTPRequest} from "../../OTP/hooks/useOTPRequesr"
import VerifyOtp from "../../OTP/component/otp";
import axios from "axios";
import { useNavigate } from "react-router";

const LoginPage = () => {
  const [showOtp, setShowOtp] = useState(false);
  const [userId, setUserId] = useState(0);
  const [identifier, setIdentifier] = useState("");
  const [otp,setOtp]=useState(0);
const router = useNavigate();
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  const loginMutation = UseLogin();
  const otpMutation = UseOTPRequest();

  // Login
  const onSubmit = async (data: LoginFormData) => {
    setIdentifier(data.identifier);

    try {
     const result= await loginMutation.mutateAsync(data);
localStorage.setItem("token", result.token);
console.log("Login successful:", result.user.role);
if (result.user.role === "Admin") router("/admin");
if (result.user.role === "Customer") router("/Customer");
if (result.user.role === "Artisan") router("/artisan");
      
    } catch (error: unknown) {
      if (axios.isAxiosError(error)) {
        const userId = error.response?.data?.userId;
        console.log(error.response)
        if (userId) {
          setUserId(Number(userId));
        }

        console.log("User ID:", userId);
      }
    }
  };

  // Request OTP to activate the account
  const SendOTPRequest = async (id: number) => {
    if (!identifier) {
      console.error("Please enter your email or phone number first.");
      return;
    }
    const channel = /^\S+@\S+\.\S+$/.test(identifier)
    ? "Email"
    : "Phone";
    console.log("id=",id,"identifier channel=",channel);

    try {
     const result= await otpMutation.mutateAsync({
        userId: id,
        channel,
      });
    console.log("result=",result.data.developmentCode)
    setOtp(result.data.developmentCode)
    } catch (error: unknown) {
      console.error("OTP request failed:", error);
    }
  };

  return (
    <div
      className="flex h-screen w-full flex-row-reverse font-sans overflow-hidden bg-white"
      dir="rtl"
    >
      <div className="h-auto w-full flex flex-col justify-between p-8 lg:p-16 overflow-y-auto">

            <LoginForm
              register={register}
              handleSubmit={handleSubmit}
              onSubmit={onSubmit}
              errors={errors}
              isSubmitting={isSubmitting}
              isPending={loginMutation.isPending}
              error={loginMutation.error}
              id={userId}
              setShowotp={setShowOtp}
              SendOTPRequest={SendOTPRequest}
              />
              <p className="text-green-600 text-4xl">{otp>0&&otp}</p>
            {showOtp&&
            <VerifyOtp
            userId={userId}
            identifier={identifier}
            />
          }
          </div>

        <LoginSidebar />
    </div>
  );
};

export default LoginPage;
