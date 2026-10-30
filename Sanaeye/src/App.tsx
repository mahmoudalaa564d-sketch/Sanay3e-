
import './App.css'
// import Navbar from '../features/Navbar/component/Navbar'
// import HeroSection from '../features/HomePage/component/HeroSection'
// import OurServices from '../features/HomePage/component/ourServices'
// import HowItWorks from '../features/HomePage/component/howItWork'
// import CommentsSection from "../features/HomePage/component/CommentsSection"
// import WhySanayey from "../features/HomePage/component/WhySanayey"
// import Footer from "../features/HomePage/component/AboutUs"
import RegisterPage from '../features/CustomerRegistration/component/RegisterPage'
import { BrowserRouter, Route, Routes } from 'react-router'
import HomePage from"../features/HomePage/page/HomePage";
import LoginPage from"../features/Login/page/loginpage";
import Navbar from "../features/Navbar/component/Navbar"
function App() {

  return (
    <>
      <Navbar/>
      <BrowserRouter>
      <Routes>
        <Route path='/' element={<HomePage/>}/>
        <Route path='/login' element={<LoginPage/>}/>
        <Route path='/Register' element={<RegisterPage/>}/>
      </Routes>
      </BrowserRouter>
    </>
  )
}

export default App
