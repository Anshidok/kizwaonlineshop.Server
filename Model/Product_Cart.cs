using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kizwaonlineshop.Server.Model
{
    public class Product_Cart
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Ensures auto-increment in PostgreSQL
        public int Id { get; set; }

        [Required]
        public string userId { get; set; } = string.Empty;
        public int productId { get; set; }
        public string productName { get; set; } = string.Empty;
        public string category { get; set; } = string.Empty;
        public string? size { get; set; } = string.Empty;
        public string? image { get; set; }

        [Required]
        [Column(TypeName = "timestamp without time zone")] // PostgreSQL default for DateTime
        public DateTime addedDate { get; set; }

    }
}
