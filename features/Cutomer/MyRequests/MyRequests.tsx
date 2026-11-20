
import { useMemo, useState } from "react";
import { Link } from "react-router";
import {
  Plus,
  ClipboardList,
  Eye,
  Clock,
  CheckCircle2,
  XCircle,
  Filter,
} from "lucide-react";

type OrderStatus = "active" | "completed" | "cancelled" | "offers";

type Order = {
  id: string;
  number: string;
  title: string;
  service: string;
  date: string;
  price: string;
  status: OrderStatus;
};

type TabType = "all" | "active" | "completed" | "cancelled";

const orders: Order[] = [
  {
    id: "r1",
    number: "SNY-2026-1001",
    title: "انقطاع الكهرباء",
    service: "كهربائي",
    date: "24/10/2026",
    price: "220 ج.م",
    status: "active",
  },
  {
    id: "r2",
    number: "SNY-2026-1002",
    title: "تنظيف التكييف",
    service: "فني تكييف",
    date: "23/10/2026",
    price: "—",
    status: "offers",
  },
  {
    id: "r3",
    number: "SNY-2026-1003",
    title: "إصلاح الحنفيات",
    service: "سباك",
    date: "12/10/2026",
    price: "180 ج.م",
    status: "completed",
  },
  {
    id: "r4",
    number: "SNY-2026-1004",
    title: "إصلاح الغسالات",
    service: "صيانة أجهزة منزلية",
    date: "24/09/2026",
    price: "250 ج.م",
    status: "completed",
  },
  {
    id: "r5",
    number: "SNY-2026-1005",
    title: "تسليك الصرف",
    service: "سباك",
    date: "04/10/2026",
    price: "—",
    status: "cancelled",
  },
];

const tabs: { label: string; value: TabType }[] = [
  { label: "الكل", value: "all" },
  { label: "النشطة", value: "active" },
  { label: "المكتملة", value: "completed" },
  { label: "الملغاة", value: "cancelled" },
];

const statusConfig: Record<
  OrderStatus,
  { label: string; className: string; icon: typeof Clock }
> = {
  active: {
    label: "جاري التنفيذ",
    className: "bg-blue-50 text-blue-700",
    icon: Clock,
  },
  offers: {
    label: "عروض متاحة",
    className: "bg-amber-50 text-amber-700",
    icon: ClipboardList,
  },
  completed: {
    label: "مكتمل",
    className: "bg-green-50 text-green-700",
    icon: CheckCircle2,
  },
  cancelled: {
    label: "ملغي",
    className: "bg-red-50 text-red-700",
    icon: XCircle,
  },
};

