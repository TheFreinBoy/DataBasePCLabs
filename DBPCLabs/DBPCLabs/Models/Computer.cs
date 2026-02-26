using System.ComponentModel.DataAnnotations;

namespace DBPCLabs.Models;

public class Computer
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Інвентарний номер є обов'язковим.")]
    public string InventoryNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть процесор (напр., Intel Core i5).")]
    public string Cpu { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть обсяг оперативної пам'яті.")]
    [Range(8, 256, ErrorMessage = "Обсяг RAM має бути від 8 до 256 ГБ.")] 
    public int RamGb { get; set; }

    [Required(ErrorMessage = "Оберіть лабораторію для цього ПК.")]
    [Range(1, int.MaxValue, ErrorMessage = "Будь ласка, оберіть лабораторію зі списку.")]
    public int LaboratoryId { get; set; }
    
    public Laboratory? Laboratory { get; set; }
    public ICollection<Software> InstalledSoftware { get; set; } = new List<Software>();
}