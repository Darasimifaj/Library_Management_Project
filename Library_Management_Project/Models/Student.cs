using System.ComponentModel.DataAnnotations;

namespace Library_Management_Project.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string MatricNumber { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }

        public int BorrowedBooks => BorrowRecords?.Count ?? 0;

        [Range(1.0, 10.0)]
        public double Rating { get; set; } = 5.0;

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
