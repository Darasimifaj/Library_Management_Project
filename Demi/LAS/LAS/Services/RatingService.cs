using LAS.Data;
using LAS.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace LAS.Services
{
    public class RatingService
    {
        private readonly AppDbContext _context;

        public RatingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task UpdateStudentRating(string matricNumber, bool isLateReturn, bool isEarlyReturn)
        {
            // Fetch the student
            var student = await _context.Students.FirstOrDefaultAsync(s => s.MatricNumber == matricNumber);

            if (student == null)
            {
                return; // Student not found, do nothing
            }

            // Adjust rating
            if (isLateReturn)
                student.Rating = Math.Max(1.0, student.Rating - 0.25); // Min rating = 1.0
            if (isEarlyReturn)
                student.Rating = Math.Min(10.0, student.Rating + 0.15); // Max rating = 10.0

            await _context.SaveChangesAsync();
        }
    }
}
