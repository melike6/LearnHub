namespace LearnHub.Models
{
    public class Enrollment
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}