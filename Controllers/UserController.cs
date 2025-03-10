using Microsoft.AspNetCore.Mvc;
using kizwaonlineshop.Server.Model;
using Microsoft.EntityFrameworkCore;
using kizwaonlineshop.Server.Data;
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using kizwaonlineshop.Server.Services;
using Npgsql;

namespace kizwaonlineshop.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly kizwacartContext _context;
        private readonly string? _connectionString;
        private readonly AuthService _authService;
        public UserController(IConfiguration configuration, kizwacartContext context, AuthService authService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _context = context;
            _authService = authService;
        }

        [HttpPost("loginuser")]
        public IActionResult Login([FromBody] User model)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT \"Password\", \"UserType\" FROM \"user\" WHERE \"Username\" = @Username";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", model.Username);
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        string password = reader.GetString(0);
                        string userType = reader.GetString(1);
                        if (password == null)
                        {
                            return Unauthorized(new { Message = "Invalid username or password" });
                        }
                        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, password);
                        var token = _authService.GenerateJwtToken(model.Username, userType);
                        if (isPasswordValid)
                        {
                            return Ok(new {
                                Message = "Login successful",
                                UserType = userType,
                                Token = token
                            }); ;
                        }
                    }
                }
            }
            return Unauthorized(new { Message = "Invalid username or password" });
        }

        [HttpPost("signupuser")]
        public async Task<IActionResult> signup([FromBody] User model)
        {
            if (await _context.user.AnyAsync(u => u.Username == model.Username))
            {
                return BadRequest(new UserResponse { IsSuccess = false, Message = "Username is alredy exist." });
            }
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };
            user.UserType = string.IsNullOrEmpty(model.UserType) ? "User" : model.UserType;
            _context.user.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new UserResponse { IsSuccess = true, Message = "User registered successfully." });
        }

        [HttpPost("userexist")]
        public async Task<IActionResult> userexist([FromBody] UserRequest model)
        {
            if (await _context.user.AnyAsync(u => u.Username == model.Username))
            {
                return BadRequest(new UserResponse { IsSuccess = false, Message = "Username is already exist." });
            }
            return Ok(new UserResponse { IsSuccess = true, Message = "Username is available." });

        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.user.ToListAsync();
            return Ok(users);
        }

        [HttpDelete("deleteuser/{username}")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            var user = await _context.user.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            _context.user.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User deleted successfully" });
        }

    }

}
