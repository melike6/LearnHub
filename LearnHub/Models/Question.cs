using Microsoft.CodeAnalysis.Options;
using System.ComponentModel.DataAnnotations;

namespace LearnHub.Models
{
    public class Question
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Soru metni zorunludur.")]
        [Display(Name = "Soru")]
        public string Text { get; set; } = string.Empty;

        [Display(Name = "Sıra")]
        public int Order { get; set; }

        public int QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;

        public ICollection<Option> Options { get; set; } = new List<Option>();
    }
}