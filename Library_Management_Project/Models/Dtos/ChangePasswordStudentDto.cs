namespace Library_Management_Project.Models.Dtos
{
    public class ChangePasswordStudentDto
    {
        public string MatricNumber { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
