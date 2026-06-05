using System.ComponentModel.DataAnnotations;

namespace LearnHub.Models
{
    public enum LessonType
    {
        Video,
        Text,
        PDF
    }

    public class Lesson
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ders adı zorunludur.")]
        [StringLength(200, ErrorMessage = "Ders adı en fazla 200 karakter olabilir.")]
        [Display(Name = "Ders Adı")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "İçerik Tipi")]
        public LessonType Type { get; set; } = LessonType.Video;

        [Display(Name = "Video URL")]
        public string? VideoUrl { get; set; }

        [Display(Name = "Metin İçerik")]
        public string? TextContent { get; set; }

        [Display(Name = "Dosya Yolu")]
        public string? FileUrl { get; set; }

        [Display(Name = "Sıra")]
        public int Order { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
    }
}