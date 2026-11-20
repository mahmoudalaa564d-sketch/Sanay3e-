import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useState } from "react";

import {
  artisanRegisterSchema,
  type ArtisanRegisterFormData,
} from "../component/ArtisanRegisterSchema";

import { UseArtisanRegister } from "../hooks/UseArtisanRegister";
import type { ArtisanRegisterPayload } from "../type/ArtisanRegisterPelyod";

import ArtisanRegisterForm from "../component/ArtisanRegisterForm";
import { UseOTPRequest } from "../../OTP/hooks/useOTPRequesr";
import VerifyOtp from "../../OTP/component/otp";
// import VerifyOtp from "../../OTP/component/otp";

export default function ArtisanRegisterPage() {
  const [showOTP, setShowOTP] = useState(false);
  const [identifier, setIdentifier] = useState("");

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ArtisanRegisterFormData>({
    resolver: zodResolver(artisanRegisterSchema),
    defaultValues: {
      fullName: "",
      identifier: "",
      password: "",
      profession: "",
      experienceYears: 0,
      bio: "",
      governorate: "",
      center: "",
      village: "",
      specialties: "",
      serviceTypes: "",
      termsAccepted: false,
    },
  });

  const { mutateAsync, isPending, isSuccess,data } = UseArtisanRegister();
  const {mutateAsync:OTPRequest,data:otpData} = UseOTPRequest();
    const SendOTPRequest = async () => {
    if (!identifier) {
      console.error("Please enter your email or phone number first.");
      return;
    }
    const channel = /^\S+@\S+\.\S+$/.test(identifier)
    ? "Email"
    : "Phone";
    console.log("id=",data?.userId || 0,"identifier channel=",channel);

    try {
     const result= await OTPRequest({
        userId: data?.userId || 0,
        channel,
      });
    console.log("result=",result.data.developmentCode)
    } catch (error: unknown) {
      console.error("OTP request failed:", error);
    }
  };
  // دالة إرسال التسجيل
  const onSubmit = async (formData: ArtisanRegisterFormData) => {
    const {
      identifier,
      nationalIdFrontImage,
      nationalIdBackImage,
      profileImage,
      ...rest
    } = formData;
    setIdentifier(identifier);
    const input = identifier.trim();
    const isPhone = /^(\+)?\d+$/.test(input);

    const payload: ArtisanRegisterPayload = {
      ...rest,
      ...(isPhone ? { phoneNumber: input } : { email: input }),
      nationalIdFrontImage: nationalIdFrontImage[0],
      nationalIdBackImage: nationalIdBackImage[0],
      profileImage: profileImage[0],
    };

    try {
      const response = await mutateAsync(payload);
      console.log(response);
    } catch (error) {
      console.error(error);
    }
  };

  if (showOTP) {
    return(
        <>
        <button onClick={() => setShowOTP(false)}>Back to Registration</button>
        {otpData&&<p>{otpData.data.developmentCode}</p>}
        <VerifyOtp  identifier={identifier} userId={data?.userId || 0}/>
        </>
        
    ) ;
  }

  return (
    <ArtisanRegisterForm
      register={register}
      errors={errors}
      onSubmit={handleSubmit(onSubmit)}
      isPending={isPending}
      isSuccess={isSuccess}
      onActivateOtp={() =>{
       void SendOTPRequest()
        setShowOTP(true)}
        }
    />
  );
}