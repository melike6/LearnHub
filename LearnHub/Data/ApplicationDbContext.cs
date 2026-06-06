using LearnHub.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LearnHub.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<LessonProgress> LessonProgresses { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<AttemptAnswer> AttemptAnswers { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Course>()
                .HasOne(c => c.Category)
                .WithMany(cat => cat.Courses)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Course>()
                .HasOne(c => c.Instructor)
                .WithMany()
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Enrollment>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany()
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LessonProgress>()
                .HasOne(lp => lp.User)
                .WithMany()
                .HasForeignKey(lp => lp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LessonProgress>()
                .HasOne(lp => lp.Lesson)
                .WithMany()
                .HasForeignKey(lp => lp.LessonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Enrollment>()
                .HasIndex(e => new { e.UserId, e.CourseId })
                .IsUnique();

            builder.Entity<LessonProgress>()
                .HasIndex(lp => new { lp.UserId, lp.LessonId })
                .IsUnique();

            builder.Entity<QuizAttempt>()
                .HasOne(qa => qa.User)
                .WithMany()
                .HasForeignKey(qa => qa.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<QuizAttempt>()
                .HasOne(qa => qa.Quiz)
                .WithMany(q => q.Attempts)
                .HasForeignKey(qa => qa.QuizId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AttemptAnswer>()
                .HasOne(aa => aa.QuizAttempt)
                .WithMany(qa => qa.Answers)
                .HasForeignKey(aa => aa.QuizAttemptId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AttemptAnswer>()
                .HasOne(aa => aa.Question)
                .WithMany()
                .HasForeignKey(aa => aa.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AttemptAnswer>()
                .HasOne(aa => aa.SelectedOption)
                .WithMany()
                .HasForeignKey(aa => aa.SelectedOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>()
                .HasOne(r => r.Course)
                .WithMany()
                .HasForeignKey(r => r.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Bir kullanıcı bir kursa sadece bir yorum yapabilsin
            builder.Entity<Review>()
                .HasIndex(r => new { r.UserId, r.CourseId })
                .IsUnique();
        }
    }
}