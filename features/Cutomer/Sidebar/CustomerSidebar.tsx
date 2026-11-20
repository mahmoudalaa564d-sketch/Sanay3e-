import {
  LayoutDashboard,
  PlusCircle,
  ClipboardList,
  Bookmark,
  Star,
  Wallet,
  CreditCard,
  MapPin,
  Bell,
  Hammer,
  X,
} from "lucide-react";
import { NavLink } from "react-router";

interface CustomerSidebarProps {
  isOpen?: boolean;
  onClose?: () => void;
}

const menuItems = [
  {
    title: "نظرة عامة",
    path: "/customer",
    icon: LayoutDashboard,
    end: true,
  },
  {
    title: "طلب جديد",
    path: "/customer/request/new",
    icon: PlusCircle,
  },
  {
    title: "طلباتي",
    path: "/customer/requests",
    icon: ClipboardList,
  },
  {
    title: "الفنيون المحفوظون",
    path: "/customer/saved",
    icon: Bookmark,
  },
  {
    title: "تقييماتي",
    path: "/customer/reviews",
    icon: Star,
  },
  {
    title: "المحفظة",
    path: "/customer/wallet",
    icon: Wallet,
  },
  {
    title: "سجل المدفوعات",
    path: "/customer/payments",
    icon: CreditCard,
  },
  {
    title: "العناوين",
    path: "/customer/addresses",
    icon: MapPin,
  },
  {
    title: "الإشعارات",
    path: "/customer/notifications",
    icon: Bell,
  },
];

const CustomerSidebar = ({
  isOpen = false,
  onClose,
}: CustomerSidebarProps) => {
  return (
    <>
      {/* Mobile Overlay */}
      {isOpen && (
        <div
          onClick={onClose}
          className="fixed inset-0 z-40 bg-black/40 lg:hidden"
        />
      )}

      <aside
        dir="rtl"
        className={`
          fixed right-0 top-0 z-50 flex h-screen w-72 flex-col
          border-l border-gray-200 bg-white transition-transform duration-300
          lg:sticky lg:top-0 lg:z-20 lg:translate-x-0
          ${isOpen ? "translate-x-0" : "translate-x-full lg:translate-x-0"}
        `}
      >
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
                onClick={onClose}
                className={({ isActive }) => `
                  flex items-center gap-3 rounded-xl px-4 py-3
                  text-sm font-medium transition-all duration-200
                  ${
                    isActive
                      ? "bg-blue-50 text-[#0966C1]"
                      : "text-gray-600 hover:bg-gray-50 hover:text-[#0966C1]"
                  }
                `}
              >
                {({ isActive }) => (
                  <>
                    <Icon
                      size={20}
                      strokeWidth={isActive ? 2.5 : 1.8}
                    />

                    <span>{item.title}</span>
                  </>
                )}
              </NavLink>
            );
          })}
        </nav>

        {/* Footer */}
        <div className="border-t border-gray-100 p-5">
          <div className="rounded-xl bg-blue-50 p-4">
            <h3 className="mb-1 text-sm font-bold text-[#0966C1]">
              محتاج مساعدة؟
            </h3>

            <p className="text-xs leading-6 text-gray-600">
              فريق الدعم موجود لمساعدتك في أي وقت.
            </p>
          </div>
        </div>
      </aside>
    </>
  );
};

export default CustomerSidebar;