const MyRequests = () => {
  const [activeTab, setActiveTab] = useState<TabType>("all");

  const filteredOrders = useMemo(() => {
    if (activeTab === "all") return orders;

    return orders.filter((order) => {
      if (activeTab === "active") {
        return order.status === "active" || order.status === "offers";
      }

      return order.status === activeTab;
    });
  }, [activeTab]);

  const counts = {
    all: orders.length,
    active: orders.filter(
      (order) => order.status === "active" || order.status === "offers"
    ).length,
    completed: orders.filter((order) => order.status === "completed").length,
    cancelled: orders.filter((order) => order.status === "cancelled").length,
  };

  return (
    <div dir="rtl" className="min-h-screen bg-slate-50 p-4 sm:p-6 lg:p-8">
      <div className="mx-auto max-w-7xl space-y-6">

        {/* Header */}
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h1 className="text-2xl font-bold text-slate-800">
              طلباتي
            </h1>
            <p className="mt-2 text-sm text-slate-500">
              تابع جميع طلبات الخدمات وحالة كل طلب.
            </p>
          </div>

          <Link
            to="/customer/request/new"
            className="inline-flex items-center justify-center gap-2 rounded-xl bg-[#0966C1] px-5 py-3 text-sm font-semibold text-white transition hover:bg-blue-700"
          >
            <Plus size={18} />
            طلب جديد
          </Link>
        </div>

        {/* Summary Cards */}
        <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
          {tabs.map((tab) => {
            const isActive = activeTab === tab.value;

            return (
              <button
                key={tab.value}
                onClick={() => setActiveTab(tab.value)}
                className={`rounded-2xl border bg-white p-4 text-right transition sm:p-5 ${
                  isActive
                    ? "border-[#0966C1] ring-1 ring-[#0966C1]"
                    : "border-slate-100 hover:border-blue-200"
                }`}
              >
                <p className="text-sm font-medium text-slate-500">
                  {tab.label}
                </p>

                <p className="mt-2 text-2xl font-bold text-slate-800">
                  {counts[tab.value]}
                </p>
              </button>
            );
          })}
        </div>

        {/* Orders Table */}
        <section className="overflow-hidden rounded-2xl border border-slate-100 bg-white shadow-sm">

          <div className="flex flex-col gap-3 border-b border-slate-100 p-5 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <h2 className="text-lg font-bold text-slate-800">
                قائمة الطلبات
              </h2>
              <p className="mt-1 text-sm text-slate-500">
                إجمالي {filteredOrders.length} طلب
              </p>
            </div>

            <div className="flex items-center gap-2 text-sm text-slate-500">
              <Filter size={17} />
              تصفية الطلبات
            </div>
          </div>

          {/* Desktop Table */}
          <div className="hidden overflow-x-auto md:block">
            <table className="w-full text-right">
              <thead className="bg-slate-50 text-sm text-slate-500">
                <tr>
                  <th className="px-6 py-4 font-semibold">رقم الطلب</th>
                  <th className="px-6 py-4 font-semibold">الخدمة</th>
                  <th className="px-6 py-4 font-semibold">التاريخ</th>
                  <th className="px-6 py-4 font-semibold">السعر</th>
                  <th className="px-6 py-4 font-semibold">الحالة</th>
                  <th className="px-6 py-4 font-semibold">التفاصيل</th>
                </tr>
              </thead>

              <tbody className="divide-y divide-slate-100">
                {filteredOrders.map((order) => {
                  const status = statusConfig[order.status];
                  const StatusIcon = status.icon;

                  return (
                    <tr
                      key={order.id}
                      className="transition hover:bg-slate-50"
                    >
                      <td className="whitespace-nowrap px-6 py-5 text-sm font-bold text-slate-700">
                        {order.number}
                      </td>

                      <td className="px-6 py-5">
                        <p className="font-semibold text-slate-800">
                          {order.title}
                        </p>
                        <p className="mt-1 text-xs text-slate-500">
                          {order.service}
                        </p>
                      </td>

                      <td className="whitespace-nowrap px-6 py-5 text-sm text-slate-500">
                        {order.date}
                      </td>

                      <td className="whitespace-nowrap px-6 py-5 text-sm font-semibold text-slate-700">
                        {order.price}
                      </td>

                      <td className="px-6 py-5">
                        <span
                          className={`inline-flex items-center gap-1.5 whitespace-nowrap rounded-lg px-3 py-2 text-xs font-semibold ${status.className}`}
                        >
                          <StatusIcon size={14} />
                          {status.label}
                        </span>
                      </td>

                      <td className="px-6 py-5">
                        <Link
                          to={`/customer/requests/${order.id}`}
                          className="inline-flex items-center gap-1 text-sm font-semibold text-[#0966C1] hover:text-blue-800"
                        >
                          <Eye size={16} />
                          فتح
                        </Link>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>

          {/* Mobile Cards */}
          <div className="divide-y divide-slate-100 md:hidden">
            {filteredOrders.map((order) => {
              const status = statusConfig[order.status];
              const StatusIcon = status.icon;

              return (
                <div key={order.id} className="space-y-4 p-4">

                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <h3 className="font-bold text-slate-800">
                        {order.title}
                      </h3>
                      <p className="mt-1 text-xs text-slate-500">
                        {order.number}
                      </p>
                    </div>

                    <span
                      className={`inline-flex shrink-0 items-center gap-1 rounded-lg px-2 py-1.5 text-xs font-semibold ${status.className}`}
                    >
                      <StatusIcon size={13} />
                      {status.label}
                    </span>
                  </div>

                  <div className="grid grid-cols-2 gap-3 text-sm">
                    <div>
                      <p className="text-xs text-slate-400">الخدمة</p>
                      <p className="mt-1 font-medium text-slate-700">
                        {order.service}
                      </p>
                    </div>

                    <div>
                      <p className="text-xs text-slate-400">التاريخ</p>
                      <p className="mt-1 font-medium text-slate-700">
                        {order.date}
                      </p>
                    </div>

                    <div>
                      <p className="text-xs text-slate-400">السعر</p>
                      <p className="mt-1 font-medium text-slate-700">
                        {order.price}
                      </p>
                    </div>
                  </div>

                  <Link
                    to={`/customer/requests/${order.id}`}
                    className="flex w-full items-center justify-center gap-2 rounded-xl border border-slate-200 py-2.5 text-sm font-semibold text-[#0966C1] transition hover:bg-blue-50"
                  >
                    <Eye size={16} />
                    عرض تفاصيل الطلب
                  </Link>

                </div>
              );
            })}
          </div>

          {/* Empty State */}
          {filteredOrders.length === 0 && (
            <div className="flex flex-col items-center justify-center px-4 py-16 text-center">
              <div className="mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-slate-100 text-slate-400">
                <ClipboardList size={30} />
              </div>

              <h3 className="font-bold text-slate-800">
                لا توجد طلبات
              </h3>

              <p className="mt-2 text-sm text-slate-500">
                لا توجد طلبات في هذا التصنيف حالياً.
              </p>

              <Link
                to="/customer/request/new"
                className="mt-5 rounded-xl bg-[#0966C1] px-5 py-3 text-sm font-semibold text-white hover:bg-blue-700"
              >
                إنشاء طلب جديد
              </Link>
            </div>
          )}

        </section>
      </div>
    </div>
  );
};

export default MyRequests;