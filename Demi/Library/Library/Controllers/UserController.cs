//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Cryptography;
//using System.Text;
//using System.Threading.Tasks;
//using ClosedXML.Excel;
//using DocumentFormat.OpenXml.Spreadsheet;
//using Library.Data;
//using Library.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace Library.Controllers
//{
//    [Route("api/user")]
//    [ApiController]
//    [Authorize]
//    public class UserController : ControllerBase
//    {
//        private readonly ILogger<UserController> _logger;
//        private readonly AppDbContext _context;

//        public UserController(AppDbContext context, ILogger<UserController> logger)
//        {
//            _logger = logger;
//            _context = context;
//        }

//        // ✅ Get all users
//        [HttpGet]
//        public async Task<ActionResult<object>> GetUsers()
//        {


//            var users = await _context.Users
//                .Select(u => new
//                {
//                    u.Id,
//                    u.UserId,
//                    u.FirstName,
//                    u.LastName,
//                    u.Email,
//                    u.UserType,
//                    u.IsAdmin,
//                    u.Department,
//                    u.School,
//                    u.IsLoggedIn,
//                    u.IsActive,
//                    u.Rating,
//                    CurrentlyBorrowed = _context.BorrowRecords.Count(br => br.UserId == u.UserId && !br.IsReturned),

//                    borrowlimit=u.GetBorrowLimit()
//                }).ToListAsync();

//            return Ok(users);
//        }


//        [HttpGet("Users")]
//        public async Task<IActionResult> GetAllBorrowHistory([FromQuery] string? UserId, [FromQuery] string? UserType, [FromQuery] string? FirstName,[FromQuery] string? LastName, [FromQuery] bool? IsAdmin, [FromQuery] bool? IsActive,  [FromQuery] bool? IsLogged, [FromQuery] string? Department, [FromQuery] string? School ,[FromQuery] double? Rating, [FromQuery] string? email)// [FromQuery] int? Level 
//        {
//            if (!string.IsNullOrEmpty(UserId))
//            {
//                UserId = Uri.UnescapeDataString(UserId);
//            }
//            var query = _context.Users.AsQueryable();
//            if (Rating.HasValue)
//            {
//                query = query.Where(b => b.Rating == Rating);
//            }
//            if (!string.IsNullOrEmpty(Department))
//            {
//                query = query.Where(b => b.Department.Contains(Department));
//            }
//            if (!string.IsNullOrEmpty(UserType))
//            {
//                query = query.Where(b => b.UserType.Contains(UserType));
//            }
//            if (!string.IsNullOrEmpty(FirstName))
//            {
//                query = query.Where(b => b.FirstName.Contains(FirstName));
//            }

//            if (!string.IsNullOrEmpty(School))
//            {
//                query = query.Where(b => b.School.Contains(School));
//            }
//            if (!string.IsNullOrEmpty(UserId))
//            {
//                query = query.Where(b => b.UserId.Contains(UserId));
//            }
//            if (!string.IsNullOrEmpty(email))
//            {
//                query = query.Where(b => b.Email.Contains(email));
//            }
//            if (!string.IsNullOrEmpty(LastName))
//            {
//                query = query.Where(b => b.LastName.Contains(LastName));
//            }

//            if (IsActive.HasValue)
//            {
//                query = query.Where(b => b.IsActive == IsActive.Value);
//            }

//            if (IsAdmin.HasValue)
//            {
//                query = query.Where(b => b.IsAdmin == IsAdmin);
//            }


//            if (IsLogged.HasValue)
//            {
//                query = query.Where(b => b.IsLoggedIn == IsLogged.Value);
//            }

//            // Compute the statistics
//            var total = await query.CountAsync();
//            int currentlyBorrowed = await _context.BorrowRecords
//                .CountAsync(b => b.UserId == UserId && !b.IsReturned);

