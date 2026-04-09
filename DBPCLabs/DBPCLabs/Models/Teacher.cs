using System.ComponentModel.DataAnnotations;

namespace DBPCLabs.Models
{
    public class Teacher
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "ПІБ викладача є обов'язковим")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Оберіть кафедру зі списку")]
        [Range(1, int.MaxValue, ErrorMessage = "Оберіть кафедру зі списку")]
        public int DepartmentId { get; set; }

        public string? DepartmentName { get; set; } 

        [Required(ErrorMessage = "Email є обов'язковим")]
        [EmailAddress(ErrorMessage = "Коректний формат: name@domain.com")]
        public string Email { get; set; } = string.Empty;
    }
}