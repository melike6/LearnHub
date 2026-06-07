using LearnHub.Data;
using LearnHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearnHub.Controllers
{
    [Authorize]
    public class CertificateController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CertificateService _certificateService;

        public CertificateController(ApplicationDbContext context,
                                     CertificateService certificateService)
        {
            _context = context;
            _certificateService = certificateService;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Sertifika sayfası — almaya hak kazandıysa göster
        public async Task<IActionResult> Index(int courseId)
        {
            var userId = GetUserId();

            var isEligible = await _certificateService
                .IsEligibleAsync(userId, courseId);

            if (!isEligible)
            {
                TempData["Error"] = "Sertifika alabilmek için tüm dersleri " +
                                    "tamamlayıp quiz'i geçmeniz gerekiyor.";
                return RedirectToAction("Detail", "Course", new { id = courseId });
            }

            var certificate = await _certificateService
                .GetOrCreateAsync(userId, courseId);

            return View(certificate);
        }

        // PDF indir
        public async Task<IActionResult> Download(int courseId)
        {
            var userId = GetUserId();

            var isEligible = await _certificateService
                .IsEligibleAsync(userId, courseId);

            if (!isEligible)
                return Forbid();

            var certificate = await _certificateService
                .GetOrCreateAsync(userId, courseId);

            var pdfBytes = _certificateService.GeneratePdf(certificate);

            var fileName = $"LearnHub_Sertifika_{certificate.CertificateCode}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        // Sertifika doğrulama — giriş yapmadan erişilebilir
        [AllowAnonymous]
        public async Task<IActionResult> Verify(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return View(null as LearnHub.Models.Certificate);

            var certificate = await _context.Certificates
                .Include(c => c.User)
                .Include(c => c.Course)
                    .ThenInclude(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.CertificateCode == code);

            return View(certificate);
        }

        // Tüm sertifikalarım
        public async Task<IActionResult> MyCertificates()
        {
            var userId = GetUserId();

            var certificates = await _context.Certificates
                .Include(c => c.Course)
                    .ThenInclude(c => c.Category)
                .Include(c => c.Course)
                    .ThenInclude(c => c.Instructor)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.IssuedAt)
                .ToListAsync();

            return View(certificates);
        }
    }
}