//            var Users = await query
//                .Select(b => new
//                {
//                    b.UserId,
//                    b.FirstName,
//                    b.LastName,
//                    b.Email,
//                    b.UserType,
//                    b.IsAdmin,
//                    b.Department,
//                    b.School,
//                    b.Rating,
//                    b.IsActive,
//                    b.IsLoggedIn,
//                    CurrentlyBorrowed = _context.BorrowRecords.Count(br => br.UserId == b.UserId && !br.IsReturned),
//                    borrowlimit = b.GetBorrowLimit()
//                })
//                .ToListAsync();

//            return Ok(new
//            {
//                total=total,
//                Users=Users
//            });
//        }


//        [HttpPost("register")]
//        public async Task<IActionResult> RegisterUser([FromQuery] RegisterUserDto dto)
//        {
//            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
//            {
//                return BadRequest("Email already in use.");
//            }

//            // Hash the password before storing it
//            string passwordHash = HashPassword(dto.Password);

//            var newUser = new User
//            {
//                UserId = dto.UserId,
//                FirstName = dto.FirstName,
//                LastName = dto.LastName,
//                Email = dto.Email,
//                UserType = dto.UserType,
//                IsAdmin = dto.IsAdmin,
//                Department = dto.Department,
//                School = dto.School,
//                PasswordHash = passwordHash,
//                Rating = (dto.UserType == "Student" || dto.UserType == "Lecturer") ? 5.0 : 1

//            };

//            _context.Users.Add(newUser);
//            await _context.SaveChangesAsync();

//            return CreatedAtAction(nameof(GetUser), new { userId = newUser.UserId }, new
//            {
//                newUser.UserId,
//                newUser.FirstName,
//                newUser.LastName,
//                newUser.Email,
//                newUser.UserType,
//                newUser.IsAdmin,
//                newUser.Department,
//                newUser.School,
//                newUser.Rating
//            });
//        }

//        [HttpGet("{UserId}")]
//        public async Task<ActionResult<object>> GetUser(string UserId)
//        {
//            int currentlyBorrowed = await _context.BorrowRecords
//                .CountAsync(b => b.UserId == UserId && !b.IsReturned);
//            UserId = Uri.UnescapeDataString(UserId);
//            var user = await _context.Users
//                .Where(u => u.UserId == UserId)
//                .Select(u => new
//                {
//                    u.Id,
//                    u.UserId,
//                    u.FirstName,
//                    u.LastName,
//                    u.Email,
//                    u.UserType,
//                    u.IsAdmin,
//                    u.Department,
//                    u.School,
//                    u.Rating,
//                    CurrentlyBorrowed = _context.BorrowRecords.Count(br => br.UserId == u.UserId && !br.IsReturned),
//                    borrowlimit = u.GetBorrowLimit(),
//                    RCategory =u.GetRatingCategory()
//                })
//                .FirstOrDefaultAsync();

//            if (user == null)
//                return NotFound();

//            return Ok(user);
//        }

//        // Helper function to hash passwords

//        // ✅ Update an existing user
//        [HttpPut("{UserId}")]
//        public async Task<IActionResult> UpdateUser(string UserId, User updatedUser)
//        {
//            UserId = Uri.UnescapeDataString(UserId);
//            var user = await _context.Users.FindAsync(UserId);
//            if (user == null)
//                return NotFound();

//            user.FirstName = updatedUser.FirstName;
//            user.LastName = updatedUser.LastName;
//            user.Email = updatedUser.Email;
//            user.UserType = updatedUser.UserType;
//            user.IsAdmin = updatedUser.IsAdmin;
//            user.Department = updatedUser.Department;
//            user.School = updatedUser.School;

//            if (!string.IsNullOrWhiteSpace(updatedUser.PasswordHash))
//                user.PasswordHash = HashPassword(updatedUser.PasswordHash); // Update password

//            await _context.SaveChangesAsync();
//            return NoContent();
//        }
//        [HttpGet("export-users")]
//        public async Task<IActionResult> ExportUsers()
//        {
//            var users = await _context.Users.ToListAsync();

