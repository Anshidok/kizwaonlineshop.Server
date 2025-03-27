using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kizwaonlineshop.Server.Model
{
    public class Products_master
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Ensures auto-increment for PostgreSQL
        public int Id { get; set; }

        [Required]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        public string? Size { get; set; } = string.Empty;

        [Column(TypeName = "numeric(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal Stocks { get; set; }

        public string? Image { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }
}
