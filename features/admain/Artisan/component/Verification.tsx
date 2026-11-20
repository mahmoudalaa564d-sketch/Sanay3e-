import { useState } from "react";

import { useGetPendingArtisans } from "../hooks/useGetPendingArtisans";
import type { PendingArtisan } from "../type/pendingArtisan";

import ArtisanReviewModal from "./ArtisanReviewModal";

type TabType = "pending" | "reviewed";

const Verification = () => {
  const [activeTab, setActiveTab] = useState<TabType>("pending");

  const [selectedArtisan, setSelectedArtisan] =
    useState<PendingArtisan | null>(null);

  const {
    data: artisans = [],
    isLoading,
    isError,
  } = useGetPendingArtisans();

  return (
    <section dir="rtl" className="p-6 md:p-8">

      {/* Header */}
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-900">
          توثيق الفنيين
        </h1>

        <p className="mt-2 text-sm text-gray-500">
          مراجعة طلبات انضمام الفنيين إلى منصة صنايعي
        </p>
      </div>

      {/* Tabs */}
      <div className="mb-6 flex gap-6 border-b border-gray-200">

        <button
          onClick={() => setActiveTab("pending")}
          className={`relative pb-4 text-sm font-semibold transition ${
            activeTab === "pending"
              ? "text-blue-600"
              : "text-gray-500 hover:text-gray-800"
          }`}
        >
          قيد المراجعة

          <span className="mr-2 rounded-full bg-blue-100 px-2 py-1 text-xs text-blue-700">
            {artisans.length}
          </span>

          {activeTab === "pending" && (
            <span className="absolute bottom-0 right-0 h-0.5 w-full bg-blue-600" />
          )}
        </button>

        <button
          onClick={() => setActiveTab("reviewed")}
          className={`relative pb-4 text-sm font-semibold transition ${
            activeTab === "reviewed"
              ? "text-blue-600"
              : "text-gray-500 hover:text-gray-800"
          }`}
        >
          تمت مراجعتها

          {activeTab === "reviewed" && (
            <span className="absolute bottom-0 right-0 h-0.5 w-full bg-blue-600" />
          )}
        </button>

      </div>

      {/* Content */}
      <div className="overflow-hidden rounded-xl border border-gray-200 bg-white">

        {isLoading ? (
          <div className="p-10 text-center text-gray-500">
            جاري تحميل الفنيين...
          </div>

        ) : isError ? (
          <div className="p-10 text-center text-red-500">
            حدث خطأ أثناء تحميل البيانات
          </div>

        ) : activeTab === "reviewed" ? (
          <div className="p-10 text-center text-gray-500">
            لا يوجد API لجلب الفنيين الذين تمت مراجعتهم حتى الآن.
          </div>

        ) : artisans.length === 0 ? (
          <div className="p-10 text-center text-gray-500">
            لا يوجد فنيون قيد المراجعة حالياً.
          </div>

        ) : (
          <div className="overflow-x-auto">

            <table className="w-full text-right">

              <thead className="bg-gray-50">
                <tr className="text-sm text-gray-500">

                  <th className="whitespace-nowrap px-6 py-4 font-medium">
                    المتقدم
                  </th>

                  <th className="whitespace-nowrap px-6 py-4 font-medium">
                    الفئة
                  </th>

                  <th className="whitespace-nowrap px-6 py-4 font-medium">
                    الخبرة
                  </th>

                  <th className="whitespace-nowrap px-6 py-4 font-medium">
                    المنطقة
                  </th>

                  <th className="whitespace-nowrap px-6 py-4 font-medium">
                    تاريخ التقديم
                  </th>

                  <th className="whitespace-nowrap px-6 py-4 font-medium">
                    الحالة
                  </th>

                </tr>
              </thead>

              <tbody className="divide-y divide-gray-100">

                {artisans.map((artisan) => (

                  <tr
                    key={artisan.userId}
                    className="text-sm transition hover:bg-gray-50"
                  >

                    {/* Artisan Name - Open Modal */}
                    <td className="whitespace-nowrap px-6 py-5">

                      <button
                        onClick={() => setSelectedArtisan(artisan)}
                        className="font-semibold text-gray-900 transition hover:text-blue-600 hover:underline"
                      >
                        {artisan.fullName}
                      </button>

                    </td>

                    <td className="whitespace-nowrap px-6 py-5 text-gray-600">
                      {artisan.profession}
                    </td>

                    <td className="whitespace-nowrap px-6 py-5 text-gray-600">
                      {artisan.experienceYears} سنوات
                    </td>

                    <td className="whitespace-nowrap px-6 py-5 text-gray-600">
                      {artisan.serviceArea}
                    </td>

                    <td className="whitespace-nowrap px-6 py-5 text-gray-600">
                      {artisan.createdAtUtc
                        ? new Date(artisan.createdAtUtc).toLocaleDateString(
                            "ar-EG"
                          )
                        : "—"}
                    </td>

                    <td className="whitespace-nowrap px-6 py-5">

                      <span className="rounded-full bg-amber-50 px-3 py-1.5 text-xs font-medium text-amber-700">
                        قيد المراجعة
                      </span>

                    </td>

                  </tr>

                ))}

              </tbody>

            </table>

          </div>
        )}

      </div>

      {/* Artisan Review Modal */}
      {selectedArtisan && (
        <ArtisanReviewModal
          artisan={selectedArtisan}
          onClose={() => setSelectedArtisan(null)}
        />
      )}

    </section>
  );
};

export default Verification;