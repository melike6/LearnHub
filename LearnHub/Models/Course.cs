namespace LearnHub.Models
{
    public enum CourseStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CoverImage { get; set; }
        public CourseStatus Status { get; set; } = CourseStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public string InstructorId { get; set; } = string.Empty;
        public ApplicationUser Instructor { get; set; } = null!;

        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}