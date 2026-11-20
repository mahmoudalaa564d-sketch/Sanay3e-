import {
  Star,
  MapPin,
  BadgeCheck,
  ArrowLeft,
  BriefcaseBusiness,
} from "lucide-react";

const technicians = [
  {
    id: "t1",
    name: "محمد السيد",
    specialty: "كهربائي",
    rating: "4.9",
    jobs: 412,
    location: "مدينة نصر، القاهرة",
    initials: "م س",
    color: "bg-blue-100 text-blue-700",
  },
  {
    id: "t2",
    name: "أحمد حسن",
    specialty: "سباك",
    rating: "4.8",
    jobs: 356,
    location: "المعادي، القاهرة",
    initials: "أ ح",
    color: "bg-cyan-100 text-cyan-700",
  },
  {
    id: "t3",
    name: "خالد إبراهيم",
    specialty: "فني تكييف",
    rating: "4.9",
    jobs: 289,
    location: "الجيزة",
    initials: "خ إ",
    color: "bg-orange-100 text-orange-700",
  },
  {
    id: "t4",
    name: "محمود عبدالله",
    specialty: "صيانة أجهزة منزلية",
    rating: "4.7",
    jobs: 268,
    location: "الإسكندرية",
    initials: "م ع",
    color: "bg-purple-100 text-purple-700",
  },
];

export default function CommentsSection() {
  return (
    <section dir="rtl" className="bg-gray-50 py-16 sm:py-20">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="mb-10 flex items-end justify-between gap-4">
          <div>
            <span className="mb-3 inline-block rounded-full bg-blue-100 px-4 py-2 text-sm font-bold text-blue-700">
              الأفضل تقييماً
            </span>

            <h2 className="text-2xl font-extrabold text-gray-900 sm:text-3xl lg:text-4xl">
              فنيون مميّزون
            </h2>

            <p className="mt-3 text-sm leading-7 text-gray-500 sm:text-base">
              تعرّف على أفضل الفنيين واختر الأنسب لاحتياجاتك.
            </p>
          </div>

          <a
            href="/technicians"
            className="hidden items-center gap-2 rounded-xl border border-gray-200 bg-white px-5 py-3 text-sm font-bold text-gray-700 transition hover:border-blue-600 hover:text-blue-700 sm:inline-flex"
          >
            كل الفنيين
            <ArrowLeft size={17} />
          </a>
        </div>

        {/* Demo Notice */}
        <div className="mb-7 flex items-center gap-3 rounded-xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
          <span className="h-2 w-2 shrink-0 rounded-full bg-amber-500" />
          بيانات تجريبية لأغراض العرض
        </div>

        {/* Technicians */}
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
          {technicians.map((technician) => (
            <article
              key={technician.id}
              className="group rounded-2xl border border-gray-100 bg-white p-5 shadow-sm transition duration-300 hover:-translate-y-1 hover:border-blue-200 hover:shadow-xl"
            >
              {/* Profile */}
              <div className="flex items-start justify-between gap-3">
                <div
                  className={`flex h-16 w-16 shrink-0 items-center justify-center rounded-2xl text-xl font-extrabold ${technician.color}`}
                >
                  {technician.initials}
                </div>

                <div className="flex items-center gap-1 rounded-lg bg-blue-50 px-2.5 py-1.5 text-xs font-bold text-blue-700">
                  <BadgeCheck size={16} />
                  موثّق
                </div>
              </div>

              {/* Name & Specialty */}
              <div className="mt-5">
                <h3 className="text-lg font-extrabold text-gray-900 transition group-hover:text-blue-700">
                  {technician.name}
                </h3>

                <p className="mt-1 text-sm font-medium text-gray-500">
                  {technician.specialty}
                </p>
              </div>

              {/* Rating & Jobs */}
              <div className="mt-5 flex items-center justify-between gap-2">
                <div className="flex items-center gap-1.5">
                  <Star
                    size={17}
                    className="fill-amber-400 text-amber-400"
                  />
                  <span className="font-extrabold text-gray-900">
                    {technician.rating}
                  </span>
                </div>

                <div className="flex items-center gap-1.5 text-sm text-gray-500">
                  <BriefcaseBusiness size={16} />
                  <span>{technician.jobs} مهمة</span>
                </div>
              </div>

              {/* Location */}
              <div className="mt-4 flex items-center gap-2 text-sm text-gray-500">
                <MapPin size={17} className="shrink-0 text-blue-600" />
                <span>{technician.location}</span>
              </div>

              <div className="my-5 border-t border-gray-100" />

              {/* Profile Link */}
              <a
                href={`/technicians/${technician.id}`}
                className="flex w-full items-center justify-center gap-2 rounded-xl bg-blue-50 px-4 py-3 text-sm font-bold text-blue-700 transition hover:bg-blue-600 hover:text-white"
              >
                عرض الملف الشخصي
                <ArrowLeft size={17} />
              </a>
            </article>
          ))}
        </div>

        {/* Mobile Link */}
        <a
          href="/technicians"
          className="mt-7 flex w-full items-center justify-center gap-2 rounded-xl border border-gray-200 bg-white px-5 py-3 font-bold text-gray-700 transition hover:border-blue-600 hover:text-blue-700 sm:hidden"
        >
          عرض كل الفنيين
          <ArrowLeft size={18} />
        </a>
      </div>
    </section>
  );
}