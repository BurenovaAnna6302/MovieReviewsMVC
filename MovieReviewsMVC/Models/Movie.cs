using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieReviewsMVC.Models
{
    [Table("Movies")]
    public class Movie
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название фильма")]
        [Display(Name = "Название")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Жанр")]
        public string Genre { get; set; } = string.Empty;

        [Display(Name = "Год выпуска")]
        [Range(1900, 2026, ErrorMessage = "Год должен быть от 1900 до 2026")]
        public int Year { get; set; }

        [Display(Name = "Рейтинг")]
        [Range(0, 10, ErrorMessage = "Рейтинг от 0 до 10")]
        public double Rating { get; set; }

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Display(Name = "Режиссер")]
        public string? Director { get; set; }
    }
}