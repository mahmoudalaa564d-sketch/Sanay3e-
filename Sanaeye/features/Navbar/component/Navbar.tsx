
import { KeyRound, Menu, X } from "lucide-react";
import { useState } from "react";

const Navbar = () => {
  const [isOpen, setIsOpen] = useState(false);

  const navLinks = [
    { label: "الخدمات", href: "/" },
    { label: "كيف يعمل", href: "/" },
    { label: "انضم كفني", href: "/" },
    { label: "الرئيسية", href: "/" },
  ];

  return (
    <header
      dir="rtl"
      className="sticky top-0 z-50 border-b border-blue-100/60 bg-white/80 backdrop-blur-xl"
    >
      <nav className="mx-auto flex min-h-20 max-w-7xl items-center justify-between px-4 py-3 sm:px-6 lg:px-8">
        {/* Logo */}
        <a
          href="/"
          className="flex shrink-0 items-center gap-2"
          aria-label="صنايعي - الصفحة الرئيسية"
        >
          <span className="flex h-11 w-11 items-center justify-center rounded-xl bg-[#0966C1] text-white shadow-md shadow-blue-200">
            <KeyRound size={25} strokeWidth={2.2} />
          </span>

          <span className="text-2xl font-extrabold tracking-tight text-slate-900">
            صنايعي
            <span className="text-[#0966C1]">.</span>
          </span>
        </a>

        {/* Desktop Links */}
        <div className="hidden items-center gap-8 lg:flex">
          {navLinks.map((link) => (
            <a
              key={link.href}
              href={link.href}
              className="text-lg font-semibold text-slate-600 transition-colors hover:text-[#0966C1]"
            >
              {link.label}
            </a>
          ))}
        </div>

        {/* Desktop Actions */}
        <div className="hidden items-center gap-3 lg:flex">
          <a
            href="/login"
            className="rounded-xl text-lg px-4 py-2.5  font-bold text-slate-700 transition-colors hover:bg-blue-50 hover:text-[#0966C1]"
          >
            تسجيل الدخول
          </a>

          <a
            href="/register"
            className="rounded-xl border border-[#0966C1] px-4 py-2.5 text-lg font-bold text-[#0966C1] transition-colors hover:bg-[#0966C1] hover:text-white"
          >
            إنشاء حساب
          </a>

          <a
            href="/request-technician"
            className="rounded-xl bg-[#0966C1] px-5 py-3 text-lg font-bold text-white shadow-md shadow-blue-200/60 transition-all hover:-translate-y-0.5 hover:bg-blue-800"
          >
            اطلب فني
          </a>
        </div>

        {/* Mobile Menu Button */}
        <button
          type="button"
          onClick={() => setIsOpen(!isOpen)}
          className="flex h-11 w-11 items-center justify-center rounded-xl border border-slate-200 text-slate-700 transition-colors hover:bg-blue-50 lg:hidden"
          aria-label={isOpen ? "إغلاق القائمة" : "فتح القائمة"}
          aria-expanded={isOpen}
        >
          {isOpen ? <X size={24} /> : <Menu size={24} />}
        </button>
      </nav>

      {/* Mobile Menu */}
      {isOpen && (
        <div className="border-t border-blue-100 bg-white/95 px-4 pb-5 pt-3 backdrop-blur-xl lg:hidden">
          <div className="mx-auto flex max-w-7xl flex-col gap-2">
            {navLinks.map((link) => (
              <a
                key={link.href}
                href={link.href}
                onClick={() => setIsOpen(false)}
                className="rounded-xl px-4 py-3 font-semibold text-slate-700 transition-colors hover:bg-blue-50 hover:text-[#0966C1]"
              >
                {link.label}
              </a>
            ))}

            <div className="my-2 border-t border-slate-100" />

            <a
              href="/login"
              onClick={() => setIsOpen(false)}
              className="rounded-xl px-4 py-3 text-center font-bold text-slate-700 hover:bg-blue-50"
            >
              تسجيل الدخول
            </a>

            <a
              href="/register"
              onClick={() => setIsOpen(false)}
              className="rounded-xl border border-[#0966C1] px-4 py-3 text-center font-bold text-[#0966C1] hover:bg-blue-50"
            >
              إنشاء حساب
            </a>

            <a
              href="/request-technician"
              onClick={() => setIsOpen(false)}
              className="rounded-xl bg-[#0966C1] px-4 py-3 text-center font-bold text-white transition-colors hover:bg-blue-800"
            >
              اطلب فني
            </a>
          </div>
        </div>
      )}
    </header>
  );
};

export default Navbar;
