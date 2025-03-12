namespace Library_Management_Project.Models.Dtos
{
    public class ChangePasswordStaffDto
    {
        public string StaffId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
