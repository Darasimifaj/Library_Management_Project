using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LAS.Models
{
    public class BorrowRecord
    {
        public int Id { get; set; }
        public string MatricNumber { get; set; } // Student identifier
        public string Department { get; set; }
        //public int Level { get; set; }
        public string School { get; set; }
        public string SerialNumber { get; set; } // Book Serial Number
        public DateTime BorrowTime { get; set; } = DateTime.UtcNow;
        public float AllowedBorrowHours { get; set; } // Allowed borrowing time (in hours)
        public DateTime DueDate { get; set; } // Calculated due date
        public DateTime? ReturnTime { get; set; } // Nullable return time
        public bool IsReturned { get; set; } = false;
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public bool Overdue { get; private set; }


        public bool overdue()
        {
            return !IsReturned && DateTime.UtcNow > DueDate;
        }


        // Determines if the book was returned late
        public bool IsLateReturn()
        {
            return ReturnTime.HasValue && ReturnTime.Value > DueDate;
        }

        // Determines if the book was returned early (before due date)
        public bool IsEarlyReturn()
        {
            return ReturnTime.HasValue && ReturnTime.Value < DueDate;
        }
        
    }
}
