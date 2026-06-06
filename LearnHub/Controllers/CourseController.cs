using LearnHub.Data;
using LearnHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearnHub.Controllers
{
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string? GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        public async Task<IActionResult> Index(int? categoryId, string? search)
        {
            var query = _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Where(c => c.Status == CourseStatus.Approved && c.IsActive);

            if (categoryId.HasValue)
                query = query.Where(c => c.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Title.Contains(search) ||
                                         c.Description!.Contains(search));

            var courses = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
            var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.Search = search;

            return View(courses);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Lessons.Where(l => l.IsActive).OrderBy(l => l.Order))
                .Include(c => c.Quizzes.Where(q => q.IsActive))
                .FirstOrDefaultAsync(c => c.Id == id && c.Status == CourseStatus.Approved);

            if (course == null)
                return NotFound();

            var userId = GetUserId();
            bool isEnrolled = false;
            int progressPercent = 0;

            if (userId != null)
            {
                isEnrolled = await _context.Enrollments
                    .AnyAsync(e => e.UserId == userId && e.CourseId == id);

                if (isEnrolled && course.Lessons.Any())
                {
                    var completedCount = await _context.LessonProgresses
                        .CountAsync(lp => lp.UserId == userId &&
                                          lp.Lesson.CourseId == id &&
                                          lp.IsCompleted);
                    progressPercent = (int)((double)completedCount / course.Lessons.Count * 100);
                }
            }

            ViewBag.IsEnrolled = isEnrolled;
            ViewBag.ProgressPercent = progressPercent;

            return View(course);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var userId = GetUserId()!;

            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (!alreadyEnrolled)
            {
                _context.Enrollments.Add(new Enrollment
                {
                    UserId = userId,
                    CourseId = courseId,
                    EnrolledAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                TempData["Success"] = "Kursa başarıyla kayıt oldunuz!";
            }

            return RedirectToAction(nameof(Detail), new { id = courseId });
        }

        [Authorize]
        public async Task<IActionResult> Lesson(int id)
        {
            var userId = GetUserId()!;

            var lesson = await _context.Lessons
                .Include(l => l.Course)
                    .ThenInclude(c => c.Lessons.Where(l => l.IsActive).OrderBy(l => l.Order))
                .FirstOrDefaultAsync(l => l.Id == id && l.IsActive);

            if (lesson == null)
                return NotFound();

            // Kursa kayıtlı mı kontrol et
            var isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == lesson.CourseId);

            if (!isEnrolled)
            {
                TempData["Error"] = "Bu dersi izlemek için önce kursa kayıt olmalısınız.";
                return RedirectToAction(nameof(Detail), new { id = lesson.CourseId });
            }

            // Bu dersi tamamladı mı
            var progress = await _context.LessonProgresses
                .FirstOrDefaultAsync(lp => lp.UserId == userId && lp.LessonId == id);

            ViewBag.IsCompleted = progress?.IsCompleted ?? false;

            return View(lesson);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteLesson(int lessonId)
        {
            var userId = GetUserId()!;

            var progress = await _context.LessonProgresses
                .FirstOrDefaultAsync(lp => lp.UserId == userId && lp.LessonId == lessonId);

            if (progress == null)
            {
                _context.LessonProgresses.Add(new LessonProgress
                {
                    UserId = userId,
                    LessonId = lessonId,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow
                });
            }
            else
            {
                progress.IsCompleted = true;
                progress.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Ders tamamlandı olarak işaretlendi!";

            var lesson = await _context.Lessons.FindAsync(lessonId);
            return RedirectToAction(nameof(Lesson), new { id = lessonId });
        }
    }
}