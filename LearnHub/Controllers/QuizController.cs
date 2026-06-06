using LearnHub.Data;
using LearnHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearnHub.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuizController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public async Task<IActionResult> Take(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.Id == id && q.IsActive);

            if (quiz == null) return NotFound();

            // Kursa kayıtlı mı kontrol et
            var userId = GetUserId();
            var isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == quiz.CourseId);

            if (!isEnrolled)
            {
                TempData["Error"] = "Quiz çözmek için kursa kayıtlı olmalısınız.";
                return RedirectToAction("Detail", "Course", new { id = quiz.CourseId });
            }

            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int quizId,
            Dictionary<int, int> answers)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null) return NotFound();

            var userId = GetUserId();
            int correctCount = 0;
            var attemptAnswers = new List<AttemptAnswer>();

            foreach (var question in quiz.Questions)
            {
                if (!answers.TryGetValue(question.Id, out int selectedOptionId))
                    continue;

                var selectedOption = question.Options
                    .FirstOrDefault(o => o.Id == selectedOptionId);

                bool isCorrect = selectedOption?.IsCorrect ?? false;
                if (isCorrect) correctCount++;

                attemptAnswers.Add(new AttemptAnswer
                {
                    QuestionId = question.Id,
                    SelectedOptionId = selectedOptionId,
                    IsCorrect = isCorrect
                });
            }

            int score = quiz.Questions.Count > 0
                ? (int)((double)correctCount / quiz.Questions.Count * 100)
                : 0;

            bool isPassed = score >= quiz.PassingScore;

            var attempt = new QuizAttempt
            {
                UserId = userId,
                QuizId = quizId,
                Score = score,
                IsPassed = isPassed,
                AttemptedAt = DateTime.UtcNow,
                Answers = attemptAnswers
            };

            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Result), new { id = attempt.Id });
        }

        public async Task<IActionResult> Result(int id)
        {
            var attempt = await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(q => q.Options)
                .Include(qa => qa.Answers)
                    .ThenInclude(aa => aa.SelectedOption)
                .Include(qa => qa.Answers)
                    .ThenInclude(aa => aa.Question)
                        .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(qa => qa.Id == id && qa.UserId == GetUserId());

            if (attempt == null) return NotFound();

            return View(attempt);
        }
    }
}