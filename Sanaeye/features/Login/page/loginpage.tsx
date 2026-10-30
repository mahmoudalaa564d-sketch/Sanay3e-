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

const LoginPage = () => {
  const [showOtp, setShowOtp] = useState(false);
  const [userId, setUserId] = useState(0);
  const [identifier, setIdentifier] = useState("");

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
      await loginMutation.mutateAsync(data);
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

    try {
      await otpMutation.mutateAsync({
        userId: id,
        channel,
      });

      setShowOtp(true);
    } catch (error: unknown) {
      console.error("OTP request failed:", error);
    }
  };

  return (
    <div
      className="flex h-screen w-full flex-row-reverse font-sans overflow-hidden bg-white"
      dir="rtl"
    >
          {!showOtp ? (
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
          ) : (
            <VerifyOtp
              userId={userId}
              identifier={identifier}
            />
          )}

        <LoginSidebar />
    </div>
  );
};

export default LoginPage;
