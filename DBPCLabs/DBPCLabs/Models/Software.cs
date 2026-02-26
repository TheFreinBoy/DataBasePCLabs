using System.ComponentModel.DataAnnotations;

namespace DBPCLabs.Models;

public class Software
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Назва програми є обов'язковою.")]
    [StringLength(100, ErrorMessage = "Назва не може перевищувати 100 символів.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть версію (напр. 2024, 1.0 або ОСТАННЯ).")]
    public string Version { get; set; } = string.Empty;

    [Required(ErrorMessage = "Оберіть тип ліцензії.")]
    public string LicenseType { get; set; } = "Безкоштовна";
    
    public List<Computer> Computers { get; set; } = new();
}