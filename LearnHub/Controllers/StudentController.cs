using LearnHub.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearnHub.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Lessons.Where(l => l.IsActive))
                .Include(e => e.Course)
                    .ThenInclude(c => c.Category)
                .Where(e => e.UserId == userId && e.IsActive)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();

            var progressData = await _context.LessonProgresses
                .Where(lp => lp.UserId == userId && lp.IsCompleted)
                .ToListAsync();

            var completedLessonIds = progressData.Select(lp => lp.LessonId).ToHashSet();

            ViewBag.CompletedLessonIds = completedLessonIds;
            return View(enrollments);
        }
    }
}