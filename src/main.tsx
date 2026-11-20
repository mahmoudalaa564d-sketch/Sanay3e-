import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import './index.css'
import App from './App.tsx'

// 1. إنشاء نسخة من QueryClient
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false, // إيقاف إعادة جلب البيانات عند التنقل بين النوافذ (اختياري)
      retry: 1,                   // عدد المحاولات في حالة فشل الطلب
    },
  },
})

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    {/* 2. تغليف مكون App بـ QueryClientProvider */}
    <QueryClientProvider client={queryClient}>
      <App />
    </QueryClientProvider>
  </StrictMode>,
)