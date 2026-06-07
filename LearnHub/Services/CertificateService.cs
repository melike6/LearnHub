using LearnHub.Data;
using LearnHub.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LearnHub.Services
{
    public class CertificateService
    {
        private readonly ApplicationDbContext _context;

        public CertificateService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Sertifika almaya hak kazandı mı kontrol et
        public async Task<bool> IsEligibleAsync(string userId, int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Lessons.Where(l => l.IsActive))
                .Include(c => c.Quizzes)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null) return false;

            // Tüm dersler tamamlanmış mı
            if (course.Lessons.Any())
            {
                var completedCount = await _context.LessonProgresses
                    .CountAsync(lp => lp.UserId == userId &&
                                      lp.Lesson.CourseId == courseId &&
                                      lp.IsCompleted);

                if (completedCount < course.Lessons.Count)
                    return false;
            }

            // Quiz varsa en az birini geçmiş mi
            if (course.Quizzes.Any())
            {
                var hasPassed = await _context.QuizAttempts
                    .AnyAsync(qa => qa.UserId == userId &&
                                    qa.Quiz.CourseId == courseId &&
                                    qa.IsPassed);

                if (!hasPassed) return false;
            }

            return true;
        }

        // Sertifika oluştur veya mevcutu getir
        public async Task<Certificate> GetOrCreateAsync(string userId, int courseId)
        {
            var existing = await _context.Certificates
                .Include(c => c.User)
                .Include(c => c.Course)
                    .ThenInclude(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.UserId == userId &&
                                          c.CourseId == courseId);

            if (existing != null) return existing;

            var certificate = new Certificate
            {
                UserId = userId,
                CourseId = courseId,
                CertificateCode = GenerateCode(),
                IssuedAt = DateTime.UtcNow
            };

            _context.Certificates.Add(certificate);
            await _context.SaveChangesAsync();

            // İlişkileri yükle
            return await _context.Certificates
                .Include(c => c.User)
                .Include(c => c.Course)
                    .ThenInclude(c => c.Instructor)
                .FirstAsync(c => c.Id == certificate.Id);
        }

        // PDF oluştur
        public byte[] GeneratePdf(Certificate certificate)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(0);
                    page.DefaultTextStyle(x => x.FontFamily("Arial"));

                    page.Content().Column(col =>
                    {
                        col.Item().Height(PageSizes.A4.Landscape().Height)
                            .Background("#1a1a2e")
                            .Padding(40)
                            .Column(inner =>
                            {
                                // Üst dekoratif çizgi
                                inner.Item()
                                    .Height(6)
                                    .Background("#e94560");

                                inner.Item().Height(20);

                                // Başlık
                                inner.Item()
                                    .AlignCenter()
                                    .Text("LearnHub")
                                    .FontSize(42)
                                    .FontColor("#e94560")
                                    .Bold();

                                inner.Item().Height(10);

                                inner.Item()
                                    .AlignCenter()
                                    .Text("BAŞARI SERTİFİKASI")
                                    .FontSize(18)
                                    .FontColor("#ffffff")
                                    .LetterSpacing(0.15f);

                                inner.Item().Height(30);

                                // Dekoratif çizgi
                                inner.Item()
                                    .AlignCenter()
                                    .Width(300)
                                    .Height(1)
                                    .Background("#e94560");

                                inner.Item().Height(30);

                                // Bu belge onaylar
                                inner.Item()
                                    .AlignCenter()
                                    .Text("Bu belge,")
                                    .FontSize(14)
                                    .FontColor("#a8a8b3");

                                inner.Item().Height(16);

                                // Öğrenci adı
                                inner.Item()
                                    .AlignCenter()
                                    .Text(certificate.User.FullName)
                                    .FontSize(36)
                                    .FontColor("#ffffff")
                                    .Bold();

                                inner.Item().Height(16);

                                inner.Item()
                                    .AlignCenter()
                                    .Text("adlı katılımcının")
                                    .FontSize(14)
                                    .FontColor("#a8a8b3");

                                inner.Item().Height(16);

                                // Kurs adı
                                inner.Item()
                                    .AlignCenter()
                                    .Text(certificate.Course.Title)
                                    .FontSize(28)
                                    .FontColor("#e94560")
                                    .Bold();

                                inner.Item().Height(16);

                                inner.Item()
                                    .AlignCenter()
                                    .Text("kursunu başarıyla tamamladığını onaylar.")
                                    .FontSize(14)
                                    .FontColor("#a8a8b3");

                                inner.Item().Height(30);

                                // Dekoratif çizgi
                                inner.Item()
                                    .AlignCenter()
                                    .Width(300)
                                    .Height(1)
                                    .Background("#e94560");

                                inner.Item().Height(20);

                                // Alt bilgiler
                                inner.Item()
                                    .Row(row =>
                                    {
                                        row.RelativeItem()
                                            .AlignLeft()
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .Text("Eğitmen")
                                                    .FontSize(11)
                                                    .FontColor("#a8a8b3");
                                                c.Item()
                                                    .Text(certificate.Course.Instructor.FullName)
                                                    .FontSize(14)
                                                    .FontColor("#ffffff")
                                                    .Bold();
                                            });

                                        row.RelativeItem()
                                            .AlignCenter()
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .Text("Tarih")
                                                    .FontSize(11)
                                                    .FontColor("#a8a8b3");
                                                c.Item()
                                                    .Text(certificate.IssuedAt
                                                        .ToString("dd MMMM yyyy"))
                                                    .FontSize(14)
                                                    .FontColor("#ffffff")
                                                    .Bold();
                                            });

                                        row.RelativeItem()
                                            .AlignRight()
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .Text("Sertifika No")
                                                    .FontSize(11)
                                                    .FontColor("#a8a8b3");
                                                c.Item()
                                                    .Text(certificate.CertificateCode)
                                                    .FontSize(12)
                                                    .FontColor("#e94560")
                                                    .Bold();
                                            });
                                    });

                                inner.Item().Height(20);

                                // Alt dekoratif çizgi
                                inner.Item()
                                    .Height(6)
                                    .Background("#e94560");
                            });
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static string GenerateCode()
        {
            return "LH-" + Guid.NewGuid().ToString("N")[..8].ToUpper();
        }
    }
}