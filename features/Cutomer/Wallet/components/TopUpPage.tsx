import TopUpForm from "../components/TopUpForm";

const TopUpPage = () => {
  return (
    <div dir="rtl" className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">شحن المحفظة</h1>
        <p className="mt-2 text-sm text-gray-500">
          اختر المبلغ الذي تريد إضافته إلى محفظتك.
        </p>
      </div>

      <TopUpForm />
    </div>
  );
};

export default TopUpPage;