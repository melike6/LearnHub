using System.ComponentModel.DataAnnotations;

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

        [Required(ErrorMessage = "Kurs adı zorunludur.")]
        [StringLength(200, ErrorMessage = "Kurs adı en fazla 200 karakter olabilir.")]
        [Display(Name = "Kurs Adı")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Display(Name = "Kapak Görseli")]
        public string? CoverImage { get; set; }

        public CourseStatus Status { get; set; } = CourseStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Kategori seçiniz.")]
        [Display(Name = "Kategori")]
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public string InstructorId { get; set; } = string.Empty;
        public ApplicationUser Instructor { get; set; } = null!;

        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}