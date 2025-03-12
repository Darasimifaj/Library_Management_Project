namespace Library_Management_Project.Models
{
    public class LateReturn
    {

        public int Id { get; set; }
        public string MatricNumber { get; set; } 
        public int LateReturnCount { get; set; } = 0;
    }
}
