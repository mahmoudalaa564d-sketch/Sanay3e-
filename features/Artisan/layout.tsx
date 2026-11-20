import { Outlet } from "react-router";
import ArtisanSidebar from "./SideBar/ArtisanSideBar";

const ArtisanLayout = () => {
  return (
    <div dir="rtl" className="flex min-h-screen bg-[#F8FAFC]">

      {/* Sidebar */}
      <ArtisanSidebar />

      {/* Main Content */}
      <div className="min-w-0 flex-1">

        {/* Page Content */}
        <main className="p-6 lg:p-8">
          <Outlet />
        </main>

      </div>
    </div>
  );
};

export default ArtisanLayout;