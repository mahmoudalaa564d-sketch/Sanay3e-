import { useEffect } from "react";
import { useNavigate } from "react-router";
import { useGetOrders, useCancelOrder } from "../hooks/CustomerOrderHooks";
import { Eye, XCircle, ClipboardList } from "lucide-react";

const OrdersPage = () => {
  const navigate = useNavigate();
  const { data, isLoading, error } = useGetOrders();
  const { mutate: cancelOrder, isPending } = useCancelOrder();

  useEffect(() => {
    if (data) console.log("Orders:", data);
  }, [data]);

  const orders = Array.isArray(data) ? data : [];

  const statuses: Record<string, { label: string; style: string }> = {
    PendingOffers: { label: "في انتظار العروض", style: "bg-yellow-50 text-yellow-700" },
    Accepted: { label: "تم قبول العرض", style: "bg-blue-50 text-blue-700" },
    InProgress: { label: "جاري التنفيذ", style: "bg-indigo-50 text-indigo-700" },
    Completed: { label: "مكتمل", style: "bg-green-50 text-green-700" },
    Cancelled: { label: "ملغي", style: "bg-red-50 text-red-600" },
  };

  if (isLoading) return <p className="p-10 text-center">جاري تحميل الطلبات...</p>;
  if (error) return <p className="p-10 text-center text-red-500">حدث خطأ أثناء تحميل الطلبات</p>;

  return (
    <div dir="rtl" className="space-y-6">

      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">طلباتي</h1>
          <p className="mt-2 text-sm text-gray-500">تابع وأدر طلبات الخدمات الخاصة بك.</p>
        </div>

        <div className="flex items-center gap-3 rounded-xl bg-blue-50 p-4 text-[#0966C1]">
          <ClipboardList size={24} />
          <div>
            <p className="text-xs">إجمالي الطلبات</p>
            <p className="text-xl font-bold">{orders.length}</p>
          </div>
        </div>
      </div>

      {/* Table */}
      <div className="overflow-hidden rounded-2xl border border-gray-100 bg-white shadow-sm">
        {orders.length === 0 ? (
          <div className="p-12 text-center text-gray-500">
            لا توجد طلبات حتى الآن.
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-right text-sm">
              <thead className="bg-gray-50 text-gray-500">
                <tr>
                  <th className="px-5 py-4 font-semibold">رقم الطلب</th>
                  <th className="px-5 py-4 font-semibold">التخصص</th>
                  <th className="px-5 py-4 font-semibold">نوع الخدمة</th>
                  <th className="px-5 py-4 font-semibold">تاريخ الطلب</th>
                  <th className="px-5 py-4 font-semibold">الحالة</th>
                  <th className="px-5 py-4 text-center font-semibold">الإجراءات</th>
                </tr>
              </thead>

              <tbody className="divide-y divide-gray-100">
                {orders.map((order: any) => {
                  const status = statuses[order.status] || {
                    label: order.status,
                    style: "bg-gray-100 text-gray-600",
                  };

                  const isFinal = ["Completed", "Cancelled"].includes(order.status);

                  return (
                    <tr key={order.id} className="transition hover:bg-gray-50">

                      <td className="px-5 py-4 font-semibold text-gray-800">
                        {order.requestNumber}
                      </td>

                      <td className="px-5 py-4 text-gray-700">
                        {order.specialtyName || "-"}
                      </td>

                      <td className="px-5 py-4 text-gray-700">
                        {order.serviceTypeName || "-"}
                      </td>

                      <td className="px-5 py-4 text-gray-500">
                        {new Date(order.createdAtUtc).toLocaleDateString("ar-EG")}
                      </td>

                      <td className="px-5 py-4">
                        <span className={`rounded-full px-3 py-1.5 text-xs font-semibold ${status.style}`}>
                          {status.label}
                        </span>
                      </td>

                      <td className="px-5 py-4">
                        <div className="flex items-center justify-center gap-2">

                          <button
                            onClick={() => navigate(`/customer/requests/${order.id}`)}
                            title="عرض التفاصيل"
                            className="rounded-lg bg-blue-50 p-2 text-[#0966C1] hover:bg-blue-100"
                          >
                            <Eye size={18} />
                          </button>

                          {!isFinal && (
                            <button
                              onClick={() => window.confirm("هل تريد إلغاء الطلب؟") && cancelOrder(order.id)}
                              disabled={isPending}
                              title="إلغاء الطلب"
                              className="rounded-lg bg-red-50 p-2 text-red-600 hover:bg-red-100 disabled:opacity-50"
                            >
                              <XCircle size={18} />
                            </button>
                          )}

                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};

export default OrdersPage;