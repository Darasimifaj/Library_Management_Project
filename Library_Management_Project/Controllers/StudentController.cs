using System;
using Library_Management_Project.Data;
using Library_Management_Project.Models.Dtos;
using Library_Management_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;


namespace Library_Management_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost] 
        public async Task<IActionResult> RegisterStudent([FromBody] RegisterStudentDto registerDto)
        {
            if (await _context.Students.AnyAsync(s => s.MatricNumber == registerDto.MatricNumber))
            {
                return BadRequest(new { message = "Matric Number already exists." });
            }

            var hashedPassword = HashPassword(registerDto.Password);

            var student = new Student
            {
                MatricNumber = registerDto.MatricNumber,
                Name = registerDto.Name,
                PasswordHash = hashedPassword,
                Email = registerDto.Email,
                Rating = 5.0 
            };

            _context.Students.Add(student);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
               
                Console.WriteLine(ex.InnerException?.Message);
                throw;
            }


            return CreatedAtAction(nameof(GetStudentByMatricNumber), new { matricNumber = student.MatricNumber }, new
            {
                student.Id,
                student.MatricNumber,
                student.Name,
                student.Email,
                student.Rating
            });
        }

        [HttpGet("matricNumber")] 
        public async Task<IActionResult> GetStudentByMatricNumber([FromQuery] string matricNumber)
        {
            var student = await _context.Students
                .Where(s => s.MatricNumber == matricNumber)
                .Select(s => new
                {
                    s.Id,
                    s.MatricNumber,
                    s.Name,
                    s.Rating,
                    BorrowLimit = s.GetBorrowLimit(),
                    RatingCategory = s.GetRatingCategory()
                })
                .FirstOrDefaultAsync();

            if (student == null)
            {
                return NotFound(new { message = "Student not found. Try Again." });
            }

            return Ok(student);
        }

        [HttpGet] 
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _context.Students
                .Select(d => new
                {
                    d.Id,
                    d.MatricNumber,
                    d.Name
                })
                .ToListAsync();

            return Ok(students);
        }

        [HttpPost("login")] 
        public IActionResult LoginStudent([FromQuery] LoginStudentDto loginDto)
        {
            if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return BadRequest("Invalid login data. Password is incorrect. Try Again.");
            }

            var student = _context.Students.FirstOrDefault(s => s.MatricNumber == loginDto.MatricNumber);

            if (student == null || student.PasswordHash != HashPassword(loginDto.Password))
            {
                return Unauthorized("Invalid credentials. Matric Number is incorrect. Try Again.");
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

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordStudentDto changePasswordDto)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.MatricNumber == changePasswordDto.MatricNumber);
            if (student == null)
            {
                return NotFound(new { message = "Student not found." });
            }

            var oldPasswordHash = HashPassword(changePasswordDto.OldPassword);
            if (student.PasswordHash != oldPasswordHash)
            {
                return BadRequest(new { message = "Incorrect old password. Try Again" });
            }

            student.PasswordHash = HashPassword(changePasswordDto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully." });
        }

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
