import { Outlet } from "react-router";
import AdminSidebar from "./Sidebar/AdminSidebar";

const AdminLayout = () => {
  return (
    <div className="flex min-h-screen flex-row-reverse">
      <AdminSidebar/>

      <main className="flex-1">
        <Outlet />
      </main>
    </div>
  );
};

export default AdminLayout;