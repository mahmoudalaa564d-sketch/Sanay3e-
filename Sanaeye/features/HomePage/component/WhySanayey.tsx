import {
  ShieldCheck,
  Wallet,
  Clock,
  Star,
  MapPin,
} from "lucide-react";

const features = [
  {
    id: 1,
    title: "فنيون موثّقون",
    description: "نراجع هوية كل فني وخبرته قبل الانضمام.",
    icon: ShieldCheck,
  },
  {
    id: 2,
    title: "عروض شفافة",
    description: "أسعار واضحة مقدماً بدون مفاجآت.",
    icon: Wallet,
  },
  {
    id: 3,
    title: "حجز سهل",
    description: "اطلب فني في أقل من دقيقتين.",
    icon: Clock,
  },
  {
    id: 4,
    title: "تقييمات العملاء",
    description: "تقييمات حقيقية بعد كل خدمة مكتملة.",
    icon: Star,
  },
  {
    id: 5,
    title: "تتبع الطلب",
    description: "تابع كل مرحلة من الطلب لحظة بلحظة.",
    icon: MapPin,
  },
];

export default function WhySanayie() {
  return (
    <section dir="rtl" className="bg-white py-16 sm:py-20">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="mx-auto mb-12 max-w-2xl text-center">
          <span className="mb-4 inline-block rounded-full bg-blue-100 px-4 py-2 text-sm font-bold text-blue-700">
            مميزاتنا
          </span>

          <h2 className="text-3xl font-extrabold text-gray-900 sm:text-4xl">
            لماذا صنايعي؟
          </h2>

          <p className="mt-4 text-base leading-7 text-gray-500">
            بنسهّل عليك الوصول للفني المناسب، بأسعار واضحة وتجربة مريحة وآمنة.
          </p>
        </div>

        {/* Features */}
        <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
          {features.map((feature) => {
            const Icon = feature.icon;

            return (
              <div
                key={feature.id}
                className="group rounded-2xl border border-gray-100 bg-gray-50 p-6 transition duration-300 hover:-translate-y-1 hover:border-blue-200 hover:bg-white hover:shadow-xl"
              >
                <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-blue-100 text-blue-700 transition duration-300 group-hover:bg-blue-600 group-hover:text-white">
                  <Icon size={27} strokeWidth={1.8} />
                </div>

                <h3 className="mt-5 text-lg font-extrabold text-gray-900 transition group-hover:text-blue-700">
                  {feature.title}
                </h3>

                <p className="mt-3 text-sm leading-7 text-gray-500">
                  {feature.description}
                </p>
              </div>
            );
          })}
        </div>

      </div>
    </section>
  );
}