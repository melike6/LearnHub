using System.ComponentModel.DataAnnotations;

namespace LearnHub.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Kategori Adı")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}