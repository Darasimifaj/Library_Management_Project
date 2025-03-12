namespace Library_Management_Project.Models
{
    public class Lecturer
    {
        public int Id { get; set; }
        public string StaffId { get; set; }
        public string StaffEmail { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }
    }
}
