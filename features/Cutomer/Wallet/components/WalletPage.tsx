import { useGetWallet, useGetTransactions } from "../hooks/WalletHooks";

import WalletBalance from "../components/WalletBalance";
import WalletActions from "../components/WalletActions";
import TransactionList from "../components/TransactionList";

const WalletPage = () => {
  const wallet = useGetWallet();
  const transactions = useGetTransactions();

  if (wallet.isLoading || transactions.isLoading) {
    return <p className="p-10 text-center">جاري تحميل المحفظة...</p>;
  }

  if (wallet.error || transactions.error) {
    return <p className="p-10 text-center text-red-500">حدث خطأ أثناء تحميل البيانات</p>;
  }

  const items = Array.isArray(transactions.data)
    ? transactions.data
    : [];

  return (
    <div dir="rtl" className="space-y-7">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">محفظتي</h1>
        <p className="mt-2 text-sm text-gray-500">
          تابع رصيدك ومعاملاتك المالية.
        </p>
      </div>

      <WalletBalance balance={wallet.data?.balance ?? 0} />

      <WalletActions />

      <TransactionList transactions={items.slice(0, 5)} />
    </div>
  );
};

export default WalletPage;