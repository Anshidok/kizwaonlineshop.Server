using Microsoft.AspNetCore.Mvc;
using kizwaonlineshop.Server.Model;
using Microsoft.EntityFrameworkCore;
using kizwaonlineshop.Server.Data;
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using kizwaonlineshop.Server.Services;
using Npgsql;
using Microsoft.AspNetCore.Cors;

namespace kizwaonlineshop.Server.Controllers
{

    [ApiController]
    [Route("api/user")]
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
        public async Task<IActionResult> Login([FromBody] User model)
        {
            //using (var connection = new NpgsqlConnection(_connectionString))
            //{
            //    connection.Open();
            //    var query = "SELECT \"Password\", \"UserType\" FROM \"user\" WHERE \"Username\" = @Username";
            //    using (var command = new NpgsqlCommand(query, connection))
            //    {
            //        command.Parameters.AddWithValue("@Username", model.Username);
            //        var reader = command.ExecuteReader();
            //        if (reader.Read())
            //        {
            //            string password = reader.GetString(0);
            //            string userType = reader.GetString(1);
            //            if (password == null)
            //            {
            //                return Unauthorized(new { Message = "Invalid username or password" });
            //            }
            //            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, password);
            //            var token = _authService.GenerateJwtToken(model.Username, userType);
            //            if (isPasswordValid)
            //            {
            //                return Ok(new {
            //                    Message = "Login successful",
            //                    UserType = userType,
            //                    Token = token
            //                }); ;
            //            }
            //        }
            //    }
            //}
            //return Unauthorized(new { Message = "Invalid username or password" });

            if (model == null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest(new { Message = "Username and password are required" });
            }
            var user = await _context.user
                .Where(u => u.Username == model.Username)
                .FirstOrDefaultAsync();
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid username or password" });
            }
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);
            if (!isPasswordValid)
            {
                return Unauthorized(new { Message = "Invalid username or password" });
            }
            var token = _authService.GenerateJwtToken(model.Username, user.UserType);
            return Ok(new { IsSuccess = true, Message = "Login successful", UserType = user.UserType, Token = token });
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

        [HttpGet("getUserDet/{userId}")]
        public async Task<IActionResult> getUserDet(string userId)
        {
            var userDet = await _context.user.
                Where(u => u.Username == userId).
                FirstOrDefaultAsync();
            if(userDet == null)
            {
                return NotFound(new { Messge = "User details not fetch" });
            }
            return Ok(userDet);
        }

    }

}
