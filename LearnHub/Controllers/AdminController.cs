using LearnHub.Data;
using LearnHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnHub.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // ── KATEGORİLER ──────────────────────────────────────

        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View(categories);
        }

        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            category.CreatedAt = DateTime.UtcNow;
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Kategori başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Categories));
        }

        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category category)
        {
            if (id != category.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(category);

            _context.Update(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Kategori güncellendi.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Kategori silindi.";
            return RedirectToAction(nameof(Categories));
        }

        // ── KURSLAR ──────────────────────────────────────────

        public async Task<IActionResult> Courses()
        {
            var courses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View(courses);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            course.Status = CourseStatus.Approved;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Kurs onaylandı.";
            return RedirectToAction(nameof(Courses));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            course.Status = CourseStatus.Rejected;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Kurs reddedildi.";
            return RedirectToAction(nameof(Courses));
        }
    }
}