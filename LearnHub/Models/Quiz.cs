using System.ComponentModel.DataAnnotations;

namespace LearnHub.Models
{
    public class Quiz
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Quiz adı zorunludur.")]
        [StringLength(200)]
        [Display(Name = "Quiz Adı")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Display(Name = "Geçme Puanı")]
        public int PassingScore { get; set; } = 70;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
    }
}