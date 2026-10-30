
using System;

namespace Sanay3y.API.Models
{
    public class PlatformWallet
    {
        public int Id { get; set; }

        // الحساب الرئيسي لأموال المنصة
        // ستكون قيمته MAIN
        public string AccountCode { get; set; } = "MAIN";

        // الأرباح التي تم تحصيلها فعليًا والمتاحة للسحب
        public decimal Balance { get; set; } = 0m;

        public string Currency { get; set; } = "EGP";

        public DateTime CreatedAtUtc { get; set; }
            = DateTime.UtcNow;

        public DateTime UpdatedAtUtc { get; set; }
            = DateTime.UtcNow;
    }
}
