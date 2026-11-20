
import { useState } from "react";
import {
  Search,
  MapPin,
  ChevronDown,
  Wrench,
  ShieldCheck,
  Clock,
} from "lucide-react";

const HeroSection = () => {
  const [location, setLocation] = useState("طنطا");
  const [service, setService] = useState("");

  const services = [
    { value: "plumber", label: "سباك" },
    { value: "electrician", label: "كهربائي" },
    { value: "ac-technician", label: "فني تكييف" },
    { value: "painter", label: "نقاش" },
    { value: "carpenter", label: "نجار" },
    { value: "other", label: "خدمات أخرى" },
  ];

  const handleSearch = () => {
    const params = new URLSearchParams();

    if (service) params.set("service", service);
    if (location) params.set("location", location);

    window.location.href = `/services?${params.toString()}`;
  };

  return (
    <section
      dir="rtl"
      className="relative overflow-hidden bg-[#0966C1] text-white"
    >
      {/* Background Decorations */}
      <div className="pointer-events-none absolute -right-32 -top-32 h-96 w-96 rounded-full bg-white/5 blur-3xl" />
      <div className="pointer-events-none absolute -bottom-40 left-1/4 h-96 w-96 rounded-full bg-blue-300/10 blur-3xl" />

      <div className="relative mx-auto grid max-w-7xl grid-cols-1 items-center gap-8 px-5 py-10 sm:px-8 sm:py-16 lg:grid-cols-2 lg:gap-16 lg:px-10 lg:py-24">
        {/* Image: First on Mobile, Left on Desktop */}
        <div className="relative order-1 mx-auto w-full max-w-xl lg:order-2 lg:max-w-none">
          <div className="absolute -inset-4 rounded-[2rem] bg-white/10 blur-2xl" />

          <div className="relative overflow-hidden rounded-[2rem] border border-white/20 bg-white/10 p-2 sm:p-3 shadow-2xl">
            <img
              src="/images/hero-maintenance.png"
              alt="فني متخصص في خدمات الصيانة المنزلية"
              className="h-[220px] w-full rounded-[1.5rem] object-cover sm:h-[350px] lg:h-[460px]"
            />

            {/* Floating Card */}
            <div className="absolute bottom-5 right-5 left-5 flex items-center gap-3 rounded-2xl border border-white/60 bg-white/95 p-3 text-slate-800 shadow-xl backdrop-blur-md sm:bottom-9 sm:right-9 sm:left-9 sm:gap-4 sm:p-4">
              <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-blue-100 text-[#0966C1] sm:h-12 sm:w-12">
                <ShieldCheck size={25} />
              </div>

              <div>
                <p className="text-sm font-extrabold sm:text-base">
                  خدمات منزلية تثق بها
                </p>
                <p className="mt-1 text-xs text-slate-500 sm:text-sm">
                  اختار الخدمة المناسبة ليك بسهولة
                </p>
              </div>
            </div>
          </div>

          {/* Decorative Icon */}
          <div className="absolute -left-2 -top-2 flex h-12 w-12 items-center justify-center rounded-2xl border border-white/20 bg-white/10 text-white shadow-lg backdrop-blur-sm sm:-left-5 sm:-top-5 sm:h-16 sm:w-16">
            <Wrench size={26} />
          </div>
        </div>

        {/* Content: Second on Mobile, Right on Desktop */}
        <div className="order-2 space-y-5 sm:space-y-7 lg:order-1">
          {/* Badge */}
          <div className="inline-flex items-center gap-2 rounded-full border border-white/20 bg-white/10 px-4 py-2 text-sm font-medium backdrop-blur-sm">
            <Wrench size={17} />
            <span>كل خدمات بيتك في مكان واحد</span>
          </div>

          {/* Heading */}
          <h1 className="max-w-2xl text-3xl font-extrabold leading-tight tracking-tight sm:text-5xl lg:text-6xl">
            كل خدمات الصيانة المنزلية
            <span className="mt-2 block text-blue-200">
              في مكان واحد
            </span>
          </h1>

          {/* Description */}
          <p className="max-w-xl text-sm leading-7 text-blue-100 sm:text-lg sm:leading-8">
            اعثر على أفضل فني بالقرب منك، وقارن العروض،
            واحجز خدمتك بكل سهولة.
          </p>

          {/* Search Box */}
          <div className="rounded-2xl bg-white p-3 shadow-2xl shadow-blue-950/20 sm:p-4">
            <div className="flex flex-col gap-3">
              {/* Location */}
              <div className="flex min-w-0 items-center gap-3 rounded-xl border border-slate-200 px-4 py-3">
                <MapPin
                  size={21}
                  className="shrink-0 text-[#0966C1]"
                />

                <div className="min-w-0 flex-1">
                  <label
                    htmlFor="hero-location"
                    className="mb-1 block text-xs font-medium text-slate-500"
                  >
                    المنطقة
                  </label>

                  <div className="relative">
                    <select
                      id="hero-location"
                      value={location}
                      onChange={(e) => setLocation(e.target.value)}
                      className="w-full appearance-none bg-transparent pl-6 text-sm font-bold text-slate-800 outline-none"
                    >
                      <option value="القاهرة">القاهرة</option>
                      <option value="الجيزة">الجيزة</option>
                      <option value="الإسكندرية">الإسكندرية</option>
                      <option value="طنطا">طنطا</option>
                    </select>

                    <ChevronDown
                      size={16}
                      className="pointer-events-none absolute left-0 top-1/2 -translate-y-1/2 text-slate-500"
                    />
                  </div>
                </div>
              </div>

              {/* Service Selection */}
              <div className="flex min-w-0 items-center gap-3 rounded-xl border border-slate-200 px-4 py-3">
                <Wrench
                  size={21}
                  className="shrink-0 text-[#0966C1]"
                />

                <div className="min-w-0 flex-1">
                  <label
                    htmlFor="hero-service"
                    className="mb-1 block text-xs font-medium text-slate-500"
                  >
                    نوع الخدمة
                  </label>

                  <div className="relative">
                    <select
                      id="hero-service"
                      value={service}
                      onChange={(e) => setService(e.target.value)}
                      className="w-full appearance-none bg-transparent pl-6 text-sm font-bold text-slate-800 outline-none"
                    >
                      <option value="">اختر نوع الفني</option>

                      {services.map((item) => (
                        <option key={item.value} value={item.value}>
                          {item.label}
                        </option>
                      ))}
                    </select>

                    <ChevronDown
                      size={16}
                      className="pointer-events-none absolute left-0 top-1/2 -translate-y-1/2 text-slate-500"
                    />
                  </div>
                </div>
              </div>

              {/* Search Button */}
              <button
                type="button"
                onClick={handleSearch}
                className="flex w-full items-center justify-center gap-2 rounded-xl bg-[#0966C1] px-6 py-4 font-bold text-white transition hover:bg-blue-800"
              >
                <Search size={19} />
                ابحث عن فني
              </button>
            </div>
          </div>

          {/* Action Buttons */}
          <div className="flex flex-col gap-3 pt-1 sm:flex-row">
            <a
              href="/request-technician"
              className="inline-flex items-center justify-center gap-2 rounded-xl bg-white px-6 py-4 font-bold text-[#0966C1] shadow-lg transition hover:-translate-y-1 hover:bg-blue-50 sm:px-7"
            >
              <Wrench size={19} />
              اطلب فني الآن
            </a>

            <a
              href="/become-technician"
              className="inline-flex items-center justify-center gap-2 rounded-xl border border-white/40 bg-white/10 px-6 py-4 font-bold text-white transition hover:bg-white/20 sm:px-7"
            >
              <ShieldCheck size={19} />
              انضم كفني
            </a>
          </div>

          {/* Trust Features */}
          <div className="flex flex-wrap gap-x-6 gap-y-3 pt-2 text-sm text-blue-100">
            <div className="flex items-center gap-2">
              <ShieldCheck size={17} />
              فنيون موثوقون
            </div>

            <div className="flex items-center gap-2">
              <Clock size={17} />
              حجز بكل سهولة
            </div>
          </div>
        </div>
      </div>
    </section>
  );
};

export default HeroSection;
