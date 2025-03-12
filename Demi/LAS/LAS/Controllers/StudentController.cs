using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LAS.Data;
using LAS.Models;
using LAS.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace LAS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StudentsController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ GET ALL STUDENTS (ID, MatricNumber, Name)
        // ✅ GET ALL STUDENTS (ID, MatricNumber, Name)
        [HttpGet]
        public async Task<IActionResult> GetAllStudents(
            [FromQuery] string? matricNumber,
            [FromQuery] string? Email,
            [FromQuery] string? Department,
            [FromQuery] string? School,
            [FromQuery] string? Name,
            [FromQuery] double? Rating)
        {
            var query = _context.Students.AsQueryable();

            if (!string.IsNullOrEmpty(Department))
            {
                query = query.Where(b => b.Department.Contains(Department));
            }

            if (!string.IsNullOrEmpty(School))
            {
                query = query.Where(b => b.School.Contains(School));
            }

            if (!string.IsNullOrEmpty(matricNumber))
            {
                query = query.Where(b => b.MatricNumber.Contains(matricNumber));
            }

            if (!string.IsNullOrEmpty(Email))
            {
                query = query.Where(b => b.Email.Contains(Email));
            }

            if (!string.IsNullOrEmpty(Name))
            {
                query = query.Where(b => b.Name.Contains(Name));
            }

            if (Rating.HasValue)
            {
                query = query.Where(b => b.Rating == Rating);
            }

            // ✅ Properly awaiting ToListAsync()
            var students = await query
                .Select(s => new
                {
                    s.Id,
                    s.MatricNumber,
                    s.Name,
                    s.Email,
                    s.Department,
                    s.School,
                    s.Rating
                })
                .ToListAsync();

            return Ok(students);
        }

        // ✅ GET STUDENT BY MATRIC NUMBER
        [HttpGet("{matricNumber}")]
        public async Task<IActionResult> GetStudentByMatricNumber([FromRoute] string matricNumber)
        {
            var decodedMatricNumber = Uri.UnescapeDataString(matricNumber);
    
            if (string.IsNullOrEmpty(matricNumber))
            {
                return BadRequest(new { message = "Matric number is required." });
            }

            var student = await _context.Students
                .Where(s => s.MatricNumber == decodedMatricNumber)
                .Select(s => new
                {
                    s.Id,
                    s.MatricNumber,
                    s.Name,
                    s.Email,
                    BorrowLimit = s.GetBorrowLimit(),
                    RatingCategory = s.GetRatingCategory()
                })
                .FirstOrDefaultAsync();

            if (student == null)
            {
                return NotFound(new { message = "Student not found." });
            }

            return Ok(student);
        }


        // ✅ REGISTER A NEW STUDENT
        [HttpPost]
        public async Task<IActionResult> RegisterStudent([FromQuery] RegisterStudentDto registerDto)
        {
            if (await _context.Students.AnyAsync(s => s.MatricNumber == registerDto.MatricNumber))
            {
                return BadRequest(new { message = "Matric Number already exists." });
            }
            if (await _context.Students.AnyAsync(s => s.Email == registerDto.Email))
            {
                return BadRequest(new { message = "Email already exists." });
            }

            var hashedPassword = HashPassword(registerDto.Password);

            var student = new Student
            {
                MatricNumber = registerDto.MatricNumber,
                Name = registerDto.Name,
                Department =registerDto.Department,
                School= registerDto.School,
                //Level =registerDto.Level,
                PasswordHash = hashedPassword,
                Email=registerDto.Email,
                Rating = 5.0 // Default rating
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStudentByMatricNumber), new { matricNumber = student.MatricNumber }, new
            {
                student.Id,
                student.MatricNumber,
                student.Name,
                student.Email,
                student.Rating
            });
        }

        // ✅ LOGIN STUDENT
        [HttpPost("login")]
        public IActionResult LoginStudent([FromBody] LoginStudentDto loginDto)
        {
            if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return BadRequest("Invalid login data.");
            }

            var student = _context.Students.FirstOrDefault(s => s.MatricNumber == loginDto.MatricNumber);

            if (student == null || student.PasswordHash != HashPassword(loginDto.Password))
            {
                return Unauthorized("Invalid credentials.");
            }

            return Ok(new
            {
                message = "Login successful.",
                student.Id,
                student.MatricNumber,
                student.Name,
                student.Rating,
                BorrowLimit = student.GetBorrowLimit(),
                RatingCategory = student.GetRatingCategory()
            });
        }

        // ✅ CHANGE PASSWORD
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.MatricNumber == changePasswordDto.MatricNumber);
            if (student == null)
            {
                return NotFound(new { message = "Student not found." });
            }

            var oldPasswordHash = HashPassword(changePasswordDto.OldPassword);
            if (student.PasswordHash != oldPasswordHash)
            {
                return BadRequest(new { message = "Incorrect old password." });
            }

            student.PasswordHash = HashPassword(changePasswordDto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully." });
        }

        // ✅ GET STUDENT RATING
        [HttpGet("rating/{matricNumber}")]
        public async Task<IActionResult> GetStudentRating(string matricNumber)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.MatricNumber == matricNumber);
            if (student == null)
                return NotFound("Student not found.");

            return Ok(new
            {
                student.Rating,
                BorrowLimit = student.GetBorrowLimit(),
                RatingCategory = student.GetRatingCategory()
            });
        }

        // ✅ HELPER METHOD: HASH PASSWORD
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }
}
