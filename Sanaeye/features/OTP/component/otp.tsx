import { useState } from "react";
import axios from "axios";
import { UsecheckOTP } from "../hooks/useCheckOTP";

interface VerifyOtpProps {
  userId: number;
  identifier: string;
}

const VerifyOtp = ({ userId, identifier }: VerifyOtpProps) => {
  const [code, setCode] = useState("");

  const { mutateAsync, isPending } = UsecheckOTP();

  const handleVerify = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    if (!code.trim()) {
      console.log("من فضلك أدخل كود التحقق");
      return;
    }
    const input =identifier.trim();
    const isPhone = /^\+?\d+$/.test(input);

    const channel =isPhone?"phone":"email";
    try {
      const data = await mutateAsync({
        userId,
        channel,
        code,
      });

      console.log("OTP verified successfully");
      console.log("User ID:", userId);
      console.log("Channel:", channel);
      console.log("OTP Code:", code);
      console.log("Response:", data);
    } catch (error: unknown) {
      if (axios.isAxiosError(error)) {
        console.error(
          "Verification failed:",
          error.response?.data ?? error.message
        );
      } else {
        console.error("Verification failed:", error);
      }
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-100 px-4">
      <form
        onSubmit={handleVerify}
        className="w-full max-w-md space-y-5 rounded-2xl bg-white p-8 shadow-lg"
      >
        <h1 className="text-center text-2xl font-bold text-gray-800">
          تحقق من حسابك
        </h1>

        <div>
          <label
            htmlFor="otp"
            className="mb-2 block text-sm font-medium text-gray-700"
          >
            كود التحقق OTP
          </label>

          <input
            id="otp"
            type="text"
            inputMode="numeric"
            autoComplete="one-time-code"
            value={code}
            onChange={(e) => setCode(e.target.value)}
            placeholder="أدخل كود التحقق"
            disabled={isPending}
            className="w-full rounded-lg border border-gray-300 px-4 py-3 text-center tracking-widest outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-200"
          />
        </div>

        <button
          type="submit"
          disabled={isPending || !code.trim()}
          className="w-full rounded-lg bg-blue-600 py-3 font-semibold text-white transition hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {isPending ? "جارٍ التحقق..." : "تحقق"}
        </button>
      </form>
    </div>
  );
};

export default VerifyOtp;
