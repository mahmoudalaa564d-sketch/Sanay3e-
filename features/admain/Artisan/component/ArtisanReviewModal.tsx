import { useState } from "react";
import type { PendingArtisan } from "../type/pendingArtisan";

import { useApproveArtisan } from "../hooks/useApproveArtisan";
import { useRejectArtisan } from "../hooks/useRejectArtisan";

interface Props {
  artisan: PendingArtisan;
  onClose: () => void;
}

const ArtisanReviewModal = ({ artisan, onClose }: Props) => {
  const [reason, setReason] = useState("");
  const [error, setError] = useState("");

  const approveMutation = useApproveArtisan();
  const rejectMutation = useRejectArtisan();

  const isPending =
    approveMutation.isPending || rejectMutation.isPending;

  const handleApprove = async () => {
    try {
      await approveMutation.mutateAsync(artisan.userId);
      onClose();
    } catch {
      setError("حدث خطأ أثناء قبول الفني");
    }
  };

  const handleReject = async () => {
    const trimmedReason = reason.trim();

    if (trimmedReason.length < 5) {
      setError("يرجى إدخال سبب رفض لا يقل عن 5 أحرف");
      return;
    }

    if (trimmedReason.length > 500) {
      setError("سبب الرفض يجب ألا يتجاوز 500 حرف");
      return;
    }

    try {
      await rejectMutation.mutateAsync({
        userId: artisan.userId,
        reason: trimmedReason,
      });

      onClose();
    } catch {
      setError("حدث خطأ أثناء رفض الفني");
    }
  };

  return (
    <div
      onClick={(e) => {
        if (e.target === e.currentTarget) onClose();
      }}
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4"
    >
      <div
        dir="rtl"
        className="w-full max-w-lg rounded-2xl bg-white p-6 shadow-2xl"
      >
        {/* Header */}
        <div className="mb-6 flex items-center justify-between">
          <h2 className="text-xl font-bold text-gray-900">
            مراجعة طلب الفني
          </h2>

          <button
            onClick={onClose}
            disabled={isPending}
            className="text-2xl text-gray-400 hover:text-gray-700"
          >
            ×
          </button>
        </div>

        {/* Artisan Info */}
        <div className="mb-6 rounded-xl bg-gray-50 p-4">
          <h3 className="mb-3 font-bold text-gray-900">
            {artisan.fullName}
          </h3>

          <div className="space-y-2 text-sm text-gray-600">
            <p>الفئة: {artisan.profession}</p>

            <p>الخبرة: {artisan.experienceYears} سنوات</p>

            <p>المنطقة: {artisan.serviceArea}</p>

            <p>رقم الهاتف: {artisan.phoneNumber || "غير متوفر"}</p>
          </div>
        </div>

        {/* Rejection Reason */}
        <div className="mb-6">
          <label className="mb-2 block text-sm font-semibold text-gray-800">
            يرجى إدخال السبب عند الرفض
          </label>

          <textarea
            value={reason}
            onChange={(e) => {
              setReason(e.target.value);
              setError("");
            }}
            placeholder="اكتب سبب رفض طلب الفني..."
            rows={4}
            maxLength={500}
            className="w-full resize-none rounded-xl border border-gray-200 p-3 text-sm outline-none transition focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
          />

          <p className="mt-1 text-left text-xs text-gray-400">
            {reason.length}/500
          </p>
        </div>

        {/* Error */}
        {error && (
          <p className="mb-4 rounded-lg bg-red-50 p-3 text-sm text-red-600">
            {error}
          </p>
        )}

        {/* Buttons */}
        <div className="flex gap-3">
          <button
            onClick={handleApprove}
            disabled={isPending}
            className="flex-1 rounded-xl bg-green-600 py-3 font-semibold text-white transition hover:bg-green-700 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {approveMutation.isPending ? "جاري القبول..." : "قبول الفني"}
          </button>

          <button
            onClick={handleReject}
            disabled={isPending}
            className="flex-1 rounded-xl bg-red-600 py-3 font-semibold text-white transition hover:bg-red-700 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {rejectMutation.isPending ? "جاري الرفض..." : "رفض الفني"}
          </button>
        </div>
      </div>
    </div>
  );
};

export default ArtisanReviewModal;