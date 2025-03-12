using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library_Management_Project.Models
{
    public class Book
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } 

        public string SerialNumber {  get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Author { get; set; }

        public int PublishedYear { get; set; }

        public string Description { get; set; }

        public int Quantity {  get; set; }

        public string ImagePath { get; set; }


    }
}
