import React from 'react'
import HeroSection from '../component/HeroSection'
import OurServices from "../component/ourServices"
import HowItWorks from "../component/howItWork"
import CommentsSection from "../component/CommentsSection"
import WhySanayie from '../component/WhySanayey'
import Footer from "../component/AboutUs"

export default function HomePage() {
  return (
    <div>
      <HeroSection/>
      <OurServices/>
      <HowItWorks/>
      <CommentsSection/>
      <WhySanayie/>
      <Footer/>  
    </div>
  )
}
