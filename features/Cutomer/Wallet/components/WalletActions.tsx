import { useNavigate } from "react-router";
import { Plus, History } from "lucide-react";

const WalletActions = () => {
  const navigate = useNavigate();

  return (
    <div className="flex flex-wrap gap-3">
      <button
        onClick={() => navigate("/customer/wallet/top-up")}
        className="flex items-center gap-2 rounded-xl bg-[#0966C1] px-5 py-3 text-white hover:bg-blue-800"
      >
        <Plus size={18} />
        شحن المحفظة
      </button>

      <button
        onClick={() => navigate("/customer/wallet/transactions")}
        className="flex items-center gap-2 rounded-xl border border-gray-200 px-5 py-3 text-gray-700 hover:bg-gray-50"
      >
        <History size={18} />
        سجل المعاملات
      </button>
    </div>
  );
};

export default WalletActions;