//            using (var workbook = new XLWorkbook())
//            {
//                var worksheet = workbook.Worksheets.Add("Users");
//                worksheet.Cell(1, 1).Value = "User ID";
//                worksheet.Cell(1, 2).Value = "First Name";
//                worksheet.Cell(1, 3).Value = "Last Name";
//                worksheet.Cell(1, 4).Value = "Email";
//                worksheet.Cell(1, 5).Value = "User Type";
//                worksheet.Cell(1, 6).Value = "Department";
//                worksheet.Cell(1, 7).Value = "School";
//                worksheet.Cell(1, 8).Value = "Rating";
//                worksheet.Cell(1, 9).Value = "IsActive";


//                int row = 2;
//                foreach (var user in users)
//                {
//                    worksheet.Cell(row, 1).Value = user.UserId;
//                    worksheet.Cell(row, 2).Value = user.FirstName;
//                    worksheet.Cell(row, 3).Value = user.LastName;
//                    worksheet.Cell(row, 4).Value = user.Email;
//                    worksheet.Cell(row, 5).Value = user.UserType;
//                    worksheet.Cell(row, 6).Value = user.Department;
//                    worksheet.Cell(row, 7).Value = user.School;
//                    worksheet.Cell(row, 8).Value = user.Rating;
//                    worksheet.Cell(row, 9).Value = user.IsActive;
//                    row++;
//                }

//                using (var stream = new MemoryStream())
//                {
//                    workbook.SaveAs(stream);
//                    var content = stream.ToArray();
//                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Users.xlsx");
//                }
//            }
//        }

//        [HttpPost("import-users")]
//        [Consumes("multipart/form-data")]
//        public async Task<IActionResult> ImportUsers(IFormFile file)
//        {
//            if (file == null || file.Length == 0)
//                return BadRequest("Invalid file.");

//            using (var stream = new MemoryStream())
//            {
//                await file.CopyToAsync(stream);
//                using (var workbook = new XLWorkbook(stream))
//                {
//                    var worksheet = workbook.Worksheet(1);
//                    var rows = worksheet.RowsUsed().Skip(1);

//                    foreach (var row in rows)
//                    {
//                        var user = new User
//                        {
//                            UserId = row.Cell(1).GetString(),
//                            FirstName = row.Cell(2).GetString(),
//                            LastName = row.Cell(3).GetString(),
//                            Email = row.Cell(4).GetString(),
//                            UserType = row.Cell(5).GetString(),
//                            Department = row.Cell(6).GetString(),
//                            School = row.Cell(7).GetString(),
//                            Rating = row.Cell(8).GetDouble(),
//                            IsAdmin= false,
//                            IsActive=false
//                        };
//                        _context.Users.Add(user);
//                    }

//                    await _context.SaveChangesAsync();
//                }
//            }

//            return Ok("Users imported successfully.");
//        }

//         //✅ Delete a user
//         //✅ Deactivate User(Soft Delete)
//        [HttpPost("deactivate")]
//        public async Task<IActionResult> DeactivateUser([FromQuery] string userId)
//        {
//            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
//            if (user == null)
//                return NotFound("User not found.");

//            user.IsActive = false; // Set user as inactive
//            await _context.SaveChangesAsync();

//            return Ok(new { message = "User deactivated successfully." });
//        }

//        // ✅ Reactivate User
//        [HttpPost("reactivate")]
//        public async Task<IActionResult> ReactivateUser([FromQuery] string userId)
//        {
//            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
//            if (user == null)
//                return NotFound("User not found.");

//            user.IsActive = true; // Set user as active
//            await _context.SaveChangesAsync();

//            return Ok(new { message = "User reactivated successfully." });
//        }

//        // ❌ Delete User (Permanent)
//        [HttpDelete("delete")]
//        public async Task<IActionResult> DeleteUser([FromQuery] string userId)
//        {
//            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
//            if (user == null)
//                return NotFound("User not found.");

//            _context.Users.Remove(user);
//            await _context.SaveChangesAsync();

//            return Ok(new { message = "User deleted successfully." });
//        }


