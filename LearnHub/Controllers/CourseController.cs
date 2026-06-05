using LearnHub.Data;
using LearnHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnHub.Controllers
{
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

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
                .FirstOrDefaultAsync(c => c.Id == id && c.Status == CourseStatus.Approved);

            if (course == null)
                return NotFound();

            return View(course);
        }

        public async Task<IActionResult> Lesson(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Course)
                    .ThenInclude(c => c.Lessons.Where(l => l.IsActive).OrderBy(l => l.Order))
                .FirstOrDefaultAsync(l => l.Id == id && l.IsActive);

            if (lesson == null)
                return NotFound();

            return View(lesson);
        }
    }
}