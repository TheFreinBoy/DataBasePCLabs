using System.ComponentModel.DataAnnotations;

namespace DBPCLabs.Models
{
    public class Group
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва групи є обов'язковою")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Оберіть кафедру зі списку")]
        [Range(1, int.MaxValue, ErrorMessage = "Оберіть кафедру зі списку")]
        public int DepartmentId { get; set; }
        
        public string? DepartmentName { get; set; } 
    }
}