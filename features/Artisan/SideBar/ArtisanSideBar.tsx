import { NavLink } from "react-router";
import {
  LayoutDashboard,
  BriefcaseBusiness,
  ClipboardList,
  Wallet,
  Star,
  BadgeCheck,
  Settings,
  Wrench,
  LogOut,
} from "lucide-react";

const menuItems = [
  {
    title: "نظرة عامة",
    path: "/artisan",
    icon: LayoutDashboard,
    end: true,
  },
  {
    title: "الطلبات المتاحة",
    path: "/artisan/jobs",
    icon: BriefcaseBusiness,
  },
  {
    title: "الأعمال النشطة",
    path: "/artisan/active",
    icon: ClipboardList,
  },
  {
    title: "الأرباح",
    path: "/artisan/earnings",
    icon: Wallet,
  },
  {
    title: "التقييمات",
    path: "/artisan/reviews",
    icon: Star,
  },
  {
    title: "حالة التوثيق",
    path: "/artisan/verification",
    icon: BadgeCheck,
  },
  {
    title: "الإعدادات",
    path: "/artisan/settings",
    icon: Settings,
  },
];

const ArtisanSidebar = () => {
  return (
    <aside
      dir="rtl"
      className="flex h-screen w-72 shrink-0 flex-col border-l border-gray-100 bg-white"
    >
      {/* Logo */}
      <div className="flex items-center gap-3 border-b border-gray-100 px-6 py-6">
        <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-[#0966C1] text-white">
          <Wrench size={23} />
        </div>

        <div>
          <h1 className="text-xl font-extrabold text-gray-900">
            صنايعي
          </h1>
          <p className="mt-1 text-xs text-gray-500">
            بوابة الفني
          </p>
        </div>
      </div>

      {/* Navigation */}
      <nav className="flex-1 space-y-2 overflow-y-auto px-4 py-6">
        <p className="mb-4 px-3 text-xs font-semibold text-gray-400">
          القائمة الرئيسية
        </p>

        {menuItems.map((item) => {
          const Icon = item.icon;

          return (
            <NavLink
              key={item.path}
              to={item.path}
              end={item.end}
              className={({ isActive }) =>
                `flex items-center gap-3 rounded-xl px-4 py-3 text-sm font-semibold transition-all ${
                  isActive
                    ? "bg-blue-50 text-[#0966C1]"
                    : "text-gray-600 hover:bg-gray-50 hover:text-[#0966C1]"
                }`
              }
            >
              {({ isActive }) => (
                <>
                  <Icon
                    size={20}
                    className={
                      isActive ? "text-[#0966C1]" : "text-gray-400"
                    }
                  />

                  <span>{item.title}</span>

                  {isActive && (
                    <span className="mr-auto h-2 w-2 rounded-full bg-[#0966C1]" />
                  )}
                </>
              )}
            </NavLink>
          );
        })}
      </nav>

      {/* Footer */}
      <div className="border-t border-gray-100 p-4">
        <button
          onClick={() => {
            localStorage.removeItem("token");
            window.location.href = "/login";
          }}
          className="flex w-full items-center gap-3 rounded-xl px-4 py-3 text-sm font-semibold text-red-500 transition hover:bg-red-50"
        >
          <LogOut size={20} />
          تسجيل الخروج
        </button>
      </div>
    </aside>
  );
};

export default ArtisanSidebar;