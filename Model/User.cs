using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kizwaonlineshop.Server.Model
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        //[EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        [Required]
        [MinLength(1)] // Ensures password security
        public string Password { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? UserType { get; set; } = "User"; // Default user type
    }

    public class UserRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;
    }

    public class UserResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
