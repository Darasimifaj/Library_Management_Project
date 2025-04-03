using DocumentFormat.OpenXml.Spreadsheet;
using Library.Data;

using Library.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Controllers
{
    [Route("api/userlogin")]
    [ApiController]
    public class UserLoginController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserLoginController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Student Login
        [HttpPost("student")]
        public async Task<IActionResult> StudentLogin([FromQuery] LoginUserDto dto)
        {
            return await AuthenticateUser(dto, "Student", true);
        }

        // ✅ Lecturer Login
        [HttpPost("lecturer")]
        public async Task<IActionResult> LecturerLogin([FromQuery] LoginUserDto dto)
        {
            return await AuthenticateUser(dto, "Lecturer", true);
        }

        // ✅ Admin Login
        [HttpPost("admin")]
        public async Task<IActionResult> AdminLogin([FromQuery] LoginUserDto dto)
        {
            return await AuthenticateUser(dto, "Admin",true);
        }

        // 🔹 Helper method to authenticate users
        private async Task<IActionResult> AuthenticateUser(LoginUserDto dto, string userType, bool? IsAdmin)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("UserId and Password are required.");
            }

            // Find user
            var user = _context.Users.FirstOrDefault(u => u.UserId == dto.UserId && (u.UserType == userType || u.IsAdmin == IsAdmin));

            if (user == null)
            {
                return Unauthorized($"Invalid {userType} credentials.");
            }

            // ❌ Check if the user is deactivated
            if (!user.IsActive)
            {
                return Unauthorized("Account is deactivated. Please contact the admin.");
            }

            // Verify password
            if (BCrypt.Net.BCrypt.HashPassword(dto.Password) == user.PasswordHash)
            {
                return Unauthorized($"Invalid {userType} credentials.");
            }

            // Session handling
            var sessionDuration = TimeSpan.FromHours(0.5);
            var expiryTime = DateTime.UtcNow.Add(sessionDuration);

            var loginHistory = new UserLoginHistory
            {
                UserId = user.UserId,
                UserType = user.UserType,
                LoginTime = DateTime.UtcNow,
                SessionExpiry = expiryTime,
            };
            user.IsLoggedIn = true;
            _context.Users.Update(user);
            _context.UserLoginHistories.Add(loginHistory);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"{userType} login successful",
                sessionExpiry = expiryTime
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromQuery] string userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId&& u.IsLoggedIn==true);
            var lastLogin = _context.UserLoginHistories
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.LoginTime)
                .FirstOrDefault();

            if (lastLogin == null || lastLogin.LogoutTime != null)
            {
                return NotFound("No active login found.");
            }

            user.IsLoggedIn = false;
            lastLogin.LogoutTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Logout successful" });
        }
        [HttpGet("check-session")]
        public IActionResult CheckSession([FromQuery] string userId)
        {
            var lastLogin = _context.UserLoginHistories
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.LoginTime)
                .FirstOrDefault();

            if (lastLogin == null)
            {
                return NotFound("No login record found.");
            }

            if (lastLogin.LogoutTime != null || DateTime.UtcNow > lastLogin.SessionExpiry)
            {
                return Unauthorized("Session expired. Please log in again.");
            }

            return Ok(new { message = "Session is still active", sessionExpiry = lastLogin.SessionExpiry });
        }
        [HttpPost("auto-logout")]
        public async Task<IActionResult> AutoLogout([FromQuery] string userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId && u.IsLoggedIn == true);
            var lastLogin = _context.UserLoginHistories
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.LoginTime)
                .FirstOrDefault();

            if (lastLogin == null || lastLogin.LogoutTime != null)
            {
                return NotFound("No active session found.");
            }

            if (DateTime.UtcNow > lastLogin.SessionExpiry)
            {
                user.IsLoggedIn = false;
                lastLogin.LogoutTime = lastLogin.SessionExpiry; // Auto-logout at expiry time
                await _context.SaveChangesAsync();
                return Unauthorized("Session expired. You have been logged out.");
            }

            return Ok(new { message = "Session is still active" });
        }
        [HttpGet("Login-History")]
        public async Task<ActionResult<IEnumerable<UserLoginHistory>>> GetAllLogin()
        {
            return await _context.UserLoginHistories.ToListAsync();
        }

    }
}
