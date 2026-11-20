import "./App.css";
// import Navbar from '../features/Navbar/component/Navbar'
// import HeroSection from '../features/HomePage/component/HeroSection'
// import OurServices from '../features/HomePage/component/ourServices'
// import HowItWorks from '../features/HomePage/component/howItWork'
// import CommentsSection from "../features/HomePage/component/CommentsSection"
// import WhySanayey from "../features/HomePage/component/WhySanayey"
// import Footer from "../features/HomePage/component/AboutUs"
// import RegisterPage from '../features/CustomerRegistration/component/RegisterPage'
import { BrowserRouter, Route, Routes } from "react-router";
import HomePage from "../features/HomePage/page/HomePage";
import LoginPage from "../features/Login/page/loginpage";
import RegisterPage from "../features/CustomerRegistration/page/RegisterPage";
import ArtisanRegisterPage from "../features/ArtisanRegistration/page/ArtisanRegisterPage";
import Navbar from "../features/Navbar/component/Navbar";
import AdminLayout from "../features/admain/Layout";
import Verification from "../features/admain/Artisan/component/Verification";
import RegisterSwitch from "../features/CustomerRegistration/switch/Switch";
import CustomerLayout from "../features/Cutomer/Layout";
import CustomerDashboard from "../features/Cutomer/dashboard/component/dashbord";
import MyRequests from "../features/Cutomer/MyRequests/MyRequests";
import SpecialtiesPage from "../features/admain/specialties/page/SpecialtiesPage";
import CreateServiceRequestPage from "../features/Cutomer/AddRequest/page/CreateServiceRequestPage";
import AddressesPage from "../features/Cutomer/AddAddress/page/AddressesPage";
import WalletPage from "../features/Cutomer/Wallet/page/WalletPage";
import TransactionsPage from "../features/Cutomer/Wallet/page/TransactionsPage";
import TopUpPage from "../features/Cutomer/Wallet/page/TopUpPage";
import ArtisanLayout from "../features/Artisan/layout";
import ArtisanDashboard from "../features/Artisan/Dashboard/ArtisanDashboard";
function App() {
  return (
    <>
      <Navbar />
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/switch" element={<RegisterSwitch />} />
          <Route path="/Register" element={<RegisterPage />} />
          <Route path="/register-artisan" element={<ArtisanRegisterPage />} />
          
          
          
          <Route path="/admin" element={<AdminLayout />}>
            <Route index element={<Verification />} />
            <Route path="specialties" element={<SpecialtiesPage />} />
            {/*<Route path="artisans" element={<Artisans />} />
          <Route path="services" element={<Services />} /> */}
          </Route>
          
          
          
          <Route path="/Customer" element={<CustomerLayout />}>
            <Route index element={<CustomerDashboard />} />
            <Route path="wallet" element={<WalletPage />} />
            <Route path="payments" element={<TransactionsPage />} />
            <Route path="wallet/top-up" element={<TopUpPage />} />
            <Route path="addresses" element={<AddressesPage />} />
            <Route path="request/new" element={<CreateServiceRequestPage />} />
            <Route path="requests" element={<MyRequests />} />
            {/*<Route path="services" element={<Services />} /> */}
          </Route>
          
          <Route path="/artisan" element={<ArtisanLayout />}>
            <Route index element={<ArtisanDashboard />} />
            <Route path="wallet" element={<WalletPage />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </>
  );
}

export default App;
