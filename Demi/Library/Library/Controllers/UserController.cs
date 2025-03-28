using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Library.Data;
using Library.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Get all users
        [HttpGet]
        public async Task<ActionResult<object>> GetUsers()
        {
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
                    u.Rating
                }).ToListAsync();

            return Ok(users);
        }


        [HttpGet("Users")]
        public async Task<IActionResult> GetAllBorrowHistory([FromQuery] string? UserId, [FromQuery] string? UserType, [FromQuery] string? FirstName,[FromQuery] string? LastName, [FromQuery] bool? IsAdmin, [FromQuery] bool? IsActive,  [FromQuery] bool? IsLogged, [FromQuery] string? Department, [FromQuery] string? School ,[FromQuery] double? Rating, [FromQuery] string? email)// [FromQuery] int? Level 
        {
            if (!string.IsNullOrEmpty(UserId))
            {
                UserId = Uri.UnescapeDataString(UserId);
            }
            var query = _context.Users.AsQueryable();
            if (Rating.HasValue)
            {
                query = query.Where(b => b.Rating == Rating);
            }
            if (!string.IsNullOrEmpty(Department))
            {
                query = query.Where(b => b.Department.Contains(Department));
            }
            if (!string.IsNullOrEmpty(UserType))
            {
                query = query.Where(b => b.UserType.Contains(UserType));
            }
            if (!string.IsNullOrEmpty(FirstName))
            {
                query = query.Where(b => b.FirstName.Contains(FirstName));
            }

            if (!string.IsNullOrEmpty(School))
            {
                query = query.Where(b => b.School.Contains(School));
            }
            if (!string.IsNullOrEmpty(UserId))
            {
                query = query.Where(b => b.UserId.Contains(UserId));
            }
            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(b => b.Email.Contains(email));
            }
            if (!string.IsNullOrEmpty(LastName))
            {
                query = query.Where(b => b.LastName.Contains(LastName));
            }

            if (IsActive.HasValue)
            {
                query = query.Where(b => b.IsActive == IsActive.Value);
            }

            if (IsAdmin.HasValue)
            {
                query = query.Where(b => b.IsAdmin == IsAdmin);
            }


            if (IsLogged.HasValue)
            {
                query = query.Where(b => b.IsLoggedIn == IsLogged.Value);
            }

            // Compute the statistics
            var total = await query.CountAsync();
            
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
                })
                .ToListAsync();

            return Ok(new
            {
                total=total,
                Users=Users
            });
        }


        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromQuery] RegisterUserDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest("Email already in use.");
            }

            // Hash the password before storing it
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
                    u.Rating
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // Helper function to hash passwords
       
        // ✅ Update an existing user
        [HttpPut("{UserId}")]
        public async Task<IActionResult> UpdateUser(string UserId, User updatedUser)
        {
            UserId = Uri.UnescapeDataString(UserId);
            var user = await _context.Users.FindAsync(UserId);
            if (user == null)
                return NotFound();

            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.Email = updatedUser.Email;
            user.UserType = updatedUser.UserType;
            user.IsAdmin = updatedUser.IsAdmin;
            user.Department = updatedUser.Department;
            user.School = updatedUser.School;

            if (!string.IsNullOrWhiteSpace(updatedUser.PasswordHash))
                user.PasswordHash = HashPassword(updatedUser.PasswordHash); // Update password

            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpGet("export-users")]
        public async Task<IActionResult> ExportUsers()
        {
            var users = await _context.Users.ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Users");
                worksheet.Cell(1, 1).Value = "User ID";
                worksheet.Cell(1, 2).Value = "First Name";
                worksheet.Cell(1, 3).Value = "Last Name";
                worksheet.Cell(1, 4).Value = "Email";
                worksheet.Cell(1, 5).Value = "User Type";
                worksheet.Cell(1, 6).Value = "Department";
                worksheet.Cell(1, 7).Value = "School";
                worksheet.Cell(1, 8).Value = "Rating";
                worksheet.Cell(1, 9).Value = "IsActive";


                int row = 2;
                foreach (var user in users)
                {
                    worksheet.Cell(row, 1).Value = user.UserId;
                    worksheet.Cell(row, 2).Value = user.FirstName;
                    worksheet.Cell(row, 3).Value = user.LastName;
                    worksheet.Cell(row, 4).Value = user.Email;
                    worksheet.Cell(row, 5).Value = user.UserType;
                    worksheet.Cell(row, 6).Value = user.Department;
                    worksheet.Cell(row, 7).Value = user.School;
                    worksheet.Cell(row, 8).Value = user.Rating;
                    worksheet.Cell(row, 9).Value = user.IsActive;
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Users.xlsx");
                }
            }
        }

        [HttpPost("import-users")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportUsers(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file.");

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RowsUsed().Skip(1);

                    foreach (var row in rows)
                    {
                        var user = new User
                        {
                            UserId = row.Cell(1).GetString(),
                            FirstName = row.Cell(2).GetString(),
                            LastName = row.Cell(3).GetString(),
                            Email = row.Cell(4).GetString(),
                            UserType = row.Cell(5).GetString(),
                            Department = row.Cell(6).GetString(),
                            School = row.Cell(7).GetString(),
                            Rating = row.Cell(8).GetDouble(),
                            IsAdmin= false,
                            IsActive=false
                        };
                        _context.Users.Add(user);
                    }

                    await _context.SaveChangesAsync();
                }
            }

            return Ok("Users imported successfully.");
        }

         //✅ Delete a user
         //✅ Deactivate User(Soft Delete)
        [HttpPost("deactivate")]
        public async Task<IActionResult> DeactivateUser([FromQuery] string userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return NotFound("User not found.");

            user.IsActive = false; // Set user as inactive
            await _context.SaveChangesAsync();

            return Ok(new { message = "User deactivated successfully." });
        }

        // ✅ Reactivate User
        [HttpPost("reactivate")]
        public async Task<IActionResult> ReactivateUser([FromQuery] string userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return NotFound("User not found.");

            user.IsActive = true; // Set user as active
            await _context.SaveChangesAsync();

            return Ok(new { message = "User reactivated successfully." });
        }

        // ❌ Delete User (Permanent)
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUser([FromQuery] string userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return NotFound("User not found.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully." });
        }


        // ✅ Password Hashing Function
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
