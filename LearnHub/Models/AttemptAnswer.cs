namespace LearnHub.Models
{
    public class AttemptAnswer
    {
        public int Id { get; set; }
        public int QuizAttemptId { get; set; }
        public QuizAttempt QuizAttempt { get; set; } = null!;
        public int QuestionId { get; set; }
        public Question Question { get; set; } = null!;
        public int SelectedOptionId { get; set; }
        public Option SelectedOption { get; set; } = null!;
        public bool IsCorrect { get; set; }
    }
}