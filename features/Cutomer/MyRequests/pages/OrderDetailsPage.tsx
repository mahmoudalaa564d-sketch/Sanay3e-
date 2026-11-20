import { useEffect } from "react";
import { useNavigate, useParams } from "react-router";

import {
  useGetOrderById,
  useCancelOrder,
} from "../hooks/CustomerOrderHooks";

import {
  ArrowRight,
  ClipboardList,
  CalendarDays,
  MapPin,
  Clock,
  Wallet,
  XCircle,
  FileText,
  Building2,
  Navigation,
} from "lucide-react";

const OrderDetailsPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();

  const orderId = Number(id);

  const { data, isLoading, error } = useGetOrderById(orderId);
  const { mutate: cancelOrder, isPending } = useCancelOrder();

  useEffect(() => {
    if (data) console.log("Order Details:", data);
  }, [data]);

  const handleCancel = () => {
    if (window.confirm("هل أنت متأكد من إلغاء الطلب؟")) {
      cancelOrder(orderId);
    }
  };

  if (!id || !Number.isInteger(orderId) || orderId <= 0)
    return <p className="p-6 text-red-500">رقم الطلب غير صحيح</p>;

  if (isLoading)
    return <p className="p-10 text-center text-gray-500">جاري تحميل الطلب...</p>;

  if (error)
    return <p className="p-10 text-center text-red-500">حدث خطأ أثناء تحميل الطلب</p>;

  if (!data)
    return <p className="p-10 text-center text-gray-500">الطلب غير موجود</p>;

  const isCancelled = data.status === "Cancelled";
  const isCompleted = data.status === "Completed";

  const statusLabels: Record<string, string> = {
    PendingOffers: "في انتظار عروض الفنيين",
    Accepted: "تم قبول العرض",
    InProgress: "جاري التنفيذ",
    Completed: "مكتمل",
    Cancelled: "ملغي",
  };

  const address = data.address;

  return (
    <div dir="rtl" className="space-y-6 pb-8">

      {/* Header */}
      <div>
        <button
          onClick={() => navigate("/customer/requests")}
          className="mb-4 flex items-center gap-2 text-sm text-gray-500 hover:text-[#0966C1]"
        >
          <ArrowRight size={18} />
          العودة إلى طلباتي
        </button>

        <h1 className="text-2xl font-bold text-gray-900">
          تفاصيل الطلب
        </h1>

        <p className="mt-2 text-sm text-gray-500">
          يمكنك متابعة تفاصيل طلب الخدمة وحالته.
        </p>
      </div>

      {/* Main Card */}
      <div className="rounded-2xl border border-gray-100 bg-white p-6 shadow-sm">

        {/* Order Header */}
        <div className="flex flex-wrap items-center justify-between gap-4 border-b border-gray-100 pb-6">

          <div className="flex items-center gap-3">
            <div className="rounded-xl bg-blue-50 p-3 text-[#0966C1]">
              <ClipboardList size={25} />
            </div>

            <div>
              <p className="text-xs text-gray-400">رقم الطلب</p>
              <h2 className="mt-1 font-bold text-gray-900">
                {data.requestNumber}
              </h2>
            </div>
          </div>

          <span className={`rounded-full px-4 py-2 text-sm font-semibold ${
            isCancelled
              ? "bg-red-50 text-red-600"
              : isCompleted
              ? "bg-green-50 text-green-600"
              : "bg-blue-50 text-[#0966C1]"
          }`}>
            {statusLabels[data.status] || data.status}
          </span>

        </div>

        {/* Service Information */}
        <section className="py-6">
          <h3 className="mb-5 font-bold text-gray-900">
            معلومات الخدمة
          </h3>

          <div className="grid gap-6 sm:grid-cols-2">

            <div>
              <p className="mb-2 text-sm text-gray-400">التخصص</p>
              <p className="font-semibold text-gray-800">
                {data.specialty?.name || "غير محدد"}
              </p>
            </div>

            <div>
              <p className="mb-2 text-sm text-gray-400">نوع الخدمة</p>
              <p className="font-semibold text-gray-800">
                {data.serviceTypeName || "غير محدد"}
              </p>
            </div>

            <div>
              <p className="mb-2 text-sm text-gray-400">تاريخ إنشاء الطلب</p>
              <p className="flex items-center gap-2 text-sm text-gray-700">
                <CalendarDays size={17} className="text-[#0966C1]" />
                {new Date(data.createdAtUtc).toLocaleString("ar-EG")}
              </p>
            </div>

            <div>
              <p className="mb-2 text-sm text-gray-400">موعد التنفيذ</p>
              <p className="flex items-center gap-2 text-sm text-gray-700">
                <Clock size={17} className="text-[#0966C1]" />
                {data.isAsSoonAsPossible
                  ? "في أقرب وقت ممكن"
                  : data.scheduledAtUtc
                  ? new Date(data.scheduledAtUtc).toLocaleString("ar-EG")
                  : "غير محدد"}
              </p>
            </div>

          </div>
        </section>

        {/* Problem Description */}
        <section className="border-t border-gray-100 py-6">
          <h3 className="mb-4 flex items-center gap-2 font-bold text-gray-900">
            <FileText size={19} className="text-[#0966C1]" />
            وصف المشكلة
          </h3>

          <p className="rounded-xl bg-gray-50 p-4 text-sm leading-8 text-gray-600">
            {data.problemDescription || "لا يوجد وصف"}
          </p>

          <h4 className="mb-3 mt-5 text-sm font-semibold text-gray-800">
            ملاحظات إضافية
          </h4>

          <p className="rounded-xl bg-gray-50 p-4 text-sm leading-8 text-gray-600">
            {data.additionalNotes || "لا توجد ملاحظات إضافية"}
          </p>
        </section>

        {/* Address */}
        {address && (
          <section className="border-t border-gray-100 py-6">

            <h3 className="mb-5 flex items-center gap-2 font-bold text-gray-900">
              <MapPin size={20} className="text-[#0966C1]" />
              عنوان تنفيذ الخدمة
            </h3>

            <div className="grid gap-5 rounded-xl bg-gray-50 p-5 sm:grid-cols-2">

              <div>
                <p className="text-xs text-gray-400">اسم العنوان</p>
                <p className="mt-2 font-semibold text-gray-800">
                  {address.title}
                </p>
              </div>

              <div>
                <p className="text-xs text-gray-400">المحافظة والمدينة</p>
                <p className="mt-2 font-semibold text-gray-800">
                  {address.governorate} - {address.city}
                </p>
              </div>

              <div>
                <p className="text-xs text-gray-400">المنطقة والشارع</p>
                <p className="mt-2 font-semibold text-gray-800">
                  {address.district} - {address.street}
                </p>
              </div>

              <div>
                <p className="text-xs text-gray-400">المبنى والطابق والشقة</p>
                <p className="mt-2 font-semibold text-gray-800">
                  {address.buildingNumber || "-"} / {address.floorNumber || "-"} / {address.apartmentNumber || "-"}
                </p>
              </div>

              <div className="sm:col-span-2">
                <p className="text-xs text-gray-400">علامة مميزة</p>
                <p className="mt-2 text-sm text-gray-700">
                  {address.landmark || "لا توجد"}
                </p>
              </div>

              {address.latitude && address.longitude && (
                <a
                  href={`https://www.google.com/maps?q=${address.latitude},${address.longitude}`}
                  target="_blank"
                  rel="noreferrer"
                  className="flex items-center gap-2 text-sm font-semibold text-[#0966C1] hover:underline sm:col-span-2"
                >
                  <Navigation size={17} />
                  عرض الموقع على الخريطة
                </a>
              )}

            </div>
          </section>
        )}

        {/* Attachments */}
        <section className="border-t border-gray-100 py-6">
          <h3 className="mb-4 flex items-center gap-2 font-bold text-gray-900">
            <Building2 size={19} className="text-[#0966C1]" />
            المرفقات
          </h3>

          {data.attachments?.length > 0 ? (
            <div className="space-y-3">
              {data.attachments.map((attachment: any) => (
                <div key={attachment.id}>
                  {JSON.stringify(attachment)}
                </div>
              ))}
            </div>
          ) : (
            <p className="rounded-xl bg-gray-50 p-4 text-sm text-gray-500">
              لا توجد مرفقات لهذا الطلب.
            </p>
          )}
        </section>

        {/* Actions */}
        <div className="flex flex-wrap gap-3 border-t border-gray-100 pt-6">

          {!isCancelled && !isCompleted && (
            <button
              onClick={handleCancel}
              disabled={isPending}
              className="flex items-center gap-2 rounded-xl bg-red-50 px-6 py-3 text-sm font-semibold text-red-600 transition hover:bg-red-100 disabled:opacity-50"
            >
              <XCircle size={18} />
              {isPending ? "جاري الإلغاء..." : "إلغاء الطلب"}
            </button>
          )}

          <button
            onClick={() => navigate("/customer/requests")}
            className="rounded-xl bg-[#0966C1] px-6 py-3 text-sm font-semibold text-white transition hover:bg-blue-800"
          >
            العودة إلى طلباتي
          </button>

        </div>

      </div>
    </div>
  );
};

export default OrderDetailsPage;