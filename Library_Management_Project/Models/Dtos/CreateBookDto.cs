using System.ComponentModel.DataAnnotations;

namespace Library_Management_Project.Models.Dtos
{
    public class CreateBookDto
    {
        [Required(ErrorMessage = "Serial Number is required.")]
        public string SerialNumber { get; set; }

        [Required(ErrorMessage = "Name of Book is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Author of Book is required.")]
        public string Author { get; set; }

        [Required(ErrorMessage = "Published year is required.")]
        public int PublishedYear{ get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity of Books must be at least 1.")]
        public int Quantity { get; set; }
    }
}
