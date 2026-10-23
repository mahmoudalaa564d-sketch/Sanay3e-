
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

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // إعداد جدول المستخدمين.
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

            // إعداد جدول ملفات الصنايعية.
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

                entity.Property(p => p.ServiceArea)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(p => p.ApprovalStatus)
                    .HasMaxLength(20)
                    .IsRequired()
                    .HasDefaultValue("Pending");

                entity.Property(p => p.RejectionReason)
                    .HasMaxLength(500);

                // كل صنايعي له ملف واحد.
                entity.HasOne(p => p.User)
                    .WithOne(u => u.ArtisanProfile)
                    .HasForeignKey<ArtisanProfile>(
                        p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // الأدمن الذي راجع طلب الصنايعي.
                entity.HasOne(p => p.ReviewedByAdmin)
                    .WithMany(u => u.ReviewedArtisanProfiles)
                    .HasForeignKey(p => p.ReviewedByAdminId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(p => p.ApprovalStatus);
            });

            // إعداد جدول أكواد التحقق.
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
        }
    }
}
