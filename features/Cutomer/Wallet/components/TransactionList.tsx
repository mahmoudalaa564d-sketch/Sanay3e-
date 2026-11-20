import { Eye } from "lucide-react";
import { useNavigate } from "react-router";

interface Props {
  transactions: any[];
}

const TransactionList = ({ transactions }: Props) => {
  const navigate = useNavigate();

  return (
    <div className="overflow-hidden rounded-2xl border bg-white">
      <div className="border-b p-5">
        <h3 className="font-bold text-gray-800">آخر المعاملات</h3>
      </div>

      <div className="overflow-x-auto">
        <table className="w-full text-right text-sm">
          <thead className="bg-gray-50 text-gray-500">
            <tr>
              <th className="p-4">النوع</th>
              <th className="p-4">المبلغ</th>
              <th className="p-4">التاريخ</th>
              <th className="p-4">الحالة</th>
              <th className="p-4">التفاصيل</th>
            </tr>
          </thead>

          <tbody className="divide-y">
            {transactions.map((item) => (
              <tr key={item.id} className="hover:bg-gray-50">
                <td className="p-4">{item.type}</td>

                <td className="p-4 font-semibold">
                  {item.amount} ج.م
                </td>

                <td className="p-4 text-gray-500">
                  {new Date(item.createdAtUtc).toLocaleDateString("ar-EG")}
                </td>

                <td className="p-4">{item.status}</td>

                <td className="p-4">
                  <button
                    onClick={() =>
                      navigate(`/customer/wallet/transactions/${item.id}`)
                    }
                    className="rounded-lg bg-blue-50 p-2 text-[#0966C1]"
                  >
                    <Eye size={18} />
                  </button>
                </td>
              </tr>
            ))}

            {transactions.length === 0 && (
              <tr>
                <td colSpan={5} className="p-8 text-center text-gray-400">
                  لا توجد معاملات حتى الآن
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default TransactionList;