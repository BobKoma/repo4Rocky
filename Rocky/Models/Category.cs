using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Rocky.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        [Required]
        public required string? Name { get; set; }
        [Required]
        [Range( 1, int.MaxValue, ErrorMessage = $"Display Order must be in range from 1 to ... " )]
        [DisplayName("Display Order")]
        public int? DisplayOrder { get; set; }

        public ICollection<Product>? Products { get; }
    }
}
