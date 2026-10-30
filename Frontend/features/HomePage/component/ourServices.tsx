import {
  ArrowLeft,
  Zap,
  Droplets,
  Snowflake,
  WashingMachine,
  ArrowLeftRight,
} from "lucide-react";

const services = [
  {
    id: 1,
    title: "كهربائي",
    description: "انقطاع الكهرباء · تركيب مراوح · تركيب إضاءة",
    price: "١٥٠",
    image:
      "https://images.unsplash.com/photo-1660330589693-99889d60181e?w=900&h=600&fit=crop&auto=format",
    href: "/services/electric",
    icon: Zap,
  },
  {
    id: 2,
    title: "سباك",
    description: "إصلاح تسرب المياه · إصلاح الحنفيات · تسليك الصرف",
    price: "١٢٠",
    image:
      "https://images.unsplash.com/photo-1542013936693-884638332954?w=900&h=600&fit=crop&auto=format",
    href: "/services/plumbing",
    icon: Droplets,
  },
  {
    id: 3,
    title: "فني تكييف",
    description: "تنظيف التكييف · إصلاح الأعطال · تركيب التكييف",
    price: "٢٠٠",
    image:
      "https://images.unsplash.com/photo-1642749776312-aa42ce20c9f5?w=900&h=600&fit=crop&auto=format",
    href: "/services/ac",
    icon: Snowflake,
  },
  {
    id: 4,
    title: "صيانة أجهزة منزلية",
    description: "إصلاح الغسالات · إصلاح الثلاجات · إصلاح الأفران",
    price: "١٨٠",
    image:
      "https://images.unsplash.com/photo-1604335399105-a0c585fd81a1?w=900&h=600&fit=crop&auto=format",
    href: "/services/appliances",
    icon: WashingMachine,
  },
];

export default function OurServices() {
  return (
    <section dir="rtl" className="bg-gray-50 py-16 sm:py-20">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="mb-10 flex items-end justify-between gap-4">
          <div>
            <span className="mb-3 inline-block rounded-full bg-green-100 px-4 py-2 text-sm font-semibold text-green-700">
              خدماتنا
            </span>

            <h2 className="text-2xl font-extrabold text-gray-900 sm:text-3xl lg:text-4xl">
              خدماتنا الرئيسية
            </h2>

            <p className="mt-3 text-sm leading-7 text-gray-500 sm:text-base">
              اختر الفئة المناسبة وابدأ طلبك
            </p>
          </div>

          <a
            href="/services"
            className="hidden shrink-0 items-center gap-2 rounded-xl border border-gray-200 bg-white px-5 py-3 text-sm font-bold text-gray-700 transition hover:border-blue-600 hover:text-blue-700 sm:inline-flex"
          >
            كل الخدمات
            <ArrowLeft size={17} />
          </a>
        </div>

        {/* Services Grid */}
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
          {services.map((service) => {
            const Icon = service.icon;

            return (
              <a
                key={service.id}
                href={service.href}
                className="group overflow-hidden rounded-2xl border border-gray-100 bg-white shadow-sm transition duration-300 hover:-translate-y-1 hover:border-blue-200 hover:shadow-xl hover:shadow-blue-100"
              >
                {/* Image */}
                <div className="relative h-52 overflow-hidden">
                  <img
                    src={service.image}
                    alt={service.title}
                    loading="lazy"
                    className="h-full w-full object-cover transition duration-500 group-hover:scale-110"
                  />

                  <div className="absolute inset-0 bg-gradient-to-t from-black/50 via-transparent to-transparent" />

                  <div className="absolute right-4 top-4 flex h-12 w-12 items-center justify-center rounded-xl bg-white text-blue-600 shadow-md">
                    <Icon size={24} strokeWidth={1.8} />
                  </div>

                  <div className="absolute bottom-4 right-4">
                    <span className="rounded-full bg-white/95 px-3 py-1.5 text-xs font-bold text-gray-700">
                      خدمة منزلية
                    </span>
                  </div>
                </div>

                {/* Content */}
                <div className="p-5">
                  <h3 className="text-lg font-extrabold text-gray-900 transition group-hover:text-blue-600">
                    {service.title}
                  </h3>

                  <p className="mt-3 min-h-12 text-sm leading-6 text-gray-500">
                    {service.description}
                  </p>

                  <div className="my-5 border-t border-gray-100" />

                  <div className="flex items-center justify-between gap-3">
                    <div>
                      <span className="block text-xs text-gray-400">
                        تبدأ من
                      </span>

                      <p className="mt-1 text-lg font-extrabold text-blue-600">
                        {service.price}{" "}
                        <span className="text-sm font-semibold">ج.م</span>
                      </p>
                    </div>

                    <span className="flex h-10 w-10 items-center justify-center rounded-xl bg-green-50 text-blue-700 transition group-hover:bg-blue-600 group-hover:text-white">
                      <ArrowLeft size={19} />
                    </span>
                  </div>
                </div>
              </a>
            );
          })}
        </div>

        {/* Mobile Link */}
        <a
          href="/services"
          className="mt-7 flex w-full items-center justify-center gap-2 rounded-xl border border-gray-200 bg-white px-5 py-3 font-bold text-gray-700 transition hover:border-green-600 hover:text-green-700 sm:hidden"
        >
          عرض كل الخدمات
          <ArrowLeftRight size={18} />
        </a>
      </div>
    </section>
  );
}