import { ArrowRight, Wallet } from "lucide-react";
import { useNavigate } from "react-router";
import TopUpForm from "../components/TopUpForm";

const TopUpPage = () => {
  const navigate = useNavigate();

  return (
    <div dir="rtl" className="mx-auto max-w-5xl space-y-6">

      <button
        onClick={() => navigate("/Customer/wallet")}
        className="flex items-center gap-2 text-sm text-gray-500 hover:text-[#0966C1]"
      >
        <ArrowRight size={18} />
        العودة للمحفظة
      </button>

      <div className="flex items-center gap-4">
        <div className="rounded-2xl bg-blue-50 p-3 text-[#0966C1]">
          <Wallet size={28} />
        </div>

        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            شحن المحفظة
          </h1>

          <p className="mt-1 text-sm text-gray-500">
            أضف رصيدًا إلى محفظتك لاستخدامه في دفع الخدمات.
          </p>
        </div>
      </div>

      <TopUpForm />

    </div>
  );
};

export default TopUpPage;