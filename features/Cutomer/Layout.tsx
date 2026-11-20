import { Outlet } from "react-router";
import CustomerSidebar from "./Sidebar/CustomerSidebar";

const CustomerLayout = () => {
  return (
    <div className="flex min-h-screen flex-row-reverse">
      <CustomerSidebar />

      <main className="flex-1 min-w-0">
        <Outlet />
      </main>
    </div>
  );
};

export default CustomerLayout;