import {
  ClipboardList,
  Users,
  BadgeCheck,
  Star,
} from "lucide-react";

const steps = [
  {
    id: 1,
    title: "اختر الخدمة",
    description: "حدد المشكلة وأضف صوراً ووصفاً.",
    icon: ClipboardList,
  },
  {
    id: 2,
    title: "استقبل عروض الفنيين",
    description: "يرسل الفنيون القريبون أسعارهم ومواعيدهم.",
    icon: Users,
  },
  {
    id: 3,
    title: "اختر الفني المناسب",
    description: "قارن التقييمات والأسعار ثم اقبل العرض.",
    icon: BadgeCheck,
  },
  {
    id: 4,
    title: "تابع الطلب وقيّم الخدمة",
    description: "تابع وصول الفني ثم قيّم تجربتك.",
    icon: Star,
  },
];

export default function HowItWorks() {
  return (
    <section dir="rtl" className="overflow-hidden bg-white py-16 sm:py-20">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        {/* Section Header */}
        <div className="mx-auto mb-14 max-w-2xl text-center">
          <span className="mb-4 inline-flex rounded-full bg-blue-100 px-4 py-2 text-sm font-bold text-blue-700">
            خطوات بسيطة
          </span>

          <h2 className="text-3xl font-extrabold text-gray-900 sm:text-4xl">
            كيف يعمل صنايعي؟
          </h2>

          <p className="mt-4 text-base leading-7 text-gray-500">
            اطلب خدمتك بسهولة، وقارن عروض الفنيين، واختار الأنسب ليك في
            خطوات بسيطة.
          </p>
        </div>

        {/* Steps */}
        <div className="relative grid grid-cols-1 gap-10 sm:grid-cols-2 lg:grid-cols-4 lg:gap-6">
          {/* Connecting Line */}
          <div className="absolute right-[12.5%] left-[12.5%] top-10 hidden h-0.5 bg-blue-100 lg:block" />

          {steps.map((step) => {
            const Icon = step.icon;

            return (
              <div
                key={step.id}
                className="group relative flex flex-col items-center text-center"
              >
                {/* Icon */}
                <div className="relative z-10 mb-6 flex h-20 w-20 items-center justify-center rounded-2xl border border-blue-100 bg-blue-50 text-blue-700 transition duration-300 group-hover:-translate-y-1 group-hover:bg-blue-600 group-hover:text-white group-hover:shadow-lg group-hover:shadow-blue-600/20">
                  <Icon size={32} strokeWidth={1.8} />

                  {/* Step Number */}
                  <span className="absolute -right-2 -top-2 flex h-8 w-8 items-center justify-center rounded-full border-4 border-white bg-blue-600 text-sm font-extrabold text-white">
                    {step.id}
                  </span>
                </div>

                {/* Text */}
                <h3 className="mb-3 text-lg font-extrabold text-gray-900 transition group-hover:text-blue-700">
                  {step.title}
                </h3>

                <p className="max-w-xs text-sm leading-7 text-gray-500">
                  {step.description}
                </p>
              </div>
            );
          })}
        </div>
      </div>
    </section>
  );
}