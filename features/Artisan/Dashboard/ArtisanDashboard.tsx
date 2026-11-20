import {
  BriefcaseBusiness,
  CheckCircle2,
  Clock,
  Wallet,
  Star,
  TrendingUp,
  CalendarDays,
  MapPin,
  ArrowLeft,
  CircleCheck,
  AlertCircle,
} from "lucide-react";

import { useNavigate } from "react-router";

const stats = [
  {
    title: "الطلبات المتاحة",
    value: "12",
    description: "طلبات في انتظار عروضك",
    icon: BriefcaseBusiness,
    color: "bg-blue-50 text-blue-600",
  },
  {
    title: "الأعمال النشطة",
    value: "4",
    description: "طلبات قيد التنفيذ",
    icon: Clock,
    color: "bg-orange-50 text-orange-600",
  },
  {
    title: "الأعمال المكتملة",
    value: "28",
    description: "إجمالي الأعمال المنجزة",
    icon: CheckCircle2,
    color: "bg-green-50 text-green-600",
  },
  {
    title: "إجمالي الأرباح",
    value: "8,450",
    description: "جنيه مصري",
    icon: Wallet,
    color: "bg-purple-50 text-purple-600",
  },
];

const activeJobs = [
  {
    id: "SNY-2026-1021",
    service: "إصلاح أعطال الكهرباء",
    customer: "أحمد محمد",
    location: "طنطا، الغربية",
    date: "اليوم - 10:30 صباحًا",
    price: "350 جنيه",
    status: "جاري التنفيذ",
    statusColor: "bg-blue-50 text-blue-600",
  },
  {
    id: "SNY-2026-1022",
    service: "صيانة تكييف",
    customer: "محمد إبراهيم",
    location: "سبرباي، طنطا",
    date: "اليوم - 02:00 مساءً",
    price: "500 جنيه",
    status: "في انتظار البدء",
    statusColor: "bg-orange-50 text-orange-600",
  },
  {
    id: "SNY-2026-1023",
    service: "إصلاح سخان مياه",
    customer: "محمود علي",
    location: "شارع البحر، طنطا",
    date: "غدًا - 11:00 صباحًا",
    price: "250 جنيه",
    status: "تم قبول العرض",
    statusColor: "bg-green-50 text-green-600",
  },
];

const recentReviews = [
  {
    name: "أحمد محمد",
    service: "إصلاح كهرباء",
    rating: 5,
    comment: "فني ممتاز ومحترف، وتم حل المشكلة بسرعة.",
    date: "منذ يومين",
  },
  {
    name: "محمد إبراهيم",
    service: "صيانة تكييف",
    rating: 4,
    comment: "خدمة جيدة والتعامل محترم.",
    date: "منذ 4 أيام",
  },
];

