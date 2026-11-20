const LoginSidebar = () => {
  return (
      <div className="hidden lg:flex lg:w-1/2 bg-gradient-to-br from-blue-600 to-blue-800 flex-col justify-between p-16 text-white relative overflow-hidden">
        <div className="absolute -top-24 -left-24 w-96 h-96 rounded-full bg-blue-500/20 blur-3xl pointer-events-none"></div>
        <div className="absolute -bottom-24 -right-24 w-96 h-96 rounded-full bg-blue-400/20 blur-3xl pointer-events-none"></div>

        <div className="z-10">
          <span className="inline-block bg-blue-500/30 backdrop-blur-md text-blue-100 text-xs font-bold px-3 py-1.5 rounded-full border border-blue-400/30">
            Verified Platform
          </span>
        </div>

        <div className="z-10 space-y-6 max-w-lg">
          <h2 className="text-4xl lg:text-5xl font-black tracking-tight leading-tight">
            كل خدمات الصيانة المنزلية في مكان واحد
          </h2>
          <p className="text-blue-100 text-lg leading-relaxed">
            Verified technicians. Transparent offers. Effortless booking.
          </p>
        </div>

        <div className="z-10 flex items-center gap-4 bg-white/10 backdrop-blur-md p-4 rounded-2xl border border-white/10 w-fit">
          <div className="flex items-center gap-1 text-amber-400 text-lg">
            ★ ★ ★ ★ ★
          </div>
          <div>
            <p className="text-sm font-bold text-white">4.9 avg.</p>
            <p className="text-xs text-blue-200">Verified technicians</p>
          </div>
        </div>
      </div>
  );
};

export default LoginSidebar;