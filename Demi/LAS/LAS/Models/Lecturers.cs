
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LAS.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // MatricNumber for students, StaffID for lecturers

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string UserType { get; set; } // "Student", "Lecturer", or "Admin"

        public bool IsAdmin { get; set; } = false; // Only applies to Lecturers who are Admins

        public string Department { get; set; }
        public string School { get; set; }

        public double? Rating { get; set; } // Only for students, nullable for lecturers

        public List<BorrowRecord> BorrowRecords { get; set; }
    }
}
