import { useState } from "react";
import { useCreateTopUp } from "../hooks/WalletHooks";

const amounts = [100, 200, 500, 1000];

const TopUpForm = () => {
  const [amount, setAmount] = useState(500);

  const { mutate, isPending, isSuccess, error, reset } =
    useCreateTopUp();

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    if (amount <= 0) return;

    reset();
    mutate(amount);
  };

  return (
    <form
      onSubmit={handleSubmit}
      dir="rtl"
      className="max-w-xl space-y-6 rounded-2xl border border-gray-100 bg-white p-6 shadow-sm"
    >
      <div>
        <h2 className="text-xl font-bold text-gray-900">
          شحن المحفظة
        </h2>
        <p className="mt-2 text-sm text-gray-500">
          اختر المبلغ الذي تريد شحنه.
        </p>
      </div>

      {/* Preset Amounts */}
      <div>
        <label className="mb-3 block text-sm font-semibold text-gray-700">
          اختر مبلغ الشحن
        </label>

        <div className="grid grid-cols-2 gap-3">
          {amounts.map((value) => (
            <button
              key={value}
              type="button"
              onClick={() => {
                setAmount(value);
                reset();
              }}
              className={`rounded-xl border p-4 font-bold transition ${
                amount === value
                  ? "border-[#0966C1] bg-blue-50 text-[#0966C1]"
                  : "border-gray-200 text-gray-700 hover:border-blue-300"
              }`}
            >
              {value} جنيه
            </button>
          ))}
        </div>
      </div>

      {/* Custom Amount */}
      <div>
        <label className="mb-2 block text-sm font-semibold text-gray-700">
          أو أدخل مبلغًا آخر
        </label>

        <div className="relative">
          <input
            type="number"
            min="1"
            value={amount}
            onChange={(e) => {
              setAmount(Number(e.target.value));
              reset();
            }}
            className="w-full rounded-xl border border-gray-200 p-3 pl-12 outline-none transition focus:border-[#0966C1]"
          />

          <span className="absolute left-4 top-1/2 -translate-y-1/2 text-sm text-gray-400">
            ج.م
          </span>
        </div>
      </div>

      {/* Submit */}
      <button
        type="submit"
        disabled={isPending || amount <= 0}
        className="w-full rounded-xl bg-[#0966C1] py-3 font-semibold text-white transition hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
      >
        {isPending ? "جاري إنشاء طلب الشحن..." : "متابعة الشحن"}
      </button>

      {isSuccess && (
        <p className="rounded-xl bg-green-50 p-3 text-sm text-green-700">
          تم إنشاء طلب الشحن بنجاح.
        </p>
      )}

      {error && (
        <p className="rounded-xl bg-red-50 p-3 text-sm text-red-600">
          حدث خطأ أثناء إنشاء طلب الشحن.
        </p>
      )}
    </form>
  );
};

export default TopUpForm;