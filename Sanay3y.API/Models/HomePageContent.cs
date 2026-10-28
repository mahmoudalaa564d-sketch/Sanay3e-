
namespace Sanay3y.API.Models
{
    public class HomePageContent
    {
        public int Id { get; set; }

        public string PlatformName { get; set; } = "Sanay3y";

        public string HeroTitle { get; set; } = string.Empty;

        public string HeroDescription { get; set; } = string.Empty;

        public string AboutTitle { get; set; } = string.Empty;

        public string AboutDescription { get; set; } = string.Empty;

        public string WhyUsTitle { get; set; } = string.Empty;

        public string WhyUsDescription { get; set; } = string.Empty;

        public string HowItWorksTitle { get; set; } = string.Empty;

        public string Step1Title { get; set; } = string.Empty;

        public string Step2Title { get; set; } = string.Empty;

        public string Step3Title { get; set; } = string.Empty;

        public string? ContactEmail { get; set; }

        public string? ContactPhone { get; set; }

        public string PrivacyPolicyText { get; set; } = "سياسة الخصوصية";

        public string TermsText { get; set; } = "الشروط والأحكام";

        public string FaqText { get; set; } = "الأسئلة الشائعة";

        public string CopyrightText { get; set; } = "Sanay3y";
    }
}
