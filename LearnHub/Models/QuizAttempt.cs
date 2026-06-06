namespace LearnHub.Models
{
    public class QuizAttempt
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;
        public int Score { get; set; }
        public bool IsPassed { get; set; }
        public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

        public ICollection<AttemptAnswer> Answers { get; set; } = new List<AttemptAnswer>();
    }
}