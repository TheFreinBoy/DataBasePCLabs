using System.ComponentModel.DataAnnotations;

namespace DBPCLabs.Models
{
    public class ComputerSoftware
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Оберіть комп'ютер")]
        [Range(1, int.MaxValue, ErrorMessage = "Оберіть комп'ютер")]
        public int ComputerId { get; set; }
        public string? ComputerInventoryNumber { get; set; }

        [Required(ErrorMessage = "Оберіть програму")]
        [Range(1, int.MaxValue, ErrorMessage = "Оберіть програму")]
        public int SoftwareId { get; set; }
        public string? SoftwareName { get; set; }
        public string? SoftwareVersion { get; set; }

        [Required(ErrorMessage = "Вкажіть дату встановлення")]
        public DateTime InstallationDate { get; set; } = DateTime.Today;
    }
}