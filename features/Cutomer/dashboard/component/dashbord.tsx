import { Link } from "react-router";
import {
  ClipboardList,
  CheckCircle2,
  Wallet,
  Zap,
  Wrench,
  Wind,
  Refrigerator,
  ArrowLeft,
  ArrowUpLeft,
  MapPin,
  Star,
  BadgeCheck,
  Clock,
  Eye,
  Plus,
} from "lucide-react";

// ================= MOCK DATA =================

const stats = [
  {
    title: "طلبات نشطة",
    value: "2",
    icon: ClipboardList,
    color: "bg-blue-50 text-blue-600",
    border: "border-blue-100",
  },
  {
    title: "طلبات مكتملة",
    value: "2",
    icon: CheckCircle2,
    color: "bg-green-50 text-green-600",
    border: "border-green-100",
  },
  {
    title: "رصيد المحفظة",
    value: "450 ج.م",
    icon: Wallet,
    color: "bg-amber-50 text-amber-600",
    border: "border-amber-100",
  },
];

const services = [
  {
    title: "كهربائي",
    icon: Zap,
    color: "bg-yellow-50 text-yellow-600",
    category: "electric",
  },
  {
    title: "سباك",
    icon: Wrench,
    color: "bg-blue-50 text-blue-600",
    category: "plumbing",
  },
  {
    title: "فني تكييف",
    icon: Wind,
    color: "bg-cyan-50 text-cyan-600",
    category: "ac",
  },
  {
    title: "صيانة أجهزة منزلية",
    icon: Refrigerator,
    color: "bg-purple-50 text-purple-600",
    category: "appliances",
  },
];

const recentOrders = [
  {
    id: "r1",
    title: "انقطاع الكهرباء",
    category: "كهربائي",
    number: "SNY-2026-1001",
    date: "24/10/2026",
    status: "جاري التنفيذ",
    statusColor: "bg-blue-50 text-blue-700",
  },
  {
    id: "r2",
    title: "تنظيف التكييف",
    category: "فني تكييف",
    number: "SNY-2026-1002",
    date: "23/10/2026",
    status: "عروض متاحة",
    statusColor: "bg-amber-50 text-amber-700",
  },
  {
    id: "r3",
    title: "إصلاح الحنفيات",
    category: "سباك",
    number: "SNY-2026-1003",
    date: "12/10/2026",
    status: "مكتمل",
    statusColor: "bg-green-50 text-green-700",
  },
  {
    id: "r4",
    title: "إصلاح الغسالات",
    category: "صيانة أجهزة منزلية",
    number: "SNY-2026-1004",
    date: "24/09/2026",
    status: "مكتمل",
    statusColor: "bg-green-50 text-green-700",
  },
  {
    id: "r5",
    title: "تسليك الصرف",
    category: "سباك",
    number: "SNY-2026-1005",
    date: "04/10/2026",
    status: "ملغي",
    statusColor: "bg-red-50 text-red-700",
  },
];

const technicians = [
  {
    id: "t1",
    name: "محمد السيد",
    specialty: "كهربائي",
    rating: "4.9",
    jobs: "412",
    location: "مدينة نصر، القاهرة",
  },
  {
    id: "t2",
    name: "أحمد حسن",
    specialty: "سباك",
    rating: "4.8",
    jobs: "356",
    location: "المعادي، القاهرة",
  },
];

// ================= COMPONENT =================

