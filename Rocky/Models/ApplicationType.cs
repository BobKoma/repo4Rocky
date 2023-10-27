using System.ComponentModel.DataAnnotations;

namespace Rocky.Models
{
    public class ApplicationType
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage ="Введіть ім'я, будь ласка!") ]
        public string Name { get; set; }
       
    }
}
