import { Link, NavLink } from "react-router";
import {
  LayoutDashboard,
  Users,
  ShieldCheck,
  ClipboardList,
  BarChart3,
  Star,
  Hammer,
  ArrowRightFromLine,
  Settings,
} from "lucide-react";

const navItems = [
  {
    path: "/admin",
    label: "لوحة التحكم",
    icon: LayoutDashboard,
    end: true,
  },
  {
    path: "/admin/users",
    label: "المستخدمون",
    icon: Users,
  },
  {
    path: "/admin/verification",
    label: "توثيق الفنيين",
    icon: ShieldCheck,
    badge: "5",
  },
  {
    path: "/admin/requests",
    label: "الطلبات",
    icon: ClipboardList,
  },
  {
    path: "/admin/reports",
    label: "التقارير",
    icon: BarChart3,
  },
  {
    path: "/admin/ratings",
    label: "إدارة التقييمات",
    icon: Star,
  },
    {
    path: "/admin/specialties",
    label: "إدارة التخصصات",
    icon: Star,
  },
  
    {
    path: "/admin/CreateService",
    label: "إنشاء خدمة",
    icon: Star,
  },
];

const AdminSidebar = () => {
  return (
    <aside
      dir="rtl"
      className="sticky top-0 flex h-screen w-72 shrink-0 flex-col border-l border-gray-100 bg-white"
    >
      {/* Logo */}
      <div className="border-b border-gray-100 px-6 py-6">
        <Link to="/admin" className="flex items-center gap-3">
          <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-[#0966C1] text-white shadow-lg shadow-blue-100">
            <Hammer size={23} />
          </div>

          <div>
            <h1 className="text-xl font-extrabold text-gray-900">
              صنايعي
            </h1>
            <p className="mt-0.5 text-xs text-gray-400">
              لوحة تحكم الإدارة
            </p>
          </div>
        </Link>
      </div>

      {/* Admin Info */}
      <div className="mx-4 mt-6 flex items-center gap-3 rounded-xl bg-blue-50/70 p-3">
        <div className="flex h-11 w-11 items-center justify-center rounded-full bg-[#0966C1] text-lg font-bold text-white">
          A
        </div>

        <div className="min-w-0">
          <p className="truncate text-sm font-bold text-gray-800">
            مدير النظام
          </p>
          <p className="mt-1 text-xs text-gray-500">
            Admin
          </p>
        </div>

        <div className="mr-auto h-2.5 w-2.5 rounded-full bg-green-500" />
      </div>

      {/* Navigation */}
      <div className="flex-1 overflow-y-auto px-4 py-7">
        <p className="mb-4 px-3 text-xs font-bold tracking-wider text-gray-400">
          القائمة الرئيسية
        </p>

        <nav className="flex flex-col gap-2">
          {navItems.map((item) => {
            const Icon = item.icon;

            return (
              <NavLink
                key={item.path}
                to={item.path}
                end={item.end}
                className={({ isActive }) =>
                  `group flex items-center gap-3 rounded-xl px-4 py-3.5 text-sm font-medium transition-all duration-200 ${
                    isActive
                      ? "bg-[#0966C1] text-white shadow-md shadow-blue-100"
                      : "text-gray-600 hover:bg-blue-50 hover:text-[#0966C1]"
                  }`
                }
              >
                {({ isActive }) => (
                  <>
                    <Icon
                      size={20}
                      className={
                        isActive
                          ? "text-white"
                          : "text-gray-400 group-hover:text-[#0966C1]"
                      }
                    />

                    <span className="flex-1">{item.label}</span>

                    {item.badge && (
                      <span
                        className={`rounded-full px-2 py-0.5 text-xs font-bold ${
                          isActive
                            ? "bg-white/20 text-white"
                            : "bg-blue-100 text-[#0966C1]"
                        }`}
                      >
                        {item.badge}
                      </span>
                    )}
                  </>
                )}
              </NavLink>
            );
          })}
        </nav>

        {/* Settings */}
        <div className="mt-9">
          <p className="mb-4 px-3 text-xs font-bold tracking-wider text-gray-400">
            أخرى
          </p>

          <Link
            to="/admin/settings"
            className="flex items-center gap-3 rounded-xl px-4 py-3.5 text-sm font-medium text-gray-600 transition hover:bg-blue-50 hover:text-[#0966C1]"
          >
            <Settings size={20} className="text-gray-400" />
            الإعدادات
          </Link>
        </div>
      </div>

      {/* Footer */}
      <div className="border-t border-gray-100 p-4">
        <Link
          to="/"
          className="flex items-center gap-3 rounded-xl px-4 py-3 text-sm font-medium text-gray-500 transition hover:bg-gray-100 hover:text-gray-900"
        >
          <ArrowRightFromLine size={20} />
          العودة للموقع
        </Link>

        <p className="mt-3 text-center text-[11px] text-gray-400">
          صنايعي © 2026 جميع الحقوق محفوظة
        </p>
      </div>
    </aside>
  );
};

export default AdminSidebar;