import {
  Wrench,
  Users,
  MessageCircle,
  Globe,
  ArrowUpLeft,
  MapPin,
  Phone,
  Mail,
} from "lucide-react";

const companyLinks = [
  { label: "من نحن", href: "/info/about" },
  { label: "تواصل معنا", href: "/info/contact" },
  { label: "انضم كفني", href: "/become-technician" },
];

const serviceLinks = [
  { label: "كهربائي", href: "/services/electric" },
  { label: "سباك", href: "/services/plumbing" },
  { label: "فني تكييف", href: "/services/ac" },
  { label: "صيانة أجهزة", href: "/services/appliances" },
];

const legalLinks = [
  { label: "الشروط والأحكام", href: "/info/terms" },
  { label: "سياسة الخصوصية", href: "/info/privacy" },
];

export default function Footer() {
  return (
    <footer dir="rtl" className="bg-[#111827] text-gray-300">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        {/* Main Footer */}
        <div className="grid grid-cols-1 gap-10 py-14 sm:grid-cols-2 lg:grid-cols-4 lg:gap-12 lg:py-16">
          {/* Brand */}
          <div>
            <a
              href="/"
              className="inline-flex items-center gap-3"
              aria-label="صنايعي - الصفحة الرئيسية"
            >
              <span className="flex h-12 w-12 items-center justify-center rounded-2xl bg-blue-600 text-white shadow-lg shadow-blue-600/20">
                <Wrench size={26} />
              </span>

              <span className="text-2xl font-extrabold text-white">
                صنايعي
              </span>
            </a>

            <p className="mt-5 max-w-sm text-sm leading-8 text-gray-400">
              منصة صنايعي تربطك بأفضل الفنيين الموثّقين لخدمات الصيانة
              المنزلية في مصر، بأسعار شفافة وتجربة سهلة.
            </p>

            {/* Social Links */}
            <div className="mt-6 flex items-center gap-3">
              <a
                href="https://www.facebook.com/"
                target="_blank"
                rel="noreferrer"
                aria-label="فيسبوك"
                className="flex h-10 w-10 items-center justify-center rounded-xl border border-gray-700 text-gray-300 transition hover:border-blue-500 hover:bg-blue-600 hover:text-white"
              >
                <Users size={19} />
              </a>

              <a
                href="https://www.instagram.com/"
                target="_blank"
                rel="noreferrer"
                aria-label="إنستجرام"
                className="flex h-10 w-10 items-center justify-center rounded-xl border border-gray-700 text-gray-300 transition hover:border-blue-500 hover:bg-blue-600 hover:text-white"
              >
                <MessageCircle size={19} />
              </a>

              <a
                href="https://www.linkedin.com/"
                target="_blank"
                rel="noreferrer"
                aria-label="لينكد إن"
                className="flex h-10 w-10 items-center justify-center rounded-xl border border-gray-700 text-gray-300 transition hover:border-blue-500 hover:bg-blue-600 hover:text-white"
              >
                <Globe size={19} />
              </a>
            </div>
          </div>

          {/* Company Links */}
          <div>
            <h3 className="text-lg font-extrabold text-white">
              الشركة
            </h3>

            <ul className="mt-6 space-y-4">
              {companyLinks.map((link) => (
                <li key={link.href}>
                  <a
                    href={link.href}
                    className="inline-flex items-center gap-2 text-sm text-gray-400 transition hover:text-blue-400"
                  >
                    <ArrowUpLeft size={15} className="text-blue-400" />
                    {link.label}
                  </a>
                </li>
              ))}
            </ul>
          </div>

          {/* Services Links */}
          <div>
            <h3 className="text-lg font-extrabold text-white">
              الخدمات
            </h3>

            <ul className="mt-6 space-y-4">
              {serviceLinks.map((link) => (
                <li key={link.href}>
                  <a
                    href={link.href}
                    className="inline-flex items-center gap-2 text-sm text-gray-400 transition hover:text-blue-400"
                  >
                    <ArrowUpLeft size={15} className="text-blue-400" />
                    {link.label}
                  </a>
                </li>
              ))}
            </ul>
          </div>

          {/* Legal & Contact */}
          <div>
            <h3 className="text-lg font-extrabold text-white">
              قانوني
            </h3>

            <ul className="mt-6 space-y-4">
              {legalLinks.map((link) => (
                <li key={link.href}>
                  <a
                    href={link.href}
                    className="inline-flex items-center gap-2 text-sm text-gray-400 transition hover:text-blue-400"
                  >
                    <ArrowUpLeft size={15} className="text-blue-400" />
                    {link.label}
                  </a>
                </li>
              ))}
            </ul>

            {/* Contact Info */}
            <div className="mt-7 space-y-4 border-t border-gray-700 pt-5">
              <div className="flex items-center gap-3 text-sm text-gray-400">
                <MapPin size={17} className="shrink-0 text-blue-400" />
                <span>نخدم العملاء في مصر</span>
              </div>

              <div className="flex items-center gap-3 text-sm text-gray-400">
                <Phone size={17} className="shrink-0 text-blue-400" />
                <span>خدمة العملاء</span>
              </div>

              <div className="flex items-center gap-3 text-sm text-gray-400">
                <Mail size={17} className="shrink-0 text-blue-400" />
                <span>تواصل معنا عبر الموقع</span>
              </div>
            </div>
          </div>
        </div>

        {/* Bottom Bar */}
        <div className="flex flex-col items-center justify-between gap-4 border-t border-gray-700 py-6 text-center sm:flex-row sm:text-right">
          <p className="text-sm text-gray-400">
            © {new Date().getFullYear()}{" "}
            <span className="font-bold text-white">صنايعي</span>.
            جميع الحقوق محفوظة.
          </p>

          <a
            href="/"
            className="text-sm font-semibold text-gray-400 transition hover:text-blue-400"
          >
            صنعنا لتسهيل حياتك ❤️
          </a>
        </div>
      </div>
    </footer>
  );
}