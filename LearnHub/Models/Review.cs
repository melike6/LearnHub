using System.ComponentModel.DataAnnotations;

namespace LearnHub.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Range(1, 5, ErrorMessage = "Puan 1 ile 5 arasında olmalıdır.")]
        [Display(Name = "Puan")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Yorum zorunludur.")]
        [StringLength(1000, ErrorMessage = "Yorum en fazla 1000 karakter olabilir.")]
        [Display(Name = "Yorum")]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
    }
}