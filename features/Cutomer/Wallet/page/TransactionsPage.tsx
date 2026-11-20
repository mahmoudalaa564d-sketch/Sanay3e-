import { useGetTransactions } from "../hooks/WalletHooks";
import TransactionList from "../components/TransactionList";

const TransactionsPage = () => {
  const { data, isLoading, error } = useGetTransactions();

  if (isLoading)
    return <p className="p-10 text-center">جاري تحميل المعاملات...</p>;

  if (error)
    return <p className="p-10 text-center text-red-500">حدث خطأ أثناء تحميل المعاملات</p>;

  return (
    <div dir="rtl" className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">سجل المعاملات</h1>
        <p className="mt-2 text-sm text-gray-500">
          جميع عمليات الشحن والدفع والاسترجاع.
        </p>
      </div>

      <TransactionList transactions={Array.isArray(data) ? data : []} />
    </div>
  );
};

export default TransactionsPage;