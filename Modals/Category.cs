
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [DisplayName("Category Name")]
        [MaxLength(10)]
        public string Name { get; set; }
        [DisplayName("Display Order")]
        [Range(1,100)]
        [Required]
        public int Displayorder { get; set; }
    }
}
