
using Microsoft.EntityFrameworkCore;
using Sanay3y.API.Models;

namespace Sanay3y.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

        public DbSet<CustomerAddress> CustomerAddresses
            => Set<CustomerAddress>();

        public DbSet<ArtisanProfile> ArtisanProfiles
            => Set<ArtisanProfile>();

        public DbSet<OtpVerification> OtpVerifications
            => Set<OtpVerification>();

        public DbSet<ArtisanSpecialty> ArtisanSpecialties
            => Set<ArtisanSpecialty>();

        public DbSet<ArtisanServiceType> ArtisanServiceTypes
            => Set<ArtisanServiceType>();

        public DbSet<ArtisanServiceArea> ArtisanServiceAreas
            => Set<ArtisanServiceArea>();

        public DbSet<Specialty> Specialties
            => Set<Specialty>();

        public DbSet<HomePageContent> HomePageContents
            => Set<HomePageContent>();

        public DbSet<ServiceRequest> ServiceRequests
            => Set<ServiceRequest>();

        public DbSet<ServiceRequestAttachment> ServiceRequestAttachments
            => Set<ServiceRequestAttachment>();

        public DbSet<ServiceOffer> ServiceOffers
            => Set<ServiceOffer>();

        public DbSet<ServicePayment> ServicePayments
            => Set<ServicePayment>();

        public DbSet<PlatformCommission> PlatformCommissions
            => Set<PlatformCommission>();

        public DbSet<PlatformWallet> PlatformWallets
            => Set<PlatformWallet>();

        public DbSet<PlatformWalletTransaction> PlatformWalletTransactions
            => Set<PlatformWalletTransaction>();

        public DbSet<PlatformWithdrawal> PlatformWithdrawals
            => Set<PlatformWithdrawal>();

        public DbSet<ServiceReview> ServiceReviews
            => Set<ServiceReview>();

        public DbSet<Wallet> Wallets
            => Set<Wallet>();

        public DbSet<WalletTransaction> WalletTransactions
            => Set<WalletTransaction>();

        public DbSet<WalletTopUp> WalletTopUps
            => Set<WalletTopUp>();

        public DbSet<WalletWithdrawal> WalletWithdrawals
            => Set<WalletWithdrawal>();

        // Notifications
        public DbSet<Notification> Notifications
            => Set<Notification>();

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users", table =>
                {
                    table.HasCheckConstraint(
                        "CK_Users_EmailOrPhoneRequired",
                        "[Email] IS NOT NULL OR [PhoneNumber] IS NOT NULL");
                });

                entity.HasKey(u => u.Id);

                entity.Property(u => u.FullName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(u => u.Email)
                    .HasMaxLength(256);

                entity.Property(u => u.PhoneNumber)
                    .HasMaxLength(20);

                entity.Property(u => u.PasswordHash)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(u => u.Role)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(u => u.AccountStatus)
                    .HasMaxLength(30)
                    .IsRequired()
                    .HasDefaultValue("PendingVerification");

                entity.Property(u => u.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasIndex(u => u.Email)
                    .IsUnique()
                    .HasFilter("[Email] IS NOT NULL");

                entity.HasIndex(u => u.PhoneNumber)
                    .IsUnique()
                    .HasFilter("[PhoneNumber] IS NOT NULL");
            });

            // Customer addresses
            modelBuilder.Entity<CustomerAddress>(entity =>
            {
                entity.ToTable("CustomerAddresses", table =>
                {
                    table.HasCheckConstraint(
                        "CK_CustomerAddresses_LatitudeRange",
                        "[Latitude] IS NULL OR ([Latitude] >= -90 AND [Latitude] <= 90)");

                    table.HasCheckConstraint(
                        "CK_CustomerAddresses_LongitudeRange",
                        "[Longitude] IS NULL OR ([Longitude] >= -180 AND [Longitude] <= 180)");
                });

                entity.HasKey(a => a.Id);

                entity.Property(a => a.Title)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(a => a.Governorate)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(a => a.City)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(a => a.District)
                    .HasMaxLength(100);

                entity.Property(a => a.Street)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(a => a.BuildingNumber)
                    .HasMaxLength(50);

                entity.Property(a => a.FloorNumber)
                    .HasMaxLength(50);

                entity.Property(a => a.ApartmentNumber)
                    .HasMaxLength(50);

                entity.Property(a => a.Landmark)
                    .HasMaxLength(250);

                entity.Property(a => a.Latitude)
                    .HasPrecision(10, 7);

                entity.Property(a => a.Longitude)
                    .HasPrecision(10, 7);

                entity.Property(a => a.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()")
                    .IsRequired();

                entity.Property(a => a.IsDefault)
                    .HasDefaultValue(false)
                    .IsRequired();

                entity.HasOne(a => a.Customer)
                    .WithMany()
                    .HasForeignKey(a => a.CustomerUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(a => new
                {
                    a.CustomerUserId,
                    a.CreatedAtUtc
                });

                entity.HasIndex(a => a.CustomerUserId)
                    .IsUnique()
                    .HasFilter("[IsDefault] = 1");
            });

            // Artisan profiles
            modelBuilder.Entity<ArtisanProfile>(entity =>
            {
                entity.ToTable("ArtisanProfiles", table =>
                {
                    table.HasCheckConstraint(
                        "CK_ArtisanProfiles_ExperienceYears",
                        "[ExperienceYears] >= 0");
                });

                entity.HasKey(p => p.UserId);

                entity.Property(p => p.Profession)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(p => p.ExperienceYears)
                    .IsRequired();

                entity.Property(p => p.ServiceArea)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(p => p.Bio)
                    .HasMaxLength(1500);

                entity.Property(p => p.ProfileImagePath)
                    .HasMaxLength(300);

                entity.Property(p => p.NationalIdFrontPath)
                    .HasMaxLength(300);

                entity.Property(p => p.NationalIdBackPath)
                    .HasMaxLength(300);

                entity.Property(p => p.Governorate)
                    .HasMaxLength(100);

                entity.Property(p => p.Center)
                    .HasMaxLength(100);

                entity.Property(p => p.Village)
                    .HasMaxLength(100);

                entity.Property(p => p.TermsAccepted)
                    .HasDefaultValue(false)
                    .IsRequired();

                entity.Property(p => p.TermsVersion)
                    .HasMaxLength(30);

                entity.Property(p => p.ApprovalStatus)
                    .HasMaxLength(20)
                    .IsRequired()
                    .HasDefaultValue("Pending");

                entity.Property(p => p.RejectionReason)
                    .HasMaxLength(500);

                entity.HasOne(p => p.User)
                    .WithOne(u => u.ArtisanProfile)
                    .HasForeignKey<ArtisanProfile>(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.ReviewedByAdmin)
                    .WithMany(u => u.ReviewedArtisanProfiles)
                    .HasForeignKey(p => p.ReviewedByAdminId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(p => p.ApprovalStatus);

                entity.HasIndex(p => p.ReviewedByAdminId);

                entity.HasMany(p => p.Specialties)
                    .WithOne(s => s.ArtisanProfile)
                    .HasForeignKey(s => s.ArtisanUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.ServiceTypes)
                    .WithOne(s => s.ArtisanProfile)
                    .HasForeignKey(s => s.ArtisanUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.NearbyServiceAreas)
                    .WithOne(a => a.ArtisanProfile)
                    .HasForeignKey(a => a.ArtisanUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Artisan specialties
            modelBuilder.Entity<ArtisanSpecialty>(entity =>
            {
                entity.ToTable("ArtisanSpecialties");

                entity.HasKey(s => s.Id);

                entity.Property(s => s.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasIndex(s => new
                {
                    s.ArtisanUserId,
                    s.Name
                }).IsUnique();
            });

            // Artisan service types
            modelBuilder.Entity<ArtisanServiceType>(entity =>
            {
                entity.ToTable("ArtisanServiceTypes");

                entity.HasKey(s => s.Id);

                entity.Property(s => s.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasIndex(s => new
                {
                    s.ArtisanUserId,
                    s.Name
                }).IsUnique();
            });

            // Additional artisan service areas
            modelBuilder.Entity<ArtisanServiceArea>(entity =>
            {
                entity.ToTable("ArtisanServiceAreas");

                entity.HasKey(a => a.Id);

                entity.Property(a => a.Governorate)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(a => a.Center)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(a => a.Village)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasIndex(a => new
                {
                    a.ArtisanUserId,
                    a.Governorate,
                    a.Center,
                    a.Village
                }).IsUnique();
            });

            // OTP verifications
            modelBuilder.Entity<OtpVerification>(entity =>
            {
                entity.ToTable("OtpVerifications", table =>
                {
                    table.HasCheckConstraint(
                        "CK_OtpVerifications_Attempts",
                        "[Attempts] >= 0");
                });

                entity.HasKey(o => o.Id);

                entity.Property(o => o.Channel)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(o => o.CodeHash)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(o => o.TargetValue)
                    .HasMaxLength(256);

                entity.Property(o => o.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.Property(o => o.Attempts)
                    .HasDefaultValue(0);

                entity.HasOne(o => o.User)
                    .WithMany(u => u.OtpVerifications)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(o => new
                {
                    o.UserId,
                    o.Channel,
                    o.ExpiresAtUtc
                });
            });

            // Master specialties catalog
            modelBuilder.Entity<Specialty>(entity =>
            {
                entity.ToTable("Specialties");

                entity.HasKey(s => s.Id);

                entity.Property(s => s.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(s => s.NameEn)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(s => s.Description)
                    .HasMaxLength(500);

                entity.Property(s => s.IsActive)
                    .HasDefaultValue(true)
                    .IsRequired();

                entity.Property(s => s.SortOrder)
                    .IsRequired();

                entity.Property(s => s.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()")
                    .IsRequired();

                entity.HasIndex(s => s.Name)
                    .IsUnique();

                entity.HasIndex(s => s.NameEn)
                    .IsUnique();
            });

            // Homepage content
            modelBuilder.Entity<HomePageContent>(entity =>
            {
                entity.ToTable("HomePageContents");

                entity.HasKey(h => h.Id);

                entity.Property(h => h.PlatformName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(h => h.HeroTitle)
                    .HasMaxLength(250)
                    .IsRequired();

                entity.Property(h => h.HeroDescription)
                    .HasMaxLength(1000)
                    .IsRequired();

                entity.Property(h => h.AboutTitle)
                    .HasMaxLength(250)
                    .IsRequired();

                entity.Property(h => h.AboutDescription)
                    .HasMaxLength(1500)
                    .IsRequired();

                entity.Property(h => h.WhyUsTitle)
                    .HasMaxLength(250)
                    .IsRequired();

                entity.Property(h => h.WhyUsDescription)
                    .HasMaxLength(1000)
                    .IsRequired();

                entity.Property(h => h.HowItWorksTitle)
                    .HasMaxLength(250)
                    .IsRequired();

                entity.Property(h => h.Step1Title)
                    .HasMaxLength(250)
                    .IsRequired();

                entity.Property(h => h.Step2Title)
                    .HasMaxLength(250)
                    .IsRequired();

                entity.Property(h => h.Step3Title)
                    .HasMaxLength(250)
                    .IsRequired();

                entity.Property(h => h.ContactEmail)
                    .HasMaxLength(256);

                entity.Property(h => h.ContactPhone)
                    .HasMaxLength(20);

                entity.Property(h => h.PrivacyPolicyText)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(h => h.TermsText)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(h => h.FaqText)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(h => h.CopyrightText)
                    .HasMaxLength(200)
                    .IsRequired();
            });

            // Customer service requests
            modelBuilder.Entity<ServiceRequest>(entity =>
            {
                entity.ToTable("ServiceRequests", table =>
                {
                    table.HasCheckConstraint(
                        "CK_ServiceRequests_ScheduleChoice",
                        "([IsAsSoonAsPossible] = 1 AND [ScheduledAtUtc] IS NULL) " +
                        "OR ([IsAsSoonAsPossible] = 0 AND [ScheduledAtUtc] IS NOT NULL)");

                    table.HasCheckConstraint(
                        "CK_ServiceRequests_LatitudeRange",
                        "[Latitude] IS NULL OR ([Latitude] >= -90 AND [Latitude] <= 90)");

                    table.HasCheckConstraint(
                        "CK_ServiceRequests_LongitudeRange",
                        "[Longitude] IS NULL OR ([Longitude] >= -180 AND [Longitude] <= 180)");
                });

                entity.HasKey(r => r.Id);

                entity.Property(r => r.RequestNumber)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(r => r.ServiceTypeName)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(r => r.ProblemDescription)
                    .HasMaxLength(3000)
                    .IsRequired();

                entity.Property(r => r.AdditionalNotes)
                    .HasMaxLength(1000);

                entity.Property(r => r.AddressTitle)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(r => r.Governorate)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(r => r.City)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(r => r.District)
                    .HasMaxLength(100);

                entity.Property(r => r.Street)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(r => r.BuildingNumber)
                    .HasMaxLength(50);

                entity.Property(r => r.FloorNumber)
                    .HasMaxLength(50);

                entity.Property(r => r.ApartmentNumber)
                    .HasMaxLength(50);

                entity.Property(r => r.Landmark)
                    .HasMaxLength(250);

                entity.Property(r => r.Status)
                    .HasMaxLength(30)
                    .IsRequired()
                    .HasDefaultValue("PendingOffers");

                entity.Property(r => r.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.Property(r => r.UpdatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasIndex(r => r.RequestNumber)
                    .IsUnique();

                entity.HasIndex(r => new
                {
                    r.CustomerUserId,
                    r.CreatedAtUtc
                });

                entity.HasIndex(r => new
                {
                    r.SpecialtyId,
                    r.Status
                });

                entity.HasOne(r => r.Customer)
                    .WithMany()
                    .HasForeignKey(r => r.CustomerUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Specialty)
                    .WithMany()
                    .HasForeignKey(r => r.SpecialtyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Service request attachments
            modelBuilder.Entity<ServiceRequestAttachment>(entity =>
            {
                entity.ToTable("ServiceRequestAttachments", table =>
                {
                    table.HasCheckConstraint(
                        "CK_ServiceRequestAttachments_SizeBytes",
                        "[SizeBytes] >= 0");
                });

                entity.HasKey(a => a.Id);

                entity.Property(a => a.OriginalFileName)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(a => a.StoredFileName)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(a => a.ContentType)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(a => a.SizeBytes)
                    .IsRequired();

                entity.Property(a => a.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasOne(a => a.ServiceRequest)
                    .WithMany(r => r.Attachments)
                    .HasForeignKey(a => a.ServiceRequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(a => a.ServiceRequestId);
            });

            // Service offers
            modelBuilder.Entity<ServiceOffer>(entity =>
            {
                entity.ToTable("ServiceOffers", table =>
                {
                    table.HasCheckConstraint(
                        "CK_ServiceOffers_Price",
                        "[Price] > 0");

                    table.HasCheckConstraint(
                        "CK_ServiceOffers_EstimatedArrival",
                        "[EstimatedArrivalMinutes] IS NULL OR [EstimatedArrivalMinutes] > 0");

                    table.HasCheckConstraint(
                        "CK_ServiceOffers_Status",
                        "[Status] IN ('Pending', 'Accepted', 'Rejected', 'Withdrawn')");
                });

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Price)
                    .HasPrecision(18, 2)
                    .IsRequired();

                entity.Property(e => e.Message)
                    .HasMaxLength(1000);

                entity.Property(e => e.Status)
                    .HasMaxLength(30)
                    .IsRequired()
                    .HasDefaultValue("Pending");

                entity.Property(e => e.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()")
                    .IsRequired();

                entity.Property(e => e.UpdatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()")
                    .IsRequired();

                entity.HasIndex(e => new
                {
                    e.ServiceRequestId,
                    e.ArtisanUserId
                })
                .IsUnique();

                entity.HasIndex(e => new
                {
                    e.ServiceRequestId,
                    e.Status
                });

                entity.HasIndex(e => e.ServiceRequestId)
                    .IsUnique()
                    .HasFilter("[Status] = 'Accepted'");

                entity.HasOne(e => e.ServiceRequest)
                    .WithMany()
                    .HasForeignKey(e => e.ServiceRequestId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Artisan)
                    .WithMany()
                    .HasForeignKey(e => e.ArtisanUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Service payments
            modelBuilder.Entity<ServicePayment>(entity =>
            {
                entity.ToTable("ServicePayments", table =>
                {
                    table.HasCheckConstraint(
                        "CK_ServicePayments_Amount",
                        "[Amount] > 0");

                    table.HasCheckConstraint(
                        "CK_ServicePayments_Status",
                        "[Status] IN (" +
                        "'Pending', " +
                        "'AwaitingCustomerAction', " +
                        "'AwaitingCashConfirmation', " +
                        "'Paid', " +
                        "'Failed', " +
                        "'Cancelled', " +
                        "'Expired', " +
                        "'Refunded')");

                    table.HasCheckConstraint(
                        "CK_ServicePayments_CommissionRate",
                        "[CommissionRate] >= 0 AND [CommissionRate] <= 100");

                    table.HasCheckConstraint(
                        "CK_ServicePayments_CommissionAmount",
                        "[CommissionAmount] >= 0");

                    table.HasCheckConstraint(
                        "CK_ServicePayments_ArtisanNetAmount",
                        "[ArtisanNetAmount] >= 0");
                });

                entity.HasKey(p => p.Id);

                entity.Property(p => p.Amount)
                    .HasPrecision(18, 2)
                    .IsRequired();

                entity.Property(p => p.CommissionRate)
                    .HasPrecision(5, 2)
                    .HasDefaultValue(0m)
                    .IsRequired();

                entity.Property(p => p.CommissionAmount)
                    .HasPrecision(18, 2)
                    .HasDefaultValue(0m)
                    .IsRequired();

                entity.Property(p => p.ArtisanNetAmount)
                    .HasPrecision(18, 2)
                    .HasDefaultValue(0m)
                    .IsRequired();

                entity.Property(p => p.Currency)
                    .HasMaxLength(3)
                    .HasDefaultValue("EGP")
                    .IsRequired();

                entity.Property(p => p.PaymentMethod)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(p => p.Provider)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(p => p.Status)
                    .HasMaxLength(40)
                    .HasDefaultValue("Pending")
                    .IsRequired();

                entity.Property(p => p.ProviderOrderId)
                    .HasMaxLength(150);

                entity.Property(p => p.ProviderTransactionId)
                    .HasMaxLength(150);

                entity.Property(p => p.CheckoutUrl)
                    .HasMaxLength(2000);

                entity.Property(p => p.PaymentReference)
                    .HasMaxLength(150);

                entity.Property(p => p.FailureReason)
                    .HasMaxLength(500);

                entity.Property(p => p.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.Property(p => p.UpdatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasOne(p => p.ServiceRequest)
                    .WithMany()
                    .HasForeignKey(p => p.ServiceRequestId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(p => p.CustomerUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(p => p.ServiceRequestId)
                    .IsUnique()
                    .HasFilter(
                        "[Status] IN (" +
                        "'Pending', " +
                        "'AwaitingCustomerAction', " +
                        "'AwaitingCashConfirmation')");

                entity.HasIndex(p => p.ProviderTransactionId)
                    .IsUnique()
                    .HasFilter("[ProviderTransactionId] IS NOT NULL");

                entity.HasIndex(p => new
                {
                    p.CustomerUserId,
                    p.CreatedAtUtc
                });
            });

            // Platform commissions
            modelBuilder.Entity<PlatformCommission>(entity =>
            {
                entity.ToTable("PlatformCommissions", table =>
                {
                    table.HasCheckConstraint(
                        "CK_PlatformCommissions_Amount",
                        "[Amount] > 0");

                    table.HasCheckConstraint(
                        "CK_PlatformCommissions_Status",
                        "[Status] IN ('Collected', 'Due', 'Reversed')");
                });

                entity.HasKey(c => c.Id);

                entity.Property(c => c.Amount)
                    .HasPrecision(18, 2)
                    .IsRequired();

                entity.Property(c => c.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue("Collected")
                    .IsRequired();

                entity.Property(c => c.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()")
                    .IsRequired();

                entity.HasIndex(c => c.ServicePaymentId)
                    .IsUnique();

                entity.HasIndex(c => new
                {
                    c.ArtisanUserId,
                    c.Status,
                    c.CreatedAtUtc
                });

                entity.HasOne<ServicePayment>()
                    .WithMany()
                    .HasForeignKey(c => c.ServicePaymentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(c => c.ArtisanUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Service reviews
            modelBuilder.Entity<ServiceReview>(entity =>
            {
                entity.ToTable("ServiceReviews", table =>
                {
                    table.HasCheckConstraint(
                        "CK_ServiceReviews_Rating",
                        "[Rating] >= 1 AND [Rating] <= 5");
                });

                entity.HasKey(r => r.Id);

                entity.Property(r => r.Rating)
                    .IsRequired();

                entity.Property(r => r.Comment)
                    .HasMaxLength(1000);

                entity.Property(r => r.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()")
                    .IsRequired();

                entity.HasOne<ServiceRequest>()
                    .WithMany()
                    .HasForeignKey(r => r.ServiceRequestId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(r => r.CustomerUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(r => r.ArtisanUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(r => r.ServiceRequestId)
                    .IsUnique();

                entity.HasIndex(r => new
                {
                    r.ArtisanUserId,
                    r.CreatedAtUtc
                });
            });

            // Wallets
            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.ToTable("Wallets", table =>
                {
                    table.HasCheckConstraint(
                        "CK_Wallets_Balance",
                        "[Balance] >= 0");
                });

                entity.HasKey(w => w.Id);

                entity.Property(w => w.Balance)
                    .HasPrecision(18, 2)
                    .HasDefaultValue(0m)
                    .IsRequired();

                entity.Property(w => w.Currency)
                    .HasMaxLength(3)
                    .HasDefaultValue("EGP")
                    .IsRequired();

                entity.Property(w => w.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()")
                    .IsRequired();

                entity.Property(w => w.UpdatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()")
                    .IsRequired();

                entity.HasIndex(w => w.UserId)
                    .IsUnique();

                entity.HasOne(w => w.User)
                    .WithMany()
                    .HasForeignKey(w => w.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Wallet transactions
            modelBuilder.Entity<WalletTransaction>(entity =>
            {
                entity.ToTable("WalletTransactions", table =>
                {
                    table.HasCheckConstraint(
                        "CK_WalletTransactions_Amount",
                        "[Amount] > 0");

                    table.HasCheckConstraint(
                        "CK_WalletTransactions_BalanceAfter",
                        "[BalanceAfter] >= 0");

                    table.HasCheckConstraint(
                        "CK_WalletTransactions_Direction",
                        "[Direction] IN ('Credit', 'Debit')");

                    table.HasCheckConstraint(
                        "CK_WalletTransactions_Status",
                        "[Status] IN ('Pending', 'Completed', 'Failed', 'Cancelled')");

                    table.HasCheckConstraint(
                        "CK_WalletTransactions_Type",
                        "[Type] IN (" +
                        "'TopUp', " +
                        "'ServicePaymentDebit', " +
                        "'ServicePaymentCredit', " +
                        "'RefundCredit', " +
                        "'WithdrawalDebit', " +
                        "'WithdrawalRefund')");
                });

                entity.HasKey(t => t.Id);

                entity.Property(t => t.Type)
                    .HasMaxLength(40)
                    .IsRequired();

                entity.Property(t => t.Direction)
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(t => t.Amount)
                    .HasPrecision(18, 2)
                    .IsRequired();

                entity.Property(t => t.BalanceAfter)
                    .HasPrecision(18, 2)
                    .IsRequired();

                entity.Property(t => t.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue("Pending")
                    .IsRequired();

                entity.Property(t => t.Description)
                    .HasMaxLength(500);

                entity.Property(t => t.Provider)
                    .HasMaxLength(30);

                entity.Property(t => t.ProviderTransactionId)
                    .HasMaxLength(150);

                entity.Property(t => t.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()")
                    .IsRequired();

                entity.HasOne(t => t.Wallet)
                    .WithMany(w => w.Transactions)
                    .HasForeignKey(t => t.WalletId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.ServicePayment)
                    .WithMany()
                    .HasForeignKey(t => t.ServicePaymentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.WalletWithdrawal)
                    .WithMany()
                    .HasForeignKey(t => t.WalletWithdrawalId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(t => new
                {
                    t.WalletId,
                    t.CreatedAtUtc
                });

                entity.HasIndex(t => new
                {
                    t.Provider,
                    t.ProviderTransactionId
                })
                .IsUnique()
                .HasFilter("[ProviderTransactionId] IS NOT NULL");

                entity.HasIndex(t => t.WalletWithdrawalId);
            });

            // Wallet top-up requests
            modelBuilder.Entity<WalletTopUp>(entity =>
            {
                entity.ToTable("WalletTopUps", table =>
                {
                    table.HasCheckConstraint(
                        "CK_WalletTopUps_Amount",
                        "[Amount] > 0");

                    table.HasCheckConstraint(
                        "CK_WalletTopUps_Status",
                        "[Status] IN (" +
                        "'Pending', " +
                        "'Paid', " +
                        "'Failed', " +
                        "'Cancelled', " +
                        "'Expired')");
                });

                entity.HasKey(t => t.Id);

                entity.Property(t => t.Amount)
                    .HasPrecision(18, 2)
                    .IsRequired();

                entity.Property(t => t.Currency)
                    .HasMaxLength(3)
                    .HasDefaultValue("EGP")
                    .IsRequired();

                entity.Property(t => t.Provider)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(t => t.SpecialReference)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(t => t.ProviderOrderId)
                    .HasMaxLength(100);

                entity.Property(t => t.ProviderIntentionId)
                    .HasMaxLength(150);

                entity.Property(t => t.ProviderTransactionId)
                    .HasMaxLength(100);

                entity.Property(t => t.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue("Pending")
                    .IsRequired();

                entity.Property(t => t.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()")
                    .IsRequired();

                entity.Property(t => t.UpdatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()")
                    .IsRequired();

                entity.HasOne(t => t.Wallet)
                    .WithMany()
                    .HasForeignKey(t => t.WalletId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(t => t.SpecialReference)
                    .IsUnique();

                entity.HasIndex(t => t.ProviderOrderId)
                    .IsUnique()
                    .HasFilter("[ProviderOrderId] IS NOT NULL");

                entity.HasIndex(t => t.ProviderTransactionId)
                    .IsUnique()
                    .HasFilter("[ProviderTransactionId] IS NOT NULL");

                entity.HasIndex(t => new
                {
                    t.WalletId,
                    t.CreatedAtUtc
                });
            });

            // Wallet withdrawal requests
            modelBuilder.Entity<WalletWithdrawal>(entity =>
            {
                entity.ToTable("WalletWithdrawals", table =>
                {
                    table.HasCheckConstraint(
                        "CK_WalletWithdrawals_Amount",
                        "[Amount] > 0");

                    table.HasCheckConstraint(
                        "CK_WalletWithdrawals_Status",
                        "[Status] IN (" +
                        "'Pending', " +
                        "'Approved', " +
                        "'Rejected', " +
                        "'Paid', " +
                        "'Failed', " +
                        "'Cancelled')");

                    table.HasCheckConstraint(
                        "CK_WalletWithdrawals_PayoutMethod",
                        "[PayoutMethod] IN ('MobileWallet', 'BankTransfer')");
                });

                entity.HasKey(w => w.Id);

                entity.Property(w => w.Amount)
                    .HasPrecision(18, 2)
                    .IsRequired();

                entity.Property(w => w.Currency)
                    .HasMaxLength(3)
                    .HasDefaultValue("EGP")
                    .IsRequired();

                entity.Property(w => w.PayoutMethod)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(w => w.PayoutProvider)
                    .HasMaxLength(50);

                entity.Property(w => w.AccountHolderName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(w => w.DestinationAccount)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(w => w.BankName)
                    .HasMaxLength(100);

                entity.Property(w => w.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue("Pending")
                    .IsRequired();

                entity.Property(w => w.AdminNotes)
                    .HasMaxLength(500);

                entity.Property(w => w.RejectionReason)
                    .HasMaxLength(500);

                entity.Property(w => w.TransferReference)
                    .HasMaxLength(150);

                entity.Property(w => w.RequestedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()")
                    .IsRequired();

                entity.Property(w => w.UpdatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()")
                    .IsRequired();

                entity.HasOne(w => w.Wallet)
                    .WithMany()
                    .HasForeignKey(w => w.WalletId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(w => w.ReviewedByAdmin)
                    .WithMany()
                    .HasForeignKey(w => w.ReviewedByAdminId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(w => new
                {
                    w.WalletId,
                    w.RequestedAtUtc
                });

                entity.HasIndex(w => new
                {
                    w.Status,
                    w.RequestedAtUtc
                });

                entity.HasIndex(w => w.ReviewedByAdminId);
            });

            // Platform main wallet
            modelBuilder.Entity<PlatformWallet>(entity =>
            {
                entity.ToTable("PlatformWallets", table =>
                {
                    table.HasCheckConstraint(
                        "CK_PlatformWallets_Balance",
                        "[Balance] >= 0");
                });

                entity.HasKey(w => w.Id);

                entity.Property(w => w.AccountCode)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(w => w.Balance)
                    .HasPrecision(18, 2)
                    .HasDefaultValue(0m)
                    .IsRequired();

                entity.Property(w => w.Currency)
                    .HasMaxLength(3)
                    .HasDefaultValue("EGP")
                    .IsRequired();

                entity.Property(w => w.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()")
                    .IsRequired();

                entity.Property(w => w.UpdatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()")
                    .IsRequired();

                entity.HasIndex(w => w.AccountCode)
                    .IsUnique();
            });

            // Platform wallet transactions
            modelBuilder.Entity<PlatformWalletTransaction>(entity =>
            {
                entity.ToTable("PlatformWalletTransactions", table =>
                {
                    table.HasCheckConstraint(
                        "CK_PlatformWalletTransactions_Amount",
                        "[Amount] > 0");

                    table.HasCheckConstraint(
                        "CK_PlatformWalletTransactions_BalanceAfter",
                        "[BalanceAfter] >= 0");

                    table.HasCheckConstraint(
                        "CK_PlatformWalletTransactions_Direction",
                        "[Direction] IN ('Credit', 'Debit')");

                    table.HasCheckConstraint(
                        "CK_PlatformWalletTransactions_Status",
                        "[Status] IN ('Pending', 'Completed', 'Failed', 'Cancelled')");

                    table.HasCheckConstraint(
                        "CK_PlatformWalletTransactions_Type",
                        "[Type] IN (" +
                        "'CommissionCredit', " +
                        "'WithdrawalDebit', " +
                        "'WithdrawalRefund')");
                });

                entity.HasKey(t => t.Id);

                entity.Property(t => t.Type)
                    .HasMaxLength(40)
                    .IsRequired();

                entity.Property(t => t.Direction)
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(t => t.Amount)
                    .HasPrecision(18, 2)
                    .IsRequired();

                entity.Property(t => t.BalanceAfter)
                    .HasPrecision(18, 2)
                    .IsRequired();

                entity.Property(t => t.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue("Completed")
                    .IsRequired();

                entity.Property(t => t.Description)
                    .HasMaxLength(500);

                entity.Property(t => t.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()")
                    .IsRequired();

                entity.HasOne(t => t.PlatformWallet)
                    .WithMany()
                    .HasForeignKey(t => t.PlatformWalletId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<PlatformCommission>()
                    .WithMany()
                    .HasForeignKey(t => t.PlatformCommissionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<PlatformWithdrawal>()
                    .WithMany()
                    .HasForeignKey(t => t.PlatformWithdrawalId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(t => new
                {
                    t.PlatformWalletId,
                    t.CreatedAtUtc
                });

                entity.HasIndex(t => t.PlatformCommissionId)
                    .IsUnique()
                    .HasFilter("[PlatformCommissionId] IS NOT NULL");

                entity.HasIndex(t => t.PlatformWithdrawalId);
            });

            // Platform withdrawal requests
            modelBuilder.Entity<PlatformWithdrawal>(entity =>
            {
                entity.ToTable("PlatformWithdrawals", table =>
                {
                    table.HasCheckConstraint(
                        "CK_PlatformWithdrawals_Amount",
                        "[Amount] > 0");

                    table.HasCheckConstraint(
                        "CK_PlatformWithdrawals_Status",
                        "[Status] IN (" +
                        "'Pending', " +
                        "'Approved', " +
                        "'Rejected', " +
                        "'Paid', " +
                        "'Failed')");

                    table.HasCheckConstraint(
                        "CK_PlatformWithdrawals_PayoutMethod",
                        "[PayoutMethod] IN ('MobileWallet', 'BankTransfer')");
                });

                entity.HasKey(w => w.Id);

                entity.Property(w => w.Amount)
                    .HasPrecision(18, 2)
                    .IsRequired();

                entity.Property(w => w.Currency)
                    .HasMaxLength(3)
                    .HasDefaultValue("EGP")
                    .IsRequired();

                entity.Property(w => w.PayoutMethod)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(w => w.PayoutProvider)
                    .HasMaxLength(50);

                entity.Property(w => w.AccountHolderName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(w => w.DestinationAccount)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(w => w.BankName)
                    .HasMaxLength(100);

                entity.Property(w => w.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue("Pending")
                    .IsRequired();

                entity.Property(w => w.AdminNotes)
                    .HasMaxLength(500);

                entity.Property(w => w.RejectionReason)
                    .HasMaxLength(500);

                entity.Property(w => w.TransferReference)
                    .HasMaxLength(150);

                entity.Property(w => w.RequestedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()")
                    .IsRequired();

                entity.Property(w => w.UpdatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()")
                    .IsRequired();

                entity.HasOne(w => w.PlatformWallet)
                    .WithMany()
                    .HasForeignKey(w => w.PlatformWalletId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(w => w.RequestedByAdmin)
                    .WithMany()
                    .HasForeignKey(w => w.RequestedByAdminId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(w => w.ReviewedByAdmin)
                    .WithMany()
                    .HasForeignKey(w => w.ReviewedByAdminId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(w => new
                {
                    w.PlatformWalletId,
                    w.RequestedAtUtc
                });

                entity.HasIndex(w => new
                {
                    w.Status,
                    w.RequestedAtUtc
                });

                entity.HasIndex(w => w.RequestedByAdminId);

                entity.HasIndex(w => w.ReviewedByAdminId);
            });

            // Notifications
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");

                entity.HasKey(n => n.Id);

                entity.Property(n => n.Title)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(n => n.Message)
                    .HasMaxLength(1000)
                    .IsRequired();

                entity.Property(n => n.Type)
                    .HasMaxLength(50)
                    .IsRequired()
                    .HasDefaultValue("System");

                entity.Property(n => n.IsRead)
                    .HasDefaultValue(false)
                    .IsRequired();

                entity.Property(n => n.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()")
                    .IsRequired();

                entity.Property(n => n.RelatedEntityType)
                    .HasMaxLength(50);

                entity.HasOne(n => n.User)
                    .WithMany()
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(n => new
                {
                    n.UserId,
                    n.IsRead,
                    n.CreatedAtUtc
                });
            });
        }
    }
}
