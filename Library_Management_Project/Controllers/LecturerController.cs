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
    public class LecturerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LecturerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterStaff([FromBody] RegisterStaffDto registerDto)
        {
            if (await _context.Lecturers.AnyAsync(s => s.StaffId == registerDto.StaffId))
            {
                return BadRequest(new { message = "Staff ID already exists." });
            }

            var hashedPassword = HashPassword(registerDto.Password);

            var staff = new Lecturer
            {
                StaffId = registerDto.StaffId,
                Name = registerDto.Name,
                PasswordHash = hashedPassword,
                StaffEmail = registerDto.StaffEmail
            };

            _context.Lecturers.Add(staff);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
                throw;
            }

            return CreatedAtAction(nameof(GetStaffByStaffId), new { staffId = staff.StaffId }, new
            {
                staff.Id,
                staff.StaffId,
                staff.Name,
                staff.StaffEmail
            });
        }

        [HttpGet("{staffId}")]
        public async Task<IActionResult> GetStaffByStaffId(string staffId)
        {
            var staff = await _context.Lecturers
                .Where(s => s.StaffId == staffId)
                .Select(s => new
                {
                    s.Id,
                    s.StaffId,
                    s.Name
                })
                .FirstOrDefaultAsync();

            if (staff == null)
            {
                return NotFound(new { message = "Staff not found. Try Again." });
            }

            return Ok(staff);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStaff()
        {
            var staffList = await _context.Lecturers
                .Select(d => new
                {
                    d.Id,
                    d.StaffId,
                    d.Name
                })
                .ToListAsync();

            return Ok(staffList);
        }

        [HttpPost("login")]
        public IActionResult LoginStaff([FromBody] LoginStaffDto loginDto)
        {
            if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return BadRequest("Invalid login data. Password is incorrect. Try Again.");
            }

            var staff = _context.Lecturers.FirstOrDefault(s => s.StaffId == loginDto.StaffId);

            if (staff == null || staff.PasswordHash != HashPassword(loginDto.Password))
            {
                return Unauthorized("Invalid credentials. Staff ID is incorrect. Try Again.");
            }

            return Ok(new
            {
                message = "Login successful.",
                staff.Id,
                staff.StaffId,
                staff.Name
            });
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordStaffDto changePasswordDto)
        {
            var staff = await _context.Lecturers.FirstOrDefaultAsync(s => s.StaffId == changePasswordDto.StaffId);
            if (staff == null)
            {
                return NotFound(new { message = "Staff not found." });
            }

            var oldPasswordHash = HashPassword(changePasswordDto.OldPassword);
            if (staff.PasswordHash != oldPasswordHash)
            {
                return BadRequest(new { message = "Incorrect old password. Try Again" });
            }

            staff.PasswordHash = HashPassword(changePasswordDto.NewPassword);
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
