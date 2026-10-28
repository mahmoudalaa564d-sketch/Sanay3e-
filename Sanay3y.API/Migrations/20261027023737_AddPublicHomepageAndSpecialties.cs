
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sanay3y.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicHomepageAndSpecialties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create homepage content table
            migrationBuilder.CreateTable(
                name: "HomePageContents",
                columns: table => new
                {
                    Id = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    PlatformName = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false),

                    HeroTitle = table.Column<string>(
                        type: "nvarchar(250)",
                        maxLength: 250,
                        nullable: false),

                    HeroDescription = table.Column<string>(
                        type: "nvarchar(1000)",
                        maxLength: 1000,
                        nullable: false),

                    AboutTitle = table.Column<string>(
                        type: "nvarchar(250)",
                        maxLength: 250,
                        nullable: false),

                    AboutDescription = table.Column<string>(
                        type: "nvarchar(1500)",
                        maxLength: 1500,
                        nullable: false),

                    WhyUsTitle = table.Column<string>(
                        type: "nvarchar(250)",
                        maxLength: 250,
                        nullable: false),

                    WhyUsDescription = table.Column<string>(
                        type: "nvarchar(1000)",
                        maxLength: 1000,
                        nullable: false),

                    HowItWorksTitle = table.Column<string>(
                        type: "nvarchar(250)",
                        maxLength: 250,
                        nullable: false),

                    Step1Title = table.Column<string>(
                        type: "nvarchar(250)",
                        maxLength: 250,
                        nullable: false),

                    Step2Title = table.Column<string>(
                        type: "nvarchar(250)",
                        maxLength: 250,
                        nullable: false),

                    Step3Title = table.Column<string>(
                        type: "nvarchar(250)",
                        maxLength: 250,
                        nullable: false),

                    ContactEmail = table.Column<string>(
                        type: "nvarchar(256)",
                        maxLength: 256,
                        nullable: true),

                    ContactPhone = table.Column<string>(
                        type: "nvarchar(20)",
                        maxLength: 20,
                        nullable: true),

                    PrivacyPolicyText = table.Column<string>(
                        type: "nvarchar(150)",
                        maxLength: 150,
                        nullable: false),

                    TermsText = table.Column<string>(
                        type: "nvarchar(150)",
                        maxLength: 150,
                        nullable: false),

                    FaqText = table.Column<string>(
                        type: "nvarchar(150)",
                        maxLength: 150,
                        nullable: false),

                    CopyrightText = table.Column<string>(
                        type: "nvarchar(200)",
                        maxLength: 200,
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_HomePageContents",
                        x => x.Id);
                });

            // Create master specialties table
            migrationBuilder.CreateTable(
                name: "Specialties",
                columns: table => new
                {
                    Id = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Name = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false),

                    NameEn = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false),

                    Description = table.Column<string>(
                        type: "nvarchar(500)",
                        maxLength: 500,
                        nullable: true),

                    IsActive = table.Column<bool>(
                        type: "bit",
                        nullable: false,
                        defaultValue: true),

                    SortOrder = table.Column<int>(
                        type: "int",
                        nullable: false),

                    CreatedAtUtc = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false,
                        defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_Specialties",
                        x => x.Id);
                });

            // Unique indexes for specialties
            migrationBuilder.CreateIndex(
                name: "IX_Specialties_Name",
                table: "Specialties",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Specialties_NameEn",
                table: "Specialties",
                column: "NameEn",
                unique: true);

            // Insert initial specialties
            migrationBuilder.Sql(@"
                INSERT INTO [Specialties]
                    ([Name], [NameEn], [Description],
                     [IsActive], [SortOrder], [CreatedAtUtc])
                VALUES
                    (
                        N'كهربائي',
                        N'Electrician',
                        N'خدمات الكهرباء المنزلية',
                        1, 1, SYSUTCDATETIME()
                    ),
                    (
                        N'سباك',
                        N'Plumber',
                        N'خدمات السباكة',
                        1, 2, SYSUTCDATETIME()
                    ),
                    (
                        N'فني تكييف',
                        N'Air Conditioning Technician',
                        N'تركيب وصيانة التكييف',
                        1, 3, SYSUTCDATETIME()
                    ),
                    (
                        N'صيانة أجهزة منزلية',
                        N'Home Appliances Repair',
                        N'إصلاح وصيانة الأجهزة المنزلية',
                        1, 4, SYSUTCDATETIME()
                    );
            ");

            // Insert initial homepage content
            migrationBuilder.Sql(@"
                INSERT INTO [HomePageContents]
                (
                    [PlatformName],
                    [HeroTitle],
                    [HeroDescription],
                    [AboutTitle],
                    [AboutDescription],
                    [WhyUsTitle],
                    [WhyUsDescription],
                    [HowItWorksTitle],
                    [Step1Title],
                    [Step2Title],
                    [Step3Title],
                    [ContactEmail],
                    [ContactPhone],
                    [PrivacyPolicyText],
                    [TermsText],
                    [FaqText],
                    [CopyrightText]
                )
                VALUES
                (
                    N'Sanay3y',
                    N'كل خدمات الصيانة في مكان واحد',
                    N'اعثر على الفني المناسب لاحتياجات منزلك بسهولة.',
                    N'من نحن',
                    N'صنايعي منصة تهدف إلى تسهيل الوصول إلى خدمات الصيانة والفنيين المناسبين.',
                    N'لماذا صنايعي؟',
                    N'نسعى لتسهيل طلب خدمات الصيانة من فنيين موثوقين.',
                    N'كيف يعمل صنايعي؟',
                    N'اختر نوع الخدمة المطلوبة',
                    N'حدد العنوان والموعد المناسب',
                    N'تابع طلبك حتى انتهاء الخدمة',
                    NULL,
                    NULL,
                    N'سياسة الخصوصية',
                    N'الشروط والأحكام',
                    N'الأسئلة الشائعة',
                    N'Sanay3y'
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HomePageContents");

            migrationBuilder.DropTable(
                name: "Specialties");
        }
    }
}
