using System.ComponentModel.DataAnnotations; 

namespace DBPCLabs.Models;

public class Laboratory
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Назва лабораторії є обов'язковою.")]
    [MinLength(3, ErrorMessage = "Назва має містити мінімум 3 символи.")]
    [RegularExpression(@"^[a-zA-Zа-яА-ЯіІїЇєЄґҐ\s]+$", ErrorMessage = "Назва може містити лише літери та пробіли.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Номер аудиторії є обов'язковим.")]
    
    [RegularExpression(@"^\d+$", ErrorMessage = "Номер аудиторії має містити лише цифри (без літер чи дефісів).")]
    public string RoomNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Місткість є обов'язковою.")]
    [Range(1, 100, ErrorMessage = "Кількість ПК має бути від 1 до 100.")]
    public int Capacity { get; set; }
    
    public ICollection<Computer> Computers { get; set; } = new List<Computer>();
}