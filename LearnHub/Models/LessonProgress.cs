namespace LearnHub.Models
{
    public class LessonProgress
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public int LessonId { get; set; }
        public Lesson Lesson { get; set; } = null!;
        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedAt { get; set; }
    }
}