using System.ComponentModel.DataAnnotations;

namespace DBPCLabs.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва кафедри є обов'язковою")]
        [StringLength(150, ErrorMessage = "Назва занадто довга")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Назва факультету є обов'язковою")]
        [StringLength(150, ErrorMessage = "Назва занадто довга")]
        public string Faculty { get; set; } = string.Empty;
    }
}