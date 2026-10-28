
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

        // Master list of platform specialties
        public DbSet<Specialty> Specialties
            => Set<Specialty>();

        // Database-driven homepage content
        public DbSet<HomePageContent> HomePageContents
            => Set<HomePageContent>();

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

            // Existing specialties assigned to individual artisans
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

            // Types of services offered by an artisan
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

            // Additional service areas
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
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(o => o.CodeHash)
                    .HasMaxLength(500)
                    .IsRequired();

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

            // Homepage content stored in the database
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
        }
    }
}
