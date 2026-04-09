using System.ComponentModel.DataAnnotations;

namespace DBPCLabs.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "ПІБ студента є обов'язковим")]
        [StringLength(150, ErrorMessage = "ПІБ не може перевищувати 150 символів")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email є обов'язковим")]
        [EmailAddress(ErrorMessage = "Введіть коректний формат електронної пошти")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Оберіть групу зі списку")]
        [Range(1, int.MaxValue, ErrorMessage = "Оберіть групу зі списку")]
        public int GroupId { get; set; }
        
        public string? GroupName { get; set; }
    }
}