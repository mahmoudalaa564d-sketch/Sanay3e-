import { Wallet } from "lucide-react";

interface Props {
  balance: number;
}

const WalletBalance = ({ balance }: Props) => {
  return (
    <div className="rounded-2xl bg-[#0966C1] p-7 text-white shadow-lg">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm text-blue-100">رصيد المحفظة</p>
          <h2 className="mt-3 text-3xl font-bold">
            {balance.toLocaleString("ar-EG")} ج.م
          </h2>
        </div>

        <div className="rounded-xl bg-white/20 p-4">
          <Wallet size={32} />
        </div>
      </div>
    </div>
  );
};

export default WalletBalance;