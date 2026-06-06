using System.ComponentModel.DataAnnotations;

namespace LearnHub.Models
{
    public class Option
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Seçenek metni zorunludur.")]
        [Display(Name = "Seçenek")]
        public string Text { get; set; } = string.Empty;

        [Display(Name = "Doğru Cevap")]
        public bool IsCorrect { get; set; } = false;

        public int QuestionId { get; set; }
        public Question Question { get; set; } = null!;
    }
}