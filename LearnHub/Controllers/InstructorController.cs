using LearnHub.Data;
using LearnHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearnHub.Controllers
{
    [Authorize(Roles = "Instructor,Admin")]
    public class InstructorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InstructorController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // ── PANEL ────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var courses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Lessons)
                .Where(c => c.InstructorId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View(courses);
        }

        // ── KURS OLUŞTUR ─────────────────────────────────────

        public async Task<IActionResult> CreateCourse()
        {
            await LoadCategories();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(Course course, IFormFile? coverImage)
        {
            ModelState.Remove("InstructorId");
            ModelState.Remove("Instructor");
            ModelState.Remove("Category");

            if (!ModelState.IsValid)
            {
                await LoadCategories();
                return View(course);
            }

            course.InstructorId = GetUserId();
            course.CreatedAt = DateTime.UtcNow;
            course.Status = CourseStatus.Pending;

            if (coverImage != null && coverImage.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(coverImage.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(),
                    "wwwroot", "uploads", "covers");
                Directory.CreateDirectory(uploadPath);
                var filePath = Path.Combine(uploadPath, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await coverImage.CopyToAsync(stream);
                course.CoverImage = "/uploads/covers/" + fileName;
            }

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Kurs oluşturuldu. Admin onayı bekleniyor.";
            return RedirectToAction(nameof(Index));
        }

        // ── KURS DÜZENLE ─────────────────────────────────────

        public async Task<IActionResult> EditCourse(int id)
        {
            var userId = GetUserId();
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id && c.InstructorId == userId);
            if (course == null)
                return NotFound();

            await LoadCategories(course.CategoryId);
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(int id, Course course, IFormFile? coverImage)
        {
            if (id != course.Id)
                return NotFound();

            ModelState.Remove("InstructorId");
            ModelState.Remove("Instructor");
            ModelState.Remove("Category");

            if (!ModelState.IsValid)
            {
                await LoadCategories(course.CategoryId);
                return View(course);
            }

            var existing = await _context.Courses.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Title = course.Title;
            existing.Description = course.Description;
            existing.CategoryId = course.CategoryId;

            if (coverImage != null && coverImage.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(coverImage.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(),
                    "wwwroot", "uploads", "covers");
                Directory.CreateDirectory(uploadPath);
                var filePath = Path.Combine(uploadPath, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await coverImage.CopyToAsync(stream);
                existing.CoverImage = "/uploads/covers/" + fileName;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Kurs güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // ── DERSLER ──────────────────────────────────────────

        public async Task<IActionResult> Lessons(int courseId)
        {
            var userId = GetUserId();
            var course = await _context.Courses
                .Include(c => c.Lessons.OrderBy(l => l.Order))
                .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == userId);

            if (course == null)
                return NotFound();

            ViewBag.Course = course;
            return View(course.Lessons.ToList());
        }

        public async Task<IActionResult> CreateLesson(int courseId)
        {
            var userId = GetUserId();
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == userId);
            if (course == null)
                return NotFound();

            ViewBag.CourseId = courseId;
            ViewBag.CourseName = course.Title;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLesson(Lesson lesson, IFormFile? pdfFile)
        {
            ModelState.Remove("Course");

            if (!ModelState.IsValid)
            {
                ViewBag.CourseId = lesson.CourseId;
                return View(lesson);
            }

            if (pdfFile != null && pdfFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(pdfFile.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(),
                    "wwwroot", "uploads", "pdfs");
                Directory.CreateDirectory(uploadPath);
                var filePath = Path.Combine(uploadPath, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await pdfFile.CopyToAsync(stream);
                lesson.FileUrl = "/uploads/pdfs/" + fileName;
            }

            var lastOrder = await _context.Lessons
                .Where(l => l.CourseId == lesson.CourseId)
                .MaxAsync(l => (int?)l.Order) ?? 0;
            lesson.Order = lastOrder + 1;
            lesson.CreatedAt = DateTime.UtcNow;

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Ders eklendi.";
            return RedirectToAction(nameof(Lessons), new { courseId = lesson.CourseId });
        }

        public async Task<IActionResult> EditLesson(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.Id == id);
            if (lesson == null)
                return NotFound();

            ViewBag.CourseId = lesson.CourseId;
            ViewBag.CourseName = lesson.Course.Title;
            return View(lesson);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLesson(int id, Lesson lesson, IFormFile? pdfFile)
        {
            if (id != lesson.Id)
                return NotFound();

            ModelState.Remove("Course");

            if (!ModelState.IsValid)
            {
                ViewBag.CourseId = lesson.CourseId;
                return View(lesson);
            }

            var existing = await _context.Lessons.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Title = lesson.Title;
            existing.Type = lesson.Type;
            existing.VideoUrl = lesson.VideoUrl;
            existing.TextContent = lesson.TextContent;

            if (pdfFile != null && pdfFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(pdfFile.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(),
                    "wwwroot", "uploads", "pdfs");
                Directory.CreateDirectory(uploadPath);
                var filePath = Path.Combine(uploadPath, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await pdfFile.CopyToAsync(stream);
                existing.FileUrl = "/uploads/pdfs/" + fileName;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Ders güncellendi.";
            return RedirectToAction(nameof(Lessons), new { courseId = existing.CourseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
                return NotFound();

            var courseId = lesson.CourseId;
            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Ders silindi.";
            return RedirectToAction(nameof(Lessons), new { courseId });
        }

        private async Task LoadCategories(int? selectedId = null)
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedId);
        }
    }
}