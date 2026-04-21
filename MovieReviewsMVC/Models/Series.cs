using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieReviewsMVC.Models
{
    [Table("Series")]
    public class Series
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Название обязательно")]
        [Display(Name = "Название")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Название (англ.)")]
        public string? TitleEn { get; set; }

        [Display(Name = "Краткое описание")]
        public string? ShortDescription { get; set; }

        [Display(Name = "Полное описание")]
        public string? FullDescription { get; set; }

        [Required(ErrorMessage = "Год начала обязателен")]
        [Display(Name = "Год начала")]
        [Range(1900, 2026, ErrorMessage = "Год должен быть от 1900 до 2026")]
        public int YearStart { get; set; }

        [Display(Name = "Год окончания")]
        public int? YearEnd { get; set; }

        [Display(Name = "Жанр")]
        public int? GenreId { get; set; }

        [Display(Name = "Язык")]
        public int? LanguageId { get; set; }

        [Display(Name = "Рейтинг")]
        [Range(0, 10, ErrorMessage = "Рейтинг должен быть от 0 до 10")]
        public double Rating { get; set; }

        [Display(Name = "В ролях")]
        public string? Cast { get; set; }

        // Хранение изображения в BLOB
        [Display(Name = "Постер")]
        public byte[]? PosterImage { get; set; }

        [Display(Name = "Тип изображения")]
        public string? PosterContentType { get; set; }

        [ForeignKey("GenreId")]
        public virtual Genre? Genre { get; set; }

        [ForeignKey("LanguageId")]
        public virtual Language? Language { get; set; }
    }
}