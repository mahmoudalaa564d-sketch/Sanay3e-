import { Link } from "react-router"
import { UserRound, Wrench, ArrowLeft } from "lucide-react";
const RegisterSwitch = () => {
  return (



    


    <section
      dir="rtl"
      className="min-h-screen flex items-center justify-center bg-slate-50 px-4"
    >
      <div className="w-full max-w-2xl">

        {/* Header */}
        <div className="text-center mb-10">
          <h1 className="text-3xl font-bold text-slate-900 mb-3">
            مرحباً بك في صنايعي
          </h1>

          <p className="text-slate-500 text-lg">
            اختر نوع الحساب الذي تريد إنشاءه
          </p>
        </div>

        {/* Options */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">

          {/* User */}
          <Link
            to="/register"
            className="group bg-white border border-slate-200 rounded-2xl p-8 text-center
            hover:border-[#0966C1] hover:shadow-lg transition-all duration-300"
          >
            <div className="w-16 h-16 mx-auto mb-5 rounded-2xl bg-blue-50
            flex items-center justify-center group-hover:bg-blue-100 transition">
              <UserRound size={32} className="text-[#0966C1]" />
            </div>

            <h2 className="text-xl font-bold text-slate-900 mb-3">
              التسجيل كمستخدم
            </h2>

            <p className="text-sm text-slate-500 leading-7 mb-6">
              ابحث عن الفني المناسب واطلب الخدمة التي تحتاجها بسهولة.
            </p>

            <span className="inline-flex items-center gap-2 text-[#0966C1] font-semibold">
              إنشاء حساب مستخدم
              <ArrowLeft size={18} />
            </span>
          </Link>

          {/* Artisan */}
          <Link
            to="/register-artisan"
            className="group bg-white border border-slate-200 rounded-2xl p-8 text-center
            hover:border-[#0966C1] hover:shadow-lg transition-all duration-300"
          >
            <div className="w-16 h-16 mx-auto mb-5 rounded-2xl bg-blue-50
            flex items-center justify-center group-hover:bg-blue-100 transition">
              <Wrench size={32} className="text-[#0966C1]" />
            </div>

            <h2 className="text-xl font-bold text-slate-900 mb-3">
              التسجيل كصنايعي
            </h2>

            <p className="text-sm text-slate-500 leading-7 mb-6">
              انضم إلى منصتنا واعرض خدماتك واستقبل طلبات العملاء.
            </p>

            <span className="inline-flex items-center gap-2 text-[#0966C1] font-semibold">
              إنشاء حساب صنايعي
              <ArrowLeft size={18} />
            </span>
          </Link>

        </div>

        {/* Login */}
        <p className="text-center text-slate-500 mt-8">
          لديك حساب بالفعل؟{" "}
          <Link
            to="/login"
            className="text-[#0966C1] font-semibold hover:underline"
          >
            تسجيل الدخول
          </Link>
        </p>

      </div>
    </section>


  );
};

export default RegisterSwitch;
