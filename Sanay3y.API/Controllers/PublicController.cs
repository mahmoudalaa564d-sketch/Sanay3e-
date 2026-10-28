
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sanay3y.API.Data;

namespace Sanay3y.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublicController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PublicController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Public/home
        [HttpGet("home")]
        [AllowAnonymous]
        public IActionResult GetHomePage()
        {
            var response = new
            {
                platformName = "Sanay3y",

                title = "كل خدمات الصيانة في مكان واحد",

                description =
                    "اعثر على الفني المناسب لاحتياجات منزلك بسهولة.",

                whySanay3y = new[]
                {
                    new
                    {
                        title = "فنيون موثوقون",
                        description =
                            "نسعى لتوفير فنيين يتم التحقق من بياناتهم."
                    },
                    new
                    {
                        title = "سهولة طلب الخدمة",
                        description =
                            "اختر التخصص المناسب وقدم طلبك بسهولة."
                    },
                    new
                    {
                        title = "متابعة الطلبات",
                        description =
                            "تابع حالة طلبات الصيانة من حسابك."
                    }
                },

                howItWorks = new[]
                {
                    "اختر نوع الخدمة المطلوبة",
                    "حدد العنوان والموعد المناسب",
                    "تابع طلبك حتى انتهاء الخدمة"
                },

                about = new
                {
                    title = "من نحن",
                    description =
                        "صنايعي منصة تهدف إلى تسهيل الوصول إلى خدمات الصيانة والفنيين المناسبين."
                },

                contact = new
                {
                    title = "تواصل معنا",
                    email = (string?)null,
                    phone = (string?)null
                },

                footer = new
                {
                    company = new[]
                    {
                        "من نحن",
                        "لماذا صنايعي؟",
                        "تواصل معنا"
                    },

                    help = new[]
                    {
                        "كيف يعمل صنايعي؟",
                        "الأسئلة الشائعة"
                    },

                    legal = new[]
                    {
                        "سياسة الخصوصية",
                        "الشروط والأحكام"
                    },

                    copyright = "Sanay3y"
                }
            };

            return Ok(response);
        }

        // GET: api/Public/specialties
        [HttpGet("specialties")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSpecialties()
        {
            var specialties = await _context.Specialties
                .AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .Select(s => new
                {
                    id = s.Id,
                    name = s.Name,
                    nameEn = s.NameEn,
                    description = s.Description,
                    sortOrder = s.SortOrder
                })
                .ToListAsync();

            return Ok(specialties);
        }
    }
}
