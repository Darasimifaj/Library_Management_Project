namespace Library_Management_Project.Models
{
    public class BorrowRecord
    {

        public int Id { get; set; }
        public string MatricNumber { get; set; }
        public string SerialNumber { get; set; } 
        public DateTime BorrowTime { get; set; } = DateTime.UtcNow;
        public float AllowedBorrowHours { get; set; } 
        public DateTime DueDate { get; set; } 
        public DateTime? ReturnTime { get; set; } 
        public bool IsReturned { get; set; } = false;

       
        public bool IsLateReturn()
        {
            return ReturnTime.HasValue && ReturnTime.Value > DueDate;
        }

        public bool IsEarlyReturn()
        {
            return ReturnTime.HasValue && ReturnTime.Value < DueDate;
        }
    }
}
