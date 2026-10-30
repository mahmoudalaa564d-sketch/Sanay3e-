
using System;
using System.Collections.Generic;

namespace Sanay3y.API.Models
{
    public class Wallet
    {
        public int Id { get; set; }

        // ID صاحب المحفظة
        public int UserId { get; set; }

        // الرصيد الحالي بالجنيه المصري
        public decimal Balance { get; set; } = 0m;

        public string Currency { get; set; } = "EGP";

        public DateTime CreatedAtUtc { get; set; }
            = DateTime.UtcNow;

        public DateTime UpdatedAtUtc { get; set; }
            = DateTime.UtcNow;

        // المستخدم صاحب المحفظة
        public User User { get; set; } = null!;

        // سجل جميع عمليات المحفظة
        public ICollection<WalletTransaction> Transactions
        {
            get;
            set;
        } = new List<WalletTransaction>();
    }
}
