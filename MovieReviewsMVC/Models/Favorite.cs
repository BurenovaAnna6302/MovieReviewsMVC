using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieReviewsMVC.Models
{
    [Table("Favorites")]
    public class Favorite
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SeriesId { get; set; }

        [Required]
        public string UserId { get; set; } = "default_user";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационное свойство
        [ForeignKey("SeriesId")]
        public virtual Series? Series { get; set; }
    }
}