namespace LearnHub.Models
{
    public class Certificate
    {
        public int Id { get; set; }
        public string CertificateCode { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
    }
}