const ArtisanDashboard = () => {
  const navigate = useNavigate();

  return (
    <div dir="rtl" className="space-y-8">

      {/* Welcome */}
      <section className="flex flex-col justify-between gap-4 rounded-2xl bg-gradient-to-l from-[#0966C1] to-blue-500 p-7 text-white md:flex-row md:items-center">
        <div>
          <p className="text-sm text-blue-100">
            الخميس، 19 نوفمبر 2026
          </p>

          <h1 className="mt-3 text-2xl font-bold">
            أهلاً بك، أحمد محمد 👋
          </h1>

          <p className="mt-2 text-sm text-blue-100">
            تابع أعمالك وطلباتك وأرباحك من مكان واحد.
          </p>

          <div className="mt-5 flex items-center gap-2 text-sm">
            <CircleCheck size={18} />
            <span>حسابك موثق ونشط</span>
          </div>
        </div>

        <button
          onClick={() => navigate("/artisan/jobs")}
          className="flex items-center justify-center gap-2 rounded-xl bg-white px-5 py-3 text-sm font-bold text-[#0966C1] transition hover:bg-blue-50"
        >
          تصفح الطلبات المتاحة
          <ArrowLeft size={18} />
        </button>
      </section>

      {/* Statistics */}
      <section>
        <div className="mb-5">
          <h2 className="text-xl font-bold text-gray-900">
            نظرة عامة
          </h2>
          <p className="mt-1 text-sm text-gray-500">
            ملخص نشاطك خلال الفترة الحالية
          </p>
        </div>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
          {stats.map((stat) => {
            const Icon = stat.icon;

            return (
              <div
                key={stat.title}
                className="rounded-2xl border border-gray-100 bg-white p-5 shadow-sm transition hover:-translate-y-1 hover:shadow-md"
              >
                <div className="flex items-center justify-between">
                  <div className={`rounded-xl p-3 ${stat.color}`}>
                    <Icon size={23} />
                  </div>

                  <TrendingUp size={18} className="text-green-500" />
                </div>

                <p className="mt-5 text-sm text-gray-500">
                  {stat.title}
                </p>

                <h3 className="mt-2 text-2xl font-extrabold text-gray-900">
                  {stat.value}
                  {stat.title === "إجمالي الأرباح" && (
                    <span className="mr-2 text-sm font-medium text-gray-500">
                      ج.م
                    </span>
                  )}
                </h3>

                <p className="mt-2 text-xs text-gray-400">
                  {stat.description}
                </p>
              </div>
            );
          })}
        </div>
      </section>

      {/* Jobs and Rating */}
      <section className="grid grid-cols-1 gap-6 xl:grid-cols-3">

        {/* Active Jobs */}
        <div className="rounded-2xl border border-gray-100 bg-white p-5 shadow-sm xl:col-span-2">

          <div className="mb-6 flex items-center justify-between">
            <div>
              <h2 className="font-bold text-gray-900">
                الأعمال النشطة
              </h2>

              <p className="mt-1 text-xs text-gray-500">
                تابع الطلبات التي تعمل عليها حاليًا
              </p>
            </div>

            <button
              onClick={() => navigate("/artisan/active")}
              className="flex items-center gap-1 text-sm font-semibold text-[#0966C1]"
            >
              عرض الكل
              <ArrowLeft size={16} />
            </button>
          </div>

          <div className="space-y-4">
            {activeJobs.map((job) => (
              <div
                key={job.id}
                className="rounded-xl border border-gray-100 p-4 transition hover:border-blue-200"
              >
                <div className="flex flex-col justify-between gap-3 sm:flex-row sm:items-start">

                  <div>
                    <h3 className="font-bold text-gray-900">
                      {job.service}
                    </h3>

                    <p className="mt-2 text-xs text-gray-500">
                      رقم الطلب: {job.id}
                    </p>

                    <div className="mt-3 flex flex-wrap gap-4 text-xs text-gray-500">
                      <span>{job.customer}</span>

                      <span className="flex items-center gap-1">
                        <MapPin size={14} />
                        {job.location}
                      </span>
                    </div>
                  </div>

                  <span className={`w-fit rounded-full px-3 py-1.5 text-xs font-semibold ${job.statusColor}`}>
                    {job.status}
                  </span>
                </div>

                <div className="mt-4 flex flex-wrap items-center justify-between gap-3 border-t border-gray-100 pt-4">

                  <div className="flex items-center gap-4 text-xs text-gray-500">
                    <span className="flex items-center gap-1">
                      <CalendarDays size={14} />
                      {job.date}
                    </span>

                    <span className="font-bold text-gray-900">
                      {job.price}
                    </span>
                  </div>

                  <button
                    onClick={() => navigate("/artisan/active")}
                    className="text-sm font-semibold text-[#0966C1] hover:underline"
                  >
                    التفاصيل
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Rating */}
        <div className="space-y-6">

          <div className="rounded-2xl border border-gray-100 bg-white p-5 shadow-sm">

            <h2 className="font-bold text-gray-900">
              تقييمك العام
            </h2>

            <div className="mt-6 text-center">
              <h3 className="text-5xl font-extrabold text-gray-900">
                4.8
              </h3>

              <div className="mt-3 flex justify-center gap-1 text-yellow-400">
                {[1, 2, 3, 4, 5].map((star) => (
                  <Star key={star} size={21} fill="currentColor" />
                ))}
              </div>

              <p className="mt-3 text-sm text-gray-500">
                بناءً على 24 تقييم
              </p>
            </div>

            <div className="mt-6 space-y-3">
              {[5, 4, 3, 2, 1].map((rating) => (
                <div key={rating} className="flex items-center gap-3 text-xs">
                  <span className="w-3 text-gray-500">
                    {rating}
                  </span>

                  <Star size={13} className="text-yellow-400" />

                  <div className="h-2 flex-1 overflow-hidden rounded-full bg-gray-100">
                    <div
                      className="h-full rounded-full bg-yellow-400"
                      style={{
                        width: `${rating === 5 ? 75 : rating === 4 ? 20 : 5}%`,
                      }}
                    />
                  </div>
                </div>
              ))}
            </div>

            <button
              onClick={() => navigate("/artisan/reviews")}
              className="mt-6 w-full rounded-xl border border-gray-200 py-3 text-sm font-semibold text-gray-700 transition hover:bg-gray-50"
            >
              عرض جميع التقييمات
            </button>
          </div>

          {/* Verification */}
          <div className="rounded-2xl border border-green-100 bg-green-50 p-5">
            <div className="flex items-center gap-3">
              <div className="rounded-xl bg-green-100 p-2 text-green-600">
                <CircleCheck size={22} />
              </div>

              <div>
                <h3 className="font-bold text-green-800">
                  حسابك موثق
                </h3>

                <p className="mt-1 text-xs text-green-700">
                  يمكنك استقبال طلبات العملاء.
                </p>
              </div>
            </div>
          </div>

        </div>
      </section>

      {/* Recent Reviews */}
      <section className="rounded-2xl border border-gray-100 bg-white p-5 shadow-sm">

        <div className="mb-6 flex items-center justify-between">
          <div>
            <h2 className="font-bold text-gray-900">
              أحدث التقييمات
            </h2>

            <p className="mt-1 text-xs text-gray-500">
              آراء العملاء في خدماتك
            </p>
          </div>

          <button
            onClick={() => navigate("/artisan/reviews")}
            className="text-sm font-semibold text-[#0966C1]"
          >
            عرض الكل
          </button>
        </div>

        <div className="grid gap-4 md:grid-cols-2">
          {recentReviews.map((review) => (
            <div
              key={review.name}
              className="rounded-xl border border-gray-100 p-4"
            >
              <div className="flex items-center justify-between">
                <div>
                  <h3 className="font-bold text-gray-800">
                    {review.name}
                  </h3>

                  <p className="mt-1 text-xs text-gray-400">
                    {review.service}
                  </p>
                </div>

                <span className="text-xs text-gray-400">
                  {review.date}
                </span>
              </div>

              <div className="mt-3 flex gap-1 text-yellow-400">
                {Array.from({ length: review.rating }).map((_, i) => (
                  <Star key={i} size={15} fill="currentColor" />
                ))}
              </div>

              <p className="mt-3 text-sm leading-7 text-gray-600">
                {review.comment}
              </p>
            </div>
          ))}
        </div>
      </section>

      {/* Notice */}
      <div className="flex items-start gap-3 rounded-xl border border-blue-100 bg-blue-50 p-4">
        <AlertCircle className="mt-0.5 shrink-0 text-[#0966C1]" size={20} />

        <p className="text-sm leading-7 text-blue-800">
          حافظ على جودة خدماتك وسرعة استجابتك للعملاء لتحصل على تقييمات
          أفضل وتزيد فرص حصولك على طلبات جديدة.
        </p>
      </div>

    </div>
  );
};

export default ArtisanDashboard;