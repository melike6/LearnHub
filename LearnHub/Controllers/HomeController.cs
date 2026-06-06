using LearnHub.Data;
using LearnHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var featuredCourses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Where(c => c.Status == CourseStatus.Approved && c.IsActive)
                .OrderByDescending(c => c.CreatedAt)
                .Take(6)
                .ToListAsync();

            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .ToListAsync();

            ViewBag.FeaturedCourses = featuredCourses;
            ViewBag.Categories = categories;

            return View();
        }

        public async Task<IActionResult> Search(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return RedirectToAction("Index", "Course");

            var courses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Where(c => c.Status == CourseStatus.Approved &&
                            c.IsActive &&
                            (c.Title.Contains(q) ||
                             c.Description!.Contains(q) ||
                             c.Instructor.FullName.Contains(q) ||
                             c.Category.Name.Contains(q)))
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Query = q;
            ViewBag.ResultCount = courses.Count;
            return View(courses);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}