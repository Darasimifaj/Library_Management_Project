using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace LAS.Models
{
    public class Student
    {
        public int Id { get; set; }
        //[Required]
        //[RegularExpression(@"^\d{2}/\d{4}$", ErrorMessage = "Matric number must be in the format XX/XXXX (e.g., 22/2464).")]
        //[Column(TypeName = "nvarchar(10)")] // Ensure correct DB storage
        public string MatricNumber { get; set; }
        //[Required]
        //[RegularExpression(@"^[a-zA-Z0-9._%+-]+@student\.babcock\.edu\.ng$",
        //ErrorMessage = "Email must be a valid student email (@student.babcock.edu.ng).")]
        public string Email { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
        public string School { get; set; }
        public string PasswordHash { get; set; }
        
        //private int _level = 100;

        //public int Level
        //{
        //    get => _level;
        //    set
        //    {
        //        if (value % 100 != 0 || value < 100 || value > 600)
        //        {
        //            throw new ArgumentException("Level must be a multiple of 100 and between 100 and 600.");
        //        }
        //        _level = value;
        //    }
        //}
      


        // ✅ Remove manual BorrowedBooks and use computed property
        public int BorrowedBooks => BorrowRecords?.Count ?? 0;

        [Range(1.0, 10.0)]
        public double Rating { get; set; } = 5.0;

        // ✅ Navigation property for BorrowRecords
        public List<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();

        public string GetRatingCategory()
        {
            if (Rating >= 10) return "Excellent";
            if (Rating >= 8) return "Very Good";
            if (Rating >= 6) return "Good";
            if (Rating >= 4) return "Fair";
            return "Bad";
        }

        public int GetBorrowLimit()
        {
            return (int)Math.Floor(Rating);
        }
    }
}
