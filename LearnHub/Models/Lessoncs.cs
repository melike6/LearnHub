namespace LearnHub.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsActive { get; set; } = true;

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
    }
}