//        // ✅ Password Hashing Function
//        private string HashPassword(string password)
//        {
//            using (var sha256 = SHA256.Create())
//            {
//                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
//                StringBuilder builder = new StringBuilder();
//                foreach (byte b in bytes)
//                {
//                    builder.Append(b.ToString("x2"));
//                }
//                return builder.ToString();
//            }
//        }
//    }
//}
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Library.Data;
using Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Controllers
{
    [Route("api/user")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly AppDbContext _context;

        public UserController(AppDbContext context, ILogger<UserController> logger)
        {
            _logger = logger;
            _context = context;
        }

        // ✅ Get all users
        [HttpGet]
        public async Task<ActionResult<object>> GetUsers()
        {
            _logger.LogInformation("Fetching all users.");

            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.UserId,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.UserType,
                    u.IsAdmin,
                    u.Department,
                    u.School,
                    u.IsLoggedIn,
                    u.IsActive,
                    u.Rating,
                    CurrentlyBorrowed = _context.BorrowRecords.Count(br => br.UserId == u.UserId && !br.IsReturned),
                    borrowlimit = u.GetBorrowLimit()
                }).ToListAsync();

            _logger.LogInformation($"Fetched {users.Count} users.");
            return Ok(users);
        }

        [HttpGet("Users")]
        public async Task<IActionResult> GetAllBorrowHistory([FromQuery] string? UserId, [FromQuery] string? UserType, [FromQuery] string? FirstName, [FromQuery] string? LastName, [FromQuery] bool? IsAdmin, [FromQuery] bool? IsActive, [FromQuery] bool? IsLogged, [FromQuery] string? Department, [FromQuery] string? School, [FromQuery] double? Rating, [FromQuery] string? email)
        {
            _logger.LogInformation("Fetching filtered user history.");

            if (!string.IsNullOrEmpty(UserId))
            {
                UserId = Uri.UnescapeDataString(UserId);
            }

            var query = _context.Users.AsQueryable();

            if (Rating.HasValue)
                query = query.Where(b => b.Rating == Rating);
            if (!string.IsNullOrEmpty(Department))
                query = query.Where(b => b.Department.Contains(Department));
            if (!string.IsNullOrEmpty(UserType))
                query = query.Where(b => b.UserType.Contains(UserType));
            if (!string.IsNullOrEmpty(FirstName))
                query = query.Where(b => b.FirstName.Contains(FirstName));
            if (!string.IsNullOrEmpty(School))
                query = query.Where(b => b.School.Contains(School));
            if (!string.IsNullOrEmpty(UserId))
                query = query.Where(b => b.UserId.Contains(UserId));
            if (!string.IsNullOrEmpty(email))
                query = query.Where(b => b.Email.Contains(email));
            if (!string.IsNullOrEmpty(LastName))
                query = query.Where(b => b.LastName.Contains(LastName));
            if (IsActive.HasValue)
                query = query.Where(b => b.IsActive == IsActive.Value);
            if (IsAdmin.HasValue)
                query = query.Where(b => b.IsAdmin == IsAdmin);
            if (IsLogged.HasValue)
                query = query.Where(b => b.IsLoggedIn == IsLogged.Value);

            var total = await query.CountAsync();
            int currentlyBorrowed = await _context.BorrowRecords
                .CountAsync(b => b.UserId == UserId && !b.IsReturned);

            var Users = await query
                .Select(b => new
                {
                    b.UserId,
                    b.FirstName,
                    b.LastName,
                    b.Email,
                    b.UserType,
                    b.IsAdmin,
                    b.Department,
                    b.School,
                    b.Rating,
                    b.IsActive,
                    b.IsLoggedIn,
                    CurrentlyBorrowed = _context.BorrowRecords.Count(br => br.UserId == b.UserId && !br.IsReturned),
                    borrowlimit = b.GetBorrowLimit()
                })
                .ToListAsync();

            _logger.LogInformation($"Fetched {Users.Count} users after applying filters.");
            return Ok(new { total, Users });
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromQuery] RegisterUserDto dto)
        {
            _logger.LogInformation($"Registering new user with email: {dto.Email}");

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                _logger.LogWarning($"Registration failed for email {dto.Email}: Email already in use.");
                return BadRequest("Email already in use.");
            }

            string passwordHash = HashPassword(dto.Password);

            var newUser = new User
            {
                UserId = dto.UserId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                UserType = dto.UserType,
                IsAdmin = dto.IsAdmin,
                Department = dto.Department,
                School = dto.School,
                PasswordHash = passwordHash,
                Rating = (dto.UserType == "Student" || dto.UserType == "Lecturer") ? 5.0 : 1
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User with email {dto.Email} registered successfully.");
            return CreatedAtAction(nameof(GetUser), new { userId = newUser.UserId }, new
            {
                newUser.UserId,
                newUser.FirstName,
                newUser.LastName,
                newUser.Email,
                newUser.UserType,
                newUser.IsAdmin,
                newUser.Department,
                newUser.School,
                newUser.Rating
            });
        }

        [HttpGet("{UserId}")]
        public async Task<ActionResult<object>> GetUser(string UserId)
        {
            _logger.LogInformation($"Fetching details for user with ID: {UserId}");

            int currentlyBorrowed = await _context.BorrowRecords
                .CountAsync(b => b.UserId == UserId && !b.IsReturned);

            UserId = Uri.UnescapeDataString(UserId);
            var user = await _context.Users
                .Where(u => u.UserId == UserId)
                .Select(u => new
                {
                    u.Id,
                    u.UserId,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.UserType,
                    u.IsAdmin,
                    u.Department,
                    u.School,
                    u.Rating,
                    CurrentlyBorrowed = _context.BorrowRecords.Count(br => br.UserId == u.UserId && !br.IsReturned),
                    borrowlimit = u.GetBorrowLimit(),
                    RCategory = u.GetRatingCategory()
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                _logger.LogWarning($"User with ID {UserId} not found.");
                return NotFound();
            }

            _logger.LogInformation($"Fetched details for user with ID: {UserId}");
            return Ok(user);
        }

        [HttpPut("{UserId}")]
        public async Task<IActionResult> UpdateUser(string UserId, User updatedUser)
        {
            _logger.LogInformation($"Updating user with ID: {UserId}");

            UserId = Uri.UnescapeDataString(UserId);
            var user = await _context.Users.FindAsync(UserId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {UserId} not found.");
                return NotFound();
            }

            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.Email = updatedUser.Email;
            user.UserType = updatedUser.UserType;
            user.IsAdmin = updatedUser.IsAdmin;
            user.Department = updatedUser.Department;
            user.School = updatedUser.School;

            if (!string.IsNullOrWhiteSpace(updatedUser.PasswordHash))
                user.PasswordHash = HashPassword(updatedUser.PasswordHash);

            await _context.SaveChangesAsync();
            _logger.LogInformation($"User with ID {UserId} updated successfully.");
            return NoContent();
        }

        [HttpPost("deactivate")]
        public async Task<IActionResult> DeactivateUser([FromQuery] string userId)
        {
            _logger.LogInformation($"Deactivating user with ID: {userId}");

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found.");
                return NotFound("User not found.");
            }

            user.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User with ID {userId} deactivated.");
            return Ok(new { message = "User deactivated successfully." });
        }

        [HttpPost("reactivate")]
        public async Task<IActionResult> ReactivateUser([FromQuery] string userId)
        {
            _logger.LogInformation($"Reactivating user with ID: {userId}");

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found.");
                return NotFound("User not found.");
            }

            user.IsActive = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User with ID {userId} reactivated.");
            return Ok(new { message = "User reactivated successfully." });
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUser([FromQuery] string userId)
        {
            _logger.LogInformation($"Deleting user with ID: {userId}");

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found.");
                return NotFound("User not found.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User with ID {userId} deleted.");
            return Ok(new { message = "User deleted successfully." });
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
