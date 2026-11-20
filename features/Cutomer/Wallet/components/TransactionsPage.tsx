import { useGetTransactions } from "../hooks/WalletHooks";
import TransactionList from "../components/TransactionList";

const TransactionsPage = () => {
  const { data, isLoading, error } = useGetTransactions();

  if (isLoading) return <p className="p-10 text-center">جاري التحميل...</p>;

  if (error) return <p className="p-10 text-center text-red-500">حدث خطأ</p>;

  return (
    <div dir="rtl" className="space-y-6">
      <h1 className="text-2xl font-bold">سجل المعاملات</h1>

      <TransactionList transactions={Array.isArray(data) ? data : []} />
    </div>
  );
};

export default TransactionsPage;