const CustomerDashboard = () => {
  return (
    <div dir="rtl" className="min-h-screen bg-slate-50 p-4 sm:p-6 lg:p-8">

      <div className="mx-auto max-w-7xl space-y-8">

        {/* Welcome Header */}

        <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">

          <div>
            <h1 className="text-2xl font-bold text-slate-800">
              مرحباً بك 👋
            </h1>

            <p className="mt-2 text-sm text-slate-500">
              تابع طلباتك واستكشف الخدمات المتاحة لك.
            </p>
          </div>

          <Link
            to="/customer/request/new"
            className="inline-flex items-center justify-center gap-2 rounded-xl bg-[#0966C1] px-5 py-3 text-sm font-semibold text-white transition hover:bg-blue-700"
          >
            <Plus size={18} />
            طلب خدمة جديدة
          </Link>

        </div>

        {/* Statistics */}

        <section className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">

          {stats.map((stat) => {
            const Icon = stat.icon;

            return (
              <div
                key={stat.title}
                className={`flex items-center justify-between rounded-2xl border ${stat.border} bg-white p-5 shadow-sm`}
              >
                <div>
                  <p className="text-sm font-medium text-slate-500">
                    {stat.title}
                  </p>

                  <h2 className="mt-3 text-2xl font-bold text-slate-800">
                    {stat.value}
                  </h2>
                </div>

                <div
                  className={`flex h-14 w-14 items-center justify-center rounded-xl ${stat.color}`}
                >
                  <Icon size={27} />
                </div>
              </div>
            );
          })}

        </section>

        {/* Services */}

        <section>

          <div className="mb-5 flex items-center justify-between">

            <div>
              <h2 className="text-xl font-bold text-slate-800">
                الخدمات
              </h2>

              <p className="mt-1 text-sm text-slate-500">
                اختر الخدمة التي تحتاج إليها
              </p>
            </div>

          </div>

          <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">

            {services.map((service) => {
              const Icon = service.icon;

              return (
                <Link
                  key={service.category}
                  to={`/customer/request/new?cat=${service.category}`}
                  className="group flex flex-col items-center justify-center rounded-2xl border border-slate-100 bg-white p-6 text-center shadow-sm transition hover:-translate-y-1 hover:border-blue-200 hover:shadow-md"
                >

                  <div
                    className={`mb-4 flex h-16 w-16 items-center justify-center rounded-2xl ${service.color} transition group-hover:scale-110`}
                  >
                    <Icon size={30} />
                  </div>

                  <h3 className="text-sm font-bold text-slate-800">
                    {service.title}
                  </h3>

                  <span className="mt-3 flex items-center gap-1 text-xs font-medium text-[#0966C1]">
                    اطلب الخدمة
                    <ArrowLeft size={14} />
                  </span>

                </Link>
              );
            })}

          </div>

        </section>

        {/* Recent Orders */}

        <section className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm sm:p-6">

          <div className="mb-6 flex items-center justify-between">

            <div>
              <h2 className="text-xl font-bold text-slate-800">
                الطلبات الأخيرة
              </h2>

              <p className="mt-1 text-sm text-slate-500">
                تابع حالة طلبات الخدمات الخاصة بك
              </p>
            </div>

            <Link
              to="/customer/requests"
              className="flex items-center gap-1 text-sm font-semibold text-[#0966C1] transition hover:text-blue-800"
            >
              عرض الكل
              <ArrowLeft size={16} />
            </Link>

          </div>

          <div className="space-y-3">

            {recentOrders.map((order) => (

              <div
                key={order.id}
                className="flex flex-col gap-4 rounded-xl border border-slate-100 p-4 transition hover:border-blue-200 sm:flex-row sm:items-center sm:justify-between"
              >

                <div className="flex items-start gap-3">

                  <div className="mt-1 flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-blue-50 text-[#0966C1]">
                    <ClipboardList size={21} />
                  </div>

                  <div>

                    <h3 className="font-bold text-slate-800">
                      {order.title}
                    </h3>

                    <p className="mt-1 text-xs text-slate-500">
                      {order.category} · {order.number}
                    </p>

                    <p className="mt-2 flex items-center gap-1 text-xs text-slate-400">
                      <Clock size={13} />
                      {order.date}
                    </p>

                  </div>

                </div>

                <div className="flex items-center justify-between gap-3 sm:justify-end">

                  <span
                    className={`rounded-lg px-3 py-2 text-xs font-semibold ${order.statusColor}`}
                  >
                    {order.status}
                  </span>

                  <Link
                    to={`/customer/requests/${order.id}`}
                    className="flex items-center gap-1 rounded-lg border border-slate-200 px-3 py-2 text-xs font-semibold text-slate-600 transition hover:border-[#0966C1] hover:text-[#0966C1]"
                  >
                    <Eye size={15} />
                    فتح
                  </Link>

                </div>

              </div>

            ))}

          </div>

        </section>

        {/* Suggested Technicians */}

        <section>

          <div className="mb-5 flex items-center justify-between">

            <div>
              <h2 className="text-xl font-bold text-slate-800">
                فنيون مقترحون
              </h2>

              <p className="mt-1 text-sm text-slate-500">
                تعرف على أفضل الفنيين
              </p>
            </div>

          </div>

          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">

            {technicians.map((technician) => (

              <div
                key={technician.id}
                className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm transition hover:shadow-md"
              >

                <div className="flex items-start justify-between">

                  <div className="flex items-center gap-3">

                    <div className="flex h-14 w-14 items-center justify-center rounded-full bg-blue-50 text-[#0966C1]">
                      <Wrench size={25} />
                    </div>

                    <div>

                      <h3 className="font-bold text-slate-800">
                        {technician.name}
                      </h3>

                      <p className="mt-1 text-sm text-slate-500">
                        {technician.specialty}
                      </p>

                    </div>

                  </div>

                  <div className="flex items-center gap-1 rounded-lg bg-amber-50 px-2 py-1 text-sm font-bold text-amber-600">
                    <Star size={15} fill="currentColor" />
                    {technician.rating}
                  </div>

                </div>

                <div className="mt-5 flex flex-wrap items-center gap-4 border-b border-slate-100 pb-4 text-xs text-slate-500">

                  <span className="flex items-center gap-1">
                    <CheckCircle2 size={15} className="text-green-600" />
                    {technician.jobs} مهمة
                  </span>

                  <span className="flex items-center gap-1">
                    <MapPin size={15} />
                    {technician.location}
                  </span>

                </div>

                <div className="mt-4 flex items-center justify-between">

                  <span className="flex items-center gap-1 text-xs font-semibold text-green-600">
                    <BadgeCheck size={16} />
                    موثّق
                  </span>

                  <Link
                    to={`/customer/technicians/${technician.id}`}
                    className="flex items-center gap-1 text-sm font-semibold text-[#0966C1] transition hover:text-blue-800"
                  >
                    عرض الملف
                    <ArrowUpLeft size={16} />
                  </Link>

                </div>

              </div>

            ))}

          </div>

        </section>

      </div>

    </div>
  );
};

export default CustomerDashboard;